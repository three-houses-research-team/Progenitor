using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeHousesPersonDataEditor;

namespace Progenitor
{
    using ThreeHousesPersonDataEditor.PersonData.Sections;
    using System.Reflection;
    public partial class PersonDataEditor : Form
    {
        //fix flickering/ghosting issues when swapping between tabs
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private PersonDataFile currentPersonData;
        private PersonDataFile currentPersonDataCopy;
        public PersonDataEditor(string infile, string inpath)
        {
            InitializeComponent();
            PersonDataEditorMain_Load();
            OpenFile(infile, inpath);
        }
        public int SelectedLanguage { get; set; }
        public List<String> msgDataNames { get; private set; }
        public List<uint> msgDataLanguageSections { get; private set; }
        public string filePath { get; set; }
        public string nameOfFile { get; set; }

        string[] SectionNames = new string[] { "Character Data", "Asset IDs", "Voice IDs", "Weapon Ranks/Combat Assets", "Spell Learnset", "Skill Learnset", "Starting Inventory", "Combat Arts Learnset", "Support Bonuses", "Support Bonuses", "Support List", "Budding Talents", "Generic Learnset", "Faculty Teachings", "Seminar Teachings", "Character Goals", "Portrait IDs", "Enemy Personal Skills" };

        private void Open_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "fixed_persondata.bin (*.bin)|*.bin|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "open fixed_persondata.bin (v1.1.0 or later)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    nameOfFile = Path.GetFileName(filePath);
                    OpenFile(nameOfFile, filePath);
                }
                else return;
            }
        }

        public void OpenFile(string infile, string inpath)
        {
            //Get the path of specified file
            filePath = inpath;
            nameOfFile = infile;

            //Reset Stuff
            PointersListBox.Items.Clear();
            characterListBox.Items.Clear();

            //Read the contents of the file into a stream
            //var fileStream = openFileDialog.OpenFile();
            currentPersonData = new PersonDataFile();
            currentPersonData.ReadPersonData(filePath);

            //Write Info in Misc Section
            if (currentPersonData.numOfPointers == 18)
            {
                if (tabControl1.TabPages.Contains(CharacterBlocksTab) == false)
                {
                    tabControl1.Visible = true;
                    tabControl1.TabPages.Add(CharacterBlocksTab);
                    tabControl1.TabPages.Add(AssetIDTab);
                    tabControl1.TabPages.Add(VoiceIDTab);
                    tabControl1.TabPages.Add(MiscInfoTab);
                }
                for (int i = 0; i < currentPersonData.numOfPointers; i++)
                {
                    FillMiscSection(i);
                }

                //read msgData sections
                LoadLanguageTomsgDataNames();
                languageToolStripMenuItem.Visible = true;

                for (int i = 0; i < currentPersonData.SectionBlockCount[1]; i++)
                {
                    assetIDListbox.Items.Add("Asset ID #" + i.ToString());
                }
                for (int i = 0; i < currentPersonData.SectionBlockCount[2]; i++)
                {
                    voiceIDListbox.Items.Add("VoiceID #" + i.ToString());
                }
            }
            else
            {
                ResetLabels();
                return;
            }
        }

        public void LoadLanguageTomsgDataNames()
        {
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream msgDataStream = myAssembly.GetManifestResourceStream("Progenitor.msgData." + SelectedLanguage.ToString() + ".bin");
            Console.WriteLine("Loading language file " + SelectedLanguage.ToString() + ".bin");

            using (EndianBinaryReader msgData = new EndianBinaryReader(msgDataStream, Endianness.Little))
            {
                msgData.SeekCurrent(0x8); // skip header, we don't care
                var numOfmsgDataPointers = msgData.ReadUInt16();
                msgDataNames = new List<String>();

                //store all strings in msgData on List
                for (int i = 0; i < numOfmsgDataPointers; i++)
                {
                    msgData.Seek(0x14 + (4 * i), SeekOrigin.Begin);
                    var msgDataLanguageSectionPointer = msgData.ReadUInt32();
                    msgData.Seek(msgDataLanguageSectionPointer + 0x14, SeekOrigin.Begin);
                    string msgname = DecodeUTF8(msgData.ReadString(StringBinaryFormat.NullTerminated));
                    msgDataNames.Add(msgname);
                }
                ClearBoxesForLangChange();

                //read List for character names
                for (int i = 0; i < currentPersonData.SectionBlockCount[0]; i++)
                {
                    if (currentPersonData.Character[i].nameID == 0 && currentPersonData.Character[i].voiceID == 60)
                    {
                        characterListBox.Items.Add(i.ToString("D" + 4) + " : " + "----------");
                    }
                    else characterListBox.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[(currentPersonData.Character[i].nameID) + 1157]);
                }
                //read List for class names
                for (int i = 0; i <= 100; i++)
                {
                    classComboBox.Items.Add(msgDataNames[i + 3453]);
                    preCertClassCombobox1.Items.Add(msgDataNames[i + 3453]);
                    preCertClassCombobox2.Items.Add(msgDataNames[i + 3453]);
                    preCertClassCombobox3.Items.Add(msgDataNames[i + 3453]);
                    preCertClassCombobox4.Items.Add(msgDataNames[i + 3453]);
                    part1ClassCombobox.Items.Add(msgDataNames[i + 3453]);
                    part2ClassCombobox1.Items.Add(msgDataNames[i + 3453]);
                    part2ClassCombobox2.Items.Add(msgDataNames[i + 3453]);
                    part2ClassCombobox3.Items.Add(msgDataNames[i + 3453]);
                }
                for (int i = 0; i < 256 - classComboBox.Items.Count; i++)
                {
                    preCertClassCombobox1.Items.Add("-----------");
                    preCertClassCombobox2.Items.Add("-----------");
                    preCertClassCombobox3.Items.Add("-----------");
                    preCertClassCombobox4.Items.Add("-----------");
                    part1ClassCombobox.Items.Add("-----------");
                    part2ClassCombobox1.Items.Add("-----------");
                    part2ClassCombobox2.Items.Add("-----------");
                    part2ClassCombobox3.Items.Add("-----------");
                }
                // Spell names
                for (int i = 0; i < 54; i++)
                {
                    FaithSpellCombobox1.Items.Add(msgDataNames[i + 7836]);
                    FaithSpellCombobox2.Items.Add(msgDataNames[i + 7836]);
                    FaithSpellCombobox3.Items.Add(msgDataNames[i + 7836]);
                    FaithSpellCombobox4.Items.Add(msgDataNames[i + 7836]);
                    FaithSpellCombobox5.Items.Add(msgDataNames[i + 7836]);

                    reasonSpellCombobox1.Items.Add(msgDataNames[i + 7836]);
                    reasonSpellCombobox2.Items.Add(msgDataNames[i + 7836]);
                    reasonSpellCombobox3.Items.Add(msgDataNames[i + 7836]);
                    reasonSpellCombobox4.Items.Add(msgDataNames[i + 7836]);
                    reasonSpellCombobox5.Items.Add(msgDataNames[i + 7836]);
                }

                for (int i = 0; i < 202; i++)
                {
                    FaithSpellCombobox1.Items.Add("-----------");
                    FaithSpellCombobox2.Items.Add("-----------");
                    FaithSpellCombobox3.Items.Add("-----------");
                    FaithSpellCombobox4.Items.Add("-----------");
                    FaithSpellCombobox5.Items.Add("-----------");

                    reasonSpellCombobox1.Items.Add("-----------");
                    reasonSpellCombobox2.Items.Add("-----------");
                    reasonSpellCombobox3.Items.Add("-----------");
                    reasonSpellCombobox4.Items.Add("-----------");
                    reasonSpellCombobox5.Items.Add("-----------");
                }

                //Skill names
                for (int i = 0; i < 255; i++)
                {
                    SkillCombobox.Items.Add(msgDataNames[i + 7236]);
                    part1PersonalCombobox.Items.Add(msgDataNames[i + 7236]);
                    part2PersonalCombobox.Items.Add(msgDataNames[i + 7236]);
                }
                SkillCombobox.Items.Add("--------------");
                part1PersonalCombobox.Items.Add("--------------");
                part2PersonalCombobox.Items.Add("--------------");

                //read List for allegiances
                for (int i = 0; i <= 30; i++)
                {
                    allegianceComboBox.Items.Add(msgDataNames[i + 9498]);
                }

                //read List for Crest Names
                for (int i = 0; i <= 85; i++)
                {
                    crest1ComboBox.Items.Add(msgDataNames[i + 9590]);
                    crest2ComboBox.Items.Add(msgDataNames[i + 9590]);
                }
                crest1ComboBox.Items.Add(msgDataNames[9096 + 200]);
                crest2ComboBox.Items.Add(msgDataNames[9096 + 200]);

                //read List for battalion names
                for (int i = 0; i <= 200; i++)
                {
                    battalionComboBox.Items.Add(msgDataNames[i + 9096]);
                }
                for (int i = 1; i < 1200; i++)
                {
                    if (i < 200) //weapons
                    {
                        inventoryCombobox1.Items.Add(msgDataNames[i + 3746]);
                        inventoryCombobox2.Items.Add(msgDataNames[i + 3746]);
                        inventoryCombobox3.Items.Add(msgDataNames[i + 3746]);
                        inventoryCombobox4.Items.Add(msgDataNames[i + 3746]);
                        inventoryCombobox5.Items.Add(msgDataNames[i + 3746]);
                        inventoryCombobox6.Items.Add(msgDataNames[i + 3746]);
                    }
                    else if (i >= 1000) //consumibles
                    {
                        inventoryCombobox1.Items.Add(msgDataNames[(i + 4656) - 1000]);
                        inventoryCombobox2.Items.Add(msgDataNames[(i + 4656) - 1000]);
                        inventoryCombobox3.Items.Add(msgDataNames[(i + 4656) - 1000]);
                        inventoryCombobox4.Items.Add(msgDataNames[(i + 4656) - 1000]);
                        inventoryCombobox5.Items.Add(msgDataNames[(i + 4656) - 1000]);
                        inventoryCombobox6.Items.Add(msgDataNames[(i + 4656) - 1000]);
                    }
                    else
                    {
                        inventoryCombobox1.Items.Add("--------------");
                        inventoryCombobox2.Items.Add("--------------");
                        inventoryCombobox3.Items.Add("--------------");
                        inventoryCombobox4.Items.Add("--------------");
                        inventoryCombobox5.Items.Add("--------------");
                        inventoryCombobox6.Items.Add("--------------");
                    }
                }
                for (int i = 0; i < 80; i++)
                {
                    combatArtBox.Items.Add(msgDataNames[i + 6014]);
                }
                characterListBox.SelectedIndex = 0;
            }
        }

        private string DecodeUTF8(string instring)
        {
            byte[] bytes = Encoding.Default.GetBytes(instring);
            string utf8string = Encoding.UTF8.GetString(bytes);
            return utf8string;
        }

        private void PointersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            pointerLabel.Text = "Pointer to Section: " + currentPersonData.SectionPointers[PointersListBox.SelectedIndex].ToString();
            totalSizeLabel.Text = "Total Size of Section: " + currentPersonData.SectionTotalSize[PointersListBox.SelectedIndex].ToString();

            // Nonsensical stuff pls ignore
            charsectFileMagicLabel.Text = "File Magic: " + currentPersonData.SectionMagic[PointersListBox.SelectedIndex].ToString("X8");
            charsectNumOfBlocksLabel.Text = "Number of Blocks: " + currentPersonData.SectionBlockCount[PointersListBox.SelectedIndex].ToString();
            charsectBlockSizeLabel.Text = "Size of Each Block: " + currentPersonData.SectionBlockSize[PointersListBox.SelectedIndex].ToString();
        }
        // Not Necessary, Misc Section info
        private void FillMiscSection(int j)
        {
            numOfPointersLabel.Text = "Number of Pointers: " + currentPersonData.numOfPointers.ToString();
            PointersListBox.Items.Add(SectionNames[j]);
        }

        public void ResetLabels()
        {
            numOfPointersLabel.Text = "Number of Pointers: 0";
            pointerLabel.Text = "Pointer to Section: 0";
            totalSizeLabel.Text = "Total Size of Section: 0";
            charsectFileMagicLabel.Text = "File Magic: 00000000";
            charsectNumOfBlocksLabel.Text = "Number of Blocks: 0";
            charsectBlockSizeLabel.Text = "Size of Each Block: 0";
            MessageBox.Show("This file is not a valid PersonData file", "Invalid file");
            //Hide Tabs to prevent data being set
            tabControl1.TabPages.Remove(CharacterBlocksTab);
            tabControl1.TabPages.Remove(AssetIDTab);
            tabControl1.TabPages.Remove(VoiceIDTab);
            tabControl1.TabPages.Remove(MiscInfoTab);
            characterListBox.Items.Clear();
            assetIDListbox.Items.Clear();
        }
        private void PersonDataEditorMain_Load()
        {
            tabControl1.Visible = false;
            //Hide Tabs to prevent data being set
            tabControl1.TabPages.Remove(CharacterBlocksTab);
            tabControl1.TabPages.Remove(MiscInfoTab);

            //Hide Tabs under construction
            tabControl1.TabPages.Remove(AssetIDTab);
            tabControl1.TabPages.Remove(PortraitIDTab);
            tabControl1.TabPages.Remove(VoiceIDTab);
            tabControl1.TabPages.Remove(SupportBonuses1Tab);
            tabControl1.TabPages.Remove(SupportBonuses2Tab);
            tabControl1.TabPages.Remove(SupportListTab);
            tabControl1.TabPages.Remove(SeminarTeachingTab);
            tabControl1.TabPages.Remove(FacultyTeachingTab);
            tabControl1.TabPages.Remove(CharacterGoalsTab);
            tabControl1.TabPages.Remove(GenericLearnsetTab);
            tabControl1.TabPages.Remove(EnemyPersonalSkillTab);

            SelectedLanguage = 1;
            languageToolStripMenuItem.Visible = false;
        }

        private void ClearBoxesForLangChange()
        {
            //Combo Boxes
            classComboBox.Items.Clear();
            allegianceComboBox.Items.Clear();
            crest1ComboBox.Items.Clear();
            crest2ComboBox.Items.Clear();
            battalionComboBox.Items.Clear();
            preCertClassCombobox1.Items.Clear();
            preCertClassCombobox2.Items.Clear();
            preCertClassCombobox3.Items.Clear();
            preCertClassCombobox4.Items.Clear();
            part1ClassCombobox.Items.Clear();
            part2ClassCombobox1.Items.Clear();
            part2ClassCombobox2.Items.Clear();
            part2ClassCombobox3.Items.Clear();
            characterListBox.Items.Clear();

            FaithSpellCombobox1.Items.Clear();
            FaithSpellCombobox2.Items.Clear();
            FaithSpellCombobox3.Items.Clear();
            FaithSpellCombobox4.Items.Clear();
            FaithSpellCombobox5.Items.Clear();

            reasonSpellCombobox1.Items.Clear();
            reasonSpellCombobox2.Items.Clear();
            reasonSpellCombobox3.Items.Clear();
            reasonSpellCombobox4.Items.Clear();
            reasonSpellCombobox5.Items.Clear();

            part1PersonalCombobox.Items.Clear();
            part2PersonalCombobox.Items.Clear();
            inventoryCombobox1.Items.Clear();
            inventoryCombobox2.Items.Clear();
            inventoryCombobox3.Items.Clear();
            inventoryCombobox4.Items.Clear();
            inventoryCombobox5.Items.Clear();
            inventoryCombobox6.Items.Clear();

            SkillCombobox.Items.Clear();
            combatArtBox.Items.Clear();
        }

        private void characterListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayCurrentCharacter();
        }

        private void DisplayCurrentCharacter()
        {
            height1NumberBox.Text = currentPersonData.Character[characterListBox.SelectedIndex].height1.ToString();
            height2NumberBox.Text = currentPersonData.Character[characterListBox.SelectedIndex].height2.ToString();
            ageNumberBox.Text = currentPersonData.Character[characterListBox.SelectedIndex].age.ToString();
            birthMonthNumberBox.Text = currentPersonData.Character[characterListBox.SelectedIndex].month.ToString();
            birthDayNumberBox.Text = currentPersonData.Character[characterListBox.SelectedIndex].birthDay.ToString();

            //Load Character Portrait
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Face.unk.bmp");
            if (currentPersonData.Character[characterListBox.SelectedIndex].assetID <= 56 || currentPersonData.Character[characterListBox.SelectedIndex].assetID == 85 || (currentPersonData.Character[characterListBox.SelectedIndex].assetID >= 500 && currentPersonData.Character[characterListBox.SelectedIndex].assetID <= 507))
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Face.Face_" + currentPersonData.Character[characterListBox.SelectedIndex].assetID.ToString() + ".bmp");
            }
            Bitmap face = new Bitmap(myStream);
            facePicBox.Image = face;

            //Combo Boxes
            classComboBox.SelectedIndex = currentPersonData.Character[characterListBox.SelectedIndex].classID;
            allegianceComboBox.SelectedIndex = currentPersonData.Character[characterListBox.SelectedIndex].allegiance;
            genderComboBox.SelectedIndex = currentPersonData.Character[characterListBox.SelectedIndex].gender;
            crest1ComboBox.SelectedIndex = currentPersonData.Character[characterListBox.SelectedIndex].crest1;
            crest2ComboBox.SelectedIndex = currentPersonData.Character[characterListBox.SelectedIndex].crest2;
            battalionComboBox.SelectedIndex = currentPersonData.Character[characterListBox.SelectedIndex].baseBattalion;

            //Build the full character name
            //Checking for empty middle names, because the way KT constructs the full name is very dumb
            if (msgDataNames[currentPersonData.Character[characterListBox.SelectedIndex].nameID + 1731] == "")
            {
                nameTextBox.Text = msgDataNames[currentPersonData.Character[characterListBox.SelectedIndex].nameID + 1157] + " " + msgDataNames[currentPersonData.Character[characterListBox.SelectedIndex].nameID + 2305];
            }
            else nameTextBox.Text = msgDataNames[currentPersonData.Character[characterListBox.SelectedIndex].nameID + 1157] + " " + msgDataNames[currentPersonData.Character[characterListBox.SelectedIndex].nameID + 1731] + " " + msgDataNames[currentPersonData.Character[characterListBox.SelectedIndex].nameID + 2305];

            //Stats stuff
            //Base Stats
            baseHPNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseHP;
            baseStrNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseStr;
            baseMagNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseMag;
            baseDexNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseDex;
            baseSpdNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseSpd;
            baseLckNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseLck;
            baseDefNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseDef;
            baseResNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseRes;
            baseMovNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseMov;
            baseChaNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].baseCha;

            //Growths
            HPGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].hpGrowth;
            StrGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].strGrowth;
            MagGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].magGrowth;
            DexGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].dexGrowth;
            SpdGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].spdGrowth;
            LckGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].lckGrowth;
            DefGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].defGrowth;
            ResGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].resGrowth;
            MovGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].movGrowth;
            ChaGrowthNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].chaGrowth;

            //Max Stats
            MaxHPNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxHP;
            MaxStrNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxStr;
            MaxMagNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxMag;
            MaxDexNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxDex;
            MaxSpdNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxSpd;
            MaxLckNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxLck;
            MaxDefNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxDef;
            MaxResNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxRes;
            MaxMovNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxMov;
            MaxChaNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].maxCha;

            //Scale values
            chestSize1NumBox.Value = Convert.ToDecimal(currentPersonData.Character[characterListBox.SelectedIndex].chestSize1);
            chestSize2NumBox.Value = Convert.ToDecimal(currentPersonData.Character[characterListBox.SelectedIndex].chestSize2);
            chestWidthScale.Value = Convert.ToDecimal(currentPersonData.Character[characterListBox.SelectedIndex].chestBandMod);
            modelScaleNumBox.Value = Convert.ToDecimal(currentPersonData.Character[characterListBox.SelectedIndex].modelScale);

            //Other Section
            unk0x10NumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x10;
            unk0x14NumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x14;
            unk0x1FNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x1F;
            unk0x21NumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x21;
            unk0x23NumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x23;
            unk0x25NumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x25;
            unk0x2ENumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x2E;
            unk0x31NumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].unk_0x31;

            NameIDNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].nameID;
            voiceIDNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].voiceID;
            assetIDNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].assetID;
            birthdayFlagNumbox.Value = currentPersonData.Character[characterListBox.SelectedIndex].birthDayFlag;
            saveDataIDNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].saveDataID;
            bodyTypeNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].bodyType;
            nonCombatAnimNumBox.Value = currentPersonData.Character[characterListBox.SelectedIndex].nonCombatAnimSet;

            if (currentPersonData.Character[characterListBox.SelectedIndex].saveDataID != -1 && currentPersonData.Character[characterListBox.SelectedIndex].saveDataID < currentPersonData.WeaponRanks.Count)
            {
                wpnRanksGroupbox.Visible = true;
                wpnProfGroupbox.Visible = true;
                CombatAssetsGroupbox.Visible = true;
                spelllistgroupbox.Visible = true;
                skillListGroupbox.Visible = true;
                skillsGroupbox.Visible = true;
                inventoryGroupbox.Visible = true;
                combatArtListGroupbox2.Visible = true;
                combatArtListGroupbox.Visible = true;

                defaultSwordCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultSwordRank;
                defaultLanceCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultLanceRank;
                defaultAxeCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultAxeRank;
                defaultBowCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultBowRank;
                defaultBrawlingCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultBrawlingRank;
                defaultReasonCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultReasonRank;
                defaultFaithCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultFaithRank;
                defaultAuthorityCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultAuthorityRank;
                defaultArmorCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultArmorRank;
                defaultRidingCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultRidingRank;
                defaultFlyingCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultFlyingRank;

                SwordProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Swordaffinity - 1;
                LanceProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Lanceaffinity - 1;
                AxeProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Axeaffinity - 1;
                BowProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Bowaffinity - 1;
                BrawlingProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Brawlingaffinity - 1;
                ReasonProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Reasonaffinity - 1;
                FaithProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Faithaffinity - 1;
                AuthorityProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Authorityaffinity - 1;
                ArmorProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Armoraffinity - 1;
                RidingProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Ridingaffinity - 1;
                FlyingProfCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Flyingaffinity - 1;

                unk0x0numbox.Value = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].unk_0x0;
                unk0x8numbox.Value = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].unk_0x8;
                preCertClassCombobox1.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass1;
                preCertClassCombobox2.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass2;
                preCertClassCombobox3.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass3;
                preCertClassCombobox4.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass4;
                part1ClassCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part1Class;
                part2ClassCombobox1.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part2Class1;
                part2ClassCombobox2.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part2Class2;
                part2ClassCombobox3.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part2Class3;
                unitColorCombobox.SelectedIndex = currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].charColor;

                reasonSpellCombobox1.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell1;
                reasonSpellCombobox2.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell2;
                reasonSpellCombobox3.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell3;
                reasonSpellCombobox4.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell4;
                reasonSpellCombobox5.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell5;

                FaithSpellCombobox1.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell1;
                FaithSpellCombobox2.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell2;
                FaithSpellCombobox3.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell3;
                FaithSpellCombobox4.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell4;
                FaithSpellCombobox5.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell5;

                ReasonRankCombobox1.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank1;
                ReasonRankCombobox2.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank2;
                ReasonRankCombobox3.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank3;
                ReasonRankCombobox4.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank4;
                ReasonRankCombobox5.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank5;

                FaithRankCombobox1.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank1;
                FaithRankCombobox2.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank2;
                FaithRankCombobox3.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank3;
                FaithRankCombobox4.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank4;
                FaithRankCombobox5.SelectedIndex = currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank5;

                inventoryCombobox1.SelectedIndex = currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[0];
                inventoryCombobox2.SelectedIndex = currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[1];
                inventoryCombobox3.SelectedIndex = currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[2];
                inventoryCombobox4.SelectedIndex = currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[3];
                inventoryCombobox5.SelectedIndex = currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[4];
                inventoryCombobox6.SelectedIndex = currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[5];
                inventoryFlagsNumbox.Value = currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].wpnFlags;
                //Resetting displayed skill between characters
                var curIndx = skillListbox.SelectedIndex;
                skillListbox.SelectedIndex = -1;
                skillListbox.SelectedIndex = curIndx;

                curIndx = combatArtsListbox.SelectedIndex;
                combatArtsListbox.SelectedIndex = -1;
                combatArtsListbox.SelectedIndex = curIndx;
            }
            else
            {
                wpnRanksGroupbox.Visible = false;
                wpnProfGroupbox.Visible = false;
                CombatAssetsGroupbox.Visible = false;
                spelllistgroupbox.Visible = false;
                skillListGroupbox.Visible = false;
                skillsGroupbox.Visible = false;
                inventoryGroupbox.Visible = false;
                combatArtListGroupbox.Visible = false;
                combatArtListGroupbox2.Visible = false;
            }
        }

        private void height1NumberBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].height1 = Decimal.ToByte(height1NumberBox.Value);
        }

        private void height2NumberBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].height2 = Decimal.ToByte(height2NumberBox.Value);
        }

        private void birthMonthNumberBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].month = Decimal.ToByte(birthMonthNumberBox.Value);
        }

        private void birthDayNumberBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].birthDay = Decimal.ToByte(birthDayNumberBox.Value);
        }

        private void ageNumberBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].age = Decimal.ToByte(ageNumberBox.Value);
        }

        private void baseHPNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseHP = Decimal.ToByte(baseHPNumBox.Value);
        }

        private void baseStrNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseStr = Decimal.ToByte(baseStrNumBox.Value);
        }

        private void baseMagNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseMag = Decimal.ToByte(baseMagNumBox.Value);
        }

        private void baseDexNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseDex = Decimal.ToByte(baseDexNumBox.Value);
        }

        private void baseSpdNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseSpd = Decimal.ToByte(baseSpdNumBox.Value);
        }

        private void baseLckNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseLck = Decimal.ToByte(baseLckNumBox.Value);
        }

        private void baseDefNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseDef = Decimal.ToByte(baseDefNumBox.Value);
        }

        private void baseResNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseRes = Decimal.ToByte(baseResNumBox.Value);
        }

        private void baseMovNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseMov = Decimal.ToByte(baseMovNumBox.Value);
        }

        private void baseChaNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseCha = Decimal.ToByte(baseChaNumBox.Value);
        }

        private void HPGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].hpGrowth = Decimal.ToByte(HPGrowthNumBox.Value);
        }

        private void StrGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].strGrowth = Decimal.ToByte(StrGrowthNumBox.Value);
        }

        private void MagGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].magGrowth = Decimal.ToByte(MagGrowthNumBox.Value);
        }

        private void DexGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].dexGrowth = Decimal.ToByte(DexGrowthNumBox.Value);
        }

        private void SpdGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].spdGrowth = Decimal.ToByte(SpdGrowthNumBox.Value);
        }

        private void LckGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].lckGrowth = Decimal.ToByte(LckGrowthNumBox.Value);
        }

        private void DefGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].defGrowth = Decimal.ToByte(DefGrowthNumBox.Value);
        }

        private void ResGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].resGrowth = Decimal.ToByte(ResGrowthNumBox.Value);
        }

        private void MovGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].movGrowth = Decimal.ToByte(MovGrowthNumBox.Value);
        }

        private void ChaGrowthNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].chaGrowth = Decimal.ToByte(ChaGrowthNumBox.Value);
        }

        private void MaxHPNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxHP = Decimal.ToByte(MaxHPNumBox.Value);
        }

        private void MaxStrNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxStr = Decimal.ToByte(MaxStrNumBox.Value);
        }

        private void MaxMagNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxMag = Decimal.ToByte(MaxMagNumBox.Value);
        }

        private void MaxDexNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxDex = Decimal.ToByte(MaxDexNumBox.Value);
        }

        private void MaxSpdNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxSpd = Decimal.ToByte(MaxSpdNumBox.Value);
        }

        private void MaxLckNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxLck = Decimal.ToByte(MaxLckNumBox.Value);
        }

        private void MaxDefNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxDef = Decimal.ToByte(MaxDefNumBox.Value);
        }

        private void MaxResNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxRes = Decimal.ToByte(MaxResNumBox.Value);
        }

        private void MaxMovNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxMov = Decimal.ToByte(MaxMovNumBox.Value);
        }

        private void MaxChaNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].maxCha = Decimal.ToByte(MaxChaNumBox.Value);
        }

        private void classComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].classID = Convert.ToByte(classComboBox.SelectedIndex);
        }

        private void allegianceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].allegiance = Convert.ToByte(allegianceComboBox.SelectedIndex);
        }

        private void chestSize1NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].chestSize1 = Decimal.ToSingle(chestSize1NumBox.Value);
        }

        private void chestSize2NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].chestSize2 = Decimal.ToSingle(chestSize2NumBox.Value);
        }

        private void chestWidthScale_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].chestBandMod = Decimal.ToSingle(chestWidthScale.Value);
        }

        private void modelScaleNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].modelScale = Decimal.ToSingle(modelScaleNumBox.Value);
        }

        private void genderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].gender = Convert.ToByte(genderComboBox.SelectedIndex);
        }

        private void battalionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].baseBattalion = Convert.ToByte(battalionComboBox.SelectedIndex);
        }

        private void unk0x10NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x10 = Decimal.ToInt16(unk0x10NumBox.Value);
        }

        private void NameIDNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].nameID = Decimal.ToUInt16(NameIDNumBox.Value);
        }

        private void unk0x14NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x14 = Decimal.ToUInt16(unk0x14NumBox.Value);
        }

        private void voiceIDNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].voiceID = Decimal.ToUInt16(voiceIDNumBox.Value);
        }

        private void assetIDNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].assetID = Decimal.ToUInt16(assetIDNumBox.Value);
        }

        private void birthdayFlagNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].birthDayFlag = Decimal.ToByte(birthdayFlagNumbox.Value);
        }

        private void unk0x1FNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x1F = Decimal.ToByte(unk0x1FNumBox.Value);
        }

        private void saveDataIDNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].saveDataID = Decimal.ToSByte(saveDataIDNumBox.Value);
        }

        private void unk0x21NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x21 = Decimal.ToByte(unk0x21NumBox.Value);
        }

        private void unk0x23NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x23 = Decimal.ToByte(unk0x23NumBox.Value);
        }

        private void unk0x25NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x25 = Decimal.ToSByte(unk0x25NumBox.Value);
        }

        private void bodyTypeNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].bodyType = Decimal.ToByte(bodyTypeNumBox.Value);
        }

        private void nonCombatAnimNumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].nonCombatAnimSet = Decimal.ToByte(nonCombatAnimNumBox.Value);
        }

        private void unk0x2ENumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x2E = Decimal.ToByte(unk0x2ENumBox.Value);
        }

        private void unk0x31NumBox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].unk_0x31 = Decimal.ToUInt16(unk0x31NumBox.Value);
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                using (EndianBinaryWriter fixed_persondata = new EndianBinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write), Endianness.Little))
                {
                    currentPersonData.WritePersonData(fixed_persondata);
                    MessageBox.Show("File saved to: " + filePath, "File saved");
                }
            }
            else
                MessageBox.Show("No file is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void saveFileAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                {
                    saveFileDialog1.Filter = "fixed_persondata.bin (*.bin)|*.bin|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 1;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var savePath = saveFileDialog1.FileName;
                        using (EndianBinaryWriter fixed_persondata = new EndianBinaryWriter(File.Open(savePath, FileMode.Create, FileAccess.Write), Endianness.Little))
                        {
                            filePath = savePath; //now the Save File option writes here too
                            currentPersonData.WritePersonData(fixed_persondata);
                            MessageBox.Show("File saved to: " + savePath, "File saved");
                        }
                    }
                }
            }
            else
                MessageBox.Show("No file is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            englishToolStripMenuItem.Checked = true;
            SelectedLanguage = 1;
            LoadLanguageTomsgDataNames();
        }
        private void UncheckLanguageStrips()
        {
            englishToolStripMenuItem.Checked = false;
            japaneseToolStripMenuItem.Checked = false;
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem4.Checked = false;
            toolStripMenuItem5.Checked = false;
            toolStripMenuItem6.Checked = false;
            toolStripMenuItem7.Checked = false;
            toolStripMenuItem8.Checked = false;
            toolStripMenuItem9.Checked = false;
            toolStripMenuItem10.Checked = false;
            toolStripMenuItem11.Checked = false;
        }

        private void japaneseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            japaneseToolStripMenuItem.Checked = true;
            SelectedLanguage = 0;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem2.Checked = true;
            SelectedLanguage = 2;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem3.Checked = true;
            SelectedLanguage = 3;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem4.Checked = true;
            SelectedLanguage = 4;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem5.Checked = true;
            SelectedLanguage = 5;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem6.Checked = true;
            SelectedLanguage = 6;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem7.Checked = true;
            SelectedLanguage = 7;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem8.Checked = true;
            SelectedLanguage = 8;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem9.Checked = true;
            SelectedLanguage = 9;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem10.Checked = true;
            SelectedLanguage = 10;
            LoadLanguageTomsgDataNames();
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            UncheckLanguageStrips();
            toolStripMenuItem11.Checked = true;
            SelectedLanguage = 11;
            LoadLanguageTomsgDataNames();
        }

        private void part1HeadNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].part1Head = Decimal.ToInt16(part1HeadNumbox.Value);
        }

        private void part1BodyNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].part1Body = Decimal.ToInt16(part1BodyNumbox.Value);
        }

        private void part1FaceIDNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].part1FaceID = Decimal.ToInt16(part1FaceIDNumbox.Value);
        }

        private void part2HeadNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].part2Head = Decimal.ToInt16(part2HeadNumbox.Value);
        }

        private void part2BodyNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].part2Body = Decimal.ToInt16(part2BodyNumbox.Value);
        }

        private void part2FaceIDNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].part2FaceID = Decimal.ToInt16(part2FaceIDNumbox.Value);
        }

        private void ngplusHair_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].ngplusHair = Decimal.ToInt16(ngplusHair.Value);
        }

        private void sothisFusionAIDNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].sothisFusedID = Decimal.ToInt16(sothisFusionAIDNumbox.Value);
        }

        private void altFaceIDNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.AssetID[assetIDListbox.SelectedIndex].altFaceID = Decimal.ToInt16(altFaceIDNumbox.Value);
        }

        private void assetIDListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            part1HeadNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].part1Head;
            part2HeadNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].part2Head;
            part1BodyNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].part1Body;
            part2BodyNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].part2Body;
            part1FaceIDNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].part1FaceID;
            part2FaceIDNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].part2FaceID;
            sothisFusionAIDNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].sothisFusedID;
            ngplusHair.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].ngplusHair;
            altFaceIDNumbox.Value = currentPersonData.AssetID[assetIDListbox.SelectedIndex].altFaceID;

            AIDTabTextbox.Text = "";
            for (int i = 0; i < currentPersonData.Character.Count; i++)
            {
                if (currentPersonData.Character[i].assetID == assetIDListbox.SelectedIndex)
                {
                    AIDTabTextbox.Text += (characterListBox.Items[i].ToString() + "\r\n");
                }
            }
            //Console.WriteLine("Current Index: " + assetIDListbox.SelectedIndex.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Note: This will only reset the stats sub-tab and not everything.", "Reset Character stats?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                currentPersonDataCopy = new PersonDataFile();
                currentPersonDataCopy.ReadPersonData(filePath);
                currentPersonData.Character[characterListBox.SelectedIndex] = currentPersonDataCopy.Character[characterListBox.SelectedIndex];
                DisplayCurrentCharacter();
                MessageBox.Show("The currently selected character has been reset!", "Reset Complete");
            }
        }

        private void defaultSwordCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultSwordRank = Convert.ToByte(defaultSwordCombobox.SelectedIndex);
        }

        private void defaultLanceCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultLanceRank = Convert.ToByte(defaultLanceCombobox.SelectedIndex);
        }

        private void defaultAxeCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultAxeRank = Convert.ToByte(defaultAxeCombobox.SelectedIndex);
        }

        private void defaultBowCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultBowRank = Convert.ToByte(defaultBowCombobox.SelectedIndex);
        }

        private void defaultBrawlingCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultBrawlingRank = Convert.ToByte(defaultBrawlingCombobox.SelectedIndex);
        }

        private void defaultReasonCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultReasonRank = Convert.ToByte(defaultReasonCombobox.SelectedIndex);
        }

        private void defaultFaithCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultFaithRank = Convert.ToByte(defaultFaithCombobox.SelectedIndex);
        }

        private void defaultAuthorityCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultAuthorityRank = Convert.ToByte(defaultAuthorityCombobox.SelectedIndex);
        }

        private void defaultArmorCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultArmorRank = Convert.ToByte(defaultArmorCombobox.SelectedIndex);
        }

        private void defaultRidingCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultRidingRank = Convert.ToByte(defaultRidingCombobox.SelectedIndex);
        }

        private void defaultFlyingCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].defaultFlyingRank = Convert.ToByte(defaultFlyingCombobox.SelectedIndex);
        }

        private void SwordProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Swordaffinity = Convert.ToByte(SwordProfCombobox.SelectedIndex + 1);
        }

        private void LanceProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Lanceaffinity = Convert.ToByte(LanceProfCombobox.SelectedIndex + 1);
        }

        private void AxeProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Axeaffinity = Convert.ToByte(AxeProfCombobox.SelectedIndex + 1);
        }

        private void BowProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Bowaffinity = Convert.ToByte(BowProfCombobox.SelectedIndex + 1);
        }

        private void BrawlingProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Brawlingaffinity = Convert.ToByte(BrawlingProfCombobox.SelectedIndex + 1);
        }

        private void ReasonProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Reasonaffinity = Convert.ToByte(ReasonProfCombobox.SelectedIndex + 1);
        }

        private void FaithProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Faithaffinity = Convert.ToByte(FaithProfCombobox.SelectedIndex + 1);
        }

        private void AuthorityProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Authorityaffinity = Convert.ToByte(AuthorityProfCombobox.SelectedIndex + 1);
        }

        private void ArmorProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Armoraffinity = Convert.ToByte(ArmorProfCombobox.SelectedIndex + 1);
        }

        private void RidingProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Ridingaffinity = Convert.ToByte(RidingProfCombobox.SelectedIndex + 1);
        }

        private void FlyingProfCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Flyingaffinity = Convert.ToByte(FlyingProfCombobox.SelectedIndex + 1);
        }

        private void part1ClassCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part1Class = Convert.ToByte(part1ClassCombobox.SelectedIndex);
        }

        private void part2ClassCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part2Class1 = Convert.ToByte(part2ClassCombobox1.SelectedIndex);
        }

        private void part2ClassCombobox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part2Class2 = Convert.ToByte(part2ClassCombobox2.SelectedIndex);
        }

        private void part2ClassCombobox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].part2Class3 = Convert.ToByte(part2ClassCombobox3.SelectedIndex);
        }

        private void preCertClassCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass1 = Convert.ToByte(preCertClassCombobox1.SelectedIndex);
        }

        private void preCertClassCombobox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass2 = Convert.ToByte(preCertClassCombobox2.SelectedIndex);
        }

        private void preCertClassCombobox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass3 = Convert.ToByte(preCertClassCombobox3.SelectedIndex);
        }

        private void preCertClassCombobox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].certifiedClass4 = Convert.ToByte(preCertClassCombobox4.SelectedIndex);
        }

        private void unk0x0numbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].unk_0x0 = Convert.ToInt16(unk0x0numbox.Value);
        }

        private void unk0x8numbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].unk_0x8 = Convert.ToByte(unk0x8numbox.Value);
        }

        private void unitColorCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.WeaponRanks[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].charColor = Convert.ToByte(unitColorCombobox.SelectedIndex);
        }

        private void voiceIDListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            mainVoiceIDNumbox.Value = currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].VoiceID;
            altVoiceIDNumbox1.Value = currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].altVoiceID1;
            altVoiceIDNumbox2.Value = currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].altVoiceID2;
            altVoiceIDNumbox3.Value = currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].altVoiceID3;

            voiceIDTexbox.Text = "";
            for (int i = 0; i < currentPersonData.Character.Count; i++)
            {
                if (currentPersonData.Character[i].voiceID == voiceIDListbox.SelectedIndex)
                {
                    voiceIDTexbox.Text += (characterListBox.Items[i].ToString() + "\r\n");
                }
            }
        }

        private void mainVoiceIDNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].VoiceID = Decimal.ToInt16(mainVoiceIDNumbox.Value);
        }

        private void altVoiceIDNumbox1_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].altVoiceID1 = Decimal.ToInt16(altVoiceIDNumbox1.Value);
        }

        private void altVoiceIDNumbox2_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].altVoiceID2 = Decimal.ToInt16(altVoiceIDNumbox2.Value);
        }

        private void altVoiceIDNumbox3_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.VoiceID[voiceIDListbox.SelectedIndex].altVoiceID3 = Decimal.ToInt16(altVoiceIDNumbox3.Value);
        }

        private void reasonSpellCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell1 = Decimal.ToByte(reasonSpellCombobox1.SelectedIndex);
        }

        private void reasonSpellCombobox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell2 = Decimal.ToByte(reasonSpellCombobox2.SelectedIndex);
        }

        private void reasonSpellCombobox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell3 = Decimal.ToByte(reasonSpellCombobox3.SelectedIndex);
        }

        private void reasonSpellCombobox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell4 = Decimal.ToByte(reasonSpellCombobox4.SelectedIndex);
        }

        private void reasonSpellCombobox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonSpell5 = Decimal.ToByte(reasonSpellCombobox5.SelectedIndex);
        }

        private void FaithSpellCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell1 = Decimal.ToByte(FaithSpellCombobox1.SelectedIndex);
        }

        private void FaithSpellCombobox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell2 = Decimal.ToByte(FaithSpellCombobox2.SelectedIndex);
        }

        private void FaithSpellCombobox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell3 = Decimal.ToByte(FaithSpellCombobox3.SelectedIndex);
        }

        private void FaithSpellCombobox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell4 = Decimal.ToByte(FaithSpellCombobox4.SelectedIndex);
        }

        private void FaithSpellCombobox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithSpell5 = Decimal.ToByte(FaithSpellCombobox5.SelectedIndex);
        }

        private void FaithRankCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank1 = Decimal.ToByte(FaithRankCombobox1.SelectedIndex);
        }

        private void FaithRankCombobox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank2 = Decimal.ToByte(FaithRankCombobox2.SelectedIndex);
        }

        private void FaithRankCombobox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank3 = Decimal.ToByte(FaithRankCombobox3.SelectedIndex);
        }

        private void FaithRankCombobox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank4 = Decimal.ToByte(FaithRankCombobox4.SelectedIndex);
        }

        private void FaithRankCombobox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].FaithRank5 = Decimal.ToByte(FaithRankCombobox5.SelectedIndex);
        }

        private void ReasonRankCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank1 = Decimal.ToByte(ReasonRankCombobox1.SelectedIndex);
        }

        private void ReasonRankCombobox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank2 = Decimal.ToByte(ReasonRankCombobox2.SelectedIndex);
        }

        private void ReasonRankCombobox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank3 = Decimal.ToByte(ReasonRankCombobox3.SelectedIndex);
        }

        private void ReasonRankCombobox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank4 = Decimal.ToByte(ReasonRankCombobox4.SelectedIndex);
        }

        private void ReasonRankCombobox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SpellLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].ReasonRank5 = Decimal.ToByte(ReasonRankCombobox5.SelectedIndex);
        }

        private void SkillCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillLearned[skillListbox.SelectedIndex] = Decimal.ToByte(SkillCombobox.SelectedIndex);
        }

        private void skillRankCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillRank[skillListbox.SelectedIndex] = Decimal.ToByte(skillRankCombobox.SelectedIndex);
        }

        private void skillTypeCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillType[skillListbox.SelectedIndex] == 11)
            {
                currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillType[skillListbox.SelectedIndex] = 255;
            }
            else currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillType[skillListbox.SelectedIndex] = Decimal.ToByte(skillTypeCombobox.SelectedIndex);
        }

        private void skillListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (skillListbox.SelectedIndex != -1)
            {
                DisplayCurrentCharSkills();
            }
        }
        private void part1PersonalCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].PersonalskillPart1 = Decimal.ToByte(part1PersonalCombobox.SelectedIndex);
        }

        private void part2PersonalCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].PersonalskillPart2 = Decimal.ToByte(part2PersonalCombobox.SelectedIndex);
        }
        private void DisplayCurrentCharSkills()
        {
            skillRankCombobox.SelectedIndex = currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillRank[skillListbox.SelectedIndex];

            if (currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillType[skillListbox.SelectedIndex] == 255)
            {
                skillTypeCombobox.SelectedIndex = 11; //I dont want to make 255 entries when there only exist single digit valid ones
            }
            else skillTypeCombobox.SelectedIndex = currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillType[skillListbox.SelectedIndex];

            SkillCombobox.SelectedIndex = currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].SkillLearned[skillListbox.SelectedIndex];
            part1PersonalCombobox.SelectedIndex = currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].PersonalskillPart1;
            part2PersonalCombobox.SelectedIndex = currentPersonData.SkillLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].PersonalskillPart2;
        }

        private void inventoryCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[0] = Decimal.ToUInt16(inventoryCombobox1.SelectedIndex);
        }

        private void inventoryCombobox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[1] = Decimal.ToUInt16(inventoryCombobox2.SelectedIndex);
        }

        private void inventoryCombobox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[2] = Decimal.ToUInt16(inventoryCombobox3.SelectedIndex);
        }

        private void inventoryCombobox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[3] = Decimal.ToUInt16(inventoryCombobox4.SelectedIndex);
        }

        private void inventoryCombobox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[4] = Decimal.ToUInt16(inventoryCombobox5.SelectedIndex);
        }

        private void inventoryCombobox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].Weapons[5] = Decimal.ToUInt16(inventoryCombobox6.SelectedIndex);
        }

        private void inventoryFlagsNumbox_ValueChanged(object sender, EventArgs e)
        {
            currentPersonData.Weapons[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].wpnFlags = Decimal.ToUInt16(inventoryFlagsNumbox.Value);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combatArtsListbox.SelectedIndex != -1)
            {
                combatRankBox.SelectedIndex = currentPersonData.CombatArtsLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].CombatArtRank[combatArtsListbox.SelectedIndex];
                combatTypeBox.SelectedIndex = currentPersonData.CombatArtsLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].CombatArtType[combatArtsListbox.SelectedIndex];
                combatArtBox.SelectedIndex = currentPersonData.CombatArtsLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].CombatArtLearned[combatArtsListbox.SelectedIndex];
            }
        }

        private void combatTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.CombatArtsLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].CombatArtType[combatArtsListbox.SelectedIndex] = Decimal.ToByte(combatTypeBox.SelectedIndex);
            Console.WriteLine("Value being written to combat art type :" + currentPersonData.CombatArtsLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].CombatArtType[combatArtsListbox.SelectedIndex]);
        }

        private void combatArtBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.CombatArtsLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].CombatArtLearned[combatArtsListbox.SelectedIndex] = Decimal.ToByte(combatArtBox.SelectedIndex);
        }

        private void combatRankBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.CombatArtsLists[currentPersonData.Character[characterListBox.SelectedIndex].saveDataID].CombatArtRank[combatArtsListbox.SelectedIndex] = Decimal.ToByte(combatRankBox.SelectedIndex);
        }

        private void crest1Box_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].crest1 = Convert.ToByte(crest1ComboBox.SelectedIndex);
        }

        private void crest2Box_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPersonData.Character[characterListBox.SelectedIndex].crest2 = Convert.ToByte(crest2ComboBox.SelectedIndex);
        }
    }
}
