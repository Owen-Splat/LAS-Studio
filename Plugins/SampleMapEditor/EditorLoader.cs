using System.IO;
using Toolbox.Core;
using MapStudio.UI;
using GLFrameworkEngine;
using System.Collections.Generic;
using UIFramework;
using System.Linq;
using SampleMapEditor.FileData.Grezzo;
using ImGuiNET;
using Toolbox.Core.ViewModels;
using OpenTK;

namespace SampleMapEditor
{
    /// <summary>
    /// Represents a class used for loading files into the editor.
    /// IFileFormat determines what files to use. FileEditor is used to store all the editor information.
    /// </summary>
    public class EditorLoader : FileEditor, IFileFormat
    {
        /// <summary>
        /// The description of the file extension of the plugin.
        /// </summary>
        public string[] Description => new string[] { "Level Data" };

        /// <summary>
        /// The extension of the plugin. This should match whatever file you plan to open.
        /// </summary>
        public string[] Extension => new string[] { "*.lvb" };

        /// <summary>
        /// Determines if the plugin can save or not.
        /// </summary>
        public bool CanSave { get; set; } = true;

        /// <summary>
        /// File info of the loaded file format.
        /// </summary>
        public File_Info FileInfo { get; set; }

        /// <summary>
        /// Determines when to use the map editor from a given file.
        /// You can check from file extension or check the data inside the file stream.
        /// The file stream is always decompressed if the given file has a supported ICompressionFormat like Yaz0.
        /// </summary>
        public bool Identify(File_Info fileInfo, Stream stream)
        {
            //Example for loading as extension check
            //return Extension.Contains(fileInfo.Extension);
            return fileInfo.Extension == ".lvb";
        }

        /// <summary>
        /// Gets the path to a model. Mod path is checked first, then the base game path
        /// </summary>
        public string GetContentPath(string relativePath)
        {
            // check mod path first
            string path = $"{PluginConfig.ModPath}\\{relativePath}";
            if (File.Exists(path))
                return path;

            // use game path if there isn't a file in the mod path
            path = $"{PluginConfig.GamePath}\\{relativePath}";
            if (File.Exists(path))
                return path;

            // return null if there's no file anywhere
            return null;
        }


        public class ActorObj
        {
            public ulong Hash { get; set; }
            public ushort ID { get; set; }
            public string Name { get; set; }
            public string ModelName { get; set; }
            public System.Numerics.Vector3 Position = System.Numerics.Vector3.Zero;
            public System.Numerics.Vector3 Rotation = System.Numerics.Vector3.Zero;
            public System.Numerics.Vector3 Scale = System.Numerics.Vector3.One;
            public dynamic Parameters { get; set; }
            public string[] StringParams { get; set; }

            public ActorObj(ActorObj actor)
            {
                Hash = actor.Hash;
                ID = actor.ID;
                Name = actor.Name;
                ModelName = actor.ModelName;
                Position = actor.Position;
                Rotation = actor.Rotation;
                Scale = actor.Scale;
                Parameters = actor.Parameters;
                StringParams = actor.StringParams;
            }

            public ActorObj(Actor actor)
            {
                Hash = actor.Hash;
                ID = actor.ID;
                ActorDefinition actor_info = GlobalSettings.ActorDatabase.FirstOrDefault(x => x.Value.ID == ID).Value;
                Name = actor_info.Name;
                ModelName = actor_info.Model;
                Position = new System.Numerics.Vector3(actor.Position[0], actor.Position[1], actor.Position[2]);
                Rotation = new System.Numerics.Vector3(actor.Rotation[0], actor.Rotation[1], actor.Rotation[2]);
                Scale = new System.Numerics.Vector3(actor.Scale[0], actor.Scale[1], actor.Scale[2]);
                Parameters = ParamDatabase.GetParameterClass(Name);
                Parameters.Parameter1.Value = actor.Parameters[0];
                Parameters.Parameter2.Value = actor.Parameters[1];
                Parameters.Parameter3.Value = actor.Parameters[2];
                Parameters.Parameter4.Value = actor.Parameters[3];
                Parameters.Parameter5.Value = actor.Parameters[4];
                Parameters.Parameter6.Value = actor.Parameters[5];
                Parameters.Parameter7.Value = actor.Parameters[6];
                Parameters.Parameter8.Value = actor.Parameters[7];
                StringParams = actor.Parameters;
            }
        }


