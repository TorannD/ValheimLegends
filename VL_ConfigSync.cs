using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ValheimLegends
{
    public class VL_ConfigSync
    {

        //public const string Version = "0.2.3";
        //public const string ModName = "Valheim Legends";

        //public static ConfigVariable<float> EnergyCost;
        //public static ConfigVariable<float> Cooldown;
        //public static ConfigVariable<float> 

        //public static void MCE_Register()
        //{
        //    ConfigManager.RegisterMod(ModName, ValheimLegends.VL_Config);
        //    ValheimLegends.energyCostMultiplier.Value = ConfigManager.RegisterModConfigVariable(ModName, "energyCostMultiplier", 1f, "Modifiers", "This value multiplied on overall ability use energy cost\nAbility modifiers are not fully implemented", false).Value;
        //    ValheimLegends.cooldownMultiplier.Value = ConfigManager.RegisterModConfigVariable(ModName, "cooldownMultiplier", 1f, "Modifiers", "This value multiplied on overall cooldown time of abilities", false).Value;
        //    ValheimLegends.abilityDamageMultiplier.Value = ConfigManager.RegisterModConfigVariable(ModName, "abilityDamageMultiplier", 1f, "Modifiers", "This value multiplied on overall ability power", false).Value;
        //    ValheimLegends.skillGainMultiplier.Value = ConfigManager.RegisterModConfigVariable(ModName, "skillGainMultiplier", 1f, "Modifiers", "This value modifies the amount of skill experience gained after using an ability", false).Value;
        //    ZLog.Log("Valheim Legends - MCE registration completed successfully.");
        //}
    }
}
