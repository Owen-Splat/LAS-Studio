using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Toolbox.Core;

namespace MapStudio.UI
{
    public class AssetConfig
    {
        public float IconSize = 70.0f;

        public Dictionary<string, AssetSettings> Settings = new Dictionary<string, AssetSettings>();

        public void AddToFavorites(AssetItem item)
        {
            if (!Settings.ContainsKey(item.ID))
                Settings.Add(item.ID, new AssetSettings());

            if (!item.Favorited)
                item.Categories.Remove("Favorites");
            else
                item.Categories.Add("Favorites");

            Settings[item.ID].Categories = item.Categories.ToArray();
            this.Save();
        }

        public void ApplySettings(AssetItem item)
        {
            if (!Settings.ContainsKey(item.ID))
                return;

            var settings = Settings[item.ID];
            item.Categories = settings.Categories.ToList();
            if (item.Categories.Contains("Favorites"))
                item.Favorited = true;
        }

        public static AssetConfig Load()
        {
            string path = Path.Combine(Runtime.ExecutableDir,"Lib","AssetConfig.json");
            if (!File.Exists(path))
                new AssetConfig().Save();

            return JsonConvert.DeserializeObject<AssetConfig>(File.ReadAllText(path));
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(Runtime.ExecutableDir,"Lib","AssetConfig.json"), json);
        }

        public class AssetSettings
        {
            public string[] Categories = new string[0];
        }
    }
}
