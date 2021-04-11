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
    public class Class_Shaman
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock");        

        private static GameObject GO_CastFX;

        public static void Process_Input(Player player)
        {
            System.Random rnd = new System.Random();
            Vector3 pVec = default(Vector3);
            if (VL_Utility.Ability3_Input_Down)
            {
                //player.Message(MessageHud.MessageType.Center, "Spirit Bomb"); //deals moderate spirit damage in PBAoE and applies spirit dot
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    if (player.GetStamina() > VL_Utility.GetSpiritBombCost(player))
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetSpiritBombCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetSpiritBombCost(player));

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.EvocationSkillDef).m_level;

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("battleaxe_attack1");
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_goblinking_nova"), player.transform.position, Quaternion.identity);

                        //Lingering effects
                        SE_SpiritDrain se_spiritdrain = (SE_SpiritDrain)ScriptableObject.CreateInstance(typeof(SE_SpiritDrain));
                        se_spiritdrain.m_ttl = SE_SpiritDrain.m_baseTTL;
                        se_spiritdrain.damageModifier = 1f + (.1f * sLevel);

                        //Apply effects
                        List<Character> allCharacters = Character.GetAllCharacters();
                        foreach (Character ch in allCharacters)
                        {
                            if ((BaseAI.IsEnemy(player, ch) && (ch.transform.position - player.transform.position).magnitude <= 11f + (.05f * sLevel)))
                            {
                                Vector3 direction = (ch.transform.position - player.transform.position);
                                HitData hitData = new HitData();
                                hitData.m_damage.m_spirit = UnityEngine.Random.Range(12f + (.75f * sLevel), 24f + (1.25f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                                hitData.m_pushForce = 25f + (.1f * sLevel);
                                hitData.m_point = ch.GetEyePoint();
                                hitData.m_dir = (player.transform.position - ch.transform.position);
                                hitData.m_skill = ValheimLegends.EvocationSkill;
                                ch.ApplyDamage(hitData, true, true, HitData.DamageModifier.Normal);
                                ch.GetSEMan().AddStatusEffect(se_spiritdrain);
                            }
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.EvocationSkill, VL_Utility.GetSpiritBombSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Spirit Shock: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetSpiritBombCost(player) + ")");
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
                    if (player.GetStamina() > VL_Utility.GetShellCost(player))
                    {
                        //player.Message(MessageHud.MessageType.Center, "Shell"); //add elemental resistances and spirt damage to attacks
                        //Ability Cooldown                        
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetShellCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetShellCost(player));

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AbjurationSkillDef).m_level;

                        //Effects, animations, and sounds
                        ValheimLegends.shouldUseGuardianPower = false;
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(1.25f);
                        //player.StartEmote("cheer");

                        //Lingering effects
                        
                        List<Player> allPlayers = new List<Player>();
                        allPlayers.Clear();
                        Player.GetPlayersInRange(player.transform.position, (30f + .2f * sLevel), allPlayers);
                        GameObject effect = ZNetScene.instance.GetPrefab("fx_guardstone_permitted_add");
                        foreach (Player p in allPlayers)
                        {
                            SE_Shell se_shell = (SE_Shell)ScriptableObject.CreateInstance(typeof(SE_Shell));
                            se_shell.m_ttl = SE_Shell.m_baseTTL + (.35f * sLevel);
                            se_shell.spiritDamageOffset = (6f + (.3f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                            se_shell.resistModifier = .6f - (.006f * sLevel);
                            se_shell.m_icon = ZNetScene.instance.GetPrefab("ShieldSerpentscale").GetComponent<ItemDrop>().m_itemData.GetIcon();
                            se_shell.doOnce = false;
                            if(p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(se_shell, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(se_shell.name, true);
                            }
                            UnityEngine.Object.Instantiate(effect, p.GetCenterPoint(), Quaternion.identity);                            
                        }

                        //Apply effects
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_wraith_death"), player.transform.position, Quaternion.identity);

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AbjurationSkill, VL_Utility.GetShellSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Shell: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetShellCost(player) + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (VL_Utility.Ability1_Input_Down)
            {
                //add movement speed, stamina regeneration
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability1_CD"))
                {
                    if (player.GetStamina() > VL_Utility.GetEnrageCost(player))
                    {
                        //player.Message(MessageHud.MessageType.Center, "Enrage");
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetEnrageCooldown(player);
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetEnrageCost(player));

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("challenge");
                        GameObject effectPlayer = ZNetScene.instance.GetPrefab("fx_guardstone_permitted_removed");
                        effectPlayer.transform.localScale = Vector3.one * 3f;
                        UnityEngine.Object.Instantiate(effectPlayer, player.GetCenterPoint(), Quaternion.identity);
                        //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_wraith_death"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //if, else added
                        //ObjectDB.instance.GetStatusEffect(se.name).m_ttl += 115; added

                        //Apply effects
                        GameObject effectApplied = ZNetScene.instance.GetPrefab("fx_GP_Activation");
                        List<Player> allPlayers = new List<Player>();
                        Player.GetPlayersInRange(player.transform.position, 30f, allPlayers);
                        SE_Enrage se_enrage = (SE_Enrage)ScriptableObject.CreateInstance(typeof(SE_Enrage));
                        se_enrage.m_ttl = 20f + (.2f * sLevel);
                        se_enrage.staminaModifier = (5f + (.1f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        se_enrage.speedModifier = 1.25f + (.0025f * sLevel); 
                        se_enrage.m_icon = ZNetScene.instance.GetPrefab("TrophyGoblinBrute").GetComponent<ItemDrop>().m_itemData.GetIcon();
                        se_enrage.doOnce = false;
                        
                        for(int i = 0; i < allPlayers.Count; i++)
                        {
                            if (allPlayers[i] == Player.m_localPlayer)
                            {
                                allPlayers[i].GetSEMan().AddStatusEffect(se_enrage, true);
                            }
                            else
                            {                                
                                allPlayers[i].GetSEMan().AddStatusEffect(se_enrage.name, true);
                                
                            }
                            UnityEngine.Object.Instantiate(effectApplied, allPlayers[i].GetCenterPoint(), Quaternion.identity); 
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetEnrageSkillGain(player));
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina for Enrage: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetEnrageCost(player) + ")");
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
