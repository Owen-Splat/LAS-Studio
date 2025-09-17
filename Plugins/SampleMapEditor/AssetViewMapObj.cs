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

            var actorList = GlobalSettings.ActorDatabase.Keys.ToList();
            actorList.Sort();

            assets.Clear();
            foreach (string actor in actorList)
                AddAsset(assets, GlobalSettings.ActorDatabase[actor]);

            return assets;
        }

        public void AddAsset(List<AssetItem> assets, ActorDefinition actor)
        {
            string icon = "Node";
            if (IconManager.HasIcon(System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapObjects",$"{actor.Name}.png")))
                icon = System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapObjects",$"{actor.Name}.png");

            MapObjectAsset asset = new MapObjectAsset($"MapObject_{actor.Name}")
            {
                Name = actor.Name,
                ObjDefinition = actor,
                Icon = IconManager.GetTextureIcon(icon)
            };

            if (actor.Name.StartsWith("Enemy"))
                asset.Categories = new string[] { "Enemies" };
            else if (actor.Name.StartsWith("Item"))
                asset.Categories = new string[] { "Items" };
            else if (actor.Name.StartsWith("Npc"))
                asset.Categories = new string[] { "NPCs" };
            else if (actor.Name.StartsWith("Obj"))
                asset.Categories = new string[] { "Objects" };

            assets.Add(asset);
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
        public ActorDefinition ObjDefinition { get; set; }

        public MapObjectAsset(string id) : base(id)
        {

        }
    }
}
