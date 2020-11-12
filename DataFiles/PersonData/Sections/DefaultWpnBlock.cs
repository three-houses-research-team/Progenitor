using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class DefaultWpnBlock
    {
        public ushort wpnFlags { get; set; }
        public ushort[] Weapons { get; set; }
        public void Read(EndianBinaryReader fixed_persondata)
        {
            Weapons = new ushort[6];
            wpnFlags = fixed_persondata.ReadUInt16();
            for (int i = 0; i < 6; i++)
            {
                Weapons[i] = fixed_persondata.ReadUInt16();
            }
        }
        public void Write(EndianBinaryWriter fixed_persondata)
        {
            fixed_persondata.WriteUInt16(wpnFlags);
            for (int i = 0; i < 6; i++)
            {
                fixed_persondata.WriteUInt16(Weapons[i]);
            }
        }
    }
}
