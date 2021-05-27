using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimLegends
{
    public class VL_GlobalConfigs
    {
        public static Dictionary<string, float> ConfigStrings;

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

        public static float g_UnarmedDamage
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_unarmedDamageMultiplier"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_berserkerDash
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_berserkerDash.Value"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_berserkerBerserk
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_berserkerBerserk"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_berserkerExecute
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_berserkerExecute"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_berserkerBonusDamage
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_berserkerBonusDamage"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_berserkerBonus2h
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_berserkerBonus2h"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_druidVines
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_druidVines"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_druidRegen
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_druidRegen"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_druidDefenders
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_druidDefenders"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_druidBonusSeeds
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_druidBonusSeeds"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_duelistSeismicSlash
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_duelistSeismicSlash"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_duelistRiposte
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_duelistRiposte"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_duelistHipShot
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_duelistHipShot"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_duelistBonusParry
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_duelistBonusParry"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_enchanterWeaken
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_enchanterWeaken"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_enchanterCharm
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_enchanterCharm"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_enchanterBiome
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_enchanterBiome"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_enchanterBiomeShock
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_enchanterBiomeShock"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_enchanterBonusElementalBlock
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_enchanterBonusElementalBlock"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_enchanterBonusElementalTouch
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_enchanterbonusElementalTouch"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_mageFireball
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_mageFireball"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_mageFrostDagger
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_mageFrostDagger"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_mageFrostNova
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_mageFrostNova"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_mageInferno
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_mageInferno"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_mageMeteor
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_mageMeteor"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_meteavokerLight
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_metavokerLight"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_metavokerReplica
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_metavokerReplica"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_metavokerWarpDamage
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_metavokerWarpDamage"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_metavokerWarpDistance
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_metavokerWarpDistance"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_metavokerBonusSafeFallCost
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_metavokerBonusSafeFallCost"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_metavokerBonusForceWave
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_metavokerBonusForceWave"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_monkChiPunch
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkChiPunch"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_monkChiSlam
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkChiSlam"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_monkChiBlast
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkChiBlast"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_monkFlyingKick
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkFlyingKick"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_monkBonusBlock
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkBonusBlock"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_monkSurge
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkSurge"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_priestHeal
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_priestHeal"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_priestPurgeHeal
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_priestPurgeHeal"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_priestPurgeDamage
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_priestPurgeDamage"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_priestSanctify
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_priestSanctify"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_priestBonusDyingLightCooldown
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_priestBonusDyingLightCooldown"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_rangerPowerShot
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rangerPowerShot"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_rangerShadowWolf
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rangerShadowWolf"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_rangerShadowStalk
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rangerShadowStalk"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_rangerBonusPoisonResistance
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rangerBonusPoisonResistance"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_rangerBonusRunCost
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rangerBonusRunCost"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_rogueBackstab
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rogueBackstab"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_rogueFadeCooldown
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rogueFadeCooldown"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_roguePoisonBomb
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_roguePoisonBomb"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_rogueBonusThrowingDagger
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rogueBonusThrowingDagger"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_shamanSpiritShock
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_shamanSpiritShock"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_shamanEnrage
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_shamanEnrage"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_shamanShell
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_shamanShell"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_shamanBonusSpiritGuide
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_shamanBonusSpiritGuide"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_shamanBonusWaterGlideCost
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_shamanBonusWaterGlideCost"];
                }
                catch
                {
                    return 1f;
                }
            }
        }

        public static float c_valkyrieLeap
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_valkyrieLeap"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_valkyrieStaggerCooldown
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_valkyrieStaggerCooldown"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_valkyrieBulwark
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_valkyrieBulwark"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_valkyrieBonusChillWave
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_valkyrieBonusChillWave"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_valkyrieBonusIceLance
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_valkyrieBonusIceLance"];
                }
                catch
                {
                    return 1f;
                }
            }
        }
    }   
}
