using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class AssetIDBlock
    {
        public short ngplusHair { get; set; }
        public short part2Head { get; set; }
        public short part2FaceID { get; set; }
        public short part1Head { get; set; }
        public short part1FaceID { get; set; }
        public short part2Body { get; set; }
        public short sothisFusedID { get; set; }
        public short part1Body { get; set; }
        public short altFaceID { get; set; }

        public void Read(EndianBinaryReader fixed_persondata)
        {
            ngplusHair = fixed_persondata.ReadInt16();
            part2Head = fixed_persondata.ReadInt16();
            part2FaceID = fixed_persondata.ReadInt16();
            part1Head = fixed_persondata.ReadInt16();
            part1FaceID = fixed_persondata.ReadInt16();
            part2Body = fixed_persondata.ReadInt16();
            sothisFusedID = fixed_persondata.ReadInt16();
            part1Body = fixed_persondata.ReadInt16();
            altFaceID = fixed_persondata.ReadInt16();
        }

        public void Write(EndianBinaryWriter fixed_persondata)
        {
            fixed_persondata.WriteInt16(ngplusHair);
            fixed_persondata.WriteInt16(part2Head);
            fixed_persondata.WriteInt16(part2FaceID);
            fixed_persondata.WriteInt16(part1Head);
            fixed_persondata.WriteInt16(part1FaceID);
            fixed_persondata.WriteInt16(part2Body);
            fixed_persondata.WriteInt16(sothisFusedID);
            fixed_persondata.WriteInt16(part1Body);
            fixed_persondata.WriteInt16(altFaceID);
        }
    }
}
