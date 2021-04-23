using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.IO;
using UnityEngine.UI;

namespace ValheimLegends
{
    [BepInPlugin("ValheimLegends", "ValheimLegends", "0.3.0")]
    public class ValheimLegends : BaseUnityPlugin
    {
        public static Harmony _Harmony;

        public const string Version = "0.3.0";
        public const float VersionF = 0.30f;
        public const string ModName = "Valheim Legends";
        public static bool playerEnabled = true;

        //loaded info
        public static List<VL_Player> vl_playerList;
        public static VL_Player vl_player;

        public static Sprite RiposteIcon;
        public static Sprite RogueIcon;
        public static Sprite MonkIcon;
        public static Sprite WeakenIcon;
        public static Sprite BiomeMeadowsIcon;
        public static Sprite BiomeBlackForestIcon;
        public static Sprite BiomeSwampIcon;
        public static Sprite BiomeMountainIcon;
        public static Sprite BiomePlainsIcon;
        public static Sprite BiomeOceanIcon;
        public static Sprite BiomeMistIcon;
        public static Sprite BiomeAshIcon;

        //configs
        //public static ConfigVariable<bool> modEnabled;
        //public static ConfigVariable<string> chosenClass;
        //public static ConfigVariable<bool> vl_svr_enforceConfigurationClass;
        //public static ConfigVariable<string> Ability1_Hotkey;
        //public static ConfigVariable<string> Ability1_Hotkey_Combo;
        //public static ConfigVariable<string> Ability2_Hotkey;
        //public static ConfigVariable<string> Ability2_Hotkey_Combo;
        //public static ConfigVariable<string> Ability3_Hotkey;
        //public static ConfigVariable<string> Ability3_Hotkey_Combo;

        //public static ConfigVariable<float> vl_svr_energyCostMultiplier;
        //public static ConfigVariable<float> vl_svr_cooldownMultiplier;
        //public static ConfigVariable<float> vl_svr_abilityDamageMultiplier;
        //public static ConfigVariable<float> vl_svr_skillGainMultiplier;

        //public static ConfigVariable<float> icon_X_Offset;
        //public static ConfigVariable<float> icon_Y_Offset;

        //public static ConfigVariable<bool> showAbilityIcons;
        //public static ConfigVariable<string> iconAlignment;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<string> Ability1_Hotkey;
        public static ConfigEntry<string> Ability1_Hotkey_Combo;
        public static ConfigEntry<string> Ability2_Hotkey;
        public static ConfigEntry<string> Ability2_Hotkey_Combo;
        public static ConfigEntry<string> Ability3_Hotkey;
        public static ConfigEntry<string> Ability3_Hotkey_Combo;

        public static ConfigEntry<string> vl_svr_energyCostMultiplier;
        public static ConfigEntry<string> vl_svr_cooldownMultiplier;
        public static ConfigEntry<string> vl_svr_abilityDamageMultiplier;
        public static ConfigEntry<string> vl_svr_skillGainMultiplier;

        public static ConfigEntry<float> icon_X_Offset;
        public static ConfigEntry<float> icon_Y_Offset;

        public static ConfigEntry<bool> showAbilityIcons;
        public static ConfigEntry<string> iconAlignment;

        public static ConfigEntry<string> chosenClass;
        public static ConfigEntry<bool> vl_svr_enforceConfigClass;
        public static readonly Color abilityCooldownColor = new Color(1f, .3f, .3f, .5f);

        //Save and load data

        public class VL_Player
        {
            public string vl_name;
            public PlayerClass vl_class;
        }

        public enum PlayerClass
        {
            None = 0,
            Berserker = 1,
            Druid = 2,
            Metavoker = 3,
            Mage = 4,
            Priest = 5,
            //Necromancer = 6,
            Monk = 7,
            Ranger = 8,
            Duelist = 9,
            Enchanter = 10,
            Rogue = 11,
            Shaman = 16,
            Valkyrie = 32
        }

        public static int GetPlayerClassNum
        {
            get
            {
                if(vl_player.vl_class == PlayerClass.Berserker)
                {
                    return (int)PlayerClass.Berserker;
                }
                else if(vl_player.vl_class == PlayerClass.Druid)
                {
                    return (int)PlayerClass.Druid;
                }
                else if (vl_player.vl_class == PlayerClass.Metavoker)
                {
                    return (int)PlayerClass.Metavoker;
                }
                else if (vl_player.vl_class == PlayerClass.Mage)
                {
                    return (int)PlayerClass.Mage;
                }
                else if (vl_player.vl_class == PlayerClass.Priest)
                {
                    return (int)PlayerClass.Priest;
                }
                //else if (vl_player.vl_class == PlayerClass.Necromancer)
                //{
                //    return (int)PlayerClass.Necromancer;
                //}
                else if (vl_player.vl_class == PlayerClass.Monk)
                {
                    return (int)PlayerClass.Monk;
                }
                else if (vl_player.vl_class == PlayerClass.Ranger)
                {
                    return (int)PlayerClass.Ranger;
                }
                else if (vl_player.vl_class == PlayerClass.Duelist)
                {
                    return (int)PlayerClass.Duelist;
                }
                else if (vl_player.vl_class == PlayerClass.Enchanter)
                {
                    return (int)PlayerClass.Enchanter;
                }
                else if (vl_player.vl_class == PlayerClass.Rogue)
                {
                    return (int)PlayerClass.Rogue;
                }
                else if (vl_player.vl_class == PlayerClass.Shaman)
                {
                    return (int)PlayerClass.Shaman;
                }
                else if (vl_player.vl_class == PlayerClass.Valkyrie)
                {
                    return (int)PlayerClass.Valkyrie;
                }                
                else
                {
                    return (int)PlayerClass.None;
                }
            }
        }

