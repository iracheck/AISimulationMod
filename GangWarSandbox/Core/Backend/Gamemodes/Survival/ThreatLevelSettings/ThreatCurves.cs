using GangWarSandbox.Gamemodes.Survival;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangWarSandbox.Gamemodes.Survival
{

    public abstract class ThreatCurve
    {
        // the name that will be displayed in the UI
        public abstract string Name { get; }
        // the description that will be displayed in the UI
        public abstract string Description { get; }
        // the multiplier for all points gained
        public virtual float PointMultiplier { get; } = 1.0f;

        // Note that the max number of squads is a global value. This means that you could have 15 vehicle squads, 5 weaponized vehicles squads, but only 15 max squads, and 
        // it will be a mixture of those two types, but not more than 15 total squads.
        // Any left over slots will be filled with infantry squads, as the default squad type.
        public virtual List<ThreatLevel> ThreatLevels { get; } = new List<ThreatLevel>
        {
            new ThreatLevel(1,1,0,0,1,0)
        };

        public ThreatLevel Get(int level)
        {
            if (ThreatLevels.Count < level) return ThreatLevels.Last();
            return ThreatLevels[level];
        }

    }

    public class NormalCurve : ThreatCurve
    {
        public override string Name => "Normal";
        public override string Description => "A balanced curve where survival will gradually get harder over time, and there are no breaks. The game is trying to kill you.";
        public override List<ThreatLevel> ThreatLevels => new List<ThreatLevel>
        {
            // max squads(total) (0) - vehicles (1) - weaponized vehicles (2) - helicopters (3) - max faction tier[1-3] (4) - threat weight (5)
            new ThreatLevel(2,1,0,0,1,0), // 1
            new ThreatLevel(3,2,0,0,1,80), // 2
            new ThreatLevel(4,2,0,0,1,400), // 3
            new ThreatLevel(5,3,0,1,1,900), // 4
            new ThreatLevel(6,4,0,1,1,1800), // 5
            new ThreatLevel(6,4,0,1,2,2700), // 6
            new ThreatLevel(6,3,0,1,2,3900), // 7
            new ThreatLevel(7,3,1,1,2,5300), // 8
            new ThreatLevel(7,4,1,1,2,6900), // 9
            new ThreatLevel(8,3,1,1,3,8200), // 10
            new ThreatLevel(8,3,1,2,3,9500), // 11
            new ThreatLevel(9,3,1,2,3,11500), // 12
            new ThreatLevel(9,4,1,2,3,14000), // 13
            new ThreatLevel(10,4,1,2,3,18000), // 14
            new ThreatLevel(11,4,2,3,3,24000), //15
            new ThreatLevel(12,5,3,3,3,40000), // 16: Endgame
        };
    }

    public class EasyCurve : ThreatCurve
    {
        public override string Name => "Easy";
        public override string Description => "An easier curve where you will recieve breaks inbetween difficult rounds, intending to give you time to reposition or regain health. \n\nYou will gain only 85% of the score from this curve.";
        public override float PointMultiplier => 0.85f;
        public override List<ThreatLevel> ThreatLevels => new List<ThreatLevel>
        {
            new ThreatLevel(1,1,0,0,1,0), // 1
            new ThreatLevel(2,1,0,0,1,150), // 2
            new ThreatLevel(4,3,0,0,1,600), // 3
            new ThreatLevel(2,1,0,0,1,1200), // 4: Rest
            new ThreatLevel(4,3,0,0,1,1500), // 5 : Back to attack
            new ThreatLevel(5,3,0,0,1,2500), // 6
            new ThreatLevel(3,3,0,0,1,2500), // 7: Rest
            new ThreatLevel(6,3,0,1,1, 4000), // 8: Back to attack
            new ThreatLevel(6,3,0,1,2, 5700), // 9 
            new ThreatLevel(7,4,0,1,2, 7200), // 10
            new ThreatLevel(3,2,0,0,2, 8600), // 11: Rest 
            new ThreatLevel(7,4,0,1,2, 9200), // 12: Back to attack
            new ThreatLevel(8,4,0,1,2, 11000), // 13
            new ThreatLevel(7,5,1,0,3, 12500), // 14: Lowered spawns due to weaponized vehicle first appearance
            new ThreatLevel(8,4,1,1,3, 14000), // 15
            new ThreatLevel(9,4,1,1,3, 16000), // 16
            new ThreatLevel(5,2,0,1,3, 19000), // 17: Rest
            new ThreatLevel(9,4,0,1,3, 20000), // 18: Back to attack
            new ThreatLevel(9,4,1,2,3, 20500), // 19
            new ThreatLevel(10,4,1,2,3, 26000), // 20 "endgame"

        };
    }

    public class QuickplayCurve : ThreatCurve
    {
        public override string Name => "Quickplay";
        public override string Description => "A very fast ramp up, Quickplay allows you to experience the endgame much earlier, but isnt as extreme as No Mercy.\n\nYou will gain an EXTRA 5% score from this curve.";
        public override float PointMultiplier => 1.05f;
        public override List<ThreatLevel> ThreatLevels => new List<ThreatLevel>
        {
            // max squads(total) (0) - vehicles (1) - weaponized vehicles (2) - helicopters (3) - max faction tier[1-3] (4) - threat weight (5)
            new ThreatLevel(2,1,0,0,1,0), // 1
            new ThreatLevel(3,2,0,0,1,80), // 2
            new ThreatLevel(4,2,0,0,1,400), // 3
            new ThreatLevel(5,3,0,1,2,900), // 4
            new ThreatLevel(6,4,0,1,2,1800), // 5
            new ThreatLevel(7,4,0,1,2,2700), // 6
            new ThreatLevel(7,3,0,1,2,3900), // 7
            new ThreatLevel(8,3,1,1,3,5300), // 8
            new ThreatLevel(9,4,1,1,3,6900), // 9
            new ThreatLevel(10,3,1,1,3,8200), // 10
            new ThreatLevel(11,3,1,2,3,9500), // 11
            new ThreatLevel(12,3,2,2,3,12000), // 12: Endgame
        };
    }

    public class NoMercyCurve : ThreatCurve
    {
        public override string Name => "No Mercy";
        public override string Description => "An extremely difficult gamemode. It ramps up extremely quickly, and you will probably die. Note: There will be a LOT of enemies.\n\nYou will gain an extra 50% score from this curve.";
        public override float PointMultiplier => 1.5f;
        public override List<ThreatLevel> ThreatLevels => new List<ThreatLevel>
        {
            // max squads(total) (0) - vehicles (1) - weaponized vehicles (2) - helicopters (3) - max faction tier[1-3] (4) - threat weight (5)
            new ThreatLevel(5,1,0,1,2,0), // 1
            new ThreatLevel(6,2,0,1,2,80), // 2
            new ThreatLevel(7,2,1,1,2,400), // 3
            new ThreatLevel(7,3,1,1,3,900), // 4
            new ThreatLevel(8,4,1,2,3,1800), // 5
            new ThreatLevel(8,4,2,1,3,2700), // 6
            new ThreatLevel(9,3,2,2,3,3600), // 7
            new ThreatLevel(10,3,2,2,3,4900), // 8
            new ThreatLevel(10,4,2,2,3,6200), // 9
            new ThreatLevel(11,3,2,2,3,7900), // 10
            new ThreatLevel(12,3,2,2,3,9200), // 11
            new ThreatLevel(12,5,2,2,3,10000), // 12
            new ThreatLevel(12,5,2,2,3,11500), // 13
            new ThreatLevel(12,3,2,3,3,14000), // 14
            new ThreatLevel(13,3,3,2,3,16000), // 15
            new ThreatLevel(14,3,3,3,3,17000), // 16: Endgame
        };
    }
}
