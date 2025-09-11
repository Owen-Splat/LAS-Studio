using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SampleMapEditor.FileData.Grezzo
{
    public class Entry
    {
        public ulong NodeIndex;
        public string Name;
        public ulong NextOffset;
        public dynamic Data;


        public Entry(ulong nodeIndex, string name, ulong nextOffset, byte[] data)
        {
            NodeIndex = nodeIndex;
            Name = name;
            NextOffset = nextOffset;
            Data = data;
        }

        public Entry(ulong nodeIndex, string name, ulong nextOffset, FixedHash data)
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
        public ushort X6;
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
                uint nextOffset = ReadInt(data, currentOffset + 2, 4);

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


        public byte[] ToBinary()
        {
            return new byte[0];
        }


        public static dynamic ReadInt(byte[] data, dynamic start, int size)
        {
            dynamic num = 0;
            switch(size)
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

        public static float ReadFloat(byte[] data, ulong start)
        {
            return BitConverter.ToSingle(data, (int)start);
        }

        public static Vector3 ReadVector3(byte[] data, ulong start)
        {
            Vector3 vector = new Vector3(
                ReadFloat(data, start),
                ReadFloat(data, start + 4),
                ReadFloat(data, start + 8));
            return vector;
        }
        
        public static string ReadString(byte[] data, ulong start)
        {
            string result = "";
            for (ulong i = start; i < (ulong)data.Length && data[i] != 0; i++)
            {
                result += (char)data[i];
            }
            return result;
        }

        public ulong HashString(byte[] s)
        {
            byte[] n = Encoding.UTF8.GetBytes("\x00");
            byte[] data = new byte[s.Length + n.Length];
            Buffer.BlockCopy(s, 0, data, 0, s.Length);
            Buffer.BlockCopy(n, 0, data, s.Length, n.Length);
            ulong h = 0;
            int i = 0;
            while (data[i] != 0)
            {
                h ^= (data[i] + (h >> 2) + (h << 5)) & 0xFFFFFFFF;
                i++;
            }
            return h;
        }
    }
}
