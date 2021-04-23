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
    public class Class_Priest
    {
        private static int Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "character", "Water");

        private static GameObject GO_CastFX;

        private static GameObject GO_Sanctify;        
        private static Projectile P_Sanctify;
        private static StatusEffect SE_Fireball;

        private static GameObject GO_Meteor;
        private static Projectile P_Meteor;
        private static StatusEffect SE_Meteor;       
        private static bool healCharging = false;
        private static int healCount;
        private static int healChargeAmount;
        private static int healChargeAmountMax;

        private static float healSkillGain = 0f;

        public static void PurgeStatus_NearbyPlayers(Player healer, float radius, List<string> effectNames)
        {
            if(effectNames != null && effectNames.Count > 0)
            {
                List<Player> allPlayers = new List<Player>();
                allPlayers.Clear();
                Player.GetPlayersInRange(healer.transform.position, radius, allPlayers);
                foreach (Player p in allPlayers)
                {
                    foreach(string effect in effectNames)
                    {
                        if(p.GetSEMan().HaveStatusEffect(effect))
                        {
                            p.GetSEMan().RemoveStatusEffect(effect);
                            break;
                        }
                    }
                }
            }
        }

        public static void HealNearbyPlayers(Player healer, float radius, float amount)
        {
            List<Player> allPlayers = new List<Player>();
            allPlayers.Clear();
            Player.GetPlayersInRange(healer.transform.position, radius, allPlayers);
            foreach (Player p in allPlayers)
            {
                p.Heal(amount, true);
            }
        }

        public static void Process_Input(Player player, ref float altitude)
        {
            System.Random rnd = new System.Random();
            if (VL_Utility.Ability3_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "heal - starting");
                    if (player.GetStamina() >= VL_Utility.GetHealCost)
                    {
                        ValheimLegends.shouldUseGuardianPower = false;
                        ValheimLegends.isChanneling = true;

                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetHealCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetHealCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");                        
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_HealPulse"), player.GetCenterPoint(), Quaternion.identity);

                        //Lingering effects
                        healCharging = true;
                        healChargeAmount = 0;
                        healChargeAmountMax = 30;
                        //Apply effects
                        List<string> effectList = new List<string>();
                        effectList.Clear();
                        effectList.Add("Burning");
                        effectList.Add("Poison");
                        effectList.Add("Frost");
                        effectList.Add("Wet");
                        effectList.Add("Smoked");
                        PurgeStatus_NearbyPlayers(player, 30f + .2f * sLevel, effectList);
                        HealNearbyPlayers(player, 30f + .2f * sLevel, 10f + sLevel);

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetHealSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to begin heal : (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetHealCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (VL_Utility.Ability3_Input_Pressed && healCharging && player.GetStamina() > VL_Utility.GetHealCostPerUpdate && Mathf.Max(0f, altitude - player.transform.position.y) <= 1f)
            {
                healChargeAmount++;                
                player.UseStamina(VL_Utility.GetHealCostPerUpdate);
                ValheimLegends.isChanneling = true;
                if (healChargeAmount >= healChargeAmountMax)
                {
                    healCount++;
                    healChargeAmount = 0;                    
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");
                    //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(1.5f);                    
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_HealPulse"), player.GetCenterPoint(), Quaternion.identity);
                    float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;
                    HealNearbyPlayers(player, 20f + .2f * sLevel, ((healCount + sLevel * .3f) * 2f));

                    //Skill gain
                    player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetHealSkillGain * .5f);
                }
            }
            else if((VL_Utility.Ability3_Input_Up || player.GetStamina() <= VL_Utility.GetHealCostPerUpdate || Mathf.Max(0f, altitude - player.transform.position.y) > 1f) && healCharging)
            {
                healCount = 0;
                healChargeAmount = 0;
                healCharging = false;
                ValheimLegends.isChanneling = false;
            }            
            else if (VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    if (player.GetStamina() >= VL_Utility.GetPurgeCost)
                    {
                        ValheimLegends.shouldUseGuardianPower = false;
                        //Skill influence
                        float sPurgeLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;
                        float sHealLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetPurgeCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetPurgeCost);

                        //Effects, animations, and sounds
                        player.StartEmote("challenge");
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Purge"), player.GetCenterPoint(), Quaternion.identity);

                        //Lingering effects

                        //Apply effects
                        HealNearbyPlayers(player, 20f + (.2f * sHealLevel), .5f + UnityEngine.Random.Range(.4f, .6f) * sHealLevel);
                        List<Character> allCharacters = new List<Character>();
                        allCharacters.Clear();
                        Character.GetCharactersInRange(player.transform.position, 20f + (.2f * sPurgeLevel), allCharacters);
                        foreach (Character ch in allCharacters)
                        {
                            if (BaseAI.IsEnemy(player, ch))
                            {
                                Vector3 direction = (ch.transform.position - player.transform.position);
                                HitData hitData = new HitData();
                                hitData.m_damage.m_spirit = UnityEngine.Random.Range(4f + (.4f * sPurgeLevel), 8f + (.8f * sPurgeLevel)) * VL_GlobalConfigs.g_DamageModifer;
                                hitData.m_damage.m_fire = UnityEngine.Random.Range(4f + (.4f * sPurgeLevel), 8f + (.8f * sPurgeLevel)) * VL_GlobalConfigs.g_DamageModifer;
                                hitData.m_pushForce = 0f;
                                hitData.m_point = ch.GetEyePoint();
                                hitData.m_dir = (player.transform.position - ch.transform.position);
                                hitData.m_skill = ValheimLegends.EvocationSkill;
                                ch.ApplyDamage(hitData, true, true, HitData.DamageModifier.Normal);
                            }
                        }
                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetPurgeSkillGain * .5f);
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetPurgeSkillGain * .5f);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to for Purge: (" + player.GetStamina().ToString("#.#") + "/" + (VL_Utility.GetPurgeCost) +")");
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
                    //player.Message(MessageHud.MessageType.Center, "Sanctify");
                    if (player.GetStamina() >= VL_Utility.GetSanctifyCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetSanctifyCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetSanctifyCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                        //Effects, animations, and sounds
                        //player.StartEmote("cheer");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("battleaxe_attack0");
                        //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_activate"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects

                        
                        RaycastHit hitInfo = default(RaycastHit);
                        Vector3 position = player.transform.position;
                        Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                        Vector3 vector = target + player.transform.up * 12f;
                        GameObject prefab = ZNetScene.instance.GetPrefab("VL_SanctifyHammer");
                        GO_Sanctify = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                        P_Sanctify = GO_Sanctify.GetComponent<Projectile>();
                        P_Sanctify.name = "Sanctify";
                        P_Sanctify.m_respawnItemOnHit = false;
                        P_Sanctify.m_spawnOnHit = null;
                        P_Sanctify.m_ttl = 30f;
                        P_Sanctify.m_gravity = 9f;
                        P_Sanctify.m_rayRadius = 1f;
                        P_Sanctify.m_aoe = 8f + (.04f * sLevel);

                        GO_Sanctify.transform.localScale = Vector3.one;

                        HitData hitData = new HitData();
                        hitData.m_damage.m_fire = UnityEngine.Random.Range(10f + (.5f * sLevel), 20f + (.75f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        hitData.m_damage.m_blunt = UnityEngine.Random.Range(10f + (.5f * sLevel), 20f + (.75f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        hitData.m_damage.m_spirit = UnityEngine.Random.Range(10f + (.5f * sLevel), 20f + (.75f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        hitData.m_pushForce = 50f;
                        hitData.m_skill = ValheimLegends.EvocationSkill;
                        //Vector3 a = Vector3.MoveTowards(GO_Sanctify.transform.position, target, 1f);
                        P_Sanctify.Setup(player, new Vector3(0f, -1f, 0f), -1f, hitData, null);
                        
                        Traverse.Create(root: P_Sanctify).Field("m_skill").SetValue(ValheimLegends.EvocationSkill);
                        GO_Sanctify = null;


                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetSanctifySkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Sanctify: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetSanctifyCost + ")");
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