        //[HarmonyPatch(typeof(Odinflight.PlayerPatches.PlayerVisuals), "SetupPlayerVisuals")]
        //public static class Odinflight_patch
        //{
        //    public static void Postfix(Odinflight.PlayerPatches.PlayerVisuals __instance)
        //    {
        //        ZLog.Log("" + __instance.PlayerRAC);
        //        foreach(AnimationClip ac in __instance.PlayerRAC.animationClips)
        //        {
        //            ZLog.Log("animation clip name: " + ac.name);
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(ZNet), "Awake")]
        [HarmonyPriority(int.MaxValue)]
        public static class ZNet_VL_Register
        {
            public static void Postfix(ZNet __instance, ZRoutedRpc ___m_routedRpc)
            {
                ___m_routedRpc.Register<ZPackage>("VL_ConfigSync", VL_ConfigSync.RPC_VL_ConfigSync);
            }
        }

        public static long ServerID;
        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        public static class ConfigServerSync
        {
            private static void Postfix(ref ZNet __instance, ZRpc rpc)
            {
                MethodBase GetServerPeerID = AccessTools.Method(typeof(ZRoutedRpc), "GetServerPeerID", null, null);
                ServerID = (long)GetServerPeerID.Invoke(ZRoutedRpc.instance, new object[0]);
                if (!__instance.IsServer())
                {
                    ZRoutedRpc.instance.InvokeRoutedRPC(ServerID, "VL_ConfigSync", new object[] { new ZPackage() });
                }
            }
        }

        //[HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        //public static class ConfigServerSync
        //{
        //    private static void Postfix(ref ZNet __instance)
        //    {
        //        if (!__instance.IsServer())
        //        {
        //            ZLog.Log("-------------------- SENDING VL_CONFIGSYNC REQUEST");
        //            ZRoutedRpc.instance.InvokeRoutedRPC("VL_ConfigSync", new object[] { new ZPackage() });
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(PlayerProfile), "SavePlayerToDisk", null)]
        public class SaveVLPlayer_Patch
        {
            public static void Postfix(PlayerProfile __instance, string ___m_filename, string ___m_playerName)
            {
                try
                {
                    Directory.CreateDirectory(Utils.GetSaveDataPath() + "/characters/VL");
                    string text = Utils.GetSaveDataPath() + "/characters/VL/" + ___m_filename + "_vl.fch";
                    string text3 = Utils.GetSaveDataPath() + "/characters/VL/" + ___m_filename + "_vl.fch.new";
                    ZPackage zPackage = new ZPackage();
                    zPackage.Write(GetPlayerClassNum);
                    byte[] array = zPackage.GenerateHash();
                    byte[] array2 = zPackage.GetArray();
                    FileStream fileStream = File.Create(text3);
                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                    binaryWriter.Write(array2.Length);
                    binaryWriter.Write(array2);
                    binaryWriter.Write(array.Length);
                    binaryWriter.Write(array);
                    binaryWriter.Flush();
                    fileStream.Flush(flushToDisk: true);
                    fileStream.Close();
                    fileStream.Dispose();
                    if (File.Exists(text))
                    {
                        File.Delete(text);
                    }
                    File.Move(text3, text);
                }
                catch(NullReferenceException ex)
                {
                    //failed to save, return to normal process
                }
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), "LoadPlayerFromDisk", null)]
        public class LoadVLPlayer_Patch
        {
            public static void Postfix(PlayerProfile __instance, string ___m_filename, string ___m_playerName)
            {
                //ZLog.Log("Loading player: (" + ___m_playerName + ")");
                try
                {
                    if(vl_playerList == null)
                    {
                        vl_playerList = new List<VL_Player>();                        
                    }
                    vl_playerList.Clear();
                    ZPackage zPackage = LoadPlayerDataFromDisk(___m_filename);
                    if (zPackage == null)
                    {
                        //ZLog.LogWarning("No player data for valheim legends");
                        goto LoadExit;
                    }
                    int num = zPackage.ReadInt();
                    VL_Player newLegend = new VL_Player();
                    newLegend.vl_name = ___m_playerName;
                    newLegend.vl_class = (PlayerClass)num;
                    vl_playerList.Add(newLegend);
                    //ZLog.Log("VL adding " + ___m_playerName + " as class num " + num);
                }
                catch (Exception ex)
                {
                    ZLog.LogWarning("Exception while loading player VL profile: " + ex.ToString());
                }
                LoadExit:;
            }

            private static ZPackage LoadPlayerDataFromDisk(string m_filename)
            {
                string text = Utils.GetSaveDataPath() + "/characters/VL/" + m_filename + "_vl.fch";
                //ZLog.Log("Player load file is : (" + text + ")");
                FileStream fileStream;
                try
                {
                    fileStream = File.OpenRead(text);
                }
                catch
                {
                    //ZLog.Log("  failed to load " + text);
                    return null;
                }
                byte[] data;
                try
                {
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    int count = binaryReader.ReadInt32();
                    data = binaryReader.ReadBytes(count);
                    int count2 = binaryReader.ReadInt32();
                    binaryReader.ReadBytes(count2);
                }
                catch
                {
                    ZLog.LogError("  error loading VL player data");
                    fileStream.Dispose();
                    return null;
                }
                fileStream.Dispose();
                return new ZPackage(data);
            }
        }

        public static bool ClassIsValid
        {
            get
            {
                if(vl_player != null)
                {
                    return vl_player.vl_class != PlayerClass.None;
                }
                return false;
            }
        }

        private static readonly Type patchType = typeof(ValheimLegends);

        //objects
        public static Sprite Ability1_Sprite;
        public static Sprite Ability2_Sprite;
        public static Sprite Ability3_Sprite;

        public static string Ability1_Name;
        public static string Ability2_Name;
        public static string Ability3_Name;

        public static string Ability1_Description;
        public static string Ability2_Description;
        public static string Ability3_Description;

        public static List<RectTransform> abilitiesStatus = new List<RectTransform>();

        //global variables
        public static bool shouldUseGuardianPower = true;
        public static bool shouldValkyrieImpact = false;
        public static bool isChanneling = false;
        public static int channelingCancelDelay = 0;
        public static bool isChargingDash = false;
        public static int dashCounter = 0;
        public static int logCheck = 0;

        public static int animationCountdown = 0;

        //Skills
        public static readonly int DisciplineSkillID = 781;
        public static readonly int AbjurationSkillID = 791;
        public static readonly int AlterationSkillID = 792;                
        public static readonly int ConjurationSkillID = 793;
        public static readonly int EvocationSkillID = 794;
        public static readonly int IllusionSkillID = 795;

        public enum SkillName
        {
            Discipline = 781,
            Abjuration = 791,
            Alteration = 792,
            Conjuration = 793,
            Evocation = 794,
            Illusion = 795
        }

        public static Skills.SkillType DisciplineSkill = (Skills.SkillType)DisciplineSkillID;
        public static Skills.SkillType AbjurationSkill = (Skills.SkillType)AbjurationSkillID;
        public static Skills.SkillType AlterationSkill = (Skills.SkillType)AlterationSkillID;
        public static Skills.SkillType ConjurationSkill = (Skills.SkillType)ConjurationSkillID;
        public static Skills.SkillType EvocationSkill = (Skills.SkillType)EvocationSkillID;
        public static Skills.SkillType IllusionSkill = (Skills.SkillType)IllusionSkillID;

        public static Skills.SkillDef DisciplineSkillDef;
        public static Skills.SkillDef AbjurationSkillDef;
        public static Skills.SkillDef AlterationSkillDef;
        public static Skills.SkillDef ConjurationSkillDef;
        public static Skills.SkillDef EvocationSkillDef;
        public static Skills.SkillDef IllusionSkillDef;

        public static List<Skills.SkillDef> legendsSkills = new List<Skills.SkillDef>();

        //informational patches
        //[HarmonyPatch(typeof(StatusEffect), "Setup", null)]
        //public class MonitorStatusEffects_Patch
        //{
        //    public static void Postfix(StatusEffect __instance, string ___m_name, string ___m_category)
        //    {
        //        ZLog.Log("Setup status: (" + ___m_name + ") category: (" + ___m_category + ")");
        //    }
        //}

        //[HarmonyPatch(typeof(ZSyncAnimation), "SetTrigger", null)]
        //public class AnimationTrigger_Prevention_Patch
        //{
        //    public static bool Prefix(ZSyncAnimation __instance, string name, ref Animator ___m_animator)
        //    {
        //        if(name == "gpower")
        //        {
        //            ___m_animator.speed = 5f;
        //        }
        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(ZSyncAnimation), "RPC_SetTrigger", null)]
        //public class AnimationTrigger_Monitor_Patch
        //{
        //    public static void Postfix(ZSyncAnimation __instance, long sender, string name)
        //    {
        //        ZLog.Log("animation: " + name);
        //    }
        //}

        //[HarmonyPatch(typeof(ZNetScene), "RemoveObjects", null)]
        //public class AnimationTrigger_Monitor_Patch
        //{
        //    public static bool Prefix(ZNetScene __instance, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects, Dictionary<ZDO, ZNetView> ___m_instances, List<ZNetView> ___m_tempRemoved)
        //    {
        //        int frameCount = Time.frameCount;
        //        foreach (ZDO currentNearObject in currentNearObjects)
        //        {
        //            //ZLog.Log("rmv: near object - " + currentNearObject.m_type.ToString());
        //            currentNearObject.m_tempRemoveEarmark = frameCount;
        //        }
        //        foreach (ZDO currentDistantObject in currentDistantObjects)
        //        {
        //            //ZLog.Log("rmv: far object - " + currentDistantObject.m_type.ToString());
        //            currentDistantObject.m_tempRemoveEarmark = frameCount;
        //        }
        //        ___m_tempRemoved.Clear();
        //        foreach (ZNetView value in ___m_instances.Values)
        //        {
        //            //ZLog.Log("val name " +value.name);
        //            if (value.GetZDO().m_tempRemoveEarmark != frameCount)
        //            {
        //                if (value != null)
        //                {
        //                    ___m_tempRemoved.Add(value);
        //                }
        //                else
        //                {
        //                    ZLog.Log("not adding " + value);
        //                }
        //            }
        //        }
        //        for (int i = 0; i < ___m_tempRemoved.Count; i++)
        //        {
        //            ZNetView zNetView = ___m_tempRemoved[i];
        //            if (zNetView != null)
        //            {
        //                ZDO zDO = zNetView.GetZDO();
        //                if (zDO != null)
        //                {
        //                    zNetView.ResetZDO();
        //                    UnityEngine.Object.Destroy(zNetView.gameObject);
        //                    if (!zDO.m_persistent && zDO.IsOwner())
        //                    {
        //                        ZDOMan.instance.DestroyZDO(zDO);
        //                    }
        //                }
        //                else
        //                {
        //                    ZLog.Log("null zdo");
        //                }
        //                ___m_instances.Remove(zDO);
        //            }
        //            else
        //            {
        //                ZLog.Log("znet view " + ___m_tempRemoved[i]);
        //                ___m_tempRemoved.Remove(zNetView);
        //                i--;
        //            }
        //        }
        //        return false;
        //    }
        //}

        //[HarmonyPatch(typeof(CharacterTimedDestruction), "Trigger", new Type[]
        //{
        //    typeof(float)
        //})]
        //public class TimedDestruction_testpatch
        //{
        //    public static bool Prefix(CharacterTimedDestruction __instance, Character ___m_character)
        //    {
        //        ZLog.Log("destroying " + ___m_character.name + " from timed event of " + __instance.m_timeoutMin + "min " + __instance.m_timeoutMax + "max");
        //        return true;
        //    }
        //}

        //
        //console commands
        //

        [HarmonyPatch(typeof(Skills), "CheatRaiseSkill", null)]
        public class CheatRaiseSkill_VL_Patch
        {
            public static bool Prefix(Skills __instance, string name, float value, Player ___m_player)
            {
                if(VL_Console.CheatRaiseSkill(__instance, name, value, ___m_player))
                {
                    Console.instance.Print("Skill " + name + " raised " + value);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Console), "InputText", null)]
        public class Cheats_VL_Patch
        {
            public static void Postfix(Console __instance, InputField ___m_input)
            {
                if ((bool)ZNet.instance && ZNet.instance.IsServer() && (bool)Player.m_localPlayer && __instance.IsCheatsEnabled() && playerEnabled)
                {
                    string text = ___m_input.text;
                    string[] array = text.Split(' ');
                    if (array.Length > 1)
                    {
                        if (array[0] == "vl_changeclass")
                        {
                            string className = array[1];
                            VL_Console.CheatChangeClass(className);
                        }
                    }
                }
            }
        }

        //
        //mod patches
        //

        [HarmonyPatch(typeof(Player), "ActivateGuardianPower", null)]
        public class ActivatePowerPrevention_Patch
        {
            public static bool Prefix(Player __instance, ref bool __result)
            {                
                if (!shouldUseGuardianPower)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "OnDodgeMortal", null)]
        public class DodgeBreaksChanneling_Patch
        {
            public static void Postfix(Player __instance)
            {
                if (isChanneling)
                {
                    ValheimLegends.isChanneling = false;
                }
            }
        }

        [HarmonyPatch(typeof(Player), "StartGuardianPower", null)]
        public class StartPowerPrevention_Patch
        {
            public static bool Prefix(Player __instance, ref bool __result)
            {
                if (!shouldUseGuardianPower)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "CanMove", null)]
        public class CanMove_Casting_Patch
        {
            public static void Postfix(Player __instance, ref bool __result)
            {
                if(isChanneling)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Menu), "OnQuit", null)]
        public class QuitYes_Patch
        {
            public static bool Prefix()
            {
                RemoveSummonedWolf();
                return true;
            }
        }

        [HarmonyPatch(typeof(Menu), "OnLogout", null)]
        public class RemoveWolfOnLogout_Patch
        {
            public static bool Prefix()
            {
                RemoveSummonedWolf();
                return true;
            }
        }


        private static int Script_WolfAttackMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece_nonsolid", "terrain", "vehicle", "piece", "viewblock", "character", "character_noenv", "character_trigger");
        [HarmonyPatch(typeof(Attack), "Start", null)]
        public class ShadowWolfAttack_Patch
        {
            public static bool Prefix(Attack __instance, Humanoid character, Rigidbody body, ZSyncAnimation zanim, CharacterAnimEvent animEvent, VisEquipment visEquipment, ItemDrop.ItemData weapon, Attack previousAttack, float timeSinceLastAttack, float attackDrawPercentage)
            {
                if (character != null && character.m_name == "Shadow Wolf")
                {
                    Vector3 hitVec = character.GetEyePoint();
                    RaycastHit hitInfo = default(RaycastHit);
                    //ZLog.Log("hitVec position " + hitVec);
                    //Vector3 target = (!Physics.Raycast(hitVec, character.transform.forward, out hitInfo, 5f, Script_WolfAttackMask) || !(bool)hitInfo.collider) ? (character.transform.position + character.transform.forward * 5f) : hitInfo.point;
                    Physics.SphereCast(hitVec, 0.2f, character.transform.forward, out hitInfo, 2f, Script_WolfAttackMask);
                    if (hitInfo.collider != null && hitInfo.collider.gameObject != null)
                    {
                        //ZLog.Log("collider " + hitInfo.collider);
                        //ZLog.Log("collider distance " + hitInfo.distance);
                        Character ch;
                        hitInfo.collider.gameObject.TryGetComponent<Character>(out ch);
                        bool flag = ch != null;
                        if (ch == null)
                        {
                            ch = (Character)hitInfo.collider.GetComponentInParent(typeof(Character));
                            flag = ch != null;
                            if (ch == null)
                            {
                                ch = (Character)hitInfo.collider.GetComponentInChildren<Character>();
                                flag = ch != null;
                            }
                        }
                        if (flag)
                        {
                            //ZLog.Log("collider game object is character");
                            //ZLog.Log("" + ch.m_name + " position " + ch.transform.position + " distance of " + (ch.transform.position - hitVec).magnitude + "  away and center is " + (ch.GetCenterPoint() - hitVec).magnitude);
                            //ZLog.Log("hitting " + ch.m_name + " at range " + (ch.transform.position - hitVec).magnitude);
                            Vector3 direction = (hitVec - ch.GetEyePoint());
                            float dmgMod = UnityEngine.Random.Range(.6f, 1.2f);
                            if (character.GetSEMan().HaveStatusEffect("SE_VL_Companion"))
                            {
                                SE_Companion se_comp = (SE_Companion)character.GetSEMan().GetStatusEffect("SE_VL_Companion");
                                dmgMod *= se_comp.damageModifier;
                            }
                            HitData hitData = new HitData();
                            hitData.m_damage = weapon.GetDamage();
                            hitData.m_damage.m_slash = weapon.GetDamage().m_slash * dmgMod;
                            hitData.m_point = hitInfo.point;
                            hitData.m_dir = (character.transform.position - ch.transform.position);
                            hitData.m_skill = Skills.SkillType.Unarmed;
                            ch.Damage(hitData);
                        }
                    }
                }
                return true;
            }
        }

        private static void RemoveSummonedWolf()
        {
            foreach (Character ch in Character.GetAllCharacters())
            {
                if (ch != null && ch.GetSEMan() != null)
                {
                    if (ch.GetSEMan().HaveStatusEffect("SE_VL_Companion"))
                    {
                        SE_Companion se_c = ch.GetSEMan().GetStatusEffect("SE_VL_Companion") as SE_Companion;
                        if (se_c.summoner == Player.m_localPlayer)
                        {
                            ch.m_faction = Character.Faction.MountainMonsters;
                            HitData hit = new HitData();
                            hit.m_damage.m_slash = 9999f;
                            ch.Damage(hit);
                            UnityEngine.GameObject.Destroy(ch.gameObject);
                        }
                    }
                    else if(ch.GetSEMan().HaveStatusEffect("SE_VL_Charm"))
                    {
                        SE_Charm se_charm = (SE_Charm)ch.GetSEMan().GetStatusEffect("SE_VL_Charm");
                        ch.m_faction = se_charm.originalFaction;
                    }
                }
            }
        }


        [HarmonyPatch(typeof(BaseAI), "CanSenseTarget", new Type[]
        {
            typeof(Character)
        })]
        public class CanSee_Shadow_Patch
        {
            public static bool Prefix(BaseAI __instance, Character target, ref bool __result)
            {
                if (target != null)
                {
                    Player player = target as Player;
                    if (player != null && player.GetSEMan().HaveStatusEffect("SE_VL_ShadowStalk"))
                    {
                        if (player.IsCrouching())
                        {
                            __result = false;
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "UpdateGroundContact", null)]
        public class Valkyrie_ValidateHeight_Patch
        {
            public static bool Prefix(Character __instance, ref float ___m_maxAirAltitude, bool ___m_groundContact)
            {
                if (vl_player != null && vl_player.vl_class == PlayerClass.Monk && ___m_groundContact && Mathf.Max(0f, ___m_maxAirAltitude - __instance.transform.position.y) > 4f)
                {
                    ___m_maxAirAltitude -= 6;
                }
                return true;
            }

            public static void Postfix(Character __instance, float ___m_maxAirAltitude, bool ___m_groundContact)
            {
                if (__instance == Player.m_localPlayer)
                {
                    if (Class_Valkyrie.inFlight)
                    {
                        if (Mathf.Max(0f, ___m_maxAirAltitude - __instance.transform.position.y) > 1f)
                        {
                            ValheimLegends.shouldValkyrieImpact = true;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Character), "ResetGroundContact", null)]
        public class Valkyrie_GroundContact_Patch
        {
            public static void Postfix(Character __instance, float ___m_maxAirAltitude, bool ___m_groundContact)
            {
                if (__instance == Player.m_localPlayer && shouldValkyrieImpact)
                {
                    float maxAltitude = Mathf.Max(0f, ___m_maxAirAltitude - __instance.transform.position.y);
                    ValheimLegends.shouldValkyrieImpact = false;
                    if (vl_player.vl_class == PlayerClass.Valkyrie)
                    {
                        Class_Valkyrie.Impact_Effect(Player.m_localPlayer, maxAltitude);
                        Class_Valkyrie.inFlight = false;
                    }
                    if(vl_player.vl_class == PlayerClass.Monk)
                    {
                        Class_Monk.Impact_Effect(Player.m_localPlayer, maxAltitude);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(OfferingBowl), "UseItem", null)]
        public class OfferingForClass_Patch
        {
            public static bool Prefix(OfferingBowl __instance, Humanoid user, ItemDrop.ItemData item, Transform ___m_itemSpawnPoint, EffectList ___m_fuelAddedEffects, ref bool __result)
            {
                //ZLog.Log("offered item is " + item.m_shared.m_name + " to string " + item.ToString());
                int num = user.GetInventory().CountItems(item.m_shared.m_name);
                bool flag = false;
                if (item.m_shared.m_name.Contains("item_greydwarfeye") && vl_player.vl_class != PlayerClass.Shaman)
                {                    
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Shaman");
                    vl_player.vl_class = PlayerClass.Shaman;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_meat_raw") && vl_player.vl_class != PlayerClass.Ranger)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Ranger");
                    vl_player.vl_class = PlayerClass.Ranger;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_coal") && vl_player.vl_class != PlayerClass.Mage)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Mage");
                    vl_player.vl_class = PlayerClass.Mage;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_flint") && vl_player.vl_class != PlayerClass.Valkyrie)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Valkyrie");
                    vl_player.vl_class = PlayerClass.Valkyrie;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_dandelion") && vl_player.vl_class != PlayerClass.Druid)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Druid");
                    vl_player.vl_class = PlayerClass.Druid;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_bonefragments") && vl_player.vl_class != PlayerClass.Berserker)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Berserker");
                    vl_player.vl_class = PlayerClass.Berserker;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_raspberries") && vl_player.vl_class != PlayerClass.Metavoker)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Metavoker");
                    vl_player.vl_class = PlayerClass.Metavoker;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_stone") && vl_player.vl_class != PlayerClass.Priest)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Priest");
                    vl_player.vl_class = PlayerClass.Priest;
                    flag = true;
                }
                //else if (item.m_shared.m_name.Contains("item_trophy_skeleton") && vl_player.vl_class != PlayerClass.Necromancer)
                //{
                //    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Necromancer");
                //    vl_player.vl_class = PlayerClass.Necromancer;
                //    flag = true;
                //}
                else if (item.m_shared.m_name.Contains("item_wood") && vl_player.vl_class != PlayerClass.Monk)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Monk");
                    vl_player.vl_class = PlayerClass.Monk;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_thistle") && vl_player.vl_class != PlayerClass.Duelist)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Duelist");
                    vl_player.vl_class = PlayerClass.Duelist;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_resin") && vl_player.vl_class != PlayerClass.Enchanter)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Enchanter");
                    vl_player.vl_class = PlayerClass.Enchanter;
                    flag = true;
                }
                else if (item.m_shared.m_name.Contains("item_honey") && vl_player.vl_class != PlayerClass.Rogue)
                {
                    user.Message(MessageHud.MessageType.Center, "Acquired the powers of a Rogue");
                    vl_player.vl_class = PlayerClass.Rogue;
                    flag = true;
                }
                if (flag)
                {
                    user.GetInventory().RemoveItem(item.m_shared.m_name, 1);
                    user.ShowRemovedMessage(item, 1);
                    UpdateVLPlayer(Player.m_localPlayer);
                    NameCooldowns();
                    if ((bool)___m_itemSpawnPoint && ___m_fuelAddedEffects != null)
                    {
                        ___m_fuelAddedEffects.Create(___m_itemSpawnPoint.position, __instance.transform.rotation);
                    }
                    UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_GP_Activation"), user.GetCenterPoint(), Quaternion.identity);

                    if (ValheimLegends.abilitiesStatus != null)
                    {
                        foreach (RectTransform ability in ValheimLegends.abilitiesStatus)
                        {
                            if (ability.gameObject != null)
                            {
                                UnityEngine.Object.Destroy(ability.gameObject);
                            }
                        }
                        ValheimLegends.abilitiesStatus.Clear();
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "Stagger", null)]
        public class VL_StaggerPrevention_Patch
        {
            public static bool Prefix(Character __instance)
            {
                if(__instance.GetSEMan().HaveStatusEffect("SE_VL_Berserk"))
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "Damage", null)]
        public class VL_Damage_Patch
        {
            public static bool Prefix(Character __instance, ref HitData hit, float ___m_maxAirAltitude)
            {
                Character attacker = hit.GetAttacker();
                if (__instance == Player.m_localPlayer)
                {
                    if (Class_Valkyrie.inFlight)// && Mathf.Max(0f, ___m_maxAirAltitude - __instance.transform.position.y) > 4f)
                    {
                        Class_Valkyrie.inFlight = false;                        
                        return false;
                    }
                    //if(__instance.GetSEMan().HaveStatusEffect("SE_VL_Bulwark"))
                    //{
                    //    //ZLog.Log("has status effect SE_VL_Bulwark");
                    //    hit.m_damage.Modify(.75f - (Player.m_localPlayer.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.AbjurationSkillDef).m_level/200f));
                    //}                    
                }

                if(__instance.GetSEMan() != null && __instance.GetSEMan().HaveStatusEffect("SE_VL_Charm") && attacker.IsPlayer())
                {
                    SE_Charm se_charm = (SE_Charm)__instance.GetSEMan().GetStatusEffect("SE_VL_Charm");
                    __instance.m_faction = se_charm.originalFaction;
                    __instance.GetSEMan().RemoveStatusEffect(se_charm, true);
                    //ZLog.Log("removing status due to hit");
                }

                if (attacker != null)
                {
                    if(attacker.GetSEMan().HaveStatusEffect("SE_VL_Weaken"))
                    {
                        SE_Weaken se_w = (SE_Weaken)attacker.GetSEMan().GetStatusEffect("SE_VL_Weaken");
                        hit.m_damage.Modify(1f - se_w.damageReduction);
                    }
                    //ZLog.Log("attacker is " + attacker.name);
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_ShadowStalk"))
                    {
                        //ZLog.Log("removing shadowstalk status");
                        attacker.GetSEMan().RemoveStatusEffect("SE_VL_ShadowStalk", true);
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_Rogue"))
                    {                        
                        if(Class_Rogue.PlayerUsingDaggerOnly)
                        {
                            hit.m_damage.Modify(1.25f);
                        }
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_Monk"))
                    {
                        SE_Monk se_m = (SE_Monk)attacker.GetSEMan().GetStatusEffect("SE_VL_Monk");
                        if (Class_Monk.PlayerUsingUnarmed)
                        {
                            hit.m_damage.Modify(1.25f);
                            se_m.hitCount++;
                        }
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_Shell"))
                    {
                        SE_Shell se_shell = attacker.GetSEMan().GetStatusEffect("SE_VL_Shell") as SE_Shell;
                        hit.m_damage.m_spirit += se_shell.spiritDamageOffset;
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_BiomeMist"))
                    {
                        SE_BiomeMist se_BiomeMist = attacker.GetSEMan().GetStatusEffect("SE_VL_BiomeMist") as SE_BiomeMist;
                        hit.m_damage.m_frost += se_BiomeMist.iceDamageOffset;
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_BiomeAsh"))
                    {
                        SE_BiomeAsh se_BiomeAsh = attacker.GetSEMan().GetStatusEffect("SE_VL_BiomeAsh") as SE_BiomeAsh;
                        hit.m_damage.m_fire += se_BiomeAsh.fireDamageOffset;
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_Berserk"))
                    {
                        SE_Berserk se_berserk = attacker.GetSEMan().GetStatusEffect("SE_VL_Berserk") as SE_Berserk;
                        attacker.Heal(hit.GetTotalPhysicalDamage() * se_berserk.healthAbsorbPercent, true);
                        attacker.AddStamina(hit.GetTotalElementalDamage() * se_berserk.healthAbsorbPercent);
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_Execute"))
                    {
                        SE_Execute se_berserk = attacker.GetSEMan().GetStatusEffect("SE_VL_Execute") as SE_Execute;
                        hit.m_staggerMultiplier *= se_berserk.staggerForce;
                        hit.m_damage.m_blunt *= se_berserk.damageBonus;
                        hit.m_damage.m_pierce *= se_berserk.damageBonus;
                        hit.m_damage.m_slash *= se_berserk.damageBonus;
                        se_berserk.hitCount--;
                        if(se_berserk.hitCount <= 0)
                        {
                            attacker.GetSEMan().RemoveStatusEffect(se_berserk, true);
                        }
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_Companion"))
                    {
                        SE_Companion se_companion = attacker.GetSEMan().GetStatusEffect("SE_VL_Companion") as SE_Companion;
                        hit.m_damage.Modify(se_companion.damageModifier);
                    }
                    if (attacker.GetSEMan().HaveStatusEffect("SE_VL_RootsBuff"))
                    {
                        SE_RootsBuff se_RootsBuff = attacker.GetSEMan().GetStatusEffect("SE_VL_RootsBuff") as SE_RootsBuff;
                        hit.m_damage.Modify(se_RootsBuff.damageModifier);
                    }
                }

                //hit.m_damage.Modify(.5f);
                //__instance.GetSEMan().HaveStatusEffect()

                return true;
            }
        }

        [HarmonyPatch(typeof(Attack), "DoMeleeAttack", null)]
        public class MeleeAttack_Patch
        {
            public static bool Prefix(Attack __instance, Humanoid ___m_character, ref float ___m_damageMultiplier)
            {
                if (___m_character.GetSEMan().HaveStatusEffect("SE_VL_Berserk"))
                {
                    ___m_damageMultiplier *= 1.2f;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Attack), "FireProjectileBurst", null)]
        public class ProjectileAttack_Prefix
        {
            public static bool Prefix(Attack __instance, Humanoid ___m_character, ref float ___m_projectileVel, ref float ___m_forceMultiplier, ref float ___m_staggerMultiplier, ref float ___m_damageMultiplier, ref ItemDrop.ItemData ___m_weapon)
            {
                if (___m_character.GetSEMan().HaveStatusEffect("SE_VL_PowerShot"))
                {
                    ___m_projectileVel *= 2f;
                    ___m_damageMultiplier = 1.4f + (___m_character.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == DisciplineSkillDef).m_level * .015f);
                    SE_PowerShot se_shot = ___m_character.GetSEMan().GetStatusEffect("SE_VL_PowerShot") as SE_PowerShot;

                    se_shot.hitCount--;
                    if (se_shot.hitCount <= 0)
                    {
                        ___m_character.GetSEMan().RemoveStatusEffect(se_shot, true);
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Projectile), "OnHit", null)]
        public class Projectile_Hit_Patch
        {
            public static void Postfix(Projectile __instance, Collider collider, Vector3 hitPoint, bool water, float ___m_aoe, int ___m_rayMaskSolids, Character ___m_owner, Vector3 ___m_vel)
            {
                if (__instance.name == "VL_Charm")
                {
                    bool hitCharacter = false;
                    if (__instance.m_aoe > 0f)
                    {
                        Collider[] array = Physics.OverlapSphere(hitPoint, __instance.m_aoe, ___m_rayMaskSolids, QueryTriggerInteraction.UseGlobal);
                        HashSet<GameObject> hashSet = new HashSet<GameObject>();
                        Collider[] array2 = array;
                        foreach (Collider collider2 in array2)
                        {
                            GameObject gameObject = Projectile.FindHitObject(collider2);
                            IDestructible component = gameObject.GetComponent<IDestructible>();
                            if (component != null && !hashSet.Contains(gameObject))
                            {
                                hashSet.Add(gameObject);
                                if (IsValidTarget(component, ref hitCharacter, ___m_owner, __instance.m_dodgeable))
                                {
                                    Character ch = null;
                                    gameObject.TryGetComponent<Character>(out ch);
                                    bool flag = ch != null;
                                    if (ch == null)
                                    {
                                        ch = (Character)gameObject.GetComponentInParent(typeof(Character));
                                        flag = ch != null;                                        
                                    }
                                    if (flag && !ch.IsPlayer() && ___m_owner is Player &&
                                        !ch.m_boss)
                                    {
                                        //ZLog.Log("charming " + ch.m_name);
                                        Player p = ___m_owner as Player;
                                        SE_Charm se_charm = (SE_Charm)ScriptableObject.CreateInstance(typeof(SE_Charm));
                                        se_charm.m_ttl = SE_Charm.m_baseTTL;
                                        se_charm.summoner = p;
                                        se_charm.originalFaction = ch.m_faction;
                                        ch.m_faction = p.GetFaction();                                        
                                        UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("fx_boar_pet"), ch.GetEyePoint(), Quaternion.identity);
                                        ch.GetSEMan().AddStatusEffect(se_charm);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsValidTarget(IDestructible destr, ref bool hitCharacter, Character owner, bool m_dodgeable)
        {
            Character character = destr as Character;
            if ((bool)character)
            {
                if (character == owner)
                {
                    return false;
                }
                if (owner != null && !owner.IsPlayer() && !BaseAI.IsEnemy(owner, character))
                {
                    return false;
                }
                if (m_dodgeable && character.IsDodgeInvincible())
                {
                    return false;
                }
                hitCharacter = true;
            }
            return true;
        }

        [HarmonyPatch(typeof(Player), "InShelter", null)]
        public class PlayerShelter_BiomePatch
        {
            public static void Postfix(Player __instance, ref bool __result)
            {
                if(__instance.IsPlayer() && __instance.GetSEMan().HaveStatusEffect("SE_VL_BiomeBlackForest"))
                {
                    __result = true;                    
                }

            }
        }

        //[HarmonyPatch(typeof(Character), "UpdateMotion", null)]
        //public class ClassMotionUpdate_Postfix
        //{
        //    public static bool Prefix(Character __instance, ref bool ___m_flying, float ___m_waterLevel)
        //    {
        //        if (vl_player == null || vl_player.vl_class == PlayerClass.Shaman)
        //        {
        //            if (ZInput.GetButton("Jump"))
        //            {
        //                if (__instance.transform.position.y <= (___m_waterLevel + .25f))
        //                {
        //                    ___m_flying = true;
        //                }
        //                else
        //                {
        //                    ___m_flying = false;
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(Player), "GetMaxCarryWeight", null)]
        public class PlayerCarryWeight_BiomePatch
        {
            public static void Postfix(Player __instance, ref float __result)
            {
                if (__instance.IsPlayer() && __instance.GetSEMan().HaveStatusEffect("SE_VL_BiomeBlackForest"))
                {
                    SE_BiomeBlackForest se = (SE_BiomeBlackForest)__instance.GetSEMan().GetStatusEffect("SE_VL_BiomeBlackForest");
                    __result += se.carryModifier;
                }
            }
        }

        //[HarmonyPatch(typeof(Character), "Jump", null)]
        //public class JumpAdditions_Patch
        //{
        //    public static void Postfix(Character __instance, ZSyncAnimation ___m_zanim, Rigidbody ___m_body, ref float ___m_maxAirAltitude)
        //    {

        //    }
        //}

        //[HarmonyPatch(typeof(Attack), "FireProjectileBurst", null)]
        //public class ProjectileAttack_Postfix
        //{
        //    public static void Postfix(Attack __instance, Humanoid ___m_character, ref float ___m_forceMultiplier, ref float ___m_staggerMultiplier, ref float ___m_damageMultiplier, ref ItemDrop.ItemData ___m_weapon)
        //    {
        //        MethodBase FireProjectileBurst = AccessTools.Method(typeof(Attack), "FireProjectileBurst", null, null);

        //        if (___m_character.GetSEMan().HaveStatusEffect("SE_VL_PowerShot"))
        //        {
        //            //ZLog.Log("last projectile name " + ___m_weapon.m_lastProjectile.name);
        //            SE_PowerShot se_shot = ___m_character.GetSEMan().GetStatusEffect("SE_VL_PowerShot") as SE_PowerShot;

        //            if (se_shot.shouldActivate)
        //            {
        //                se_shot.shouldActivate = false;
        //                se_shot.hitCount--;
        //                if (se_shot.hitCount <= 0)
        //                {
        //                    ___m_character.GetSEMan().RemoveStatusEffect(se_shot, true);
        //                }
        //                //if (___m_weapon.m_lastProjectile != null && ___m_weapon.m_lastProjectile.name.Contains("bow"))
        //                //{
        //                //    //ZLog.Log("fire second shot");

        //                //    ___m_damageMultiplier = .5f + (___m_character.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == DisciplineSkillDef).m_level * .025f);
        //                //    FireProjectileBurst.Invoke(__instance, new object[] { });
        //                //}
        //                //else
        //                //{

        //                ___m_damageMultiplier = 1f + (___m_character.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == DisciplineSkillDef).m_level * .015f);
        //                //}
        //            }
        //            else
        //            {
        //                se_shot.shouldActivate = true;
        //            }
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(Humanoid), "BlockAttack", null)]
        public class Block_Class_Patch
        {
            public static bool Prefix(Humanoid __instance, HitData hit, Character attacker, float ___m_blockTimer, ItemDrop.ItemData ___m_leftItem, ref bool __result)
            {
                if (__instance == Player.m_localPlayer)
                {
                    if (__instance.GetSEMan().HaveStatusEffect("SE_VL_Bulwark"))
                    {
                        Class_Valkyrie.isBlocking = true;
                    }
                    else if (vl_player.vl_class == PlayerClass.Duelist && ___m_leftItem == null)
                    {
                        if (Vector3.Dot(hit.m_dir, __instance.transform.forward) > 0f)
                        {
                            __result = false;
                            return false;
                        }
                        ItemDrop.ItemData currentBlocker = __instance.GetCurrentWeapon();
                        if (currentBlocker == null)
                        {
                            __result = false;
                            return false;
                        }
                        HitData hitData2 = new HitData();
                        hitData2.m_damage = hit.m_damage;
                        //ZLog.Log("entering riposte against: " + hit.m_damage + " " + hit.m_damage.m_blunt + "b " + hit.m_damage.m_fire + "fi " + hit.m_damage.m_frost + "fr " + hit.m_damage.m_lightning + "l " + hit.m_damage.m_pierce + "p " + hit.m_damage.m_poison + "po " + hit.m_damage.m_slash + "s " + hit.m_damage.m_spirit);
                        float sLevel = __instance.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                        bool flag = currentBlocker.m_shared.m_timedBlockBonus > 1f && ___m_blockTimer != -1f && ___m_blockTimer < 0.25f;
                        float skillFactor = __instance.GetSkillFactor(Skills.SkillType.Blocking);
                        float num = currentBlocker.GetBlockPower(skillFactor);                        
                        if (flag)
                        {
                            num *= currentBlocker.m_shared.m_timedBlockBonus;
                            if(__instance.GetSEMan().HaveStatusEffect("SE_VL_Riposte"))
                            {
                                num += 10f * sLevel;
                            }
                            else
                            {
                                num += 2f * sLevel;
                            }
                        }
                        float totalBlockableDamage = hit.GetTotalBlockableDamage();
                        float num2 = Mathf.Min(totalBlockableDamage, num);
                        float num3 = Mathf.Clamp01(num2 / num);
                        float stamina = __instance.m_blockStaminaDrain * num3 * .5f;
                        __instance.UseStamina(stamina);
                        bool num4 = __instance.HaveStamina();
                        bool flag2 = num4 && num >= totalBlockableDamage;
                        if (num4)
                        {
                            hit.m_statusEffect = "";
                            hit.BlockDamage(num2);
                            DamageText.instance.ShowText(DamageText.TextType.Blocked, hit.m_point + Vector3.up * 0.5f, num2);
                        }
                        if (!num4 || !flag2)
                        {
                            __instance.Stagger(hit.m_dir);
                        }
                        __instance.RaiseSkill(Skills.SkillType.Blocking, flag ? 2f : 1f);
                        currentBlocker.m_shared.m_blockEffect.Create(hit.m_point, Quaternion.identity);
                        if (((bool)attacker && flag) & flag2)
                        {
                            __instance.m_perfectBlockEffect.Create(hit.m_point, Quaternion.identity);
                            if (attacker.m_staggerWhenBlocked)
                            {
                                attacker.Stagger(-hit.m_dir);
                            }
                        }
                        if (flag2)
                        {
                            float num6 = Mathf.Clamp01(num3 * 0.5f);
                            hit.m_pushForce *= num6;
                            if ((bool)attacker && flag)
                            {
                                HitData hitData = new HitData();
                                hitData.m_pushForce = currentBlocker.GetDeflectionForce() * (1f - num6);
                                hitData.m_dir = attacker.transform.position - __instance.transform.position;
                                hitData.m_dir.y = 0f;
                                hitData.m_dir.Normalize();
                                hitData.m_point = attacker.GetEyePoint();
                                if (__instance.GetSEMan().HaveStatusEffect("SE_VL_Riposte") && (attacker.transform.position - __instance.transform.position).magnitude < 3f)
                                {
                                    __instance.GetSEMan().RemoveStatusEffect("SE_VL_Riposte");
                                    hitData.m_damage = hitData2.m_damage;
                                    //ZLog.Log("riposting damage: " + hitData2.m_damage.m_blunt + "b " + hitData2.m_damage.m_fire + "fi " + hitData2.m_damage.m_frost + "fr " + hitData2.m_damage.m_lightning + "l " + hitData2.m_damage.m_pierce + "p " + hitData2.m_damage.m_poison + "po " + hitData2.m_damage.m_slash + "s " + hitData2.m_damage.m_spirit);
                                    //((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("knife_stab2");
                                    ((ZSyncAnimation)typeof(Player).GetField("m_zanim", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Player.m_localPlayer)).SetTrigger("atgeir_attack2");
                                    float dmgMod = UnityEngine.Random.Range(.3f, .5f) + sLevel / 150f;
                                    //ZLog.Log("damage mod was " + dmgMod);
                                    hitData.ApplyModifier(dmgMod);
                                    __instance.RaiseSkill(ValheimLegends.DisciplineSkill, VL_Utility.GetRiposteSkillGain * 2f);
                                    //ZLog.Log("riposting damage: " + hitData.m_damage.m_blunt + "b " + hitData.m_damage.m_fire + "fi " + hitData.m_damage.m_frost + "fr " + hitData.m_damage.m_lightning + "l " + hitData.m_damage.m_pierce + "p " + hitData.m_damage.m_poison + "po " + hitData.m_damage.m_slash + "s " + hitData.m_damage.m_spirit);
                                    //battleaxe_attack1
                                    //atgeir_attack1
                                    //"swing_longsword2"
                                }
                                attacker.Damage(hitData);
                            }
                        }
                        
                        __result = true;
                        return false;
                    }
                    else
                    {
                        Class_Valkyrie.isBlocking = false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ItemDrop.ItemData), "GetBaseBlockPower", new Type[]
        {
            typeof(int)
        })]
        public class BaseBlockPower_Bulwark_Patch
        {
            public static void Postfix(ItemDrop.ItemData __instance, ref float __result)
            {                
                if (Class_Valkyrie.isBlocking)
                {
                    __result += 20f;
                }
                if(vl_player.vl_class == PlayerClass.Monk && __instance.m_shared != null && __instance.m_shared.m_name == "Unarmed")
                {
                    __result += Player.m_localPlayer.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ValheimLegends.DisciplineSkillDef).m_level;
                }
            }
        }

        [HarmonyPatch(typeof(HitData), "BlockDamage")]
        public static class BlockDamage_Patch
        {
            public static bool Prefix(HitData __instance, float damage)
            {
                if (vl_player != null && vl_player.vl_class == PlayerClass.Monk && Class_Monk.PlayerUsingUnarmed)
                {
                    if (__instance.GetTotalBlockableDamage() >= damage)
                    {
                        SE_Monk se_monk = (SE_Monk)Player.m_localPlayer.GetSEMan().GetStatusEffect("SE_VL_Monk");
                        se_monk.hitCount++;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Skills), "GetSkillDef")]
        public static class GetSkillDef_Patch
        {
            public static void Postfix(Skills __instance, Skills.SkillType type, List<Skills.SkillDef> ___m_skills, ref Skills.SkillDef __result)
            {
                MethodInfo methodInfo = AccessTools.Method(typeof(Localization), "AddWord", (Type[])null, (Type[])null);

                if (__result == null)
                {
                    if(legendsSkills != null)
                    {
                        foreach(Skills.SkillDef sd in legendsSkills)
                        {
                            if(!___m_skills.Contains(sd))
                            {
                                ___m_skills.Add(sd);
                                methodInfo.Invoke(Localization.instance, new object[2]
                                {
                                    "skill_"+ sd.m_skill,
                                    ((SkillName)sd.m_skill).ToString()
                                });
                            }
                        }
                        __result = ___m_skills.FirstOrDefault((Skills.SkillDef x) => x.m_skill == type);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnDestroy")]
        public static class RemoveHud_Patch
        {
            public static bool Prefix()
            {
                if (abilitiesStatus != null)
                {
                    foreach (RectTransform ability in ValheimLegends.abilitiesStatus)
                    {
                        if (ability.gameObject != null)
                        {
                            UnityEngine.Object.Destroy(ability.gameObject);
                        }
                    }
                    abilitiesStatus.Clear();
                    abilitiesStatus = null;
                }
                if(vl_player != null)
                {
                    vl_player = null;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(Hud), "UpdateStatusEffects")]
        public static class SkillIcon_Patch
        {
            public static void Postfix(Hud __instance)
            {
                if(__instance != null && ClassIsValid && showAbilityIcons.Value)
                {
                    if(abilitiesStatus == null)
                    {
                        abilitiesStatus = new List<RectTransform>();
                        abilitiesStatus.Clear();                        
                    }
                    if(abilitiesStatus.Count != 3)
                    {
                        foreach (RectTransform ability in ValheimLegends.abilitiesStatus)
                        {
                            UnityEngine.Object.Destroy(ability.gameObject);
                        }
                        ValheimLegends.abilitiesStatus.Clear();
                        VL_Utility.InitiateAbilityStatus(__instance); 
                    }
                    if (abilitiesStatus != null)
                    {
                        for (int j = 0; j < abilitiesStatus.Count; j++)
                        {
                            RectTransform rectTransform2 = abilitiesStatus[j];
                            Image component = rectTransform2.Find("Icon").GetComponent<Image>();
                            string iconText = "";
                            if (j == 0)
                            {
                                component.sprite = Ability1_Sprite;
                                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect("SE_VL_Ability1_CD"))
                                {
                                    component.color = abilityCooldownColor;
                                    iconText = StatusEffect.GetTimeString(Player.m_localPlayer.GetSEMan().GetStatusEffect("SE_VL_Ability1_CD").GetRemaningTime());
                                }
                                else
                                {
                                    component.color = Color.white;
                                    iconText = Ability1_Hotkey.Value;
                                }
                            }
                            else if (j == 1)
                            {

                                component.sprite = Ability2_Sprite;
                                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect("SE_VL_Ability2_CD"))
                                {
                                    component.color = abilityCooldownColor;
                                    iconText = StatusEffect.GetTimeString(Player.m_localPlayer.GetSEMan().GetStatusEffect("SE_VL_Ability2_CD").GetRemaningTime());
                                }
                                else
                                {
                                    component.color = Color.white;
                                    iconText = Ability2_Hotkey.Value;
                                }
                            }
                            else
                            {

                                component.sprite = Ability3_Sprite;
                                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect("SE_VL_Ability3_CD"))
                                {
                                    component.color = abilityCooldownColor;
                                    iconText = StatusEffect.GetTimeString(Player.m_localPlayer.GetSEMan().GetStatusEffect("SE_VL_Ability3_CD").GetRemaningTime());
                                }
                                else
                                {
                                    component.color = Color.white;
                                    iconText = Ability3_Hotkey.Value;
                                }
                            }


                            //rectTransform2.GetComponentInChildren<Text>().text = Localization.instance.Localize((Ability1.Name).ToString());
                            Text component2 = rectTransform2.Find("TimeText").GetComponent<Text>();
                            if (!string.IsNullOrEmpty(iconText))
                            {
                                component2.gameObject.SetActive(value: true);
                                component2.text = iconText;
                            }
                            else
                            {
                                component2.gameObject.SetActive(value: false);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Skills), "IsSkillValid")]
        public static class ValidSkill_Patch
        {
            public static bool Prefix(Skills __instance, Skills.SkillType type, ref bool __result)
            {
                if (type == AlterationSkill || type == AbjurationSkill || type == ConjurationSkill || type == EvocationSkill || type == DisciplineSkill || type == IllusionSkill)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        public static class AbilityInput_Prefix
        {
            public static bool Prefix(Player __instance)
            {
                if (ZInput.GetButtonDown("GPower") || ZInput.GetButtonDown("JoyGPower"))
                {
                    ValheimLegends.shouldUseGuardianPower = true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "OnSpawned", null)]
        public class SetLegendClass_Postfix
        {
            public static void Postfix(Player __instance)
            {
                if (vl_player == null || vl_player.vl_name != __instance.GetPlayerName())
                {                    
                    SetVLPlayer(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Player), "RaiseSkill", null)]
        public static class VinesHit_SkillRaise_Prefix
        {
            public static bool Prefix(Player __instance, Skills.SkillType skill, ref float value)
            {
                if (__instance.IsPlayer() && skill == ValheimLegends.ConjurationSkill)
                {
                    if (value == 0.5f)
                    {
                        value = .1f;
                    }
                    else if(value == 1f)
                    {
                        value = .5f;
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(Player), "Update", null)]
        public class AbilityInput_Postfix
        {
            public static void Postfix(Player __instance, ref float ___m_maxAirAltitude, ref Rigidbody ___m_body, ref Animator ___m_animator, ref float ___m_lastGroundTouch, float ___m_waterLevel)
            {
                Player localPlayer = Player.m_localPlayer;                
                if (localPlayer != null && playerEnabled)
                {
                    //for log checking only
                    //logCheck++;
                    //if (logCheck > 120)
                    //{
                    //    logCheck = 0;
                    //    ZLog.Log("player position " + localPlayer.transform.position);
                    //}
                    //end logging

                    if (VL_Utility.TakeInput(localPlayer) && !localPlayer.InPlaceMode())
                    {
                        if (vl_player.vl_class == PlayerClass.Mage)
                        {
                            Class_Mage.Process_Input(localPlayer, ___m_maxAirAltitude);
                        }
                        else if (vl_player.vl_class == PlayerClass.Druid)
                        {
                            Class_Druid.Process_Input(localPlayer, ___m_maxAirAltitude);
                        }
                        else if (vl_player.vl_class == PlayerClass.Shaman)
                        {
                            Class_Shaman.Process_Input(localPlayer, ref ___m_body, ref ___m_maxAirAltitude, ref ___m_lastGroundTouch, ___m_waterLevel);
                        }
                        else if (vl_player.vl_class == PlayerClass.Ranger)
                        {
                            Class_Ranger.Process_Input(localPlayer);
                        }
                        else if (vl_player.vl_class == PlayerClass.Berserker)
                        {
                            Class_Berserker.Process_Input(localPlayer, ref ___m_maxAirAltitude);
                        }
                        else if (vl_player.vl_class == PlayerClass.Valkyrie)
                        {
                            Class_Valkyrie.Process_Input(localPlayer);
                        }
                        else if (vl_player.vl_class == PlayerClass.Metavoker)
                        {
                            Class_Metavoker.Process_Input(localPlayer, ref ___m_maxAirAltitude, ref ___m_body);
                        }
                        else if (vl_player.vl_class == PlayerClass.Priest)
                        {
                            Class_Priest.Process_Input(localPlayer, ref ___m_maxAirAltitude);
                        }
                        //else if (vl_player.vl_class == PlayerClass.Necromancer)
                        //{
                        //    Class_Necromancer.Process_Input(localPlayer);
                        //}
                        else if (vl_player.vl_class == PlayerClass.Monk)
                        {
                            Class_Monk.Process_Input(localPlayer, ref ___m_body, ref ___m_maxAirAltitude, ref ___m_animator);

                            if ((!localPlayer.GetSEMan().HaveStatusEffect("SE_VL_Monk")))
                            {
                                SE_Monk se_m = (SE_Monk)ScriptableObject.CreateInstance(typeof(SE_Monk));
                                se_m.m_ttl = SE_Monk.m_baseTTL;
                                localPlayer.GetSEMan().AddStatusEffect(se_m, true);
                            }
                        }
                        else if (vl_player.vl_class == PlayerClass.Duelist)
                        {
                            Class_Duelist.Process_Input(localPlayer);
                        }
                        else if (vl_player.vl_class == PlayerClass.Enchanter)
                        {
                            Class_Enchanter.Process_Input(localPlayer, ref ___m_maxAirAltitude);
                        }
                        else if (vl_player.vl_class == PlayerClass.Rogue)
                        {
                            Class_Rogue.Process_Input(localPlayer, ref ___m_body, ref ___m_maxAirAltitude);
                            
                            if ((!localPlayer.GetSEMan().HaveStatusEffect("SE_VL_Rogue")))
                            {
                                SE_Rogue se_r = (SE_Rogue)ScriptableObject.CreateInstance(typeof(SE_Rogue));
                                se_r.m_ttl = SE_Rogue.m_baseTTL;
                                localPlayer.GetSEMan().AddStatusEffect(se_r,true);
                            }
                        }
                    }
                }

                if (isChargingDash)
                {
                    dashCounter++;
                    if(dashCounter >= 10 && vl_player.vl_class == PlayerClass.Berserker)
                    {
                        isChargingDash = false;
                        Class_Berserker.Execute_Dash(localPlayer, ref ___m_maxAirAltitude);
                    }
                    if (dashCounter >= 10 && vl_player.vl_class == PlayerClass.Duelist)
                    {
                        isChargingDash = false;
                        ValheimLegends.isChanneling = false;
                        Class_Duelist.Execute_Slash(localPlayer);
                    }
                    if (dashCounter >= 16 && vl_player.vl_class == PlayerClass.Rogue)
                    {
                        isChargingDash = false;
                        Class_Rogue.Execute_Throw(localPlayer);
                    }
                    if(vl_player.vl_class == PlayerClass.Monk && dashCounter >= (int)Class_Monk.QueuedAttack)
                    {
                        isChargingDash = false;
                        Class_Monk.Execute_Attack(localPlayer, ref ___m_body, ref ___m_maxAirAltitude);
                    }
                    if(vl_player.vl_class == PlayerClass.Enchanter && dashCounter >= (int)Class_Enchanter.QueuedAttack)
                    {
                        isChargingDash = false;
                        Class_Enchanter.Execute_Attack(localPlayer, ref ___m_body, ref ___m_maxAirAltitude);
                    }
                }

                if(animationCountdown > 0)
                {
                    animationCountdown--;
                }
            }
        }

        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static class PlayerMuninNotification_Patch
        {
            public static void Postfix(Player __instance)
            {
                Tutorial.TutorialText vl = new Tutorial.TutorialText
                {
                    m_label = "Valheim Legends",
                    m_name = "Valheim_Legends",
                    m_text = "The Allfather has granted you a spark of power as a reward for your past deeds - you need only sacrifice a minor token at a Eikthyr's altar to awaken your power.\n\nGo to the altar and activate the nearby glowing tablet - I will explain more there!\nFjolner is watching over you.",
                    m_topic = "Become a Legend!"
                };
                if (!Tutorial.instance.m_texts.Contains(vl))
                {
                    Tutorial.instance.m_texts.Add(vl);
                }
                __instance.ShowTutorial("Valheim_Legends");

                Tutorial.TutorialText vl2 = new Tutorial.TutorialText
                {
                    m_label = "Legends Offerings",
                    m_name = "VL_Offerings",
                    m_text = "You can inherit legendary powers by placing token on the altar:\nBerserker - Bone fragments\nDruid - Dandelion\nDuelist - Thistle\nEnchanter - Resin\nMage - Coal\nMetavoker - Raspberry\n" +
                    "Monk - Wood\nPriest - Stone\nRanger - Raw Meat\nRogue - Honey\nShaman - Greydwarf Eye\nValkyrie - Flint",
                    m_topic = "Token Offering"
                };
                if (!Tutorial.instance.m_texts.Contains(vl2))
                {
                    Tutorial.instance.m_texts.Add(vl2);
                }

                //Class Entries
                Tutorial.TutorialText vl_mage = new Tutorial.TutorialText
                {
                    m_label = "Legend: Mage",
                    m_name = "VL_Mage",
                    m_text = "The story of a mage centers around one key element - raw power. A mage focuses on harnessing raw, destructive energy.\n\n" +
                    "Skills: Evocation\n" +
                    "Sacrifice: coal\n\n" +
                    "Fireball: creates a ball of fire above the caster that arcs towards the casters target.\nDamage:\n Fire - 10->40 + 2*Evocation\n Blunt - 1/2 Fire\n AoE - 3m + 1%*Evocation\nCooldown: 12s\nEnergy: 50 + 0.5*Evocation\n*Afflicts targets with burning\n\n"
                    + "Frost Nova: point blank area of effect frost damage that slows victims for a short period.\nDamage:\n Ice - 10 + 0.5*Evocation -> 20 + Evocation\n AoE - 10m + 1%*Evocation\nCooldown: 30s\nEnergy: 40\n*Slows movement of affected targets by 60% for 4s\n**Removes burning effect from caster\n\n"
                    + "Meteor: channels energy to call down a meteor storm on the targeted area.\nDamage (per meteor):\n Fire - 30 + 0.5*Evocation -> 50 + Evocation\n Blunt - 1/2 Fire\n AoE - 8m + 0.5%*Evocation\nCooldown: 180s\nEnergy: 60 initial + 30 per second channeled\n*Afflicts targets with burning\n**Press and hold the ability button to channel the spell to create multiple meteors\n***Jump or dodge to cancel ability\n\n",
                    m_topic = "Legend Mage"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_mage))
                {
                    Tutorial.instance.m_texts.Add(vl_mage);
                }

                Tutorial.TutorialText vl_berserker = new Tutorial.TutorialText
                {
                    m_label = "Legend: Berserker",
                    m_name = "VL_Berserker",
                    m_text = "Berserkers harness their rage into physical carnage and will sacrifice their own health to fuel their rage.\n\n" +
                    "Skills: Discipline and Alteration\n" +
                    "Sacrifice: bone fragment\n\n" +
                    "Execute: empower the next several physical attacks to deal extra damage.\nDamage:\n Physical bonus (blunt/slash/pierce) - + 40% + 0.5% * Discipline\n Stagger - 50% + 0.5% * Discipline\nCharges: 3 + 0.04*Discipline\nCooldown: 120s\nEnergy: 50\n\n"
                    + "Berserk: sacrifices health to increase movement speed, attack power, remove stamina regeneration delay and gain renewed energy through combat.\nDamage:\n Bonus +20% + 0.5%*Alteration\nMovement Speed - +20% + 0.5%*Alteration\nCooldown: 120s\nEnergy: 50\n*Absorbs 20%+0.2%*Alteration of inflicted physical damage as health\n**Absorbs 20%+0.2%*Alteration of inflicted elemental damage as stamina\n\n"
                    + "Dash: dash forward in the blink of an eye, cutting through enemies in your way.\nDamage:\n 80% + 0.5%*Discipline of equipped weapon damage\nCooldown: 10s\nEnergy: 70\n*10m dash distance\n\n",
                    m_topic = "Legend Berserker"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_berserker))
                {
                    Tutorial.instance.m_texts.Add(vl_berserker);
                }
                Tutorial.TutorialText vl_druid = new Tutorial.TutorialText
                {
                    m_label = "Legend: Druid",
                    m_name = "VL_Druid",
                    m_text = "Druid's are the embodiment of nature's resilience, cunning, and fury and act as a conduit of its will.\n\n" +
                    "Skills: Conjuration and Alteration\n" +
                    "Sacrifice: dandelion\n\n" +
                    "Regeneration: applies a heal over time to the caster and all nearby allies.\nHealing:\n Self - 0.5 + 0.4*Alteration\n Other - 2 + 0.25*Average Skill Level\nDuration: Heals every 2s for 20s\nCooldown: 120s\nEnergy: 60\n\n"
                    + "Nature's Defense: calls upon nature to defend an area.\nSummon:\n Duration - 24s + 0.3s*Conjuration\n 3x Root defenders\n 2x + 0.05*Conjuration Drusquitos\nCooldown: 120s\nEnergy: 80\n*Defender's health and attack power increase with Conjuration\n**Each Root defender restores stamina to the caster as long as the caster remains near the point Nature's Defense was activated\n\n"
                    + "Vines: create vines that grow at an alarming speed.\nDamage:\n Piercing - 10 + 0.6*Conjuration -> 15 + 1.2*Conjuration per vine\nCooldown: 20s\nEnergy: 30 initial + 9 every .5s\n*Vines are a channeled ability, press and hold the ability button to continuously project vines\n\n",
                    m_topic = "Legend Druid"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_druid))
                {
                    Tutorial.instance.m_texts.Add(vl_druid);
                }
                Tutorial.TutorialText vl_metavoker = new Tutorial.TutorialText
                {
                    m_label = "Legend: Metavoker",
                    m_name = "VL_Metavoker",
                    m_text = "Metavoker's manipulate energy in a manner that affects light, space, and potential\n\n" +
                    "Skills: Illusion and Evocation\n" +
                    "Sacrifice: raspberries\n" +
                    "Light: creates a light that follows the caster and illuminates a large area\nDamage:\n Lightning - 2 + 0.25*Illusion -> 5 + 0.5*Illusion\n Force - 100 + Illusion\nDuration: 5m (or until directed)\nCooldown: 20s\nEnergy: 50\n*Use the ability once to summon the mage light for illumination\n**Use the ability with a mage light active to direct the light as a projectile\n\n"
                    + "Replica: bends light and energy to create reinforced illusions of every nearby enemy.\nSummon:\n Duration - 8s + 0.2s*Illusion\nCooldown: 30s\nEnergy: 70\n*Replica's health and attack power increase with Illusion\n\n"
                    + "Warp: collects the energy of the caster and projects it to a target location; any excess energy is released at the exit point.\nDamage:\n Lightning - excess distance * (0.033*Evocation -> 0.05*Evocation)\nCooldown: 20s\nEnergy: 40 initial + 60 every 1s\n*Tap ability button to instantly warp towards the target\n**Press and hold the ability button to collect energy to warp longer distances or warp with excess energy\n\n"
                    + "Bonus skills:\n - Safe Fall - press and hold jump to slow your descent; this ability requires stamina to maintain\n\n",
                    m_topic = "Legend Metavoker"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_metavoker))
                {
                    Tutorial.instance.m_texts.Add(vl_metavoker);
                }
                Tutorial.TutorialText vl_duelist = new Tutorial.TutorialText
                {
                    m_label = "Legend: Duelist",
                    m_name = "VL_Duelist",
                    m_text = "Duelist's specialize in offensive combat techniques that exploit openings in an opponent's defense.\n\n" +
                    "Skills: Discipline\n" +
                    "Sacrifice: thistle\n\n" +
                    "Hip Shot: fires a high velocity projectile from a concealed, mechanical contraption.\nDamage:\n Pierce - 5->30 + Discipline\nCooldown: 10s\nEnergy: 25\n\n"
                    + "Riposte: turns the energy of an attack into a devastating counter-attack\nDamage:\n returns 50%+1%*Discipline of the damage upon the attacker and quickly launches an attack maneuver that deals damage based on the equipped weapon\nCooldown: 6s\nEnergy: 30\n*Riposte must be timed well to be effective. Block amount and parry force is increased 10x while riposte is active and the player executes a perfect block.\n**Riposte can only be used with a weapon equipped and without a shield.\n\n"
                    + "Seismic Slash: a combat technique that compresses energy and releases it in a tight arc as a razor thin burst.\nDamage:\n 60%+0.06%*Discipline of weapon damage\nForce: 25+0.1*Discipline\nCooldown: 30s\nEnergy: 60\n*Deals damage to all targets in a 25 degree cone in front of the caster\n\n" +
                    "Bonus skills:\n - Weapon Master - gain a bonus to block and parry based on Discipline while wielding only a weapon\n\n",
                    m_topic = "Legend Duelist"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_duelist))
                {
                    Tutorial.instance.m_texts.Add(vl_duelist);
                }
                Tutorial.TutorialText vl_rogue = new Tutorial.TutorialText
                {
                    m_label = "Legend: Rogue",
                    m_name = "VL_Rogue",
                    m_text = "Rogue's are infamous for their dirty fighting and ruthless cunning.\n\n" +
                    "Skills: Discipline and Alteration\n" +
                    "Sacrifice: honey\n\n" +
                    "Poison Bomb: throw a vial of highly caustic poison that affects an area for a short time.\nDamage:\n Poison DoT - 10 + alteration\nCooldown: 30s\nEnergy: 50\n*Duration and hit frequency increase with Alteration\n\n"
                    + "Fade: returns the rogue to a previous point and adds a supply to the bag of tricks\nCooldown: 15s\nEnergy: 10\n*Set the fade point by using the ability. While fade is on cooldown, use the ability again to instantly return to the fade.\n\n"
                    + "Backstab: instantly move behind the target and strike a critical blow\nDamage:\n 70% + 0.5%*Discipline of weapon damage\nForce: 10 + 0.5*Discipline\nCooldown: 20s\nEnergy: 60\n\n" +
                    "Bonus skills:\n - Bag of Tricks - prepare class charges every 20s to use bonus skills\n" +
                     " - Stealthy - gain a passive bonus to move speed while crouched\n" +
                     " - Dagger Mastery - gain a passive 25% bonus damage while using daggers (offhand shield or torches are not allowed)\n" +
                     " - Throwing Knives - quickly throw a small dagger using a rogue charge; this skill is activated by pressing attack while holding block\n" +
                     " - Double Jump - leap to extraordinary heights by stepping on seemingly invisible footholds; activate this skill by pressing jump while in the air\n" +
                     "*Double Jump may only be activated once, after leaving the ground\n\n",
                    m_topic = "Legend Rogue"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_rogue))
                {
                    Tutorial.instance.m_texts.Add(vl_rogue);
                }
                Tutorial.TutorialText vl_priest = new Tutorial.TutorialText
                {
                    m_label = "Legend: Priest",
                    m_name = "VL_Priest",
                    m_text = "Priest's command a balanced set of offensive and healing abilities that makes them a formidable ally, or foe.\n\n" +
                    "Skills: Alteration and Evocation\n" +
                    "Sacrifice: stone\n\n" +
                    "Sanctify: calls down the fiery hammer of Ragnarök to purify a target area.\nDamage:\n Blunt - (10 + 0.5*Evocation)->(20 + 0.75*Evocation)\n Fire - (10 + 0.5*Evocation)->(20 + 0.75*Evocation)\n Spirit - (10 + 0.5*Evocation)->(20 + 0.75*Evocation)\nAoE: 8m + 0.04m*Evocation\nCooldown: 45s\nEnergy: 70\n\n"
                    + "Purge: release a burst of power around the caster that burns enemies and heals allies\nDamage:\n Fire - (4 + 0.4*Evocation)->(8 + 0.8*Evocation)\n Spirit - (4 + 0.4*Evocation)->(8 + 0.8*Evocation)\nAoE: 20m + 0.2m*Evocation\nHealing: 0.5 + 0.5*Alteration in a 20m + 0.2m*Alteration around the caster\nCooldown: 15s\nEnergy: 50\n\n"
                    + "Heal: a channeled ability that increases heal rate the longer its channeled.\nHealing:\n Initial - 10 + Alteration\n Continuous - 2x (pulse count + 0.3*Alteration)\nCooldown: 30s\nEnergy: 40 (initial), 22.5 per pulse\n*Press and hold the ability button to provide continuous healing waves\n**Each healing pulse occurs every .5s\n***Initial pulse removes 1x negative status effect (poison, burning, smoked, wet, frost)\n\n",
                    m_topic = "Legend Priest"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_priest))
                {
                    Tutorial.instance.m_texts.Add(vl_priest);
                }
                Tutorial.TutorialText vl_enchanter = new Tutorial.TutorialText
                {
                    m_label = "Legend: Enchanter",
                    m_name = "VL_Enchanter",
                    m_text = "Enchanters use a variety of indirect abilities to shape the situation in their favor.\n\n" +
                    "Skills: Alteration and Abjuration\n" +
                    "Sacrifice: resin\n\n" +
                    "Weaken: weakens all enemies in a target area.\nAoE: 5m + 0.01m*Alteration\nDebuff:\n Movement Speed -20%+0.1%*Alteration\n Attack Power -15%+0.15%*Alteration\nCooldown: 30s\nEnergy: 40\n*10% of the damage dealt to a weakened enemy is returned as stamina to the attacker\n\n"
                    + "Charm: turn enemies into allies for a short time\nDuration: 30s\nCooldown: 60s\nEnergy: 50\n*Charm does not work on boss enemies\n\n"
                    + "Zone(Biome) Buff: renders a unique, long lasting boon to all nearby allies that differs in each biome.\nCooldown: 180s\nEnergy: 40 (initial) + 60 per second channeled\n*Press and hold the ability button to increase the duration and power of the boon\n**Ally buffs are dependent on their average skill level; the caster's buff is based on their abjuration skill and channeled charge amount\n***The enchanter may 'burn' an active zone buff by pressing the ability button while a zone buff is active; this creates a burst of electric energy from the casters hands that deals damage based on the time remaining on the zone buff\n"
                    + "\nThe benefits of each biome are:\n Meadows - 1 + 0.1 Health every 5s\n\n Black Forest - carry capacity increased by 50 + Abjuration; always under cover\n\n Swamp - poison resistance increased 20% + 0.2%*Abjuration; the player emits a small amount of light\n\n Mountain - frost resistance increased 20% + 0.2%*Abjuration; stamina regeneration 5 + 0.075*Abjuration every 5s\n\n Plains - fire resistance increased 20% + 0.2%*Abjuration; run speed increased 10% + 0.1%*Abjuration\n\n Ocean - lightning resistance increased 20% + 0.2%*Abjuration; swim speed increased 50% + 1%*Abjuration\n\n Mistland -  spirit and frost resistance increased 20% + 0.2%*Abjuration; each attack deals 20 + 0.3*Abjuration as additional frost damage\n\n Ashland - fire and poison resistance increase 20% + 0.2%*Abjuration; each attack deals 26 + 0.4*Abjuration as additional fire damage",
                    m_topic = "Legend Enchanter"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_enchanter))
                {
                    Tutorial.instance.m_texts.Add(vl_enchanter);
                }
                Tutorial.TutorialText vl_monk = new Tutorial.TutorialText
                {
                    m_label = "Legend: Monk",
                    m_name = "VL_Monk",
                    m_text = "Monks are masters of unarmed combat, turning their body into a living weapon.\n\n" +
                    "Skills: Discipline\n" +
                    "Sacrifice: wood\n\n" +
                    "Chi strike: attack with a blow so powerful it creates a shockwave.\nDamage:\n Blunt - 12 + 0.5*Discipline -> 24 + Discipline\nCooldown: 1s\nEnergy: 3 chi\n*Uses chi instead of stamina; build chi through unarmed combat\n**Activate while on the ground to create a powerful frontal attack; use from sufficient height to propel the monk to the ground, creating a powerful AoE attack\n\n"
                    + "Flying Kick: launches into a flying whirlwind kick.\nDamage:\n Blunt - 80% + 0.5% of unarmed damage per hit\nCooldown: 6s\nEnergy: 50\n*Can strike multiple times - attack past or above targets to land multiple hits\n**Attack directly at the target for an assured strike that will rebound the monk into the air (hint: combo with Chi Strike)\n\n"
                    + "Chi Bolt: projects condensed energy that detonates on impact.\nDamage:\n Blunt - 10 + Discipline -> 40 + 2*Discipline\n Spirit - 10 + Discipline + 20 + Discipline\nAoE - 3m\nCooldown: 1s\nEnergy: 5 chi\n\n"
                    + "Bonus Skills:\nChi - each unarmed attack that hits and each fully blocked attack generates a charge of chi\nLiving Weapon - unarmed attacks deal 25% more damage\nStrong Body - unarmed block amount is increased by 1 for each level in Discipline and monks can fall from over double the height before taking damage\n\n",
                    m_topic = "Legend Monk"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_monk))
                {
                    Tutorial.instance.m_texts.Add(vl_monk);
                }
                Tutorial.TutorialText vl_shaman = new Tutorial.TutorialText
                {
                    m_label = "Legend: Shaman",
                    m_name = "VL_Shaman",
                    m_text = "Shaman's are known and respected for their ability to inspire their allies to greatness.\n\n" +
                    "Skills: Abjuration, Alteration and Evocation\n" +
                    "Sacrifice: greydwarf eye\n\n" +
                    "Enrage: incite allies into a frenzied rage that increases movement and endurance.\nAugment:\n Speed - 125% + 0.25%*Alteration\n Stamina Regeneration - 5 + 0.1*Alteration per second\nAoE: 30m\nDuration: 20s + 0.2s*Alteration\nCooldown: 120s\nEnergy: 60\n*Skill bonus is calculated as the average of all skills for allies, and Alteration skill for the caster\n\n"
                    + "Shell: surround allies in a protection shell that resists elemental attacks and augments attacks with spirit damage.\nDamage:\n Spirit - 6 + 0.3 * Abjuration added to each attack\nBuff: reduces all elemental damage by 40% + 0.6%*Abjuration\nAoE: 30m\nDuration: 35s + 0.35*Abjuration\nCooldown: 180s\nEnergy: 80\n\n"
                    + "Spirit Shock: generate a powerful blast that shocks all nearby enemies\nDamage:\n Lightning - 6 + 0.4*Evocation -> 12 + 0.6*Evocation\n Spirit - 6 + 0.4*Evocation -> 12 + 0.6*Evocation\nAoE: 11m + 0.05m*Evocation\nCooldown: 30s\nEnergy: 80\n\n",
                    m_topic = "Legend Shaman"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_shaman))
                {
                    Tutorial.instance.m_texts.Add(vl_shaman);
                }
                Tutorial.TutorialText vl_valkyrie = new Tutorial.TutorialText
                {
                    m_label = "Legend: Valkyrie",
                    m_name = "VL_Valkyrie",
                    m_text = "Valkyrie's are a versatile class focused on defense and movement.\n" +
                    "Sacrifice: flint\n\n" +
                    "Skills: Discipline and Abjuration\n\n" +
                    "Bulwark: manifest a powerful shield that reduces all damage to the valkyrie.\nAugment: damage reduced by 25% + 0.5%*Abjuration\nDuration: 15s + 0.2s*Alteration\nCooldown: 90s\nEnergy: 60\n\n"
                    + "Stagger: send forth a shock wave that staggers all nearby enemies.\nAoE: 6m\nCooldown: 20s\nEnergy: 40\n\n"
                    + "Leap: jump high into the air to come crashing down on your enemies.\nDamage:\n Blunt - 2*Discipline -> 3*Discipline + velocity bonus\nAoE: 8m + 0.05m*Discipline\nCooldown: 15s\nEnergy: 50\n*Velocity bonus is calculated based on the max height reached above ground\n**Leap multiplies existing velocity; triggering leap while running and jumping will produce the longest jumps\n\n",
                    m_topic = "Legend Valkyrie"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_valkyrie))
                {
                    Tutorial.instance.m_texts.Add(vl_valkyrie);
                }
                Tutorial.TutorialText vl_ranger = new Tutorial.TutorialText
                {
                    m_label = "Legend: Ranger",
                    m_name = "VL_Ranger",
                    m_text = "The Ranger fearless warriors with peerless survival techniques\n\n" +
                    "Skills: Discipline and Conjuration\n" +
                    "Sacrifice: raw meat\n\n" +
                    "Shadow Stalk: fade into the shadows gaining a burst of speed and augmenting stealth.\nAugment:\n All movement speed increased by 50% + 1%*Discipline for 3s + 0.03s*Discipline\n Stealth movement speed increased by 50% + 1%*Discipline\nDuration: 30s + 0.9s*Discipline\nCooldown: 120s\nEnergy: 40\n*Shadow stalk causes enemies to lose track of the ranger\n\n"
                    + "Shadow Wolf: call a trained shadow wolf to fight by your side.\nDamage:\n Slash - 70 * (0.05 + 0.01*Conjuration)\nHealth: 25 + 9*Conjuration\nHealth Regeneration: 1 + 0.1*Conjuration every 5s\nCooldown: 10m\nEnergy: 75\n*Shadow wolves will vanish when the player logs out or after the duration expires\n**Feeding the shadow wolf will restore its health by 250hp\n\n"
                    + "Power Shot: charge the next few projectiles with great velocity and damage.\nDamage:\n Bonus - 40% + 1.5%*Discipline\nVelocity doubled\nCharge Count: 3 + 0.05*Discipline\nCooldown: 60s\nEnergy: 60\n*Bonus damage applies to all projectiles created by the player (not just arrows)\n**Using Power Shot while the buff is still active will refresh the number of charges\n\n",
                    m_topic = "Legend Ranger"
                };
                if (!Tutorial.instance.m_texts.Contains(vl_ranger))
                {
                    Tutorial.instance.m_texts.Add(vl_ranger);
                }
            }
        }

        [HarmonyPatch(typeof(RuneStone), "Interact")]
        public static class ClassOfferingTutorial_Patch
        {
            public static void Postfix()
            {
                Player.m_localPlayer.ShowTutorial("VL_Offerings");
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), "LoadPlayerData")]
        public static class LoadSkillsPatch
        {
            public static void Postfix(PlayerProfile __instance, Player player)
            {
                VL_SkillData skillData = __instance.LoadModData<VL_SkillData>();
                if (player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == DisciplineSkillDef) == null)
                {
                    Skills.Skill skill = (Skills.Skill)AccessTools.Method(typeof(Skills), "GetSkill", (Type[])null, (Type[])null).Invoke(player.GetSkills(), new object[1]
                    {
                        DisciplineSkill
                    });
                    skill.m_level = (float)skillData.level;
                    skill.m_accumulator = skillData.accumulator;                    
                }
                if (player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == AbjurationSkillDef) == null)
                {
                    Skills.Skill skill = (Skills.Skill)AccessTools.Method(typeof(Skills), "GetSkill", (Type[])null, (Type[])null).Invoke(player.GetSkills(), new object[1]
                    {
                        AbjurationSkill
                    });
                    skill.m_level = (float)skillData.level;
                    skill.m_accumulator = skillData.accumulator;
                }
                if (player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == AlterationSkillDef) == null)
                {
                    Skills.Skill skill = (Skills.Skill)AccessTools.Method(typeof(Skills), "GetSkill", (Type[])null, (Type[])null).Invoke(player.GetSkills(), new object[1]
                    {
                        AlterationSkill
                    });
                    skill.m_level = (float)skillData.level;
                    skill.m_accumulator = skillData.accumulator;
                }
                if (player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == ConjurationSkillDef) == null)
                {
                    Skills.Skill skill = (Skills.Skill)AccessTools.Method(typeof(Skills), "GetSkill", (Type[])null, (Type[])null).Invoke(player.GetSkills(), new object[1]
                    {
                        ConjurationSkill
                    });
                    skill.m_level = (float)skillData.level;
                    skill.m_accumulator = skillData.accumulator;
                }
                if (player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == EvocationSkillDef) == null)
                {
                    Skills.Skill skill = (Skills.Skill)AccessTools.Method(typeof(Skills), "GetSkill", (Type[])null, (Type[])null).Invoke(player.GetSkills(), new object[1]
                    {
                        EvocationSkill
                    });
                    skill.m_level = (float)skillData.level;
                    skill.m_accumulator = skillData.accumulator;
                }
                if (player.GetSkills().GetSkillList().FirstOrDefault((Skills.Skill x) => x.m_info == IllusionSkillDef) == null)
                {
                    Skills.Skill skill = (Skills.Skill)AccessTools.Method(typeof(Skills), "GetSkill", (Type[])null, (Type[])null).Invoke(player.GetSkills(), new object[1]
                    {
                        IllusionSkill
                    });
                    skill.m_level = (float)skillData.level;
                    skill.m_accumulator = skillData.accumulator;
                }
            }
        }

        private void Awake()
        {

            //configs
            //ConfigManager.RegisterMod(ModName, this.Config);
            //modEnabled = ConfigManager.RegisterModConfigVariable<bool>(ModName, "modEnabled", true, "General", "Enabled or Disable Valheim Legends Mod", true);
            modEnabled = this.Config.Bind<bool>("General", "modEnabled", true, "Enable/Disable mod");
            chosenClass = this.Config.Bind<string>("General", "chosenClass", "None", "Assigns a class to the player if no class is assigned.\nThis will not overwrite an existing class selection.\nA value of None will not attempt to assign any class.");
            vl_svr_enforceConfigClass = this.Config.Bind<bool>("General", "vl_svr_enforceConfigClass", false, "True - always sets the player class to this value when the player logs in. False - uses player profile to determine class\nDoes not apply if the chosen class is None.");
            //showAbilityIcons = ConfigManager.RegisterModConfigVariable<bool>(ModName, "showAbilityIcons", true, "Display", "Displays Icons on Hud for each ability", true);
            showAbilityIcons = this.Config.Bind<bool>("Display", "showAbilityIcons", true, "Displays Icons on Hud for each ability");
            //iconAlignment = ConfigManager.RegisterModConfigVariable<string>(ModName, "iconAlignment", "horizontal", "Display", "Aligns icons horizontally or vertically off the guardian power icon; options are horizontal or vertical", true);
            iconAlignment = this.Config.Bind<string>("Display", "iconAlignment", "horizontal", "Aligns icons horizontally or vertically off the guardian power icon; options are horizontal or vertical");
            //icon_X_Offset = ConfigManager.RegisterModConfigVariable<float>(ModName, "icon_X_Offset", 0f, "Display", "Offsets the icon bar horizontally. The icon bar is anchored to the Guardian power icon.", true);
            icon_X_Offset = this.Config.Bind<float>("Display", "icon_X_Offset", 0, "Offsets the icon bar horizontally. The icon bar is anchored to the Guardian power icon.");
            //icon_Y_Offset = ConfigManager.RegisterModConfigVariable<float>(ModName, "icon_Y_Offset", 0f, "Display", "Offsets the icon bar vertically. The icon bar is anchored to the Guardian power icon.", true);
            icon_Y_Offset = this.Config.Bind<float>("Display", "icon_Y_Offset", 0, "Offsets the icon bar vertically. The icon bar is anchored to the Guardian power icon.");
            //Ability1_Hotkey = ConfigManager.RegisterModConfigVariable<string>(ModName, "Ability1_Hotkey", "Z", "Keybinds", "Ability 1 Hotkey", true); //\nUse mouse # to bind an ability to a mouse button\nThe # represents the mouse button; mouse 0 is left click, mouse 1 right click, etc", true);
            Ability1_Hotkey = this.Config.Bind<string>("Keybinds", "Ability1_Hotkey", "Z", "Ability 1 Hotkey\nUse mouse # to bind an ability to a mouse button\nThe # represents the mouse button; mouse 0 is left click, mouse 1 right click, etc");
            //Ability1_Hotkey_Combo = ConfigManager.RegisterModConfigVariable<string>(ModName, "Ability1_Hotkey_Combo", "", "Keybinds", "Ability 1 Combination Key - entering a value will trigger the ability only when both the Hotkey and Hotkey_Combo buttons are pressed", true); //\nAllows input from a combination of keys when a value is entered for the combo key\nIf only one key is used, leave the combo key blank\nExamples: space, Q, left shift, left ctrl, right alt, right cmd", true);
            Ability1_Hotkey_Combo = this.Config.Bind<string>("Keybinds", "Ability1_Hotkey_Combo", "", "Ability 1 Combination Key - entering a value will trigger the ability only when both the Hotkey and Hotkey_Combo buttons are pressed\nAllows input from a combination of keys when a value is entered for the combo key\nIf only one key is used, leave the combo key blank\nExamples: space, Q, left shift, left ctrl, right alt, right cmd");
            //Ability2_Hotkey = ConfigManager.RegisterModConfigVariable<string>(ModName, "Ability2_Hotkey", "X", "Keybinds", "Ability 2 Combination Key", true);
            Ability2_Hotkey = this.Config.Bind<string>("Keybinds", "Ability2_Hotkey", "X", "Ability 2 Hotkey");
            //Ability2_Hotkey_Combo = ConfigManager.RegisterModConfigVariable<string>(ModName, "Ability2_Hotkey_Combo", "", "Keybinds", "Ability 2 Combination Key", true);
            Ability2_Hotkey_Combo = this.Config.Bind<string>("Keybinds", "Ability2_Hotkey_Combo", "", "Ability 2 Combination Key");
            //Ability3_Hotkey = ConfigManager.RegisterModConfigVariable<string>(ModName, "Ability3_Hotkey", "C", "Keybinds", "Ability 3 Hotkey", true);
            Ability3_Hotkey = this.Config.Bind<string>("Keybinds", "Ability3_Hotkey", "C", "Ability 3 Hotkey");
            //Ability3_Hotkey_Combo = ConfigManager.RegisterModConfigVariable<string>(ModName, "Ability3_Hotkey_Combo", "", "Keybinds", "Ability 3 Combination Key", true);
            Ability3_Hotkey_Combo = this.Config.Bind<string>("Keybinds", "Ability3_Hotkey_Combo", "", "Ability 3 Combination Key");
            //energyCostMultiplier = ConfigManager.RegisterModConfigVariable<float>(ModName, "energyCostMultiplier", 1f, "Modifiers", "This value multiplied on overall ability use energy cost", false);
            vl_svr_energyCostMultiplier = this.Config.Bind<string>("Modifiers", "vl_svr_energyCostMultiplier", "1.0", "Ability modifiers are always enforced by the server host\nThis value multiplied on overall ability use energy cost");
            //cooldownMultiplier = ConfigManager.RegisterModConfigVariable<float>(ModName, "cooldownMultiplier", 1f, "Modifiers", "This value multiplied on overall cooldown time of abilities", false);
            vl_svr_cooldownMultiplier = this.Config.Bind<string>("Modifiers", "vl_svr_cooldownMultiplier", "1.0", "This value multiplied on overall cooldown time of abilities");
            //abilityDamageMultiplier = ConfigManager.RegisterModConfigVariable<float>(ModName, "abilityDamageMultiplier", 1f, "Modifiers", "This value multiplied on overall ability power", false);
            vl_svr_abilityDamageMultiplier = this.Config.Bind<string>("Modifiers", "vl_svr_abilityDamageMultiplier", "1.0", "This value multiplied on overall ability power");
            //skillGainMultiplier = ConfigManager.RegisterModConfigVariable<float>(ModName, "skillGainMultiplier", 1f, "Modifiers", "This value modifies the amount of skill experience gained after using an ability", false);
            vl_svr_skillGainMultiplier = this.Config.Bind<string>("Modifiers", "vl_svr_skillGainMultiplier", "1.0", "This value modifies the amount of skill experience gained after using an ability");

            VL_GlobalConfigs.ConfigStrings = new Dictionary<string, float>();
            VL_GlobalConfigs.ConfigStrings.Clear();
            VL_GlobalConfigs.ConfigStrings.Add("vl_svr_energyCostMultiplier", float.Parse(vl_svr_energyCostMultiplier.Value));
            VL_GlobalConfigs.ConfigStrings.Add("vl_svr_cooldownMultiplier", float.Parse(vl_svr_cooldownMultiplier.Value));
            VL_GlobalConfigs.ConfigStrings.Add("vl_svr_abilityDamageMultiplier",float.Parse(vl_svr_abilityDamageMultiplier.Value));
            VL_GlobalConfigs.ConfigStrings.Add("vl_svr_skillGainMultiplier", float.Parse(vl_svr_skillGainMultiplier.Value));
            VL_GlobalConfigs.ConfigStrings.Add("vl_svr_enforceConfigClass", vl_svr_enforceConfigClass.Value ? 1f : 0f);
            //VL_GlobalConfigs.ConfigStrings.Add("vl_svr_version", Version);            

            //assets
            VL_Utility.ModID = "valheim.torann.valheimlegends";
            VL_Utility.Folder = Path.GetDirectoryName(this.Info.Location);
            Texture2D tex_abjuration = VL_Utility.LoadTextureFromAssets("abjuration_skill.png");
            Sprite icon_abjuration = Sprite.Create(tex_abjuration, new Rect(0f, 0f, (float)tex_abjuration.width, (float)tex_abjuration.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_conjuration = VL_Utility.LoadTextureFromAssets("conjuration_skill.png");
            Sprite icon_conjuration = Sprite.Create(tex_conjuration, new Rect(0f, 0f, (float)tex_conjuration.width, (float)tex_conjuration.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_alteration = VL_Utility.LoadTextureFromAssets("alteration_skill.png");
            Sprite icon_alteration = Sprite.Create(tex_alteration, new Rect(0f, 0f, (float)tex_alteration.width, (float)tex_alteration.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_discipline = VL_Utility.LoadTextureFromAssets("discipline_skill.png");
            Sprite icon_discipline = Sprite.Create(tex_discipline, new Rect(0f, 0f, (float)tex_discipline.width, (float)tex_discipline.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_evocation = VL_Utility.LoadTextureFromAssets("evocation_skill.png");
            Sprite icon_evocation = Sprite.Create(tex_evocation, new Rect(0f, 0f, (float)tex_evocation.width, (float)tex_evocation.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_illusion = VL_Utility.LoadTextureFromAssets("illusion_skill.png");
            Sprite icon_illusion = Sprite.Create(tex_illusion, new Rect(0f, 0f, (float)tex_illusion.width, (float)tex_illusion.height), new Vector2(0.5f, 0.5f));

            Texture2D tex_movement = VL_Utility.LoadTextureFromAssets("movement_icon.png");
            Ability3_Sprite = Sprite.Create(tex_movement, new Rect(0f, 0f, (float)tex_movement.width, (float)tex_movement.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_strength = VL_Utility.LoadTextureFromAssets("strength_icon.png");
            Ability2_Sprite = Sprite.Create(tex_strength, new Rect(0f, 0f, (float)tex_strength.width, (float)tex_strength.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_protection = VL_Utility.LoadTextureFromAssets("protection_icon.png");
            Ability1_Sprite = Sprite.Create(tex_protection, new Rect(0f, 0f, (float)tex_protection.width, (float)tex_protection.height), new Vector2(0.5f, 0.5f));

            Texture2D tex_riposte = VL_Utility.LoadTextureFromAssets("riposte_icon.png");
            RiposteIcon = Sprite.Create(tex_riposte, new Rect(0f, 0f, (float)tex_riposte.width, (float)tex_riposte.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_rogue = VL_Utility.LoadTextureFromAssets("rogue_icon.png");
            RogueIcon = Sprite.Create(tex_rogue, new Rect(0f, 0f, (float)tex_rogue.width, (float)tex_rogue.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_monk = VL_Utility.LoadTextureFromAssets("monk_icon.png");
            MonkIcon = Sprite.Create(tex_monk, new Rect(0f, 0f, (float)tex_monk.width, (float)tex_monk.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_weaken = VL_Utility.LoadTextureFromAssets("weaken_icon.png");
            WeakenIcon = Sprite.Create(tex_weaken, new Rect(0f, 0f, (float)tex_weaken.width, (float)tex_weaken.height), new Vector2(0.5f, 0.5f));

            Texture2D tex_biome_meadows = VL_Utility.LoadTextureFromAssets("biome_meadows_icon.png");
            BiomeMeadowsIcon = Sprite.Create(tex_biome_meadows, new Rect(0f, 0f, (float)tex_biome_meadows.width, (float)tex_biome_meadows.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_biome_blackforest = VL_Utility.LoadTextureFromAssets("biome_blackforest_icon.png");
            BiomeBlackForestIcon = Sprite.Create(tex_biome_blackforest, new Rect(0f, 0f, (float)tex_biome_blackforest.width, (float)tex_biome_blackforest.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_biome_swamp = VL_Utility.LoadTextureFromAssets("biome_swamp_icon.png");
            BiomeSwampIcon = Sprite.Create(tex_biome_swamp, new Rect(0f, 0f, (float)tex_biome_swamp.width, (float)tex_biome_swamp.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_biome_mountain = VL_Utility.LoadTextureFromAssets("biome_mountain_icon.png");
            BiomeMountainIcon = Sprite.Create(tex_biome_mountain, new Rect(0f, 0f, (float)tex_biome_mountain.width, (float)tex_biome_mountain.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_biome_plains = VL_Utility.LoadTextureFromAssets("biome_plains_icon.png");
            BiomePlainsIcon = Sprite.Create(tex_biome_plains, new Rect(0f, 0f, (float)tex_biome_plains.width, (float)tex_biome_plains.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_biome_ocean = VL_Utility.LoadTextureFromAssets("biome_ocean_icon.png");
            BiomeOceanIcon = Sprite.Create(tex_biome_ocean, new Rect(0f, 0f, (float)tex_biome_ocean.width, (float)tex_biome_ocean.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_biome_mist = VL_Utility.LoadTextureFromAssets("biome_mist_icon.png");
            BiomeMistIcon = Sprite.Create(tex_biome_mist, new Rect(0f, 0f, (float)tex_biome_mist.width, (float)tex_biome_mist.height), new Vector2(0.5f, 0.5f));
            Texture2D tex_biome_ash = VL_Utility.LoadTextureFromAssets("biome_ash_icon.png");
            BiomeAshIcon = Sprite.Create(tex_biome_ash, new Rect(0f, 0f, (float)tex_biome_ash.width, (float)tex_biome_ash.height), new Vector2(0.5f, 0.5f));


            LoadModAssets_Awake();

            //skills            
            AbjurationSkillDef = new Skills.SkillDef
            {
                m_skill = (Skills.SkillType)AbjurationSkillID,
                m_icon = icon_abjuration,
                m_description = "Skill in creating protective spells and wards",
                m_increseStep = 1f
            };
            AlterationSkillDef = new Skills.SkillDef
            {
                m_skill = (Skills.SkillType)AlterationSkillID,
                m_icon = icon_alteration,
                m_description = "Skill in temporarily enhancing or modifying attributes",
                m_increseStep = 1f
            };
            ConjurationSkillDef = new Skills.SkillDef
            {
                m_skill = (Skills.SkillType)ConjurationSkillID,
                m_icon = icon_conjuration,
                m_description = "Skill in temporarily manifesting reality by molding objects and energy",                
                m_increseStep = 1f
            };
            DisciplineSkillDef = new Skills.SkillDef
            {
                m_skill = (Skills.SkillType)DisciplineSkillID,
                m_icon = icon_discipline,
                m_description = "Ability to perform or resist phenomenal feats through strength of body and mind",
                m_increseStep = 1f
            };
            EvocationSkillDef = new Skills.SkillDef
            {
                m_skill = (Skills.SkillType)EvocationSkillID,
                m_icon = icon_evocation,
                m_description = "Skill in creating and manipulating energy",
                m_increseStep = 1f
            };
            IllusionSkillDef = new Skills.SkillDef
            {
                m_skill = (Skills.SkillType)IllusionSkillID,
                m_icon = icon_illusion,
                m_description = "Skill in creating convincing illusions",
                m_increseStep = 1f
            };
            legendsSkills.Add(DisciplineSkillDef);
            legendsSkills.Add(AbjurationSkillDef);
            legendsSkills.Add(AlterationSkillDef);
            legendsSkills.Add(ConjurationSkillDef);
            legendsSkills.Add(EvocationSkillDef);
            legendsSkills.Add(IllusionSkillDef);

            _Harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)"valheim.torann.valheimlegends");
            
        }

        private void OnDestroy()
        {
            if (_Harmony != null)
            {
                _Harmony.UnpatchSelf();
            }
        }

        public static void SetVLPlayer(Player p)
        {
            vl_player = new VL_Player();
            foreach (VL_Player player in vl_playerList)
            {
                if (p.GetPlayerName() == player.vl_name)
                {
                    vl_player.vl_name = player.vl_name;
                    vl_player.vl_class = (PlayerClass)player.vl_class;
                    if((vl_player.vl_class == PlayerClass.None && chosenClass.Value.ToLower() != "none") || (chosenClass.Value.ToLower() != "none" && VL_GlobalConfigs.ConfigStrings["vl_svr_enforceConfigClass"] != 0))
                    {
                        string newClass = chosenClass.Value.ToLower();
                        if(newClass == "berserker")
                        {
                            vl_player.vl_class = PlayerClass.Berserker;                            
                        }
                        else if(newClass == "druid")
                        {
                            vl_player.vl_class = PlayerClass.Druid;
                        }
                        else if(newClass == "mage")
                        {
                            vl_player.vl_class = PlayerClass.Mage;
                        }
                        else if(newClass == "ranger")
                        {
                            vl_player.vl_class = PlayerClass.Ranger;
                        }
                        else if(newClass == "shaman")
                        {
                            vl_player.vl_class = PlayerClass.Shaman;
                        }
                        else if(newClass == "valkyrie")
                        {
                            vl_player.vl_class = PlayerClass.Valkyrie;
                        }
                        else if(newClass == "metavoker")
                        {
                            vl_player.vl_class = PlayerClass.Metavoker;
                        }
                        else if (newClass == "priest")
                        {
                            vl_player.vl_class = PlayerClass.Priest;
                        }
                        //else if (newClass == "necromancer")
                        //{
                        //    vl_player.vl_class = PlayerClass.Necromancer;
                        //}
                        else if (newClass == "monk")
                        {
                            vl_player.vl_class = PlayerClass.Monk;
                        }
                        else if (newClass == "duelist")
                        {
                            vl_player.vl_class = PlayerClass.Duelist;
                        }
                        else if (newClass == "enchanter")
                        {
                            vl_player.vl_class = PlayerClass.Enchanter;
                        }
                        else if (newClass == "rogue")
                        {
                            vl_player.vl_class = PlayerClass.Rogue;
                        }
                    }

                    NameCooldowns();
                    //break;
                }
            }
        }

        public static void UpdateVLPlayer(Player p)
        {
            foreach(VL_Player player in vl_playerList)
            {
                if(p.GetPlayerName() == player.vl_name)
                {
                    player.vl_class = vl_player.vl_class;
                }
            }
        }

        public static void NameCooldowns()
        {    
            if (vl_player.vl_class == PlayerClass.Mage)
            {
                ZLog.Log("Valheim Legend: Mage");
                Ability1_Name = "Fireball";
                Ability2_Name = "F. Nova";
                Ability3_Name = "Meteor";
                Player.m_localPlayer.ShowTutorial("VL_Mage");
            }
            else if (vl_player.vl_class == PlayerClass.Druid)
            {
                ZLog.Log("Valheim Legend: Druid");
                Ability1_Name = "Regen";
                Ability2_Name = "Living Def.";
                Ability3_Name = "Vines";
                Player.m_localPlayer.ShowTutorial("VL_Druid");
            }
            else if (vl_player.vl_class == PlayerClass.Shaman)
            {
                ZLog.Log("Valheim Legend: Shaman");
                Ability1_Name = "Enrage";
                Ability2_Name = "Shell";
                Ability3_Name = "S. Shock";
                Player.m_localPlayer.ShowTutorial("VL_Shaman");
            }
            else if (vl_player.vl_class == PlayerClass.Ranger)
            {
                ZLog.Log("Valheim Legend: Ranger");
                Ability1_Name = "Shadow";
                Ability2_Name = "Wolf";
                Ability3_Name = "P. Shot";
                Player.m_localPlayer.ShowTutorial("VL_Ranger");
            }
            else if (vl_player.vl_class == PlayerClass.Berserker)
            {
                ZLog.Log("Valheim Legend: Berserker");
                Ability1_Name = "Execute";
                Ability2_Name = "Berserk";
                Ability3_Name = "Dash";
                Player.m_localPlayer.ShowTutorial("VL_Berserker");
            }
            else if (vl_player.vl_class == PlayerClass.Valkyrie)
            {
                ZLog.Log("Valheim Legend: Valkyrie");
                Ability1_Name = "Bulwark";
                Ability2_Name = "Stagger";
                Ability3_Name = "Leap";
                Player.m_localPlayer.ShowTutorial("VL_Valkyrie");
            }
            else if (vl_player.vl_class == PlayerClass.Metavoker)
            {
                ZLog.Log("Valheim Legend: Metavoker");
                Ability1_Name = "Light";
                Ability2_Name = "Replica";
                Ability3_Name = "Warp";
                Player.m_localPlayer.ShowTutorial("VL_Metavoker");
            }
            else if (vl_player.vl_class == PlayerClass.Duelist)
            {
                ZLog.Log("Valheim Legend: Duelist");
                Ability1_Name = "Hip Shot";
                Ability2_Name = "Riposte";
                Ability3_Name = "S. Slash";
                Player.m_localPlayer.ShowTutorial("VL_Duelist");
            }
            else if (vl_player.vl_class == PlayerClass.Priest)
            {
                ZLog.Log("Valheim Legend: Priest");
                Ability1_Name = "Sanctify";
                Ability2_Name = "Purge";
                Ability3_Name = "Heal";
                Player.m_localPlayer.ShowTutorial("VL_Priest");
            }
            else if (vl_player.vl_class == PlayerClass.Rogue)
            {
                ZLog.Log("Valheim Legend: Rogue");
                Ability1_Name = "P. Bomb";
                Ability2_Name = "Fade";
                Ability3_Name = "Backstab";
                Player.m_localPlayer.ShowTutorial("VL_Rogue");
            }
            else if (vl_player.vl_class == PlayerClass.Monk)
            {
                ZLog.Log("Valheim Legend: Monk");
                Ability1_Name = "Ch'i Strike";
                Ability2_Name = "F. Kick";
                Ability3_Name = "Ch'i Blast";
                Player.m_localPlayer.ShowTutorial("VL_Monk");
            }
            else if (vl_player.vl_class == PlayerClass.Enchanter)
            {
                ZLog.Log("Valheim Legend: Enchanter");
                Ability1_Name = "Weaken";
                Ability2_Name = "Charm";
                Ability3_Name = "Z. Charge";
                Player.m_localPlayer.ShowTutorial("VL_Enchanter");
            }
            else
            {
                ZLog.Log("Valheim Legend: --None--");
            }
        }

        //Custom prefabs
        private static GameObject VL_Deathsquit;
        private static GameObject VL_ShadowWolf;
        private static GameObject VL_Light;
        private static GameObject VL_SanctifyHammer;
        private static GameObject VL_PoisonBomb;
        private static GameObject VL_PoisonBombExplosion;
        private static GameObject VL_ThrowingKnife;
        private static GameObject VL_PsiBolt;
        private static GameObject VL_Charm;
        //private static GameObject VL_BiomeLight;

        private static GameObject fx_VL_Lightburst;
        private static GameObject fx_VL_ParticleLightburst;
        private static GameObject fx_VL_ParticleLightSuction;
        private static GameObject fx_VL_ReverseLightburst;
        private static GameObject fx_VL_BlinkStrike;
        private static GameObject fx_VL_QuickShot;
        private static GameObject fx_VL_Purge;
        private static GameObject fx_VL_Smokeburst;
        private static GameObject fx_VL_Shadowburst;
        private static GameObject fx_VL_Shockwave;
        private static GameObject fx_VL_FlyingKick;
        private static GameObject fx_VL_MeteorSlam;
        private static GameObject fx_VL_Weaken;
        private static GameObject fx_VL_WeakenStatus;
        private static GameObject fx_VL_Shock;
        private static GameObject fx_VL_ParticleTailField;
        private static GameObject fx_VL_ParticleFieldBurst;
        private static GameObject fx_VL_ChiPulse;
        private static GameObject fx_VL_HeavyCrit;

        private static GameObject fx_VL_HealPulse;

        public static AnimationClip anim_player_float;

        private static void LoadModAssets_Awake()
        {
            var assetBundle = GetAssetBundleFromResources("vl_assetbundle");
            VL_Deathsquit = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_Deathsquit.prefab");
            VL_ShadowWolf = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_ShadowWolf.prefab");
            VL_Light = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_Light.prefab");
            VL_SanctifyHammer = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_SanctifyHammer.prefab");
            VL_PoisonBomb = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_PoisonBomb.prefab");
            VL_PoisonBombExplosion = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_PoisonBombExplosion.prefab");
            VL_ThrowingKnife = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_ThrowingKnife.prefab");
            VL_PsiBolt = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_PsiBolt.prefab");
            VL_Charm = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_Charm.prefab");
            //VL_BiomeLight = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/VL_BiomeLight.prefab");

            fx_VL_Lightburst = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_Lightburst.prefab");
            fx_VL_ParticleLightburst = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_ParticleLightburst.prefab");
            fx_VL_ParticleLightSuction = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_ParticleLightSuction.prefab");
            fx_VL_ReverseLightburst = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_ReverseLightburst.prefab");
            fx_VL_BlinkStrike = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_BlinkStrike.prefab");
            fx_VL_QuickShot = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_QuickShot.prefab");
            fx_VL_HealPulse = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_HealPulse.prefab");
            fx_VL_Purge = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_Purge.prefab");
            fx_VL_Smokeburst = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_Smokeburst.prefab");
            fx_VL_Shadowburst = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_Shadowburst.prefab");
            fx_VL_Shockwave = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_Shockwave.prefab");
            fx_VL_FlyingKick = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_FlyingKick.prefab");
            fx_VL_MeteorSlam = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_MeteorSlam.prefab");
            fx_VL_Weaken = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_Weaken.prefab");
            fx_VL_WeakenStatus = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_WeakenStatus.prefab");
            fx_VL_Shock = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_Shock.prefab");
            fx_VL_ParticleTailField = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_ParticleTailField.prefab");
            fx_VL_ParticleFieldBurst = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_ParticleFieldBurst.prefab");
            fx_VL_HeavyCrit = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_HeavyCrit.prefab");
            fx_VL_ChiPulse = assetBundle.LoadAsset<GameObject>("Assets/CustomAssets/fx_VL_ChiPulse.prefab");

            anim_player_float = assetBundle.LoadAsset<AnimationClip>("Assets/CustomAssets/anim_float.anim");
        }

        public static AssetBundle GetAssetBundleFromResources(string fileName)
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            var stream = execAssembly.GetManifestResourceStream(resourceName);

            using (stream)
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }

        private static void Add_VL_Assets()
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0)
            {
                return;
            }

            var itemDrop = VL_Deathsquit.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                if (ObjectDB.instance.GetItemPrefab(VL_Deathsquit.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_Deathsquit);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_Deathsquit.name.GetStableHashCode()] = VL_Deathsquit;
                }
                if (ObjectDB.instance.GetItemPrefab(VL_ShadowWolf.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_ShadowWolf);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_ShadowWolf.name.GetStableHashCode()] = VL_ShadowWolf;
                }
                if (ObjectDB.instance.GetItemPrefab(VL_Light.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_Light);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_Light.name.GetStableHashCode()] = VL_Light;
                }
                if (ObjectDB.instance.GetItemPrefab(VL_PoisonBomb.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_PoisonBomb);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_PoisonBomb.name.GetStableHashCode()] = VL_PoisonBomb;
                }
                if (ObjectDB.instance.GetItemPrefab(VL_PoisonBombExplosion.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_PoisonBombExplosion);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_PoisonBombExplosion.name.GetStableHashCode()] = VL_PoisonBombExplosion;
                }
                if (ObjectDB.instance.GetItemPrefab(VL_ThrowingKnife.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_ThrowingKnife);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_ThrowingKnife.name.GetStableHashCode()] = VL_ThrowingKnife;
                }
                if (ObjectDB.instance.GetItemPrefab(VL_PsiBolt.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_PsiBolt);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_PsiBolt.name.GetStableHashCode()] = VL_PsiBolt;
                }
                if (ObjectDB.instance.GetItemPrefab(VL_Charm.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_Charm);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_Charm.name.GetStableHashCode()] = VL_Charm;
                }
                //if (ObjectDB.instance.GetItemPrefab(VL_BiomeLight.name.GetStableHashCode()) == null)
                //{
                //    ObjectDB.instance.m_items.Add(VL_BiomeLight);
                //    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                //    m_itemsByHash[VL_BiomeLight.name.GetStableHashCode()] = VL_BiomeLight;
                //}


                if (ObjectDB.instance.GetItemPrefab(VL_SanctifyHammer.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(VL_SanctifyHammer);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[VL_SanctifyHammer.name.GetStableHashCode()] = VL_SanctifyHammer;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_Lightburst.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_Lightburst);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_Lightburst.name.GetStableHashCode()] = fx_VL_Lightburst;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_ParticleLightburst.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_ParticleLightburst);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_ParticleLightburst.name.GetStableHashCode()] = fx_VL_ParticleLightburst;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_ParticleLightSuction.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_ParticleLightSuction);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_ParticleLightSuction.name.GetStableHashCode()] = fx_VL_ParticleLightSuction;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_ReverseLightburst.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_ReverseLightburst);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_ReverseLightburst.name.GetStableHashCode()] = fx_VL_ReverseLightburst;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_BlinkStrike.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_BlinkStrike);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_BlinkStrike.name.GetStableHashCode()] = fx_VL_BlinkStrike;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_QuickShot.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_QuickShot);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_QuickShot.name.GetStableHashCode()] = fx_VL_QuickShot;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_HealPulse.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_HealPulse);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_HealPulse.name.GetStableHashCode()] = fx_VL_HealPulse;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_Purge.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_Purge);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_Purge.name.GetStableHashCode()] = fx_VL_Purge;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_Smokeburst.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_Smokeburst);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_Smokeburst.name.GetStableHashCode()] = fx_VL_Smokeburst;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_Shadowburst.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_Shadowburst);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_Shadowburst.name.GetStableHashCode()] = fx_VL_Shadowburst;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_Shockwave.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_Shockwave);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_Shockwave.name.GetStableHashCode()] = fx_VL_Shockwave;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_FlyingKick.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_FlyingKick);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_FlyingKick.name.GetStableHashCode()] = fx_VL_FlyingKick;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_MeteorSlam.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_MeteorSlam);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_MeteorSlam.name.GetStableHashCode()] = fx_VL_MeteorSlam;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_Weaken.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_Weaken);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_Weaken.name.GetStableHashCode()] = fx_VL_Weaken;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_WeakenStatus.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_WeakenStatus);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_WeakenStatus.name.GetStableHashCode()] = fx_VL_WeakenStatus;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_Shock.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_Shock);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_Shock.name.GetStableHashCode()] = fx_VL_Shock;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_ParticleTailField.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_ParticleTailField);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_ParticleTailField.name.GetStableHashCode()] = fx_VL_ParticleTailField;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_ParticleFieldBurst.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_ParticleFieldBurst);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_ParticleFieldBurst.name.GetStableHashCode()] = fx_VL_ParticleFieldBurst;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_HeavyCrit.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_HeavyCrit);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_HeavyCrit.name.GetStableHashCode()] = fx_VL_HeavyCrit;
                }
                if (ObjectDB.instance.GetItemPrefab(fx_VL_ChiPulse.name.GetStableHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(fx_VL_ChiPulse);
                    Dictionary<int, GameObject> m_itemsByHash = (Dictionary<int, GameObject>)typeof(ObjectDB).GetField("m_itemByHash", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ObjectDB.instance);
                    m_itemsByHash[fx_VL_ChiPulse.name.GetStableHashCode()] = fx_VL_ChiPulse;
                }
            }
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class ZNetScene_Awake_Patch
        {
            public static void Prefix(ZNetScene __instance)
            {
                if (__instance == null)
                {
                    return;
                }

                __instance.m_prefabs.Add(VL_Deathsquit);
                __instance.m_prefabs.Add(VL_ShadowWolf);
                __instance.m_prefabs.Add(VL_Light);
                __instance.m_prefabs.Add(VL_PoisonBomb);
                __instance.m_prefabs.Add(VL_PoisonBombExplosion);
                __instance.m_prefabs.Add(VL_SanctifyHammer);
                __instance.m_prefabs.Add(VL_ThrowingKnife);
                __instance.m_prefabs.Add(VL_PsiBolt);
                __instance.m_prefabs.Add(VL_Charm);
                //__instance.m_prefabs.Add(VL_BiomeLight);
                __instance.m_prefabs.Add(fx_VL_Lightburst);
                __instance.m_prefabs.Add(fx_VL_ParticleLightburst);
                __instance.m_prefabs.Add(fx_VL_ParticleLightSuction);
                __instance.m_prefabs.Add(fx_VL_ReverseLightburst);
                __instance.m_prefabs.Add(fx_VL_BlinkStrike);
                __instance.m_prefabs.Add(fx_VL_QuickShot);
                __instance.m_prefabs.Add(fx_VL_HealPulse);
                __instance.m_prefabs.Add(fx_VL_Purge);
                __instance.m_prefabs.Add(fx_VL_Smokeburst);
                __instance.m_prefabs.Add(fx_VL_Shadowburst);
                __instance.m_prefabs.Add(fx_VL_Shockwave);
                __instance.m_prefabs.Add(fx_VL_FlyingKick);
                __instance.m_prefabs.Add(fx_VL_MeteorSlam);
                __instance.m_prefabs.Add(fx_VL_Weaken);
                __instance.m_prefabs.Add(fx_VL_WeakenStatus);
                __instance.m_prefabs.Add(fx_VL_Shock);
                __instance.m_prefabs.Add(fx_VL_ParticleTailField);
                __instance.m_prefabs.Add(fx_VL_ParticleFieldBurst);
                __instance.m_prefabs.Add(fx_VL_HeavyCrit);
                __instance.m_prefabs.Add(fx_VL_ChiPulse);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
        public static class ObjectDB_CopyOtherDB_Patch
        {
            public static void Postfix()
            {
                Add_VL_Assets();
            }
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        public static class ObjectDB_Awake_Patch
        {
            public static void Postfix()
            {
                Add_VL_Assets();
            }
        }

        public ValheimLegends()
        {
            
        }
    }
}
