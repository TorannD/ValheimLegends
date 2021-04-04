using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ValheimLegends
{
    public static class ObjectDBPatches
    {
        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        public static class ObjectDBAwake
        {
            public static void Postfix(ObjectDB __instance)
            {
                AddStatusEffect(__instance);
                AddCooldownStatusEffect(__instance);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
        public static class ObjectDBCopyOtherDB
        {
            public static void Postfix(ObjectDB __instance)
            {
                AddStatusEffect(__instance);
                AddCooldownStatusEffect(__instance);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), "GetStatusEffect")]
        public static class ObjectDBGetStatusEffect
        {
            public static void Postfix(ObjectDB __instance, string name, StatusEffect __result)
            {
                if (__result != null)
                {
                    if (name == "SE_Regeneration" || name == "SE_VL_Regeneration")
                    {
                        (__result as SE_Regeneration).m_icon = SE_Regeneration.AbilityIcon;
                    }
                    else if (name == "SE_Bulwark")
                    {
                        (__result as SE_Bulwark).m_icon = SE_Bulwark.AbilityIcon;
                    }
                    else if (name == "SE_Enrage" || name == "SE_VL_Enrage")
                    {
                        (__result as SE_Enrage).m_icon = SE_Enrage.AbilityIcon;
                    }
                    else if (name == "SE_Shell" || name == "SE_VL_Shell")
                    {
                        (__result as SE_Shell).m_icon = SE_Shell.AbilityIcon;
                    }
                    else if (name == "SE_SpiritDrain")
                    {
                        (__result as SE_SpiritDrain).m_icon = SE_SpiritDrain.AbilityIcon;
                    }
                    else if (name == "SE_Berserk")
                    {
                        (__result as SE_Berserk).m_icon = SE_Berserk.AbilityIcon;
                    }
                    else if (name == "SE_Execute")
                    {
                        (__result as SE_Execute).m_icon = SE_Execute.AbilityIcon;
                    }
                    else if (name == "SE_Slow")
                    {
                        (__result as SE_Slow).m_icon = SE_Slow.AbilityIcon;
                    }
                    else if (name == "SE_PowerShot")
                    {
                        (__result as SE_PowerShot).m_icon = SE_PowerShot.AbilityIcon;
                    }
                    else if (name == "SE_ShadowStalk")
                    {
                        (__result as SE_ShadowStalk).m_icon = SE_ShadowStalk.AbilityIcon;
                    }
                    else if (name == "SE_Companion")
                    {
                        (__result as SE_Companion).m_icon = SE_Companion.AbilityIcon;
                    }
                    else if (name == "SE_RootsBuff")
                    {
                        (__result as SE_RootsBuff).m_icon = SE_RootsBuff.AbilityIcon;
                    }

                    if (name == "SE_Ability1_CD")
                    {
                        (__result as SE_Ability1_CD).m_icon = SE_Ability1_CD.AbilityIcon;
                    }
                    else if (name == "SE_Ability2_CD")
                    {
                        (__result as SE_Ability2_CD).m_icon = SE_Ability2_CD.AbilityIcon;
                    }
                    if (name == "SE_Ability3_CD")
                    {
                        (__result as SE_Ability3_CD).m_icon = SE_Ability3_CD.AbilityIcon;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Hud), "Awake")]
        public static class HudAwake
        {
            public static void Postfix(Hud __instance)
            {
                SE_Regeneration.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyGreydwarfShaman").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Bulwark.AbilityIcon = ZNetScene.instance.GetPrefab("ShieldBlackmetalTower").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Enrage.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyGoblinBrute").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Shell.AbilityIcon = ZNetScene.instance.GetPrefab("ShieldSerpentscale").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_SpiritDrain.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyDragonQueen").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Berserk.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyGoblinKing").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Execute.AbilityIcon = ZNetScene.instance.GetPrefab("SwordCheat").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_PowerShot.AbilityIcon = ZNetScene.instance.GetPrefab("ArrowFire").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_ShadowStalk.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyWraith").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Companion.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyWolf").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_RootsBuff.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyWolf").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Slow.AbilityIcon = ZNetScene.instance.GetPrefab("TrophyWolf").GetComponent<ItemDrop>().m_itemData.GetIcon();

                SE_Ability1_CD.AbilityIcon = ZNetScene.instance.GetPrefab("ShieldWood").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Ability2_CD.AbilityIcon = ZNetScene.instance.GetPrefab("ShieldBanded").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Ability3_CD.AbilityIcon = ZNetScene.instance.GetPrefab("ShieldSilver").GetComponent<ItemDrop>().m_itemData.GetIcon();
            }
        }

        private static void AddCooldownStatusEffect(ObjectDB odb)
        {
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Ability1_CD"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Ability1_CD>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Ability2_CD"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Ability2_CD>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Ability3_CD"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Ability3_CD>());
            }
        }

        private static void AddStatusEffect(ObjectDB odb)
        {
            if(!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Regeneration"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Regeneration>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Bulwark"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Bulwark>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Enrage"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Enrage>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Shell"))
            { 
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Shell>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_SpiritDrain"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_SpiritDrain>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Berserk"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Berserk>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Execute"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Execute>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_PowerShot"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_PowerShot>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_ShadowStalk"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_ShadowStalk>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Companion"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Companion>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_RootsBuff"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_RootsBuff>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Slow"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Slow>());
            }
        }
    }
}
