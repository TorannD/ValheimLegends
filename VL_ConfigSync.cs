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
        public static string ConfigPath = Path.GetDirectoryName(Paths.BepInExConfigPath) + Path.DirectorySeparatorChar + "ValheimLegends.cfg";


        public static void RPC_VL_ConfigSync(long sender, ZPackage configPkg)
        {
            if (ZNet.instance.IsServer()) //Server
            {
                ZPackage pkg = new ZPackage();
                //ZLog.Log("VL SERVER -------------- Sending client #" + sender + " server configs");
                string[] rawConfigData = File.ReadAllLines(ConfigPath);
                List<string> cleanConfigData = new List<string>();

                for (int i = 0; i < rawConfigData.Length; i++)
                {
                    //if (rawConfigData[i].Trim().StartsWith(";") ||
                    //    rawConfigData[i].Trim().StartsWith("#")) continue; //Skip comments
                    //if (rawConfigData[i].Trim().IsNullOrWhiteSpace()) continue; //Skip blank lines
                    if (!rawConfigData[i].Trim().StartsWith("vl_svr_")) continue; //Skip local lines

                    //Add to clean data
                    cleanConfigData.Add(rawConfigData[i]);
                    //ZLog.Log("VL SERVER -------------- sending config: " + rawConfigData[i]);
                }

                cleanConfigData.Add("vl_svr_version = " + ValheimLegends.Version);
                //ZLog.Log("VL SERVER -------------- sending config: " + "vl_svr_version = " + ValheimLegends.VersionF);
                //Add number of clean lines to package
                pkg.Write(cleanConfigData.Count);

                //Add each line to the package
                foreach (string line in cleanConfigData)
                {
                    pkg.Write(line);
                }

                ZRoutedRpc.instance.InvokeRoutedRPC(sender, "VL_ConfigSync", new object[]
                {
                    pkg
                });

                ZLog.Log("Valheim Legends server configurations synced to peer #" + sender);
            }
            else //Client
            {
                if (configPkg != null &&
                    configPkg.Size() > 0 &&
                    sender == ValheimLegends.ServerID)
                {
                    int numLines = configPkg.ReadInt();
                    //ZLog.Log("VL CLIENT -------------- Receiving server configs from #" + sender);
                    if (numLines == 0)
                    {
                        ZLog.LogWarning("Got zero line config file from server. Cannot load.");
                        return;
                    }

                    char[] trm = { ' ', '=' };
                    bool syncOrVersionFailure = false;
                    for (int i = 0; i < numLines; i++)
                    {
                        string line = configPkg.ReadString();
                        //ZLog.Log("VL CLIENT -------------- Received line: " + line);
                        //ZLog.Log("reading line: " + line);
                        string key = line.Substring(0, line.IndexOf('=') + 1);  //line.Substring(0, line.IndexOf('=') + 1);    
                        key = key.Trim(trm);
                        //ZLog.Log("key string is " + key);
                        if (key == "vl_svr_version")
                        {
                            string val = line.Substring(line.IndexOf('=') + 1);
                            val = val.Trim(trm);
                            //ZLog.Log("val is " + val + " server evrsion is " + ValheimLegends.Version);
                            if (val != ValheimLegends.Version)
                            {
                                char[] trm_e = { '.', ',', '0' };
                                string val2 = val.Trim(trm_e);
                                string keyString = VL_GlobalConfigs.ConfigStrings[key].ToString();
                                string key2 = keyString.Trim(trm_e);
                                //ZLog.Log("VL CLIENT -------------- Initial version check FAILED --- Europeon localization check: server had version [" + val2 + "] and client had version [" + key2 + "]");
                                //if (val2 == key2)
                                //{
                                //    ZLog.Log("VL CLIENT -------------- version MATCH for european localization: server had version [" + val2 + "] and client had version [" + key2 + "] VL DLL constant set to [" + ValheimLegends.VersionF + "]");
                                //}
                                ZLog.Log("VL CLIENT -------------- version failure: server had version [" + val + "] and client had version [" + ValheimLegends.Version + "]");
                                syncOrVersionFailure = true;
                            }

                        }
                        else if (VL_GlobalConfigs.ConfigStrings.ContainsKey(key))
                        {
                            //ZLog.Log("VL CLIENT -------------- found config match for: " + key + " ----- changing running modifiers ");
                            string val = line.Substring(line.IndexOf('=') + 1);
                            val = val.Trim(trm);
                            if (key == "vl_svr_enforceConfigClass")
                            {
                                val = val.ToLower().ToString() == "true" ? "1" : "0";
                            }
                            else if(key == "vl_svr_aoeRequiresLoS")
                            {
                                val = val.ToLower().ToString() == "true" ? "1" : "0";
                            }
                            else if (key == "vl_svr_allowAltarClassChange")
                            {
                                val = val.ToLower().ToString() == "true" ? "1" : "0";
                            }
                            //ZLog.Log("value is: " + val + " parsed to " + float.Parse(val));
                            float val2 = 1f;
                            try
                            {
                                val2 = float.Parse(val);
                            }
                            catch
                            {
                                val = val.Replace(",", ".");
                            }
                            try
                            {
                                val2 = float.Parse(val);
                            }
                            catch
                            {
                                val = val.Replace(".", ",");
                            }
                            try
                            {
                                val2 = float.Parse(val);
                            }
                            catch
                            {
                                ZLog.Log("Valheim Legends: unable to sync modifiers - setting to default");
                                val2 = 1f;
                            }
                            VL_GlobalConfigs.ConfigStrings[key] = val2;
                            //ZLog.Log("config value is " + VL_GlobalConfigs.ConfigStrings[key]);
                        }
                    }
                    if (syncOrVersionFailure)
                    {
                        ZLog.LogWarning("Valheim Legends version mismatch; disabling.");
                        ValheimLegends.playerEnabled = false;
                        //ValheimLegends._Harmony.UnpatchSelf();
                    }
                    else
                    {
                        ZLog.Log("Valheim Legends configurations synced to server.");
                    }
                }
            }
        }
    }
}