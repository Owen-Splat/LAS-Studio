using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleMapEditor.FileData.Grezzo
{
    public class Actor
    {
        public byte[] Names;
        public ulong Hash;
        public string Name;
        public ushort ID;
        public ushort XE;
        public uint RoomID;
        public float[] Position = new float[3];
        public float[] Rotation = new float[3];
        public float[] Scale = new float[3];
        public string[] Parameters = new string[8];
        public List<Tuple<byte, ushort>> Switches = new List<Tuple<byte, ushort>>();


        public Actor(byte[] data, byte[] names)
        {
            Names = names;
            Hash = FixedHash.ReadInt(data, 0, 8);
            Name = FixedHash.ReadString(names, FixedHash.ReadInt(data, 8, 4));
            Console.WriteLine(Name);
            if (Hash != ulong.Parse(Name.Substring(Name.Length - 0x10), NumberStyles.HexNumber))
                throw new Exception($"Actor hash does not match for actor {Hash} | {Name}");

            ID = FixedHash.ReadInt(data, 0xC, 2);
            XE = FixedHash.ReadInt(data, 0xE, 2);
            RoomID = FixedHash.ReadInt(data, 0x10, 4);
            Position = FixedHash.ReadVector3(data, 0x14);
            Rotation = FixedHash.ReadVector3(data, 0x20);
            Scale = FixedHash.ReadVector3(data, 0x2C);

            for (uint i = 0; i < 8; i++)
            {
                uint param_type = FixedHash.ReadInt(data, 0x38 + (0x8 * i) + 0x4, 4);
                dynamic param;

                if (param_type == 2)
                    param = FixedHash.ReadFloat(data, 0x38 + (0x8 * i));
                else
                    param = FixedHash.ReadInt(data, 0x38 + (0x8 * i), 4);

                if (param_type == 4)
                    Parameters[i] = FixedHash.ReadString(names, param);
                else
                    Parameters[i] = param.ToString();
            }

            for (int i = 0; i < 4; i++)
                Switches.Add(new Tuple<byte, ushort>(FixedHash.ReadInt(data, 0x78 + i, 1), FixedHash.ReadInt(data, 0x7C + (2 * i), 2)));
        }
    }


    public class Room
    {
        public FixedHash Fixed;
        public List<Actor> Actors = new List<Actor>();


        public Room(byte[] data)
        {
            Fixed = new FixedHash(data);
            Entry actorEntry = new Entry();
            foreach (Entry e in Fixed.Entries)
            {
                if (e.Name == "actor")
                {
                    actorEntry = e;
                    break;
                }
            }
            foreach (Entry e in actorEntry.Data.Entries)
                Actors.Add(new Actor(e.Data, Fixed.NamesSection));
        }
    }
}
