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

        [HarmonyPatch(typeof(ObjectDB), "GetStatusEffect", 
            new Type[] {
                typeof(int)
            })]
        public static class ObjectDBGetStatusEffect
        {
            public static void Postfix(ObjectDB __instance, int nameHash, StatusEffect __result)
            {
                if (__result != null)
                {
                    if (nameHash == "SE_Regeneration".GetHashCode() || nameHash == "SE_VL_Regeneration".GetHashCode())
                    {
                        (__result as SE_Regeneration).m_icon = SE_Regeneration.AbilityIcon;
                    }
                    else if (nameHash == "SE_Bulwark".GetHashCode())
                    {
                        (__result as SE_Bulwark).m_icon = SE_Bulwark.AbilityIcon;
                    }
                    else if (nameHash == "SE_Enrage".GetHashCode() || nameHash == "SE_VL_Enrage".GetHashCode())
                    {
                        (__result as SE_Enrage).m_icon = SE_Enrage.AbilityIcon;
                    }
                    else if (nameHash == "SE_Shell".GetHashCode() || nameHash == "SE_VL_Shell".GetHashCode())
                    {
                        (__result as SE_Shell).m_icon = SE_Shell.AbilityIcon;
                    }
                    else if (nameHash == "SE_SpiritDrain".GetHashCode())
                    {
                        (__result as SE_SpiritDrain).m_icon = SE_SpiritDrain.AbilityIcon;
                    }
                    else if (nameHash == "SE_Berserk".GetHashCode())
                    {
                        (__result as SE_Berserk).m_icon = SE_Berserk.AbilityIcon;
                    }
                    else if (nameHash == "SE_Execute".GetHashCode())
                    {
                        (__result as SE_Execute).m_icon = SE_Execute.AbilityIcon;
                    }
                    else if (nameHash == "SE_Slow".GetHashCode())
                    {
                        (__result as SE_Slow).m_icon = SE_Slow.AbilityIcon;
                    }
                    else if (nameHash == "SE_PowerShot".GetHashCode())
                    {
                        (__result as SE_PowerShot).m_icon = SE_PowerShot.AbilityIcon;
                    }
                    else if (nameHash == "SE_ShadowStalk".GetHashCode())
                    {
                        (__result as SE_ShadowStalk).m_icon = SE_ShadowStalk.AbilityIcon;
                    }
                    else if (nameHash == "SE_Companion".GetHashCode())
                    {
                        (__result as SE_Companion).m_icon = SE_Companion.AbilityIcon;
                    }
                    else if (nameHash == "SE_RootsBuff".GetHashCode())
                    {
                        (__result as SE_RootsBuff).m_icon = SE_RootsBuff.AbilityIcon;
                    }
                    else if (nameHash == "SE_Riposte".GetHashCode())
                    {
                        (__result as SE_Riposte).m_icon = SE_Riposte.AbilityIcon;
                    }
                    else if (nameHash == "SE_Rogue".GetHashCode())
                    {
                        (__result as SE_Rogue).m_icon = SE_Rogue.AbilityIcon;
                    }
                    else if (nameHash == "SE_Monk".GetHashCode())
                    {
                        (__result as SE_Monk).m_icon = SE_Monk.AbilityIcon;
                    }
                    else if (nameHash == "SE_Ranger".GetHashCode())
                    {
                        (__result as SE_Ranger).m_icon = SE_Ranger.AbilityIcon;
                    }
                    else if (nameHash == "SE_Valkyrie".GetHashCode())
                    {
                        (__result as SE_Valkyrie).m_icon = SE_Valkyrie.AbilityIcon;
                    }
                    else if (nameHash == "SE_Weaken".GetHashCode())
                    {
                        (__result as SE_Weaken).m_icon = SE_Weaken.AbilityIcon;
                    }

                    if (nameHash == "SE_Ability1_CD".GetHashCode())
                    {
                        (__result as SE_Ability1_CD).m_icon = SE_Ability1_CD.AbilityIcon;
                    }
                    else if (nameHash == "SE_Ability2_CD".GetHashCode())
                    {
                        (__result as SE_Ability2_CD).m_icon = SE_Ability2_CD.AbilityIcon;
                    }
                    else if (nameHash == "SE_Ability3_CD".GetHashCode())
                    {
                        (__result as SE_Ability3_CD).m_icon = SE_Ability3_CD.AbilityIcon;
                    }
                    //else if(name == "SE_DyingLight_CD")
                    //{
                    //    (__result as SE_DyingLight_CD).m_icon = SE_Ability3_CD.AbilityIcon;
                    //}

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
                SE_DyingLight_CD.AbilityIcon = ZNetScene.instance.GetPrefab("TrophySkeleton").GetComponent<ItemDrop>().m_itemData.GetIcon();
                SE_Riposte.AbilityIcon = ValheimLegends.RiposteIcon;
                SE_Rogue.AbilityIcon = ValheimLegends.RogueIcon;
                SE_Monk.AbilityIcon = ValheimLegends.MonkIcon;
                SE_Ranger.AbilityIcon = ValheimLegends.RangerIcon;
                SE_Valkyrie.AbilityIcon = ValheimLegends.ValkyrieIcon;
                SE_Weaken.AbilityIcon = ValheimLegends.WeakenIcon;

                SE_BiomeMeadows.AbilityIcon = ValheimLegends.BiomeMeadowsIcon;
                SE_BiomeBlackForest.AbilityIcon = ValheimLegends.BiomeBlackForestIcon;
                SE_BiomeSwamp.AbilityIcon = ValheimLegends.BiomeSwampIcon;
                SE_BiomeMountain.AbilityIcon = ValheimLegends.BiomeMountainIcon;
                SE_BiomePlains.AbilityIcon = ValheimLegends.BiomePlainsIcon;
                SE_BiomeOcean.AbilityIcon = ValheimLegends.BiomeOceanIcon;
                SE_BiomeMist.AbilityIcon = ValheimLegends.BiomeMistIcon;
                SE_BiomeAsh.AbilityIcon = ValheimLegends.BiomeAshIcon;

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
            //if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_DyingLight_CD"))
            //{
            //    odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_DyingLight_CD>());
            //}
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
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Riposte"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Riposte>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Rogue"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Rogue>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Monk"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Monk>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Ranger"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Ranger>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Valkyrie"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Valkyrie>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_Weaken"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_Weaken>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomeMeadows"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomeMeadows>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomeBlackForest"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomeBlackForest>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomeMountain"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomeMountain>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomeSwamp"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomeSwamp>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomePlains"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomePlains>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomeOcean"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomeOcean>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomeMist"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomeMist>());
            }
            if (!(bool)odb.m_StatusEffects.Find((StatusEffect se) => se.name == "SE_VL_BiomeAsh"))
            {
                odb.m_StatusEffects.Add(ScriptableObject.CreateInstance<SE_BiomeAsh>());
            }
        }
    }
}
