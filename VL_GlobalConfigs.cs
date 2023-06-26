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
        public static Dictionary<string, string> ItemStrings;

        public static float g_CooldownModifer
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_cooldownMultiplier"]/100f;
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
                    return ConfigStrings["vl_svr_abilityDamageMultiplier"] / 100f;
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
                    return ConfigStrings["vl_svr_energyCostMultiplier"] / 100f;
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
                    return ConfigStrings["vl_svr_skillGainMultiplier"] / 100f;
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
                    return ConfigStrings["vl_svr_unarmedDamageMultiplier"] / 100f;
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
                    return ConfigStrings["vl_svr_berserkerDash.Value"] / 100f;
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
                    return ConfigStrings["vl_svr_berserkerBerserk"] / 100f;
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
                    return ConfigStrings["vl_svr_berserkerExecute"] / 100f;
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
                    return ConfigStrings["vl_svr_berserkerBonusDamage"] / 100f;
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
                    return ConfigStrings["vl_svr_berserkerBonus2h"] / 100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_berserkerItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_berserkerItem"];
                }
                catch
                {
                    return "item_bonefragments";
                }
            }
        }

        public static float c_druidVines
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_druidVines"] / 100f;
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
                    return ConfigStrings["vl_svr_druidRegen"] / 100f;
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
                    return ConfigStrings["vl_svr_druidDefenders"] / 100f;
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
                    return ConfigStrings["vl_svr_druidBonusSeeds"] / 100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_druidItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_druidItem"];
                }
                catch
                {
                    return "item_dandelion";
                }
            }
        }

        public static float c_duelistSeismicSlash
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_duelistSeismicSlash"] / 100f;
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
                    return ConfigStrings["vl_svr_duelistRiposte"] / 100f;
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
                    return ConfigStrings["vl_svr_duelistHipShot"] / 100f;
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
                    return ConfigStrings["vl_svr_duelistBonusParry"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_duelistItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_duelistItem"];
                }
                catch
                {
                    return "item_thistle";
                }
            }
        }

        public static float c_enchanterWeaken
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_enchanterWeaken"]/100f;
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
                    return ConfigStrings["vl_svr_enchanterCharm"]/100f;
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
                    return ConfigStrings["vl_svr_enchanterBiome"]/100f;
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
                    return ConfigStrings["vl_svr_enchanterBiomeShock"]/100f;
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
                    return ConfigStrings["vl_svr_enchanterBonusElementalBlock"]/100f;
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
                    return ConfigStrings["vl_svr_enchanterbonusElementalTouch"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_enchanterItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_enchanterItem"];
                }
                catch
                {
                    return "item_resin";
                }
            }
        }

        public static float c_mageFireball
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_mageFireball"]/100f;
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
                    return ConfigStrings["vl_svr_mageFrostDagger"]/100f;
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
                    return ConfigStrings["vl_svr_mageFrostNova"]/100f;
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
                    return ConfigStrings["vl_svr_mageInferno"]/100f;
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
                    return ConfigStrings["vl_svr_mageMeteor"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_mageItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_mageItem"];
                }
                catch
                {
                    return "item_coal";
                }
            }
        }

        public static float c_meteavokerLight
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_metavokerLight"]/100f;
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
                    return ConfigStrings["vl_svr_metavokerReplica"]/100f;
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
                    return ConfigStrings["vl_svr_metavokerWarpDamage"]/100f;
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
                    return ConfigStrings["vl_svr_metavokerWarpDistance"]/100f;
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
                    return ConfigStrings["vl_svr_metavokerBonusSafeFallCost"]/100f;
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
                    return ConfigStrings["vl_svr_metavokerBonusForceWave"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_metavokerItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_metavokerItem"];
                }
                catch
                {
                    return "item_raspberries";
                }
            }
        }

        public static float c_monkChiPunch
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkChiPunch"]/100f;
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
                    return ConfigStrings["vl_svr_monkChiSlam"]/100f;
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
                    return ConfigStrings["vl_svr_monkChiBlast"]/100f;
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
                    return ConfigStrings["vl_svr_monkFlyingKick"]/100f;
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
                    return ConfigStrings["vl_svr_monkBonusBlock"]/100f;
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
                    return ConfigStrings["vl_svr_monkSurge"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_monkChiDuration
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_monkChiDuration"] / 100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_monkItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_monkItem"];
                }
                catch
                {
                    return "item_wood";
                }
            }
        }

        public static float c_priestHeal
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_priestHeal"]/100f;
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
                    return ConfigStrings["vl_svr_priestPurgeHeal"]/100f;
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
                    return ConfigStrings["vl_svr_priestPurgeDamage"]/100f;
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
                    return ConfigStrings["vl_svr_priestSanctify"]/100f;
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
                    return ConfigStrings["vl_svr_priestBonusDyingLightCooldown"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_priestItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_priestItem"];
                }
                catch
                {
                    return "item_stone";
                }
            }
        }

        public static float c_rangerPowerShot
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rangerPowerShot"]/100f;
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
                    return ConfigStrings["vl_svr_rangerShadowWolf"]/100f;
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
                    return ConfigStrings["vl_svr_rangerShadowStalk"]/100f;
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
                    return ConfigStrings["vl_svr_rangerBonusPoisonResistance"]/100f;
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
                    return ConfigStrings["vl_svr_rangerBonusRunCost"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_rangerItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_rangerItem"];
                }
                catch
                {
                    return "item_boar_meat";
                }
            }
        }

        public static float c_rogueBackstab
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rogueBackstab"]/100f;
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
                    return ConfigStrings["vl_svr_rogueFadeCooldown"]/100f;
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
                    return ConfigStrings["vl_svr_roguePoisonBomb"]/100f;
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
                    return ConfigStrings["vl_svr_rogueBonusThrowingDagger"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_rogueTrickCharge
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_rogueTrickCharge"] / 100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_rogueItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_rogueItem"];
                }
                catch
                {
                    return "item_honey";
                }
            }
        }

        public static float c_shamanSpiritShock
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_shamanSpiritShock"]/100f;
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
                    return ConfigStrings["vl_svr_shamanEnrage"]/100f;
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
                    return ConfigStrings["vl_svr_shamanShell"]/100f;
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
                    return ConfigStrings["vl_svr_shamanBonusSpiritGuide"]/100f;
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
                    return ConfigStrings["vl_svr_shamanBonusWaterGlideCost"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_shamanItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_shamanItem"];
                }
                catch
                {
                    return "item_greydwarfeye";
                }
            }
        }

        public static float c_valkyrieLeap
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_valkyrieLeap"]/100f;
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
                    return ConfigStrings["vl_svr_valkyrieStaggerCooldown"]/100f;
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
                    return ConfigStrings["vl_svr_valkyrieBulwark"]/100f;
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
                    return ConfigStrings["vl_svr_valkyrieBonusChillWave"]/100f;
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
                    return ConfigStrings["vl_svr_valkyrieBonusIceLance"]/100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static float c_valkyrieChargeDuration
        {
            get
            {
                try
                {
                    return ConfigStrings["vl_svr_valkyrieChargeDuration"] / 100f;
                }
                catch
                {
                    return 1f;
                }
            }
        }
        public static string c_valkyrieItem
        {
            get
            {
                try
                {
                    return ItemStrings["vl_svr_valkyrieItem"];
                }
                catch
                {
                    return "item_flint";
                }
            }
        }
    }   
}
