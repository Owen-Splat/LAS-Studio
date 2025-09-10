using System.Collections.Generic;

namespace SampleMapEditor
{
    public class ParamDatabase
    {
        public static Dictionary<string, string[]> ParameterNames = new Dictionary<string, string[]>()
        {
            {"AreaLevelOpen", new string[] {"Level", "TagPlayerStart", "Area Event"}},
            {"TagPlayerStart", new string[] {"Name", "Animation"}},
            {"ObjBladeTrap", new string[] {"East Distance", "West Distance", "South Distance", "North Distance"}},
            {"ObjCaveRock", new string[] {"East Push", "West Push", "South Push", "North Push"}},
            {"ObjCrystalSwitch", new string[] {"State", "IsInvisible"}},
            {"ObjSquareBlock", new string[] {"East Push", "West Push", "South Push", "North Push"}},
            {"ObjTreasureBox", new string[] {"Actor Switch", "Item", "Index"}},
            {"ItemHeartPiece", new string[] {"Index"}},
            {"ItemSecretSeashell", new string[] {"Index"}},
            {"MapStatic", new string[] {"Model"}}
        };
    }
}