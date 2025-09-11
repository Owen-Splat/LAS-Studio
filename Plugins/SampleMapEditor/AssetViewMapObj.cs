using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapStudio.UI;
using ImGuiNET;
using Toolbox.Core;

namespace SampleMapEditor
{
    /// <summary>
    /// Represemts an asset view of map objects to preview, drag and drop objects into the scene.
    /// </summary>
    public class AssetViewMapObject : IAssetLoader
    {
        public virtual string Name => TranslationSource.GetText("Map Objects");

        public bool IsFilterMode => filterObjPath;

        public static bool filterObjPath = false;

        public virtual List<AssetItem> Reload()
        {
            List<AssetItem> assets = new List<AssetItem>();

            var actorList = GlobalSettings.ActorDatabase.Values.ToList();

            assets.Clear();
            foreach (ActorDefinition actor in actorList)
                AddAsset(assets, actor);

            return assets;
        }

        public void AddAsset(List<AssetItem> assets, ActorDefinition actor)
        {
            string icon = "Node";
            if (IconManager.HasIcon(System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapObjects",$"{actor.Name}.png")))
                icon = System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapObjects",$"{actor.Name}.png");

            assets.Add(new MapObjectAsset($"MapObject_{actor.ID}")
            {
                Name = actor.Name,
                ObjID = actor.ID,
                ObjDefinition = actor,
                Icon = IconManager.GetTextureIcon(icon),
            });
        }

        public bool UpdateFilterList()
        {
            bool filterUpdate = false;
            if (ImGui.Checkbox(TranslationSource.GetText("FILTER_PATH_OBJS"), ref filterObjPath))
                filterUpdate = true;

            return filterUpdate;
        }
    }

    public class MapObjectAsset : AssetItem
    {
        public int ObjID { get; set; }
        public ActorDefinition ObjDefinition { get; set; }

        public override void DoubleClicked()
        {
            return;
            // string filePath = Obj.FindFilePath(Obj.GetResourceName(ObjID));
            // FileUtility.SelectFile(filePath);
        }

        public MapObjectAsset(string id) : base(id)
        {

        }
    }
}
