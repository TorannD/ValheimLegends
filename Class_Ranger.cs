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

namespace ValheimLegends
{
    public class Class_Ranger
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");        

        private static GameObject GO_CastFX;

        public static GameObject GO_Wolf;

        public static void Process_Input(Player player)
        {
            System.Random rnd = new System.Random();
            Vector3 pVec = default(Vector3);
            if (VL_Utility.Ability3_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    //adds a status effect where projectiles deal bonus damage, increases number of bow projectiles fired
                    //player.Message(MessageHud.MessageType.Center, "Power Shot");
                    if (player.GetStamina() >= VL_Utility.GetPowerShotCost(player))
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetPowerShotCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetPowerShotCost(player));

                        //Effects, animations, and sounds
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("cheer");
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_crit"), player.GetCenterPoint(), Quaternion.identity);

                        //Lingering effects
                        SE_PowerShot se_powershot = (SE_PowerShot)ScriptableObject.CreateInstance(typeof(SE_PowerShot));
                        se_powershot.m_ttl = SE_PowerShot.m_baseTTL + Mathf.RoundToInt(.05f * player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level);
                        se_powershot.hitCount = Mathf.RoundToInt(3f + .05f * player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level);

                        //Apply effects
                        if (player.GetSEMan().HaveStatusEffect("SE_VL_PowerShot"))
                        {
                            StatusEffect se_pw_rem = player.GetSEMan().GetStatusEffect("SE_VL_PowerShot");
                            player.GetSEMan().RemoveStatusEffect(se_pw_rem);
                        }
                        player.GetSEMan().AddStatusEffect(se_powershot);

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetPowerShotSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Power Shot: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetPowerShotCost(player) + ")");
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
                    //summons a wolf companion
                    //player.Message(MessageHud.MessageType.Center, "Summon Wolf");
                    if (player.GetStamina() >= VL_Utility.GetSummonWolfCost(player))
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetSummonWolfCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetSummonWolfCost(player));

                        //Effects, animations, and sounds
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("cheer");
                        player.StartEmote("cheer", true);
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(1f);                        

                        //Lingering effects

                        //Apply effects
                        pVec = player.transform.position + player.transform.forward * 4f;
                        
                        GameObject prefab = ZNetScene.instance.GetPrefab("Wolf");
                        GO_Wolf = UnityEngine.Object.Instantiate(prefab, pVec, Quaternion.identity);
                        Character ch = GO_Wolf.GetComponent<Character>();
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_Potion_stamina_medium"), ch.transform.position, Quaternion.identity);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_WishbonePing"), ch.transform.position, Quaternion.identity);
                        if (ch != null)
                        {
                            ch.m_faction = Character.Faction.Players;
                            ch.SetTamed(true);
                            float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.ConjurationSkillDef).m_level;
                            ch.SetMaxHealth(25 + (7 + sLevel));
                            ch.transform.localScale = (.5f + (.01f * sLevel)) * Vector3.one;
                            ch.m_swimSpeed *= 2f;
                            SE_Companion se_companion = (SE_Companion)ScriptableObject.CreateInstance(typeof(SE_Companion));
                            se_companion.m_ttl = SE_Companion.m_baseTTL;
                            se_companion.damageModifier = .05f + (.01f * sLevel);
                            se_companion.healthRegen = .5f + (.05f * sLevel);
                            se_companion.speedModifier = 1.2f;
                            se_companion.summoner = player;

                            CharacterDrop comp = ch.GetComponent<CharacterDrop>();
                            MonsterAI ai = ch.GetBaseAI() as MonsterAI;
                            ai.SetFollowTarget(player.gameObject);
                            if (comp != null)
                            {
                                comp.m_drops.Clear();
                                //foreach (CharacterDrop.Drop d in comp.m_drops)
                                //{
                                //    d.m_chance = 0f;
                                //}
                            }
                            ch.GetSEMan().AddStatusEffect(se_companion);
                            //apply wolf buff, buff prevents loot drops, makes wolf not suck
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.ConjurationSkill, VL_Utility.GetSummonWolfSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to Summon Wolf: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetSummonWolfCost(player) + ")");
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
                    //enables improved sneak, rapid movement during sneak
                    //player.Message(MessageHud.MessageType.Center, "Shadow stalk");
                    if (player.GetStamina() >= VL_Utility.GetShadowStalkCost(player))
                    {
                        //Skill Influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetShadowStalkCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetShadowStalkCost(player));

                        //Effects, animations, and sounds
                        GameObject effect = ZNetScene.instance.GetPrefab("vfx_odin_despawn");
                        UnityEngine.Object.Instantiate(effect, player.transform.position, Quaternion.identity);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_wraith_death"), player.transform.position, Quaternion.identity);

                        //Lingering effects
                        SE_ShadowStalk se_shadowstalk = (SE_ShadowStalk)ScriptableObject.CreateInstance(typeof(SE_ShadowStalk));
                        se_shadowstalk.m_ttl = 30f * (1f + (.03f * sLevel));
                        se_shadowstalk.speedAmount = 1.5f + (.01f * sLevel);
                        se_shadowstalk.speedDuration = 3f + (.03f * sLevel);

                        //Apply effects
                        player.GetSEMan().AddStatusEffect(se_shadowstalk);

                        List<Character> allCharacters = new List<Character>();
                        allCharacters.Clear();
                        Character.GetCharactersInRange(player.GetCenterPoint(), 500f, allCharacters);
                        foreach (Character ch in allCharacters)
                        {
                            if (ch.GetBaseAI() != null && ch.GetBaseAI() is MonsterAI && ch.GetBaseAI().IsEnemey(player))
                            {
                                MonsterAI ai = ch.GetBaseAI() as MonsterAI;
                                if(ai.GetTargetCreature() == player)
                                {
                                    Traverse.Create(root: ai).Field("m_alerted").SetValue(false);
                                    Traverse.Create(root: ai).Field("m_targetCreature").SetValue(null);
                                }
                            }
                        }


                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetShadowStalkSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Shadow Stalk: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetShadowStalkCost(player) + ")");
                    }                    
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
        }
    }
}
