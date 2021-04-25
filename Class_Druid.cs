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
    public class Class_Druid
    {
        private static int Script_Layermask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "character");        

        private static GameObject GO_CastFX;

        private static GameObject GO_Root;        
        private static Projectile P_Root;
        private static StatusEffect SE_Root;

        private static GameObject GO_RootDefender;
    
        private static int rootCount;
        private static int rootCountTrigger;

        public static void Process_Input(Player player, float altitude)
        {
            System.Random rnd = new System.Random();
            Vector3 rootVec = default(Vector3);
            if (VL_Utility.Ability3_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                {
                    ValheimLegends.shouldUseGuardianPower = false;
                    //player.Message(MessageHud.MessageType.Center, "root - starting");
                    if (player.GetStamina() >= VL_Utility.GetRootCost && !ValheimLegends.isChanneling)
                    {
                        ValheimLegends.isChanneling = true;
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability3_CD)ScriptableObject.CreateInstance(typeof(SE_Ability3_CD));
                        se_cd.m_ttl = VL_Utility.GetRootCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetRootCost);

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(.3f);

                        //Lingering effects

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.ConjurationSkillDef).m_level;

                        //Apply effects
                        rootCount = 0;
                        rootCountTrigger = 32 - Mathf.RoundToInt(.12f * sLevel);
                        Vector3 shiftVec = player.transform.right * 2.5f;
                        if (UnityEngine.Random.Range(0f, 1f) < .5f)
                        {
                            shiftVec *= -1f;
                        }
                        rootVec = player.transform.position + player.transform.up * 3f + player.GetLookDir() * 2f + shiftVec;
                        //rootVec.x += rnd.Next(-2, 2);
                        //rootVec.z += rnd.Next(-2, 2);

                        GameObject prefab = ZNetScene.instance.GetPrefab("gdking_root_projectile");
                        GO_Root = UnityEngine.Object.Instantiate(prefab, new Vector3(rootVec.x, rootVec.y, rootVec.z), Quaternion.identity);
                        P_Root = GO_Root.GetComponent<Projectile>();
                        P_Root.name = "Root";
                        P_Root.m_respawnItemOnHit = false;
                        P_Root.m_spawnOnHit = null;
                        P_Root.m_ttl = 35f;
                        P_Root.m_gravity = 0f;
                        P_Root.m_rayRadius = .1f;                        
                        Traverse.Create(root: P_Root).Field("m_skill").SetValue(ValheimLegends.ConjurationSkill);
                        P_Root.transform.localRotation = Quaternion.LookRotation(player.GetLookDir());
                        GO_Root.transform.localScale = Vector3.one * 1.5f;

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.ConjurationSkill, VL_Utility.GetRootSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to channel Root: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetRootCost + ")");
                    }
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, "Ability not ready");
                }
            }
            else if (VL_Utility.Ability3_Input_Pressed && player.GetStamina() > VL_Utility.GetRootCostPerUpdate && ValheimLegends.isChanneling && Mathf.Max(0f, altitude - player.transform.position.y) <= 2f)
            {
                rootCount++;
                VL_Utility.SetTimer();
                player.UseStamina(VL_Utility.GetRootCostPerUpdate);
                //player.transform.rotation = Quaternion.LookRotation(player.GetLookDir());
                ValheimLegends.isChanneling = true;
                if (rootCount >= rootCountTrigger)
                {
                    //Skill gain
                    player.RaiseSkill(ValheimLegends.ConjurationSkill, .06f);
                    float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.ConjurationSkillDef).m_level;
                    //player.Message(MessageHud.MessageType.Center, "root - fire");
                    rootCount = 0;
                    if (GO_Root != null && GO_Root.transform != null)
                    {
                        RaycastHit hitInfo = default(RaycastHit);
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("unarmed_attack0");
                        Vector3 position = player.transform.position;
                        Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, Script_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                        HitData hitData = new HitData();
                        hitData.m_damage.m_pierce = UnityEngine.Random.Range(10f + (.6f * sLevel), 15 + (1.2f * sLevel)) * VL_GlobalConfigs.g_DamageModifer;
                        hitData.m_pushForce = 2f;
                        Vector3 a = Vector3.MoveTowards(GO_Root.transform.position, target, 1f);
                        if (P_Root != null && P_Root.name == "Root")
                        {
                            P_Root.Setup(player, (a - GO_Root.transform.position) * 75f, -1f, hitData, null);
                            Traverse.Create(root: P_Root).Field("m_skill").SetValue(ValheimLegends.ConjurationSkill);
                        }
                    }
                    GO_Root = null;

                    Vector3 shiftVec = player.transform.right * 2.5f;
                    if (UnityEngine.Random.Range(0f, 1f) < .5f)
                    {
                        shiftVec *= -1f;
                    }
                    rootVec = player.transform.position + player.transform.up * 3f + player.GetLookDir() * 2f + shiftVec;
                    //rootVec.x += rnd.Next(-2, 2);
                    //rootVec.z += rnd.Next(-2, 2);

                    GameObject prefab = ZNetScene.instance.GetPrefab("gdking_root_projectile");
                    GO_Root = UnityEngine.Object.Instantiate(prefab, new Vector3(rootVec.x, rootVec.y, rootVec.z), Quaternion.identity);
                    P_Root = GO_Root.GetComponent<Projectile>();
                    P_Root.name = "Root";
                    P_Root.m_respawnItemOnHit = false;
                    P_Root.m_spawnOnHit = null;
                    P_Root.m_ttl = rootCountTrigger + 1;
                    P_Root.m_gravity = 0f;
                    P_Root.m_rayRadius = .1f;
                    Traverse.Create(root: P_Root).Field("m_skill").SetValue(ValheimLegends.ConjurationSkill);
                    P_Root.transform.localRotation = Quaternion.LookRotation(player.GetLookDir());
                    GO_Root.transform.localScale = Vector3.one * 1.5f;
                    //P_Root.Setup(player, Vector3.zero, -1f, null, null);
                }
            }
            else if (((VL_Utility.Ability3_Input_Up || player.GetStamina() <= VL_Utility.GetRootCostPerUpdate) && ValheimLegends.isChanneling) || Mathf.Max(0f, altitude - player.transform.position.y) > 2f)
            {
                //player.Message(MessageHud.MessageType.Center, "root - deactivate");
                //ZLog.Log("altitude is " + altitude);
                if (GO_Root != null && GO_Root.transform != null)
                {
                    RaycastHit hitInfo = default(RaycastHit);
                    //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("unarmed_attack0");
                    Vector3 position = player.transform.position;
                    Vector3 target = (!Physics.Raycast(player.GetEyePoint(), player.GetLookDir(), out hitInfo, float.PositiveInfinity, Script_Layermask) || !(bool)hitInfo.collider) ? (position + player.GetLookDir() * 1000f) : hitInfo.point;
                    HitData hitData = new HitData();
                    hitData.m_damage.m_pierce = 10f;
                    hitData.m_pushForce = 10f;
                    Vector3 a = Vector3.MoveTowards(GO_Root.transform.position, target, 1f);
                    P_Root.Setup(player, (a - GO_Root.transform.position) * 65f, -1f, hitData, null);
                    Traverse.Create(root: P_Root).Field("m_skill").SetValue(ValheimLegends.ConjurationSkill);
                }
                //GO_Root.SetActive(false);
                GO_Root = null;
                ValheimLegends.isChanneling = false;
            }
            else if(VL_Utility.Ability2_Input_Down)
            {
                if (!player.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                {
                    //player.Message(MessageHud.MessageType.Center, "Plant defenders");
                    if (player.GetStamina() >= VL_Utility.GetDefenderCost)
                    {
                        Vector3 lookVec = player.GetLookDir();
                        lookVec.y = 0f;
                        player.transform.rotation = Quaternion.LookRotation(lookVec);

                        ValheimLegends.shouldUseGuardianPower = false;
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability2_CD)ScriptableObject.CreateInstance(typeof(SE_Ability2_CD));
                        se_cd.m_ttl = VL_Utility.GetDefenderCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetDefenderCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.ConjurationSkillDef).m_level;

                        //Effects, animations, and sounds
                        ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetSpeed(.7f);
                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_WishbonePing"), player.transform.position, Quaternion.identity);

                        //Lingering effects

                        //Apply effects
                        GameObject prefab = ZNetScene.instance.GetPrefab("TentaRoot"); //ZNetScene.instance.GetPrefab("Deathsquito");  //
                        CharacterTimedDestruction td = prefab.GetComponent<CharacterTimedDestruction>();
                        if (td != null)
                        {
                            //ZLog.Log("td valid: " + td.isActiveAndEnabled + " timeout min " + td.m_timeoutMin + " timeout max " + td.m_timeoutMax);
                            td.m_timeoutMin = 24f + (.3f *sLevel);
                            td.m_timeoutMax = td.m_timeoutMin;
                        }
                        List<Vector3> rootVecs = new List<Vector3>();
                        rootVecs.Clear();
                        rootVec = player.transform.position + (player.GetLookDir() * 5f) + (player.transform.right * 5f);
                        rootVecs.Add(rootVec);
                        rootVec = player.transform.position + (player.GetLookDir() * 5f) + (player.transform.right * 5f * -1f);
                        rootVecs.Add(rootVec);
                        rootVec = player.transform.position + (player.GetLookDir() * 5f * -1f);
                        rootVecs.Add(rootVec);
                        
                        for(int i = 0; i < rootVecs.Count; i++)
                        {
                            GO_RootDefender = UnityEngine.Object.Instantiate(prefab, rootVecs[i], Quaternion.identity);
                            Character ch = GO_RootDefender.GetComponent<Character>();

                            if (ch != null)
                            {
                                SE_RootsBuff se_RootsBuff = (SE_RootsBuff)ScriptableObject.CreateInstance(typeof(SE_RootsBuff));
                                se_RootsBuff.m_ttl = SE_RootsBuff.m_baseTTL;
                                se_RootsBuff.damageModifier = .5f + (.015f * sLevel) * VL_GlobalConfigs.g_DamageModifer;
                                se_RootsBuff.staminaRegen = .5f + (.05f * sLevel);
                                se_RootsBuff.summoner = player;
                                se_RootsBuff.centerPoint = player.transform.position;
                                ch.GetSEMan().AddStatusEffect(se_RootsBuff);

                                ch.SetMaxHealth(30f + (6f * sLevel));
                                ch.transform.localScale = (.75f + (.005f * sLevel)) * Vector3.one;
                                ch.m_faction = Character.Faction.Players;
                                ch.SetTamed(true);
                            }
                            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_Potion_stamina_medium"), ch.transform.position, Quaternion.identity);
                        }

                        //Deathsquito's actually
                        GameObject prefab2 = ZNetScene.instance.GetPrefab("VL_Deathsquit");  //
                        CharacterTimedDestruction td2 = prefab2.GetComponent<CharacterTimedDestruction>();
                        if (td2 != null)
                        {
                            //ZLog.Log("td valid: " + td2.isActiveAndEnabled + " timeout min " + td2.m_timeoutMin + " timeout max " + td2.m_timeoutMax);
                            td2.m_timeoutMin = 24f + (.3f * sLevel);
                            td2.m_timeoutMax = td2.m_timeoutMin;
                        }
                        int rootDefenderCount = 2 + Mathf.RoundToInt(.05f * sLevel);
                        for (int i = 0; i < rootDefenderCount; i++)
                        {
                            rootVec = player.transform.position + player.transform.up * 4f + (player.GetLookDir() * UnityEngine.Random.Range(-(5f + .1f * sLevel), (5f + .1f * sLevel)) + player.transform.right * UnityEngine.Random.Range(-(5f + .1f * sLevel), (5f + .1f * sLevel)));
                            GameObject go_deathsquit = UnityEngine.Object.Instantiate(prefab2, rootVec, Quaternion.identity);                           
                            
                            Character ch2 = go_deathsquit.GetComponent<Character>();
                            ch2.m_name = "Drusquito";
                            if (ch2 != null)
                            {
                                SE_Companion se_companion = (SE_Companion)ScriptableObject.CreateInstance(typeof(SE_Companion));
                                se_companion.m_ttl = 60f;
                                se_companion.damageModifier = .05f + (.0075f * sLevel) * VL_GlobalConfigs.g_DamageModifer;
                                se_companion.summoner = player;
                                ch2.GetSEMan().AddStatusEffect(se_companion);

                                ch2.transform.localScale = (.4f + (.005f * sLevel)) * Vector3.one;
                                ch2.m_faction = Character.Faction.Players;
                                ch2.SetTamed(true);
                                //MonsterAI ai = ch2.GetBaseAI() as MonsterAI;
                                //ai.SetFollowTarget(player.gameObject);
                                //CharacterDrop comp = ch2.GetComponent<CharacterDrop>();
                                //if (comp != null)
                                //{
                                //    comp.m_drops.Clear();
                                //}
                            }
                            UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_float_hitwater"), ch2.transform.position, Quaternion.identity);
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.ConjurationSkill, VL_Utility.GetDefenderSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to summon root defenders: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetDefenderCost + ")");
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
                    //player.Message(MessageHud.MessageType.Center, "Regeneration");
                    if (player.GetStamina() >= VL_Utility.GetRegenerationCost)
                    {
                        //Ability Cooldown
                        StatusEffect se_cd = (SE_Ability1_CD)ScriptableObject.CreateInstance(typeof(SE_Ability1_CD));
                        se_cd.m_ttl = VL_Utility.GetRegenerationCooldownTime;
                        player.GetSEMan().AddStatusEffect(se_cd);

                        //Ability Cost
                        player.UseStamina(VL_Utility.GetRegenerationCost);

                        //Skill influence
                        float sLevel = player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AlterationSkillDef).m_level;

                        //Effects, animations, and sounds
                        //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("gpower");
                        player.StartEmote("cheer");
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_guardstone_permitted_add"), player.GetCenterPoint(), Quaternion.identity);
                        GO_CastFX = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_WishbonePing"), player.transform.position, Quaternion.identity);

                        //Lingering effects
                        SE_Regeneration se_regen = (SE_Regeneration)ScriptableObject.CreateInstance(typeof(SE_Regeneration));
                        se_regen.m_ttl = SE_Regeneration.m_baseTTL;
                        se_regen.m_icon = ZNetScene.instance.GetPrefab("TrophyGreydwarfShaman").GetComponent<ItemDrop>().m_itemData.GetIcon();
                        se_regen.m_HealAmount = .5f + (.4f * sLevel) * VL_GlobalConfigs.g_DamageModifer;
                        se_regen.doOnce = false;

                        //Apply effects
                        List<Player> allPlayers = new List<Player>();
                        allPlayers.Clear();
                        Player.GetPlayersInRange(player.GetCenterPoint(), 30f + (.2f * sLevel), allPlayers);
                        foreach (Player p in allPlayers)
                        {
                            if(p == Player.m_localPlayer)
                            {
                                p.GetSEMan().AddStatusEffect(se_regen, true);
                            }
                            else
                            {
                                p.GetSEMan().AddStatusEffect(se_regen.name, true);
                            }
                        }

                        //Skill gain
                        player.RaiseSkill(ValheimLegends.AlterationSkill, VL_Utility.GetRegenerationSkillGain);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.TopLeft, "Not enough stamina to for Regeneration: (" + player.GetStamina().ToString("#.#") + "/" + VL_Utility.GetRegenerationCost + ")");
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
