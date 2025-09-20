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
using System;
using CafeLibrary.Rendering;

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
            string path = $"{PluginConfig.ModPath}\\RomFS\\{relativePath}";
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
            public string Hash = "";
            public ushort ID { get; set; }
            public string Name = "";
            public string ModelName { get; set; }
            public System.Numerics.Vector3 Position = System.Numerics.Vector3.Zero;
            public System.Numerics.Vector3 Rotation = System.Numerics.Vector3.Zero;
            public System.Numerics.Vector3 Scale = System.Numerics.Vector3.One;
            public dynamic Parameters { get; set; }
            public ActorSwitch[] Flags = new ActorSwitch[4]
            {
                new(4, 0),
                new(4, 0),
                new(4, 0),
                new(4, 0)
            };
            public List<ActorLink> Links = new List<ActorLink>();
            public List<ObjPointLink> Points = new List<ObjPointLink>();

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
                Flags = actor.Flags;
                Links = actor.Links;
                Points = actor.Points;
            }

            public ActorObj(Room room, Actor actor)
            {
                Hash = actor.Hash.ToString();
                ID = actor.ID;
                ActorDefinition actor_info = GlobalSettings.ActorDatabase.FirstOrDefault(x => x.Value.ID == ID).Value;
                Name = actor_info.Name;
                ModelName = actor_info.Model;
                Position = actor.Position;
                Rotation = actor.Rotation;
                Scale = actor.Scale;
                Parameters = ParamDatabase.GetParameterClass(Name);
                Parameters.SetParameters(actor.Parameters);
                Flags = actor.Switches;
                Links = actor.Links;
                foreach (ActorLink link in Links)
                {
                    int actorIndex = int.Parse(link.Hash);
                    link.Hash = room.Actors[actorIndex].Hash.ToString();
                }
                foreach (PointLink point in actor.Points)
                    Points.Add(new ObjPointLink(room.Points[point.PointIndex], point.Parameters));
            }

            public ActorObj(ActorDefinition definition)
            {
                ID = definition.ID;
                Name = definition.Name;
                ModelName = definition.Model;
                Parameters = ParamDatabase.GetParameterClass(Name);
                Flags = new ActorSwitch[4]
                {
                    new(4, 0),
                    new(4, 0),
                    new(4, 0),
                    new(4, 0)
                };
            }

            public void Change(ActorDefinition definition)
            {
                ID = definition.ID;
                Name = definition.Name;
                ModelName = definition.Model;
                var newParameters = ParamDatabase.GetParameterClass(Name);
                if (Parameters.GetType() != newParameters.GetType())
                    Parameters = newParameters;
            }

            public class ObjPointLink
            {
                public System.Numerics.Vector3 Point = System.Numerics.Vector3.Zero;
                public string[] Parameters = new string[2] { "", "" };

                public ObjPointLink(System.Numerics.Vector3 point, string[] parameters)
                {
                    Point = point;
                    Parameters = parameters;
                }

                public ObjPointLink() { }
            }
        }


        public Dictionary<string, List<ActorObj>> MapObjList { get; set; } = new Dictionary<string, List<ActorObj>>();
        public List<ulong> HashList { get; set; } = new List<ulong>();
        public Random RNG { get; set; } = new Random();

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

        public string GetModelPathFromObject(ActorObj actor)
        {
            if (actor == null) return null;
            if (actor.ModelName == "Null")
            {
                if (actor.Name == "MapStatic")
                    return GetContentPath($"region_common\\map\\{actor.Parameters.p1}.bfres"); // Read Parameters[0] for the map model
                else
                    return GetContentPath($"region_common\\actor\\{actor.Name}.bfres");
            }
            return GetContentPath($"region_common\\actor\\{actor.ModelName}");
        }


        private NodeBase currentObj { get; set; }

        /// <summary>
        /// Loads the given file data from a stream.
        /// </summary>
        public void Load(Stream stream)
        {
            if (!GlobalSettings.PathsValid)
                return;

            GlobalSettings.LoadDatabases(); // Build ActorDatabase and RoomDatabase
            GlobalSettings.LoadTextures(); // Build TextureArchive

            string levelName = FileInfo.FileName.Split('.')[0];
            string levelFolder = $"{GlobalSettings.GamePath}\\region_common\\level\\{levelName}";
            string[] roomFiles = Directory.GetFiles(levelFolder);
            foreach (string roomFile in roomFiles)
            {
                string roomName = roomFile.Split("/").Last().Split("\\").Last();

                if (!roomName.EndsWith(".leb"))
                    continue;

                string filePath = GetContentPath($"region_common\\level\\{levelName}\\{roomName}");

                Room room = new Room(File.ReadAllBytes(filePath));
                List<ActorObj> actors = new List<ActorObj>();
                foreach (Actor actor in room.Actors)
                {
                    ActorObj obj = new ActorObj(room, actor);
                    obj.Parameters.loader = this;
                    HashList.Add(Convert.ToUInt64(obj.Hash));
                    actors.Add(obj);
                }
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

            Workspace.Outliner.SelectionChanged += (o, e) =>
            {
                // Set currentObj to the outliner selected node
                var node = o as NodeBase;
                if (currentObj != node)
                {
                    if (currentObj != null)
                        currentObj.IsSelected = false;
                    currentObj = o as NodeBase;
                    currentObj.IsSelected = true;
                }
            };

            // Asset Window
            Workspace.AddAssetCategory(new AssetViewMapObject());
            // Workspace.AddAssetCategory(new AssetViewMapModel());
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
            // Return if a room or actor is not selected, we need to know which room to place the actor!
            // Later down the line I might get rid of rooms and instead handle that stuff when saving
            // That would be less confusing for the user if it's just map objects
            if (currentObj == Root || Workspace.Outliner.SelectedNodes.Count == 0)
                return;

            // Now get the AssetWindow object
            MapObjectAsset asset = item as MapObjectAsset;
            if (asset == null)
                return;

            if (currentObj != null)
                currentObj.IsSelected = false;

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

            //Snap it to half an in-game grid unit
            float unit_size = 1.5f;
            position.X = (float)Math.Round(position.X / (unit_size / 2)) * (unit_size / 2);
            position.Z = (float)Math.Round(position.Z / (unit_size / 2)) * (unit_size / 2);

            // Add the object
            var obj = new ActorObj(asset.ObjDefinition);
            obj.Parameters.loader = this;
            EditableObject render = AddObject(obj);
            render.Transform.Position = position;
            render.Transform.UpdateMatrix(true);
            render.UINode.IsSelected = true;
            currentObj = render.UINode;
            AddRender(render);
            Scene.SelectionUIChanged?.Invoke(render.UINode, EventArgs.Empty);

            //Update the SRT tool if active
            GLContext.ActiveContext.TransformTools.UpdateOrigin();
            GLContext.ActiveContext.UpdateViewport = true;
        }


        public List<string> hiddenObjs = new List<string>() { "Area", "Roof", "Tag" };
        public Dictionary<string, GenericRenderer.TextureView> textureArchive = new Dictionary<string, GenericRenderer.TextureView>();


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
            List<DockWindow> windows = new List<DockWindow>
            {
                Workspace.Outliner,
                Workspace.PropertyWindow,
                Workspace.ConsoleWindow,
                Workspace.AssetViewWindow,
                Workspace.HelpWindow,
                Workspace.ToolWindow,
                Workspace.ViewportWindow
            };
            // windows.Add(Workspace.TimelineWindow);
            // windows.Add(Workspace.GraphWindow);
            Workspace.ToolWindow.SplitRatio = 0.1f;
            Workspace.ViewportWindow.Pipeline._context.TransformTools.TransformSettings.SnapTransform = true; // snap objects when moving
            return windows;
        }


        private string copiedHash = "";
        public void DrawActorProperties(EditableObject obj)
        {
            NodeBase node = obj.UINode;
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

            System.Numerics.Vector2 buttonIconSize = new System.Numerics.Vector2(32);
            float width = ImGui.GetWindowWidth();

            if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputFloat3("Position", ref actor.Position);
                if (ImGui.IsItemEdited())
                {
                    obj.Transform.Position = GetObjPos(actor);
                    obj.Transform.UpdateMatrix(true);
                    GLContext.ActiveContext.TransformTools.UpdateOrigin();
                    GLContext.ActiveContext.UpdateViewport = true;
                }
                ImGui.InputFloat3("Rotation", ref actor.Rotation);
                if (ImGui.IsItemEdited())
                {
                    obj.Transform.RotationEulerDegrees = GetObjRotation(actor);
                    obj.Transform.UpdateMatrix(true);
                    GLContext.ActiveContext.TransformTools.UpdateOrigin();
                    GLContext.ActiveContext.UpdateViewport = true;
                }
                ImGui.InputFloat3("Scale", ref actor.Scale);
                if (ImGui.IsItemEdited())
                {
                    obj.Transform.Scale = GetObjScale(actor);
                    obj.Transform.UpdateMatrix(true);
                    GLContext.ActiveContext.TransformTools.UpdateOrigin();
                    GLContext.ActiveContext.UpdateViewport = true;
                }
            }

            if (ImGui.CollapsingHeader("Info", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText("Hash", ref actor.Hash, 20, ImGuiInputTextFlags.ReadOnly);
                ImGui.SameLine();
                if (ImGui.Button(IconManager.COPY_ICON.ToString() + "##HashCopyButton", buttonIconSize))
                    copiedHash = actor.Hash;

                ImGui.InputText("Type", ref actor.Name, 64, ImGuiInputTextFlags.ReadOnly);
                ImGui.SameLine();
                if (ImGui.Button(IconManager.EDIT_ICON.ToString() + "##TypeEditButton", buttonIconSize))
                    ChangeObj();
            }

            if (ImGui.CollapsingHeader("Parameters", ImGuiTreeNodeFlags.DefaultOpen))
                actor.Parameters.DrawParameters();

            if (ImGui.CollapsingHeader("Switches (Flags)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                string[] items = new string[5] {
                    "Local Flag",
                    "Global Flag",
                    "Hardcoded Value",
                    "Panel Flag",
                    "Unused"
                };

                ImGui.Combo("##Flag1Usage", ref actor.Flags[0].Usage, items, items.Length);
                ImGui.SameLine();
                ImGui.InputInt("##Flag1Index", ref actor.Flags[0].Index);

                ImGui.Combo("##Flag2Usage", ref actor.Flags[1].Usage, items, items.Length);
                ImGui.SameLine();
                ImGui.InputInt("##Flag2Index", ref actor.Flags[1].Index);

                ImGui.Combo("##Flag3Usage", ref actor.Flags[2].Usage, items, items.Length);
                ImGui.SameLine();
                ImGui.InputInt("##Flag3Index", ref actor.Flags[2].Index);

                ImGui.Combo("##Flag4Usage", ref actor.Flags[3].Usage, items, items.Length);
                ImGui.SameLine();
                ImGui.InputInt("##Flag4Index", ref actor.Flags[3].Index);
            }

            if (ImGui.CollapsingHeader("Links", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // float width = ImGui.GetWindowWidth();
                for (int i = 0; i < actor.Links.Count; i++)//var link in actor.Links)
                {
                    var link = actor.Links[i];
                    ImGui.InputText($"##Hash{i}", ref link.Hash, 20, ImGuiInputTextFlags.ReadOnly);
                    ImGui.SameLine();
                    if (ImGui.Button(IconManager.PASTE_ICON.ToString() + $"##Link{i}", buttonIconSize))
                        actor.Links[i].Hash = copiedHash;
                    ImGui.SameLine();
                    if (ImGui.Button(IconManager.DELETE_ICON.ToString() + $"##Link{i}", buttonIconSize))
                        actor.Links.Remove(link);

                    // Most things do not use these link parameters, so lets leave it out for simplicity for now
                    ImGui.PushItemWidth(width / 2.5f);
                    ImGui.InputText($"##LinkParameter1{i}", ref link.Parameters[0], 64);
                    ImGui.SameLine();
                    ImGui.InputText($"##LinkParameter2{i}", ref link.Parameters[1], 64);

                    ImGui.PopItemWidth();
                    ImGui.Separator();
                }
                if (ImGui.Button(IconManager.ADD_ICON.ToString() + "##LinkAddButton", buttonIconSize))
                    actor.Links.Add(new ActorLink());
            }

            if (ImGui.CollapsingHeader("Points", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < actor.Points.Count; i++)
                {
                    var point = actor.Points[i];
                    ImGui.InputFloat3($"##Point{i}", ref point.Point);
                    ImGui.SameLine();
                    if (ImGui.Button(IconManager.DELETE_ICON.ToString() + $"##Point{i}", buttonIconSize))
                        actor.Points.Remove(point);

                    // Most things do not use these link parameters, so lets leave it out for simplicity for now
                    ImGui.PushItemWidth(width / 2.5f);
                    ImGui.InputText($"##PointParameter1{i}", ref point.Parameters[0], 64);
                    ImGui.SameLine();
                    ImGui.InputText($"##PointParameter2{i}", ref point.Parameters[1], 64);

                    ImGui.PopItemWidth();
                    ImGui.Separator();
                }
                if (ImGui.Button(IconManager.ADD_ICON.ToString() + "##PointAddButton", buttonIconSize))
                    actor.Points.Add(new ActorObj.ObjPointLink());
            }
        }


        public void DrawLevelProperties()
        {
            // ImGui.InputText("Level Name", ref n)
        }


        public override void DrawHelpWindow()
        {
            if (ImGuiNET.ImGui.CollapsingHeader("Camera", ImGuiNET.ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiHelper.BoldTextLabel("WASD", "Move camera");
                ImGuiHelper.BoldTextLabel("Spacebar", "Move up");
                ImGuiHelper.BoldTextLabel("Spacebar + Shift", "Move down");
                ImGuiHelper.BoldTextLabel("Spacebar + Ctrl", "Focus Viewport");
                ImGuiHelper.BoldTextLabel("MouseWheel", "Zoom in/out");
            }
        }


        private bool HideObjectsWithoutModels = false;
        // private bool ShowHiddenContents = false;

        public override void DrawToolWindow()
        {
            if (ImGui.Checkbox("Hide Objects Without Models", ref HideObjectsWithoutModels))
            {
                foreach (EditableObject render in Scene.Objects)
                {
                    if (render is CustomRender)
                        render.IsVisible = !HideObjectsWithoutModels;
                    else
                        foreach (string sub in hiddenObjs)
                            if (render.UINode.Header.Contains(sub))
                                render.IsVisible = !HideObjectsWithoutModels;
                }
            }

            // if (ImGui.Checkbox("Show Hidden Contents", ref ShowHiddenContents))
            // {
            //     foreach (EditableObject render in Scene.Objects)
            //     {
            //         if (render is not EditableObject)
            //             continue;
            //         // Add new bfres child to show contents of chests/pots/rocks/grass
            //     }
            // }

            // if (ImGui.CollapsingHeader("Actions", ImGuiTreeNodeFlags.DefaultOpen))
            // {
            //     System.Numerics.Vector2 buttonSize = new System.Numerics.Vector2(32, 32);

            //     if (ImGui.Button(IconManager.ADD_ICON.ToString(), buttonSize))
            //     {
            //         if (currentObj != null)
            //             if (currentObj.UINode != Root)
            //                 if (currentObj.UINode.Parent != Root)
            //                     AddObject(new ActorObj((ActorObj)currentObj.UINode.Tag));
            //     }

            //     if (ImGui.Button(IconManager.DUPE_ICON.ToString(), buttonSize))
            //     {
            //         DuplicateCurrentObj();
            //     }

            //     if (ImGui.Button(IconManager.DELETE_ICON.ToString(), buttonSize))
            //     {
            //         DeleteCurrentObj();
            //     }
            // }
        }


        public EditableObject AddObject(ActorObj actor, bool generateHash = true)
        {
            if (generateHash)
            {
                ulong newHash = NextUInt64();
                while (HashList.Contains(newHash))
                    newHash = NextUInt64();
                actor.Hash = newHash.ToString();
            }

            NodeBase roomNode = currentObj;
            if (roomNode.Parent != Root)
                roomNode = currentObj.Parent;

            List<ActorObj> actors = MapObjList[roomNode.Header];
            MapObjList[roomNode.Header].Add(actor);

            // now add the render - same code from MapScene
            string modelPath = GetModelPathFromObject(actor);
            if (File.Exists(modelPath))
            {
                BfresRender o = new BfresRender(modelPath, roomNode);

                string modelPathName = modelPath.Split("\\").Last();
                if (GlobalSettings.TextureArchive.ContainsKey(modelPathName.Split('_')[0]))
                    o.Textures = GlobalSettings.TextureArchive[modelPathName.Split('_')[0]];

                o.Models.ForEach(model =>
                {
                    bool state = true;
                    if (modelPathName.StartsWith("Obj"))
                        if (model != o.Models.Last())
                            state = false;
                    model.IsVisible = state;
                });

                o.UINode.Header = actor.Name;
                o.UINode.Icon = IconManager.MESH_ICON.ToString();
                o.UINode.Tag = actor;
                o.UINode.TagUI.UIDrawer += delegate
                {
                    DrawActorProperties(o);
                };
                foreach (string sub in hiddenObjs)
                {
                    if (actor.Name.Contains(sub))
                    {
                        o.IsVisible = false;
                        break;
                    }
                }
                return o;
            }
            else
            {
                CustomRender o = new CustomRender(roomNode);
                o.UINode.Header = actor.Name;
                o.UINode.Icon = IconManager.MESH_ICON.ToString();
                o.UINode.Tag = actor;
                o.UINode.TagUI.UIDrawer += delegate
                {
                    DrawActorProperties(o);
                };
                return o;
            }
        }


        public ulong NextUInt64()
        {
            var buffer = new byte[sizeof(ulong)];
            RNG.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }


        public void DuplicateObj()
        {

        }


        public void DeleteObj()
        {

        }


        /// <summary>
        /// Edits the Actor Type of the currently selected actor
        /// </summary>
        public void ChangeObj()
        {
            var selected = Scene.GetSelected().ToList();
            if (selected.Count == 0)
                return;

            List<ActorDefinition> actors = GlobalSettings.ActorDatabase.Values.OrderBy(x => x.Name).ToList();
            MapObjectSelector selector = new MapObjectSelector(actors);
            DialogHandler.Show("Select Object", 400, 800, () =>
            {
                selector.Render();
            }, (result) =>
            {
                string newName = selector.GetSelectedID();
                if (!result || string.IsNullOrEmpty(newName))
                    return;

                foreach (EditableObject render in selected)
                {
                    ActorObj actor = (ActorObj)render.UINode.Tag;
                    actor.Change(GlobalSettings.ActorDatabase[newName]);
                    EditObj(render, actor, generateHash: false); // keep hash to avoid breaking links
                }
            });
        }

        /// <summary>
        /// Redraws all the selected objects
        /// </summary>
        public void UpdateObj()
        {
            var selected = Scene.GetSelected().ToList();
            if (selected.Count == 0)
                return;

            foreach (EditableObject render in selected)
                EditObj(render, (ActorObj)render.UINode.Tag, generateHash: false); // keep hash
        }


        /// <summary>
        /// Edits the render to become the given ActorObj
        /// This is done by deleting the old render and creating a new one in its place
        /// </summary>
        public void EditObj(EditableObject render, ActorObj actor, bool generateHash)
        {
            var parentNode = render.ParentUINode;
            int index = render.UINode.Index;

            EditableObject newRender = AddObject(actor, generateHash);
            newRender.Transform.Position = GetObjPos(actor);
            newRender.Transform.RotationEulerDegrees = GetObjRotation(actor);
            newRender.Transform.Scale = GetObjScale(actor);
            newRender.Transform.UpdateMatrix(true);
            newRender.UINode.IsSelected = true;
            currentObj = newRender.UINode;

            RemoveRender(render);
            Root.Children.Remove(render.UINode);
            GLContext.ActiveContext.TransformTools.RemoveTransform(render);

            AddRender(newRender);
            parentNode.Children.Move(parentNode.Children.Count - 1, index);
            Scene.SelectionUIChanged?.Invoke(newRender.UINode, EventArgs.Empty);
            GLContext.ActiveContext.TransformTools.InitAction(new List<ITransformableObject>() { newRender });

            //Update the SRT tool if active
            GLContext.ActiveContext.UpdateViewport = true;
        }
    }
}
