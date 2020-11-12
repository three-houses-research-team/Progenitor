using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor
{
    using PersonData.Sections;

    class PersonDataFile
    {
        public uint numOfPointers { get; set; }
        public uint[] SectionPointers { get; set; }
        public uint[] SectionTotalSize { get; set; }

        // Header stuff for Misc Section
        public uint[] SectionMagic { get; set; }
        public uint[] SectionBlockCount { get; set; }
        public uint[] SectionBlockSize { get; set; }
        public CharacterBlocks CharacterSection { get; set; }
        public List<CharacterBlocks> Character { get; set; }
        public AssetIDBlock AssetIDSection { get; set; }
        public List<AssetIDBlock> AssetID { get; set; }
        public VoiceIDBlock VoiceIDSection { get; set; }
        public List<VoiceIDBlock> VoiceID { get; set; }
        public DefaultWpnRanksAndCombatAssets WeaponRanksSection { get; set; }
        public List<DefaultWpnRanksAndCombatAssets> WeaponRanks { get; set; }
        public SpellListBlock SpellListSection { get; set; }
        public List<SpellListBlock> SpellLists { get; set; }
        public SkillListBlock SkillListSection { get; set; }
        public List<SkillListBlock> SkillLists { get; set; }
        public List<byte> SectionBytes { get; set; }
        public List<List<byte>> OtherSections { get; set; }
        public DefaultWpnBlock WeaponsSection { get; set; }
        public List<DefaultWpnBlock> Weapons { get; set; }
        public CombatArtsBlock CombatArtsSection { get; set; }
        public List<CombatArtsBlock> CombatArtsLists { get; set; }

        public void ReadPersonData(string fixed_persondata_path)
        {
            SectionPointers = new uint[20];
            SectionTotalSize = new uint[20];
            SectionMagic = new uint[20];
            SectionBlockCount = new uint[20];
            SectionBlockSize = new uint[20];
            byte nullByte = 0;
            using (EndianBinaryReader fixed_persondata = new EndianBinaryReader(fixed_persondata_path, Endianness.Little))
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

                numOfPointers = fixed_persondata.ReadUInt32();
                if (numOfPointers == 18)
                {

                    for (int i = 0; i < numOfPointers; i++)
                    {
                        SectionPointers[i] = fixed_persondata.ReadUInt32();
                        SectionTotalSize[i] = fixed_persondata.ReadUInt32();
                    }

                    // For Misc Section, Header of each section
                    for (int i = 0; i < numOfPointers; i++)
                    {
                        fixed_persondata.Seek(SectionPointers[i], SeekOrigin.Begin);
                        SectionMagic[i] = fixed_persondata.ReadUInt32();
                        SectionBlockCount[i] = fixed_persondata.ReadUInt32();
                        SectionBlockSize[i] = fixed_persondata.ReadUInt32();
                        fixed_persondata.SeekCurrent(0x34);//skip padding

                        switch(i)
                        {
                            case 0:
                                // Section 0 is Character Block, data such as base stats and growths
                                Character = new List<CharacterBlocks>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    CharacterSection = new CharacterBlocks();
                                    CharacterSection.Read(fixed_persondata);
                                    Character.Add(CharacterSection);
                                }
                                break;
                            case 1:
                                // Section 1 is Asset ID, data such as 3D models used, character portraits, etc
                                AssetID = new List<AssetIDBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    AssetIDSection = new AssetIDBlock();
                                    AssetIDSection.Read(fixed_persondata);
                                    AssetID.Add(AssetIDSection);
                                }
                                break;
                            case 2:
                                // Section 2 is Void ID, ties voice sets to the voice ID used in the Asset ID
                                VoiceID = new List<VoiceIDBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    VoiceIDSection = new VoiceIDBlock();
                                    VoiceIDSection.Read(fixed_persondata);
                                    VoiceID.Add(VoiceIDSection);
                                }
                                break;
                            case 3:
                                // Section 3 is default weapon ranks, and combat related assets
                                WeaponRanks = new List<DefaultWpnRanksAndCombatAssets>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    WeaponRanksSection = new DefaultWpnRanksAndCombatAssets();
                                    WeaponRanksSection.Read(fixed_persondata);
                                    WeaponRanks.Add(WeaponRanksSection);
                                }
                                break;
                            case 4:
                                // Section 4 is the character's spell list
                                SpellLists = new List<SpellListBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    SpellListSection = new SpellListBlock();
                                    SpellListSection.Read(fixed_persondata);
                                    SpellLists.Add(SpellListSection);
                                }
                                break;
                            case 5:
                                // Section 5 is the character's skill list
                                SkillLists = new List<SkillListBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    SkillListSection = new SkillListBlock();
                                    SkillListSection.Read(fixed_persondata);
                                    SkillLists.Add(SkillListSection);
                                }
                                break;
                            case 6:
                                // Section 6 is default starting inventory
                                Weapons = new List<DefaultWpnBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    WeaponsSection = new DefaultWpnBlock();
                                    WeaponsSection.Read(fixed_persondata);
                                    Weapons.Add(WeaponsSection);
                                }
                                break;
                            case 7:
                                // Section 7 is the character's combat arts list
                                CombatArtsLists = new List<CombatArtsBlock>();
                                for (int j = 0; j < SectionBlockCount[i]; j++)
                                {
                                    CombatArtsSection = new CombatArtsBlock();
                                    CombatArtsSection.Read(fixed_persondata);
                                    CombatArtsLists.Add(CombatArtsSection);
                                }
                                break;
                            default:
                                SectionBytes = fixed_persondata.ReadByteList((int)(SectionBlockCount[i] * SectionBlockSize[i]));
                                OtherSections.Add(SectionBytes);
                                break;
                        }
                    }
                }
            }
        }
        
        public void WritePersonData(EndianBinaryWriter fixed_persondata)
        {
            //Write bingz header
            fixed_persondata.WriteUInt32(18);
            for (int i = 0; i < 18; i++)
            {
                fixed_persondata.WriteUInt32(SectionPointers[i]);
                fixed_persondata.WriteUInt32(SectionTotalSize[i]);
            }

            //Under construction, for now we write empty sections for the rest of the file
            //So I can build a "valid" persondata with whatever sections are done
            //So I can test how well the program works as each section is added
            for (int i = 0; i < 18; i++)
            {
                fixed_persondata.Seek(SectionPointers[i], SeekOrigin.Begin);
                fixed_persondata.WriteUInt32(SectionMagic[i]);
                fixed_persondata.WriteUInt32(SectionBlockCount[i]);
                fixed_persondata.WriteUInt32(SectionBlockSize[i]);
                fixed_persondata.WritePadding(0x34);
                switch(i)
                {
                    case 0:
                        foreach (var character in Character)
                        {
                            character.Write(fixed_persondata);
                        }
                        break;
                    case 1:
                        foreach (var assetid in AssetID)
                        {
                            assetid.Write(fixed_persondata);
                        }
                        break;
                    case 2:
                        foreach (var voiceid in VoiceID)
                        {
                            voiceid.Write(fixed_persondata);
                        }
                        break;
                    case 3:
                        foreach (var weaponrank in WeaponRanks)
                        {
                            weaponrank.Write(fixed_persondata);
                        }
                        break;
                    case 4:
                        foreach (var spell in SpellLists)
                        {
                            spell.Write(fixed_persondata);
                        }
                        break;
                    case 5:
                        foreach (var skill in SkillLists)
                        {
                            skill.Write(fixed_persondata);
                        }
                        break;
                    case 6:
                        foreach (var wpn in Weapons)
                        {
                            wpn.Write(fixed_persondata);
                        }
                        break;
                    case 7:
                        foreach (var combatart in CombatArtsLists)
                        {
                            combatart.Write(fixed_persondata);
                        }
                        break;
                    default:
                        fixed_persondata.WriteBytes(OtherSections[i]);
                        break;
                }
            }
        }
    }
}
