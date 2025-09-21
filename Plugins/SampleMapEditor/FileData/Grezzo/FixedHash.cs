using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SampleMapEditor.FileData.Grezzo
{
    public class Entry
    {
        public ushort NodeIndex;
        public string Name;
        public uint NextOffset;
        public dynamic Data;


        public Entry(ushort nodeIndex, string name, uint nextOffset, byte[] data)
        {
            NodeIndex = nodeIndex;
            Name = name;
            NextOffset = nextOffset;
            Data = data;
        }

        public Entry(ushort nodeIndex, string name, uint nextOffset, FixedHash data)
        {
            NodeIndex = nodeIndex;
            Name = name;
            NextOffset = nextOffset;
            Data = data;
        }

        public Entry()
        {

        }
    }


    public class FixedHash
    {
        public byte Magic = 0x3D;
        public byte Version = 1;
        public ushort NumBuckets;
        public ushort NumNodes;
        public ushort X6 = 0;
        public List<uint> Buckets = new List<uint>();
        public byte[] NamesSection;
        public List<Entry> Entries = new List<Entry>();


        public FixedHash(byte[] data, ulong offset=0)
        {
            Magic = ReadInt(data, offset, 1);
            Version = ReadInt(data, offset + 0x1, 1);
            NumBuckets = ReadInt(data, offset + 0x2, 2);
            NumNodes = ReadInt(data, offset + 0x4, 2);
            X6 = ReadInt(data, offset + 0x6, 2);

            for (ulong i = 0; i < NumBuckets; i++)
                Buckets.Add(ReadInt(data, offset + 0x8 + (i * 4), 4));

            uint entriesOffset = (uint)((int)(offset + 0x8 + (ulong)(4 * (NumBuckets + 1)) + 3) & -8) + 8;
            ulong numEntries = ReadInt(data, entriesOffset - 8, 8) / 0x10;

            ulong entryOffsetsOffset = entriesOffset + (numEntries * 0x10) + 8;

            ulong dataSectionOffset = (ulong)((int)(entryOffsetsOffset + (4 * numEntries) + 7) & -8) + 8;

            ulong namesSectionOffset = (ulong)((int)(dataSectionOffset + ReadInt(data, dataSectionOffset - 8, 8) + 3) & -4) + 4;
            uint namesSize = ReadInt(data, namesSectionOffset - 4, 4);
            NamesSection = new byte[namesSize];
            Buffer.BlockCopy(data, (int)namesSectionOffset, NamesSection, 0, (int)namesSize);

            for (ulong i = 0; i < numEntries; i++)
            {
                ulong currentOffset = entriesOffset + (i * 0x10);

                ushort nodeIndex = ReadInt(data, currentOffset, 2);
                uint nextOffset = ReadInt(data, currentOffset + 8, 4);

                string name = "";
                if (namesSize > 0)
                    name = ReadString(data, namesSectionOffset + ReadInt(data, currentOffset + 2, 2));

                uint entryDataOffset = ReadInt(data, currentOffset + 0xC, 4);

                dynamic entryData;
                if (nodeIndex <= 0xFFED)
                    entryData = new FixedHash(data, dataSectionOffset + entryDataOffset);
                else if (nodeIndex >= 0xFFF0)
                {
                    ulong dataSize = ReadInt(data, dataSectionOffset + entryDataOffset, 8);
                    entryData = new byte[dataSize];
                    Buffer.BlockCopy(data, (int)(dataSectionOffset + entryDataOffset + 8), entryData, 0, (int)dataSize);
                }
                else
                    throw new Exception("Invalid node index!");

                Entries.Add(new Entry(nodeIndex, name, nextOffset, entryData));
            }
        }


        public byte[] ToBinary(ulong offset = 0)
        {
            List<byte> intro = new List<byte>() { Magic, Version };
            intro.AddRange(GetBytes(NumBuckets));
            intro.AddRange(GetBytes(NumNodes));
            intro.AddRange(GetBytes(X6));
            foreach (uint bucket in Buckets)
                intro.AddRange(GetBytes(bucket));

            List<byte> entriesSect = new List<byte>();
            entriesSect.AddRange(GetBytes((ulong)(Entries.Count * 0x10)));
            List<byte> entryOffsetsSect = new List<byte>();
            entryOffsetsSect.AddRange(GetBytes((ulong)(Entries.Count * 0x4)));
            List<byte> dataSect = new List<byte>();

            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                entriesSect.AddRange(GetBytes(entry.NodeIndex));
                int subIndex = IndexInByteArray(NamesSection, GetBytes(entry.Name));
                if (subIndex != -1 && NamesSection.Length > 0)
                    entriesSect.AddRange(GetBytes((ushort)subIndex));
                else
                    entriesSect.AddRange(new byte[] { 0, 0 });
                entriesSect.AddRange(GetBytes(HashString(entry.Name)));
                entriesSect.AddRange(GetBytes(entry.NextOffset));
                entriesSect.AddRange(GetBytes((uint)dataSect.Count));

                entryOffsetsSect.AddRange(GetBytes((uint)(i * 0x10)));

                if (entry.NodeIndex <= 0xFFED)
                    dataSect.AddRange(entry.Data.ToBinary((ulong)dataSect.Count)); // entry Data is another FixedHash so call ToBinary()
                else if (entry.NodeIndex >= 0xFFF0) // entry Data is byte[] so add it directly
                {
                    dataSect.AddRange(GetBytes((ulong)entry.Data.Length));
                    dataSect.AddRange(entry.Data);
                    while (dataSect.Count % 8 != 0)
                        dataSect.Add(0);
                    // dataSect.AddRange(new byte[7] { 0, 0, 0, 0, 0, 0, 0 });
                    // int maxSize = dataSect.Count & -8;
                    // int extraCount = dataSect.Count - maxSize;
                    // dataSect.RemoveRange(maxSize - 1, extraCount);
                }
                else
                    throw new ArgumentException("Invalid node index");
            }

            dataSect.InsertRange(0, GetBytes((ulong)dataSect.Count));

            List<byte> result = new List<byte>();
            result.AddRange(intro);

            while (((ulong)result.Count + offset) % 8 != 0)
                result.Add(0);
            result.AddRange(entriesSect);

            while (((ulong)result.Count + offset) % 8 != 0)
                result.Add(0);
            result.AddRange(entryOffsetsSect);

            while (((ulong)result.Count + offset) % 8 != 0)
                result.Add(0);
            result.AddRange(dataSect);

            while (((ulong)result.Count + offset) % 4 != 0)
                result.Add(0);
            result.AddRange(GetBytes((uint)NamesSection.Length));
            result.AddRange(NamesSection);

            return result.ToArray();
        }


        public static byte[] GetBytes(object value)
        {
            byte[] data;

            if (value is string)
                data = Encoding.UTF8.GetBytes((string)value);
            else if (value is ushort)
                data = BitConverter.GetBytes((ushort)value);
            else if (value is uint)
                data = BitConverter.GetBytes((uint)value);
            else if (value is ulong)
                data = BitConverter.GetBytes((ulong)value);
            else if (value is float)
                data = BitConverter.GetBytes((float)value);
            else if (value is bool)
                data = BitConverter.GetBytes((bool)value);
            else
            {
                throw new Exception($"FixedHash.GetBytes unknown value: {value.GetType()}");
            }

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return data;
        }

        static int IndexInByteArray(byte[] haystack, byte[] needle)
        {
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
                if (CheckBytesInArray(haystack, needle, i))
                    return i;
            return -1;
        }

        public static bool CheckBytesInArray(byte[] haystack, byte[] needle, int start)
        {
            if (needle.Length + start > haystack.Length)
                return false;
            else
            {
                for (int i = 0; i < needle.Length; i++)
                    if (needle[i] != haystack[i + start])
                        return false;
                return true;
            }
        }

        public static dynamic ReadInt(byte[] data, dynamic start, int size)
        {
            dynamic num = 0;
            switch (size)
            {
                case 1:
                    num = data[start];
                    break;
                case 2:
                    num = BitConverter.ToUInt16(data, (int)start);
                    break;
                case 4:
                    num = BitConverter.ToUInt32(data, (int)start);
                    break;
                case 8:
                    num = BitConverter.ToUInt64(data, (int)start);
                    break;
            }
            return num;
        }

        public static float ReadFloat(byte[] data, dynamic start)
        {
            return BitConverter.ToSingle(data, (int)start);
        }

        public static Vector3 ReadVector3(byte[] data, dynamic start)
        {
            Vector3 vector = new Vector3(
                ReadFloat(data, start),
                ReadFloat(data, start + 4),
                ReadFloat(data, start + 8));
            return vector;
        }
        
        public static string ReadString(byte[] data, dynamic start)
        {
            string result = "";
            for (ulong i = start; i < (ulong)data.Length && data[i] != 0; i++)
                result += (char)data[i];
            return result;
        }

        public uint HashString(string s)
        {
            List<byte> data = new List<byte>();
            data.AddRange(GetBytes(s));
            uint h = 0;
            for (int i = 0; i < data.Count; i++)
                h ^= (data[i] + (h >> 2) + (h << 5)) & 0xFFFFFFFF;
            return h;
        }
    }
}
