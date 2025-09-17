using System;
using ImGuiNET;

namespace SampleMapEditor
{
    public class PushableObjectParams : BaseParameters
    {
        // Normally these are just 0 or 1 for if you can push in a direction
        // However, if multiple directions are valid, the direction to trigger a flag need to be 2
        // I would love to still use checkboxes, but I am unsure how to make the UI clean
        new public bool p1 = false;
        new public bool p2 = false;
        new public bool p3 = false;
        new public bool p4 = false;

        private bool solution1 = false;
        private bool solution2 = false;
        private bool solution3 = false;
        private bool solution4 = false;

        new public void DrawParameters()
        {
            ImGui.Checkbox("East Push   ", ref p1);
            ImGui.SameLine();
            if (ImGui.Checkbox("Solution##East", ref solution1))
            {
                if (solution1)
                {
                    solution2 = false;
                    solution3 = false;
                    solution4 = false;
                    if (!p1)
                        solution1 = false;
                }
            }

            ImGui.Checkbox("West Push  ", ref p2);
            ImGui.SameLine();
            if (ImGui.Checkbox("Solution##West", ref solution2))
            {
                if (solution2)
                {
                    solution1 = false;
                    solution3 = false;
                    solution4 = false;
                    if (!p2)
                        solution2 = false;
                }
            }

            ImGui.Checkbox("South Push ", ref p3);
            ImGui.SameLine();
            if (ImGui.Checkbox("Solution##South", ref solution3))
            {
                if (solution3)
                {
                    solution1 = false;
                    solution2 = false;
                    solution4 = false;
                    if (!p3)
                        solution3 = false;
                }
            }

            ImGui.Checkbox("North Push ", ref p4);
            ImGui.SameLine();
            if (ImGui.Checkbox("Solution##North", ref solution4))
            {
                if (solution4)
                {
                    solution1 = false;
                    solution2 = false;
                    solution3 = false;
                    if (!p4)
                        solution4 = false;
                }
            }

            ImGui.InputText("Parameter 5", ref p5, 64);
            ImGui.InputText("Parameter 6", ref p6, 64);
            ImGui.InputText("Parameter 7", ref p7, 64);
            ImGui.InputText("Parameter 8", ref p8, 64);
        }

        new public object[] GetParameters()
        {
            int state1 = 0;
            if (p1)
            {
                state1 = 1;
                if (solution1)
                    state1 = 2;
            }

            int state2 = 0;
            if (p2)
            {
                state2 = 1;
                if (solution2)
                    state2 = 2;
            }

            int state3 = 0;
            if (p3)
            {
                state3 = 1;
                if (solution3)
                    state3 = 2;
            }

            int state4 = 0;
            if (p4)
            {
                state4 = 1;
                if (solution4)
                    state4 = 2;
            }

            return new object[8]
            {
                ConvertParam(state1),
                ConvertParam(state2),
                ConvertParam(state3),
                ConvertParam(state4),
                ConvertParam(p5),
                ConvertParam(p6),
                ConvertParam(p7),
                ConvertParam(p8)
            };
        }

        new public void SetParameters(string[] parameters)
        {
            int state;
            try
            {
                state = Convert.ToInt32(parameters[0]);
            }
            catch (FormatException)
            {
                state = -1;
            }
            if (state > 0)
                p1 = true;
            if (state == 2)
                solution1 = true;

            try
            {
                state = Convert.ToInt32(parameters[1]);
            }
            catch (FormatException)
            {
                state = -1;
            }
            if (state > 0)
                p2 = true;
            if (state == 2)
                solution2 = true;

            try
            {
                state = Convert.ToInt32(parameters[2]);
            }
            catch (FormatException)
            {
                state = -1;
            }
            if (state > 0)
                p3 = true;
            if (state == 2)
                solution3 = true;

            try
            {
                state = Convert.ToInt32(parameters[3]);
            }
            catch (FormatException)
            {
                state = -1;
            }
            if (state > 0)
                p4 = true;
            if (state == 2)
                solution4 = true;

            p5 = parameters[4];
            p6 = parameters[5];
            p7 = parameters[6];
            p8 = parameters[7];

        }
    }
}