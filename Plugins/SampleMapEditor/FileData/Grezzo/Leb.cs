using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SampleMapEditor;

namespace SampleMapEditor.FileData.Grezzo
{
    public class Actor
    {
        public string Name;
        public ulong Hash;
        public ushort ID;
        public uint ZoneID;
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public string[] Parameters = new string[8];
        public ActorSwitch[] Switches = new ActorSwitch[4];
        public List<ActorLink> Links = new List<ActorLink>();
        public List<PointLink> Points = new List<PointLink>();
        public List<uint> Refs = new List<uint>();

        public Actor(byte[] data, byte[] names)
        {
            Hash = FixedHash.ReadInt(data, 0, 8);
            ID = FixedHash.ReadInt(data, 0xC, 2);
            ZoneID = FixedHash.ReadInt(data, 0x10, 4);
            Position = FixedHash.ReadVector3(data, 0x14);
            Rotation = FixedHash.ReadVector3(data, 0x20);
            Scale = FixedHash.ReadVector3(data, 0x2C);

            for (int i = 0; i < 8; i++)
                Parameters[i] = ReadParam(data, names, 0x38, i);

            for (int i = 0; i < 4; i++)
                Switches[i] = new ActorSwitch(FixedHash.ReadInt(data, 0x78 + i, 1), FixedHash.ReadInt(data, 0x7C + (2 * i), 2));

            byte numLinks = FixedHash.ReadInt(data, 0x87, 1);
            int pos = 0x90;
            for (int i = 0; i < numLinks; i++)
            {
                string[] linkParameters = new string[2];
                for (int i2 = 0; i2 < 2; i2++)
                    linkParameters[i2] = ReadParam(data, names, pos, i2);
                uint actorIndex = FixedHash.ReadInt(data, pos + 0x10, 4);
                Links.Add(new ActorLink(actorIndex, linkParameters));
                pos += 20;
            }

            byte numPoints = FixedHash.ReadInt(data, 0x89, 1);
            for (int i = 0; i < numPoints; i++)
            {
                string[] pointParameters = new string[2];
                for (int i2 = 0; i2 < 2; i2++)
                    pointParameters[i2] = ReadParam(data, names, pos, i2);
                uint railIndex = FixedHash.ReadInt(data, pos + 0x10, 4);
                uint pointIndex = FixedHash.ReadInt(data, pos + 0x14, 4);
                Points.Add(new PointLink(railIndex, pointIndex, pointParameters));
                pos += 0x18;
            }
        }


        public Actor(EditorLoader.ActorObj obj, Room room)
        {
            Hash = Convert.ToUInt64(obj.Hash);
            ID = obj.ID;
            ZoneID = room.ZoneID;
            Position = obj.Position;
            Rotation = obj.Rotation;
            Scale = obj.Scale;
            Parameters = obj.Parameters.GetParameters();
            Switches = obj.Flags;

            // No need to add any rails or points if the object does not reference any points
            if (obj.Points.Count == 0)
                return;

            // Add each point to the room first so we can index to them
            List<Vector3> points = new List<Vector3>();
            foreach (var point in obj.Points)
            {
                if (!room.Points.Contains(point.Point))
                    room.Points.Add(point.Point);
                points.Add(point.Point);
            }

            // Now create a new rail for each actor that uses points
            room.Rails.Add(new Rail(points, room));

            // Now we can index the last rail and any points
            foreach (var point in obj.Points)
            {
                PointLink pointLink = new PointLink();
                pointLink.Parameters = point.Parameters;
                pointLink.RailIndex = room.Rails.Count - 1;
                pointLink.PointIndex = room.Points.IndexOf(point.Point);
                Points.Add(pointLink);
            }
        }


        private static string ReadParam(byte[] data, byte[] names, int start, int paramIndex)
        {
            uint paramType = FixedHash.ReadInt(data, start + (8 * paramIndex) + 4, 4);
            dynamic param;

            if (paramType == 2)
                param = FixedHash.ReadFloat(data, start + (8 * paramIndex));
            else
                param = FixedHash.ReadInt(data, start + (8 * paramIndex), 4);

            if (paramType == 4 || paramType == 0xFFFFFF04)
                return FixedHash.ReadString(names, param);
            else
                return param.ToString();
        }


        public static TypeCode GetParamType(string parameter)
        {
            float pf;
            int pi;

            if (parameter.Contains('.') && float.TryParse(parameter, out pf))
                return TypeCode.Single;
            else if (int.TryParse(parameter, out pi))
            {
                if (pi < 0)
                    return TypeCode.Single;
                return TypeCode.UInt32;
            }
            else
                return TypeCode.String;
        }


