using CafeLibrary;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;

namespace SampleMapEditor
{
    public class ActorDefinitionDb
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public ActorDefinitionDb(string fileName)
        {
            using (StreamReader stream = new StreamReader(fileName))
            {
                Load(stream);
            }
        }


        // ---- PROPERTIES ---------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Gets or sets the list of <see cref="ActorDefinition"/> instances in this database.
        /// </summary>
        public List<ActorDefinition> Definitions
        {
            get;
            set;
        }

        private static string FindFilePath(string resName)
        {
            string resFilePath = GlobalSettings.GetContentPath($"Pack\\Actor\\{resName}.pack.zs");

            if (System.IO.File.Exists(resFilePath)) return resFilePath;

            return "";
        }

        private void Load(StreamReader stream)
        {
            Definitions = new List<ActorDefinition>();

            while (stream.Peek() >= 0)
            {
                string[] actorInfo = stream.ReadLine().Split('|');
                ActorDefinition def = new ActorDefinition();
                def.ID = ushort.Parse(actorInfo[0].Strip());
                def.Name = actorInfo[1].Strip();
                def.Model = actorInfo[2].Strip();
                Definitions.Add(def);
            }

            Console.WriteLine("Finished loading Actors.txt");
        }
    }
}
