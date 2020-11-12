using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeHousesPersonDataEditor;

namespace Progenitor.DataFiles.Data.Sections
{
    class CombatArtBlock
    {
        public short RequiredWeapon { get; set; }
        public sbyte Avoid { get; set; }
        public sbyte Crit { get; set; }
        public sbyte Hit { get; set; }
        public sbyte Avoid2 { get; set; }
        public byte Effect { get; set; }
        public byte RequiredClass { get; set; }
        public byte DurabilityCost { get; set; }
        public byte MaxRange { get; set; }
        public byte MinRange { get; set; }
        public byte unk_0xD { get; set; }
        public byte unk_0xE { get; set; }
        public byte Effectiveness { get; set; }
        public byte Flags { get; set; }
        public short unk_0x11 { get; set; }

        public void Read(EndianBinaryReader fixed_data)
        {
            RequiredWeapon = fixed_data.ReadInt16();
            Avoid = fixed_data.ReadSByte();
            Crit = fixed_data.ReadSByte();
            Hit = fixed_data.ReadSByte();
            Avoid2 = fixed_data.ReadSByte();
            Effect = fixed_data.ReadByte();
            RequiredClass = fixed_data.ReadByte();
            DurabilityCost = fixed_data.ReadByte();
            MaxRange = fixed_data.ReadByte();
            MinRange = fixed_data.ReadByte();
            unk_0xD = fixed_data.ReadByte();
            unk_0xE = fixed_data.ReadByte();
            Effectiveness = fixed_data.ReadByte();
            Flags = fixed_data.ReadByte();
            unk_0x11 = fixed_data.ReadInt16();
        }

        public void Write(EndianBinaryWriter fixed_data)
        {
            fixed_data.WriteInt16(RequiredWeapon);
            fixed_data.WriteSByte(Avoid);
            fixed_data.WriteSByte(Crit);
            fixed_data.WriteSByte(Hit);
            fixed_data.WriteSByte(Avoid2);
            fixed_data.WriteByte(Effect);
            fixed_data.WriteByte(RequiredClass);
            fixed_data.WriteByte(DurabilityCost);
            fixed_data.WriteByte(MaxRange);
            fixed_data.WriteByte(MinRange);
            fixed_data.WriteByte(unk_0xD);
            fixed_data.WriteByte(unk_0xE);
            fixed_data.WriteByte(Effectiveness);
            fixed_data.WriteByte(Flags);
            fixed_data.WriteInt16(unk_0x11);
        }
    }
}
