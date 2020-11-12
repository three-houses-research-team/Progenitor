using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class SkillListBlock
    {
        public byte[] SkillType { get; set; }
        public byte PersonalskillPart1 { get; set; }
        public byte PersonalskillPart2 { get; set; }
        public byte[] SkillLearned { get; set; }
        public byte[] SkillRank { get; set; }

        public void Read(EndianBinaryReader fixed_persondata)
        {
            SkillType = new byte[20];
            SkillLearned = new byte[20];
            SkillRank = new byte[20];

            for (int i = 0; i < 20; i++)
            {
                SkillType[i] = fixed_persondata.ReadByte();
                if (SkillType[i] == 255)
                {
                    SkillType[i] = 11;
                }
            }
            PersonalskillPart1 = fixed_persondata.ReadByte();
            PersonalskillPart2 = fixed_persondata.ReadByte();
            for (int i = 0; i < 20; i++)
            {
                SkillLearned[i] = fixed_persondata.ReadByte();
            }
            for (int i = 0; i < 20; i++)
            {
                SkillRank[i] = fixed_persondata.ReadByte();
            }
        }
        public void Write(EndianBinaryWriter fixed_persondata)
        {
            for (int i = 0; i < 20; i++)
            {
                if (SkillType[i] == 11)
                {
                    fixed_persondata.WriteByte(255);
                }
                else fixed_persondata.WriteByte(SkillType[i]);
            }
            fixed_persondata.WriteByte(PersonalskillPart1);
            fixed_persondata.WriteByte(PersonalskillPart2);
            for (int i = 0; i < 20; i++)
            {
                fixed_persondata.WriteByte(SkillLearned[i]);
            }
            for (int i = 0; i < 20; i++)
            {
                fixed_persondata.WriteByte(SkillRank[i]);
            }
        }
    }
}
