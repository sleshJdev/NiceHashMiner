using NiceHashMiner.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public partial class AuthForm : Form
    {

        public AuthDetails AuthDetails { set; get; }

        public AuthForm()
        {
            InitializeComponent();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string username = textBoxUserName.Text.Trim();
            string password = textBoxPassword.Text.Trim();
            try
            {
                AuthDetails = ApiService.Login(username, password);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Username/password is wrong.", "Authorization failed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
    }
}
