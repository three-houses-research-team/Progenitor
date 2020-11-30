using Microsoft.SqlServer.Server;
using Progenitor.DataFiles.Data;
using Progenitor.DataFiles.Data.Sections;
using Progenitor.IO.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeHousesPersonDataEditor;

namespace Progenitor
{
    public partial class FixedDataMain : Form
    {
        private DataFile currentDatafile;
        public FixedDataMain(string infile, string inpath)
        {
            InitializeComponent();
            DataEditorMain_Load();
            OpenFile(infile, inpath);
        }
        public int SelectedLanguage { get; set; }
        public List<String> msgDataNames { get; private set; }
        public List<uint> msgDataLanguageSections { get; private set; }
        public string filePath { get; set; }
        public string nameOfFile { get; set; }
        public List<String> OtherNames { get; private set; }
        public List<String> CAEffects { get; private set; }

        #region Open and Save File
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
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
            Weapon_LB_WeaponList.Items.Clear();
            Magic_LB_MagicList.Items.Clear();
            Turret_LB_TurretList.Items.Clear();
            Gambit_LB_GambitList.Items.Clear();
            MonsterAOE_LB_MonsterAOEList.Items.Clear();
            Equipment_LB_EquipmentList.Items.Clear();
            Items_LB_ItemsList.Items.Clear();
            CombatArt_LB_CombatArtList.Items.Clear();
            

            //Read the contents of the file into a stream
            currentDatafile = new DataFile();
            currentDatafile.ReadData(filePath);

