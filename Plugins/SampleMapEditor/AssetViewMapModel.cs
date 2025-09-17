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
    public class AssetViewMapModel : IAssetLoader
    {
        public virtual string Name => TranslationSource.GetText("Map Models");

        public bool IsFilterMode => filterObjPath;

        public static bool filterObjPath = false;

        public virtual List<AssetItem> Reload()
        {
            List<AssetItem> assets = new List<AssetItem>();

            assets.Clear();
            foreach (string roomName in GlobalSettings.RoomDatabase)
                AddAsset(assets, roomName);

            return assets;
        }

        public void AddAsset(List<AssetItem> assets, string roomName)
        {
            string icon = "Node";
            if (IconManager.HasIcon(System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapModels",$"{roomName}.png")))
                icon = System.IO.Path.Combine(Runtime.ExecutableDir,"Lib","Images","MapModels",$"{roomName}.png");

            MapObjectAsset asset = new MapObjectAsset($"MapObject_{roomName}")
            {
                Name = roomName,
                Icon = IconManager.GetTextureIcon(icon)
            };

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

    public class MapModelAsset : AssetItem
    {
        public MapModelAsset(string id) : base(id)
        {

        }
    }
}
