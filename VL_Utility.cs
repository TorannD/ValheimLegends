using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;
using UnityEngine.UI;
using TMPro;

namespace ValheimLegends
{
    public static class VL_Utility
    {

        //shared
        public static string ModID;
        public static string Folder;
        private static int m_interactMask = LayerMask.GetMask("item", "piece", "piece_nonsolid", "Default", "static_solid", "Default_small", "character", "character_net", "terrain", "vehicle");
        private static int m_LOSMask = LayerMask.GetMask("piece", "piece_nonsolid", "Default", "static_solid", "Default_small", "terrain", "vehicle");

        public static string GetModDataPath(this PlayerProfile profile)
        {
            return Path.Combine(Utils.GetSaveDataPath(FileHelpers.FileSource.Local), "ModData", ModID, "char_" + profile.GetFilename());
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
            try
            {
                byte[] data = File.ReadAllBytes(Path.Combine(Folder, "VLAssets", path));
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(data);
                return texture2D;
            }
            catch
            {
                byte[] data = File.ReadAllBytes(Path.Combine(Folder, path));
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(data);
                return texture2D;
            }
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
                float xMod = (float)(Screen.width / 1920f);
                float yMod = (float)(Screen.height / 1080f);
                float xStep = 80f * xMod;                
                float yStep = 0f;
                float yOffset = (106f * yMod) + ValheimLegends.icon_Y_Offset.Value;
                float xOffset = (209f * xMod) + ValheimLegends.icon_X_Offset.Value;
                if(ValheimLegends.iconAlignment.Value.ToLower() == "vertical")
                {
                    xStep = 0f; 
                    yStep = 100f * yMod;
                }
                ValheimLegends.abilitiesStatus = new List<RectTransform>();
                ValheimLegends.abilitiesStatus.Clear();
                Vector3 pos = new Vector3(xOffset + xStep, yOffset + yStep, 0);
                Quaternion rot = new Quaternion(0, 0, 0, 1);
                Transform t = hud.m_statusEffectListRoot;
                RectTransform rectTransform = UnityEngine.Object.Instantiate(hud.m_statusEffectTemplate, pos, rot, t);
                rectTransform.gameObject.SetActive(true);
                rectTransform.GetComponentInChildren<TMP_Text>().text = Localization.instance.Localize((ValheimLegends.Ability1_Name).ToString());
                ValheimLegends.abilitiesStatus.Add(rectTransform);
                pos.x += xStep;
                pos.y += yStep;
                RectTransform rectTransform2 = UnityEngine.Object.Instantiate(hud.m_statusEffectTemplate, pos, rot, t);
                rectTransform2.gameObject.SetActive(value: true);
                rectTransform2.GetComponentInChildren<TMP_Text>().text = Localization.instance.Localize((ValheimLegends.Ability2_Name).ToString());
                ValheimLegends.abilitiesStatus.Add(rectTransform2);
                pos.x += xStep;
                pos.y += yStep;
                RectTransform rectTransform3 = UnityEngine.Object.Instantiate(hud.m_statusEffectTemplate, pos, rot, t);
                rectTransform3.gameObject.SetActive(value: true);
                rectTransform3.GetComponentInChildren<TMP_Text>().text = Localization.instance.Localize((ValheimLegends.Ability3_Name).ToString());
                ValheimLegends.abilitiesStatus.Add(rectTransform3);
            }
        }

        public static void RotatePlayerToTarget(Player p)
        {
            Vector3 lookVec = p.GetLookDir();
            lookVec.y = 0f;
            p.transform.rotation = Quaternion.LookRotation(lookVec);
        }

