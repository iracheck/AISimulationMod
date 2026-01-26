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
        string savePath = ModFiles.SAVEDATA_PATH + "/InfiniteBattle/";
        List<PointSaveData> SavedData;

        private PointSaveData CreateDataFromWorld()
        {
            PointSaveData data = new PointSaveData();

            foreach (var capturePoint in GangWarSandbox.Instance.CapturePoints)
            {
                if (capturePoint != null) data.CapturePoints.Add(capturePoint.Position);
            }
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
    }
}