        public Dictionary<string, List<ActorObj>> MapObjList { get; set; } = new Dictionary<string, List<ActorObj>>();

        public static Vector3 GetObjPos(ActorObj obj)
        {
            return new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Z);
        }
        public static Vector3 GetObjRotation(ActorObj obj)
        {
            return new Vector3(obj.Rotation.X, obj.Rotation.Y, obj.Rotation.Z);
        }
        public static Vector3 GetObjScale(ActorObj obj)
        {
            return new Vector3(obj.Scale.X, obj.Scale.Y, obj.Scale.Z);
        }


        public string GetModelPathFromName(string roomName, string actorName)
        {
            ActorObj actor = MapObjList[roomName].Find(x => x.Name == actorName);
            if (actor == null) return null;
            if (actor.ModelName == "Null")
            {
                if (actor.Name == "MapStatic")
                    return GetContentPath($"region_common\\map\\{actor.Parameters.Parameter1.Value}.bfres"); // Read Parameters[0] for the map model
                else
                    return GetContentPath($"region_common\\actor\\{actor.Name}.bfres");
            }
            return GetContentPath($"region_common\\actor\\{actor.ModelName}");
        }

        public string GetModelPathFromObject(string roomName, ActorObj obj)
        {
            return GetModelPathFromName(roomName, obj.Name);
        }

        public string GetTextureArchive(string levelName)
        {
            return GetContentPath($"region_common\\map\\{levelName}.bntx");
        }

        public ActorObj GetActorFromObj(string roomName, ActorObj obj)
        {
            string ucName = obj.Name;
            return GetActorFromName(roomName, ucName);
        }

        public ActorObj GetActorFromName(string roomName, string actorName)
        {
            ActorObj actor = MapObjList[roomName].Find(x => x.Name == actorName);
            return actor;
        }

        public NodeBase currentObj { get; set; }

        /// <summary>
        /// Loads the given file data from a stream.
        /// </summary>
        public void Load(Stream stream)
        {
            GlobalSettings.LoadDataBase(); // Parse Actordb

            MapObjList.Clear();
            currentObj = null;

            string levelFolder = FileInfo.FolderPath;
            string[] roomFiles = Directory.GetFiles(levelFolder);
            foreach (string roomFile in roomFiles)
            {
                if (!roomFile.EndsWith(".leb"))
                    continue;

                Room room = new Room(File.ReadAllBytes(roomFile));
                List<ActorObj> actors = new List<ActorObj>();
                foreach (Actor actor in room.Actors)
                {
                    ActorObj obj = new ActorObj(actor);
                    actors.Add(obj);
                }
                string roomName = roomFile.Split("/").Last().Split("\\").Last();
                roomName = roomName.Split("_").Last().Split(".").First();
                MapObjList.Add(roomName, actors);
            }

            //For this example I will show loading 3D objects into the scene
            MapScene mapScene = new MapScene();
            mapScene.Setup(this);

            Root.TagUI.UIDrawer += delegate
            {
                DrawLevelProperties();
            };
            Scene.SelectionChanged += delegate
            {
                if (currentObj != null)
                    currentObj.IsSelected = false;
            };

            //Animation test
            /*Root.AddChild(new Toolbox.Core.ViewModels.NodeBase("AnimationTest")
            {
                Tag = new AnimationController(),
            });*/
        }

        /// <summary>
        /// Saves the given file data to a stream.
        /// </summary>
        public void Save(Stream stream)
        {

        }

