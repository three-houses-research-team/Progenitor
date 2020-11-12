using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeHousesPersonDataEditor.PersonData.Sections
{
    class CharacterBlocks
    {
        //Why can't I hold all these variables
        public float chestBandMod { get; set; }
        public float chestSize1 { get; set; }
        public float modelScale { get; set; }
        public float chestSize2 { get; set; }
        public short unk_0x10 { get; set; }
        public ushort nameID { get; set; }
        public ushort unk_0x14 { get; set; }
        public ushort voiceID { get; set; }
        public ushort assetID { get; set; }
        public byte classID { get; set; }
        public byte age { get; set; }
        public byte month { get; set; }
        public byte birthDayFlag { get; set; }
        public byte birthDay { get; set; }
        public byte unk_0x1F { get; set; }
        public sbyte saveDataID { get; set; }
        public byte unk_0x21 { get; set; }
        public byte maxHP { get; set; }
        public byte unk_0x23 { get; set; }
        public byte allegiance { get; set; }
        public sbyte unk_0x25 { get; set; }
        public byte gender { get; set; }
        public byte bodyType { get; set; }
        public byte baseBattalion { get; set; }
        public byte hpGrowth { get; set; }
        public byte nonCombatAnimSet { get; set; }
        public byte baseHP { get; set; }
        public byte crest1 { get; set; }
        public byte crest2 { get; set; }
        public byte unk_0x2E { get; set; }
        public byte height1 { get; set; }
        public byte height2 { get; set; }
        public ushort unk_0x31 { get; set; }
        public byte baseStr { get; set; }
        public byte baseMag { get; set; }
        public byte baseDex { get; set; }
        public byte baseSpd { get; set; }
        public byte baseLck { get; set; }
        public byte baseDef { get; set; }
        public byte baseRes { get; set; }
        public byte baseMov { get; set; }
        public byte baseCha { get; set; }
        public byte strGrowth { get; set; }
        public byte magGrowth { get; set; }
        public byte dexGrowth { get; set; }
        public byte spdGrowth { get; set; }
        public byte lckGrowth { get; set; }
        public byte defGrowth { get; set; }
        public byte resGrowth { get; set; }
        public byte movGrowth { get; set; }
        public byte chaGrowth { get; set; }
        public byte maxStr { get; set; }
        public byte maxMag { get; set; }
        public byte maxDex { get; set; }
        public byte maxSpd { get; set; }
        public byte maxLck { get; set; }
        public byte maxDef { get; set; }
        public byte maxRes { get; set; }
        public byte maxMov { get; set; }
        public byte maxCha { get; set; }
        public short padding { get; set; }
        public void Read(EndianBinaryReader fixed_persondata)
        {
            // Main Data, the important stuff
            chestBandMod = fixed_persondata.ReadSingle();
			chestSize1 = fixed_persondata.ReadSingle();
			modelScale = fixed_persondata.ReadSingle();
			chestSize2 = fixed_persondata.ReadSingle();
	
			unk_0x10 = fixed_persondata.ReadInt16();
			nameID = fixed_persondata.ReadUInt16();
			unk_0x14 = fixed_persondata.ReadUInt16();
			voiceID = fixed_persondata.ReadUInt16();
			assetID = fixed_persondata.ReadUInt16();
            classID = fixed_persondata.ReadByte();
			age = fixed_persondata.ReadByte();
			month = fixed_persondata.ReadByte();
			birthDayFlag = fixed_persondata.ReadByte();
			birthDay = fixed_persondata.ReadByte();
			unk_0x1F = fixed_persondata.ReadByte();
			saveDataID = fixed_persondata.ReadSByte();
			unk_0x21 = fixed_persondata.ReadByte();
			maxHP = fixed_persondata.ReadByte();
			unk_0x23 = fixed_persondata.ReadByte();
			allegiance = fixed_persondata.ReadByte();
			unk_0x25 = fixed_persondata.ReadSByte();
			gender = fixed_persondata.ReadByte();
			bodyType = fixed_persondata.ReadByte();
			baseBattalion = fixed_persondata.ReadByte();
			hpGrowth = fixed_persondata.ReadByte();
			nonCombatAnimSet = fixed_persondata.ReadByte();
			baseHP = fixed_persondata.ReadByte();
			crest1 = fixed_persondata.ReadByte();
			crest2 = fixed_persondata.ReadByte();
			unk_0x2E = fixed_persondata.ReadByte();
			height1 = fixed_persondata.ReadByte();
			height2 = fixed_persondata.ReadByte();
			unk_0x31 = fixed_persondata.ReadUInt16();
			
			// Base Stats
			baseStr = fixed_persondata.ReadByte();
			baseMag = fixed_persondata.ReadByte();
			baseDex = fixed_persondata.ReadByte();
			baseSpd = fixed_persondata.ReadByte();
			baseLck = fixed_persondata.ReadByte();
			baseDef = fixed_persondata.ReadByte();
			baseRes = fixed_persondata.ReadByte();
			baseMov = fixed_persondata.ReadByte();
			baseCha = fixed_persondata.ReadByte();
			
			// Stat Growths
			strGrowth = fixed_persondata.ReadByte();
			magGrowth = fixed_persondata.ReadByte();
			dexGrowth = fixed_persondata.ReadByte();
			spdGrowth = fixed_persondata.ReadByte();
			lckGrowth = fixed_persondata.ReadByte();
			defGrowth = fixed_persondata.ReadByte();
			resGrowth = fixed_persondata.ReadByte();
			movGrowth = fixed_persondata.ReadByte();
			chaGrowth = fixed_persondata.ReadByte();
			
			// Max Stats
			maxStr = fixed_persondata.ReadByte();
			maxMag = fixed_persondata.ReadByte();
			maxDex = fixed_persondata.ReadByte();
			maxSpd = fixed_persondata.ReadByte();
			maxLck = fixed_persondata.ReadByte();
			maxDef = fixed_persondata.ReadByte();
			maxRes = fixed_persondata.ReadByte();
			maxMov = fixed_persondata.ReadByte();
			maxCha = fixed_persondata.ReadByte();
			
			// Block ends with 2 bytes of padding
			padding = fixed_persondata.ReadInt16();
        }
		public void Write(EndianBinaryWriter fixed_persondata)
        {
            // Main Data, the important stuff
            fixed_persondata.WriteSingle(chestBandMod);
			fixed_persondata.WriteSingle(chestSize1);
			fixed_persondata.WriteSingle(modelScale);
			fixed_persondata.WriteSingle(chestSize2);
	
			fixed_persondata.WriteInt16(unk_0x10);
			fixed_persondata.WriteUInt16(nameID);
			fixed_persondata.WriteUInt16(unk_0x14);
			fixed_persondata.WriteUInt16(voiceID);
			fixed_persondata.WriteUInt16(assetID);
            fixed_persondata.WriteByte(classID);
			fixed_persondata.WriteByte(age);
			fixed_persondata.WriteByte(month);
			fixed_persondata.WriteByte(birthDayFlag);
			fixed_persondata.WriteByte(birthDay);
			fixed_persondata.WriteByte(unk_0x1F);
			fixed_persondata.WriteSByte(saveDataID);
			fixed_persondata.WriteByte(unk_0x21);
			fixed_persondata.WriteByte(maxHP);
			fixed_persondata.WriteByte(unk_0x23);
			fixed_persondata.WriteByte(allegiance);
			fixed_persondata.WriteSByte(unk_0x25);
			fixed_persondata.WriteByte(gender);
			fixed_persondata.WriteByte(bodyType);
			fixed_persondata.WriteByte(baseBattalion);
			fixed_persondata.WriteByte(hpGrowth);
			fixed_persondata.WriteByte(nonCombatAnimSet);
			fixed_persondata.WriteByte(baseHP);
			fixed_persondata.WriteByte(crest1);
			fixed_persondata.WriteByte(crest2);
			fixed_persondata.WriteByte(unk_0x2E);
			fixed_persondata.WriteByte(height1);
			fixed_persondata.WriteByte(height2);
			fixed_persondata.WriteUInt16(unk_0x31);
			
			// Base Stats
			fixed_persondata.WriteByte(baseStr);
			fixed_persondata.WriteByte(baseMag);
			fixed_persondata.WriteByte(baseDex);
			fixed_persondata.WriteByte(baseSpd);
			fixed_persondata.WriteByte(baseLck);
			fixed_persondata.WriteByte(baseDef);
			fixed_persondata.WriteByte(baseRes);
			fixed_persondata.WriteByte(baseMov);
			fixed_persondata.WriteByte(baseCha);
			
			// Stat Growths
			fixed_persondata.WriteByte(strGrowth);
			fixed_persondata.WriteByte(magGrowth);
			fixed_persondata.WriteByte(dexGrowth);
			fixed_persondata.WriteByte(spdGrowth);
			fixed_persondata.WriteByte(lckGrowth);
			fixed_persondata.WriteByte(defGrowth);
			fixed_persondata.WriteByte(resGrowth);
			fixed_persondata.WriteByte(movGrowth);
			fixed_persondata.WriteByte(chaGrowth);
			
			// Max Stats
			fixed_persondata.WriteByte(maxStr);
			fixed_persondata.WriteByte(maxMag);
			fixed_persondata.WriteByte(maxDex);
			fixed_persondata.WriteByte(maxSpd);
			fixed_persondata.WriteByte(maxLck);
			fixed_persondata.WriteByte(maxDef);
			fixed_persondata.WriteByte(maxRes);
			fixed_persondata.WriteByte(maxMov);
			fixed_persondata.WriteByte(maxCha);
			
			// Block ends with 2 bytes of padding
			fixed_persondata.WriteInt16(padding);
        }
    }
}
