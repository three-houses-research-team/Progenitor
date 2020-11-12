using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class SpellListBlock
    {
        public byte FaithSpell1 { get; set; }
        public byte FaithSpell2 { get; set; }
        public byte FaithSpell3 { get; set; }
        public byte FaithSpell4 { get; set; }
        public byte FaithSpell5 { get; set; }
        public byte FaithRank1 { get; set; }
        public byte FaithRank2 { get; set; }
        public byte FaithRank3 { get; set; }
        public byte FaithRank4 { get; set; }
        public byte FaithRank5 { get; set; }
        public byte ReasonSpell1 { get; set; }
        public byte ReasonSpell2 { get; set; }
        public byte ReasonSpell3 { get; set; }
        public byte ReasonSpell4 { get; set; }
        public byte ReasonSpell5 { get; set; }
        public byte ReasonRank1 { get; set; }
        public byte ReasonRank2 { get; set; }
        public byte ReasonRank3 { get; set; }
        public byte ReasonRank4 { get; set; }
        public byte ReasonRank5 { get; set; }

        public void Read(EndianBinaryReader fixed_persondata)
        {
            //Koei Tecmo, what the fuck is this ordering
            FaithRank1 = fixed_persondata.ReadByte();
            FaithRank2 = fixed_persondata.ReadByte();
            FaithRank3 = fixed_persondata.ReadByte();
            FaithRank4 = fixed_persondata.ReadByte();
            FaithRank5 = fixed_persondata.ReadByte();

            ReasonSpell1 = fixed_persondata.ReadByte();
            ReasonSpell2 = fixed_persondata.ReadByte();
            ReasonSpell3 = fixed_persondata.ReadByte();
            ReasonSpell4 = fixed_persondata.ReadByte();
            ReasonSpell5 = fixed_persondata.ReadByte();

            FaithSpell1 = fixed_persondata.ReadByte();
            FaithSpell2 = fixed_persondata.ReadByte();
            FaithSpell3 = fixed_persondata.ReadByte();
            FaithSpell4 = fixed_persondata.ReadByte();
            FaithSpell5 = fixed_persondata.ReadByte();

            ReasonRank1 = fixed_persondata.ReadByte();
            ReasonRank2 = fixed_persondata.ReadByte();
            ReasonRank3 = fixed_persondata.ReadByte();
            ReasonRank4 = fixed_persondata.ReadByte();
            ReasonRank5 = fixed_persondata.ReadByte();
        }

        public void Write(EndianBinaryWriter fixed_persondata)
        {
            fixed_persondata.WriteByte(FaithRank1);
            fixed_persondata.WriteByte(FaithRank2);
            fixed_persondata.WriteByte(FaithRank3);
            fixed_persondata.WriteByte(FaithRank4);
            fixed_persondata.WriteByte(FaithRank5);

            fixed_persondata.WriteByte(ReasonSpell1);
            fixed_persondata.WriteByte(ReasonSpell2);
            fixed_persondata.WriteByte(ReasonSpell3);
            fixed_persondata.WriteByte(ReasonSpell4);
            fixed_persondata.WriteByte(ReasonSpell5);

            fixed_persondata.WriteByte(FaithSpell1);
            fixed_persondata.WriteByte(FaithSpell2);
            fixed_persondata.WriteByte(FaithSpell3);
            fixed_persondata.WriteByte(FaithSpell4);
            fixed_persondata.WriteByte(FaithSpell5);

            fixed_persondata.WriteByte(ReasonRank1);
            fixed_persondata.WriteByte(ReasonRank2);
            fixed_persondata.WriteByte(ReasonRank3);
            fixed_persondata.WriteByte(ReasonRank4);
            fixed_persondata.WriteByte(ReasonRank5);
        }
    }
}