        public byte[] Repack(uint nameOffset)
        {
            List<byte> packed = new List<byte>();
            List<byte> nameRepr = new List<byte>();

            // Only the hex part of the name matters, so we can change the first half to "Actor" to slightly trim down file size

            string hashHex = Hash.ToString("X");
            while (hashHex.Length < 16)
                hashHex.Insert(0, "0");
            Name = $"Actor-{hashHex}";
            nameRepr.AddRange(FixedHash.GetBytes(Name));
            nameRepr.Add(0);

            packed.AddRange(FixedHash.GetBytes(Hash));
            packed.AddRange(FixedHash.GetBytes(nameOffset));
            packed.AddRange(FixedHash.GetBytes(ID));
            packed.AddRange(new byte[2] { 0, 0 }); // 0xE padding
            packed.AddRange(FixedHash.GetBytes(ZoneID));
            packed.AddRange(FixedHash.GetBytes(Position.X));
            packed.AddRange(FixedHash.GetBytes(Position.Y));
            packed.AddRange(FixedHash.GetBytes(Position.Z));
            packed.AddRange(FixedHash.GetBytes(Rotation.X));
            packed.AddRange(FixedHash.GetBytes(Rotation.Y));
            packed.AddRange(FixedHash.GetBytes(Rotation.Z));
            packed.AddRange(FixedHash.GetBytes(Scale.X));
            packed.AddRange(FixedHash.GetBytes(Scale.Y));
            packed.AddRange(FixedHash.GetBytes(Scale.Z));

            for (int i = 0; i < 8; i++)
            {
                string parameter = Parameters[i];
                TypeCode code = GetParamType(parameter);
                switch (code)
                {
                    case TypeCode.Single:
                        packed.AddRange(FixedHash.GetBytes(float.Parse(parameter)));
                        packed.AddRange(FixedHash.GetBytes((uint)2));
                        break;
                    case TypeCode.UInt32:
                        packed.AddRange(FixedHash.GetBytes(uint.Parse(parameter)));
                        packed.AddRange(FixedHash.GetBytes((uint)3));
                        break;
                    default: // TypeCode.String:
                        packed.AddRange(FixedHash.GetBytes((uint)(nameRepr.Count + nameOffset)));
                        packed.AddRange(FixedHash.GetBytes((uint)4));
                        nameRepr.AddRange(FixedHash.GetBytes(parameter));
                        nameRepr.Add(0);
                        break;
                }
            }

            List<byte> switchesBytes = new List<byte>();
            for (int i = 0; i < 4; i++)
            {
                packed.Add((byte)Switches[i].Usage);
                switchesBytes.AddRange(FixedHash.GetBytes((ushort)Switches[i].Index));
            }
            packed.AddRange(switchesBytes.ToArray());

            // relationship data here
            bool isEnemy = Name.StartsWith("Enemy");
            bool checkKills = Name.EndsWith("HolocaustChecker");
            bool isChamberEnemy = false; // So far, adding panel enemies works without needing to set this
            packed.AddRange(FixedHash.GetBytes(isEnemy));
            packed.AddRange(FixedHash.GetBytes(checkKills));
            packed.AddRange(FixedHash.GetBytes(isChamberEnemy));
            packed.Add((byte)Links.Count);
            packed.Add((byte)Refs.Count);
            packed.Add((byte)Points.Count);
            packed.AddRange(new byte[6] { 0, 0, 0, 0, 0, 0 });

            foreach (var link in Links)
            {
                // link param 1
                string param1 = link.Parameters[0];
                TypeCode code = GetParamType(param1);
                switch (code)
                {
                    case TypeCode.Single:
                        packed.AddRange(FixedHash.GetBytes(float.Parse(param1)));
                        packed.AddRange(FixedHash.GetBytes((uint)2));
                        break;
                    case TypeCode.UInt32:
                        packed.AddRange(FixedHash.GetBytes(uint.Parse(param1)));
                        packed.AddRange(FixedHash.GetBytes((uint)3));
                        break;
                    case TypeCode.String:
                        packed.AddRange(FixedHash.GetBytes((uint)(nameRepr.Count + nameOffset)));
                        packed.AddRange(FixedHash.GetBytes((uint)4));
                        nameRepr.AddRange(FixedHash.GetBytes(param1));
                        nameRepr.Add(0);
                        break;
                }

                // link param 2
                string param2 = link.Parameters[1];
                code = GetParamType(param2);
                switch (code)
                {
                    case TypeCode.Single:
                        packed.AddRange(FixedHash.GetBytes(float.Parse(param2)));
                        packed.AddRange(FixedHash.GetBytes((uint)2));
                        break;
                    case TypeCode.UInt32:
                        packed.AddRange(FixedHash.GetBytes(uint.Parse(param2)));
                        packed.AddRange(FixedHash.GetBytes((uint)3));
                        break;
                    case TypeCode.String:
                        packed.AddRange(FixedHash.GetBytes((uint)(nameRepr.Count + nameOffset)));
                        packed.AddRange(FixedHash.GetBytes((uint)4));
                        nameRepr.AddRange(FixedHash.GetBytes(param2));
                        nameRepr.Add(0);
                        break;
                }

                packed.AddRange(FixedHash.GetBytes((uint)link.Index));
            }

            foreach (var point in Points)
            {
                // point param 1
                string param1 = point.Parameters[0];
                TypeCode code = GetParamType(param1);
                switch (code)
                {
                    case TypeCode.Single:
                        packed.AddRange(FixedHash.GetBytes(float.Parse(param1)));
                        packed.AddRange(FixedHash.GetBytes((uint)2));
                        break;
                    case TypeCode.UInt32:
                        packed.AddRange(FixedHash.GetBytes(uint.Parse(param1)));
                        packed.AddRange(FixedHash.GetBytes((uint)3));
                        break;
                    case TypeCode.String:
                        packed.AddRange(FixedHash.GetBytes((uint)(nameRepr.Count + nameOffset)));
                        packed.AddRange(FixedHash.GetBytes((uint)4));
                        nameRepr.AddRange(FixedHash.GetBytes(param1));
                        nameRepr.Add(0);
                        break;
                }

                // point param 2
                string param2 = point.Parameters[1];
                code = GetParamType(param2);
                switch (code)
                {
                    case TypeCode.Single:
                        packed.AddRange(FixedHash.GetBytes(float.Parse(param2)));
                        packed.AddRange(FixedHash.GetBytes((uint)2));
                        break;
                    case TypeCode.UInt32:
                        packed.AddRange(FixedHash.GetBytes(uint.Parse(param2)));
                        packed.AddRange(FixedHash.GetBytes((uint)3));
                        break;
                    case TypeCode.String:
                        packed.AddRange(FixedHash.GetBytes((uint)(nameRepr.Count + nameOffset)));
                        packed.AddRange(FixedHash.GetBytes((uint)4));
                        nameRepr.AddRange(FixedHash.GetBytes(param2));
                        nameRepr.Add(0);
                        break;
                }

                packed.AddRange(FixedHash.GetBytes((uint)point.RailIndex));
                packed.AddRange(FixedHash.GetBytes((uint)point.PointIndex));
            }

            foreach (var actorRef in Refs)
                packed.AddRange(FixedHash.GetBytes(actorRef));

            return packed.ToArray();
        }
    }

