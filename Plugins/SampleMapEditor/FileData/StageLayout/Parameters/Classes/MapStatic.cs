using System;
using ImGuiNET;
using MapStudio.UI;

namespace SampleMapEditor
{
    public class MapStaticParameters : BaseParameters
    {
        new public string p1 = "MarinTarinHouse_01A";

        new public void DrawParameters()
        {
            ImGui.InputText("Map Model", ref p1, 64);
            ImGui.SameLine();
            if (ImGui.Button("Redraw"))
                loader.UpdateObj();
            ImGui.InputText("Parameter 2", ref p2, 64);
            ImGui.InputText("Parameter 3", ref p3, 64);
            ImGui.InputText("Parameter 4", ref p4, 64);
            ImGui.InputText("Parameter 5", ref p5, 64);
            ImGui.InputText("Parameter 6", ref p6, 64);
            ImGui.InputText("Parameter 7", ref p7, 64);
            ImGui.InputText("Parameter 8", ref p8, 64);
        }

        new public string[] GetParameters()
        {
            return new string[8]
            {
                ConvertParam(p1),
                ConvertParam(p2),
                ConvertParam(p3),
                ConvertParam(p4),
                ConvertParam(p5),
                ConvertParam(p6),
                ConvertParam(p7),
                ConvertParam(p8)
            };
        }

        new public void SetParameters(string[] parameters)
        {
            p1 = parameters[0];
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