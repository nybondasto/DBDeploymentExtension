using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using EnvDTE100;
using System.Configuration;
using t1.Properties;
using System.Diagnostics;
using System.Xml;

namespace t1
{
    public partial class ekaIkkuna : Form
    {
        private SqlConnection conn = null;
        private List<string> selectedDatabases = null;
        private string databaseUsername = "";
        private string databasePassword;

        public string buildPath = "";
        public string dacpacName = "";
        public string profileName = "";
        public string connStr = "";
        public bool windowsLogin = false;


        private string pubXml = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                                @"<Project ToolsVersion = ""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">" +
                                  "<PropertyGroup>" +
                                    "<IncludeCompositeObjects>True</IncludeCompositeObjects>" +
                                    "<TargetDatabaseName>---targetdb---</TargetDatabaseName>" +
                                    "<BlockOnPossibleDataLoss>True</BlockOnPossibleDataLoss>" +
                                    "<ScriptDatabaseOptions>False</ScriptDatabaseOptions>" +
                                    "<ProfileVersionNumber>1</ProfileVersionNumber>" +
                                  "</PropertyGroup>" +
                                "</Project>";


        public ekaIkkuna()
        {
            InitializeComponent();
        }

        public void msg(string s)
        {
            MessageBox.Show(s);
        }

        private void ekaIkkuna_Load(object sender, EventArgs e)
        {
            if (this.buildPath.Length < 30)
            {
                lblDacpac.Text = this.buildPath.ToString() + dacpacName;
            }
            else
            {
                lblDacpac.Text = this.buildPath.Substring(0, 30) + "..." + dacpacName;
            }

            HaeDBPalvelimet();
            ToolTip tt = new ToolTip();
            tt.SetToolTip(chkAll, "Select all");

            ToolTip tt2 = new ToolTip();
            tt2.SetToolTip(lblDacpac, this.buildPath + dacpacName);
        }


        private void HaeDBPalvelimet()
        {
            string servers = Settings1.Default.servers;
            string[] arr = servers.Split(',');

            this.Cursor = Cursors.WaitCursor;

            foreach (string srv in arr)
            {
                CmbServerName.Items.Add(srv);
            }
            CmbServerName.SelectedIndex = 0;
            this.Cursor = Cursors.Default;
        }


        private void AddToLog(string s)
        {
            txtLog.AppendText(s + "\r\n");
        }


