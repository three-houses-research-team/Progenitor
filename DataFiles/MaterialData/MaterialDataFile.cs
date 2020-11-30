using Progenitor.DataFiles.MaterialData.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeHousesPersonDataEditor;

namespace Progenitor.DataFiles.MaterialData
{
    class MaterialDataFile
    {
        public uint numOfPointers { get; set; }
        public uint[] SectionPointers { get; set; }
        public uint[] SectionTotalSize { get; set; }

        // Header stuff for Misc Section
        public uint[] SectionMagic { get; set; }
        public uint[] SectionBlockCount { get; set; }
        public uint[] SectionBlockSize { get; set; }
        public MaterialBlock MaterialSection { get; set; }
        public List<MaterialBlock> Materials { get; set; }
        public List<byte> SectionBytes { get; set; }
        public List<List<byte>> OtherSections { get; set; }

        public void ReadData(string materialdata_path)
        {
            SectionPointers = new uint[20];
            SectionTotalSize = new uint[20];
            SectionMagic = new uint[20];
            SectionBlockCount = new uint[20];
            SectionBlockSize = new uint[20];
            byte nullByte = 0;
            using (EndianBinaryReader material_data = new EndianBinaryReader(materialdata_path, Endianness.Little))
            {
                SectionBytes = new List<byte>();
                SectionBytes.Add(nullByte);

                OtherSections = new List<List<byte>>();
                OtherSections.Add(SectionBytes);

                numOfPointers = material_data.ReadUInt32();
                if (numOfPointers == 2)
                {
                    for (int i = 0; i < numOfPointers; i++)
                    {
                        SectionPointers[i] = material_data.ReadUInt32();
                        SectionTotalSize[i] = material_data.ReadUInt32();
                    }

                    // For Misc Section, Header of each section
                    for (int i = 0; i < numOfPointers; i++)
                    {
                        material_data.Seek(SectionPointers[i], SeekOrigin.Begin);
                        SectionMagic[i] = material_data.ReadUInt32();
                        SectionBlockCount[i] = material_data.ReadUInt32();
                        SectionBlockSize[i] = material_data.ReadUInt32();
                        material_data.SeekCurrent(0x34);//skip padding

                        switch (i)
                        {
                            case 0:
                                Materials = new List<MaterialBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    MaterialSection = new MaterialBlock();
                                    MaterialSection.Read(material_data);
                                    Materials.Add(MaterialSection);
                                }
                                break;
                            default:
                                SectionBytes = material_data.ReadByteList((int)(SectionBlockCount[i] * SectionBlockSize[i]));
                                OtherSections.Add(SectionBytes);
                                break;
                        }
                    }
                }
            }
        }

        public void WriteData(EndianBinaryWriter materialdata)
        {
            //Write bingz header
            materialdata.WriteUInt32(16);
            for (int i = 0; i < 16; i++)
            {
                materialdata.WriteUInt32(SectionPointers[i]);
                materialdata.WriteUInt32(SectionTotalSize[i]);
            }

            for (int i = 0; i < 2; i++)
            {
                materialdata.Seek(SectionPointers[i], SeekOrigin.Begin);
                materialdata.WriteUInt32(SectionMagic[i]);
                materialdata.WriteUInt32(SectionBlockCount[i]);
                materialdata.WriteUInt32(SectionBlockSize[i]);
                materialdata.WritePadding(0x34);
                switch (i)
                {
                    case 0:
                        foreach (var material in Materials)
                        {
                            material.Write(materialdata);
                        }
                        break;
                    default:
                        materialdata.WriteBytes(OtherSections[i]);
                        break;
                }
            }
        }
    }
}
