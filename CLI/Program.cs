using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI
{
    class Program
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
            string folderRoot = @"E:\New_Data_Drive\WindwakerModding\De-Arc-ed Stage\";
            Console.WriteLine("Enter the filepath to the root folder of all Stages (ie: C:/WW/root/Stage/)");
            //string folderRoot = Console.ReadLine();
            DirectoryInfo dirInfo = new DirectoryInfo(folderRoot);

            while(true)
            {
                Console.Clear();
                Console.WriteLine("Enter the ACTR/SCOB/TRES/DOOR/etc. Name to investigate:");
                string objName = Console.ReadLine();

                foreach (var map in dirInfo.GetDirectories())
                {
                    DirectoryInfo mapDirInfo = new DirectoryInfo(map.FullName);
                    foreach (var scene in map.GetDirectories())
                    {
                        ProcessEntitiesForScene(scene.FullName, mapDirInfo.Name, objName);
                    }
                }

                Console.WriteLine("Finished. Press any key.");
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape)
                    break;
            }
        }

        private static void ProcessEntitiesForScene(string folder, string mapName, string objName)
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

                for(int k = 0; k < chunkCount; k++)
                {
                    if (chunks[k].FourCC.StartsWith("ACT"))
                    {
                        chunks[k].FourCC = "ACTR";
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x20 - 0x8;
                         
                            if(name == objName)
                                Console.WriteLine("{0} - {1} ({2})", objName, mapName, folder.Substring(file.LastIndexOf("\\")));
                        }
                    }

                    if (chunks[k].FourCC.StartsWith("SCO"))
                    {
                        chunks[k].FourCC = "SCOB";
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;

                            if (name == objName)
                                Console.WriteLine("{0} - {1} ({2})", objName, mapName, folder.Substring(file.LastIndexOf("\\")));

                        }
                    }

                    if (chunks[k].FourCC == "TGDR")
                    {
                        chunks[k].FourCC = "TGDR";
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;

                            if (name == objName)
                                Console.WriteLine("{0} - {1} ({2})", objName, mapName, folder.Substring(file.LastIndexOf("\\")));

                        }
                    }

                    if (chunks[k].FourCC.StartsWith("TRE"))
                    {
                        chunks[k].FourCC = "TRES";
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x20 - 0x8;

                            if (name == objName)
                                Console.WriteLine("{0} - {1} ({2})", objName, mapName, folder.Substring(file.LastIndexOf("\\")));

                        }
                    }

                    if (chunks[k].FourCC == "DOOR")
                    {
                        chunks[k].FourCC = "DOOR";
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;

                            if (name == objName)
                                Console.WriteLine("{0} - {1} ({2})", objName, mapName, folder.Substring(file.LastIndexOf("\\")));

                        }
                    }

                    if (chunks[k].FourCC == "TGOB")
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x20 - 0x8;

                            if (name == objName)
                                Console.WriteLine("{0} - {1} ({2})", objName, mapName, folder.Substring(file.LastIndexOf("\\")));

                        }
                    }

                    if (chunks[k].FourCC == "TGSC")
                    {
                        reader.BaseStream.Position = chunks[k].ChunkOffset;
                        for (int i = 0; i < chunks[k].ElementCount; i++)
                        {
                            string name = reader.ReadString(8).Trim(new[] { '\0' });
                            reader.BaseStream.Position += 0x24 - 0x8;

                            if (name == objName)
                                Console.WriteLine("{0} - {1} ({2})", objName, mapName, folder.Substring(file.LastIndexOf("\\")));

                        }
                    }
                }
            }
        }
    }
}
