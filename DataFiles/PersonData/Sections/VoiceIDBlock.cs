using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class VoiceIDBlock
    {
        public short VoiceID { get; set; }
        public short altVoiceID1 { get; set; }
        public short altVoiceID2 { get; set; }
        public short altVoiceID3 { get; set; }

        public void Read(EndianBinaryReader fixed_persondata)
        {
            VoiceID = fixed_persondata.ReadInt16();
            altVoiceID1 = fixed_persondata.ReadInt16();
            altVoiceID2 = fixed_persondata.ReadInt16();
            altVoiceID3 = fixed_persondata.ReadInt16();
        }

        public void Write(EndianBinaryWriter fixed_persondata)
        {
            fixed_persondata.WriteInt16(VoiceID);
            fixed_persondata.WriteInt16(altVoiceID1);
            fixed_persondata.WriteInt16(altVoiceID2);
            fixed_persondata.WriteInt16(altVoiceID3);
        }
    }
}