        public static bool LOS_IsValid (Character hit_char, Vector3 splash_center, Vector3 splash_alternate = default(Vector3))
        {
            bool los = false;
            if(VL_GlobalConfigs.ConfigStrings["vl_svr_aoeRequiresLoS"] == 0)
            {
                return true;
            }
            if(splash_alternate == default(Vector3))
            {
                splash_alternate = splash_center + new Vector3(0f, .2f, 0f); //y was .2f
            }
            if(hit_char != null)
            {
                //float distanceToBlast = (hit_char.GetCenterPoint() - splash_center).magnitude;
                //Collider char_col = hit_char.GetCollider();
                //ZLog.Log("checking los to " + hit_char.m_name);
                //Vector3 col_pos = char_col.ClosestPoint(splash_center);
                RaycastHit hitInfo = default(RaycastHit);
                var rayDirection = hit_char.GetCenterPoint() - splash_center;
                if(Physics.Raycast(splash_center, rayDirection, out hitInfo))
                {
                    if(CollidedWithTarget(hit_char, hit_char.GetCollider(), hitInfo))
                    {
                        //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_ice_hit"), hitInfo.point, Quaternion.identity);
                        //ZLog.Log("has direct line of sight to splash");
                        los = true;
                    }
                    else
                    {
                        for(int i = 0; i < 8; i++)
                        {
                            Vector3 char_col_size = hit_char.GetCollider().bounds.size;
                            //ZLog.Log("character collider size is " + char_col_size);
                            var rayDirectionMod = (hit_char.GetCenterPoint() + new Vector3(char_col_size.x * (UnityEngine.Random.Range(-i, i) / 6f), char_col_size.y * (UnityEngine.Random.Range(-i, i) / 4f), char_col_size.z * (UnityEngine.Random.Range(-i, i) / 6f))) - splash_center;
                            if (Physics.Raycast(splash_center, rayDirectionMod, out hitInfo))
                            {
                                //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_ice_hit"), hitInfo.point, Quaternion.identity);
                                if (CollidedWithTarget(hit_char, hit_char.GetCollider(), hitInfo))
                                {
                                    //ZLog.Log("has mod line of sight to splash center, iteration " + i);
                                    los = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if(!los && splash_alternate != default(Vector3) && splash_alternate != splash_center)
                {
                    var rayDirectionAlt = hit_char.GetCenterPoint() - splash_alternate;
                    if (Physics.Raycast(splash_alternate, rayDirectionAlt, out hitInfo))
                    {
                        if (CollidedWithTarget(hit_char, hit_char.GetCollider(), hitInfo))
                        {
                            //UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_ice_hit"), hitInfo.point, Quaternion.identity);
                            //ZLog.Log("has direct line of sight to splash alternate");
                            los = true;
                        }
                        else
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                Vector3 char_col_size = hit_char.GetCollider().bounds.size;
                                var rayDirectionMod = (hit_char.GetCenterPoint() + new Vector3(char_col_size.x * (UnityEngine.Random.Range(-i, i) / 6f), char_col_size.y * (UnityEngine.Random.Range(-i, i) / 4f), char_col_size.z * (UnityEngine.Random.Range(-i, i) / 6f))) - splash_alternate;
                                if (Physics.Raycast(splash_alternate, rayDirectionMod, out hitInfo))
                                {
                                    if (CollidedWithTarget(hit_char, hit_char.GetCollider(), hitInfo))
                                    {
                                        //ZLog.Log("has mod line of sight to splash alternate, iteration " + i);
                                        los = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return los;
        }

        private static bool CollidedWithTarget(Character chr, Collider col, RaycastHit hit)
        {
            //ZLog.Log("chr: " + chr + " chr.collider: " + chr.GetCollider() + " hit.collider: " + hit.collider);
            if (hit.collider == chr.GetCollider())
            {
                //ZLog.Log("returning collider direct");
                return true;
            }
            Character ch = null;
            hit.collider.gameObject.TryGetComponent<Character>(out ch);
            bool flag = ch != null;
            List<Component> comps = new List<Component>();
            comps.Clear();
            hit.collider.gameObject.GetComponents<Component>(comps);
            if (ch == null)
            {
                ch = (Character)hit.collider.GetComponentInParent(typeof(Character));
                flag = ch != null;
                if (ch == null)
                {
                    ch = (Character)hit.collider.GetComponentInChildren<Character>();
                    flag = ch != null;
                }
            }
            if(flag && ch == chr)
            {
                //ZLog.Log("returning collider as same char");
                return true;
            }
            //ZLog.Log("did not find collision with character");
            return false;
        }

        public static void FindCrosshairObject(Player p, Vector3 originEyePoint, float maxDistance, out GameObject hover, out Character hoverCreature)
        {
            hover = null;
            hoverCreature = null;
            RaycastHit[] array = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, 50f, m_interactMask);
            Array.Sort(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
            RaycastHit[] array2 = array;
            int num = 0;
            RaycastHit raycastHit;
            while (true)
            {
                if (num >= array2.Length)
                {
                    return;
                }
                raycastHit = array2[num];
                if (!(bool)raycastHit.collider.attachedRigidbody || !(raycastHit.collider.attachedRigidbody.gameObject == p.gameObject))
                {
                    break;
                }
                num++;
            }
            if (hoverCreature == null)
            {
                Character character = ((bool)raycastHit.collider.attachedRigidbody) ? raycastHit.collider.attachedRigidbody.GetComponent<Character>() : raycastHit.collider.GetComponent<Character>();
                if (character != null)
                {
                    hoverCreature = character;
                }
            }
            if (Vector3.Distance(originEyePoint, raycastHit.point) < maxDistance)
            {
                if (raycastHit.collider.GetComponent<Hoverable>() != null)
                {
                    hover = raycastHit.collider.gameObject;
                }
                else if ((bool)raycastHit.collider.attachedRigidbody)
                {
                    hover = raycastHit.collider.attachedRigidbody.gameObject;
                }
                else
                {
                    hover = raycastHit.collider.gameObject;
                }
            }
        }

        /// 
        //Enchanter
        ///

        public static float GetZoneChargeCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetZoneChargeCooldownTime
        {
            get
            {
                return 180f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetZoneChargeCostPerUpdate
        {
            get
            {
                return 1f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetZoneChargeSkillGain
        {
            get
            {
                return 2f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetWeakenCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetWeakenCooldownTime
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetWeakenSkillGain
        {
            get
            {
                return .5f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetCharmCost
        {
            get
            {
                return 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetCharmCooldownTime
        {
            get
            {
                return 60f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetCharmSkillGain
        {
            get
            {
                return .85f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //Monk
        ///

        public static float GetMeteorPunchCost
        {
            get
            {
                return 3f;
            }
        }
        public static float GetMeteorPunchCooldownTime
        {
            get
            {
                return 1f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetMeteorPunchSkillGain
        {
            get
            {
                return .15f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetPsiBoltCost
        {
            get
            {
                return 5f;
            }
        }
        public static float GetPsiBoltCooldownTime
        {
            get
            {
                return 1f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetPsiBoltSkillGain
        {
            get
            {
                return .05f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetFlyingKickCost
        {
            get
            {
                return 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetFlyingKickCooldownTime
        {
            get
            {
                return 6f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetFlyingKickSkillGain
        {
            get
            {
                return .3f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //Rogue
        ///

        public static float GetPoisonBombCost
        {
            get
            {
                return 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetPoisonBombCooldownTime
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetPoisonBombSkillGain
        {
            get
            {
                return .6f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetBackstabCost
        {
            get
            {
                return 60f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetBackstabCooldownTime
        {
            get
            {
                return 20f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetBackstabSkillGain
        {
            get
            {
                return .55f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetFadeCost
        {
            get
            {
                return 10f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetFadeCooldownTime
        {
            get
            {
                return 15f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetFadeSkillGain
        {
            get
            {
                return .2f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //Priest
        ///

        public static float GetSanctifyCost
        {
            get
            {
                return 70f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetSanctifyCooldownTime
        {
            get
            {
                return 45f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetSanctifySkillGain
        {
            get
            {
                return .8f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetHealCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetHealCostPerUpdate
        {
            get
            {
                return .75f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetHealCooldownTime
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetHealSkillGain
        {
            get
            {
                return .4f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetPurgeCost
        {
            get
            {
                return 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetPurgeCooldownTime
        {
            get
            {
                return 15f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetPurgeSkillGain
        {
            get
            {
                return .4f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //Duelist
        ///

        public static float GetQuickShotCost
        {
            get
            {
                return 25f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetQuickShotCooldownTime
        {
            get
            {
                return 10f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetQuickShotSkillGain
        {
            get
            {
                return .25f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetRiposteCost
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetRiposteCooldownTime
        {
            get
            {
                return 6f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetRiposteSkillGain
        {
            get
            {
                return .15f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetBlinkStrikeCost
        {
            get
            {
                return 60f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetBlinkStrikeCooldownTime
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetBlinkStrikeSkillGain
        {
            get
            {
                return .6f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //Metavoker
        ///

        public static float GetLightCost
        {
            get
            {
                return 50f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetLightCooldownTime
        {
            get
            {
                return 20f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetLightSkillGain
        {
            get
            {
                return .75f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetWarpCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetWarpCostPerUpdate
        {
            get
            {
                return 1f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetWarpCooldownTime
        {
            get
            {
                return 6f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetWarpSkillGain
        {
            get
            {
                return .5f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetReplicaCost
        {
            get
            {
                return 70f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetReplicaCooldownTime
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetReplicaSkillGain
        {
            get
            {
                return .75f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetForceWaveCost
        {
            get
            {
                return 30f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
        public static float GetForceWaveSkillGain
        {
            get
            {
                return .4f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetForceWaveCooldown
        {
            get
            {
                return 20f * VL_GlobalConfigs.g_CooldownModifer;
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
                return 60f * VL_GlobalConfigs.g_EnergyCostModifer;
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
                return 20f * VL_GlobalConfigs.g_CooldownModifer;
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
                return 60f * VL_GlobalConfigs.g_CooldownModifer;
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
        public static float GetHarpoonPullCost
        {
            get
            {
                return 1f;
            }
        }
        public static float GetHarpoonPullSkillGain
        {
            get
            {
                return .25f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }
        public static float GetHarpoonPullCooldown
        {
            get
            {
                return 10f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetShieldReleaseSkillGain
        {
            get
            {
                return .25f * VL_GlobalConfigs.g_SkillGainModifer;
            }
        }

        /// 
        //DRUID
        ///

        public static float GetVineHookCost
        {
            get
            {
                return 40f * VL_GlobalConfigs.g_EnergyCostModifer;
            }
        }
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
                return 60f * VL_GlobalConfigs.g_CooldownModifer;
            }
        }
        public static float GetRegenerationSkillGain
        {
            get
            {
                return 1f * VL_GlobalConfigs.g_SkillGainModifer;
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
            float time = 60 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetEnrageSkillGain(Player p)
        {
            float xp = 1.2f * VL_GlobalConfigs.g_SkillGainModifer;
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
            float time = 120 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetShellSkillGain(Player p)
        {
            float xp = 1.6f * VL_GlobalConfigs.g_SkillGainModifer;
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
            float cost = 0f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetBerserkCooldown(Player p)
        {
            float time = 60 * VL_GlobalConfigs.g_CooldownModifer;
            return time;
        }
        public static float GetBerserkSkillGain(Player p)
        {
            float xp = 1.2f * VL_GlobalConfigs.g_SkillGainModifer;
            return xp;
        }
        public static float GetExecuteCost(Player p)
        {
            float cost = 60f * VL_GlobalConfigs.g_EnergyCostModifer;
            return cost;
        }
        public static float GetExecuteCooldown(Player p)
        {
            float time = 60 * VL_GlobalConfigs.g_CooldownModifer;
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
            float time = 45 * VL_GlobalConfigs.g_CooldownModifer;
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

        private static float vl_timer;
        public static void SetTimer()
        {
            vl_timer = Time.time;            
        }

        public static bool ReadyTime
        {
            get
            {
                return Time.time > (.01f + vl_timer);
            }
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
