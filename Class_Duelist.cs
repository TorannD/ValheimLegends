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
    public class Class_Duelist
    {
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "character", "character_net", "character_ghost");

        private static GameObject GO_CastFX;

        private static GameObject GO_QuickShot;        
        private static Projectile P_QuickShot;

        public static void Execute_Slash(Player player)
        {
            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_BlinkStrike"), player.GetCenterPoint() + player.GetLookDir() * 3f, Quaternion.LookRotation(player.GetLookDir()));

            //Skill influence
            float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
            float sDamageMultiplier = .6f + (sLevel * .006f) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_duelistSeismicSlash;

            //Apply effects                        
            Vector3 center = player.GetEyePoint() + player.GetLookDir() * 6f;
            List<Character> allCharacters = new List<Character>();
            allCharacters.Clear();
            Character.GetCharactersInRange(center, 6f, allCharacters);
            foreach (Character ch in allCharacters)
            {
                if (BaseAI.IsEnemy(player, ch))
                {
                    Vector3 direction = (ch.transform.position - player.transform.position);
                    HitData hitData = new HitData();
                    hitData.m_damage = player.GetCurrentWeapon().GetDamage();
                    hitData.ApplyModifier(UnityEngine.Random.Range(.8f, 1.2f) * sDamageMultiplier);
                    hitData.m_pushForce = 25f + (.1f * sLevel);
                    hitData.m_point = ch.GetEyePoint();
                    hitData.m_dir = (player.transform.position - ch.transform.position);
                    hitData.m_skill = ValheimLegends.DisciplineSkill;
                    ch.Damage(hitData);
                }
            }
        }

        public static void Process_Input(Player player)
        {
            System.Random rnd = new System.Random();
            if (VL_Utility.Ability3_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    if (player.GetStamina() >= VL_Utility.GetBlinkStrikeCost)
                    {
                        
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetBlinkStrikeCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetBlinkStrikeCost);

                        //Effects, animations, and sounds
                        
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("knife_stab1");

                        //VL_Utility.RotatePlayerToTarget(player);
                        //Lingering effects
                        ValheimLegends.isChanneling = true;
                        ValheimLegends.isChargingDash = true;
                        ValheimLegends.dashCounter = 0;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetBlinkStrikeSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for S. Slash : (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetBlinkStrikeCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }            
            else if(VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Frost Nova");
                    if (player.GetStamina() >= VL_Utility.GetRiposteCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetRiposteCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetRiposteCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;

                        //Effects, animations, and sounds
                        //player.StartEmote("cheer");                        
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_perfectblock"), player.transform.position, Quaternion.identity);
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_backstab"), player.GetCenterPoint(), Quaternion.identity);

                        //Lingering effects
                        SE_Riposte SE_riposte = (SE_Riposte)ScriptableObject.CreateInstance(typeof(SE_Riposte));                        
                        player.GetSEMan().AddStatusEffect(SE_riposte);
                        //Apply effects

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetRiposteSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Riposte: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetRiposteCost + ")");
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
                    //player.Message(MessageHud.MessageType.Center, "QuickShot");
                    
                    if (player.GetStamina() >= (VL_Utility.GetQuickShotCost))
                    {
                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;                        
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetQuickShotCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetQuickShotCost + (.5f * sLevel));

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player)).SetTrigger("interact");
                        //player.StartEmote("point");
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_smelter_add"), player.transform.position, Quaternion.identity);
                        Vector3 vector = player.transform.position + player.transform.up * 1.2f + player.GetLookDir() *.5f;
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_VL_QuickShot"), vector, Quaternion.LookRotation(player.GetLookDir()));                        
                        //Lingering effects

                        //Apply effects
                        GameObject prefab = ZNetScene.instance.GetPrefab("Greydwarf_throw_projectile");
                        GO_QuickShot = UnityEngine.Object.Instantiate(prefab, vector, Quaternion.identity);
                        P_QuickShot = GO_QuickShot.GetComponent<Projectile>();
                        P_QuickShot.name = "QuickShot";
                        P_QuickShot.m_respawnItemOnHit = false;
                        P_QuickShot.m_spawnOnHit = null;
                        P_QuickShot.m_ttl = 10f;
                        P_QuickShot.m_gravity = 1.2f;
                        P_QuickShot.m_rayRadius = .05f;
                        P_QuickShot.m_hitNoise = 20;
                        P_QuickShot.transform.localRotation = Quaternion.LookRotation(player.GetAimDir(vector));
                        GO_QuickShot.transform.localScale = new Vector3(.6f, .6f, .6f);

                        RaycastHit hitInfo = default(RaycastHit);
                        //Vector3 position = player.transform.position;
                        Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, ScriptChar_Layermask) || !(bool)hitInfo.collider) ? (player.GetEyePoint() + player.GetLookDir() * 1000f) : hitInfo.point;
                        HitData hitData = new HitData();
                        hitData.m_damage.m_pierce = UnityEngine.Random.Range(5f + (1f *sLevel), 30f + (1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer * VL_GlobalConfigs.c_duelistHipShot;
                        hitData.m_pushForce = 1f;
                        hitData.m_skill = ValheimLegends.DisciplineSkill;
                        Vector3 a = Vector3.MoveTowards(vector, target, 1f);
                        P_QuickShot.Setup(player, (a - GO_QuickShot.transform.position) * 100f, -1f, hitData, null);
                        Traverse.Create(root: P_QuickShot).Field("m_skill").SetValue(ValheimLegends.DisciplineSkill);
                        GO_QuickShot = null;

                        VL_Utility.RotatePlayerToTarget(player);
                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetQuickShotSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to for Hip Shot: (" + player.GetStamina().ToString("#.#") + "/" + (VL_Utility.GetQuickShotCost + ")"));
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
