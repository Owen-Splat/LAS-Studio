using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Toolbox.Core;

namespace SampleMapEditor
{
    [JsonObject]
    public class ActorDefinition
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        [JsonProperty]
        [BindGUI("ID")]
        public ushort ID
        {
            get => id;
            set => SetField(ref id, value);
        }
        private ushort id;


        [JsonProperty]
        [BindGUI("Name")]
        public string Name
        {
            get => name;
            set => SetField(ref name, value);
        }
        private string name;


        [JsonProperty]
        [BindGUI("Model")]
        public string Model
        {
            get => model;
            set => SetField(ref model, value);
        }
        private string model;


        public override string ToString() => $"{Name}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
