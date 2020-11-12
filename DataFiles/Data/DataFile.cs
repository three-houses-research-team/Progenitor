using Progenitor.DataFiles.Data.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeHousesPersonDataEditor;

namespace Progenitor.DataFiles.Data
{
    class DataFile
    {
        public uint numOfPointers { get; set; }
        public uint[] SectionPointers { get; set; }
        public uint[] SectionTotalSize { get; set; }

        // Header stuff for Misc Section
        public uint[] SectionMagic { get; set; }
        public uint[] SectionBlockCount { get; set; }
        public uint[] SectionBlockSize { get; set; }
        public WeaponBlock WeaponSection { get; set; }
        public List<WeaponBlock> Weapon { get; set; }
        public MagicBlock MagicSection { get; set; }
        public List<MagicBlock> Magic { get; set; }
        public TurretBlock TurretSection { get; set; }
        public List<TurretBlock> Turret { get; set; }
        public GambitBlock GambitSection { get; set; }
        public List<GambitBlock> Gambit { get; set; }
        public MonsterAOEBlock MonsterAOESection { get; set; }
        public List<MonsterAOEBlock> MonsterAOE { get; set; }
        public EquipmentBlock EquipmentSection { get; set; }
        public List<EquipmentBlock> Equipment { get; set; }
        public ItemsBlock ItemsSection { get; set; }
        public List<ItemsBlock> Items { get; set; }
        public CombatArtBlock CombatArtSection { get; set; }
        public List<CombatArtBlock> CombatArt { get; set; }
        public List<byte> SectionBytes { get; set; }
        public List<List<byte>> OtherSections { get; set; }

