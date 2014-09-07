using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HexViewer
{
    public partial class Form1 : Form
    {
        String caption = "";
        VScrollBar scrollbar = new VScrollBar();
        OpenFileDialog dialog = null;
        HFile file;

        PictureBox picMarker = new PictureBox();
        PictureBox txtCaret = new PictureBox();
        PictureBox txtMarker = new PictureBox();
        int MarkerLine = -1;

        private int currentByteLocation = 0;
        private int currentLineByteLocation = 0;
        private int MarkedByteLocation = -1;
        private int MarkedCharLocation = -1;
        private int textcursorLoc = -1;
        private const int BYTESPERLINE = 32;
        private const int BYTESGROUP = 4;

        public Form1() {
            picMarker.BackColor = Color.Red;
            picMarker.Visible = false;
            txtMarker.BackColor = Color.Red;
            txtMarker.Visible = false;
            txtCaret.BackColor = Color.Black;
            txtCaret.Visible = false;

            this.AllowTransparency = true;
            dialog = new OpenFileDialog();
            scrollbar.Dock = DockStyle.Right;
            scrollbar.Minimum = 0;
            scrollbar.Maximum = 100;
            scrollbar.ValueChanged += new EventHandler(this.OnVScrollChanged);
            dialog.Multiselect = false;
            InitializeComponent();
            richTextBox1.Controls.Add(scrollbar);
            richTextBox1.Font = new Font("Courier New", 9, FontStyle.Bold);

            updateWindowTitle(this.Text);
            richTextBox1.Controls.Add(picMarker);
            richTextBox1.Controls.Add(txtMarker);
            richTextBox1.Controls.Add(txtCaret);
            contextMenuStrip1.Show(0, 0);
            contextMenuStrip1.Hide();
            
        }

        private void OnVScrollChanged(object sender, EventArgs e) {
            loadLines();

        }

        private void Form1_Load(object sender, EventArgs e) {
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            if (file != null) {
                file.close();
                file = null;
                richTextBox1.Text = "";
            }
            dialog.Filter = "All Files|*.*";
            DialogResult results = dialog.ShowDialog();
            file = new HFile(dialog.FileName);
            scrollbar.Enabled = true;
            fileSystemWatcher1.Path = System.IO.Directory.GetParent(dialog.FileName).FullName;
            fileSystemWatcher1.Filter = dialog.SafeFileName;
            loadLines();
            contextMenuStrip1.Enabled = true;
            if(dialog.SafeFileName.IndexOf(".") > -1)
                updateWindowTitle( dialog.SafeFileName.Substring(0, dialog.SafeFileName.IndexOf(".")));
            else
                updateWindowTitle(dialog.SafeFileName);
        }

        private void updateWindowTitle(string title) {
            this.Text = title;
            caption = this.Text;
        }

        private void loadLines() {
            int startingpos = scrollbar.Value;
            float theight = richTextBox1.Font.GetHeight();
            float rheight = (float)richTextBox1.Height;

            int lines = (int)((rheight - BYTESPERLINE) / theight) - 1;
            scrollbar.Maximum = (int)((file.length / BYTESPERLINE));
            String[] str = file.read(startingpos, BYTESPERLINE, lines, BYTESGROUP);
            richTextBox1.Lines = str;
        }

        private void richTextBox1_VScroll(object sender, EventArgs e) {
            RichTextBox rt = (RichTextBox)sender;
            
        }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e) {
            file.close();
            file = new HFile(e.FullPath);
            loadLines();
            if(this.WindowState == FormWindowState.Minimized)
                this.Text = "(Changed) " + caption;
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                TopMost = true;
                TopLevel = true;
            }
            else {
                TopMost = false;
                TopLevel = true;
            }
        }

        private void Form1_Enter(object sender, EventArgs e) {
            this.Text = caption;
            if(checkBox2.Checked == false)
                this.Opacity = 1;
        }

        private void Form1_Deactivate(object sender, EventArgs e) {
            if (checkBox2.Checked == false)
                this.Opacity = (double)trackBar1.Value / (double)trackBar1.Maximum;
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e) {

            if (richTextBox1.Lines.Count() <= 0 || (
                richTextBox1.Text.IndexOf("\t") < 0 ||
                richTextBox1.Text.IndexOf("\n") < 0))
                return;

            const int sizeofByte = 2;

            int linelength = richTextBox1.Lines[0].Length;
            int bytesShown = richTextBox1.Text.Split('\t')[1].Replace(" ", "").Length / 2;
            int selectStart = richTextBox1.SelectionStart;
            int selectEnd = richTextBox1.SelectionStart + richTextBox1.SelectionLength;
            int selectionlength = richTextBox1.SelectedText.Replace("\t","").Replace(" ", "").Length;
            int bytecount = (selectionlength / 2) + (selectionlength % 2);
            
            
            int lineCountToEnd = 0;
            int lineCountToStart = 0;
            int i = 0;
            do {
                i = richTextBox1.Text.IndexOf("\n", i+1);
                if (i > -1 && i < selectEnd)
                    lineCountToEnd = lineCountToEnd + 1;
                if (i > -1 && i < selectStart)
                    lineCountToStart = lineCountToStart + 1;
            } while (i > -1 && i < selectEnd);



            int EndlineNumber = lineCountToEnd + 1;
            int StartLineNumber = lineCountToStart + 1;

            int bytesStartPos = richTextBox1.Lines[StartLineNumber - 1].IndexOf("\t")+1;
            int z = linelength - ((linelength * StartLineNumber) - selectStart);
            if(StartLineNumber > 0)
                z = z - (StartLineNumber-1);
            //adjust for spaces

            //beginningByte = (z - bytesStartPos) + ((StartLineNumber - 1) * (bytesShown * sizeofByte)) / 2;
            
            int spaces = (z-bytesStartPos) / ((BYTESGROUP * sizeofByte));
            spaces = ((z-bytesStartPos) - spaces) / ((BYTESGROUP * sizeofByte));

            int lineByte = z - bytesStartPos;
            lineByte -= (((lineByte-spaces) / ((BYTESGROUP * sizeofByte))));
            lineByte /= sizeofByte;

            int actualByte = (((StartLineNumber - 1) * bytesShown) + lineByte) + (scrollbar.Value * 32);

            label1.Text = String.Format("0x{0:X02}", actualByte);
            currentByteLocation = actualByte;
            currentLineByteLocation = lineByte;

            if (lineByte == bytesShown) {
                if (selectionlength < 2)
                    richTextBox1.SelectionStart = richTextBox1.GetFirstCharIndexFromLine(StartLineNumber) + bytesStartPos;
            }
            else if (z == 9) {
                if (selectionlength < 2) {
                    if (StartLineNumber == 1) {
                        richTextBox1.SelectionStart = richTextBox1.GetFirstCharIndexFromLine(StartLineNumber - 1) + bytesStartPos + ((bytesShown * sizeofByte) + (bytesShown / BYTESGROUP)) - 1;
                    }
                    else {
                        richTextBox1.SelectionStart = richTextBox1.GetFirstCharIndexFromLine(StartLineNumber - 2) + bytesStartPos + ((bytesShown * sizeofByte) + (bytesShown / BYTESGROUP)) - 1;
                    }
                }
            }

            updateByteDifference();
            updateTextCursor();
        }

        private void updateTextCursor() {
            int charIndex = richTextBox1.SelectionStart+richTextBox1.SelectionLength;
            int lineIndex = richTextBox1.GetLineFromCharIndex(charIndex);
            int indexOfText = richTextBox1.Text.IndexOf("\t\t", charIndex)+2;

            textcursorLoc = Math.Max(currentLineByteLocation, 0) + indexOfText;
            Point p = richTextBox1.GetPositionFromCharIndex(textcursorLoc);
            txtCaret.SetBounds(p.X-1, p.Y+1, 2, (int)richTextBox1.Font.GetHeight());
            txtCaret.Visible = true;
        }

        private void updateByteDifference() {
            if (MarkedByteLocation > -1) {
                label3.Visible = true;
                label4.Visible = true;
                label5.Visible = true;
                int difference = Math.Abs(currentByteLocation - MarkedByteLocation);
                label4.Text = String.Format("0x{0:X02}", difference);
                label5.Text = difference.ToString();
            }
            else {
                label3.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
            }
        }
        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            this.Opacity = (double)trackBar1.Value/(double)trackBar1.Maximum;
        }

        private void drawMarkers() {
            if (richTextBox1.Text.Length <= 0)
                return;

            Point p = richTextBox1.GetPositionFromCharIndex(MarkedCharLocation);
            Point p2 = richTextBox1.GetPositionFromCharIndex(textcursorLoc);
            
            picMarker.SetBounds(p.X - 1, p.Y + 1, 2, (int)richTextBox1.Font.GetHeight());
            txtMarker.SetBounds(p2.X - 1, p2.Y + 1, 2, (int)richTextBox1.Font.GetHeight());
            picMarker.Visible = true;
            txtMarker.Visible = true;
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (e.ClickedItem.GetCurrentParent() == null)
                return;
            int index = e.ClickedItem.GetCurrentParent().Items.IndexOf(e.ClickedItem);
            if (index == 0) { //set marker
                MarkedByteLocation = currentByteLocation;
                MarkedCharLocation = richTextBox1.SelectionStart;
                MarkerLine = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart);
                drawMarkers();
                updateByteDifference();
            }
            else if (index == 1) { //clear marker
                MarkedByteLocation = -1;
                MarkedCharLocation = -1;
                picMarker.Visible = false;
                txtMarker.Visible = false;
                updateByteDifference();
            }
        }
    }
}
