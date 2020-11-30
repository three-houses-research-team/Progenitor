using Progenitor.DataFiles.MaterialData;
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

namespace Progenitor.DataFiles
{
    public partial class MaterialDataEditor : Form
    {
        private MaterialDataFile currentDatafile;
        public MaterialDataEditor(string infile, string inpath)
        {
            InitializeComponent();
            MaterialEditorLoad();
            OpenFile(infile, inpath);
        }
        public int SelectedLanguage { get; set; }
        public List<String> msgDataNames { get; private set; }
        public List<uint> msgDataLanguageSections { get; private set; }
        public string filePath { get; set; }
        public string nameOfFile { get; set; }
        public List<String> OtherNames { get; private set; }

        #region Open and Save
        private void B_Open_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "fixed_materialdata.bin (*.bin)|*.bin|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "open fixed_materialdata.bin (v1.1.0 or later)";

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

        private void OpenFile(string infile, string inpath)
        {
            //Get the path of specified file
            filePath = inpath;
            nameOfFile = infile;

            //Reset Stuff
            Material_List.Items.Clear();

            //Read the contents of the file into a stream
            currentDatafile = new MaterialDataFile();
            currentDatafile.ReadData(filePath);

            //Write Info in Misc Section
            if (currentDatafile.numOfPointers == 2)
            {
                if (tabControl1.TabPages.Contains(tabPage1) == false)
                {
                    tabControl1.Visible = true;
                    tabControl1.TabPages.Add(tabPage1);
                }

                LoadLanguageTomsgDataNames();
                //languageToolStripMenuItem.Visible = true;
            }
            else
            {
                return;
            }
        }

        private void B_Save_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                using (EndianBinaryWriter material_data = new EndianBinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write), Endianness.Little))
                {
                    currentDatafile.WriteData(material_data);
                    MessageBox.Show("File saved to: " + filePath, "File saved");
                }
            }
            else
                MessageBox.Show("No file is open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void B_SaveAs_Click(object sender, EventArgs e)
        {
            if (File.Exists(filePath))
            {
                using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                {
                    saveFileDialog1.Filter = "fixed_materialdata.bin (*.bin)|*.bin|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 1;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var savePath = saveFileDialog1.FileName;
                        using (EndianBinaryWriter material_data = new EndianBinaryWriter(File.Open(savePath, FileMode.Create, FileAccess.Write), Endianness.Little))
                        {
                            filePath = savePath; //now the Save File option writes here too
                            currentDatafile.WriteData(material_data);
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

                for (int i = 0; i < currentDatafile.SectionBlockCount[0]; i++)
                {
                    Material_List.Items.Add(i.ToString("D" + 4) + " : " + msgDataNames[5056 + i]);
                }
            }
        }

        private void MaterialEditorLoad()
        {
            tabControl1.Visible = false;
            tabControl1.TabPages.Remove(tabPage1);

            SelectedLanguage = 1;
            languageToolStripMenuItem.Visible = false;
        }

        private string DecodeUTF8(string instring)
        {
            byte[] bytes = Encoding.Default.GetBytes(instring);
            string utf8string = Encoding.UTF8.GetString(bytes);
            return utf8string;
        }

        #endregion

        private void Material_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            Material_DisplayCurrent();
        }

        private void Material_DisplayCurrent()
        {
            var Current = currentDatafile.Materials[Material_List.SelectedIndex];
            textBox1.Text = msgDataNames[Material_List.SelectedIndex + 5056];

            numericUpDown1.Value = Current.Price;
            numericUpDown2.Value = Current.AltarPrice;
            numericUpDown3.Value = Current.unk_0x6;
            numericUpDown4.Value = Current.unk_0x7;
            numericUpDown5.Value = Current.Stars;
            numericUpDown6.Value = Current.Type;
            numericUpDown7.Value = Current.unk_0xA;
        }
    }
}
