using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using Unity;

namespace ValheimLegends
{
    public class Class_Enchanter
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "piece", "terrain", "vehicle", "viewblock", "character", "character_noenv", "character_trigger", "character_net", "character_ghost", "Water");


        private static bool zonechargeCharging = false;
        private static float zonechargeCount;
        private static int zonechargeChargeAmount;
        private static int zonechargeChargeAmountMax;

        private static float zonechargeSkillGain = 0f;

        public enum EnchanterAttackType
        {
            Charm = 10,
            Shock = 8,
            None = 0
        }

        public static EnchanterAttackType QueuedAttack;

        public static StatusEffect HasZoneBuffTime(Player p)
        {
            foreach(StatusEffect se in p.GetSEMan().GetStatusEffects())
            {
                if(se.m_name.StartsWith("VL_Biome"))
                {
                    return se;
                }
            }
            return null;
        }

        public static void Execute_Attack(Player player, ref Rigidbody playerBody, ref float altitude)
        {
            if (QueuedAttack == EnchanterAttackType.Charm)
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                //Apply effects
                Vector3 vector = player.GetEyePoint() + player.transform.up * .1f + player.GetLookDir() * .5f + player.transform.right * .25f;
                GameObject prefab = ZNetScene.instance.GetPrefab("VL_Charm");
                GameObject GO_Charm = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                Projectile P_Charm = GO_Charm.GetComponent<Projectile>();
                P_Charm.name = "VL_Charm";
                P_Charm.m_respawnItemOnHit = false;
                P_Charm.m_spawnOnHit = null;
                P_Charm.m_ttl = 30f * VL_GlobalConfigs.c_enchanterCharm;
                P_Charm.m_gravity = 0f;
                P_Charm.m_rayRadius = .01f;
                P_Charm.m_aoe = 1f + (.01f * sLevel);
                P_Charm.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                GO_Charm.transform.localScale = Vector3.zero;
                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, 1000f, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                HitData hitData = new HitData();
                hitData.m_skill = ValheimLegends.AlterationSkill;
                Vector3 a = Vector3.MoveTowards(GO_Charm.transform.position, target, 1f);
                P_Charm.Setup(player, (a - GO_Charm.transform.position) * 40, -1f, hitData, null, null);
                Traverse.Create(root: P_Charm).Field("m_skill").SetValue(ValheimLegends.AlterationSkill);
            }
            else if(QueuedAttack == EnchanterAttackType.Shock)
            {
                StatusEffect se_shock = HasZoneBuffTime(player);
                if (se_shock != null)
                {
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Shock"), player.GetEyePoint() + player.GetLookDir() * 2.5f + player.transform.right * .25f, Quaternion.LookRotation(player.GetLookDir()));
                    //Skill influence
                    float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                    Vector3 center = player.GetEyePoint() + player.GetLookDir() * 4f;
                    List<Character> allCharacters = new List<Character>();
                    allCharacters.Clear();
                    Character.GetCharactersInRange(center, 4f, allCharacters);
                    foreach (Character ch in allCharacters)
                    {
                        if (BaseAI.IsEnemy(player, ch) && VL_Utility.LOS_IsValid(ch, center, center))
                        {
                            Vector3 direction = (ch.transform.position - player.transform.position);
                            HitData hitData = new HitData();
                            hitData.m_damage.m_lightning = (15 + sLevel) + se_shock.m_ttl * UnityEngine.Random.Range(.03f, .06f) * (1f + .1f * sLevel) * VL_GlobalConfigs.c_enchanterBiomeShock;
                            hitData.m_pushForce = 0f;
                            hitData.m_point = ch.GetEyePoint();
                            hitData.m_dir = direction;
                            hitData.m_skill = ValheimLegends.AlterationSkill;
                            ch.Damage(hitData);
                            ch.Stagger(hitData.m_dir);
                        }
                    }
                    if(se_shock.name == "SE_VL_BiomeSwamp")
                    {
                        SE_BiomeSwamp se_swamp = se_shock as SE_BiomeSwamp;
                        if (se_swamp.biomeLight != null)
                        {
                            UnityEngine.Object.Destroy(se_swamp.biomeLight);
                        }
                    }

                    player.GetSEMan().RemoveStatusEffect(se_shock);
                }
            }
        }

        public static void Process_Input(Player player, ref float altitude)
        {
            if (VL_Utility.Ability3_Input_Down && !zonechargeCharging)
            {
                StatusEffect se_shock = HasZoneBuffTime(player);
                if(se_shock != null && se_shock.m_ttl > 0f && QueuedAttack != EnchanterAttackType.Shock)
                {                    
                    QueuedAttack = EnchanterAttackType.Shock;
                    ValheimLegends.isChargingDash = true;
                    ValheimLegends.dashCounter = 0;
                    VL_Utility.RotatePlayerToTarget(player);
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("unarmed_attack0");
                    
                }
                else if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD") && !zonechargeCharging)
                {
                    //player.Message(MessageHud.MessageType.Center, "ZoneCharge - starting");
                    if (player.GetStamina() >= VL_Utility.GetZoneChargeCost)
                    {
                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AbjurationSkillDef).m_level;
                        player.StartEmote("challenge");
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");
                        ValheimLegends.isChanneling = true;
                        ValheimLegends.shouldUseGuardianPower = false;
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetZoneChargeCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetZoneChargeCost);

                        //Effects, animations, and sounds
                                  
                        //
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ParticleTailField"), player.transform.position, Quaternion.identity);

                        //Lingering effects
                        
                        //Apply effects
                        zonechargeCharging = true;
                        zonechargeChargeAmount = 0;
                        zonechargeChargeAmountMax = Mathf.RoundToInt(15f * (1f - (sLevel * .005f))); // modified by skill
                        zonechargeCount = 10f;

                        //Skill gain
                        zonechargeSkillGain += VL_Utility.GetZoneChargeSkillGain;
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to begin Zone Charge : (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetZoneChargeCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if ((VL_Utility.Ability3_Input_Pressed && player.GetStamina() > 1 && player.GetStamina() > VL_Utility.GetZoneChargeCostPerUpdate) && Mathf.Max(0f, altitude - player.transform.position.y) <= 1f && (zonechargeCharging && ValheimLegends.isChanneling))
            {
                VL_Utility.SetTimer();
                ValheimLegends.isChanneling = true;
                zonechargeChargeAmount++;
                player.UseStamina(VL_Utility.GetZoneChargeCostPerUpdate);
                if (zonechargeChargeAmount >= zonechargeChargeAmountMax)
                {
                    zonechargeCount += 2;
                    zonechargeChargeAmount = 0;
                    //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ParticleTailField"), player.transform.position, Quaternion.identity);
                    //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(1.5f);                    
                    //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_GP_Stone"), player.GetEyePoint(), Quaternion.identity);
                    //player.Message(MessageHud.MessageType.Center, "zonecharge - charging " + zonechargeCount);

                    //Skill gain
                    zonechargeSkillGain += .2f;
                }
            }
            else if(((VL_Utility.Ability3_Input_Up || player.GetStamina() <= 1 || player.GetStamina() <= VL_Utility.GetZoneChargeCostPerUpdate) || Mathf.Max(0f, altitude - player.transform.position.y) >= 1f) && (zonechargeCharging && ValheimLegends.isChanneling))
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AbjurationSkillDef).m_level;

                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_ParticleFieldBurst"), player.transform.position, Quaternion.identity);
                List<Character> allCharacters = new List<Character>();
                allCharacters.Clear();
                Character.GetCharactersInRange(player.transform.position, (30f + .2f * sLevel), allCharacters);
                float bonusModifiers = ((3f * sLevel) + 2 * zonechargeCount) * VL_GlobalConfigs.c_enchanterBiome;
                //SE_Ability3_CD se3 = (SE_Ability3_CD)player.GetSEMan().GetStatusEffect("SE_VL_Ability3_CD");
                //if (se3 != null)
                //{
                //    //ZLog.Log("zone count was " + zonechargeCount);
                //    //se3.m_ttl -= zonechargeCount;
                //}
                if (player.GetCurrentBiome() == Heightmap.Biome.Meadows)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("vfx_Potion_stamina_medium");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomeMeadows SE_BiomeMeadows = (SE_BiomeMeadows)ScriptableObject.CreateInstance(typeof(SE_BiomeMeadows));
                        SE_BiomeMeadows.m_ttl = SE_BiomeMeadows.m_baseTTL + bonusModifiers;
                        SE_BiomeMeadows.regenBonus = (1f + (.1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        SE_BiomeMeadows.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMeadows, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMeadows.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMeadows, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }                        
                    }
                }
                else if(player.GetCurrentBiome() == Heightmap.Biome.BlackForest)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("fx_Potion_frostresist");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomeBlackForest SE_BiomeBlackForest = (SE_BiomeBlackForest)ScriptableObject.CreateInstance(typeof(SE_BiomeBlackForest));
                        SE_BiomeBlackForest.m_ttl = SE_BiomeBlackForest.m_baseTTL + bonusModifiers;
                        SE_BiomeBlackForest.carryModifier = (50f + sLevel);
                        SE_BiomeBlackForest.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeBlackForest, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeBlackForest.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeBlackForest, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }
                    }
                }
                else if(player.GetCurrentBiome() == Heightmap.Biome.Swamp)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("vfx_Potion_health_medium");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomeSwamp SE_BiomeSwamp = (SE_BiomeSwamp)ScriptableObject.CreateInstance(typeof(SE_BiomeSwamp));
                        SE_BiomeSwamp.m_ttl = SE_BiomeSwamp.m_baseTTL + bonusModifiers;
                        SE_BiomeSwamp.resistModifier = .8f - (.002f * sLevel);
                        SE_BiomeSwamp.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeSwamp, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeSwamp.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeSwamp, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }
                    }
                }
                else if(player.GetCurrentBiome() == Heightmap.Biome.Mountain)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("fx_Potion_frostresist");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomeMountain SE_BiomeMountain = (SE_BiomeMountain)ScriptableObject.CreateInstance(typeof(SE_BiomeMountain));
                        SE_BiomeMountain.m_ttl = SE_BiomeMountain.m_baseTTL + bonusModifiers;
                        SE_BiomeMountain.resistModifier = (1f + (.1f * sLevel));
                        SE_BiomeMountain.staminaRegen = 5f + (.075f * sLevel);
                        SE_BiomeMountain.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMountain, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMountain.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMountain, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }
                    }
                }
                else if(player.GetCurrentBiome() == Heightmap.Biome.Plains)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("vfx_Potion_stamina_medium");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomePlains SE_BiomePlains = (SE_BiomePlains)ScriptableObject.CreateInstance(typeof(SE_BiomePlains));
                        SE_BiomePlains.m_ttl = SE_BiomePlains.m_baseTTL + bonusModifiers;
                        //SE_BiomePlains.regenBonus = (1f + (.1f * sLevel)) * ValheimLegends.vl_mce_abilityDamageMultiplier.Value;
                        SE_BiomePlains.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomePlains, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomePlains.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomePlains, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }
                    }
                }
                else if(player.GetCurrentBiome() == Heightmap.Biome.Ocean)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("fx_Potion_frostresist");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomeOcean SE_BiomeOcean = (SE_BiomeOcean)ScriptableObject.CreateInstance(typeof(SE_BiomeOcean));
                        SE_BiomeOcean.m_ttl = SE_BiomeOcean.m_baseTTL + bonusModifiers;
                        //SE_BiomeOcean.regenBonus = (1f + (.1f * sLevel)) * ValheimLegends.vl_mce_abilityDamageMultiplier.Value;
                        SE_BiomeOcean.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeOcean, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeOcean.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeOcean, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }
                    }
                }
                else if(player.GetCurrentBiome() == Heightmap.Biome.Mistlands)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("fx_Potion_frostresist");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomeMist SE_BiomeMist = (SE_BiomeMist)ScriptableObject.CreateInstance(typeof(SE_BiomeMist));
                        SE_BiomeMist.m_ttl = SE_BiomeMist.m_baseTTL + bonusModifiers;
                        //SE_BiomeMist.regenBonus = (1f + (.1f * sLevel)) * ValheimLegends.vl_mce_abilityDamageMultiplier.Value;
                        SE_BiomeMist.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMist, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMist.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeMist, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }
                    }
                }
                else if(player.GetCurrentBiome() == Heightmap.Biome.AshLands)
                {
                    GameObject effect = ZNetScene.instance.GetPrefab("vfx_Potion_health_medium");
                    foreach (Character p in allCharacters)
                    {
                        SE_BiomeAsh SE_BiomeAsh = (SE_BiomeAsh)ScriptableObject.CreateInstance(typeof(SE_BiomeAsh));
                        SE_BiomeAsh.m_ttl = SE_BiomeAsh.m_baseTTL + bonusModifiers;
                        //SE_BiomeAsh.regenBonus = (1f + (.1f * sLevel)) * ValheimLegends.vl_mce_abilityDamageMultiplier.Value;
                        SE_BiomeAsh.doOnce = false;
                        if (!BaseAI.IsEnemy(p, player))
                        {
                            if (p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeAsh, true);
                            }
                            else if (p.IsPlayer())
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeAsh.name, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(SE_BiomeAsh, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);
                        }
                    }
                }
                else
                {
                    ZLog.Log("Biome invalid.");
                }

                zonechargeCharging = false;
                zonechargeCount = 0;
                zonechargeChargeAmount = 0;
                ValheimLegends.isChanneling = false;
                QueuedAttack = EnchanterAttackType.None;

                //Skill gain
                player.RaiseSkill(ValheimLegends.AbjurationSkill, zonechargeSkillGain);
                zonechargeSkillGain = 0f;
            }
            else if(VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Frost Nova");
                    if (player.GetStamina() >= VL_Utility.GetCharmCost)
                    {                        
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetCharmCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetCharmCost);

                        ValheimLegends.isChargingDash = true;
                        ValheimLegends.dashCounter = 0;
                        QueuedAttack = EnchanterAttackType.Charm;

                        //Effects, animations, and sounds
                        //player.StartEmote("cheer");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("knife_stab0");

                        //Lingering effects

                        VL_Utility.RotatePlayerToTarget(player);
                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetCharmSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Charm: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetCharmCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }

            }
            else if (VL_Utility.Ability1_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability1_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Weaken");                   
                    if (player.GetStamina() >= (VL_Utility.GetWeakenCost))
                    {
                        ValheimLegends.shouldUseGuardianPower = false;
                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetWeakenCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetWeakenCost + (.5f * sLevel));

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");
                        //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_GP_Stone"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects
                        RaycastHit hitInfo = default(RaycastHit);
                        Vector3 position = player.transform.position;
                        Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Weaken"), target, Quaternion.identity);
                        for(int i = 0; i < 4; i++)
                        {
                            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_WeakenStatus"), player.transform.position + player.transform.up * (UnityEngine.Random.Range(.4f, 1.1f)), Quaternion.identity);
                        }

                        SE_Weaken se_weaken = (SE_Weaken)ScriptableObject.CreateInstance(typeof(SE_Weaken));
                        se_weaken.m_ttl = SE_Weaken.m_baseTTL;
                        se_weaken.damageReduction = (.15f + (.0015f * sLevel)) * VL_GlobalConfigs.c_enchanterWeaken;
                        se_weaken.speedReduction = .8f - (.001f * sLevel);
                        se_weaken.staminaDrain = (.1f + (.001f * sLevel)) * VL_GlobalConfigs.c_enchanterWeaken;

                        List<Character> allCharacters = Character.GetAllCharacters();
                        foreach (Character ch in allCharacters)
                        {
                            if ((BaseAI.IsEnemy(player, ch) && (ch.transform.position - target).magnitude <= 5f + (.01f * sLevel)))
                            {
                                if (ch.IsPlayer())
                                {
                                    ch.GetSEMan().AddStatusEffect(se_weaken.name, true);
                                }
                                else
                                {
                                    ch.GetSEMan().AddStatusEffect(se_weaken, true);
                                }
                            }
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetWeakenSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Weaken: (" + player.GetStamina().ToString("#.#") + "/" + (VL_Utility.GetWeakenCost) +")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else
            {
                ValheimLegends.isChanneling = false;
            }
        }
    }
}
