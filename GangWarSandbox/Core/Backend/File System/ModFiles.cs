using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Core
{
    static class ModFiles
    {
        public const string MOD_PATH = "scripts/GangWarSandbox";
        public const string CONFIG_PATH = "scripts/GangWarSandbox/Configuration.ini";
        public const string VEHICLESET_PATH = MOD_PATH + "/VehicleSets";
        public const string FACTIONS_PATH = MOD_PATH + "/Factions";
        public const string SAVEDATA_PATH = MOD_PATH + "/SaveData";

        //public const string SURVIVAL_SAVE_PATH = SAVEDATA_PATH + "/survival.txt";
        public const string LOG_FILE_PATH = "scripts/GangWarSandbox/GWS.log"; // Path to the log file

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(MOD_PATH);
            Directory.CreateDirectory(VEHICLESET_PATH);
            Directory.CreateDirectory(FACTIONS_PATH);
            Directory.CreateDirectory(SAVEDATA_PATH);

            File.Create(LOG_FILE_PATH).Close(); // Ensure the log file exists

            // Ensure the log file is empty at mod start
            if (File.Exists(LOG_FILE_PATH))
            {
                File.WriteAllText(LOG_FILE_PATH, string.Empty); // Clear the log file
            }
        }

    }
}
