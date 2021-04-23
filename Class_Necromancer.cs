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
    public class Class_Necromancer
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");
        private static int ScriptChar_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "character");

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

        public static void Process_Input(Player player)
        {
            System.Random rnd = new System.Random();
            if (VL_Utility.Ability3_Input_Down && !meteorCharging)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Meteor - starting");
                    if (player.GetStamina() >= VL_Utility.GetMeteorCost)
                    {

                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetMeteorCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetMeteorCost);

                        //Skill influence

                        //Effects, animations, and sounds

                        //Lingering effects


                        //Apply effects


                        //Skill gain
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
            else if (VL_Utility.Ability3_Input_Pressed && meteorCharging && player.GetStamina() > 1)
            {
                
            }
            else if((VL_Utility.Ability3_Input_Up || player.GetStamina() <= 1 && meteorCharging))
            { 

            }
            else if(VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Frost Nova");
                    if (player.GetStamina() >= VL_Utility.GetFrostNovaCost)
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

                        //Lingering effects

                        //Apply effects


                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetFrostNovaSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Frost Nova: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetDefenderCost + ")");
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
                    if (player.GetStamina() >= (VL_Utility.GetFireballCost))
                    {

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetFireballCooldownTime - (.02f * sLevel);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetFireballCost + (.5f * sLevel));

                        //Effects, animations, and sounds

                        //Lingering effects

                        //Apply effects

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetFireballSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to for Fireball: (" + player.GetStamina().ToString("#.#") + "/" + (VL_Utility.GetFireballCost) +")");
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
