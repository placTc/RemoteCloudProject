using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteCloudProject
{
    public partial class ClientFirstForm : Form
    {
        public ClientFirstForm()
        {
            InitializeComponent();
            
        }

        private void loginButton_Click(object sender, EventArgs e)
        {

        }

        private void signupButton_Click(object sender, EventArgs e)
        {
            signupButton.Location = new Point(signupButton.Location.X, signupButton.Location.Y + 31);
            signupButton.Click -= this.signupButton_Click;
            signupButton.Click += new EventHandler(this.signupButton_secondaryClick);

            repeatPassword.Enabled = true;
            emailAddress.Enabled = true;
            loginButton.Enabled = false;

            repeatPassword.Visible = true;
            emailAddress.Visible = true;
            repeatPasswordLabel.Visible = true;
            emailAddressLabel.Visible = true;
            loginButton.Visible = false;
        }

        private void signupButton_secondaryClick(object sender, EventArgs e)
        {
            signupButton.Location = new Point(signupButton.Location.X, signupButton.Location.Y - 31);
            signupButton.Click -= this.signupButton_secondaryClick;
            signupButton.Click += new EventHandler(this.signupButton_Click);

            User 
        }
    }
}
