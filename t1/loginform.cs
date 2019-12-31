using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace t1
{
    public partial class loginform : Form
    {
        public string username { get; set; }
        public string password { get; set; }
        public bool isWindowsLogin { get; set; }

        public loginform()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (chkWinLogin.Checked == false)
            {
                this.username = txtUsername.Text.Trim();
                this.password = txtPassword.Text.Trim();
                this.isWindowsLogin = chkWinLogin.Checked;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void chkWinLogin_CheckedChanged(object sender, EventArgs e)
        {
            if (chkWinLogin.Checked)
            {
                txtUsername.Text = "";
                txtPassword.Text = "";
                txtUsername.Enabled = false;
                txtPassword.Enabled = false;
            }
            else
            {
                txtUsername.Enabled = true;
                txtPassword.Enabled = true;
            }
        }
    }
}
