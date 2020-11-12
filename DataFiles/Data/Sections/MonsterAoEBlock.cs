using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeHousesPersonDataEditor;

namespace Progenitor.DataFiles.Data.Sections
{
    class MonsterAOEBlock
    {
        //define block bytes
        public byte unk_0x0 { get; set; }
        public byte unk_0x1 { get; set; }
        public byte HPMod { get; set; }
        public byte unk_0x3 { get; set; }
        public byte MagicEffect { get; set; }
        public byte WeaponType { get; set; }
        public byte WeaponRank { get; set; }
        public byte MaxRange { get; set; }
        public byte WeaponModel { get; set; }
        public byte Crest { get; set; }
        public byte MinRange { get; set; }
        public sbyte Durability { get; set; }
        public byte ExtraEffect { get; set; }
        public sbyte ItemType { get; set; }
        public byte Effectiveness { get; set; }
        public byte flags01 { get; set; }
        public byte MT { get; set; }
        public byte Hit { get; set; }
        public sbyte Crit { get; set; }
        public byte WT { get; set; }
        public byte flags02 { get; set; }
        public byte unk_0x16 { get; set; }
        public byte flags03 { get; set; }
        public byte unk_0x18 { get; set; }
        public void Read(EndianBinaryReader fixed_data)
        {
            unk_0x0 = fixed_data.ReadByte();
            unk_0x1 = fixed_data.ReadByte();
            HPMod = fixed_data.ReadByte();
            unk_0x3 = fixed_data.ReadByte();
            MagicEffect = fixed_data.ReadByte();
            WeaponType = fixed_data.ReadByte();
            WeaponRank = fixed_data.ReadByte();
            MaxRange = fixed_data.ReadByte();
            WeaponModel = fixed_data.ReadByte();
            Crest = fixed_data.ReadByte();
            MinRange = fixed_data.ReadByte();
            Durability = fixed_data.ReadSByte();
            ExtraEffect = fixed_data.ReadByte();
            ItemType = fixed_data.ReadSByte();
            Effectiveness = fixed_data.ReadByte();
            flags01 = fixed_data.ReadByte();
            MT = fixed_data.ReadByte();
            Hit = fixed_data.ReadByte();
            Crit = fixed_data.ReadSByte();
            WT = fixed_data.ReadByte();
            flags02 = fixed_data.ReadByte();
            unk_0x16 = fixed_data.ReadByte();
            flags03 = fixed_data.ReadByte();
            unk_0x18 = fixed_data.ReadByte();
        }

        public void Write(EndianBinaryWriter fixed_data)
        {
            fixed_data.WriteByte(unk_0x0);
            fixed_data.WriteByte(unk_0x1);
            fixed_data.WriteByte(HPMod);
            fixed_data.WriteByte(unk_0x3);
            fixed_data.WriteByte(MagicEffect);
            fixed_data.WriteByte(WeaponType);
            fixed_data.WriteByte(WeaponRank);
            fixed_data.WriteByte(MaxRange);
            fixed_data.WriteByte(WeaponModel);
            fixed_data.WriteByte(Crest);
            fixed_data.WriteByte(MinRange);
            fixed_data.WriteSByte(Durability);
            fixed_data.WriteByte(ExtraEffect);
            fixed_data.WriteSByte(ItemType);
            fixed_data.WriteByte(Effectiveness);
            fixed_data.WriteByte(flags01);
            fixed_data.WriteByte(MT);
            fixed_data.WriteByte(Hit);
            fixed_data.WriteSByte(Crit);
            fixed_data.WriteByte(WT);
            fixed_data.WriteByte(flags01);
            fixed_data.WriteByte(unk_0x16);
            fixed_data.WriteByte(flags03);
            fixed_data.WriteByte(unk_0x18);
        }
    }
}
