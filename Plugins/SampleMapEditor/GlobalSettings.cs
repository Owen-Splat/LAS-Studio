using CafeLibrary.Rendering;
using GLFrameworkEngine;
using Newtonsoft.Json;
using OpenTK;
using Syroot.NintenTools.NSW.Bntx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;

namespace SampleMapEditor
{
    public class GlobalSettings
    {
        //public static Dictionary<int, ActorDefinition> ActorDatabase = new Dictionary<int, ActorDefinition>();    // use name instead
        public static Dictionary<string, ActorDefinition> ActorDatabase = new Dictionary<string, ActorDefinition>();

        public static List<string> RoomDatabase = new List<string>();

        public static Dictionary<string, Dictionary<string, GenericRenderer.TextureView>> TextureArchive = new Dictionary<string, Dictionary<string, GenericRenderer.TextureView>>();

        public static string GamePath { get; set; }

        public static string ModOutputPath { get; set; }

        public static bool PathsValid { get; set; }

        public static bool IsLinkingComboBoxActive { get; set; } = true;

        public static PathSettings PathDrawer = new PathSettings();

        /// <summary>
        /// Loads the actor and room databases
        /// </summary>
        public static void LoadDatabases()
        {
            if (ActorDatabase.Count != 0 && RoomDatabase.Count != 0)
                return;

            Console.WriteLine("~ Called GlobalSettings.LoadDatabases() ~");

            if (ActorDatabase.Count == 0)
                LoadActorDb();
            if (RoomDatabase.Count == 0)
                LoadRoomDb();
        }

        /// <summary>
        /// Reads the bntx files from the romfs to build a texture archive
        /// </summary>
        public static void LoadTextures()
        {
            if (TextureArchive.Count != 0)
                return;

            Console.WriteLine("~ Called GlobalSettings.LoadTextures() ~");

            string[] textureFiles = new string[]
            {
                "Field",
                "Lv01TailCave",
                "Lv02BottleGrotto",
                "Lv03KeyCavern",
                "Lv04AnglersTunnel",
                "Lv05CatfishsMaw",
                "Lv06FaceShrine",
                "Lv07EagleTower",
                "Lv08TurtleRock",
                "Lv09WindFishsEgg",
                "Lv10ClothesDungeon"
            };

            foreach (var textureFile in textureFiles)
            {
                string path = GetContentPath($"region_common\\map\\{textureFile}.bntx");
                BntxFile bntx = new BntxFile(path);
                var archive = new Dictionary<string, GenericRenderer.TextureView>();
                foreach (Texture tex in bntx.Textures)
                {
                    BntxTexture btex = new BntxTexture(bntx, tex);
                    archive.Add(btex.Name, new GenericRenderer.TextureView(btex) { OriginalSource = btex });
                }
                TextureArchive.Add(textureFile, archive);
            }
        }

        /// <summary>
        /// Gets content path from either the update, game, or aoc directories based on what is present.
        /// </summary>
        public static string GetContentPath(string relativePath)
        {
            //Update first then base package.
            if (File.Exists($"{ModOutputPath}\\RomFS\\{relativePath}")) return $"{ModOutputPath}\\RomFS\\{relativePath}";
            if (File.Exists($"{GamePath}\\{relativePath}")) return $"{GamePath}\\{relativePath}";

            return relativePath;
        }

        static void LoadActorDb()
        {
            Console.WriteLine("~ Called GlobalSettings.LoadActorDb() ~");

            string path = $"{Runtime.ExecutableDir}\\Lib\\Actors.txt";

            if (!File.Exists(path))
            {
                Console.WriteLine($"File \"{path}\" could not be found!");
                return;
            }

            var actorDb = new ActorDefinitionDb(path);
            foreach (var actor in actorDb.Definitions)
            {
                if (actor.Name == "Null")
                    continue;
                ActorDatabase.Add(actor.Name, actor);
            }
        }

        static void LoadRoomDb()
        {
            Console.WriteLine("~ Called GlobalSettings.LoadRoomDb() ~");

            string path = $"{Runtime.ExecutableDir}\\Lib\\Rooms.txt";

            if (!File.Exists(path))
                return;

            foreach (string roomName in File.ReadAllLines(path))
                RoomDatabase.Add(roomName);

            // How the Rooms.txt file was created
            // string path = $"{GamePath}\\region_common\\map";

            // if (!Directory.Exists(path))
            //     return;

            // string[] mapFiles = Directory.GetFiles(path);
            // foreach (string map in mapFiles)
            // {
            //     string fileName = Path.GetFileNameWithoutExtension(map);
            //     if (fileName.Contains('_'))
            //         if (fileName.Split('_').Length > 2)
            //             RoomDatabase.Add(fileName);
            // }

            // path = $"{Runtime.ExecutableDir}\\Lib\\Rooms.txt";
            // File.WriteAllLines(path, RoomDatabase);
            // Console.WriteLine("Room Database created!");
        }


        public class PathSettings
        {
            public PathColor RailColor0 = new PathColor(new Vector3(170, 0, 160), new Vector3(255, 64, 255), new Vector3(255, 64, 255));
            public PathColor RailColor1 = new PathColor(new Vector3(170, 160, 0), new Vector3(255, 0, 0), new Vector3(255, 0, 0));
        }

        public class PathColor
        {
            public Vector3 PointColor = new Vector3(0, 0, 0);
            public Vector3 LineColor = new Vector3(0, 0, 0);
            public Vector3 ArrowColor = new Vector3(0, 0, 0);

            [JsonIgnore]
            public EventHandler OnColorChanged;

            public PathColor(Vector3 point, Vector3 line, Vector3 arrow)
            {
                PointColor = new Vector3(point.X / 255f, point.Y / 255f, point.Z / 255f);
                LineColor = new Vector3(line.X / 255f, line.Y / 255f, line.Z / 255f);
                ArrowColor = new Vector3(arrow.X / 255f, arrow.Y / 255f, arrow.Z / 255f);
            }
        }
    }
}
