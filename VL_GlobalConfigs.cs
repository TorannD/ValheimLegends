using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimLegends
{
    public class VL_GlobalConfigs
    {
        public static float g_CooldownModifer
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_cooldownMultiplier"];
                }
                catch
                {
                    return 1f;
                }
                
            }
        }

        public static float g_DamageModifer
        {
            get
            {
                
                try
                {
                    return ConfigStrings["vl_svr_abilityDamageMultiplier"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float g_EnergyCostModifer
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_energyCostMultiplier"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float g_SkillGainModifer
        {
            get
            {
                try
                { 
                    return ConfigStrings["vl_svr_skillGainMultiplier"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static Dictionary<string, float> ConfigStrings;

    }

   
}