        //Extra overrides for FileEditor you can use for custom UI

        /// <summary>
        /// Draws the viewport menu bar usable for custom tools.
        /// </summary>
        public override void DrawViewportMenuBar()
        {

        }

        /// <summary>
        /// When an asset item from the asset windows gets dropped into the editor.
        /// You can configure your own asset category from the asset window and make custom asset items to drop into.
        /// </summary>
        public override void AssetViewportDrop(AssetItem item, Vector2 screenPosition)
        {
            //viewport context
            var context = GLContext.ActiveContext;

            //Screen coords can be converted into 3D space
            //By default it will spawn in the mouse position at a distance
            Vector3 position = context.ScreenToWorld(screenPosition.X, screenPosition.Y, 100);
            //Collision dropping can be used to drop these assets to the ground from CollisionCaster
            if (context.EnableDropToCollision)
            {
                Quaternion rot = Quaternion.Identity;
                CollisionDetection.SetObjectToCollision(context, context.CollisionCaster, screenPosition, ref position, ref rot);
            }
        }

        /// <summary>
        /// Checks for dropped files to use for the editor.
        /// If the value is true, the file will not be loaded as an editor if supported.
        /// </summary>
        public override bool OnFileDrop(string filePath)
        {
            return false;
        }

        public override List<DockWindow> PrepareDocks()
        {
            List<DockWindow> windows = new List<DockWindow>();
            windows.Add(Workspace.Outliner);
            windows.Add(Workspace.PropertyWindow);
            windows.Add(Workspace.ConsoleWindow);
            windows.Add(Workspace.ViewportWindow);
            windows.Add(Workspace.TimelineWindow);
            windows.Add(Workspace.GraphWindow);
            return windows;
        }


        public void DrawActorProperties(EditableObject obj)
        {
            NodeBase node = obj.UINode;
            currentObj = node;
            currentObj.IsSelected = true;

            ActorObj actor = (ActorObj)node.Tag;

            actor.Position.X = obj.Transform._position.X;
            actor.Position.Y = obj.Transform._position.Y;
            actor.Position.Z = obj.Transform._position.Z;

            actor.Rotation.X = obj.Transform.RotationEulerDegrees.X;
            actor.Rotation.Y = obj.Transform.RotationEulerDegrees.Y;
            actor.Rotation.Z = obj.Transform.RotationEulerDegrees.Z;

            actor.Scale.X = obj.Transform.Scale.X;
            actor.Scale.Y = obj.Transform.Scale.Y;
            actor.Scale.Z = obj.Transform.Scale.Z;

            if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputFloat3("Position", ref actor.Position);
                ImGui.InputFloat3("Rotation", ref actor.Rotation);
                ImGui.InputFloat3("Scale", ref actor.Scale);
            }

            if (ImGui.CollapsingHeader("Parameters", ImGuiTreeNodeFlags.Framed))
            {
                ImGui.InputText(actor.Parameters.Parameter1.Name, ref actor.StringParams[0], 64);
                ImGui.InputText(actor.Parameters.Parameter2.Name, ref actor.StringParams[1], 64);
                ImGui.InputText(actor.Parameters.Parameter3.Name, ref actor.StringParams[2], 64);
                ImGui.InputText(actor.Parameters.Parameter4.Name, ref actor.StringParams[3], 64);
                ImGui.InputText(actor.Parameters.Parameter5.Name, ref actor.StringParams[4], 64);
                ImGui.InputText(actor.Parameters.Parameter6.Name, ref actor.StringParams[5], 64);
                ImGui.InputText(actor.Parameters.Parameter7.Name, ref actor.StringParams[6], 64);
                ImGui.InputText(actor.Parameters.Parameter8.Name, ref actor.StringParams[7], 64);
            }
        }


        public void DrawLevelProperties()
        {
            // ImGui.InputText("Level Name", ref n)
        }
    }
}
