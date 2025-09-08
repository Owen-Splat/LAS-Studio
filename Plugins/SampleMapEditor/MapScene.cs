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
    internal class MapScene
    {
        public void Setup(EditorLoader loader)
        {
            //Prepare a collision caster for snapping objects onto
            SetupSceneCollision();

            //Add some objects to the scene
            SetupObjects(loader);
        }

        /// <summary>
        /// Adds objects to the scene.
        /// </summary>
        private void SetupObjects(EditorLoader loader)
        {
            List<string> hiddenObjs = new List<string>()
            {
                "Area",
                "Roof",
                "Tag"
            };

            /*List<BfshaFile> shaders = new List<BfshaFile>();
            string[] shaderFiles = Directory.GetFiles($"{PluginConfig.GamePath}\\region_common\\shader");
            foreach (string shaderFile in shaderFiles)
            {
                if (shaderFile.EndsWith(".bntx"))
                    continue;
                shaders.Add(new BfshaFile(shaderFile));
            }*/

            Dictionary<string, GenericRenderer.TextureView> textureArchive = new Dictionary<string, GenericRenderer.TextureView>();
            if (loader.FileInfo.FileName.StartsWith("Lv") || loader.FileInfo.FileName.StartsWith("Field"))
            {
                string levelName = loader.FileInfo.FileName.Split('.')[0];
                string bntxPath = loader.GetTextureArchive(levelName);
                BntxFile bntx = new BntxFile(bntxPath);
                foreach (Texture tex in bntx.Textures)
                {
                    BntxTexture btex = new BntxTexture(bntx, tex);
                    textureArchive.Add(btex.Name, new GenericRenderer.TextureView(btex) { OriginalSource = btex });
                }
            }


            foreach (var roomObj in loader.MapObjList)
            {
                NodeBase roomFolder = new NodeBase(roomObj.Key);
                roomFolder.Icon = IconManager.FOLDER_ICON.ToString();
                roomFolder.HasCheckBox = true;
                loader.Root.AddChild(roomFolder);

                foreach (var mapObj in roomObj.Value)
                {
                    string modelPath = loader.GetModelPathFromObject(roomObj.Key, mapObj);

                    if (File.Exists(modelPath))
                    {
                        BfresRender o = new BfresRender(modelPath, roomFolder);

                        /*foreach (BfshaFile shader in shaders)
                        {
                            o.ShaderFiles.Add(shader);
                        }*/

                        string modelPathName = modelPath.Split("\\").Last();
                        if (modelPathName.StartsWith("Lv") || modelPathName.StartsWith("Field"))
                            o.Textures = textureArchive;

                        ModelAsset lastModel = o.Models.Last();
                        o.Models.ForEach(model =>
                        {
                            bool state = true;
                            if (modelPathName.StartsWith("Obj"))
                                if (model != lastModel)
                                    state = false;
                            model.IsVisible = state;
                        });

                        //objFolder.AddChild(o.UINode);
                        o.UINode.Header = mapObj.Name;
                        o.UINode.Icon = IconManager.MESH_ICON.ToString();
                        o.Transform.Position = EditorLoader.GetObjPos(mapObj);
                        o.Transform.Scale = EditorLoader.GetObjScale(mapObj);
                        o.Transform.RotationEulerDegrees = EditorLoader.GetObjRotation(mapObj);
                        o.Transform.UpdateMatrix(true);
                        foreach (string sub in hiddenObjs)
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
                        TransformableObject o = new TransformableObject(roomFolder);
                        //CustomBoundingBoxRender o = new CustomBoundingBoxRender(objFolder);
                        o.UINode.Header = mapObj.Name;
                        o.UINode.Icon = IconManager.MESH_ICON.ToString();
                        o.Transform.Position = EditorLoader.GetObjPos(mapObj);
                        o.Transform.Scale = EditorLoader.GetObjScaleTiny(mapObj);
                        o.Transform.RotationEulerDegrees = EditorLoader.GetObjRotation(mapObj);
                        //o.Color = new Vector4(0.5F, 0.5F, 0.5F, 0.5F);
                        o.Transform.UpdateMatrix(true);
                        o.IsVisible = false;
                        loader.AddRender(o);
                    }
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
