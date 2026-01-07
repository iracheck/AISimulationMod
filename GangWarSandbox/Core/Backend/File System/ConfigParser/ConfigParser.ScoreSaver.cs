using GangWarSandbox.Core;
using GangWarSandbox.Utilities;
using GTA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GangWarSandbox.Core
{
    static partial class ConfigParser
    {
        public static void LoadScores()
        {
            string[] validKeys = { "MenuKeybind", "SquadUpdateFreq", "VehicleUpdateFreq", "MaxCorpses", "MaxSquadlessVehicles", "DebugMode", "AIAttackRadius" };
            Logger.Parser("Loading mod config file...");
            try
            {
                string path = ModFiles.CONFIG_PATH;
                if (!File.Exists(path))
                {
                    SendWarningDoesNotExist();
                    return;
                }
                var lines = File.ReadLines(path);

                if (lines == null || lines.Count() == 0)
                {
                    SendWarningDoesNotExist();
                }

                foreach (var line in lines)
                {
                    var thisLine = PurgeComments(line);

                    int equalsIndex = thisLine.IndexOf('=');
                    if (equalsIndex == -1 || equalsIndex == thisLine.Length - 1) continue; // skip invalid lines

                    string key = thisLine.Substring(0, equalsIndex).Trim();
                    string value = thisLine.Substring(equalsIndex + 1).Trim();

                    bool success = false;

                    switch (key)
                    {
                        case "MenuKeybind":
                            success = GWSettings.SetOpenMenuKeybind(value);
                            break;
                        case "SquadUpdateFreq":
                            success = int.TryParse(value, out GWSettings.AI_UPDATE_FREQUENCY);
                            break;
                        case "VehicleUpdateFreq":
                            success = int.TryParse(value, out GWSettings.VEHICLE_AI_UPDATE_FREQUENCY);
                            break;
                        case "MaxCorpses":
                            success = int.TryParse(value, out GWSettings.MAX_CORPSES);
                            break;
                        case "MaxSquadlessVehicles":
                            success = int.TryParse(value, out GWSettings.MAX_SQUADLESS_VEHICLES);
                            break;
                        case "DebugMode":
                            success = bool.TryParse(value, out GWSettings.DEBUG);
                            break;
                        case "AIAttackRadius":
                            success = float.TryParse(value, out GWSettings.AI_ATTACK_RADIUS);
                            break;
                        default:
                            success = false;
                            break;
                    }

                    if (success) continue;

                    Logger.ParserError("Error with parsing " + key + " in Configuration.ini." + (!validKeys.Contains(key) ? "Unrecognized key." : "Invalid value \"" + value + "\" for key.") );
                }
            }
            catch (Exception e)
            {
                Logger.ParserError("Failed to parse mod configuration file. Error: " + e.ToString());
            }
        }

    }
}