    public class ActorSwitch
    {
        public int Usage;
        public int Index;

        public ActorSwitch(int usage, int index)
        {
            Usage = usage;
            Index = index;
        }
    }


    public class ActorLink
    {
        public int Index = 0;
        public string[] Parameters = new string[2] { "", "" };

        public ActorLink(uint index, string[] parameters)
        {
            Index = (int)index;
            Parameters = parameters;
        }

        public ActorLink() { }
    }


    public class PointLink
    {
        public string[] Parameters = new string[2] { "", "" };
        public int RailIndex = 0;
        public int PointIndex = 0;

        public PointLink(uint rail, uint point, string[] parameters)
        {
            RailIndex = (int)rail;
            PointIndex = (int)point;
            Parameters = parameters;
        }

        public PointLink() { }
    }


    public class Rail
    {
        public List<ushort> PointIndexes = new List<ushort>();

        public Rail(byte[] data)
        {
            ushort numPoints = FixedHash.ReadInt(data, 0x2C, 2);
            for (int i = 0; i < numPoints; i++)
                PointIndexes.Add(FixedHash.ReadInt(data, 0x30 + (2 * i), 2));
        }

        public Rail(List<Vector3> points, Room room)
        {
            foreach (var point in points)
                PointIndexes.Add((ushort)room.Points.IndexOf(point));
        }

        public byte[] Repack()
        {
            List<byte> packed = new List<byte>();

            for (int i = 0; i < 0xC; i++)
                packed.Add(0);

            for (int i = 0; i < 4; i++)
            {
                packed.AddRange(FixedHash.GetBytes((uint)0x19));
                packed.AddRange(FixedHash.GetBytes((uint)0xFFFFFF04));
            }

            packed.AddRange(FixedHash.GetBytes((ushort)PointIndexes.Count));
            packed.AddRange(FixedHash.GetBytes((uint)1));

            foreach (ushort index in PointIndexes)
                packed.AddRange(FixedHash.GetBytes(index));

            return packed.ToArray();
        }
    }


    public class Room
    {
        public string Name;
        public FixedHash Fixed;
        public uint ZoneID = 0;
        public List<Actor> Actors = new List<Actor>();
        public List<Vector3> Points = new List<Vector3>();
        public List<Rail> Rails = new List<Rail>();

