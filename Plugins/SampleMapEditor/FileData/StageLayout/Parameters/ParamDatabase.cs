using System;
using System.Collections.Generic;

namespace SampleMapEditor
{
    public class ParamDatabase
    {
        public static BaseParameters GetParameterClass(string actorName)
        {
            switch (actorName)
            {
                case "ObjTreasureBox":
                    return new TreasureBoxParameters();
                case "ObjCaveRock" or "ObjSquareBlock":
                    return new PushableObjectParams();
                case "ItemHeartPiece" or "ItemSecretSeashell":
                    return new ItemParameters();
                default:
                    return new BaseParameters();
            }
        }
        // public static Dictionary<string, BaseParameters> ParameterClasses = new Dictionary<string, BaseParameters>()
        // {
        //     {"ObjTreasureBox", new TreasureBoxParameters()},
        //     // {"AreaLevelOpen", new string[] {"Level", "TagPlayerStart", "Area Event"}},
        //     // {"TagPlayerStart", new string[] {"Name", "Animation"}},
        //     // {"ObjBladeTrap", new string[] {"East Distance", "West Distance", "South Distance", "North Distance"}},
        //     {"ObjCaveRock", new PushableObjectParams()},
        //     // {"ObjCrystalSwitch", new string[] {"State", "IsInvisible"}},
        //     {"ObjSquareBlock", new PushableObjectParams()},
        //     {"ItemHeartPiece", new ItemParameters()},
        //     {"ItemSecretSeashell", new ItemParameters()},
        //     // {"MapStatic", new string[] {"Model"}}
        // };
    }

    // public class Parameter
    // {
    //     public string Name;
    //     public object Value;

    //     public Parameter(int index)
    //     {
    //         Name = $"Parameter {index + 1}";
    //         Value = "";
    //     }

    //     public Parameter(string name, object value)
    //     {
    //         Name = name;
    //         Value = value;
    //     }
    // }
}