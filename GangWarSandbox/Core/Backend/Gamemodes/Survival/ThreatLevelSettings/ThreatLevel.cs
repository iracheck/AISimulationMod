using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Gamemodes.Survival
{
    public sealed class ThreatLevel
    {
        public int MaxTotalSquads { get; private set; }
        public int VehicleSquads { get; private set; }
        public int WeaponizedVehicleSquads { get; private set; }
        public int HelicopterSquads { get; private set; }
        public int MaxFactionTier { get; private set; }
        public int ThreatWeight { get; private set; }

        public ThreatLevel(int maxSquads, int numVehicleSquads, int numWepVehicleSquads, int numHelicopterSquads, int maxTier, int threatWeight)
        {
            MaxTotalSquads = maxSquads;
            VehicleSquads = numVehicleSquads;
            WeaponizedVehicleSquads = numWepVehicleSquads;
            HelicopterSquads = numHelicopterSquads;
            MaxFactionTier = maxTier;
            ThreatWeight = threatWeight;
        }
    }
}
