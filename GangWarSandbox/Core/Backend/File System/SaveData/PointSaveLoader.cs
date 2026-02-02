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
    public class PointSaveLoader
    {
        public List<PointSaveData> SavedData = new List<PointSaveData>();
        const string savePath = ModFiles.SAVEDATA_PATH + "/InfiniteBattle/";

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

        /// <summary>
        /// Saves the given data to a file, and if no data is provided then it will save the current world format.
        /// </summary>
        /// <param name="data"></param>
        public void SaveToFile(string name = null, PointSaveData data = null)
        {
            if (data == null) data = CreateDataFromWorld();
            if (name == null) name = "unnamed_save_" + (SavedData.Count + 1).ToString();

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
