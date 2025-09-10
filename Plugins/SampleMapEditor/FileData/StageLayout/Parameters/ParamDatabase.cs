using System;
using System.Collections.Generic;

namespace SampleMapEditor
{
    public class ParamDatabase
    {
        public static Dictionary<string, BaseParameters> ParameterClasses = new Dictionary<string, BaseParameters>()
        {
            {"ObjTreasureBox", new TreasureBoxParameters()},
            // {"AreaLevelOpen", new string[] {"Level", "TagPlayerStart", "Area Event"}},
            // {"TagPlayerStart", new string[] {"Name", "Animation"}},
            // {"ObjBladeTrap", new string[] {"East Distance", "West Distance", "South Distance", "North Distance"}},
            {"ObjCaveRock", new PushableObjectParams()},
            // {"ObjCrystalSwitch", new string[] {"State", "IsInvisible"}},
            {"ObjSquareBlock", new PushableObjectParams()},
            {"ItemHeartPiece", new ItemParameters()},
            {"ItemSecretSeashell", new ItemParameters()},
            // {"MapStatic", new string[] {"Model"}}
        };
    }

    public class Parameter
    {
        public string Name { get; set; } = "???";
        public object Value { get; set; } = false;
    }
}