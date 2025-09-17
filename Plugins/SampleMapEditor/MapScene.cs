using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using CafeLibrary;
using CafeLibrary.Rendering;
using OpenTK;
using Toolbox.Core.IO;
using Toolbox.Core.ViewModels;
using MapStudio.UI;
using Syroot.NintenTools.NSW.Bntx;
using Toolbox.Core;
using BfshaLibrary;

namespace SampleMapEditor
{
    public class MapScene
    {
        public void Setup(EditorLoader loader)
        {
            //Prepare a collision caster for snapping objects onto
            SetupSceneCollision();

            //Prepare Texture Archive
            SetupTextures(loader);

            //Add some objects to the scene
            SetupObjects(loader);
        }

        /// <summary>
        /// Adds objects to the scene.
        /// </summary>
        private void SetupObjects(EditorLoader loader)
        {
            foreach (var roomObj in loader.MapObjList)
            {
                NodeBase roomFolder = new NodeBase(roomObj.Key);
                roomFolder.Icon = IconManager.FOLDER_ICON.ToString();
                roomFolder.HasCheckBox = true;
                loader.Root.AddChild(roomFolder);

                foreach (var mapObj in roomObj.Value)
                {
                    string modelPath = loader.GetModelPathFromObject(mapObj);
                    if (File.Exists(modelPath))
                    {
                        BfresRender o = new BfresRender(modelPath, roomFolder);

                        string modelPathName = modelPath.Split("\\").Last();
                        if (modelPathName.StartsWith("Lv") || modelPathName.StartsWith("Field"))
                            o.Textures = loader.textureArchive;

                        o.Models.ForEach(model =>
                        {
                            bool state = true;
                            if (modelPathName.StartsWith("Obj"))
                                if (model != o.Models.Last())
                                    state = false;
                            model.IsVisible = state;
                        });

                        o.UINode.Header = mapObj.Name;
                        o.UINode.Icon = IconManager.MESH_ICON.ToString();
                        o.UINode.Tag = mapObj;
                        o.UINode.TagUI.UIDrawer += delegate
                        {
                            loader.DrawActorProperties(o);
                        };
                        o.Transform.Position = EditorLoader.GetObjPos(mapObj);
                        o.Transform.Scale = EditorLoader.GetObjScale(mapObj);
                        o.Transform.RotationEulerDegrees = EditorLoader.GetObjRotation(mapObj);
                        o.Transform.UpdateMatrix(true);
                        foreach (string sub in loader.hiddenObjs)
                        {
                            if (mapObj.Name.Contains(sub))
                            {
                                o.IsVisible = false;
                                break;
                            }
                        }
                        loader.AddRender(o);
                    }
                    else
                    {
                        CustomRender o = new CustomRender(roomFolder);
                        o.UINode.Header = mapObj.Name;
                        o.UINode.Icon = IconManager.MESH_ICON.ToString();
                        o.UINode.Tag = mapObj;
                        o.UINode.TagUI.UIDrawer += delegate
                        {
                            loader.DrawActorProperties(o);
                        };
                        o.Transform.Position = EditorLoader.GetObjPos(mapObj);
                        o.Transform.Scale = EditorLoader.GetObjScale(mapObj);
                        o.Transform.RotationEulerDegrees = EditorLoader.GetObjRotation(mapObj);
                        o.Transform.UpdateMatrix(true);
                        loader.AddRender(o);
                    }
                }
            }
        }


        private void SetupTextures(EditorLoader loader)
        {
            if (loader.FileInfo.FileName.StartsWith("Lv") || loader.FileInfo.FileName.StartsWith("Field"))
            {
                string levelName = loader.FileInfo.FileName.Split('.')[0];
                string bntxPath = loader.GetTextureArchive(levelName);
                BntxFile bntx = new BntxFile(bntxPath);
                foreach (Texture tex in bntx.Textures)
                {
                    BntxTexture btex = new BntxTexture(bntx, tex);
                    loader.textureArchive.Add(btex.Name, new GenericRenderer.TextureView(btex) { OriginalSource = btex });
                }
            }
            else if (loader.FileInfo.FileName.StartsWith("End") || loader.FileInfo.FileName == "KanaletCastle.lvb")
            {
                string levelName = "Field"; // The "Ending" level files just use the same Field map models
                string bntxPath = loader.GetTextureArchive(levelName);
                BntxFile bntx = new BntxFile(bntxPath);
                foreach (Texture tex in bntx.Textures)
                {
                    BntxTexture btex = new BntxTexture(bntx, tex);
                    loader.textureArchive.Add(btex.Name, new GenericRenderer.TextureView(btex) { OriginalSource = btex });
                }
            }
        }


        /// <summary>
        /// Creates a big plane which you can drop objects onto.
        /// </summary>
        private void SetupSceneCollision()
        {
            var context = GLContext.ActiveContext;

            float size = 2000;
            float height = 0;

            //Make a big flat plane for placing spaces on.
            context.CollisionCaster.Clear();
            context.CollisionCaster.AddTri(
                new Vector3(-size, height, size),
                new Vector3(0, height, -(size * 2)),
                new Vector3(size * 2, height, 0));
            context.CollisionCaster.AddTri(
                new Vector3(-size, height, -size),
                new Vector3(size * 2, height, 0),
                new Vector3(size * 2, height, size * 2));
            context.CollisionCaster.UpdateCache();
        }
    }
}
