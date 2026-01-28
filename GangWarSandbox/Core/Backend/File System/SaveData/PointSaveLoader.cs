using GangWarSandbox.Utilities;
using GTA.Math;
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
        string savePath = ModFiles.SAVEDATA_PATH + "/InfiniteBattle/";
        List<PointSaveData> SavedData;

        GangWarSandbox Instance = GangWarSandbox.Instance;

        /// <summary>
        /// Create the save data using the information currently within the world space. This is how its going to be used most of the time.
        /// </summary>
        /// <returns></returns>
        private PointSaveData CreateDataFromWorld()
        {
            PointSaveData data = new PointSaveData();

            foreach (var capturePoint in Instance.CapturePoints)
            {
                if (capturePoint != null) data.CapturePoints.Add(capturePoint.Position);
            }

            foreach (var t in Instance.Teams)
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
        }

        public void InitializeIntoWorld(PointSaveData data)
        {
            foreach (var capturePoint in data.CapturePoints)
            {
                CapturePoint point = new CapturePoint(capturePoint);

                Instance.CapturePoints.Add(point);
            }

            foreach (var teamIndex in data.SpawnPoints.Keys)
            {
                foreach (var point in data.SpawnPoints[teamIndex])
                {
                    Instance.Teams[teamIndex].AddSpawnpoint(point);
                }
            }
        }
    }
}
