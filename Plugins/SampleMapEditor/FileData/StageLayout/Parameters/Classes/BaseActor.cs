using System;
using ImGuiNET;

namespace SampleMapEditor
{
    public class BaseParameters
    {
        public EditorLoader loader; // Since we call DrawParameters in each object, they may need to reference the loader

        // EditorLoader does not care about any variables, it will only call the functions
        // These still need to be public for the inherited classes to use
        public string p1 = "";
        public string p2 = "";
        public string p3 = "";
        public string p4 = "";
        public string p5 = "";
        public string p6 = "";
        public string p7 = "";
        public string p8 = "";

        public void DrawParameters()
        {
            ImGui.InputText("Parameter 1", ref p1, 64);
            ImGui.InputText("Parameter 2", ref p2, 64);
            ImGui.InputText("Parameter 3", ref p3, 64);
            ImGui.InputText("Parameter 4", ref p4, 64);
            ImGui.InputText("Parameter 5", ref p5, 64);
            ImGui.InputText("Parameter 6", ref p6, 64);
            ImGui.InputText("Parameter 7", ref p7, 64);
            ImGui.InputText("Parameter 8", ref p8, 64);
        }

        public object[] GetParameters()
        {
            return new object[8]
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

        public void SetParameters(string[] parameters)
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

        /// <summary>
        /// LEB Actor params can only be float, uint32, or string. We need to convert other types to these
        /// </summary>
        public object ConvertParam(dynamic parameter)
        {
            object actorParam;
            if (parameter is int)
            {
                if (parameter < 0)
                    actorParam = "";
                else
                    actorParam = (uint)parameter;
            }
            else if (parameter is bool)
            {
                if (parameter)
                    actorParam = (uint)1;
                else
                    actorParam = (uint)0;
            }
            else
                actorParam = parameter;

            return actorParam;
        }
    }
}