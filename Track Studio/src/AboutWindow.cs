using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Numerics;
using UIFramework;
using ImGuiNET;
using MapStudio.UI;
using Toolbox.Core;

namespace MapStudio
{
    public class AboutWindow : Window
    {
        public override string Name => TranslationSource.GetText("ABOUT");

        public override ImGuiWindowFlags Flags => ImGuiWindowFlags.NoDocking;

        public static string AppVersion = "0.1.1";
        string[] ChangeLog;
        string[] ChangeType;

        public AboutWindow()
        {
            Size = new Vector2(500, 600);
            Opened = false;

            //Parse changelog
            string file = Path.Combine(Runtime.ExecutableDir, "Lib", "Program", "ChangeLog.txt");
            string changeLog = File.ReadAllText(file);
            ChangeLog = changeLog.Split("\n").ToArray();

            ChangeType = new string[ChangeLog.Length];
            for (int i = 0; i < ChangeLog.Length; i++)
            {
                var log = ChangeLog[i].Split(":");
                if (log.Length == 2)
                {
                    var type = log[0];
                    var info = log[1];
                    ChangeLog[i] = info;

                    if (type == "ADDITION") ChangeType[i] = "\uf055";
                    if (type == "BUG") ChangeType[i] = "\uf188";
                    if (type == "IMPROVEMENT") ChangeType[i] = "\uf118";
                }
            }
        }

        public override void Render()
        {
            if (!IconManager.HasIcon("TOOL_ICON"))
                IconManager.AddIcon("TOOL_ICON", Properties.Resources.Icon, false);

            base.Render();

            ImGui.Image((IntPtr)IconManager.GetTextureIcon("TOOL_ICON"), new Vector2(50, 50));
            var bottom = ImGui.GetCursorPos();

            ImGui.SameLine();

            ImGui.SetWindowFontScale(1.5f);
            ImGui.AlignTextToFramePadding();

            var textPos = ImGui.GetCursorPos();
            ImGui.Text($"LAS Level Editor v{AppVersion}");
            ImGui.SetWindowFontScale(1);

            ImGui.SetCursorPos(new Vector2(textPos.X, textPos.Y + 30));
            ImGuiHelper.HyperLinkText("Copyright @ Owen_Splat 2025");

            ImGui.SetCursorPos(bottom);

            if (ImGui.CollapsingHeader("Credits"))
            {
                ImGui.BulletText("Owen_Splat - Main Developer");
                ImGui.BulletText("MapStudioProject - Sample-Editor template");

                // ImGuiHelper.BoldText("Beta Testers:");
                // ImGui.BulletText("User");
                // ImGui.BulletText("User");
                // ImGui.BulletText("User");
                // ImGui.BulletText("User");
            }

            var flag = ImGuiWindowFlags.HorizontalScrollbar;
            if (ImGui.BeginChild("changeLogCh", new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 4), false, flag))
                DrawChangeLog();
            ImGui.EndChild();
        }

        private void DrawChangeLog()
        {
            bool display = false;
            for (int i = 0; i < ChangeLog.Length; i++)
            {
                if (ChangeLog[i].StartsWith("Version"))
                {
                    //Only open the first change log by default
                    var flags = i == 0 ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
                    display = ImGui.CollapsingHeader($"Changelog {ChangeLog[i]}", flags);
                }
                else if (display)
                {
                    string type = ChangeType[i];
                    ImGui.BulletText($"   {type}   {ChangeLog[i]}");
                }
            }
        }
    }
}
