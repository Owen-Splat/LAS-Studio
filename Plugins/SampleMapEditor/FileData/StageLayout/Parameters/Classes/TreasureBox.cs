using System;
using System.Collections.Generic;
using ImGuiNET;

namespace SampleMapEditor
{
    public class TreasureBoxParameters : BaseParameters
    {
        new public string p1 = "Switch0";
        new public string p2 = "Rupee20";
        new public int p3 = -1;
        new public int p4 = 0;
        private int contentID = -1;
        private List<string> contents = new List<string>()
        {
            "$ENEMY",
            "$PANEL",
            "$EXT:MasterStalfonLetter",
            "SwordLv1",
            "SwordLv2",
            "Shield",
            "MirrorShield",
            "Bomb",
            "Bow",
            "HookShot",
            "Boomerang",
            "MagicRod",
            "Shovel",
            "SleepyMushroom",
            "MagicPowder",
            "RocsFeather",
            "PowerBraceletLv1",
            "PowerBraceletLv2",
            "PegasusBoots",
            "Ocarina",
            "ClothesGreen",
            "ClothesRed",
            "ClothesBlue",
            "Flippers",
            "SecretMedicine",
            "Seashell",
            "GoldenLeaf",
            "TailKey",
            "SlimeKey",
            "AnglerKey",
            "FaceKey",
            "BirdKey",
            "YoshiDoll",
            "Ribbon",
            "DogFood",
            "Bananas",
            "Stick",
            "Honeycomb",
            "Pineapple",
            "Hibiscus",
            "Letter",
            "Broom",
            "FishingHook",
            "PinkBra",
            "MermaidsScale",
            "MagnifyingLens",
            "Compass",
            "DungeonMap",
            "StoneBeak",
            "SmallKey",
            "NightmareKey",
            "FullMoonCello",
            "ConchHorn",
            "SeaLilysBell",
            "SurfHarp",
            "WindMarimba",
            "CoralTriangle",
            "EveningCalmOrgan",
            "ThunderDrum",
            "HeartPiece",
            "HeartContainer",
            "Arrow",
            "Heart",
            "Rupee1",
            "Rupee5",
            "Rupee20",
            "Rupee50",
            "Rupee100",
            "Rupee300",
            "Fairy",
            "DefenceUp",
            "PowerUp",
            "Apple",
            "GreenApple",
            "Bottle",
            "Song_WindFish",
            "Song_Mambo",
            "Song_Soul",
            "MagicPowder_MaxUp",
            "Bomb_MaxUp",
            "Arrow_MaxUp",
            "PanelDungeonPiece",
            "PanelDungeonPieceSet",
            "ShellRader",
            "BottleFairy",
            "PanelDungeonPlusChip"
        };

        new public void DrawParameters()
        {
            ImGui.InputText("Actor Switch", ref p1, 64);
            string[] items = contents.ToArray();
            ImGui.Combo("Item", ref contentID, items, items.Length);
            p2 = items[contentID];
            if (contentID == contents.Count - 1)
            {
                p2 = "";
                ImGui.SameLine();
                ImGui.InputText("##ItemCustom", ref p2, 64);
            }
            ImGui.InputInt("Index", ref p3);
            ImGui.Combo("Chest Type", ref p4, new string[] { "Default", "Locked", "Group" }, 3);
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
            contentID = contents.FindIndex(0, contents.Count, x => x == p2);
            p3 = index;
            int chestType = 0;
            try
            {
                chestType = Convert.ToInt32(parameters[3]);
            }
            catch (FormatException) { }
            p4 = chestType;
            p5 = parameters[4];
            p6 = parameters[5];
            p7 = parameters[6];
            p8 = parameters[7];
        }

        public TreasureBoxParameters()
        {
            contents.Sort();
            contents.Add("Custom");
            contentID = contents.FindIndex(0, contents.Count, x => x == p2);
        }
    }
}