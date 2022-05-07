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
using System.IO;
using System.Security.Cryptography;

namespace RemoteCloudClient
{
    public partial class DriveForm : Form
    {
        private User user;
        const string rootDirectory = @"\";
        string currentDirectory = @"\";
        public DriveForm(User user)
        {
            this.user = user;
            InitializeComponent();

            ImageList testList = new ImageList();
            testList.Images.Add("folderImage", Image.FromFile(@"assets\folder.png"));
            testList.Images.Add("fileImage", Image.FromFile(@"assets\file.png"));
            listView1.LargeImageList = testList;
            listView1.SmallImageList = testList;

            UpdateView(currentDirectory);
        }

        private void UploadFile(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            string filepath = fileDialog.FileName;
            string filename = Path.GetFileName(filepath);
            string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeUploadFileRequest(currentDirectory + filename, user, "300"));
            loadingLabel.Visible = true;
            progressBar1.Visible = true;
            HideShowControls(false);


            if (response == "1300")
            {
                response = "4300";
                byte[] temp = new byte[3000];

                ConvertFileToB64(filepath, @"temp.b64");
                FileStream fs = File.OpenRead(@"temp.b64");
                loadingLabel.Text = "Uploading";
                progressBar1.Minimum = 0;
                progressBar1.Maximum = (int)fs.Length;
                for (int i = 0; i <= fs.Length / 3000 && response == "4300"; i++)
                {
                    fs.Read(temp, 0, 3000);
                    temp = TrimEnd(temp);
                    progressBar1.Value += temp.Length;
                    response = AsynchronousClient.SendReceive(RequestSerializer.SerializeUploadFileRequest(currentDirectory + filename, user, "3300", Encoding.UTF8.GetString(temp)));
                    temp = new byte[3000];
                }
                loadingLabel.Text = "Processing";
                fs.Close();
                response = AsynchronousClient.SendReceive(RequestSerializer.SerializeUploadFileRequest(currentDirectory + filename, user, "5300"));
                if(response == "6300")
                {
                    UpdateView(currentDirectory);
                    File.Delete(@"temp.b64");
                    loadingLabel.Visible = false;
                    progressBar1.Visible = false;
                    progressBar1.Value = 0;
                }

            }
            HideShowControls(true);

        }

        private void DownloadFile(object sender, EventArgs e)
        {

        }

        private void DeleteFile(object sender, EventArgs e)
        {
            try
            {
                var selection = listView1.SelectedItems[0];
                if (selection.ImageKey == "fileImage")
                {
                    DialogResult res = MessageBox.Show("Are you sure you want to delete " + selection.Text + "?", "File Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeDirectoryRelatedRequest(currentDirectory + selection.Text + @"\", user, "302"));
                        if(response == "1302")
                        {
                            UpdateView(currentDirectory);
                        }
                    }
                }
            }
            catch (Exception ex) { };
        }
        private void MakeDirectory(object sender, EventArgs e)
        {
            HideShowControls(false);
            button4.Enabled = true;
            button4.Click -= this.MakeDirectory;
            button4.Click += new EventHandler(this.MakeDirectorySecondary);
            textBox2.Visible = true;
            textBox2.Enabled = true;
            button4.Text = "Confirm";
        }

        private void MakeDirectorySecondary(object sender, EventArgs e)
        {
            string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeDirectoryRelatedRequest(currentDirectory + textBox2.Text, user, "310"));
            if (response == "1310")
            {
                UpdateView(currentDirectory);
                button4.Click -= this.MakeDirectorySecondary;
                button4.Click += new EventHandler(this.MakeDirectory);
                textBox2.Visible = false;
                textBox2.Enabled = false;
                button4.Text = "Make Directory";
                HideShowControls(true);
            }
        }

        private void DeleteDirectory(object sender, EventArgs e)
        {
            try
            {
                var selection = listView1.SelectedItems[0];
                if (selection.ImageKey == "folderImage")
                {
                    DialogResult res = MessageBox.Show("Are you sure you want to delete " + selection.Text + "? \nAll subdirectories and files inside will be deleted!", "Directory Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeDirectoryRelatedRequest(currentDirectory + selection.Text + @"\", user, "311"));
                        if (response == "1311")
                        {
                            UpdateView(currentDirectory);
                        }
                    }
                }
            }
            catch (Exception ex) { };
        }

        private void LogOut(object sender, EventArgs e)
        {
            string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeLogoutRequest(user.getName()));
            this.Close();
        }

        private void UpdateView(string currentDir)
        {
            string response = AsynchronousClient.SendReceive(RequestSerializer.SerializeDirectoryRelatedRequest(currentDir, user, "100"));
            try
            {
                string[] responseSplit = response.Split(';');
                if(responseSplit[0] == "1100")
                {
                    string[] directories = responseSplit[2].Split(',');
                    string[] files = responseSplit[3].Split(',');

                    listView1.Items.Clear();

                    foreach (string dir in directories)
                    {
                        if(dir != string.Empty)
                        {
                            ListViewItem item = new ListViewItem(dir);
                            item.ImageKey = "folderImage";
                            item = listView1.Items.Add(item);
                        }
                    }

                    foreach (string file in files)
                    {
                        if(file != string.Empty)
                        {
                            ListViewItem item = new ListViewItem(file);
                            item.ImageKey = "fileImage";
                            item = listView1.Items.Add(item);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if(response == "2100")
                {
                    label1.Visible = true;
                    label1.Text = "Failed to update";
                }
            }
        }

        private void EnterDirectory(object sender, EventArgs e)
        {
            try
            {
                var selection = listView1.SelectedItems[0];
                if (selection.ImageKey == "folderImage")
                {
                    currentDirectory += selection.Text + @"\";
                    UpdateView(currentDirectory);
                    textBox1.Text = currentDirectory;
                }
            }
            catch (Exception ex) { };
        }

        private void LeaveDirectory(object sender, EventArgs e)
        {
            if(currentDirectory != rootDirectory)
            {
                string[] intermediate = currentDirectory.Split(@"\");
                intermediate = intermediate.Take(intermediate.Count() - 2).ToArray();
                currentDirectory = string.Join(@"\", intermediate) + @"\";
                UpdateView(currentDirectory);
                textBox1.Text = currentDirectory;
            }
        }

        private static void ConvertFileToB64(string filein, string fileout)
        {
            //encode 
            using (FileStream fs = File.Open(fileout, FileMode.Create))
            using (var cs = new CryptoStream(fs, new ToBase64Transform(),
                                                         CryptoStreamMode.Write))

            using (var fi = File.Open(filein, FileMode.Open))
            {
                fi.CopyTo(cs);
            }
        }

        private static void ConvertFileFromB64(string filein, string fileout)
        {
            // and decode
            using (FileStream f64 = File.Open(filein, FileMode.Open))
            using (var cs = new CryptoStream(f64, new FromBase64Transform(),
                                                        CryptoStreamMode.Read))
            using (var fo = File.Open(fileout, FileMode.Create))
            {
                cs.CopyTo(fo);
            }
        }
        public static byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }

        private void HideShowControls(bool state)
        {
            button1.Enabled = state;
            button2.Enabled = state;
            button3.Enabled = state;
            button4.Enabled = state;
            button5.Enabled = state;
            button6.Enabled = state;
            button7.Enabled = state;
            button8.Enabled = state;
        }
    }
}
