using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CLI
{
    public static class Program
    {
        private sealed class ChunkHeader
        {
            /// <summary> FourCC Tag of the Chunk </summary>
            public string FourCC;
            /// <summary> How many elements of this type exist. </summary>
            public int ElementCount;
            /// <summary> Offset from the start of the file to the chunk data. </summary>
            public int ChunkOffset;

            public override string ToString()
            {
                return string.Format("[{0}]", FourCC);
            }
        }

        static void Main(string[] args)
        {
            //string folderRoot = @"E:\New_Data_Drive\WindwakerModding\tpstagedumps\Stage\";

            string folderRoot = string.Empty;
            do
            {
                Console.WriteLine("Enter the filepath to the root folder of all Stages (ie: C:/WW/root/Stage/)");
                folderRoot = Console.ReadLine();
            }
            while (!Directory.Exists(folderRoot));

            string userDecision = string.Empty;
            do
            {
                Console.WriteLine("Does this path contain Twilight Princess rooms? Type Y or N.");
                userDecision = Console.ReadLine();
            }
            while (userDecision != "Y" && userDecision != "N");
            bool skipSCLSChunk = userDecision == "Y";

            DirectoryInfo dirInfo = new DirectoryInfo(folderRoot);

            StringBuilder outputStrs = new StringBuilder();
            outputStrs.AppendLine("Stage, Room, EntityName, Value");

            foreach (var map in dirInfo.GetDirectories())
            {
                Console.WriteLine("Processing Map/Scene: {0}", map.Name);
                DirectoryInfo mapDirInfo = new DirectoryInfo(map.FullName);
                DirectoryInfo[] sortedDirs = map.GetDirectories().OrderByNatural(x => x.Name).ToArray();
                foreach (var scene in sortedDirs)
                {
                    Console.WriteLine("Processing Room: {0}", scene.Name);
                    ProcessEntitiesForScene(scene.FullName, mapDirInfo.Name, outputStrs, map.Name, scene.Name, skipSCLSChunk);
                }
            }

            Console.WriteLine("Finished. Press any key.");
            Console.ReadKey();

            File.WriteAllText(@"EntityList.csv", outputStrs.ToString());
        }

        private static void ProcessEntitiesForScene(string folder, string mapName, StringBuilder output, string newMapName, string newSceneName, bool skipSCLSChunk)
        {
            // Check for a DZS/DZR sub-folder.
            string subFolder = string.Empty;
            if (Directory.Exists(folder + "/dzs/"))
                subFolder = folder + "/dzs/";
            else if (Directory.Exists(folder + "/dzr/"))
                subFolder = folder + "/dzr/";

            // Map doesn't have a DZS/DZR file
            if (string.IsNullOrEmpty(subFolder))
                return;

            string file = string.Empty;
            if (File.Exists(subFolder + "room.dzr"))
                file = subFolder + "room.dzr";
            else if (File.Exists(subFolder + "stage.dzs"))
                file = subFolder + "stage.dzs";

            if (string.IsNullOrEmpty(file))
                return;


            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Big))
            {
                // File Header
                int chunkCount = reader.ReadInt32();

                // Read the chunk headers
                List<ChunkHeader> chunks = new List<ChunkHeader>();
                for (int i = 0; i < chunkCount; i++)
                {
                    ChunkHeader chunk = new ChunkHeader();
                    chunk.FourCC = reader.ReadString(4);
                    chunk.ElementCount = reader.ReadInt32();
                    chunk.ChunkOffset = reader.ReadInt32();
                    chunks.Add(chunk);
                }

                for (int k = 0; k < chunkCount; k++)
                {
                    if (chunks[k].FourCC.StartsWith("ACT"))
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x20 - 0x8;

                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);
                        }
                    }
                    else if (chunks[k].FourCC.StartsWith("SCO"))
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;

                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);
                        }
                    }
                    else if (chunks[k].FourCC == "TGDR")
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;

                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);

                        }
                    }
                    else if (chunks[k].FourCC.StartsWith("TRE"))
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x20 - 0x8;
                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);

                        }
                    }
                    else if (chunks[k].FourCC == "DOOR")
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;
                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);

                        }
                    }
                    else if (chunks[k].FourCC == "TGOB")
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x20 - 0x8;

                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);
                        }
                    }
                    else if (chunks[k].FourCC == "TGSC")
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;
                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);
                        }
                    }
                    else if(!skipSCLSChunk && chunks[k].FourCC == "SCLS")
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            byte spawnNum = reader.ReadByte();
                            byte destRoomNum = reader.ReadByte();
                            byte fadeType = reader.ReadByte();
                            reader.Skip(1);

                            byte low = (byte)(spawnNum & 0xF);
                            byte high = (byte)((spawnNum & 0xF0) >> 4);
                            output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, name);
                        }
                    }
                    else
                    {
                        output.AppendFormat("{0}, {1}, {2}, {3}\n", newMapName, newSceneName, chunks[k].FourCC, "");
                    }
                }
            }
        }

        /// <summary>
        /// Orders a given list by Natural sort. Natural sort is what is most commonly expected (as compared to Alphabetical Sort) when sorting, especially
        /// when dealing with things with numbers on the end. Natural sort will put it as 1, 2,... 9, 10 while Alphabetical sort puts it as 1, 10, 2, ... 9, 99.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">List of items to sort</param>
        /// <param name="selector">Which parameter of the item to sort by</param>
        /// <param name="stringComparer">Override the string comparerator if needed.</param>
        /// <returns></returns>
        public static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
        {
            var regex = new Regex(@"\d+", RegexOptions.Compiled);

            int maxDigits = items
                          .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                          .Max() ?? 0;

            return items.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), stringComparer ?? StringComparer.CurrentCulture);
        }
    }
}
