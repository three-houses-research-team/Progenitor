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
using Progenitor.DataFiles;
using Progenitor.DataFiles.Data;

namespace Progenitor
{
    public partial class Progenitor : Form
    {
        public Progenitor()
        {
            InitializeComponent();
        }
        public string filePath { get; set; }
        public string nameOfFile { get; set; }

        private void B_DataEditor_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "fixed_data.bin (*.bin;*.datatable)|*.bin;*.datatable|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "open fixed_data.bin (v1.1.0 or later)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the Path of the specified file
                    filePath = openFileDialog.FileName;
                    nameOfFile = Path.GetFileName(filePath);

                    FixedDataMain dataeditor = new FixedDataMain(nameOfFile, filePath);
                    dataeditor.ShowDialog();
                }
                else return;
            }
        }

        private void B_PersonDataEditor_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "fixed_persondata.bin (*.bin;*.datatable)|*.bin;*.datatable|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "open fixed_persondata.bin (v1.1.0 or later)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the Path of the specified file
                    filePath = openFileDialog.FileName;
                    nameOfFile = Path.GetFileName(filePath);

                    PersonDataEditor personeditor = new PersonDataEditor(nameOfFile, filePath);
                    personeditor.ShowDialog();
                }
                else return;
            }
        }

        private void B_MaterialDataEditor_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "fixed_materialdata.bin (*.bin;*.datatable)|*.bin;*.datatable|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "open fixed_materialdata.bin (v1.2.0 or later)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the Path of the specified file
                    filePath = openFileDialog.FileName;
                    nameOfFile = Path.GetFileName(filePath);

                    MaterialDataEditor materialeditor = new MaterialDataEditor(nameOfFile, filePath);
                    materialeditor.ShowDialog();
                }
                else return;
            }
        }
    }
}
