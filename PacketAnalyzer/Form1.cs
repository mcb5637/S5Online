using System;
using System.IO;
using System.Windows.Forms;

namespace PacketAnalyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string data = "";

            Array.Sort(files);
            foreach (string file in files)
                data += ProcessFile(file) + Environment.NewLine;

            tbOutput.Text = data;
        }

        private string ProcessFile(string file)
        {
            string info = file + Environment.NewLine;
            try
            {
                var data = File.ReadAllBytes(file);
                var msgList = S5GameServices.Message.ParseIncoming(data);
                foreach (var msg in msgList)
                    info += msg.ToString() + Environment.NewLine + Environment.NewLine;
            }
            catch (Exception e)
            {
                info += e.ToString();
            }

            return info;
        }
    }
}