            //Write Info in Misc Section
            if (currentDatafile.numOfPointers == 16)
            {
                if (tabControl1.TabPages.Contains(TB_Weapons) == false)
                {
                    tabControl1.Visible = true;
                    tabControl1.TabPages.Add(TB_Weapons);
                    tabControl1.TabPages.Add(TB_Magic);
                    tabControl1.TabPages.Add(TB_Turret);
                    tabControl1.TabPages.Add(TB_Gambit);
                    tabControl1.TabPages.Add(TB_MonsterAOE);
                    tabControl1.TabPages.Add(TB_Equipment);
                    tabControl1.TabPages.Add(TB_Items);
                    tabControl1.TabPages.Add(TB_CombatArt);
                }

                LoadLanguageTomsgDataNames();
                languageToolStripMenuItem.Visible = true;
            }
            else
            {
                //ResetLabels();
                return;
            }
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                using (EndianBinaryWriter fixed_data = new EndianBinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write), Endianness.Little))
                {
                    currentDatafile.WriteData(fixed_data);
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
                    saveFileDialog1.Filter = "fixed_data.bin (*.bin)|*.bin|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 1;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var savePath = saveFileDialog1.FileName;
                        using (EndianBinaryWriter fixed_data = new EndianBinaryWriter(File.Open(savePath, FileMode.Create, FileAccess.Write), Endianness.Little))
                        {
                            filePath = savePath; //now the Save File option writes here too
                            currentDatafile.WriteData(fixed_data);
                            MessageBox.Show("File saved to: " + savePath, "File saved");
                        }
                    }
                }
            }
            else
                MessageBox.Show("No file is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region Main Load
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
                //Store names that are not found in the msgData
                OtherNames = new List<string>();
                OtherNames.Add("Sword");//0
                OtherNames.Add("Lance");
                OtherNames.Add("Axe");
                OtherNames.Add("Bow");
                OtherNames.Add("Gauntlet");
                OtherNames.Add("Black Magic");
                OtherNames.Add("White Magic");
                OtherNames.Add("Dark Magic");
                OtherNames.Add("Equipment");
                OtherNames.Add("Items");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Shield");
                OtherNames.Add("Ring");
                OtherNames.Add("Staff");
                OtherNames.Add("Necklace");
                OtherNames.Add("Healing Item");
                OtherNames.Add("Seal");
                OtherNames.Add("Gold");
                OtherNames.Add("Torch");
                OtherNames.Add("Key");
                OtherNames.Add("Stat Booster");
                OtherNames.Add("Unknown");
                OtherNames.Add("Crest Sign");
                OtherNames.Add("Not Item");
                OtherNames.Add("Food");
                OtherNames.Add("E");
                OtherNames.Add("D");
                OtherNames.Add("C");
                OtherNames.Add("B");
                OtherNames.Add("A");
                OtherNames.Add("Deal Damage");
                OtherNames.Add("Heal on Ally");
                OtherNames.Add("Heal on All Allies");
                OtherNames.Add("Reduce Damage");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Heal Half Damage");
                OtherNames.Add("Use on Self");
                OtherNames.Add("Cure Stat on All Allies");
                OtherNames.Add("Give 7 Res on Ally");
                OtherNames.Add("Silences");
                OtherNames.Add("Rescues");
                OtherNames.Add("Warps");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                OtherNames.Add("Unknown");
                //Store Combat Effect Names
                CAEffects = new List<string>();
                CAEffects.Add("None");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Leaves Foe With 1 HP");
                CAEffects.Add("Increase Damage Based on Missing HP");
                CAEffects.Add("Hits Twice");
                CAEffects.Add("Hits 3 times");
                CAEffects.Add("Astra Skill");
                CAEffects.Add("Depletes all Durability for Amount*30% Might");
                CAEffects.Add("Pervent Foe from moving for 1 turn");
                CAEffects.Add("Prevent Foe from using Magic for 1 turn");
                CAEffects.Add("Minus 5 Str for 1 turn on Foe");
                CAEffects.Add("Minus 5 Def for 1 turn on Foe");
                CAEffects.Add("Minus 5 Res for 1 turn on Foe");
                CAEffects.Add("Restores 50% of Users HP");
                CAEffects.Add("Restores HP equal to 50% of Damage");
                CAEffects.Add("Moves 1 space in front of foe");
                CAEffects.Add("After Combat user moves 1 space back");
                CAEffects.Add("Triggers Follow up Attack");
                CAEffects.Add("Swap places with Ally");
                CAEffects.Add("Pushes Ally forward 1 spaces");
                CAEffects.Add("Move to other side of Ally");
                CAEffects.Add("Move User and Ally back 1 space");
                CAEffects.Add("Pushes Ally forward 2 spaces");
                CAEffects.Add("Triangle Attack Effect");
                CAEffects.Add("Can Kill Instantly");
                CAEffects.Add("Unknown");
                CAEffects.Add("Unknown");
                CAEffects.Add("Effective Against all Foes");
                CAEffects.Add("Unknown");
                CAEffects.Add("Might increases based on Mag");
                CAEffects.Add("Might increases based on Dex");
                CAEffects.Add("Might increases based on Spd");
                CAEffects.Add("Might increases based on Res");
                CAEffects.Add("Might increases based on Def");
                CAEffects.Add("Avoid all attacks next Combat");
                CAEffects.Add("Can move again after successful hit");
                CAEffects.Add("Might increases based on Cha");
                CAEffects.Add("Calculates Damage based on lower Def or Res");
                CAEffects.Add("Swap Possition with Ally");
                ClearBoxesForLangChange();

                //read List for Weapon Tab names
                for (int i = 0; i < currentDatafile.SectionBlockCount[0]; i++)
                {
                    Weapon_LB_WeaponList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[3756 + i]);
                }
                for (int i = 0; i < currentDatafile.SectionBlockCount[1]; i++)
                {
                    Magic_LB_MagicList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[7836 + i]);
                }
                for (int i = 0; i < currentDatafile.SectionBlockCount[2]; i++)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Turret_LB_TurretList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[5569]);
                                break;
                            }
                        case 1:
                            {
                                Turret_LB_TurretList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[5562]);
                                break;
                            }
                        case 2:
                            {
                                Turret_LB_TurretList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[5563]);
                                break;
                            }
                        default:
                            {
                                Turret_LB_TurretList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[5568]);
                                break;
                            }
                    }
                    
                }
                for (int i = 0; i < currentDatafile.SectionBlockCount[3]; i++)
                {
                    Gambit_LB_GambitList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[6614 + i]);
                }
                for (int i = 0; i < currentDatafile.SectionBlockCount[4]; i++)
                {
                    MonsterAOE_LB_MonsterAOEList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[8436 + i]);
                }
                for (int i = 0; i < currentDatafile.SectionBlockCount[5]; i++)
                {
                    Equipment_LB_EquipmentList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[4556 + i]);
                }
                for (int i = 0; i < currentDatafile.SectionBlockCount[6]; i++)
                {
                    Items_LB_ItemsList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[4656 + i]);
                }
                for (int i = 0; i < currentDatafile.SectionBlockCount[7]; i++)
                {
                    CombatArt_LB_CombatArtList.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[6014 + i]);
                }
                //read List for Crest Names
                for (int i = 0; i <= 85; i++)
                {
                    Weapon_CB_Crest.Items.Add(msgDataNames[i + 9590]);
                    Magic_CB_Crest.Items.Add(msgDataNames[i + 9590]);
                    Turret_CB_Crest.Items.Add(msgDataNames[i + 9590]);
                    Gambit_CB_Crest.Items.Add(msgDataNames[i + 9590]);
                    MonsterAOE_CB_Crest.Items.Add(msgDataNames[i + 9590]);
                    Equipment_CB_Crest.Items.Add(msgDataNames[i + 9590]);
                    Items_CB_Crest.Items.Add(msgDataNames[i + 9590]);
                }
                Weapon_CB_Crest.Items.Add(msgDataNames[9096 + 200]);
                Magic_CB_Crest.Items.Add(msgDataNames[9096 + 200]);
                Turret_CB_Crest.Items.Add(msgDataNames[9096 + 200]);
                Gambit_CB_Crest.Items.Add(msgDataNames[9096 + 200]);
                MonsterAOE_CB_Crest.Items.Add(msgDataNames[9096 + 200]);
                Equipment_CB_Crest.Items.Add(msgDataNames[9096 + 200]);
                Items_CB_Crest.Items.Add(msgDataNames[9096 + 200]);
                //set List for WeaponType Names
                for (int i = 0; i <= 11; i++)
                {
                    Weapon_CB_WeaponType.Items.Add(OtherNames[i]);
                    Magic_CB_WeaponType.Items.Add(OtherNames[i]);
                    Turret_CB_WeaponType.Items.Add(OtherNames[i]);
                    Gambit_CB_WeaponType.Items.Add(OtherNames[i]);
                    MonsterAOE_CB_WeaponType.Items.Add(OtherNames[i]);
                    Equipment_CB_WeaponType.Items.Add(OtherNames[i]);
                    Items_CB_WeaponType.Items.Add(OtherNames[i]);
                }
                //read List for ItemType Names
                for (int i = 0; i <= 13; i++)
                {
                    Weapon_CB_ItemType.Items.Add(OtherNames[i + 13]);
                    Magic_CB_ItemType.Items.Add(OtherNames[i + 13]);
                    Turret_CB_ItemType.Items.Add(OtherNames[i + 13]);
                    Gambit_CB_ItemType.Items.Add(OtherNames[i + 13]);
                    MonsterAOE_CB_ItemType.Items.Add(OtherNames[i + 13]);
                    Equipment_CB_ItemType.Items.Add(OtherNames[i + 13]);
                    Items_CB_ItemType.Items.Add(OtherNames[i + 13]);
                }
                //read List for WeaponRanks Names
                for (int i = 0; i <= 4; i++)
                {
                    Weapon_CB_WeaponRank.Items.Add(OtherNames[i + 27]);
                    Magic_CB_WeaponRank.Items.Add(OtherNames[i + 27]);
                    Turret_CB_WeaponRank.Items.Add(OtherNames[i + 27]);
                    Gambit_CB_WeaponRank.Items.Add(OtherNames[i + 27]);
                    MonsterAOE_CB_WeaponRank.Items.Add(OtherNames[i + 27]);
                    Equipment_CB_WeaponRank.Items.Add(OtherNames[i + 27]);
                    Items_CB_WeaponRank.Items.Add(OtherNames[i + 27]);
                }
                //read List for Weapon Effects
                for (int i = 0; i <= 24; i++)
                {
                    Weapon_CB_MagicEffect.Items.Add(OtherNames[i + 32]);
                    Magic_CB_MagicEffect.Items.Add(OtherNames[i + 32]);
                    Turret_CB_MagicEffect.Items.Add(OtherNames[i + 32]);
                    Gambit_CB_MagicEffect.Items.Add(OtherNames[i + 32]);
                    MonsterAOE_CB_MagicEffect.Items.Add(OtherNames[i + 32]);
                    Equipment_CB_MagicEffect.Items.Add(OtherNames[i + 32]);
                    Items_CB_MagicEffect.Items.Add(OtherNames[i + 32]);
                }
                //read List for CS Effects
                for (int i = 0; i <= 54; i++)
                {
                    CombatArt_EB_Effect.Items.Add(CAEffects[i]);
                }
                for (int i = 0; i <= 99; i++)
                {
                    CombatArt_CB_RequiredClass.Items.Add(msgDataNames[3453 + i]);
                }
                CombatArt_CB_RequiredClass.Items.Add(CAEffects[0]);
                for (int i = 0; i <= 499; i++)
                {
                    CombatArt_CB_RequireWeapon.Items.Add(msgDataNames[i + 3746]);
                }
                CombatArt_CB_RequireWeapon.Items.Add(CAEffects[0]);
            }
        }

        private void DataEditorMain_Load()
        {
            tabControl1.Visible = false;
            //Hide Tabs to prevent data being set
            tabControl1.TabPages.Remove(TB_Weapons);
            tabControl1.TabPages.Remove(TB_Magic);
            tabControl1.TabPages.Remove(TB_Turret);
            tabControl1.TabPages.Remove(TB_Gambit);
            tabControl1.TabPages.Remove(TB_MonsterAOE);
            tabControl1.TabPages.Remove(TB_Equipment);
            tabControl1.TabPages.Remove(TB_Items);
            tabControl1.TabPages.Remove(TB_CombatArt);

            SelectedLanguage = 1;
            languageToolStripMenuItem.Visible = false;
        }

        private void ClearBoxesForLangChange()
        {
            //Combo Boxes
            Weapon_CB_Crest.Items.Clear();
            Weapon_CB_ItemType.Items.Clear();
            Weapon_CB_MagicEffect.Items.Clear();
            Weapon_CB_WeaponRank.Items.Clear();
            Weapon_CB_WeaponType.Items.Clear();
            Magic_CB_Crest.Items.Clear();
            Magic_CB_ItemType.Items.Clear();
            Magic_CB_MagicEffect.Items.Clear();
            Magic_CB_WeaponRank.Items.Clear();
            Magic_CB_WeaponType.Items.Clear();
            Turret_CB_Crest.Items.Clear();
            Turret_CB_ItemType.Items.Clear();
            Turret_CB_MagicEffect.Items.Clear();
            Turret_CB_WeaponRank.Items.Clear();
            Turret_CB_WeaponType.Items.Clear();
            Gambit_CB_Crest.Items.Clear();
            Gambit_CB_ItemType.Items.Clear();
            Gambit_CB_MagicEffect.Items.Clear();
            Gambit_CB_WeaponRank.Items.Clear();
            Gambit_CB_WeaponType.Items.Clear();
            MonsterAOE_CB_Crest.Items.Clear();
            MonsterAOE_CB_ItemType.Items.Clear();
            MonsterAOE_CB_MagicEffect.Items.Clear();
            MonsterAOE_CB_WeaponRank.Items.Clear();
            MonsterAOE_CB_WeaponType.Items.Clear();
            Equipment_CB_Crest.Items.Clear();
            Equipment_CB_ItemType.Items.Clear();
            Equipment_CB_MagicEffect.Items.Clear();
            Equipment_CB_WeaponRank.Items.Clear();
            Equipment_CB_WeaponType.Items.Clear();
            Items_CB_Crest.Items.Clear();
            Items_CB_ItemType.Items.Clear();
            Items_CB_MagicEffect.Items.Clear();
            Items_CB_WeaponRank.Items.Clear();
            Items_CB_WeaponType.Items.Clear();
        }

        private string DecodeUTF8(string instring)
        {
            byte[] bytes = Encoding.Default.GetBytes(instring);
            string utf8string = Encoding.UTF8.GetString(bytes);
            return utf8string;
        }

        
        #endregion

        #region Weapon Tab Load
        private void Weapon_LB_WeaponList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Weapon_DisplayCurrent();
            Weapon_ReloadImage();
        }

        private void Weapon_DisplayCurrent()
        {
            var Current = currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex];
            //Weapon Data
            Weapon_TB_Name.Text = msgDataNames[Weapon_LB_WeaponList.SelectedIndex + 3756];
            Weapon_CB_WeaponType.SelectedIndex = Current.WeaponType;
            Weapon_CB_Crest.SelectedIndex = Current.Crest;
            Weapon_CB_WeaponRank.SelectedIndex = Current.WeaponRank;
            if (Current.ItemType == -1)
            {
                Weapon_CB_ItemType.SelectedIndex = 12;
            }
            else
            {
                Weapon_CB_ItemType.SelectedIndex = Current.ItemType;
            }
            Weapon_CB_MagicEffect.SelectedIndex = Current.MagicEffect;

            //weapon Stats
            Weapon_NB_MT.Value = Current.MT;
            Weapon_NB_Hit.Value = Current.Hit;
            Weapon_NB_Crit.Value = Current.Crit;
            Weapon_NB_WT.Value = Current.WT;
            Weapon_NB_MaxRange.Value = Current.MaxRange;
            Weapon_NB_MinRange.Value = Current.MinRange;
            Weapon_NB_Durability.Value = Current.Durability;
            Weapon_NB_HPMod.Value = Current.HPMod;
            Weapon_NB_Model.Value = Current.WeaponModel;

            //Unk Values
            Weapon_NB_unk_0x0.Value = Current.unk_0x0;
            Weapon_NB_unk_0x1.Value = Current.unk_0x1;
            Weapon_NB_unk_0x3.Value = Current.unk_0x3;
            Weapon_NB_unk_0x16.Value = Current.unk_0x16;
            Weapon_NB_unk_0x18.Value = Current.unk_0x18;
            Weapon_NB_ExtraEffect.Value = Current.ExtraEffect;

            //Flags
            Weapon_Check_Flag1.Checked = BitFlags.GetFlag(Current.flags01, 0);
            Weapon_Check_Flag2.Checked = BitFlags.GetFlag(Current.flags01, 1);
            Weapon_Check_Flag3.Checked = BitFlags.GetFlag(Current.flags01, 2);
            Weapon_Check_Flag4.Checked = BitFlags.GetFlag(Current.flags01, 3);
            Weapon_Check_Flag5.Checked = BitFlags.GetFlag(Current.flags01, 4);
            Weapon_Check_Flag6.Checked = BitFlags.GetFlag(Current.flags01, 5);
            Weapon_Check_Flag7.Checked = BitFlags.GetFlag(Current.flags01, 6);
            Weapon_Check_Flag8.Checked = BitFlags.GetFlag(Current.flags01, 7);

            Weapon_Check_Flag9.Checked = BitFlags.GetFlag(Current.flags02, 0);
            Weapon_Check_Flag10.Checked = BitFlags.GetFlag(Current.flags02, 1);
            Weapon_Check_Flag11.Checked = BitFlags.GetFlag(Current.flags02, 2);
            Weapon_Check_Flag12.Checked = BitFlags.GetFlag(Current.flags02, 3);
            Weapon_Check_Flag13.Checked = BitFlags.GetFlag(Current.flags02, 4);
            Weapon_Check_Flag14.Checked = BitFlags.GetFlag(Current.flags02, 5);
            Weapon_Check_Flag15.Checked = BitFlags.GetFlag(Current.flags02, 6);
            Weapon_Check_Flag16.Checked = BitFlags.GetFlag(Current.flags02, 7);

            Weapon_Check_Flag17.Checked = BitFlags.GetFlag(Current.flags03, 0);
            Weapon_Check_Flag18.Checked = BitFlags.GetFlag(Current.flags03, 1);
            Weapon_Check_Flag19.Checked = BitFlags.GetFlag(Current.flags03, 2);
            Weapon_Check_Flag20.Checked = BitFlags.GetFlag(Current.flags03, 3);
            Weapon_Check_Flag21.Checked = BitFlags.GetFlag(Current.flags03, 4);
            Weapon_Check_Flag22.Checked = BitFlags.GetFlag(Current.flags03, 5);
            Weapon_Check_Flag23.Checked = BitFlags.GetFlag(Current.flags03, 6);
            Weapon_Check_Flag24.Checked = BitFlags.GetFlag(Current.flags03, 7);

            //Effectivness
            Weapon_Check_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            Weapon_Check_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            Weapon_Check_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            Weapon_Check_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            Weapon_Check_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            Weapon_Check_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            Weapon_Check_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            Weapon_Check_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void Weapon_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            if (currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crest <= 21)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crest.ToString()));
            }
            else if (currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crest > 21 && currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crest <= 43)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex - 22].Crest.ToString()));
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            }
            Bitmap crest = new Bitmap(myStream);
            Weapon_Crest_PictureBox.Image = crest;

            //Load Icon Images
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            string cresttype = currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crest.ToString();
            int itemtype = currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].ItemType;
            if ( itemtype == -1 || itemtype <= 3)
            {
                if (BitFlags.GetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 6))
                {
                    myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}00.png", OtherNames[Weapon_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crest <= 43)
                {
                    if (BitFlags.GetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 7))
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Weapon_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Weapon_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                    else
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Weapon_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Weapon_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                }
                else if (cresttype == "44")
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Weapon_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Weapon_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (BitFlags.GetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 7))
                {
                    myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.Stone01.png");
                }
                else
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Weapon_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Weapon_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                
            }
            else
            {

            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.unkItem.png");
            }
            Bitmap weapon = new Bitmap(myStream);
            Weapon_Icon_Picturebox.Image = weapon;
        }

        #endregion

        #region Weapon Tab Write
        private void Weapon_NB_MT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].MT = Decimal.ToByte(Weapon_NB_MT.Value);
        }

        private void Weapon_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Hit = Decimal.ToByte(Weapon_NB_Hit.Value);
        }

        private void Weapon_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crit = Decimal.ToSByte(Weapon_NB_Crit.Value);
        }

        private void Weapon_NB_WT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].WT = Decimal.ToByte(Weapon_NB_WT.Value);
        }

        private void Weapon_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].MaxRange = Decimal.ToByte(Weapon_NB_MaxRange.Value);
        }

        private void Weapon_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].MinRange = Decimal.ToByte(Weapon_NB_MinRange.Value);
        }

        private void Weapon_NB_Durability_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Durability = Decimal.ToSByte(Weapon_NB_Durability.Value);
        }

        private void Weapon_NB_HPMod_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].HPMod = Decimal.ToByte(Weapon_NB_HPMod.Value);
        }

        private void Weapon_NB_Model_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].WeaponModel = Decimal.ToByte(Weapon_NB_Model.Value);
        }

        private void Weapon_NB_unk0x0_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].unk_0x0 = Decimal.ToByte(Weapon_NB_unk_0x0.Value);
        }

        private void Weapon_NB_unk0x1_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].unk_0x1 = Decimal.ToByte(Weapon_NB_unk_0x1.Value);
        }

        private void Weapon_NB_0x3_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].unk_0x3 = Decimal.ToByte(Weapon_NB_unk_0x3.Value);
        }

        private void Weapon_NB_0x10_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].unk_0x16 = Decimal.ToByte(Weapon_NB_unk_0x16.Value);
        }

        private void Weapon_NB_0x13_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].unk_0x18 = Decimal.ToByte(Weapon_NB_unk_0x18.Value);
        }

        private void Weapon_NB_ExtraEffect_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].ExtraEffect = Decimal.ToByte(Weapon_NB_ExtraEffect.Value);
        }

        private void Weapon_CB_MagicEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].MagicEffect = Decimal.ToByte(Weapon_CB_MagicEffect.SelectedIndex);
        }

        private void Weapon_CB_WeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].WeaponType = Decimal.ToByte(Weapon_CB_WeaponType.SelectedIndex);
            Weapon_ReloadImage();
        }

        private void Weapon_CB_Crest_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Crest = Decimal.ToByte(Weapon_CB_Crest.SelectedIndex);
            Weapon_ReloadImage();
        }

        private void Weapon_CB_WeaponRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].WeaponRank = Decimal.ToByte(Weapon_CB_WeaponRank.SelectedIndex);
        }

        private void Weapon_CB_ItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Weapon_CB_ItemType.SelectedIndex == 12)
            {
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].ItemType = Decimal.ToSByte(-1);
            }
            else
            {
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].ItemType = Decimal.ToSByte(Weapon_CB_ItemType.SelectedIndex);
            }
            Weapon_ReloadImage();
        }

        private void Weapon_Check_unkFlag1_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag1.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 0, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 0, false);
        }

        private void Weapon_Check_unkFlag2_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag2.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 1, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 1, false);
        }

        private void Weapon_Check_unkFlag3_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag3.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 2, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 2, false);
        }

        private void Weapon_Check_unkFlag4_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag4.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 3, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 3, false);
        }

        private void Weapon_Check_unkFlag5_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag5.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 4, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 4, false);
        }

        private void Weapon_Check_unkFlag6_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag6.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 5, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 5, false);
        }

        private void Weapon_Check_unkFlag7_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag7.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 6, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 6, false);
            Weapon_ReloadImage();
        }

        private void Weapon_Check_unkFlag8_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag8.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 7, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags01, 7, false);
            Weapon_ReloadImage();
        }

        private void Weapon_Check_unkFlag9_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag9.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 0, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 0, false);
        }

        private void Weapon_Check_unkFlag10_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag10.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 1, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 1, false);
        }

        private void Weapon_Check_unkFlag11_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag11.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 2, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 2, false);
        }

        private void Weapon_Check_unkFlag12_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag12.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 3, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 3, false);
        }

        private void Weapon_Check_unkFlag13_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag13.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 4, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 4, false);
        }

        private void Weapon_Check_unkFlag14_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag14.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 5, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 5, false);
        }

        private void Weapon_Check_unkFlag15_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag15.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 6, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 6, false);
        }

        private void Weapon_Check_unkFlag16_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag16.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 7, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags02, 7, false);
        }

        private void Weapon_Check_Magic_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag17.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 0, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 0, false);
        }

        private void Weapon_Check_IgnoreRes_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag18.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 1, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 1, false);
        }

        private void Weapon_Check_Foe1HP_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag19.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 2, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 2, false);
        }

        private void Weapon_Check_noFlying_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag20.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 3, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 3, false);
        }

        private void Weapon_Check_NotBrave_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag21.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 4, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 4, false);
        }

        private void Weapon_Check_Brave_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag22.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 5, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 5, false);
        }

        private void Weapon_Check_Regalia_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag23.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 6, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 6, false);
        }

        private void Weapon_Check_HeroRelic_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_Flag24.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 7, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].flags03, 7, false);
            Weapon_ReloadImage();
        }

        private void Weapon_Check_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_EInfantry.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 0, false);
        }

        private void Weapon_Check_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_EArmor.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 1, false);
        }
        private void Weapon_Check_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_ECavalry.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 2, false);
        }
        private void Weapon_Check_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_EFiler.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 3, false);
        }
        private void Weapon_Check_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_EDragon.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 4, false);
        }
        private void Weapon_Check_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_EMonster.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 5, false);
        }
        private void Weapon_Check_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_EReserve1.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 6, false);
        }
        private void Weapon_Check_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (Weapon_Check_EReserve2.Checked)
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Weapon[Weapon_LB_WeaponList.SelectedIndex].Effectiveness, 7, false);
        }
        #endregion

        #region Magic Tab Load
        private void Magic_LB_MagicList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Magic_DisplayCurrent();
            Magic_ReloadImage();
        }

        private void Magic_DisplayCurrent()
        {
            var Current = currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex];
            //Weapon Data
            Magic_TB_Name.Text = msgDataNames[Magic_LB_MagicList.SelectedIndex + 7836];
            Magic_CB_WeaponType.SelectedIndex = Current.WeaponType;
            Magic_CB_Crest.SelectedIndex = Current.Crest;
            Magic_CB_WeaponRank.SelectedIndex = Current.WeaponRank;
            if (Current.ItemType == -1)
            {
                Magic_CB_ItemType.SelectedIndex = 12;
            }
            else
            {
                Magic_CB_ItemType.SelectedIndex = Current.ItemType;
            }
            Magic_CB_MagicEffect.SelectedIndex = Current.MagicEffect;

            //weapon Stats
            Magic_NB_MT.Value = Current.MT;
            Magic_NB_Hit.Value = Current.Hit;
            Magic_NB_Crit.Value = Current.Crit;
            Magic_NB_WT.Value = Current.WT;
            Magic_NB_MaxRange.Value = Current.MaxRange;
            Magic_NB_MinRange.Value = Current.MinRange;
            Magic_NB_Durability.Value = Current.Durability;
            Magic_NB_HPMod.Value = Current.HPMod;
            Magic_NB_Model.Value = Current.WeaponModel;

            //Unk Values
            Magic_NB_unk_0x0.Value = Current.unk_0x0;
            Magic_NB_unk_0x1.Value = Current.unk_0x1;
            Magic_NB_unk_0x3.Value = Current.unk_0x3;
            Magic_NB_unk_0x16.Value = Current.unk_0x16;
            Magic_NB_unk_0x18.Value = Current.unk_0x18;
            Magic_NB_ExtraEffect.Value = Current.ExtraEffect;

            //Flags
            Magic_Check_Flag1.Checked = BitFlags.GetFlag(Current.flags01, 0);
            Magic_Check_Flag2.Checked = BitFlags.GetFlag(Current.flags01, 1);
            Magic_Check_Flag3.Checked = BitFlags.GetFlag(Current.flags01, 2);
            Magic_Check_Flag4.Checked = BitFlags.GetFlag(Current.flags01, 3);
            Magic_Check_Flag5.Checked = BitFlags.GetFlag(Current.flags01, 4);
            Magic_Check_Flag6.Checked = BitFlags.GetFlag(Current.flags01, 5);
            Magic_Check_Flag7.Checked = BitFlags.GetFlag(Current.flags01, 6);
            Magic_Check_Flag8.Checked = BitFlags.GetFlag(Current.flags01, 7);

            Magic_Check_Flag9.Checked = BitFlags.GetFlag(Current.flags02, 0);
            Magic_Check_Flag10.Checked = BitFlags.GetFlag(Current.flags02, 1);
            Magic_Check_Flag11.Checked = BitFlags.GetFlag(Current.flags02, 2);
            Magic_Check_Flag12.Checked = BitFlags.GetFlag(Current.flags02, 3);
            Magic_Check_Flag13.Checked = BitFlags.GetFlag(Current.flags02, 4);
            Magic_Check_Flag14.Checked = BitFlags.GetFlag(Current.flags02, 5);
            Magic_Check_Flag15.Checked = BitFlags.GetFlag(Current.flags02, 6);
            Magic_Check_Flag16.Checked = BitFlags.GetFlag(Current.flags02, 7);

            Magic_Check_Flag17.Checked = BitFlags.GetFlag(Current.flags03, 0);
            Magic_Check_Flag18.Checked = BitFlags.GetFlag(Current.flags03, 1);
            Magic_Check_Flag19.Checked = BitFlags.GetFlag(Current.flags03, 2);
            Magic_Check_Flag20.Checked = BitFlags.GetFlag(Current.flags03, 3);
            Magic_Check_Flag21.Checked = BitFlags.GetFlag(Current.flags03, 4);
            Magic_Check_Flag22.Checked = BitFlags.GetFlag(Current.flags03, 5);
            Magic_Check_Flag23.Checked = BitFlags.GetFlag(Current.flags03, 6);
            Magic_Check_Flag24.Checked = BitFlags.GetFlag(Current.flags03, 7);

            //Effectivness
            Magic_Check_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            Magic_Check_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            Magic_Check_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            Magic_Check_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            Magic_Check_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            Magic_Check_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            Magic_Check_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            Magic_Check_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void Magic_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            if (currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crest <= 21)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crest.ToString()));
            }
            else if (currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crest > 21 && currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crest <= 43)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex - 22].Crest.ToString()));
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            }
            Bitmap crest = new Bitmap(myStream);
            Magic_Crest_PictureBox.Image = crest;

            //Load Icon Images
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            string cresttype = currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crest.ToString();
            int itemtype = currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].ItemType;
            if (itemtype == -1 || itemtype <= 3)
            {
                
                if (BitFlags.GetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 6))
                {
                    myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}00.png", OtherNames[Magic_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crest <= 43)
                {
                    if (BitFlags.GetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 7))
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Magic_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Magic_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                    else
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Magic_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Magic_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                }
                else if (cresttype == "44")
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Magic_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Magic_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (BitFlags.GetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 7))
                {
                    myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.Stone01.png");
                }
                else
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Magic_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Magic_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.unkItem.png");
            }
            Bitmap weapon = new Bitmap(myStream);
            Magic_Icon_Picturebox.Image = weapon;
        }

        #endregion

        #region Magic Tab Write
        private void Magic_NB_MT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].MT = Decimal.ToByte(Magic_NB_MT.Value);
        }

        private void Magic_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Hit = Decimal.ToByte(Magic_NB_Hit.Value);
        }

        private void Magic_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crit = Decimal.ToSByte(Magic_NB_Crit.Value);
        }

        private void Magic_NB_WT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].WT = Decimal.ToByte(Magic_NB_WT.Value);
        }

        private void Magic_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].MaxRange = Decimal.ToByte(Magic_NB_MaxRange.Value);
        }

        private void Magic_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].MinRange = Decimal.ToByte(Magic_NB_MinRange.Value);
        }

        private void Magic_NB_Durability_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Durability = Decimal.ToSByte(Magic_NB_Durability.Value);
        }

        private void Magic_NB_HPMod_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].HPMod = Decimal.ToByte(Magic_NB_HPMod.Value);
        }

        private void Magic_NB_Model_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].WeaponModel = Decimal.ToByte(Magic_NB_Model.Value);
        }

        private void Magic_NB_unk0x0_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].unk_0x0 = Decimal.ToByte(Magic_NB_unk_0x0.Value);
        }

        private void Magic_NB_unk0x1_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].unk_0x1 = Decimal.ToByte(Magic_NB_unk_0x1.Value);
        }

        private void Magic_NB_0x3_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].unk_0x3 = Decimal.ToByte(Magic_NB_unk_0x3.Value);
        }

        private void Magic_NB_0x10_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].unk_0x16 = Decimal.ToByte(Magic_NB_unk_0x16.Value);
        }

        private void Magic_NB_0x13_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].unk_0x18 = Decimal.ToByte(Magic_NB_unk_0x18.Value);
        }

        private void Magic_NB_ExtraEffect_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].ExtraEffect = Decimal.ToByte(Magic_NB_ExtraEffect.Value);
        }

        private void Magic_CB_MagicEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].MagicEffect = Decimal.ToByte(Magic_CB_MagicEffect.SelectedIndex);
        }

        private void Magic_CB_WeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].WeaponType = Decimal.ToByte(Magic_CB_WeaponType.SelectedIndex);
            Magic_ReloadImage();
        }

        private void Magic_CB_Crest_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Crest = Decimal.ToByte(Magic_CB_Crest.SelectedIndex);
            Magic_ReloadImage();
        }

        private void Magic_CB_WeaponRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].WeaponRank = Decimal.ToByte(Magic_CB_WeaponRank.SelectedIndex);
        }

        private void Magic_CB_ItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Magic_CB_ItemType.SelectedIndex == 12)
            {
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].ItemType = Decimal.ToSByte(-1);
            }
            else
            {
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].ItemType = Decimal.ToSByte(Magic_CB_ItemType.SelectedIndex);
            }
            Magic_ReloadImage();
        }

        private void Magic_Check_Flag1_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag1.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 0, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 0, false);
        }

        private void Magic_Check_Flag2_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag2.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 1, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 1, false);
        }

        private void Magic_Check_Flag3_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag3.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 2, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 2, false);
        }

        private void Magic_Check_Flag4_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag4.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 3, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 3, false);
        }

        private void Magic_Check_Flag5_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag5.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 4, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 4, false);
        }

        private void Magic_Check_Flag6_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag6.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 5, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 5, false);
        }

        private void Magic_Check_Flag7_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag7.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 6, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 6, false);
            Magic_ReloadImage();
        }

        private void Magic_Check_Flag8_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag8.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 7, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags01, 7, false);
            Magic_ReloadImage();
        }

        private void Magic_Check_Flag9_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag9.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 0, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 0, false);
        }

        private void Magic_Check_Flag10_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag10.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 1, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 1, false);
        }

        private void Magic_Check_Flag11_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag11.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 2, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 2, false);
        }

        private void Magic_Check_Flag12_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag12.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 3, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 3, false);
        }

        private void Magic_Check_Flag13_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag13.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 4, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 4, false);
        }

        private void Magic_Check_Flag14_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag14.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 5, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 5, false);
        }

        private void Magic_Check_Flag15_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag15.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 6, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 6, false);
        }

        private void Magic_Check_Flag16_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag16.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 7, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags02, 7, false);
        }

        private void Magic_Check_Flag17_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag17.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 0, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 0, false);
        }

        private void Magic_Check_Flag18_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag18.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 1, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 1, false);
        }

        private void Magic_Check_Flag19_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag19.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 2, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 2, false);
        }

        private void Magic_Check_Flag20_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag20.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 3, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 3, false);
        }

        private void Magic_Check_Flag21_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag21.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 4, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 4, false);
        }

        private void Magic_Check_Flag22_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag22.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 5, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 5, false);
        }

        private void Magic_Check_Flag23_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag23.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 6, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 6, false);
        }

        private void Magic_Check_Flag24_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_Flag24.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 7, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].flags03, 7, false);
            Magic_ReloadImage();
        }

        private void Magic_Check_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_EInfantry.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 0, false);
        }

        private void Magic_Check_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_EArmor.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 1, false);
        }
        private void Magic_Check_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_ECavalry.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 2, false);
        }
        private void Magic_Check_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_EFiler.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 3, false);
        }
        private void Magic_Check_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_EDragon.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 4, false);
        }
        private void Magic_Check_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_EMonster.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 5, false);
        }
        private void Magic_Check_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_EReserve1.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 6, false);
        }
        private void Magic_Check_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (Magic_Check_EReserve2.Checked)
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Magic[Magic_LB_MagicList.SelectedIndex].Effectiveness, 7, false);
        }
        #endregion

        #region Turret Tab Load
        private void Turret_LB_TurretList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Turret_DisplayCurrent();
            Turret_ReloadImage();
        }

        private void Turret_DisplayCurrent()
        {
            var Current = currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex];
            //Weapon Data
            switch (Turret_LB_TurretList.SelectedIndex)
            {
                case 0:
                    {
                        Turret_TB_Name.Text = msgDataNames[5569];
                        break;
                    }
                case 1:
                    {
                        Turret_TB_Name.Text = msgDataNames[5562];
                        break;
                    }
                case 2:
                    {
                        Turret_TB_Name.Text = msgDataNames[5563];
                        break;
                    }
                default:
                    {
                        Turret_TB_Name.Text = msgDataNames[5568];
                        break;
                    }
            }
            
            Turret_CB_WeaponType.SelectedIndex = Current.WeaponType;
            Turret_CB_Crest.SelectedIndex = Current.Crest;
            Turret_CB_WeaponRank.SelectedIndex = Current.WeaponRank;
            if (Current.ItemType == -1)
            {
                Turret_CB_ItemType.SelectedIndex = 12;
            }
            else
            {
                Turret_CB_ItemType.SelectedIndex = Current.ItemType;
            }
            Turret_CB_MagicEffect.SelectedIndex = Current.MagicEffect;

            //weapon Stats
            Turret_NB_MT.Value = Current.MT;
            Turret_NB_Hit.Value = Current.Hit;
            Turret_NB_Crit.Value = Current.Crit;
            Turret_NB_WT.Value = Current.WT;
            Turret_NB_MaxRange.Value = Current.MaxRange;
            Turret_NB_MinRange.Value = Current.MinRange;
            Turret_NB_Durability.Value = Current.Durability;
            Turret_NB_HPMod.Value = Current.HPMod;
            Turret_NB_Model.Value = Current.WeaponModel;

            //Unk Values
            Turret_NB_unk_0x0.Value = Current.unk_0x0;
            Turret_NB_unk_0x1.Value = Current.unk_0x1;
            Turret_NB_unk_0x3.Value = Current.unk_0x3;
            Turret_NB_unk_0x16.Value = Current.unk_0x16;
            Turret_NB_unk_0x18.Value = Current.unk_0x18;
            Turret_NB_ExtraEffect.Value = Current.ExtraEffect;

            //Flags
            Turret_Check_Flag1.Checked = BitFlags.GetFlag(Current.flags01, 0);
            Turret_Check_Flag2.Checked = BitFlags.GetFlag(Current.flags01, 1);
            Turret_Check_Flag3.Checked = BitFlags.GetFlag(Current.flags01, 2);
            Turret_Check_Flag4.Checked = BitFlags.GetFlag(Current.flags01, 3);
            Turret_Check_Flag5.Checked = BitFlags.GetFlag(Current.flags01, 4);
            Turret_Check_Flag6.Checked = BitFlags.GetFlag(Current.flags01, 5);
            Turret_Check_Flag7.Checked = BitFlags.GetFlag(Current.flags01, 6);
            Turret_Check_Flag8.Checked = BitFlags.GetFlag(Current.flags01, 7);

            Turret_Check_Flag9.Checked = BitFlags.GetFlag(Current.flags02, 0);
            Turret_Check_Flag10.Checked = BitFlags.GetFlag(Current.flags02, 1);
            Turret_Check_Flag11.Checked = BitFlags.GetFlag(Current.flags02, 2);
            Turret_Check_Flag12.Checked = BitFlags.GetFlag(Current.flags02, 3);
            Turret_Check_Flag13.Checked = BitFlags.GetFlag(Current.flags02, 4);
            Turret_Check_Flag14.Checked = BitFlags.GetFlag(Current.flags02, 5);
            Turret_Check_Flag15.Checked = BitFlags.GetFlag(Current.flags02, 6);
            Turret_Check_Flag16.Checked = BitFlags.GetFlag(Current.flags02, 7);

            Turret_Check_Flag17.Checked = BitFlags.GetFlag(Current.flags03, 0);
            Turret_Check_Flag18.Checked = BitFlags.GetFlag(Current.flags03, 1);
            Turret_Check_Flag19.Checked = BitFlags.GetFlag(Current.flags03, 2);
            Turret_Check_Flag20.Checked = BitFlags.GetFlag(Current.flags03, 3);
            Turret_Check_Flag21.Checked = BitFlags.GetFlag(Current.flags03, 4);
            Turret_Check_Flag22.Checked = BitFlags.GetFlag(Current.flags03, 5);
            Turret_Check_Flag23.Checked = BitFlags.GetFlag(Current.flags03, 6);
            Turret_Check_Flag24.Checked = BitFlags.GetFlag(Current.flags03, 7);

            //Effectivness
            Turret_Check_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            Turret_Check_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            Turret_Check_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            Turret_Check_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            Turret_Check_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            Turret_Check_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            Turret_Check_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            Turret_Check_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void Turret_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            if (currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crest <= 21)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crest.ToString()));
            }
            else if (currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crest > 21 && currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crest <= 43)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex - 22].Crest.ToString()));
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            }
            Bitmap crest = new Bitmap(myStream);
            Turret_Crest_PictureBox.Image = crest;

            //Load Icon Images
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            string cresttype = currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crest.ToString();
            int itemtype = currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].ItemType;
            if (itemtype == -1 || itemtype <= 3)
            {
                if (BitFlags.GetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 6))
                {
                    myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}00.png", OtherNames[Turret_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crest <= 43)
                {
                    if (BitFlags.GetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 7))
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Turret_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Turret_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                    else
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Turret_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Turret_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                }
                else if (cresttype == "44")
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Turret_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Turret_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (BitFlags.GetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 7))
                {
                    myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.Stone01.png");
                }
                else
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Turret_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Turret_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }

            }
            else
            {

            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.unkItem.png");
            }
            Bitmap weapon = new Bitmap(myStream);
            Turret_Icon_Picturebox.Image = weapon;
        }

        #endregion

        #region Turret Tab Write
        private void Turret_NB_MT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].MT = Decimal.ToByte(Turret_NB_MT.Value);
        }

        private void Turret_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Hit = Decimal.ToByte(Turret_NB_Hit.Value);
        }

        private void Turret_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crit = Decimal.ToSByte(Turret_NB_Crit.Value);
        }

        private void Turret_NB_WT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].WT = Decimal.ToByte(Turret_NB_WT.Value);
        }

        private void Turret_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].MaxRange = Decimal.ToByte(Turret_NB_MaxRange.Value);
        }

        private void Turret_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].MinRange = Decimal.ToByte(Turret_NB_MinRange.Value);
        }

        private void Turret_NB_Durability_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Durability = Decimal.ToSByte(Turret_NB_Durability.Value);
        }

        private void Turret_NB_HPMod_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].HPMod = Decimal.ToByte(Turret_NB_HPMod.Value);
        }

        private void Turret_NB_Model_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].WeaponModel = Decimal.ToByte(Turret_NB_Model.Value);
        }

        private void Turret_NB_unk0x0_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].unk_0x0 = Decimal.ToByte(Turret_NB_unk_0x0.Value);
        }

        private void Turret_NB_unk0x1_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].unk_0x1 = Decimal.ToByte(Turret_NB_unk_0x1.Value);
        }

        private void Turret_NB_0x3_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].unk_0x3 = Decimal.ToByte(Turret_NB_unk_0x3.Value);
        }

        private void Turret_NB_0x10_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].unk_0x16 = Decimal.ToByte(Turret_NB_unk_0x16.Value);
        }

        private void Turret_NB_0x13_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].unk_0x18 = Decimal.ToByte(Turret_NB_unk_0x18.Value);
        }

        private void Turret_NB_ExtraEffect_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].ExtraEffect = Decimal.ToByte(Turret_NB_ExtraEffect.Value);
        }

        private void Turret_CB_MagicEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].MagicEffect = Decimal.ToByte(Turret_CB_MagicEffect.SelectedIndex);
        }

        private void Turret_CB_WeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].WeaponType = Decimal.ToByte(Turret_CB_WeaponType.SelectedIndex);
            Turret_ReloadImage();
        }

        private void Turret_CB_Crest_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Crest = Decimal.ToByte(Turret_CB_Crest.SelectedIndex);
            Turret_ReloadImage();
        }

        private void Turret_CB_WeaponRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].WeaponRank = Decimal.ToByte(Turret_CB_WeaponRank.SelectedIndex);
        }

        private void Turret_CB_ItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Turret_CB_ItemType.SelectedIndex == 12)
            {
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].ItemType = Decimal.ToSByte(-1);
            }
            else
            {
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].ItemType = Decimal.ToSByte(Turret_CB_ItemType.SelectedIndex);
            }
            Turret_ReloadImage();
        }

        private void Turret_Check_Flag1_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag1.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 0, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 0, false);
        }

        private void Turret_Check_Flag2_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag2.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 1, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 1, false);
        }

        private void Turret_Check_Flag3_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag3.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 2, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 2, false);
        }

        private void Turret_Check_Flag4_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag4.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 3, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 3, false);
        }

        private void Turret_Check_Flag5_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag5.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 4, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 4, false);
        }

        private void Turret_Check_Flag6_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag6.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 5, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 5, false);
        }

        private void Turret_Check_Flag7_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag7.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 6, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 6, false);
            Turret_ReloadImage();
        }

        private void Turret_Check_Flag8_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag8.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 7, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags01, 7, false);
            Turret_ReloadImage();
        }

        private void Turret_Check_Flag9_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag9.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 0, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 0, false);
        }

        private void Turret_Check_Flag10_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag10.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 1, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 1, false);
        }

        private void Turret_Check_Flag11_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag11.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 2, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 2, false);
        }

        private void Turret_Check_Flag12_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag12.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 3, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 3, false);
        }

        private void Turret_Check_Flag13_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag13.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 4, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 4, false);
        }

        private void Turret_Check_Flag14_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag14.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 5, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 5, false);
        }

        private void Turret_Check_Flag15_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag15.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 6, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 6, false);
        }

        private void Turret_Check_Flag16_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag16.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 7, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags02, 7, false);
        }

        private void Turret_Check_Flag17_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag17.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 0, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 0, false);
        }

        private void Turret_Check_Flag18_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag18.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 1, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 1, false);
        }

        private void Turret_Check_Flag19_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag19.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 2, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 2, false);
        }

        private void Turret_Check_Flag20_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag20.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 3, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 3, false);
        }

        private void Turret_Check_Flag21_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag21.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 4, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 4, false);
        }

        private void Turret_Check_Flag22_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag22.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 5, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 5, false);
        }

        private void Turret_Check_Flag23_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag23.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 6, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 6, false);
        }

        private void Turret_Check_Flag24_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_Flag24.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 7, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].flags03, 7, false);
            Turret_ReloadImage();
        }

        private void Turret_Check_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_EInfantry.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 0, false);
        }

        private void Turret_Check_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_EArmor.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 1, false);
        }
        private void Turret_Check_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_ECavalry.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 2, false);
        }
        private void Turret_Check_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_EFiler.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 3, false);
        }
        private void Turret_Check_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_EDragon.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 4, false);
        }
        private void Turret_Check_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_EMonster.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 5, false);
        }
        private void Turret_Check_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_EReserve1.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 6, false);
        }
        private void Turret_Check_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (Turret_Check_EReserve2.Checked)
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Turret[Turret_LB_TurretList.SelectedIndex].Effectiveness, 7, false);
        }
        #endregion

        #region Gambit Tab Load
        private void Gambit_LB_GambitList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Gambit_DisplayCurrent();
            Gambit_ReloadImage();
        }

        private void Gambit_DisplayCurrent()
        {
            var Current = currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex];
            //Weapon Data
            Gambit_TB_Name.Text = msgDataNames[Gambit_LB_GambitList.SelectedIndex + 6614];
            Gambit_CB_WeaponType.SelectedIndex = Current.WeaponType;
            Gambit_CB_Crest.SelectedIndex = Current.Crest;
            Gambit_CB_WeaponRank.SelectedIndex = Current.WeaponRank;
            if (Current.ItemType == -1)
            {
                Gambit_CB_ItemType.SelectedIndex = 12;
            }
            else
            {
                Gambit_CB_ItemType.SelectedIndex = Current.ItemType;
            }
            Gambit_CB_MagicEffect.SelectedIndex = Current.MagicEffect;

            //weapon Stats
            Gambit_NB_MT.Value = Current.MT;
            Gambit_NB_Hit.Value = Current.Hit;
            Gambit_NB_Crit.Value = Current.Crit;
            Gambit_NB_WT.Value = Current.WT;
            Gambit_NB_MaxRange.Value = Current.MaxRange;
            Gambit_NB_MinRange.Value = Current.MinRange;
            Gambit_NB_Durability.Value = Current.Durability;
            Gambit_NB_HPMod.Value = Current.HPMod;
            Gambit_NB_Model.Value = Current.WeaponModel;

            //Unk Values
            Gambit_NB_unk_0x0.Value = Current.unk_0x0;
            Gambit_NB_unk_0x1.Value = Current.unk_0x1;
            Gambit_NB_unk_0x3.Value = Current.unk_0x3;
            Gambit_NB_unk_0x16.Value = Current.unk_0x16;
            Gambit_NB_unk_0x18.Value = Current.unk_0x18;
            Gambit_NB_ExtraEffect.Value = Current.ExtraEffect;

            //Flags
            Gambit_Check_Flag1.Checked = BitFlags.GetFlag(Current.flags01, 0);
            Gambit_Check_Flag2.Checked = BitFlags.GetFlag(Current.flags01, 1);
            Gambit_Check_Flag3.Checked = BitFlags.GetFlag(Current.flags01, 2);
            Gambit_Check_Flag4.Checked = BitFlags.GetFlag(Current.flags01, 3);
            Gambit_Check_Flag5.Checked = BitFlags.GetFlag(Current.flags01, 4);
            Gambit_Check_Flag6.Checked = BitFlags.GetFlag(Current.flags01, 5);
            Gambit_Check_Flag7.Checked = BitFlags.GetFlag(Current.flags01, 6);
            Gambit_Check_Flag8.Checked = BitFlags.GetFlag(Current.flags01, 7);

            Gambit_Check_Flag9.Checked = BitFlags.GetFlag(Current.flags02, 0);
            Gambit_Check_Flag10.Checked = BitFlags.GetFlag(Current.flags02, 1);
            Gambit_Check_Flag11.Checked = BitFlags.GetFlag(Current.flags02, 2);
            Gambit_Check_Flag12.Checked = BitFlags.GetFlag(Current.flags02, 3);
            Gambit_Check_Flag13.Checked = BitFlags.GetFlag(Current.flags02, 4);
            Gambit_Check_Flag14.Checked = BitFlags.GetFlag(Current.flags02, 5);
            Gambit_Check_Flag15.Checked = BitFlags.GetFlag(Current.flags02, 6);
            Gambit_Check_Flag16.Checked = BitFlags.GetFlag(Current.flags02, 7);

            Gambit_Check_Flag17.Checked = BitFlags.GetFlag(Current.flags03, 0);
            Gambit_Check_Flag18.Checked = BitFlags.GetFlag(Current.flags03, 1);
            Gambit_Check_Flag19.Checked = BitFlags.GetFlag(Current.flags03, 2);
            Gambit_Check_Flag20.Checked = BitFlags.GetFlag(Current.flags03, 3);
            Gambit_Check_Flag21.Checked = BitFlags.GetFlag(Current.flags03, 4);
            Gambit_Check_Flag22.Checked = BitFlags.GetFlag(Current.flags03, 5);
            Gambit_Check_Flag23.Checked = BitFlags.GetFlag(Current.flags03, 6);
            Gambit_Check_Flag24.Checked = BitFlags.GetFlag(Current.flags03, 7);

            //Effectivness
            Gambit_Check_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            Gambit_Check_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            Gambit_Check_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            Gambit_Check_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            Gambit_Check_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            Gambit_Check_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            Gambit_Check_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            Gambit_Check_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void Gambit_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            if (currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crest <= 21)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crest.ToString()));
            }
            else if (currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crest > 21 && currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crest <= 43)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex - 22].Crest.ToString()));
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            }
            Bitmap crest = new Bitmap(myStream);
            Gambit_Crest_PictureBox.Image = crest;

            //Load Icon Images
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            string cresttype = currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crest.ToString();
            int itemtype = currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].ItemType;
            if (itemtype == -1 || itemtype <= 3)
            {

                if (BitFlags.GetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 6))
                {
                    myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}00.png", OtherNames[Gambit_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crest <= 43)
                {
                    if (BitFlags.GetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 7))
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Gambit_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Gambit_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                    else
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Gambit_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Gambit_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                }
                else if (cresttype == "44")
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Gambit_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Gambit_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (BitFlags.GetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 7))
                {
                    myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.Stone01.png");
                }
                else
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Gambit_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Gambit_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.unkItem.png");
            }
            Bitmap weapon = new Bitmap(myStream);
            Gambit_Icon_Picturebox.Image = weapon;
        }

        #endregion

        #region Gambit Tab Write
        private void Gambit_NB_MT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].MT = Decimal.ToByte(Gambit_NB_MT.Value);
        }

        private void Gambit_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Hit = Decimal.ToByte(Gambit_NB_Hit.Value);
        }

        private void Gambit_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crit = Decimal.ToSByte(Gambit_NB_Crit.Value);
        }

        private void Gambit_NB_WT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].WT = Decimal.ToByte(Gambit_NB_WT.Value);
        }

        private void Gambit_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].MaxRange = Decimal.ToByte(Gambit_NB_MaxRange.Value);
        }

        private void Gambit_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].MinRange = Decimal.ToByte(Gambit_NB_MinRange.Value);
        }

        private void Gambit_NB_Durability_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Durability = Decimal.ToSByte(Gambit_NB_Durability.Value);
        }

        private void Gambit_NB_HPMod_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].HPMod = Decimal.ToByte(Gambit_NB_HPMod.Value);
        }

        private void Gambit_NB_Model_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].WeaponModel = Decimal.ToByte(Gambit_NB_Model.Value);
        }

        private void Gambit_NB_unk0x0_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].unk_0x0 = Decimal.ToByte(Gambit_NB_unk_0x0.Value);
        }

        private void Gambit_NB_unk0x1_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].unk_0x1 = Decimal.ToByte(Gambit_NB_unk_0x1.Value);
        }

        private void Gambit_NB_0x3_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].unk_0x3 = Decimal.ToByte(Gambit_NB_unk_0x3.Value);
        }

        private void Gambit_NB_0x10_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].unk_0x16 = Decimal.ToByte(Gambit_NB_unk_0x16.Value);
        }

        private void Gambit_NB_0x13_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].unk_0x18 = Decimal.ToByte(Gambit_NB_unk_0x18.Value);
        }

        private void Gambit_NB_ExtraEffect_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].ExtraEffect = Decimal.ToByte(Gambit_NB_ExtraEffect.Value);
        }

        private void Gambit_CB_MagicEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].MagicEffect = Decimal.ToByte(Gambit_CB_MagicEffect.SelectedIndex);
        }

        private void Gambit_CB_WeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].WeaponType = Decimal.ToByte(Gambit_CB_WeaponType.SelectedIndex);
            Gambit_ReloadImage();
        }

        private void Gambit_CB_Crest_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Crest = Decimal.ToByte(Gambit_CB_Crest.SelectedIndex);
            Gambit_ReloadImage();
        }

        private void Gambit_CB_WeaponRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].WeaponRank = Decimal.ToByte(Gambit_CB_WeaponRank.SelectedIndex);
        }

        private void Gambit_CB_ItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Gambit_CB_ItemType.SelectedIndex == 12)
            {
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].ItemType = Decimal.ToSByte(-1);
            }
            else
            {
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].ItemType = Decimal.ToSByte(Gambit_CB_ItemType.SelectedIndex);
            }
            Gambit_ReloadImage();
        }

        private void Gambit_Check_Flag1_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag1.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 0, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 0, false);
        }

        private void Gambit_Check_Flag2_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag2.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 1, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 1, false);
        }

        private void Gambit_Check_Flag3_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag3.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 2, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 2, false);
        }

        private void Gambit_Check_Flag4_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag4.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 3, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 3, false);
        }

        private void Gambit_Check_Flag5_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag5.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 4, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 4, false);
        }

        private void Gambit_Check_Flag6_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag6.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 5, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 5, false);
        }

        private void Gambit_Check_Flag7_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag7.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 6, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 6, false);
            Gambit_ReloadImage();
        }

        private void Gambit_Check_Flag8_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag8.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 7, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags01, 7, false);
            Gambit_ReloadImage();
        }

        private void Gambit_Check_Flag9_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag9.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 0, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 0, false);
        }

        private void Gambit_Check_Flag10_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag10.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 1, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 1, false);
        }

        private void Gambit_Check_Flag11_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag11.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 2, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 2, false);
        }

        private void Gambit_Check_Flag12_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag12.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 3, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 3, false);
        }

        private void Gambit_Check_Flag13_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag13.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 4, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 4, false);
        }

        private void Gambit_Check_Flag14_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag14.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 5, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 5, false);
        }

        private void Gambit_Check_Flag15_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag15.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 6, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 6, false);
        }

        private void Gambit_Check_Flag16_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag16.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 7, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags02, 7, false);
        }

        private void Gambit_Check_Flag17_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag17.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 0, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 0, false);
        }

        private void Gambit_Check_Flag18_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag18.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 1, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 1, false);
        }

        private void Gambit_Check_Flag19_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag19.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 2, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 2, false);
        }

        private void Gambit_Check_Flag20_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag20.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 3, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 3, false);
        }

        private void Gambit_Check_Flag21_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag21.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 4, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 4, false);
        }

        private void Gambit_Check_Flag22_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag22.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 5, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 5, false);
        }

        private void Gambit_Check_Flag23_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag23.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 6, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 6, false);
        }

        private void Gambit_Check_Flag24_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_Flag24.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 7, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].flags03, 7, false);
            Gambit_ReloadImage();
        }

        private void Gambit_Check_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_EInfantry.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 0, false);
        }

        private void Gambit_Check_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_EArmor.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 1, false);
        }
        private void Gambit_Check_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_ECavalry.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 2, false);
        }
        private void Gambit_Check_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_EFiler.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 3, false);
        }
        private void Gambit_Check_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_EDragon.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 4, false);
        }
        private void Gambit_Check_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_EMonster.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 5, false);
        }
        private void Gambit_Check_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_EReserve1.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 6, false);
        }
        private void Gambit_Check_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (Gambit_Check_EReserve2.Checked)
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Gambit[Gambit_LB_GambitList.SelectedIndex].Effectiveness, 7, false);
        }
        #endregion

        #region MonsterAOE Tab Load
        private void MonsterAOE_LB_MonsterAOEList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MonsterAOE_DisplayCurrent();
            MonsterAOE_ReloadImage();
        }

        private void MonsterAOE_DisplayCurrent()
        {
            var Current = currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex];
            //Weapon Data
            MonsterAOE_TB_Name.Text = msgDataNames[MonsterAOE_LB_MonsterAOEList.SelectedIndex + 8436];
            MonsterAOE_CB_WeaponType.SelectedIndex = Current.WeaponType;
            MonsterAOE_CB_Crest.SelectedIndex = Current.Crest;
            MonsterAOE_CB_WeaponRank.SelectedIndex = Current.WeaponRank;
            if (Current.ItemType == -1)
            {
                MonsterAOE_CB_ItemType.SelectedIndex = 12;
            }
            else
            {
                MonsterAOE_CB_ItemType.SelectedIndex = Current.ItemType;
            }
            MonsterAOE_CB_MagicEffect.SelectedIndex = Current.MagicEffect;

            //weapon Stats
            MonsterAOE_NB_MT.Value = Current.MT;
            MonsterAOE_NB_Hit.Value = Current.Hit;
            MonsterAOE_NB_Crit.Value = Current.Crit;
            MonsterAOE_NB_WT.Value = Current.WT;
            MonsterAOE_NB_MaxRange.Value = Current.MaxRange;
            MonsterAOE_NB_MinRange.Value = Current.MinRange;
            MonsterAOE_NB_Durability.Value = Current.Durability;
            MonsterAOE_NB_HPMod.Value = Current.HPMod;
            MonsterAOE_NB_Model.Value = Current.WeaponModel;

            //Unk Values
            MonsterAOE_NB_unk_0x0.Value = Current.unk_0x0;
            MonsterAOE_NB_unk_0x1.Value = Current.unk_0x1;
            MonsterAOE_NB_unk_0x3.Value = Current.unk_0x3;
            MonsterAOE_NB_unk_0x16.Value = Current.unk_0x16;
            MonsterAOE_NB_unk_0x18.Value = Current.unk_0x18;
            MonsterAOE_NB_ExtraEffect.Value = Current.ExtraEffect;

            //Flags
            MonsterAOE_Check_Flag1.Checked = BitFlags.GetFlag(Current.flags01, 0);
            MonsterAOE_Check_Flag2.Checked = BitFlags.GetFlag(Current.flags01, 1);
            MonsterAOE_Check_Flag3.Checked = BitFlags.GetFlag(Current.flags01, 2);
            MonsterAOE_Check_Flag4.Checked = BitFlags.GetFlag(Current.flags01, 3);
            MonsterAOE_Check_Flag5.Checked = BitFlags.GetFlag(Current.flags01, 4);
            MonsterAOE_Check_Flag6.Checked = BitFlags.GetFlag(Current.flags01, 5);
            MonsterAOE_Check_Flag7.Checked = BitFlags.GetFlag(Current.flags01, 6);
            MonsterAOE_Check_Flag8.Checked = BitFlags.GetFlag(Current.flags01, 7);

            MonsterAOE_Check_Flag9.Checked = BitFlags.GetFlag(Current.flags02, 0);
            MonsterAOE_Check_Flag10.Checked = BitFlags.GetFlag(Current.flags02, 1);
            MonsterAOE_Check_Flag11.Checked = BitFlags.GetFlag(Current.flags02, 2);
            MonsterAOE_Check_Flag12.Checked = BitFlags.GetFlag(Current.flags02, 3);
            MonsterAOE_Check_Flag13.Checked = BitFlags.GetFlag(Current.flags02, 4);
            MonsterAOE_Check_Flag14.Checked = BitFlags.GetFlag(Current.flags02, 5);
            MonsterAOE_Check_Flag15.Checked = BitFlags.GetFlag(Current.flags02, 6);
            MonsterAOE_Check_Flag16.Checked = BitFlags.GetFlag(Current.flags02, 7);

            MonsterAOE_Check_Flag17.Checked = BitFlags.GetFlag(Current.flags03, 0);
            MonsterAOE_Check_Flag18.Checked = BitFlags.GetFlag(Current.flags03, 1);
            MonsterAOE_Check_Flag19.Checked = BitFlags.GetFlag(Current.flags03, 2);
            MonsterAOE_Check_Flag20.Checked = BitFlags.GetFlag(Current.flags03, 3);
            MonsterAOE_Check_Flag21.Checked = BitFlags.GetFlag(Current.flags03, 4);
            MonsterAOE_Check_Flag22.Checked = BitFlags.GetFlag(Current.flags03, 5);
            MonsterAOE_Check_Flag23.Checked = BitFlags.GetFlag(Current.flags03, 6);
            MonsterAOE_Check_Flag24.Checked = BitFlags.GetFlag(Current.flags03, 7);

            //Effectivness
            MonsterAOE_Check_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            MonsterAOE_Check_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            MonsterAOE_Check_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            MonsterAOE_Check_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            MonsterAOE_Check_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            MonsterAOE_Check_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            MonsterAOE_Check_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            MonsterAOE_Check_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void MonsterAOE_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            if (currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crest <= 21)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crest.ToString()));
            }
            else if (currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crest > 21 && currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crest <= 43)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex - 22].Crest.ToString()));
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            }
            Bitmap crest = new Bitmap(myStream);
            MonsterAOE_Crest_PictureBox.Image = crest;

            //Load Icon Images
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            string cresttype = currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crest.ToString();
            int itemtype = currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].ItemType;
            if (itemtype == -1 || itemtype <= 3)
            {

                if (BitFlags.GetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 6))
                {
                    myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}00.png", OtherNames[MonsterAOE_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crest <= 43)
                {
                    if (BitFlags.GetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 7))
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[MonsterAOE_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[MonsterAOE_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                    else
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[MonsterAOE_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[MonsterAOE_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                }
                else if (cresttype == "44")
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[MonsterAOE_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[MonsterAOE_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (BitFlags.GetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 7))
                {
                    myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.Stone01.png");
                }
                else
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[MonsterAOE_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[MonsterAOE_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.unkItem.png");
            }
            Bitmap weapon = new Bitmap(myStream);
            MonsterAOE_Icon_Picturebox.Image = weapon;
        }

        #endregion

        #region MonsterAOE Tab Write
        private void MonsterAOE_NB_MT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].MT = Decimal.ToByte(MonsterAOE_NB_MT.Value);
        }

        private void MonsterAOE_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Hit = Decimal.ToByte(MonsterAOE_NB_Hit.Value);
        }

        private void MonsterAOE_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crit = Decimal.ToSByte(MonsterAOE_NB_Crit.Value);
        }

        private void MonsterAOE_NB_WT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].WT = Decimal.ToByte(MonsterAOE_NB_WT.Value);
        }

        private void MonsterAOE_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].MaxRange = Decimal.ToByte(MonsterAOE_NB_MaxRange.Value);
        }

        private void MonsterAOE_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].MinRange = Decimal.ToByte(MonsterAOE_NB_MinRange.Value);
        }

        private void MonsterAOE_NB_Durability_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Durability = Decimal.ToSByte(MonsterAOE_NB_Durability.Value);
        }

        private void MonsterAOE_NB_HPMod_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].HPMod = Decimal.ToByte(MonsterAOE_NB_HPMod.Value);
        }

        private void MonsterAOE_NB_Model_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].WeaponModel = Decimal.ToByte(MonsterAOE_NB_Model.Value);
        }

        private void MonsterAOE_NB_unk0x0_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].unk_0x0 = Decimal.ToByte(MonsterAOE_NB_unk_0x0.Value);
        }

        private void MonsterAOE_NB_unk0x1_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].unk_0x1 = Decimal.ToByte(MonsterAOE_NB_unk_0x1.Value);
        }

        private void MonsterAOE_NB_0x3_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].unk_0x3 = Decimal.ToByte(MonsterAOE_NB_unk_0x3.Value);
        }

        private void MonsterAOE_NB_0x10_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].unk_0x16 = Decimal.ToByte(MonsterAOE_NB_unk_0x16.Value);
        }

        private void MonsterAOE_NB_0x13_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].unk_0x18 = Decimal.ToByte(MonsterAOE_NB_unk_0x18.Value);
        }

        private void MonsterAOE_NB_ExtraEffect_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].ExtraEffect = Decimal.ToByte(MonsterAOE_NB_ExtraEffect.Value);
        }

        private void MonsterAOE_CB_MagicEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].MagicEffect = Decimal.ToByte(MonsterAOE_CB_MagicEffect.SelectedIndex);
        }

        private void MonsterAOE_CB_WeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].WeaponType = Decimal.ToByte(MonsterAOE_CB_WeaponType.SelectedIndex);
            MonsterAOE_ReloadImage();
        }

        private void MonsterAOE_CB_Crest_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Crest = Decimal.ToByte(MonsterAOE_CB_Crest.SelectedIndex);
            MonsterAOE_ReloadImage();
        }

        private void MonsterAOE_CB_WeaponRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].WeaponRank = Decimal.ToByte(MonsterAOE_CB_WeaponRank.SelectedIndex);
        }

        private void MonsterAOE_CB_ItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_CB_ItemType.SelectedIndex == 12)
            {
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].ItemType = Decimal.ToSByte(-1);
            }
            else
            {
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].ItemType = Decimal.ToSByte(MonsterAOE_CB_ItemType.SelectedIndex);
            }
            MonsterAOE_ReloadImage();
        }

        private void MonsterAOE_Check_Flag1_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag1.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 0, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 0, false);
        }

        private void MonsterAOE_Check_Flag2_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag2.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 1, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 1, false);
        }

        private void MonsterAOE_Check_Flag3_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag3.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 2, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 2, false);
        }

        private void MonsterAOE_Check_Flag4_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag4.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 3, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 3, false);
        }

        private void MonsterAOE_Check_Flag5_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag5.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 4, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 4, false);
        }

        private void MonsterAOE_Check_Flag6_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag6.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 5, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 5, false);
        }

        private void MonsterAOE_Check_Flag7_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag7.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 6, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 6, false);
            MonsterAOE_ReloadImage();
        }

        private void MonsterAOE_Check_Flag8_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag8.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 7, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags01, 7, false);
            MonsterAOE_ReloadImage();
        }

        private void MonsterAOE_Check_Flag9_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag9.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 0, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 0, false);
        }

        private void MonsterAOE_Check_Flag10_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag10.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 1, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 1, false);
        }

        private void MonsterAOE_Check_Flag11_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag11.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 2, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 2, false);
        }

        private void MonsterAOE_Check_Flag12_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag12.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 3, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 3, false);
        }

        private void MonsterAOE_Check_Flag13_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag13.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 4, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 4, false);
        }

        private void MonsterAOE_Check_Flag14_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag14.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 5, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 5, false);
        }

        private void MonsterAOE_Check_Flag15_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag15.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 6, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 6, false);
        }

        private void MonsterAOE_Check_Flag16_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag16.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 7, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags02, 7, false);
        }

        private void MonsterAOE_Check_Flag17_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag17.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 0, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 0, false);
        }

        private void MonsterAOE_Check_Flag18_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag18.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 1, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 1, false);
        }

        private void MonsterAOE_Check_Flag19_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag19.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 2, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 2, false);
        }

        private void MonsterAOE_Check_Flag20_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag20.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 3, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 3, false);
        }

        private void MonsterAOE_Check_Flag21_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag21.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 4, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 4, false);
        }

        private void MonsterAOE_Check_Flag22_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag22.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 5, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 5, false);
        }

        private void MonsterAOE_Check_Flag23_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag23.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 6, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 6, false);
        }

        private void MonsterAOE_Check_Flag24_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_Flag24.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 7, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].flags03, 7, false);
            MonsterAOE_ReloadImage();
        }

        private void MonsterAOE_Check_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_EInfantry.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 0, false);
        }

        private void MonsterAOE_Check_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_EArmor.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 1, false);
        }
        private void MonsterAOE_Check_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_ECavalry.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 2, false);
        }
        private void MonsterAOE_Check_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_EFiler.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 3, false);
        }
        private void MonsterAOE_Check_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_EDragon.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 4, false);
        }
        private void MonsterAOE_Check_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_EMonster.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 5, false);
        }
        private void MonsterAOE_Check_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_EReserve1.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 6, false);
        }
        private void MonsterAOE_Check_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterAOE_Check_EReserve2.Checked)
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.MonsterAOE[MonsterAOE_LB_MonsterAOEList.SelectedIndex].Effectiveness, 7, false);
        }
        #endregion

        #region Equipment Tab Load
        private void Equipment_LB_EquipmentList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Equipment_DisplayCurrent();
            Equipment_ReloadImage();
        }

        private void Equipment_DisplayCurrent()
        {
            var Current = currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex];
            //Weapon Data
            Equipment_TB_Name.Text = msgDataNames[Equipment_LB_EquipmentList.SelectedIndex + 4556];
            Equipment_CB_WeaponType.SelectedIndex = Current.WeaponType;
            Equipment_CB_Crest.SelectedIndex = Current.Crest;
            Equipment_CB_WeaponRank.SelectedIndex = Current.WeaponRank;
            if (Current.ItemType == -1)
            {
                Equipment_CB_ItemType.SelectedIndex = 12;
            }
            else
            {
                Equipment_CB_ItemType.SelectedIndex = Current.ItemType;
            }
            Equipment_CB_MagicEffect.SelectedIndex = Current.MagicEffect;

            //weapon Stats
            Equipment_NB_MT.Value = Current.MT;
            Equipment_NB_Hit.Value = Current.Hit;
            Equipment_NB_Crit.Value = Current.Crit;
            Equipment_NB_WT.Value = Current.WT;
            Equipment_NB_MaxRange.Value = Current.MaxRange;
            Equipment_NB_MinRange.Value = Current.MinRange;
            Equipment_NB_Durability.Value = Current.Durability;
            Equipment_NB_HPMod.Value = Current.HPMod;
            Equipment_NB_Model.Value = Current.WeaponModel;

            //Unk Values
            Equipment_NB_unk_0x0.Value = Current.unk_0x0;
            Equipment_NB_unk_0x1.Value = Current.unk_0x1;
            Equipment_NB_unk_0x3.Value = Current.unk_0x3;
            Equipment_NB_unk_0x16.Value = Current.unk_0x16;
            Equipment_NB_unk_0x18.Value = Current.unk_0x18;
            Equipment_NB_ExtraEffect.Value = Current.ExtraEffect;

            //Flags
            Equipment_Check_Flag1.Checked = BitFlags.GetFlag(Current.flags01, 0);
            Equipment_Check_Flag2.Checked = BitFlags.GetFlag(Current.flags01, 1);
            Equipment_Check_Flag3.Checked = BitFlags.GetFlag(Current.flags01, 2);
            Equipment_Check_Flag4.Checked = BitFlags.GetFlag(Current.flags01, 3);
            Equipment_Check_Flag5.Checked = BitFlags.GetFlag(Current.flags01, 4);
            Equipment_Check_Flag6.Checked = BitFlags.GetFlag(Current.flags01, 5);
            Equipment_Check_Flag7.Checked = BitFlags.GetFlag(Current.flags01, 6);
            Equipment_Check_Flag8.Checked = BitFlags.GetFlag(Current.flags01, 7);

            Equipment_Check_Flag9.Checked = BitFlags.GetFlag(Current.flags02, 0);
            Equipment_Check_Flag10.Checked = BitFlags.GetFlag(Current.flags02, 1);
            Equipment_Check_Flag11.Checked = BitFlags.GetFlag(Current.flags02, 2);
            Equipment_Check_Flag12.Checked = BitFlags.GetFlag(Current.flags02, 3);
            Equipment_Check_Flag13.Checked = BitFlags.GetFlag(Current.flags02, 4);
            Equipment_Check_Flag14.Checked = BitFlags.GetFlag(Current.flags02, 5);
            Equipment_Check_Flag15.Checked = BitFlags.GetFlag(Current.flags02, 6);
            Equipment_Check_Flag16.Checked = BitFlags.GetFlag(Current.flags02, 7);

            Equipment_Check_Flag17.Checked = BitFlags.GetFlag(Current.flags03, 0);
            Equipment_Check_Flag18.Checked = BitFlags.GetFlag(Current.flags03, 1);
            Equipment_Check_Flag19.Checked = BitFlags.GetFlag(Current.flags03, 2);
            Equipment_Check_Flag20.Checked = BitFlags.GetFlag(Current.flags03, 3);
            Equipment_Check_Flag21.Checked = BitFlags.GetFlag(Current.flags03, 4);
            Equipment_Check_Flag22.Checked = BitFlags.GetFlag(Current.flags03, 5);
            Equipment_Check_Flag23.Checked = BitFlags.GetFlag(Current.flags03, 6);
            Equipment_Check_Flag24.Checked = BitFlags.GetFlag(Current.flags03, 7);

            //Effectivness
            Equipment_Check_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            Equipment_Check_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            Equipment_Check_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            Equipment_Check_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            Equipment_Check_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            Equipment_Check_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            Equipment_Check_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            Equipment_Check_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void Equipment_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            if (currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crest <= 21)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crest.ToString()));
            }
            else if (currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crest > 21 && currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crest <= 43)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex - 22].Crest.ToString()));
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            }
            Bitmap crest = new Bitmap(myStream);
            Equipment_Crest_PictureBox.Image = crest;

            //Load Icon Images
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            string cresttype = currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crest.ToString();
            int itemtype = currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].ItemType;
            if (itemtype == -1 || itemtype <= 3)
            {

                if (BitFlags.GetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 6))
                {
                    myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}00.png", OtherNames[Equipment_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crest <= 43)
                {
                    if (BitFlags.GetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 7))
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Equipment_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Equipment_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                    else
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Equipment_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Equipment_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                }
                else if (cresttype == "44")
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Equipment_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Equipment_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (BitFlags.GetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 7))
                {
                    myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.Stone01.png");
                }
                else
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Equipment_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Equipment_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.unkItem.png");
            }
            Bitmap weapon = new Bitmap(myStream);
            Equipment_Icon_Picturebox.Image = weapon;
        }

        #endregion

        #region Equipment Tab Write
        private void Equipment_NB_MT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].MT = Decimal.ToByte(Equipment_NB_MT.Value);
        }

        private void Equipment_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Hit = Decimal.ToByte(Equipment_NB_Hit.Value);
        }

        private void Equipment_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crit = Decimal.ToSByte(Equipment_NB_Crit.Value);
        }

        private void Equipment_NB_WT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].WT = Decimal.ToByte(Equipment_NB_WT.Value);
        }

        private void Equipment_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].MaxRange = Decimal.ToByte(Equipment_NB_MaxRange.Value);
        }

        private void Equipment_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].MinRange = Decimal.ToByte(Equipment_NB_MinRange.Value);
        }

        private void Equipment_NB_Durability_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Durability = Decimal.ToSByte(Equipment_NB_Durability.Value);
        }

        private void Equipment_NB_HPMod_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].HPMod = Decimal.ToByte(Equipment_NB_HPMod.Value);
        }

        private void Equipment_NB_Model_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].WeaponModel = Decimal.ToByte(Equipment_NB_Model.Value);
        }

        private void Equipment_NB_unk0x0_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].unk_0x0 = Decimal.ToByte(Equipment_NB_unk_0x0.Value);
        }

        private void Equipment_NB_unk0x1_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].unk_0x1 = Decimal.ToByte(Equipment_NB_unk_0x1.Value);
        }

        private void Equipment_NB_0x3_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].unk_0x3 = Decimal.ToByte(Equipment_NB_unk_0x3.Value);
        }

        private void Equipment_NB_0x10_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].unk_0x16 = Decimal.ToByte(Equipment_NB_unk_0x16.Value);
        }

        private void Equipment_NB_0x13_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].unk_0x18 = Decimal.ToByte(Equipment_NB_unk_0x18.Value);
        }

        private void Equipment_NB_ExtraEffect_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].ExtraEffect = Decimal.ToByte(Equipment_NB_ExtraEffect.Value);
        }

        private void Equipment_CB_MagicEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].MagicEffect = Decimal.ToByte(Equipment_CB_MagicEffect.SelectedIndex);
        }

        private void Equipment_CB_WeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].WeaponType = Decimal.ToByte(Equipment_CB_WeaponType.SelectedIndex);
            Equipment_ReloadImage();
        }

        private void Equipment_CB_Crest_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Crest = Decimal.ToByte(Equipment_CB_Crest.SelectedIndex);
            Equipment_ReloadImage();
        }

        private void Equipment_CB_WeaponRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].WeaponRank = Decimal.ToByte(Equipment_CB_WeaponRank.SelectedIndex);
        }

        private void Equipment_CB_ItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Equipment_CB_ItemType.SelectedIndex == 12)
            {
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].ItemType = Decimal.ToSByte(-1);
            }
            else
            {
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].ItemType = Decimal.ToSByte(Equipment_CB_ItemType.SelectedIndex);
            }
            Equipment_ReloadImage();
        }

        private void Equipment_Check_Flag1_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag1.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 0, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 0, false);
        }

        private void Equipment_Check_Flag2_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag2.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 1, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 1, false);
        }

        private void Equipment_Check_Flag3_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag3.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 2, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 2, false);
        }

        private void Equipment_Check_Flag4_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag4.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 3, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 3, false);
        }

        private void Equipment_Check_Flag5_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag5.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 4, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 4, false);
        }

        private void Equipment_Check_Flag6_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag6.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 5, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 5, false);
        }

        private void Equipment_Check_Flag7_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag7.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 6, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 6, false);
            Equipment_ReloadImage();
        }

        private void Equipment_Check_Flag8_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag8.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 7, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags01, 7, false);
            Equipment_ReloadImage();
        }

        private void Equipment_Check_Flag9_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag9.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 0, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 0, false);
        }

        private void Equipment_Check_Flag10_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag10.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 1, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 1, false);
        }

        private void Equipment_Check_Flag11_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag11.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 2, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 2, false);
        }

        private void Equipment_Check_Flag12_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag12.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 3, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 3, false);
        }

        private void Equipment_Check_Flag13_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag13.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 4, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 4, false);
        }

        private void Equipment_Check_Flag14_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag14.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 5, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 5, false);
        }

        private void Equipment_Check_Flag15_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag15.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 6, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 6, false);
        }

        private void Equipment_Check_Flag16_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag16.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 7, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags02, 7, false);
        }

        private void Equipment_Check_Flag17_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag17.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 0, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 0, false);
        }

        private void Equipment_Check_Flag18_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag18.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 1, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 1, false);
        }

        private void Equipment_Check_Flag19_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag19.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 2, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 2, false);
        }

        private void Equipment_Check_Flag20_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag20.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 3, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 3, false);
        }

        private void Equipment_Check_Flag21_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag21.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 4, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 4, false);
        }

        private void Equipment_Check_Flag22_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag22.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 5, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 5, false);
        }

        private void Equipment_Check_Flag23_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag23.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 6, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 6, false);
        }

        private void Equipment_Check_Flag24_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_Flag24.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 7, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].flags03, 7, false);
            Equipment_ReloadImage();
        }

        private void Equipment_Check_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_EInfantry.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 0, false);
        }

        private void Equipment_Check_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_EArmor.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 1, false);
        }
        private void Equipment_Check_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_ECavalry.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 2, false);
        }
        private void Equipment_Check_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_EFiler.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 3, false);
        }
        private void Equipment_Check_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_EDragon.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 4, false);
        }
        private void Equipment_Check_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_EMonster.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 5, false);
        }
        private void Equipment_Check_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_EReserve1.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 6, false);
        }
        private void Equipment_Check_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (Equipment_Check_EReserve2.Checked)
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Equipment[Equipment_LB_EquipmentList.SelectedIndex].Effectiveness, 7, false);
        }
        #endregion

        #region Items Tab Load
        private void Items_LB_ItemsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Items_DisplayCurrent();
            Items_ReloadImage();
        }

        private void Items_DisplayCurrent()
        {
            var Current = currentDatafile.Items[Items_LB_ItemsList.SelectedIndex];
            //Weapon Data
            Items_TB_Name.Text = msgDataNames[Items_LB_ItemsList.SelectedIndex + 4656];
            Items_CB_WeaponType.SelectedIndex = Current.WeaponType;
            Items_CB_Crest.SelectedIndex = Current.Crest;
            Items_CB_WeaponRank.SelectedIndex = Current.WeaponRank;
            if (Current.ItemType == -1)
            {
                Items_CB_ItemType.SelectedIndex = 12;
            }
            else
            {
                Items_CB_ItemType.SelectedIndex = Current.ItemType;
            }
            Items_CB_MagicEffect.SelectedIndex = Current.MagicEffect;

            //weapon Stats
            Items_NB_MT.Value = Current.MT;
            Items_NB_Hit.Value = Current.Hit;
            Items_NB_Crit.Value = Current.Crit;
            Items_NB_WT.Value = Current.WT;
            Items_NB_MaxRange.Value = Current.MaxRange;
            Items_NB_MinRange.Value = Current.MinRange;
            Items_NB_Durability.Value = Current.Durability;
            Items_NB_HPMod.Value = Current.HPMod;
            Items_NB_Model.Value = Current.WeaponModel;

            //Unk Values
            Items_NB_unk_0x0.Value = Current.unk_0x0;
            Items_NB_unk_0x1.Value = Current.unk_0x1;
            Items_NB_unk_0x3.Value = Current.unk_0x3;
            Items_NB_unk_0x16.Value = Current.unk_0x16;
            Items_NB_unk_0x18.Value = Current.unk_0x18;
            Items_NB_ExtraEffect.Value = Current.ExtraEffect;

            //Flags
            Items_Check_Flag1.Checked = BitFlags.GetFlag(Current.flags01, 0);
            Items_Check_Flag2.Checked = BitFlags.GetFlag(Current.flags01, 1);
            Items_Check_Flag3.Checked = BitFlags.GetFlag(Current.flags01, 2);
            Items_Check_Flag4.Checked = BitFlags.GetFlag(Current.flags01, 3);
            Items_Check_Flag5.Checked = BitFlags.GetFlag(Current.flags01, 4);
            Items_Check_Flag6.Checked = BitFlags.GetFlag(Current.flags01, 5);
            Items_Check_Flag7.Checked = BitFlags.GetFlag(Current.flags01, 6);
            Items_Check_Flag8.Checked = BitFlags.GetFlag(Current.flags01, 7);

            Items_Check_Flag9.Checked = BitFlags.GetFlag(Current.flags02, 0);
            Items_Check_Flag10.Checked = BitFlags.GetFlag(Current.flags02, 1);
            Items_Check_Flag11.Checked = BitFlags.GetFlag(Current.flags02, 2);
            Items_Check_Flag12.Checked = BitFlags.GetFlag(Current.flags02, 3);
            Items_Check_Flag13.Checked = BitFlags.GetFlag(Current.flags02, 4);
            Items_Check_Flag14.Checked = BitFlags.GetFlag(Current.flags02, 5);
            Items_Check_Flag15.Checked = BitFlags.GetFlag(Current.flags02, 6);
            Items_Check_Flag16.Checked = BitFlags.GetFlag(Current.flags02, 7);

            Items_Check_Flag17.Checked = BitFlags.GetFlag(Current.flags03, 0);
            Items_Check_Flag18.Checked = BitFlags.GetFlag(Current.flags03, 1);
            Items_Check_Flag19.Checked = BitFlags.GetFlag(Current.flags03, 2);
            Items_Check_Flag20.Checked = BitFlags.GetFlag(Current.flags03, 3);
            Items_Check_Flag21.Checked = BitFlags.GetFlag(Current.flags03, 4);
            Items_Check_Flag22.Checked = BitFlags.GetFlag(Current.flags03, 5);
            Items_Check_Flag23.Checked = BitFlags.GetFlag(Current.flags03, 6);
            Items_Check_Flag24.Checked = BitFlags.GetFlag(Current.flags03, 7);

            //Effectivness
            Items_Check_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            Items_Check_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            Items_Check_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            Items_Check_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            Items_Check_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            Items_Check_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            Items_Check_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            Items_Check_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void Items_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            if (currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crest <= 21)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crest.ToString()));
            }
            else if (currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crest > 21 && currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crest <= 43)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Crests.Crest_{0}.png", currentDatafile.Items[Items_LB_ItemsList.SelectedIndex - 22].Crest.ToString()));
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            }
            Bitmap crest = new Bitmap(myStream);
            Items_Crest_PictureBox.Image = crest;

            //Load Icon Images
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            string cresttype = currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crest.ToString();
            int itemtype = currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].ItemType;
            if (itemtype == -1 || itemtype <= 3)
            {

                if (BitFlags.GetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 6))
                {
                    myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}00.png", OtherNames[Items_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crest <= 43)
                {
                    if (BitFlags.GetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 7))
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Items_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}03.png", OtherNames[Items_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                    else
                    {
                        if (itemtype >= 0)
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Items_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                        else
                            myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}02.png", OtherNames[Items_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                    }
                }
                else if (cresttype == "44")
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Items_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}04.png", OtherNames[Items_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
                else if (BitFlags.GetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 7))
                {
                    myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.Stone01.png");
                }
                else
                {
                    if (itemtype >= 0)
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Items_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}01.png", OtherNames[Items_CB_WeaponType.SelectedIndex]).Replace(' ', '_'));
                }
            }
            else
            {
                if (itemtype >= 4)
                {
                    if (itemtype == 11)
                    {
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.Crest_21.png", OtherNames[Items_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                    }
                    else
                        myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Items.{0}.png", OtherNames[Items_CB_ItemType.SelectedIndex + 13]).Replace(' ', '_'));
                }
            }
            if (myStream == null)
            {
                myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Items.unkItem.png");
            }
            Bitmap weapon = new Bitmap(myStream);
            Items_Icon_Picturebox.Image = weapon;
        }

        #endregion

        #region Items Tab Write
        private void Items_NB_MT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].MT = Decimal.ToByte(Items_NB_MT.Value);
        }

        private void Items_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Hit = Decimal.ToByte(Items_NB_Hit.Value);
        }

        private void Items_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crit = Decimal.ToSByte(Items_NB_Crit.Value);
        }

        private void Items_NB_WT_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].WT = Decimal.ToByte(Items_NB_WT.Value);
        }

        private void Items_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].MaxRange = Decimal.ToByte(Items_NB_MaxRange.Value);
        }

        private void Items_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].MinRange = Decimal.ToByte(Items_NB_MinRange.Value);
        }

        private void Items_NB_Durability_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Durability = Decimal.ToSByte(Items_NB_Durability.Value);
        }

        private void Items_NB_HPMod_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].HPMod = Decimal.ToByte(Items_NB_HPMod.Value);
        }

        private void Items_NB_Model_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].WeaponModel = Decimal.ToByte(Items_NB_Model.Value);
        }

        private void Items_NB_unk0x0_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].unk_0x0 = Decimal.ToByte(Items_NB_unk_0x0.Value);
        }

        private void Items_NB_unk0x1_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].unk_0x1 = Decimal.ToByte(Items_NB_unk_0x1.Value);
        }

        private void Items_NB_0x3_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].unk_0x3 = Decimal.ToByte(Items_NB_unk_0x3.Value);
        }

        private void Items_NB_0x10_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].unk_0x16 = Decimal.ToByte(Items_NB_unk_0x16.Value);
        }

        private void Items_NB_0x13_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].unk_0x18 = Decimal.ToByte(Items_NB_unk_0x18.Value);
        }

        private void Items_NB_ExtraEffect_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].ExtraEffect = Decimal.ToByte(Items_NB_ExtraEffect.Value);
        }

        private void Items_CB_MagicEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].MagicEffect = Decimal.ToByte(Items_CB_MagicEffect.SelectedIndex);
        }

        private void Items_CB_WeaponType_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].WeaponType = Decimal.ToByte(Items_CB_WeaponType.SelectedIndex);
            Items_ReloadImage();
        }

        private void Items_CB_Crest_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Crest = Decimal.ToByte(Items_CB_Crest.SelectedIndex);
            Items_ReloadImage();
        }

        private void Items_CB_WeaponRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].WeaponRank = Decimal.ToByte(Items_CB_WeaponRank.SelectedIndex);
        }

        private void Items_CB_ItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Items_CB_ItemType.SelectedIndex == 12)
            {
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].ItemType = Decimal.ToSByte(-1);
            }
            else
            {
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].ItemType = Decimal.ToSByte(Items_CB_ItemType.SelectedIndex);
            }
            Items_ReloadImage();
        }

        private void Items_Check_Flag1_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag1.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 0, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 0, false);
        }

        private void Items_Check_Flag2_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag2.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 1, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 1, false);
        }

        private void Items_Check_Flag3_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag3.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 2, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 2, false);
        }

        private void Items_Check_Flag4_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag4.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 3, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 3, false);
        }

        private void Items_Check_Flag5_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag5.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 4, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 4, false);
        }

        private void Items_Check_Flag6_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag6.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 5, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 5, false);
        }

        private void Items_Check_Flag7_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag7.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 6, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 6, false);
            Items_ReloadImage();
        }

        private void Items_Check_Flag8_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag8.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 7, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags01, 7, false);
            Items_ReloadImage();
        }

        private void Items_Check_Flag9_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag9.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 0, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 0, false);
        }

        private void Items_Check_Flag10_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag10.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 1, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 1, false);
        }

        private void Items_Check_Flag11_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag11.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 2, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 2, false);
        }

        private void Items_Check_Flag12_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag12.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 3, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 3, false);
        }

        private void Items_Check_Flag13_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag13.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 4, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 4, false);
        }

        private void Items_Check_Flag14_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag14.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 5, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 5, false);
        }

        private void Items_Check_Flag15_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag15.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 6, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 6, false);
        }

        private void Items_Check_Flag16_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag16.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 7, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags02, 7, false);
        }

        private void Items_Check_Flag17_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag17.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 0, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 0, false);
        }

        private void Items_Check_Flag18_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag18.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 1, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 1, false);
        }

        private void Items_Check_Flag19_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag19.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 2, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 2, false);
        }

        private void Items_Check_Flag20_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag20.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 3, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 3, false);
        }

        private void Items_Check_Flag21_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag21.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 4, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 4, false);
        }

        private void Items_Check_Flag22_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag22.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 5, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 5, false);
        }

        private void Items_Check_Flag23_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag23.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 6, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 6, false);
        }

        private void Items_Check_Flag24_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_Flag24.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 7, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03 = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].flags03, 7, false);
            Items_ReloadImage();
        }

        private void Items_Check_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_EInfantry.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 0, false);
        }

        private void Items_Check_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_EArmor.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 1, false);
        }
        private void Items_Check_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_ECavalry.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 2, false);
        }
        private void Items_Check_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_EFiler.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 3, false);
        }
        private void Items_Check_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_EDragon.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 4, false);
        }
        private void Items_Check_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_EMonster.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 5, false);
        }
        private void Items_Check_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_EReserve1.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 6, false);
        }
        private void Items_Check_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (Items_Check_EReserve2.Checked)
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.Items[Items_LB_ItemsList.SelectedIndex].Effectiveness, 7, false);
        }
        #endregion

        #region Combat Art Load

        private void CombatArt_LB_CombatArtList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine(String.Format("Selected Index for Combat Art: {0}",CombatArt_LB_CombatArtList.SelectedIndex));
            CombatArts_DisplayCurrent();
            CombatArt_ReloadImage();
        }
        private void CombatArts_DisplayCurrent()
        {
            var Current = currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex];
            //Weapon Data
            CombatArt_TB_Name.Text = msgDataNames[CombatArt_LB_CombatArtList.SelectedIndex + 6014];
            Console.WriteLine(msgDataNames[CombatArt_LB_CombatArtList.SelectedIndex + 6014]);
            if (Current.RequiredWeapon == -1)
            {
                CombatArt_CB_RequireWeapon.SelectedIndex = 500;
            }
            else
            {
                CombatArt_CB_RequireWeapon.SelectedIndex = Current.RequiredWeapon;
            }
            if (Current.RequiredClass == 255)
            {
                CombatArt_CB_RequiredClass.SelectedIndex = 100;
            }
            else
            {
                CombatArt_CB_RequiredClass.SelectedIndex = Current.RequiredClass;
            }
            CombatArt_EB_Effect.SelectedIndex = Current.Effect;
            

            //weapon Stats
            CombatArt_NB_Avoid.Value = Current.Avoid;
            CombatArt_NB_Might.Value = Current.Might;
            CombatArt_NB_Crit.Value = Current.Crit;
            CombatArt_NB_Hit.Value = Current.Hit;
            CombatArt_NB_MaxRange.Value = Current.MaxRange;
            CombatArt_NB_MinRange.Value = Current.MinRange;
            CombatArt_NB_Cost.Value = Current.DurabilityCost;

            //Unk Values
            CombatArt_NB_unk0xA.Value = Current.unk_0xA;
            CombatArt_NB_unk0xF.Value = Current.unk_0xF;
            CombatArt_NB_unk0x11.Value = Current.unk_0x11;

            //Weapons
            CombatArt_NB_Sword.Checked = BitFlags.GetFlag(Current.WeapType, 0);
            CombatArt_NB_Lance.Checked = BitFlags.GetFlag(Current.WeapType, 1);
            CombatArt_NB_Axe.Checked = BitFlags.GetFlag(Current.WeapType, 2);
            CombatArt_NB_Bow.Checked = BitFlags.GetFlag(Current.WeapType, 3);
            CombatArt_NB_Fist.Checked = BitFlags.GetFlag(Current.WeapType, 4);
            CombatArt_NB_Tome.Checked = BitFlags.GetFlag(Current.WeapType, 5);
            checkBox2.Checked = BitFlags.GetFlag(Current.WeapType, 6);
            checkBox1.Checked = BitFlags.GetFlag(Current.WeapType, 7);

            //Flags
            CombatArt_NB_Flag1.Checked = BitFlags.GetFlag(Current.Flags, 0);
            CombatArt_NB_Flag2.Checked = BitFlags.GetFlag(Current.Flags, 1);
            CombatArt_NB_Flag3.Checked = BitFlags.GetFlag(Current.Flags, 2);
            CombatArt_NB_Flag4.Checked = BitFlags.GetFlag(Current.Flags, 3);
            CombatArt_NB_Flag5.Checked = BitFlags.GetFlag(Current.Flags, 4);
            CombatArt_NB_Flag6.Checked = BitFlags.GetFlag(Current.Flags, 5);
            CombatArt_NB_Flag7.Checked = BitFlags.GetFlag(Current.Flags, 6);
            CombatArt_NB_Flag8.Checked = BitFlags.GetFlag(Current.Flags, 7);

            //Effectivness
            CombatArt_NB_EInfantry.Checked = BitFlags.GetFlag(Current.Effectiveness, 0);
            CombatArt_NB_EArmor.Checked = BitFlags.GetFlag(Current.Effectiveness, 1);
            CombatArt_NB_ECavalry.Checked = BitFlags.GetFlag(Current.Effectiveness, 2);
            CombatArt_NB_EFiler.Checked = BitFlags.GetFlag(Current.Effectiveness, 3);
            CombatArt_NB_EDragon.Checked = BitFlags.GetFlag(Current.Effectiveness, 4);
            CombatArt_NB_EMonster.Checked = BitFlags.GetFlag(Current.Effectiveness, 5);
            CombatArt_NB_EReserve1.Checked = BitFlags.GetFlag(Current.Effectiveness, 6);
            CombatArt_NB_EReserve2.Checked = BitFlags.GetFlag(Current.Effectiveness, 7);
        }

        private void CombatArt_ReloadImage()
        {
            //Load Crest Images
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("Progenitor.Images.Misc.None.png");
            
            if (CombatArt_NB_Sword.Checked && !CombatArt_NB_Lance.Checked && !CombatArt_NB_Axe.Checked && !CombatArt_NB_Bow.Checked && !CombatArt_NB_Fist.Checked)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Arts.Sword.png"));
            }
            else if (CombatArt_NB_Lance.Checked && !CombatArt_NB_Sword.Checked && !CombatArt_NB_Axe.Checked && !CombatArt_NB_Bow.Checked && !CombatArt_NB_Fist.Checked)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Arts.Lance.png"));
            }
            else if (CombatArt_NB_Axe.Checked && !CombatArt_NB_Lance.Checked && !CombatArt_NB_Sword.Checked && !CombatArt_NB_Bow.Checked && !CombatArt_NB_Fist.Checked)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Arts.Axe.png"));
            }
            else if (CombatArt_NB_Bow.Checked && !CombatArt_NB_Lance.Checked && !CombatArt_NB_Axe.Checked && !CombatArt_NB_Sword.Checked && !CombatArt_NB_Fist.Checked)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Arts.Bow.png"));
            }
            else if (CombatArt_NB_Fist.Checked && !CombatArt_NB_Lance.Checked && !CombatArt_NB_Axe.Checked && !CombatArt_NB_Bow.Checked && !CombatArt_NB_Sword.Checked)
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Arts.Fist.png"));
            }
            else
            {
                myStream = myAssembly.GetManifestResourceStream(String.Format("Progenitor.Images.Arts.Other.png"));
            }

            Bitmap icon = new Bitmap(myStream);
            CombatArt_IB_Icon.Image = icon;
        }


        #endregion

        #region Combat Art Write

        private void CombatArt_NB_Avoid_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Avoid = Decimal.ToSByte(CombatArt_NB_Avoid.Value);
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Avoid2 = Decimal.ToSByte(CombatArt_NB_Avoid.Value);
        }

        private void CombatArt_NB_Might_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Might = Decimal.ToByte(CombatArt_NB_Might.Value);
        }

        private void CombatArt_NB_Crit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Crit = Decimal.ToSByte(CombatArt_NB_Crit.Value);
        }

        private void CombatArt_NB_Hit_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Hit = Decimal.ToSByte(CombatArt_NB_Hit.Value);
        }

        private void CombatArt_NB_Cost_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].DurabilityCost = Decimal.ToByte(CombatArt_NB_Cost.Value);
        }

        private void CombatArt_NB_MaxRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].MaxRange = Decimal.ToByte(CombatArt_NB_MaxRange.Value);
        }

        private void CombatArt_NB_MinRange_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].MinRange = Decimal.ToByte(CombatArt_NB_MinRange.Value);
        }

        private void CombatArt_NB_unk0xA_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].unk_0xA = Decimal.ToByte(CombatArt_NB_unk0xA.Value);
        }

        private void CombatArt_NB_unk0xF_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].unk_0xF = Decimal.ToByte(CombatArt_NB_unk0xF.Value);
        }

        private void CombatArt_NB_unk0x11_ValueChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].unk_0x11 = Decimal.ToByte(CombatArt_NB_unk0xF.Value);
        }

        private void CombatArt_CB_RequireWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CombatArt_CB_RequireWeapon.SelectedIndex == 500)
            {
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].RequiredWeapon = Decimal.ToInt16(-1);
            }
            else
            {
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].RequiredWeapon = Decimal.ToInt16(CombatArt_CB_RequireWeapon.SelectedIndex);
            }
        }

        private void CombatArt_CB_RequiredClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CombatArt_CB_RequiredClass.SelectedIndex == 100)
            {
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].RequiredClass = Decimal.ToByte(255);
            }
            else
            {
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].RequiredClass = Decimal.ToByte(CombatArt_CB_RequiredClass.SelectedIndex);
            }
        }

        private void CombatArt_EB_Effect_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effect = Decimal.ToByte(CombatArt_EB_Effect.SelectedIndex);
        }

        private void CombatArt_NB_Sword_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Sword.Checked)
            {
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 0, true);
            }
            else
            {
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 0, false);
            }
            CombatArt_ReloadImage();
        }

        private void CombatArt_NB_Lance_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Lance.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 1, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 1, false);
            CombatArt_ReloadImage();
        }

        private void CombatArt_NB_Axe_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Axe.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 2, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 2, false);
            CombatArt_ReloadImage();
        }

        private void CombatArt_NB_Bow_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Bow.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 3, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 3, false);
            CombatArt_ReloadImage();
        }

        private void CombatArt_NB_Fist_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Fist.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 4, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 4, false);
            CombatArt_ReloadImage();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Tome.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 5, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 5, false);
            CombatArt_ReloadImage();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 6, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 6, false);
            CombatArt_ReloadImage();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 7, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].WeapType, 7, false);
            CombatArt_ReloadImage();
        }

        private void CombatArt_NB_Flag1_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag1.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 0, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 0, false);
        }

        private void CombatArt_NB_Flag2_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag2.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 1, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 1, false);
        }

        private void CombatArt_NB_Flag3_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag3.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 2, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 2, false);
        }

        private void CombatArt_NB_Flag4_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag4.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 3, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 3, false);
        }

        private void CombatArt_NB_Flag5_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag5.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 4, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 4, false);
        }

        private void CombatArt_NB_Flag6_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag6.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 5, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 5, false);
        }

        private void CombatArt_NB_Flag7_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag7.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 6, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 6, false);
        }

        private void CombatArt_NB_Flag8_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_Flag8.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 7, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Flags, 7, false);
        }

        private void CombatArt_NB_EInfantry_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_EInfantry.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 0, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 0, false);
        }

        private void CombatArt_NB_EArmor_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_EArmor.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 1, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 1, false);
        }

        private void CombatArt_NB_ECavalry_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_ECavalry.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 2, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 2, false);
        }

        private void CombatArt_NB_EFiler_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_EFiler.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 3, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 3, false);
        }

        private void CombatArt_NB_EDragon_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_EDragon.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 4, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 4, false);
        }

        private void CombatArt_NB_EMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_EMonster.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 5, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 5, false);
        }

        private void CombatArt_NB_EReserve1_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_EReserve1.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 6, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 6, false);
        }

        private void CombatArt_NB_EReserve2_CheckedChanged(object sender, EventArgs e)
        {
            if (CombatArt_NB_EReserve2.Checked)
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 7, true);
            else
                currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness = BitFlags.SetFlag(currentDatafile.CombatArt[CombatArt_LB_CombatArtList.SelectedIndex].Effectiveness, 7, false);
        }

        #endregion
    }
}
