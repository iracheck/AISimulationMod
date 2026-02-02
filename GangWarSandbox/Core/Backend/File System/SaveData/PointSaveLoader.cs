using GangWarSandbox.MapElements;
using GangWarSandbox.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Core.Backend.File_System.SaveData
{
    internal class PointSaveLoader
    {
        public List<PointSaveData> SavedData = new List<PointSaveData>();
        public static PointSaveLoader Instance { get; } = new PointSaveLoader();
        const string savePath = ModFiles.SAVEDATA_PATH + "/InfiniteBattle/";

        //
        GangWarSandbox Mod = GangWarSandbox.Instance;

        //

        public PointSaveData CreateDataFromWorld()
        {
            PointSaveData data = new PointSaveData();

            foreach (var capturePoint in MapElementManager.CapturePoints)
            {
                if (capturePoint != null) data.CapturePoints.Add(capturePoint.Position);
            }

            foreach (var t in Mod.Teams)
            {
                data.Teams.Add(t.Name);

                foreach (var pt in t.SpawnPoints)
                {
                    data.SpawnPoints[t.TeamIndex].Add(pt);
                }
            }

            return data;
        }

        public void SaveFromCurrent(PointSaveData data = null)
        {
            if (data == null) data = CreateDataFromWorld();

            try
            {
                string serializedData = JsonConvert.SerializeObject(data);
                File.WriteAllText(savePath + data.Name, serializedData);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                NotificationHandler.Send("Point save data failed to serialize. Your data was not saved.");
            }
            
        }

        public void Load(string filePath)
        {
            SavedData = JsonConvert.DeserializeObject<List<PointSaveData>>(filePath);

            // set teams
            
            // place spawnpoints

            // place capturepoints
        }
    }
}
