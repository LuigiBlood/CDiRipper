using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace CDiRipper
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofs = new OpenFileDialog())
            {
                ofs.Multiselect = false;
                ofs.Title = "Load file...";
                ofs.Filter = "All files|*.*";
                if (ofs.ShowDialog() == DialogResult.OK)
                {
                    Program.LoadedFile = new Analysis(ofs.FileName);

                    listBoxIndex.Items.Clear();
                    /*foreach (int offset in Program.LoadedFile.IDATSectors)
                    {
                        listBoxIndex.Items.Add("0x" + offset.ToString("X"));
                    }*/

                    foreach (int offset in Program.LoadedFile.RTStartSectors)
                    {
                        listBoxIndex.Items.Add(offset + " - 0x" + (offset * 0x930).ToString("X"));
                    }

                    this.Text = "CD-i Ripper [" + Path.GetFileName(ofs.FileName) + "]";
                }
            }
        }

        private void listBoxIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            //pictureBoxImage.Image = Image.GetIDAT(Program.LoadedFile.Data, Program.LoadedFile.IDATSectors[listBoxIndex.SelectedIndex]);
            numericUpDown1.Value = 0;
            pictureBoxImage.Image = Program.LoadedFile.GetImage(Program.LoadedFile.RTStartSectors[listBoxIndex.SelectedIndex], (int)numericUpDown1.Value);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //pictureBoxImage.Image = Image.GetIDAT(Program.LoadedFile.Data, Program.LoadedFile.IDATSectors[listBoxIndex.SelectedIndex]);
            pictureBoxImage.Image = Program.LoadedFile.GetImage(Program.LoadedFile.RTStartSectors[listBoxIndex.SelectedIndex], (int)numericUpDown1.Value);
        }
    }
}
