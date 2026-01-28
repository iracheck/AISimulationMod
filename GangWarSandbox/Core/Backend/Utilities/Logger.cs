using GangWarSandbox.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Utilities
{
    static class Logger
    {
        public static void Log(String data, String logType = "LOG", bool inGameNotification = false)
        {
            if (!File.Exists(ModFiles.LOG_FILE_PATH))
            {
                File.Create(ModFiles.LOG_FILE_PATH).Close(); // Create the file if it doesn't exist
            }

            File.AppendAllText(ModFiles.LOG_FILE_PATH, $"[{logType}] {data}\n");

            if (inGameNotification)
            {
                NotificationHandler.Send($"[{logType}]\n {data}");
            }
        }

        public static void LogError(String data, bool inGameNotification = false)
        {
            Log(data, "ERROR", inGameNotification);
        }

        public static void LogDebug(String data)
        {
            if (GWSettings.DEBUG)
            {
                NotificationHandler.Send("An error message was sent to log.");
                Log(data, "DEBUG");
            }
                
        }

        public static void Parser(String data, bool inGameNotification = false)
        {
            Log(data, "PARSER", inGameNotification);
        }

        public static void ParserError(String data, bool inGameNotification = false)
        {
            Log(data, "ERROR_PARSER", inGameNotification);
        }
    }
}
