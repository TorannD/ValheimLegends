using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;
using UnityEngine.UI;

namespace ValheimLegends
{
    public static class VL_Utility
    {

        //shared
        public static string ModID;
        public static string Folder;

        public static string GetModDataPath(this PlayerProfile profile)
        {
            return Path.Combine(Utils.GetSaveDataPath(), "ModData", ModID, "char_" + profile.GetFilename());
        }
        public static TData LoadModData<TData>(this PlayerProfile profile) where TData : new()
        {
            if (!File.Exists(GetModDataPath(profile)))
            {
                return new TData();
            }
            string text = File.ReadAllText(GetModDataPath(profile));
            return JsonUtility.FromJson<TData>(text);
        }

        public static void SaveModData<TData>(this PlayerProfile profile, TData data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetModDataPath(profile)));
            File.WriteAllText(GetModDataPath(profile), JsonUtility.ToJson((object)data));
        }

        public static Texture2D LoadTextureFromAssets(string path)
        {
            byte[] data = File.ReadAllBytes(Path.Combine(Folder, "VLAssets", path));
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(data);
            return texture2D;
        }

        public static bool TakeInput(Player p)
        {
            bool result = (!(bool)Chat.instance || !Chat.instance.HasFocus()) && !Console.IsVisible() && !TextInput.IsVisible() && !StoreGui.IsVisible() && !InventoryGui.IsVisible() && !Menu.IsVisible() && (!(bool)TextViewer.instance || !TextViewer.instance.IsVisible()) && !Minimap.IsOpen() && !GameCamera.InFreeFly();
            if (p.IsDead() || p.InCutscene() || p.IsTeleporting())
            {
                result = false;
            }
            return result;
        }

        public static void InitiateAbilityStatus(Hud hud)
        {
            if(ValheimLegends.ClassIsValid)
            {
                float xStep = 80f;                
                float yStep = 0f;
                float yOffset = 20f + ValheimLegends.icon_Y_Offset.Value;
                float xOffset = 0f + ValheimLegends.icon_X_Offset.Value;
                if(ValheimLegends.iconAlignment.Value.ToLower() == "vertical")
                {
                    xStep = 0f;
                    yStep = 100f;
                }

                ValheimLegends.abilitiesStatus = new List<RectTransform>();
                ValheimLegends.abilitiesStatus.Clear();
                RectTransform rectTransform = UnityEngine.Object.Instantiate(hud.m_statusEffectTemplate, hud.m_gpRoot);
                rectTransform.gameObject.SetActive(value: true);
                rectTransform.anchoredPosition = new Vector3(xOffset + xStep, yOffset + yStep, 0f);
                rectTransform.GetComponentInChildren<Text>().text = Localization.instance.Localize((ValheimLegends.Ability1_Name).ToString());
                ValheimLegends.abilitiesStatus.Add(rectTransform);
                RectTransform rectTransform2 = UnityEngine.Object.Instantiate(hud.m_statusEffectTemplate, hud.m_gpRoot);
                rectTransform2.gameObject.SetActive(value: true);
                rectTransform2.anchoredPosition = new Vector3(xOffset + (xStep * 2f), yOffset + (yStep *2f), 0f);
                rectTransform2.GetComponentInChildren<Text>().text = Localization.instance.Localize((ValheimLegends.Ability2_Name).ToString());
                ValheimLegends.abilitiesStatus.Add(rectTransform2);
                RectTransform rectTransform3 = UnityEngine.Object.Instantiate(hud.m_statusEffectTemplate, hud.m_gpRoot);
                rectTransform3.gameObject.SetActive(value: true);
                rectTransform3.anchoredPosition = new Vector3(xOffset + (xStep * 3), yOffset + (yStep * 3f), 0f);
                rectTransform3.GetComponentInChildren<Text>().text = Localization.instance.Localize((ValheimLegends.Ability3_Name).ToString());
                ValheimLegends.abilitiesStatus.Add(rectTransform3);
            }
        }

        /// 
        //MAGE
        ///

        public static float GetFireballCost
        {
            get
            {
                return 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetFireballCooldownTime
        {
            get
            {
                return 12f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetFireballSkillGain
        {
            get
            {
                return .9f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetMeteorCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetMeteorCostPerUpdate
        {
            get
            {
                return .5f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetMeteorCooldownTime
        {
            get
            {
                return 180f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetMeteorSkillGain
        {
            get
            {
                return 1.6f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetFrostNovaCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetFrostNovaCooldownTime
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetFrostNovaSkillGain
        {
            get
            {
                return .75f * VL_GlobalConfigs.g_SkillGainModifer; 
            }
        }

        /// 
        //VALKYRIE
        ///

        public static float GetBulwarkCost
        {
            get
            {
                return 60f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetBulwarkCooldownTime
        {
            get
            {
                return 90f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetBulwarkSkillGain
        {
            get
            {
                return 1.2f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetLeapCost
        {
            get
            {
                return 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetLeapCooldownTime
        {
            get
            {
                return 15f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetLeapSkillGain
        {
            get
            {
                return .4f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetStaggerCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetStaggerCooldownTime
        {
            get
            {
                return 20f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetStaggerSkillGain
        {
            get
            {
                return .6f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //DRUID
        ///

        public static float GetRegenerationCost
        {
            get
            {
                return 60f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetRegenerationCooldownTime
        {
            get
            {
                return 120f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetRegenerationSkillGain
        {
            get
            {
                return 1.5f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetRootCost
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetRootCostPerUpdate
        {
            get
            {
                return .3f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetRootCooldownTime
        {
            get
            {
                return 20f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetRootSkillGain
        {
            get
            {
                return .15f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetDefenderCost
        {
            get
            {
                return 80f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetDefenderCooldownTime
        {
            get
            {
                return 120f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetDefenderSkillGain
        {
            get
            {
                return 1.9f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //SHAMAN
        ///

        public static float GetEnrageCost(Player p)
        {
            float cost = 60f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetEnrageCooldown(Player p)
        {
            float time = 120 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetEnrageSkillGain(Player p)
        {
            float xp = 1.4f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }
        public static float GetSpiritBombCost(Player p)
        {
            float cost = 80f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetSpiritBombCooldown(Player p)
        {
            float time = 30 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetSpiritBombSkillGain(Player p)
        {
            float xp = .9f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }
        public static float GetShellCost(Player p)
        {
            float cost = 80f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetShellCooldown(Player p)
        {
            float time = 180 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetShellSkillGain(Player p)
        {
            float xp = 2.2f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }

        /// 
        //BERSERKER
        ///

        public static float GetDashCost(Player p)
        {
            float cost = 70f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetDashCooldown(Player p)
        {
            float time = 10 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetDashSkillGain (Player p)
        {
            float xp = .45f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }
        public static float GetBerserkCost(Player p)
        {
            float cost = 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetBerserkCooldown(Player p)
        {
            float time = 180 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetBerserkSkillGain(Player p)
        {
            float xp = 1.8f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }
        public static float GetExecuteCost(Player p)
        {
            float cost = 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetExecuteCooldown(Player p)
        {
            float time = 120 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetExecuteSkillGain(Player p)
        {
            float xp = 1.2f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }

        /// 
        //RANGER
        ///

        public static float GetPowerShotCost(Player p)
        {
            float cost = 60f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetPowerShotCooldown(Player p)
        {
            float time = 60 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetPowerShotSkillGain(Player p)
        {
            float xp = .9f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }
        public static float GetShadowStalkCost(Player p)
        {
            float cost = 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetShadowStalkCooldown(Player p)
        {
            float time = 120 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetShadowStalkSkillGain(Player p)
        {
            float xp = 1.4f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }
        public static float GetSummonWolfCost(Player p)
        {
            float cost = 75f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetSummonWolfCooldown(Player p)
        {
            float time = 600 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetSummonWolfSkillGain(Player p)
        {
            float xp = 3.5f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }

        //Button press validation
        public static bool Ability1_Input_Down
        {
            get
            {
                if(ValheimLegends.Ability1_Hotkey.Value == "")
                {
                    return false;
                }
                else if(ValheimLegends.Ability1_Hotkey_Combo.Value == "")
                {
                    return Input.GetKeyDown(ValheimLegends.Ability1_Hotkey.Value.ToLower()) || Input.GetButtonDown(ValheimLegends.Ability1_Hotkey.Value.ToLower());
                }
                else if((Input.GetKeyDown(ValheimLegends.Ability1_Hotkey.Value.ToLower()) && Input.GetKey(ValheimLegends.Ability1_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetKey(ValheimLegends.Ability1_Hotkey.Value.ToLower()) && Input.GetKeyDown(ValheimLegends.Ability1_Hotkey_Combo.Value.ToLower())) || 
                    (Input.GetButtonDown(ValheimLegends.Ability1_Hotkey.Value.ToLower()) && Input.GetButton(ValheimLegends.Ability1_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetButton(ValheimLegends.Ability1_Hotkey.Value.ToLower()) && Input.GetButtonDown(ValheimLegends.Ability1_Hotkey_Combo.Value.ToLower())))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool Ability2_Input_Down
        {
            get
            {
                if (ValheimLegends.Ability2_Hotkey.Value == "")
                {
                    return false;
                }
                else if (ValheimLegends.Ability2_Hotkey_Combo.Value == "")
                {
                    return Input.GetKeyDown(ValheimLegends.Ability2_Hotkey.Value.ToLower()) || Input.GetButtonDown(ValheimLegends.Ability2_Hotkey.Value.ToLower());
                }
                else if ((Input.GetKeyDown(ValheimLegends.Ability2_Hotkey.Value.ToLower()) && Input.GetKey(ValheimLegends.Ability2_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetKey(ValheimLegends.Ability2_Hotkey.Value.ToLower()) && Input.GetKeyDown(ValheimLegends.Ability2_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetButtonDown(ValheimLegends.Ability2_Hotkey.Value.ToLower()) && Input.GetButton(ValheimLegends.Ability2_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetButton(ValheimLegends.Ability2_Hotkey.Value.ToLower()) && Input.GetButtonDown(ValheimLegends.Ability2_Hotkey_Combo.Value.ToLower())))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool Ability3_Input_Down
        {
            get
            {
                if (ValheimLegends.Ability3_Hotkey.Value == "")
                {
                    return false;
                }
                else if (ValheimLegends.Ability3_Hotkey_Combo.Value == "")
                {
                    return Input.GetKeyDown(ValheimLegends.Ability3_Hotkey.Value.ToLower()) || Input.GetButtonDown(ValheimLegends.Ability3_Hotkey.Value.ToLower());
                }
                else if ((Input.GetKeyDown(ValheimLegends.Ability3_Hotkey.Value.ToLower()) && Input.GetKey(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetKey(ValheimLegends.Ability3_Hotkey.Value.ToLower()) && Input.GetKeyDown(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetButtonDown(ValheimLegends.Ability3_Hotkey.Value.ToLower()) && Input.GetButton(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower())) ||
                    (Input.GetButton(ValheimLegends.Ability3_Hotkey.Value.ToLower()) && Input.GetButtonDown(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower())))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool Ability3_Input_Pressed
        {
            get
            {
                if (ValheimLegends.Ability3_Hotkey.Value == "")
                {
                    return false;
                }
                else if (ValheimLegends.Ability3_Hotkey_Combo.Value == "")
                {
                    return Input.GetKey(ValheimLegends.Ability3_Hotkey.Value.ToLower()) || Input.GetButton(ValheimLegends.Ability3_Hotkey.Value.ToLower());
                }
                else if (Input.GetKey(ValheimLegends.Ability3_Hotkey.Value.ToLower()) && Input.GetKey(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower()) ||
                    Input.GetButton(ValheimLegends.Ability3_Hotkey.Value.ToLower()) && Input.GetButton(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower()))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool Ability3_Input_Up
        {
            get
            {
                if (ValheimLegends.Ability3_Hotkey.Value == "")
                {
                    return false;
                }
                else if (ValheimLegends.Ability3_Hotkey_Combo.Value == "")
                {
                    return Input.GetKeyUp(ValheimLegends.Ability3_Hotkey.Value.ToLower()) || Input.GetButtonUp(ValheimLegends.Ability3_Hotkey.Value.ToLower());
                }
                else if (Input.GetKeyUp(ValheimLegends.Ability3_Hotkey.Value.ToLower()) || Input.GetKeyUp(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower()) ||
                    Input.GetButtonUp(ValheimLegends.Ability3_Hotkey.Value.ToLower()) || Input.GetButtonUp(ValheimLegends.Ability3_Hotkey_Combo.Value.ToLower()))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
