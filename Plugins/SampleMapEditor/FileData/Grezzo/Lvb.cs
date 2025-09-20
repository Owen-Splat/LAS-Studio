using System.Collections.Generic;

namespace SampleMapEditor.FileData.Grezzo
{
    public class Level
    {
        public FixedHash Fixed;
        public Config Config;

        public Level(byte[] data)
        {
            Fixed = new FixedHash(data);

            foreach (var entry in Fixed.Entries)
            {
                switch (entry.Name)
                {
                    case "config":
                        Config = new Config(entry.Data);
                        break;
                }
            }
        }

        public byte[] Repack()
        {
            foreach (var entry in Fixed.Entries)
            {
                if (entry.Name == "config")
                    entry.Data = Config.Repack();
            }
            return Fixed.ToBinary();
        }
    }


    public class Config
    {
        public int LevelType;
        public bool AllowCompanions;

        public Config(byte[] data)
        {
            LevelType = FixedHash.ReadInt(data, 0, 1);
            AllowCompanions = FixedHash.ReadInt(data, 1, 1) != 0;
            // The last 5 bytes always seem to be x00\x00\x00\x00\xff
            // If we ever find out they do something, we can add support for it then
        }

        public byte[] Repack()
        {
            List<byte> packed = new List<byte>
            {
                (byte)LevelType
            };
            packed.AddRange(FixedHash.GetBytes(AllowCompanions));
            packed.AddRange(new byte[] { 0x0, 0x0, 0x0, 0x0, 0xff });
            return packed.ToArray();
        }
    }
}