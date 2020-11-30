using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeHousesPersonDataEditor;

namespace Progenitor.DataFiles.MaterialData.Sections
{
    class MaterialBlock
    {
        public int Price { get; set; }
        public short AltarPrice { get; set; }
        public byte unk_0x6 { get; set; }
        public byte unk_0x7 { get; set; }
        public byte Stars { get; set; }
        public byte Type { get; set; }
        public short unk_0xA { get; set; }
        
        public void Read(EndianBinaryReader MaterialData)
        {
            Price = MaterialData.ReadInt32();
            AltarPrice = MaterialData.ReadInt16();
            unk_0x6 = MaterialData.ReadByte();
            unk_0x7 = MaterialData.ReadByte();
            Stars = MaterialData.ReadByte();
            Type = MaterialData.ReadByte();
            unk_0xA = MaterialData.ReadInt16();
        }

        public void Write(EndianBinaryWriter MaterialData)
        {
            MaterialData.WriteInt32(Price);
            MaterialData.WriteInt16(AltarPrice);
            MaterialData.WriteByte(unk_0x6);
            MaterialData.WriteByte(unk_0x7);
            MaterialData.WriteByte(Stars);
            MaterialData.WriteByte(Type);
            MaterialData.WriteInt16(unk_0xA);
        }
    }
}
