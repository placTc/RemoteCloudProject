using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonClasses;

namespace RemoteCloudClient
{
    public partial class ClientFirstForm : Form
    {
        User user;
        public ClientFirstForm()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            User newUser = new User(this.usernameBox.Text, this.passwordBox.Text, "");
            string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeLoginRequest(newUser));
            if (response == "1200")
            {
                this.user = newUser;
                this.Hide();
                var form2 = new DriveForm(this.user);
                form2.Closed += (s, args) => this.Show();
                form2.Show();
            }
            else
            {
                errorLabel.Visible = true;
                errorLabel.Text = "Error logging in.";
            }
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
            User newUser;

            if (this.passwordBox.Text == this.repeatPassword.Text && this.passwordBox.Text.Length != 0)
            {
                newUser = new User(this.usernameBox.Text, this.passwordBox.Text, this.emailAddress.Text);
                string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeSignupRequest(newUser));
                if (response == "1202")
                {
                    signupButton.Location = new Point(signupButton.Location.X, signupButton.Location.Y - 31);
                    signupButton.Click += new EventHandler(this.signupButton_Click);
                    signupButton.Click -= this.signupButton_secondaryClick;
                    this.user = newUser;

                    repeatPassword.Enabled = false;
                    emailAddress.Enabled = false;
                    loginButton.Enabled = true;

                    this.usernameBox.Text = "";
                    this.passwordBox.Text = "";
                    this.errorLabel.Text = "";

                    repeatPassword.Visible = false;
                    emailAddress.Visible = false;
                    repeatPasswordLabel.Visible = false;
                    emailAddressLabel.Visible = false;
                    loginButton.Visible = true;
                }
                else
                {
                    this.errorLabel.Text = "Error occured.";
                }

            }
            else if (this.usernameBox.Text.Length == 0 || this.passwordBox.Text.Length == 0 || this.emailAddress.Text.Length == 0)
            {
                this.errorLabel.Text = "Please fill in all of the details.";
            }
        }
    }
}
