using System;
using ImGuiNET;

namespace SampleMapEditor
{
    public class TreasureBoxParameters : BaseParameters
    {
        new public int p3 = 0;

        new public void DrawParameters()
        {
            ImGui.InputText("Actor Switch", ref p1, 64);
            ImGui.InputText("Item", ref p2, 64);
            ImGui.InputInt("Index", ref p3);
            ImGui.InputText("Parameter 4", ref p4, 64);
            ImGui.InputText("Parameter 5", ref p5, 64);
            ImGui.InputText("Parameter 6", ref p6, 64);
            ImGui.InputText("Parameter 7", ref p7, 64);
            ImGui.InputText("Parameter 8", ref p8, 64);
        }

        new public void SetParameters(string[] parameters)
        {
            int index = -1;
            try
            {
                index = Convert.ToInt32(parameters[2]);
            }
            catch (FormatException) { }
            p1 = parameters[0];
            p2 = parameters[1];
            p3 = index;
            p4 = parameters[3];
            p5 = parameters[4];
            p6 = parameters[5];
            p7 = parameters[6];
            p8 = parameters[7];
        }
    }
}