using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class CombatArtsBlock
    {
        public byte[] CombatArtType { get; set; }
        public byte[] CombatArtLearned { get; set; }
        public byte[] CombatArtRank { get; set; }
        public void Read(EndianBinaryReader fixed_persondata)
        {
            CombatArtType = new byte[10];
            CombatArtLearned = new byte[10];
            CombatArtRank = new byte[10];

            for (int i = 0; i < 10; i++)
            {
                CombatArtLearned[i] = fixed_persondata.ReadByte();
            }
            for (int i = 0; i < 10; i++)
            {
                CombatArtType[i] = fixed_persondata.ReadByte();
                if (CombatArtType[i] == 255)
                {
                    CombatArtType[i] = 11;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                CombatArtRank[i] = fixed_persondata.ReadByte();
            }
        }
        public void Write(EndianBinaryWriter fixed_persondata)
        {
            for (int i = 0; i < 10; i++)
            {
                fixed_persondata.WriteByte(CombatArtLearned[i]);
            }
            for (int i = 0; i < 10; i++)
            {
                if (CombatArtType[i] == 11)
                {
                    fixed_persondata.WriteByte(255);
                }
                else fixed_persondata.WriteByte(CombatArtType[i]);
            }
            for (int i = 0; i < 10; i++)
            {
                fixed_persondata.WriteByte(CombatArtRank[i]);
            }
        }
    }
}