        private void HaePalvelimenTietokannat()
        {
            AddToLog("Reading databases..");
            try
            {
                string sql = "SELECT name FROM sys.databases where state=0 order by name asc";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable tbl = new DataTable();
                da.Fill(tbl);

                if (tbl.Rows.Count > 0)
                {
                    cmbDatabases.View = View.Details;
                    cmbDatabases.Columns.Add("");
                    cmbDatabases.Columns.Add("");
                    cmbDatabases.CheckBoxes = true;

                    foreach (DataRow dr in tbl.Rows)
                    {
                        string dbname = dr["name"].ToString().Trim().ToUpper();
                        ListViewItem li = new ListViewItem("");
                        li.SubItems.Add(dbname);
                        cmbDatabases.Items.Add(li);
                    }
                    cmbDatabases.Columns[0].Width = 40;
                    cmbDatabases.Columns[1].Width = 200;
                    chkAll.Parent = cmbDatabases;
                    chkAll.Left = 4;
                    chkAll.Top = 7;
                    cmbDatabases.HeaderStyle = ColumnHeaderStyle.Nonclickable;

                    cmbDatabases.Visible = true;
                    chkAll.Visible = true;
                    btnSelect.Enabled = true;
                }
                else
                {
                    cmbDatabases.Items.Clear();
                    cmbDatabases.Columns.Clear();
                    cmbDatabases.Visible = false;
                    chkAll.Visible = false;
                    btnSelect.Enabled = false;

                    AddToLog("Databases not found!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void ToggleSelectAllDatabases()
        {
            for (int i = 0; i < cmbDatabases.Items.Count; i++)
            {
                cmbDatabases.Items[i].Checked = chkAll.Checked;
            }
        }



        private void btnLogin_Click(object sender, EventArgs e)
        {

            string server = CmbServerName.SelectedItem.ToString();

            if (conn == null || conn.State != ConnectionState.Open)
            {
                using (var login = new loginform())
                {
                    var result = login.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string username = login.username;            //values preserved after close
                        string password = login.password;
                        bool winLogin = login.isWindowsLogin;
                        windowsLogin = winLogin;

                        //-- Kirjaudutaan SQL-palvelimeen
                        
                        //-- Jos on lokaali SQLExpress-instanssi ja Windows-login
                        if (server == "LOCALHOST" && winLogin)
                            connStr = "Server=.\\SQLEXPRESS;Database=tempdb;Trusted_Connection=Yes";
                        else
                        {
                            //-- Palvelin lokaali ja SQL-login
                            if (server == "LOCALHOST" && !winLogin)
                            {
                                connStr = "Server=.\\SQLEXPRESS;Database=tempdb;User Id=" + username + ";Password=" + password + "; ";
                            }
                            else
                            {
                                //-- Palvelin muu kuin lokaali ja Windows-login
                                if (server != "LOCALHOST" && winLogin)
                                    connStr = "Server=" + server + ";Database=tempdb;Trusted_Connection=Yes";
                                else
                                {
                                    //-- Palvelin muu kuin lokaali ja SQL-login
                                    if (server != "LOCALHOST" && !winLogin)
                                    {
                                        connStr = "Server=" + server + ";Database=tempdb;User Id=" + username + ";Password=" + password + "; ";
                                    }
                                }
                            }
                        }

                        conn = new SqlConnection(connStr);

                        try
                        {
                            this.Cursor = Cursors.WaitCursor;
                            conn.Open();
                            txtLog.Text = "";

                            databaseUsername = username;
                            databasePassword = password;

                            AddToLog("User successfully logged in '" + server + "'");

                            HaePalvelimenTietokannat();

                            this.Cursor = Cursors.Default;
                            btnLogin.Text = "Logout";
                            CmbServerName.ForeColor = Color.Green;

                            CmbServerName.Font = new Font(CmbServerName.Font, FontStyle.Bold);
                            CmbServerName.SelectionLength = 0;

                        }
                        catch (Exception ex)
                        {
                            this.Cursor = Cursors.Default;
                            AddToLog("ERROR: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                Logout();
            }
        }


        private void Logout()
        {
            if (conn.State == ConnectionState.Open)
            {
                this.Cursor = Cursors.WaitCursor;
                conn.Close();
                btnLogin.Text = "Login";
                this.Cursor = Cursors.Default;
                cmbDatabases.Columns.Clear();
                CmbServerName.ForeColor = Color.Black;
                CmbServerName.Font = new Font(CmbServerName.Font, FontStyle.Regular);
                cmbDatabases.Items.Clear();
                cmbDatabases.Visible = false;
                chkAll.Visible = false;
                btnSelect.Enabled = false;
                btnFixChecksum.Enabled = false;
                btnPublish.Enabled = false;
                lblModified.Text = "";
                lblModified.Visible = false;
                AddToLog("User logged out successfully");
                windowsLogin = false;
            }
        }


        private void SelectDatabases()
        {
            string database = "";
            string tmp = "";
            selectedDatabases = new List<string>();

            for (int i = 0; i < cmbDatabases.Items.Count; i++)
            {
                if (cmbDatabases.Items[i].Checked)
                {
                    database = cmbDatabases.Items[i].SubItems[1].Text;
                    tmp += database + ", ";
                    selectedDatabases.Add(database);
                }
            }
            if (selectedDatabases.Count > 0)
            {
                tmp = tmp.TrimEnd(' ').TrimEnd(',');
                AddToLog("Selected databases: " + tmp);
                btnPublish.Enabled = true;
            }
            else
                btnPublish.Enabled = false;
        }




        private void DoPublish(string dacpac, string targetServer, string targetUsername, string targetPassword, List<string> targetDatabases, string targetProfile)
        {
            this.Cursor = Cursors.WaitCursor;

            foreach (string targetDatabase in targetDatabases)
            {
                string tmpXml = "";

                if (targetProfile.StartsWith(@""""))
                    targetProfile = targetProfile.Substring(1, targetProfile.Length - 2);

                XmlDocument xml = new XmlDocument();
                xml.Load(buildPath + targetProfile);
                XmlElement ele = (XmlElement)xml.ChildNodes.Item(1).ChildNodes.Item(0).ChildNodes.Item(1); //"TargetDatabaseName";
                ele.InnerText = ele.InnerText.Replace("---targetdb---", targetDatabase);
                tmpXml = buildPath + targetDatabase + ".xml";
                xml.Save(tmpXml);

                if (tmpXml.IndexOf(' ') > -1)
                    tmpXml = @"""" + tmpXml + @"""";

                string args = "/Action:Publish " +
                              "/SourceFile:" + dacpac + " " +
                              "/TargetServerName:" + targetServer + " ";

                //-- Jos ei ole Windows-login, liitetään mukaan käyttäjätunnus ja salasana
                if (!windowsLogin)
                {
                    args +=  "/TargetUser:" + targetUsername + " " +
                             "/TargetPassword:" + targetPassword + " ";
                }

                args += "/TargetDatabaseName:" + targetDatabase + " " +
                    "/Profile:" + tmpXml + " ";
                    

                AddToLog("Publishing arguments: " + args);

                this.Cursor = Cursors.WaitCursor;

                ProcessStartInfo procStartInfo = new ProcessStartInfo();
                procStartInfo.FileName = @"C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\sqlpackage.exe";
                procStartInfo.Arguments = args;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                procStartInfo.RedirectStandardError = true;

                using (Process process = new Process())
                {
                    process.StartInfo = procStartInfo;
                    process.Start();
                    process.WaitForExit();

                    // ---> I would add this here...
                    var result = process.StandardOutput.ReadToEnd();
                    string err = process.StandardError.ReadToEnd(); // <-- Capture errors

                    AddToLog(result);
                    if (!string.IsNullOrEmpty(err))
                    {
                        AddToLog("ERR: " + err); // <---- Print any errors for troubleshooting
                    }

                    // ----------------        
                }
                btnPublish.Enabled = false;
                lblModified.Visible = false;
                this.Cursor = Cursors.Default;
            }
        }


        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            ToggleSelectAllDatabases();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            //btnFixChecksum.Enabled = true;
            lblModified.Visible = true;
            SelectDatabases();
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            string serverName = "";

            if (CmbServerName.SelectedItem.ToString() == "LOCALHOST")
                serverName = CmbServerName.SelectedItem.ToString() + "\\SQLEXPRESS";
            else
                serverName = CmbServerName.SelectedItem.ToString();

            if (dacpacName.StartsWith(@""""))
                dacpacName = dacpacName.Substring(1, dacpacName.Length - 1);

            if (this.buildPath.StartsWith(@""""))
                this.buildPath = this.buildPath.Substring(1, this.buildPath.Length - 2);

            string dacpacPath = @"""" + this.buildPath + dacpacName + @"""";
            string targetProfile = @"""" + profileName + @"""";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(pubXml);



            xml.Save(this.buildPath + profileName);
            AddToLog("'" + profileName + "' has been written");

            txtLog.Text = "";
            DoPublish(dacpacPath, serverName, databaseUsername, databasePassword, selectedDatabases, targetProfile);
        }

        private void btnFixChecksum_Click(object sender, EventArgs e)
        {
            //AddToLog("Calculating fixed checksum...");

            //DacPacModification dpm = new DacPacModification();
            //string csum = dpm.RecalculateChecksum();
            //AddToLog("Checksum: " + csum);
            lblModified.Visible = true;
        }
    }
}
