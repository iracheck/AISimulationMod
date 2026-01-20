using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// json
using Newtonsoft.Json;

namespace GangWarSandbox.Core.Backend.File_System.SaveData
{
    internal class PointSaveData
    {
        public string Name = "";

        public Dictionary<int, List<Vector3>> SpawnPoints = new Dictionary<int, List<Vector3>>();
        public List<Vector3> CapturePoints = new List<Vector3>();
    }
}
