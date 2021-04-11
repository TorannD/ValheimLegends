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
                }

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

                    if (numLines == 0)
                    {
                        ZLog.LogWarning("Got zero line config file from server. Cannot load.");
                        return;
                    }

                    char[] trm = { ' ', '=' };

                    for (int i = 0; i < numLines; i++)
                    {
                        string line = configPkg.ReadString();
                        //ZLog.Log("reading line: " + line);
                        string key = line.Substring(0, line.IndexOf('=') + 1);  //line.Substring(0, line.IndexOf('=') + 1);    
                        key = key.Trim(trm);
                        //ZLog.Log("key string is " + key);
                        if (VL_GlobalConfigs.ConfigStrings.ContainsKey(key))
                        {
                            //ZLog.Log("found match: ");
                            string val = line.Substring(line.IndexOf('=') + 1);
                            val = val.Trim(trm);
                            //ZLog.Log("value is: " + val + " parsed to " + float.Parse(val));
                            VL_GlobalConfigs.ConfigStrings[key] = float.Parse(val);
                            //ZLog.Log("config value is " + VL_GlobalConfigs.ConfigStrings[key]);
                        }          
                    }

                    ZLog.Log("Valheim Legends configurations synced to server.");
                }
            }
        }
    }
}