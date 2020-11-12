using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class DefaultWpnRanksAndCombatAssets
    {
        public short unk_0x0 { get; set; }
        public byte charColor { get; set; }
        public byte part1Class { get; set; }
        public byte certifiedClass1 { get; set; }
        public byte certifiedClass2 { get; set; }
        public byte certifiedClass3 { get; set; }
        public byte certifiedClass4 { get; set; }
        public byte unk_0x8 { get; set; }
        public byte defaultSwordRank { get; set; }
        public byte defaultLanceRank { get; set; }
        public byte defaultAxeRank { get; set; }
        public byte defaultBowRank { get; set; }
        public byte defaultBrawlingRank { get; set; }
        public byte defaultReasonRank { get; set; }
        public byte defaultFaithRank { get; set; }
        public byte defaultAuthorityRank { get; set; }
        public byte defaultArmorRank { get; set; }
        public byte defaultRidingRank { get; set; }
        public byte defaultFlyingRank { get; set; }
        public byte Swordaffinity { get; set; }
        public byte Lanceaffinity { get; set; }
        public byte Axeaffinity { get; set; }
        public byte Bowaffinity { get; set; }
        public byte Brawlingaffinity { get; set; }
        public byte Reasonaffinity { get; set; }
        public byte Faithaffinity { get; set; }
        public byte Authorityaffinity { get; set; }
        public byte Armoraffinity { get; set; }
        public byte Ridingaffinity { get; set; }
        public byte Flyingaffinity { get; set; }
        public byte part2Class1 { get; set; }
        public byte part2Class2 { get; set; }
        public byte part2Class3 { get; set; }
        public void Read(EndianBinaryReader fixed_persondata)
        {
            unk_0x0 = fixed_persondata.ReadInt16();
            charColor = fixed_persondata.ReadByte();
            part1Class = fixed_persondata.ReadByte();
            certifiedClass1 = fixed_persondata.ReadByte();
            certifiedClass2 = fixed_persondata.ReadByte();
            certifiedClass3 = fixed_persondata.ReadByte();
            certifiedClass4 = fixed_persondata.ReadByte();
            unk_0x8 = fixed_persondata.ReadByte();

            defaultSwordRank = fixed_persondata.ReadByte();
            defaultLanceRank = fixed_persondata.ReadByte();
            defaultAxeRank = fixed_persondata.ReadByte();
            defaultBowRank = fixed_persondata.ReadByte();
            defaultBrawlingRank = fixed_persondata.ReadByte();
            defaultReasonRank = fixed_persondata.ReadByte();
            defaultFaithRank = fixed_persondata.ReadByte();
            defaultAuthorityRank = fixed_persondata.ReadByte();
            defaultArmorRank = fixed_persondata.ReadByte();
            defaultRidingRank = fixed_persondata.ReadByte();
            defaultFlyingRank = fixed_persondata.ReadByte();

            Swordaffinity = fixed_persondata.ReadByte();
            Lanceaffinity = fixed_persondata.ReadByte();
            Axeaffinity = fixed_persondata.ReadByte();
            Bowaffinity = fixed_persondata.ReadByte();
            Brawlingaffinity = fixed_persondata.ReadByte();
            Reasonaffinity = fixed_persondata.ReadByte();
            Faithaffinity = fixed_persondata.ReadByte();
            Authorityaffinity = fixed_persondata.ReadByte();
            Armoraffinity = fixed_persondata.ReadByte();
            Ridingaffinity = fixed_persondata.ReadByte();
            Flyingaffinity = fixed_persondata.ReadByte();

            part2Class1 = fixed_persondata.ReadByte();
            part2Class2 = fixed_persondata.ReadByte();
            part2Class3 = fixed_persondata.ReadByte();
        }
        public void Write(EndianBinaryWriter fixed_persondata)
        {
            fixed_persondata.WriteInt16(unk_0x0);
            fixed_persondata.WriteByte(charColor);
            fixed_persondata.WriteByte(part1Class);
            fixed_persondata.WriteByte(certifiedClass1);
            fixed_persondata.WriteByte(certifiedClass2);
            fixed_persondata.WriteByte(certifiedClass3);
            fixed_persondata.WriteByte(certifiedClass4);
            fixed_persondata.WriteByte(unk_0x8);

            fixed_persondata.WriteByte(defaultSwordRank);
            fixed_persondata.WriteByte(defaultLanceRank);
            fixed_persondata.WriteByte(defaultAxeRank);
            fixed_persondata.WriteByte(defaultBowRank);
            fixed_persondata.WriteByte(defaultBrawlingRank);
            fixed_persondata.WriteByte(defaultReasonRank);
            fixed_persondata.WriteByte(defaultFaithRank);
            fixed_persondata.WriteByte(defaultAuthorityRank);
            fixed_persondata.WriteByte(defaultArmorRank);
            fixed_persondata.WriteByte(defaultRidingRank);
            fixed_persondata.WriteByte(defaultFlyingRank);

            fixed_persondata.WriteByte(Swordaffinity);
            fixed_persondata.WriteByte(Lanceaffinity);
            fixed_persondata.WriteByte(Axeaffinity);
            fixed_persondata.WriteByte(Bowaffinity);
            fixed_persondata.WriteByte(Brawlingaffinity);
            fixed_persondata.WriteByte(Reasonaffinity);
            fixed_persondata.WriteByte(Faithaffinity);
            fixed_persondata.WriteByte(Authorityaffinity);
            fixed_persondata.WriteByte(Armoraffinity);
            fixed_persondata.WriteByte(Ridingaffinity);
            fixed_persondata.WriteByte(Flyingaffinity);

            fixed_persondata.WriteByte(part2Class1);
            fixed_persondata.WriteByte(part2Class2);
            fixed_persondata.WriteByte(part2Class3);
        }
    }
}
