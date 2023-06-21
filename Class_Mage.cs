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
    public class Class_Mage
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "character", "character_net", "character_ghost");

        private static GameObject GO_CastFX;

        private static GameObject GO_Fireball;        
        private static Projectile P_Fireball;
        private static StatusEffect SE_Fireball;

        private static GameObject GO_Meteor;
        private static Projectile P_Meteor;
        private static StatusEffect SE_Meteor;       
        private static bool meteorCharging = false;
        private static int meteorCount;
        private static int meteorChargeAmount;
        private static int meteorChargeAmountMax;

        private static float meteorSkillGain = 0f;

        public enum MageAttackType
        {
            IceDagger = 20,
            FlameNova = 60
        }

        public static MageAttackType QueuedAttack;

        public static void Execute_Attack(Player player)
        {
            if (QueuedAttack == MageAttackType.FlameNova)
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_FlameBurst"), player.transform.position, Quaternion.identity);


                List<Character> allCharacters = Character.GetAllCharacters();
                foreach (Character ch in allCharacters)
                {
                    if (BaseAI.IsEnemy(player, ch) && ((ch.transform.position - player.transform.position).magnitude <= (8f + (.1f * sLevel))) && VL_Utility.LOS_IsValid(ch, player.GetCenterPoint(), player.transform.position + player.transform.up * .2f))
                    {
                        Vector3 direction = (ch.transform.position - player.transform.position);
                        HitData hitData = new HitData();
                        hitData.m_damage.m_fire = UnityEngine.Random.Range(5 + (2.75f * sLevel), 10 + (3.5f * sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_mageInferno;
                        hitData.m_pushForce = 0f;
                        hitData.m_point = ch.GetEyePoint();
                        hitData.m_dir = direction;
                        hitData.m_skill = ValheimLegends.EvocationSkill;
                        ch.Damage(hitData);
                    }
                }

                GameCamera.instance.AddShake(player.transform.position, 15f, 2f, false);
                //Skill gain
                player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetFrostNovaSkillGain * 1.5f);

            }
            else if(QueuedAttack == MageAttackType.IceDagger)
            {
                //Skill influence
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                Vector3 vector = player.GetEyePoint() + player.GetLookDir() * .2f + player.transform.up * .1f + player.transform.right * .28f;
                GameObject prefab = ZNetScene.instance.GetPrefab("VL_FrostDagger");
                GameObject GO_IceDagger = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                Projectile P_IceDagger = GO_IceDagger.GetComponent<Projectile>();
                P_IceDagger.name = "IceDagger";
                P_IceDagger.m_respawnItemOnHit = false;
                P_IceDagger.m_spawnOnHit = null;
                P_IceDagger.m_ttl = .6f;
                P_IceDagger.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                GO_IceDagger.transform.localScale = Vector3.one * .8f;
                RaycastHit hitInfo = default(RaycastHit);
                Vector3 position = player.transform.position;
                Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                HitData hitData = new HitData();
                hitData.m_damage.m_pierce = UnityEngine.Random.Range(2f + (.25f *sLevel), 5f + (.75f * sLevel)) * VL_GlobalConfigs.c_mageFrostDagger * VL_GlobalConfigs.g_DamageModifer;
                hitData.m_damage.m_frost = UnityEngine.Random.Range((.5f * sLevel), 2f + (1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_mageFrostDagger;
                hitData.m_skill = ValheimLegends.EvocationSkill;
                hitData.SetAttacker(player);
                Vector3 a = Vector3.MoveTowards(GO_IceDagger.transform.position, target, 1f);
                P_IceDagger.Setup(player, (a - GO_IceDagger.transform.position) * 55f, -1f, hitData, null, null);
                Traverse.Create(root: P_IceDagger).Field("m_skill").SetValue(ValheimLegends.EvocationSkill);
                GO_IceDagger = null;
            }

        }

        public static void Process_Input(Player player, float altitude)
        {
            System.Random rnd = new System.Random();
            if (VL_Utility.Ability3_Input_Down && !meteorCharging)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Meteor - starting");
                    if (player.GetStamina() >= VL_Utility.GetMeteorCost)
                    {
                        ValheimLegends.shouldUseGuardianPower = false;
                        ValheimLegends.isChanneling = true;
                        meteorSkillGain = 0;
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetMeteorCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetMeteorCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");                        
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Flames"), player.transform.position, Quaternion.identity);

                        //Lingering effects
                        meteorCharging = true;
                        meteorChargeAmount = 0;
                        meteorChargeAmountMax = Mathf.RoundToInt(60f * (1f - (sLevel/200f))); // modified by skill
                        meteorCount = 1;

                        //Apply effects


                        //Skill gain
                        meteorSkillGain += VL_Utility.GetMeteorSkillGain;
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to begin Meteor : (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetMeteorCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (VL_Utility.Ability3_Input_Pressed && meteorCharging && player.GetStamina() > 1 && Mathf.Max(0f, altitude - player.transform.position.y) <= 1f)
            {
                VL_Utility.SetTimer();
                meteorChargeAmount++;                
                player.UseStamina(VL_Utility.GetMeteorCostPerUpdate);
                ValheimLegends.isChanneling = true;
                if (meteorChargeAmount >= meteorChargeAmountMax)
                {
                    meteorCount++;
                    meteorChargeAmount = 0;                    
                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");
                    //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(1.5f);                    
                    GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Flames"), player.transform.position, Quaternion.identity);
                    //player.Message(MessageHud.MessageType.Center, "Meteor - charging " + meteorCount);

                    //Skill gain
                    meteorSkillGain += .2f;
                }
            }
            else if((VL_Utility.Ability3_Input_Up || player.GetStamina() <= 1 || Mathf.Max(0f, altitude - player.transform.position.y) > 1f) && meteorCharging)
            { 
                //player.Message(MessageHud.MessageType.Center, "Meteor - activate");               
                
                Vector3 vector = player.transform.position + player.transform.up * 2f + player.GetLookDir() * 1f;
                GameObject prefab = ZNetScene.instance.GetPrefab("projectile_meteor");                
                meteorCharging = false;
                float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;
                for (int i = 0; i < meteorCount; i++)
                {
                    GO_Meteor = UnityEngine.Object.Instantiate(prefab, new Vector3(vector.x + rnd.Next(-100, 100), vector.y + 250f, vector.z + rnd.Next(-100, 100)), Quaternion.identity);
                    P_Meteor = GO_Meteor.GetComponent<Projectile>();
                    P_Meteor.name = "Meteor"+i;
                    P_Meteor.m_respawnItemOnHit = false;
                    P_Meteor.m_spawnOnHit = null;
                    P_Meteor.m_ttl = 6f;
                    P_Meteor.m_gravity = 0f;
                    P_Meteor.m_rayRadius = .1f;
                    P_Meteor.m_aoe = 8f + (.04f * sLevel);     
                    P_Meteor.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                    GO_Meteor.transform.localScale = Vector3.zero;
                    RaycastHit hitInfo = default(RaycastHit);
                    Vector3 position = player.transform.position;
                    Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, 1000f, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                    target.x += rnd.Next(-8, 8);
                    target.y += rnd.Next(-8, 8);
                    target.z += rnd.Next(-8, 8);
                    HitData hitData = new HitData();
                    hitData.m_damage.m_fire = UnityEngine.Random.Range(30 + (.5f * sLevel), 50 + sLevel) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_mageMeteor;
                    hitData.m_damage.m_blunt = UnityEngine.Random.Range(15 + (.25f * sLevel), 30 + (.5f * sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_mageMeteor;
                    hitData.m_pushForce = 10f;
                    hitData.SetAttacker(player);

                    hitData.m_skill = ValheimLegends.EvocationSkill;
                    Vector3 a = Vector3.MoveTowards(GO_Meteor.transform.position, target, 1f);
                    P_Meteor.Setup(player, (a - GO_Meteor.transform.position) * UnityEngine.Random.Range(78f, 86f), -1f, hitData, null, null);
                    Traverse.Create(root: P_Meteor).Field("m_skill").SetValue(ValheimLegends.EvocationSkill);
                    GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_permitted_removed"), player.transform.position + player.transform.right * UnityEngine.Random.Range(-1f, 1f) + player.transform.up * UnityEngine.Random.Range(0, 1.5f), Quaternion.identity);
                }

                //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("unarmed_attack0");                
                meteorCount = 0;
                meteorChargeAmount = 0;
                GO_Meteor = null;
                //Skill gain
                player.RaiseSkill(ValheimLegends.EvocationSkill, meteorSkillGain);
                meteorSkillGain = 0f;
                ValheimLegends.isChanneling = false;
                //GO_CastFX.transform.position = GO_CastFX.transform.position + FireCastFx.transform.up * 1.5f;
                //if ((bool)FireCastFx && FireCastFx.activeSelf)
                //{
                //    FireCastFx.SetActive(value: false);
                //}
                //FireCastFx = null;
            }
            else if(VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Frost Nova");
                    if (player.IsBlocking())
                    {
                        if(player.GetStamina() >= VL_Utility.GetFrostNovaCost * 2f)
                        {
                            StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                            se_cd.m_ttl = VL_Utility.GetFrostNovaCooldownTime;
                            player.GetSEMan().AddStatusEffect(se_cd);

                            //Ability Cost
                            player.UseStamina(VL_Utility.GetFrostNovaCost * 2f);

                            //Effects, animations, and sounds
                            ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("swing_sledge");
                            ValheimLegends.isChargingDash = true;
                            ValheimLegends.dashCounter = 0;
                            QueuedAttack = MageAttackType.FlameNova;
                        }
                        else
                        {
                            player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Inferno: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetFrostNovaCost *2f + ")");
                        }
                    }
                    else if (player.GetStamina() >= VL_Utility.GetFrostNovaCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetFrostNovaCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetFrostNovaCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                        //Effects, animations, and sounds
                        //player.StartEmote("cheer");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("swing_axe1");                        
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_activate"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects
                        if(player.GetSEMan().HaveStatusEffect("Burning"))
                        {
                            player.GetSEMan().RemoveStatusEffect("Burning".GetStableHashCode());
                        }

                        List<Character> allCharacters = Character.GetAllCharacters();                        
                        foreach (Character ch in allCharacters)
                        {
                            if (BaseAI.IsEnemy(player, ch) && ((ch.transform.position - player.transform.position).magnitude <= (10f + (.1f * sLevel))) && VL_Utility.LOS_IsValid(ch, player.GetCenterPoint(), player.transform.position + player.transform.up * .15f))
                            {
                                Vector3 direction = (ch.transform.position - player.transform.position);
                                HitData hitData = new HitData();
                                hitData.m_damage.m_frost = UnityEngine.Random.Range(10 + (.5f * sLevel), 20 + sLevel) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_mageFrostNova;
                                hitData.m_pushForce = 20f;
                                hitData.m_point = ch.GetEyePoint();
                                hitData.m_dir = direction;
                                hitData.m_skill = ValheimLegends.EvocationSkill;
                                ch.Damage(hitData);
                                SE_Slow se_frost = (SE_Slow)ScriptableObject.CreateInstance(typeof(SE_Slow));
                                ch.GetSEMan().AddStatusEffect(se_frost.name.GetStableHashCode(), true);
                            }
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetFrostNovaSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Frost Nova: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetFrostNovaCost + ")");
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
                    float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;
                    if (player.IsBlocking())
                    {
                        if (player.GetStamina() >= VL_Utility.GetFireballCost * .5f)
                        {
                            //Ability Cooldown
                            StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                            se_cd.m_ttl = .5f;
                            player.GetSEMan().AddStatusEffect(se_cd);

                            //Ability Cost
                            player.UseStamina(VL_Utility.GetFireballCost * .5f);

                            //Effects, animations, and sounds
                            ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("swing_axe2");
                            ValheimLegends.isChargingDash = true;
                            ValheimLegends.dashCounter = 0;
                            QueuedAttack = MageAttackType.IceDagger;

                            //Skill gain
                            player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetFireballSkillGain * .5f);
                        }
                        else
                        {
                            player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Ice Dagger: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetFireballCost * .5f + ")");
                        }
                    }
                    else if(player.GetStamina() >= (VL_Utility.GetFireballCost + (.5f * sLevel)))
                    { 
                        ValheimLegends.shouldUseGuardianPower = false;
                        //Skill influence
                        

                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetFireballCooldownTime - (.02f * sLevel);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetFireballCost + (.5f * sLevel));

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("gpower");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetSpeed(3f);
                        //player.StartEmote("point");
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_Flames"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects

                        Vector3 vector = player.transform.position + player.transform.up * 2.5f + player.GetLookDir() * .5f;//player.transform.position + player.transform.up * 1.25f + player.GetLookDir() * 2f;
                        GameObject prefab = ZNetScene.instance.GetPrefab("Imp_fireball_projectile");
                        GO_Fireball = UnityEngine.Object.Instantiate(prefab, new Vector3(vector.x, vector.y, vector.z), Quaternion.identity);
                        P_Fireball = GO_Fireball.GetComponent<Projectile>();
                        P_Fireball.name = "Fireball";
                        P_Fireball.m_respawnItemOnHit = false;
                        P_Fireball.m_spawnOnHit = null;
                        P_Fireball.m_ttl = 60f;
                        P_Fireball.m_gravity = 2.5f;
                        P_Fireball.m_rayRadius = .1f;
                        P_Fireball.m_aoe = 3f + (.03f * sLevel);
                        P_Fireball.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                        GO_Fireball.transform.localScale = Vector3.zero;

                        RaycastHit hitInfo = default(RaycastHit);
                        Vector3 position = player.transform.position;
                        Vector3 target = (!Physics.Raycast(vector, player.GetLookDir(), out hitInfo, float.PositiveInfinity, Script_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                        HitData hitData = new HitData();
                        hitData.m_damage.m_fire = UnityEngine.Random.Range(5f + (1.6f *sLevel), 10f + (1.8f * sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_mageFireball;
                        hitData.m_damage.m_blunt = UnityEngine.Random.Range(5f + (.9f *sLevel), 10f + (1.1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_mageFireball;
                        hitData.m_pushForce = 2f;
                        hitData.m_skill = ValheimLegends.EvocationSkill;
                        hitData.SetAttacker(player);
                        Vector3 a = Vector3.MoveTowards(GO_Fireball.transform.position, target, 1f);
                        P_Fireball.Setup(player, (a - GO_Fireball.transform.position) * 25f, -1f, hitData, null, null);
                        Traverse.Create(root: P_Fireball).Field("m_skill").SetValue(ValheimLegends.EvocationSkill);
                        GO_Fireball = null;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetFireballSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to for Fireball: (" + player.GetStamina().ToString("#.#") + "/" + (VL_Utility.GetFireballCost + (.5f * sLevel)) +")");
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