        public Room(byte[] data, string name)
        {
            Name = name;
            Fixed = new FixedHash(data);
            Entry actorEntry = new Entry();
            Entry pointEntry = new Entry();
            Entry railEntry = new Entry();
            foreach (Entry e in Fixed.Entries)
            {
                switch (e.Name)
                {
                    case "actor":
                        actorEntry = e;
                        break;
                    case "point":
                        pointEntry = e;
                        break;
                    case "rail":
                        railEntry = e;
                        break;
                }
            }
            foreach (Entry e in actorEntry.Data.Entries)
                Actors.Add(new Actor(e.Data, Fixed.NamesSection));
            foreach (Entry e in pointEntry.Data.Entries)
                Points.Add(FixedHash.ReadVector3(e.Data, 0));
            foreach (Entry e in railEntry.Data.Entries)
                Rails.Add(new Rail(e.Data));
            ZoneID = Actors[0].ZoneID;
        }

        public byte[] Repack(List<EditorLoader.ActorObj> objList)
        {
            Rails.Clear();
            Points.Clear();
            Actors.Clear();
            foreach (var obj in objList)
            {
                var actor = new Actor(obj, this);
                for (int i = 0; i < obj.Links.Count; i++)
                {
                    var link = obj.Links[i];
                    var linkedActors = objList.Where(x => x.Hash == link.Hash).ToList();
                    if (linkedActors.Count == 0)
                    {
                        obj.Links.Remove(link);
                        i--;
                        continue;
                    }
                    ActorLink newLink = new ActorLink();
                    var linkedActor = linkedActors[0];
                    newLink.Index = objList.IndexOf(linkedActor);
                    newLink.Parameters = link.Parameters;
                    actor.Links.Add(newLink);
                }
                Actors.Add(actor);
            }
            for (int i = 0; i < Actors.Count; i++)
            {
                foreach (var link in Actors[i].Links)
                {
                    var linkedActor = Actors[link.Index];
                    linkedActor.Refs.Add((uint)i);
                }
            }

            List<byte> newNames = new List<byte>();

            foreach (var entry in Fixed.Entries)
            {
                if (entry.Name == "point")
                {
                    entry.Data.Entries = new List<Entry>();
                    foreach (var point in Points)
                    {
                        List<byte> pointData = new List<byte>();
                        pointData.AddRange(FixedHash.GetBytes(point.X));
                        pointData.AddRange(FixedHash.GetBytes(point.Y));
                        pointData.AddRange(FixedHash.GetBytes(point.Z));
                        for (int i = 0; i < 0xC; i++)
                            pointData.Add(0xFF);

                        entry.Data.Entries.Add(new Entry(0xFFF3, "", 0xFFFFFFFF, pointData.ToArray()));
                    }
                }

                if (entry.Name == "rail")
                {
                    entry.Data.Entries = new List<Entry>();
                    foreach (var rail in Rails)
                        entry.Data.Entries.Add(new Entry(0xFFF2, "", 0xFFFFFFFF, rail.Repack()));
                }
                if (entry.Name == "actor")
                    {
                        entry.Data.Entries = new List<Entry>();

                        foreach (var actor in Actors)
                        {
                            entry.Data.Entries.Add(new Entry(0xFFF0, "", 0xFFFFFFFF, actor.Repack((uint)newNames.Count)));
                            newNames.AddRange(FixedHash.GetBytes(actor.Name));
                            newNames.Add(0);

                            foreach (var parameter in actor.Parameters)
                            {
                                if (Actor.GetParamType(parameter) == TypeCode.String)
                                {
                                    newNames.AddRange(FixedHash.GetBytes(parameter));
                                    newNames.Add(0);
                                }
                            }
                            foreach (var link in actor.Links)
                            {
                                string param1 = link.Parameters[0];
                                string param2 = link.Parameters[1];
                                if (Actor.GetParamType(param1) == TypeCode.String)
                                {
                                    newNames.AddRange(FixedHash.GetBytes(param1));
                                    newNames.Add(0);
                                }
                                if (Actor.GetParamType(param2) == TypeCode.String)
                                {
                                    newNames.AddRange(FixedHash.GetBytes(param2));
                                    newNames.Add(0);
                                }
                            }
                            foreach (var point in actor.Points)
                            {
                                string param1 = point.Parameters[0];
                                string param2 = point.Parameters[1];
                                if (Actor.GetParamType(param1) == TypeCode.String)
                                {
                                    newNames.AddRange(FixedHash.GetBytes(param1));
                                    newNames.Add(0);
                                }
                                if (Actor.GetParamType(param2) == TypeCode.String)
                                {
                                    newNames.AddRange(FixedHash.GetBytes(param2));
                                    newNames.Add(0);
                                }
                            }
                        }
                    }

                newNames.AddRange(FixedHash.GetBytes(entry.Name));
                newNames.Add(0);
            }

            Fixed.NamesSection = newNames.ToArray();
            return Fixed.ToBinary();
        }
    }    
}
