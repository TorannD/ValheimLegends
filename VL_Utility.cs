using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;

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
            byte[] data = File.ReadAllBytes(Path.Combine(Folder, "VL_assets", path));
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

        /// 
        //MAGE
        ///

        public static float GetFireballCost
        {
            get
            {
                return 70f;
            }
        }
        public static float GetFireballCooldownTime
        {
            get
            {
                return 5f;
            }
        }
        public static float GetFireballSkillGain
        {
            get
            {
                return .6f;
            }
        }
        public static float GetMeteorCost
        {
            get
            {
                return 40f;
            }
        }
        public static float GetMeteorCostPerUpdate
        {
            get
            {
                return .5f;
            }
        }
        public static float GetMeteorCooldownTime
        {
            get
            {
                return 300f;
            }
        }
        public static float GetMeteorSkillGain
        {
            get
            {
                return 1.6f;
            }
        }
        public static float GetFrostNovaCost
        {
            get
            {
                return 40f;
            }
        }
        public static float GetFrostNovaCooldownTime
        {
            get
            {
                return 30f;
            }
        }
        public static float GetFrostNovaSkillGain
        {
            get
            {
                return .75f;
            }
        }

        /// 
        //VALKYRIE
        ///

        public static float GetBulwarkCost
        {
            get
            {
                return 60f;
            }
        }
        public static float GetBulwarkCooldownTime
        {
            get
            {
                return 120f;
            }
        }
        public static float GetBulwarkSkillGain
        {
            get
            {
                return 1.5f;
            }
        }
        public static float GetLeapCost
        {
            get
            {
                return 30f;
            }
        }
        public static float GetLeapCooldownTime
        {
            get
            {
                return 15f;
            }
        }
        public static float GetLeapSkillGain
        {
            get
            {
                return .4f;
            }
        }
        public static float GetStaggerCost
        {
            get
            {
                return 40f;
            }
        }
        public static float GetStaggerCooldownTime
        {
            get
            {
                return 20f;
            }
        }
        public static float GetStaggerSkillGain
        {
            get
            {
                return .6f;
            }
        }

        /// 
        //DRUID
        ///

        public static float GetRegenerationCost
        {
            get
            {
                return 60f;
            }
        }
        public static float GetRegenerationCooldownTime
        {
            get
            {
                return 180f;
            }
        }
        public static float GetRegenerationSkillGain
        {
            get
            {
                return 2f;
            }
        }
        public static float GetRootCost
        {
            get
            {
                return 30f;
            }
        }
        public static float GetRootCostPerUpdate
        {
            get
            {
                return .25f;
            }
        }
        public static float GetRootCooldownTime
        {
            get
            {
                return 60f;
            }
        }
        public static float GetRootSkillGain
        {
            get
            {
                return .6f;
            }
        }
        public static float GetDefenderCost
        {
            get
            {
                return 60f;
            }
        }
        public static float GetDefenderCooldownTime
        {
            get
            {
                return 120f;
            }
        }
        public static float GetDefenderSkillGain
        {
            get
            {
                return 1.5f;
            }
        }

        /// 
        //SHAMAN
        ///

        public static float GetEnrageCost(Player p)
        {
            float cost = 50f;
            return cost;
        }
        public static float GetEnrageCooldown(Player p)
        {
            float time = 180;
            return time;
        }
        public static float GetEnrageSkillGain(Player p)
        {
            float xp = 1.5f;
            return xp;
        }
        public static float GetSpiritBombCost(Player p)
        {
            float cost = 80f;
            return cost;
        }
        public static float GetSpiritBombCooldown(Player p)
        {
            float time = 60;
            return time;
        }
        public static float GetSpiritBombSkillGain(Player p)
        {
            float xp = .9f;
            return xp;
        }
        public static float GetShellCost(Player p)
        {
            float cost = 100f;
            return cost;
        }
        public static float GetShellCooldown(Player p)
        {
            float time = 120;
            return time;
        }
        public static float GetShellSkillGain(Player p)
        {
            float xp = 1.7f;
            return xp;
        }

        /// 
        //BERSERKER
        ///

        public static float GetDashCost(Player p)
        {
            float cost = 70f;
            return cost;
        }
        public static float GetDashCooldown(Player p)
        {
            float time = 10;
            return time;
        }
        public static float GetDashSkillGain (Player p)
        {
            float xp = .45f;
            return xp;
        }
        public static float GetBerserkCost(Player p)
        {
            float cost = 50f;
            return cost;
        }
        public static float GetBerserkCooldown(Player p)
        {
            float time = 180;
            return time;
        }
        public static float GetBerserkSkillGain(Player p)
        {
            float xp = 1.8f;
            return xp;
        }
        public static float GetExecuteCost(Player p)
        {
            float cost = 50f;
            return cost;
        }
        public static float GetExecuteCooldown(Player p)
        {
            float time = 120;
            return time;
        }
        public static float GetExecuteSkillGain(Player p)
        {
            float xp = 1.2f;
            return xp;
        }

        /// 
        //RANGER
        ///

        public static float GetPowerShotCost(Player p)
        {
            float cost = 60f;
            return cost;
        }
        public static float GetPowerShotCooldown(Player p)
        {
            float time = 60;
            return time;
        }
        public static float GetPowerShotSkillGain(Player p)
        {
            float xp = .9f;
            return xp;
        }
        public static float GetShadowStalkCost(Player p)
        {
            float cost = 40f;
            return cost;
        }
        public static float GetShadowStalkCooldown(Player p)
        {
            float time = 120;
            return time;
        }
        public static float GetShadowStalkSkillGain(Player p)
        {
            float xp = 1.4f;
            return xp;
        }
        public static float GetSummonWolfCost(Player p)
        {
            float cost = 75f;
            return cost;
        }
        public static float GetSummonWolfCooldown(Player p)
        {
            float time = 900;
            return time;
        }
        public static float GetSummonWolfSkillGain(Player p)
        {
            float xp = 3.5f;
            return xp;
        }

    }
}