        public void ReadData(string fixed_data_path)
        {
            SectionPointers = new uint[20];
            SectionTotalSize = new uint[20];
            SectionMagic = new uint[20];
            SectionBlockCount = new uint[20];
            SectionBlockSize = new uint[20];
            byte nullByte = 0;
            using (EndianBinaryReader fixed_data = new EndianBinaryReader(fixed_data_path, Endianness.Little))
            {
                //fill used sections with dummy data as they will not be read from List
                //this is temporary code and will be removed once all section classes are done
                //this is just a really terrible temporary fix so as to not write dummy bytes
                SectionBytes = new List<byte>();
                SectionBytes.Add(nullByte);

                OtherSections = new List<List<byte>>();
                OtherSections.Add(SectionBytes);
                OtherSections.Add(SectionBytes);
                OtherSections.Add(SectionBytes);
                OtherSections.Add(SectionBytes);
                OtherSections.Add(SectionBytes);
                OtherSections.Add(SectionBytes);
                OtherSections.Add(SectionBytes);
                OtherSections.Add(SectionBytes);

                numOfPointers = fixed_data.ReadUInt32();
                if (numOfPointers == 16)
                {
                    for(int i = 0; i < numOfPointers; i++)
                    {
                        SectionPointers[i] = fixed_data.ReadUInt32();
                        SectionTotalSize[i] = fixed_data.ReadUInt32();
                    }

                    // For Misc Section, Header of each section
                    for (int i = 0; i < numOfPointers; i++)
                    {
                        fixed_data.Seek(SectionPointers[i], SeekOrigin.Begin);
                        SectionMagic[i] = fixed_data.ReadUInt32();
                        SectionBlockCount[i] = fixed_data.ReadUInt32();
                        SectionBlockSize[i] = fixed_data.ReadUInt32();
                        fixed_data.SeekCurrent(0x34);//skip padding

                        switch (i)
                        {
                            case 0:
                                // Section 0 is Weapon Block, data used for weapons like swords, axes, ect
                                Weapon = new List<WeaponBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    WeaponSection = new WeaponBlock();
                                    WeaponSection.Read(fixed_data);
                                    Weapon.Add(WeaponSection);
                                }
                                break;
                            case 1:
                                // Section 1 is for Magic
                                Magic = new List<MagicBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    MagicSection = new MagicBlock();
                                    MagicSection.Read(fixed_data);
                                    Magic.Add(MagicSection);
                                }
                                break;
                            case 2:
                                // Section 2 is for Turret Damage
                                Turret = new List<TurretBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    TurretSection = new TurretBlock();
                                    TurretSection.Read(fixed_data);
                                    Turret.Add(TurretSection);
                                }
                                break;
                            case 3:
                                // Section 3 is for Gambits
                                Gambit = new List<GambitBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    GambitSection = new GambitBlock();
                                    GambitSection.Read(fixed_data);
                                    Gambit.Add(GambitSection);
                                }
                                break;
                            case 4:
                                //Section 4 is for Monster AoE attacks
                                MonsterAOE = new List<MonsterAOEBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    MonsterAOESection = new MonsterAOEBlock();
                                    MonsterAOESection.Read(fixed_data);
                                    MonsterAOE.Add(MonsterAOESection);
                                }
                                break;
                            case 5:
                                // Section 5 is for Equipment like Shields, Staffs, ect
                                Equipment = new List<EquipmentBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    EquipmentSection = new EquipmentBlock();
                                    EquipmentSection.Read(fixed_data);
                                    Equipment.Add(EquipmentSection);
                                }
                                break;
                            case 6:
                                // Section 6 is for Items
                                Items = new List<ItemsBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    ItemsSection = new ItemsBlock();
                                    ItemsSection.Read(fixed_data);
                                    Items.Add(ItemsSection);
                                }
                                break;
                            case 7:
                                // Section 7 is for Combat Arts
                                CombatArt = new List<CombatArtBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    CombatArtSection = new CombatArtBlock();
                                    CombatArtSection.Read(fixed_data);
                                    CombatArt.Add(CombatArtSection);
                                }
                                break;
                            default:
                                SectionBytes = fixed_data.ReadByteList((int)(SectionBlockCount[i] * SectionBlockSize[i]));
                                OtherSections.Add(SectionBytes);
                                break;
                        }
                    }
                }
            }
        }

        public void WriteData(EndianBinaryWriter fixed_data)
        {
            //Write bingz header
            fixed_data.WriteUInt32(16);
            for (int i = 0; i < 16; i++)
            {
                fixed_data.WriteUInt32(SectionPointers[i]);
                fixed_data.WriteUInt32(SectionTotalSize[i]);
            }

            //Under construction, for now we write empty sections for the rest of the file
            //So I can build a "valid" persondata with whatever sections are done
            //So I can test how well the program works as each section is added
            for (int i = 0; i < 16; i++)
            {
                fixed_data.Seek(SectionPointers[i], SeekOrigin.Begin);
                fixed_data.WriteUInt32(SectionMagic[i]);
                fixed_data.WriteUInt32(SectionBlockCount[i]);
                fixed_data.WriteUInt32(SectionBlockSize[i]);
                fixed_data.WritePadding(0x34);
                switch (i)
                {
                    case 0:
                        foreach (var weapon in Weapon)
                        {
                            weapon.Write(fixed_data);
                        }
                        break;
                    case 1:
                        foreach (var magic in Magic)
                        {
                            magic.Write(fixed_data);
                        }
                        break;
                    case 2:
                        foreach (var turret in Turret)
                        {
                            turret.Write(fixed_data);
                        }
                        break;
                    case 3:
                        foreach (var gambit in Gambit)
                        {
                            gambit.Write(fixed_data);
                        }
                        break;
                    case 4:
                        foreach (var monsteraoe in MonsterAOE)
                        {
                            monsteraoe.Write(fixed_data);
                        }
                        break;
                    case 5:
                        foreach (var equipment in Equipment)
                        {
                            equipment.Write(fixed_data);
                        }
                        break;
                    case 6:
                        foreach (var items in Items)
                        {
                            items.Write(fixed_data);
                        }
                        break;
                    case 7:
                        foreach (var combarart in CombatArt)
                        {
                            combarart.Write(fixed_data);
                        }
                        break;
                    default:
                        fixed_data.WriteBytes(OtherSections[i]);
                        break;
                }
            }
        }
    }
}
