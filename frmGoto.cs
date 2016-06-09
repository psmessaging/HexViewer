/*
 * Created by SharpDevelop.
 * User: User
 * Date: 5/30/2016
 * Time: 6:42 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace HexViewer
{
    /// <summary>
    /// Description of frmGoto.
    /// </summary>
    public partial class FrmGoto : Form
    {
        public int m_address;
        public FrmGoto()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
        }
        
        void Btn_okClick(object sender, EventArgs e)
        {
            try {
                if(textBox1.Text.StartsWith("0x")) {
                    string t = textBox1.Text.Replace("0x", "");
                    m_address = Convert.ToInt32(t, 16);
                } else {
                    m_address = int.Parse(textBox1.Text);
                }
            } catch(Exception ex) {
                DialogResult = DialogResult.None;
            }
            
        }
    }
}
