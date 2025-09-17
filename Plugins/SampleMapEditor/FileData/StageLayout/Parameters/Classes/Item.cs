using System;
using ImGuiNET;

namespace SampleMapEditor
{
    public class ItemParameters : BaseParameters
    {
        new public int p1 = 0;

        new public void DrawParameters()
        {
            ImGui.InputInt("Index", ref p1);
            ImGui.InputText("Parameter 2", ref p2, 64);
            ImGui.InputText("Parameter 3", ref p3, 64);
            ImGui.InputText("Parameter 4", ref p4, 64);
            ImGui.InputText("Parameter 5", ref p5, 64);
            ImGui.InputText("Parameter 6", ref p6, 64);
            ImGui.InputText("Parameter 7", ref p7, 64);
            ImGui.InputText("Parameter 8", ref p8, 64);
        }


        new public void SetParameters(string[] parameters)
        {
            p1 = Convert.ToInt32(parameters[0]);
            p2 = parameters[1];
            p3 = parameters[2];
            p4 = parameters[3];
            p5 = parameters[4];
            p6 = parameters[5];
            p7 = parameters[6];
            p8 = parameters[7];
        }
    }
}