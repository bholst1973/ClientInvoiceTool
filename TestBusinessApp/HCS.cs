﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TestBusinessApp
{
    public partial class HCS : Form
    {
        #region Collapse 
        //Collapse to definitions
        //CTRL + M, O

        //Expand all outlining
        //CTRL + M, X

        //Expand or collapse everything
        //CTRL + M, L
        #endregion

        SqlConnection HCSConnection = new SqlConnection(System.Configuration.ConfigurationManager
            .ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ConnectionString);
        ContextMenuStrip client_Column_Chooser = new System.Windows.Forms.ContextMenuStrip();
        ContextMenuStrip cell_content_Menu = new System.Windows.Forms.ContextMenuStrip();
        ContextMenuStrip invoice_Paid_Chooser = new System.Windows.Forms.ContextMenuStrip();

        string clientheaderfile;
        // Client object that holds the current client data to be updated.
        Client workingClient = new Client();
        //  Holds the current Client Data Grid row that is being updated.
        int clientupdaterow;
        decimal taxRate = 0.06875m;
        //decimal taxRate = 0.06500m;
        int invoicesupdaterow;
        Invoice workingInvoice = new Invoice();
        bool invoiceEditTaxWarning = true;
        int workingInvoiceNumber = -1;
        int workingInoviceRow = -1;
        string workingInvoiceClient = "";

        //Invoice column and row for InvoicesInvsDG
        int invsDGRow = -1;
        int invsDGColumn = -1;
        public HCS()
        {
            InitializeComponent();
        }

        private void HCS_Load(object sender, EventArgs e)
        {
            // Client Tab
            // TODO: This line of code loads data into the 'hCSDataSet.Client' table. You can move, or remove it, as needed.
            this.clientTableAdapter.Fill(this.hCSDataSet.Client);
            //  This should be executed early...
            createSytemFolderFiles();
            loadClientPresetData();
            setClientHeaderMenu();
            //  Remove grey space around the datagridview
            // invoiceDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            getClientCount();
            autosizeClientColumns();
            delClBut.Enabled = false;
            addClBut.Enabled = false;
            updtClBut.Enabled = false;

            //  Create Invoice Tab
            loadInvQty();
            loadInvDetails();
            loadInvClClientCB();
            addInvBut.Enabled = false;
            createInvBut.Enabled = false;
            createEstBut.Enabled = false;
            createInvRmvItmBut.Enabled = false;
            createInvClrItmsBut.Enabled = false;
            this.createInvoiceDataGridView.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.createInvoiceDataGridView.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.createInvoiceDataGridView.Columns["Tax"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.createInvoiceDataGridView.Columns["Cost"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            //Invoices
            loadInvoices();
            loadInvsClientCB();

            this.InvoicesInvsDG.Columns["Inv_Sub_Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.InvoicesInvsDG.Columns["Inv_Tax"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.InvoicesInvsDG.Columns["Inv_Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.InvoicesInvsDG.Columns["Inv_Cost"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.InvoicesInvsDG.Columns["Inv_Tax_Paid"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.InvoicesInvsDG.Columns["Inv_Date"].DefaultCellStyle.Format = "yyyy-MM-dd";
            this.invoiceItemsDG.Columns["Inv_Dt"].DefaultCellStyle.Format = "yyyy-MM-dd";
            enableInvEditFields(false);
            setPaidChooserMenu();

            // Admin
            adminAddHSBut.Enabled = false;
            activateComItBut.Enabled = false;
            deactivateComItBut.Enabled = false;
            adminActDelComItBut.Enabled = false;
            loadAdminCatDrpDwn();
            loadActiveInvItemDrpDwn();
            defaultTaxRateTB.Text = taxRate.ToString();
            setEffTxRateBut.Enabled = false;
            effectiveTaxRateTB.Text = taxRate.ToString();

            loadTestData();
        }

        public void loadTestData()
        {
            invClDTPicker.Value = new DateTime(2010, 03, 01);
        }

        #region         <<<<<<<<<<<<<<<<<<<----------  Client Tab  ---------->>>>>>>>>>>>>>>>>>>
        private void autosizeClientColumns()
        {
            for (int i = 0; i < clientDataGridView.Columns.Count - 1; i++)
            {
                clientDataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            clientDataGridView.Columns[clientDataGridView.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            for (int i = 0; i < clientDataGridView.Columns.Count; i++)
            {
                int colw = clientDataGridView.Columns[i].Width;
                clientDataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                clientDataGridView.Columns[i].Width = colw;
            }
        }

        private void getClientCount()
        {
            try
            {
                System.Data.SqlClient.SqlConnection sqlConnection1 =
                    new System.Data.SqlClient.SqlConnection(HCSConnection.ConnectionString);
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "SELECT COUNT(*) FROM Client";
                cmd.Connection = sqlConnection1;
                sqlConnection1.Open();
                Int32 count = (Int32)cmd.ExecuteScalar();
                // Account for blank account.
                count = count - 1;
                sqlConnection1.Close();
                clientCountLabel.Text = "Client Count: -> " + count.ToString();
            }

            catch (Exception e)
            {
                MessageBox.Show("getClientCount failed - " + e.ToString());
            }
        }

        private void AddClientButton_Click(object sender, EventArgs e)
        {
            List<string> ci = buildClientData();
            //Insert the row 
            try
            {
                insertHCSClient(ci);
                clearClientData();
                loadClientPresetData();
                loadInvClClientCB();
                loadInvsClientCB();
                autosizeClientColumns();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Insert failed.  Message - " + ex.ToString());
            }
        }

        private void delClBut_Click(object sender, EventArgs e)
        {
            int selectedRowIndex = clientDataGridView.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = clientDataGridView.Rows[selectedRowIndex];
            string id = Convert.ToString(selectedRow.Cells["clientIDDataGridViewTextBoxColumn"].Value);
            string query = "DELETE FROM Client WHERE Client_ID = " + id;
            if (MessageBox.Show("Do you want to remove this Client?", "Remove Client?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                executeQuery(query);
                getClientCount();
                delClBut.Enabled = false;
            }
            loadInvClClientCB();
        }

        //  This method builds a list of client data
        //  The data will be used to insert/update the client table
        public List<string> buildClientData()
        {
            List<string> clInfo = new List<string>();
            if (fNameTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(fNameTxtBx.Text); }

            if (lNameTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(lNameTxtBx.Text); }

            if (cNameTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(cNameTxtBx.Text); }

            if (bNameTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(bNameTxtBx.Text); }

            if (add1TxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(add1TxtBx.Text); }

            if (add2TxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(add2TxtBx.Text); }

            if (add3TxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(add3TxtBx.Text); }

            if (cityTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(cityTxtBx.Text); }

            if (stateTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            { clInfo.Add(stateTxtBx.Text); }

            if (zipTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            {
                if (zipETxtBx.Text == null || zipETxtBx.Text == "")
                {
                    clInfo.Add(zipTxtBx.Text);
                }
                else
                {
                    clInfo.Add(zipTxtBx.Text + "-" + zipETxtBx.Text);
                }
            }


            if (phone1TxtBx.Text == null)
            { clInfo.Add(""); }
            else
            {
                if (phone1TxtBx.Text.Length == 3 && phone2TxtBx.Text.Length == 3 && phone3TxtBx.Text.Length == 4)
                {
                    clInfo.Add("(" + phone1TxtBx.Text + ") - " + phone2TxtBx.Text + " - " + phone3TxtBx.Text);
                }
                else
                {
                    clInfo.Add("");
                }

            }

            if (emailTxtBx.Text == null)
            { clInfo.Add(""); }
            else
            {
                clInfo.Add(emailTxtBx.Text.ToLower());
            }

            return clInfo;
        }

        public void insertHCSClient(List<string> ci)
        {
            string fn = ci[0];
            string ln = ci[1];
            string cn = ci[2];
            string bn = ci[3];
            string ad1 = ci[4];
            string ad2 = ci[5];
            string ad3 = ci[6];
            string cty = ci[7];
            string st = ci[8];
            string zip = ci[9];
            string ph = ci[10];
            string em = ci[11];
            
            string query = "INSERT INTO Client (First_Name, Last_Name, Business_Name, Billing_Name, Address1, Address2, Address3, City, [State], Zip, Phone, Email)";
            query += " VALUES ('" + fn + "', '" + ln + "', '" + cn + "', '" + bn + "', '" + ad1 + "', '" + ad2 + "', '" + ad3 + "', '" + cty + "', '" + st + "', '" + zip + "', '" + ph + "', '" + em + "')";
            executeQuery(query);
            clientDataGridView.FirstDisplayedScrollingRowIndex = clientDataGridView.RowCount - 1;
            getClientCount();
        }

        private void createSytemFolderFiles()
        {
            //  Create HCS Folder
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string HCSFolder = Path.Combine(appData, "HCS");
            Directory.CreateDirectory(HCSFolder);

            //  Create Client Header File
            string clientheaders = appData + @"\HCS\ClientHeaders.txt";
            if (!File.Exists(clientheaders))
            {
                File.Create(clientheaders);
            }
            clientheaderfile = clientheaders;
        }

        private void setClientHeaderMenu()
        {
            client_Column_Chooser.Items.Add("ID").Name = "ID";
            client_Column_Chooser.Items.Add("First").Name = "First";
            client_Column_Chooser.Items.Add("Last").Name = "Last";
            client_Column_Chooser.Items.Add("Company").Name = "Company";
            client_Column_Chooser.Items.Add("Billing").Name = "Billing";
            client_Column_Chooser.Items.Add("Address 1").Name = "Address 1";
            client_Column_Chooser.Items.Add("Address 2").Name = "Address 2";
            client_Column_Chooser.Items.Add("Address 3").Name = "Address 3";
            client_Column_Chooser.Items.Add("City").Name = "City";
            client_Column_Chooser.Items.Add("State").Name = "State";
            client_Column_Chooser.Items.Add("Zip").Name = "Zip";
            client_Column_Chooser.Items.Add("Phone").Name = "Phone";
            client_Column_Chooser.Items.Add("Email").Name = "Email";
            client_Column_Chooser.Items.Add("-");
            client_Column_Chooser.Items.Add("Done").Name = "Done";
            client_Column_Chooser.Items.Add("Save").Name = "Save";

            //  Check if the header preset file exists if not activate all
            string path = clientheaderfile;
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                LoadClientHeaders();
            }

            else
            {
                ((ToolStripMenuItem)client_Column_Chooser.Items[0]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[1]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[2]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[3]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[4]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[5]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[6]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[7]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[8]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[9]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[10]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[11]).Checked = true;
                ((ToolStripMenuItem)client_Column_Chooser.Items[12]).Checked = true;
            }

            client_Column_Chooser.ItemClicked += new ToolStripItemClickedEventHandler(client_Column_Chooser_ItemClicked);
            client_Column_Chooser.AutoClose = false;
        }

        private void loadClientPresetData()
        {
            if (presetDataChkbx.Checked)
            {
                //fNameTxtBx.Text = "Testy";
                //lNameTxtBx.Text = "McTestFace";
                //add1TxtBx.Text = "1313 Mockingbird Lane";
                cityTxtBx.Text = "Plainview";
                stateTxtBx.Text = "MN";
                zipTxtBx.Text = "55964";
                phone1TxtBx.Text = "507";
                //phone2TxtBx.Text = "288";
                //phone3TxtBx.Text = "1313";
                //emailTxtBx.Text = "abc@123.com";
            }
        }

        private void clearClientData()
        {
            fNameTxtBx.Text = "";
            lNameTxtBx.Text = "";
            cNameTxtBx.Text = "";
            bNameTxtBx.Text = "";
            add1TxtBx.Text = "";
            add2TxtBx.Text = "";
            add3TxtBx.Text = "";
            cityTxtBx.Text = "";
            stateTxtBx.Text = "";
            zipTxtBx.Text = "";
            zipETxtBx.Text = "";
            phone1TxtBx.Text = "";
            phone2TxtBx.Text = "";
            phone3TxtBx.Text = "";
            emailTxtBx.Text = "";
        }

        // Called whenever client data is entered
        // If the criteria is met the Add Client button will appear
        private void verifyClientData()
        {
            bool fname = false;
            bool lname = false;
            bool cname = false;
            bool bname = false;
            bool phone = false;
            bool email = false;
            bool addC = false;

            if (fNameTxtBx.Text.Length > 0)
            {
                fname = true;
            }
            if (lNameTxtBx.Text.Length > 0)
            {
                lname = true;
            }
            if (cNameTxtBx.Text.Length > 0)
            {
                cname = true;
            }
            if (bNameTxtBx.Text.Length > 0)
            {
                bname = true;
            }
            if (phone1TxtBx.Text.Length + phone2TxtBx.Text.Length + phone3TxtBx.Text.Length == 10)
            {
                phone = true;
            }
            if (emailTxtBx.Text.Length > 0)
            {
                email = true;
            }

            // Conditions that return true
            // --> First name, last name, phone
            // --> First name, last name, email
            // --> Company name, phone
            // --> Company name, email
            if (fname && lname && bname && phone)
            {
                addC = true;
            }
            if (fname && lname && bname && email)
            {
                addC = true;
            }
            if (cname && bname && phone)
            {
                addC = true;
            }
            if (cname && bname && email)
            {
                addC = true;
            }

            //Enable the Add client button if conditions are met.
            if (addC)
            {
                addClBut.Enabled = true;
            }
            else
            {
                addClBut.Enabled = false;
            }
        }

        private void FNameTxtBx_TextChanged(object sender, EventArgs e)
        {
            if (trimChkbx.Checked)
            {
                fNameTxtBx.Text = fNameTxtBx.Text.Trim();
            }
            // only change billing name if no company name is present
            if(cNameTxtBx.Text.Length < 1)
            {
                bNameTxtBx.Text = fNameTxtBx.Text + " " + lNameTxtBx.Text;
            }
            verifyClientData();
        }

        private void lNameTxtBx_TextChanged(object sender, EventArgs e)
        {
            if (trimChkbx.Checked)
            {
                lNameTxtBx.Text = lNameTxtBx.Text.Trim();
            }
            // only change billing name if no company name is present
            if (cNameTxtBx.Text.Length < 1)
            {
                bNameTxtBx.Text = fNameTxtBx.Text + " " + lNameTxtBx.Text;
            }
            verifyClientData();
        }

        private void cNameTxtBx_TextChanged(object sender, EventArgs e)
        {
            if (trimChkbx.Checked)
            {
                cNameTxtBx.Text = cNameTxtBx.Text.Trim();
            }
            if(cNameTxtBx.Text.Length > 0)
            {
                bNameTxtBx.Text = cNameTxtBx.Text;
            }
            else
            {
                bNameTxtBx.Text = fNameTxtBx.Text + " " + lNameTxtBx.Text;
            }
            verifyClientData();
        }

        private void bNameTxtBx_TextChanged(object sender, EventArgs e)
        {
            if (trimChkbx.Checked)
            {
                bNameTxtBx.Text = bNameTxtBx.Text.Trim();
            }
            verifyClientData();
        }

        private void cityTxtBx_TextChanged(object sender, EventArgs e)
        {

            if (cityTxtBx.Text == "Plainview" || cityTxtBx.Text == "plainview")
            {
                this.zipTxtBx.TextChanged -= new System.EventHandler(this.zipTxtBx_TextChanged);
                cityTxtBx.Text = "Plainview";
                stateTxtBx.Text = "MN";
                zipTxtBx.Text = "55964";
                this.zipTxtBx.TextChanged += new System.EventHandler(this.zipTxtBx_TextChanged);
            }

            if (cityTxtBx.Text == "Elgin" || cityTxtBx.Text == "elgin")
            {
                this.zipTxtBx.TextChanged -= new System.EventHandler(this.zipTxtBx_TextChanged);
                cityTxtBx.Text = "Elgin";
                stateTxtBx.Text = "MN";
                zipTxtBx.Text = "55932";
                this.zipTxtBx.TextChanged += new System.EventHandler(this.zipTxtBx_TextChanged);
            }

            if (cityTxtBx.Text == "St Charles" || cityTxtBx.Text == "st charles")
            {
                this.zipTxtBx.TextChanged -= new System.EventHandler(this.zipTxtBx_TextChanged);
                cityTxtBx.Text = "St Charles";
                stateTxtBx.Text = "MN";
                zipTxtBx.Text = "55972";
                this.zipTxtBx.TextChanged += new System.EventHandler(this.zipTxtBx_TextChanged);
            }

            if (cityTxtBx.Text == "Wabasha" || cityTxtBx.Text == "wabasha")
            {
                this.zipTxtBx.TextChanged -= new System.EventHandler(this.zipTxtBx_TextChanged);
                cityTxtBx.Text = "Wabasha";
                stateTxtBx.Text = "MN";
                zipTxtBx.Text = "55981";
                this.zipTxtBx.TextChanged += new System.EventHandler(this.zipTxtBx_TextChanged);
            }

        }

        private void zipTxtBx_TextChanged(object sender, EventArgs e)
        {
            if (zipTxtBx.Text == "55964")
            {
                this.cityTxtBx.TextChanged -= new System.EventHandler(this.cityTxtBx_TextChanged);
                cityTxtBx.Text = "Plainview";
                stateTxtBx.Text = "MN";
                this.cityTxtBx.TextChanged += new System.EventHandler(this.cityTxtBx_TextChanged);
            }

            if (zipTxtBx.Text == "55932")
            {
                this.cityTxtBx.TextChanged -= new System.EventHandler(this.cityTxtBx_TextChanged);
                cityTxtBx.Text = "Elgin";
                stateTxtBx.Text = "MN";
                this.cityTxtBx.TextChanged += new System.EventHandler(this.cityTxtBx_TextChanged);
            }

            if (zipTxtBx.Text == "55972")
            {
                this.cityTxtBx.TextChanged -= new System.EventHandler(this.cityTxtBx_TextChanged);
                cityTxtBx.Text = "St Charles";
                stateTxtBx.Text = "MN";
                this.cityTxtBx.TextChanged += new System.EventHandler(this.cityTxtBx_TextChanged);
            }

            if (zipTxtBx.Text == "55981")
            {
                this.cityTxtBx.TextChanged -= new System.EventHandler(this.cityTxtBx_TextChanged);
                cityTxtBx.Text = "Wabasha";
                stateTxtBx.Text = "MN";
                this.cityTxtBx.TextChanged += new System.EventHandler(this.cityTxtBx_TextChanged);
            }

            if (zipTxtBx.Text == "55901" || zipTxtBx.Text == "55902" || zipTxtBx.Text == "55903" ||
                zipTxtBx.Text == "55904" || zipTxtBx.Text == "55905" || zipTxtBx.Text == "55906")
            {
                this.cityTxtBx.TextChanged -= new System.EventHandler(this.cityTxtBx_TextChanged);
                cityTxtBx.Text = "Rochester";
                stateTxtBx.Text = "MN";
                this.cityTxtBx.TextChanged += new System.EventHandler(this.cityTxtBx_TextChanged);
            }

        }

        private void phone1TxtBx_TextChanged(object sender, EventArgs e)
        {
            verifyClientData();
            if(phone1TxtBx.Text.Length == 3)
            {
                int x = this.phone1TxtBx.TabIndex;
                SendKeys.Send("{TAB}");
            }
        }

        private void phone2TxtBx_TextChanged(object sender, EventArgs e)
        {
            verifyClientData();
            if (phone2TxtBx.Text.Length == 3)
            {
                int x = this.phone2TxtBx.TabIndex;
                SendKeys.Send("{TAB}");
            }
        }

        private void phone3TxtBx_TextChanged(object sender, EventArgs e)
        {
            verifyClientData();
        }

        private void emailTxtBx_TextChanged(object sender, EventArgs e)
        {
            if (trimChkbx.Checked)
            {
                emailTxtBx.Text = emailTxtBx.Text.Trim();
            }
            verifyClientData();
        }

        private void clientDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Checks if one row is highlighted, if so the delete button is activated.
            if (clientDataGridView.SelectedRows.Count > 0 && clientDataGridView.SelectedRows.Count < 2)
            {
                delClBut.Enabled = true;
            }
            else
            {
                delClBut.Enabled = false;
            }

        }

        private void executeQuery(string q)
        {
            try
            {
                System.Data.SqlClient.SqlConnection sqlConnection1 =
                new System.Data.SqlClient.SqlConnection(HCSConnection.ConnectionString);
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = q;
                cmd.Connection = sqlConnection1;
                sqlConnection1.Open();
                cmd.ExecuteNonQuery();
                sqlConnection1.Close();
                refreshClientHeaders();
            }
            catch (Exception e)
            {
                MessageBox.Show("Execute Query failed: " + e.ToString());
            }
        }

        private void clientDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                client_Column_Chooser.Show(this, clientDataGridView.PointToClient(Cursor.Position));
            }
        }

        private void client_Column_Chooser_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name.ToString())
            {
                case "ID":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[0]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[0]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[0]).Checked = true;
                    }
                    break;

                case "First":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[1]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[1]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[1]).Checked = true;
                    }
                    break;

                case "Last":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[2]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[2]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[2]).Checked = true;
                    }
                    break;

                case "Company":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[3]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[3]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[3]).Checked = true;
                    }
                    break;

                case "Billing":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[4]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[4]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[4]).Checked = true;
                    }
                    break;

                case "Address 1":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[5]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[5]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[5]).Checked = true;
                    }
                    break;

                case "Address 2":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[6]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[6]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[6]).Checked = true;
                    }
                    break;

                case "Address 3":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[7]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[7]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[7]).Checked = true;
                    }
                    break;

                case "City":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[8]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[8]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[8]).Checked = true;
                    }
                    break;

                case "State":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[9]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[9]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[9]).Checked = true;
                    }
                    break;

                case "Zip":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[10]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[10]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[10]).Checked = true;
                    }
                    break;

                case "Phone":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[11]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[11]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[11]).Checked = true;
                    }
                    break;

                case "Email":
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[12]).Checked)
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[12]).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)client_Column_Chooser.Items[12]).Checked = true;
                    }
                    break;

                case "Done":
                    refreshClientHeaders();
                    autosizeClientColumns();
                    client_Column_Chooser.Close();
                    break;

                case "Save":
                    saveClientHeaders();
                    MessageBox.Show("Columns are saved!");
                    refreshClientHeaders();
                    autosizeClientColumns();
                    client_Column_Chooser.Close();
                    break;
            }
        }

        public void refreshClientHeaders()
        {
            for (int i = 0; i < clientDataGridView.ColumnCount; i++)
            {
                if (((ToolStripMenuItem)client_Column_Chooser.Items[i]).Checked)
                {
                    clientDataGridView.Columns[i].Visible = true;
                }
                else
                {
                    clientDataGridView.Columns[i].Visible = false;
                }
            }
            //Refersh data in the data grid
            this.clientTableAdapter.Fill(this.hCSDataSet.Client);
        }

        public void LoadClientHeaders()
        {
            string path = clientheaderfile;
            //  Check if the file exists
            if (File.Exists(path))
            {
                //  Check if the file is empty
                if (new FileInfo(path).Length != 0)
                {
                    StreamReader sr = new StreamReader(path);
                    //Read saved header file
                    for (int i = 0; i < clientDataGridView.ColumnCount; i++)
                    {
                        // Read File and activate saved client headers
                        int ch = Convert.ToInt32(sr.ReadLine());
                        if (ch == 1)
                        {
                            ((ToolStripMenuItem)client_Column_Chooser.Items[i]).Checked = true;
                            clientDataGridView.Columns[i].Visible = true;
                        }
                        else
                        {
                            ((ToolStripMenuItem)client_Column_Chooser.Items[i]).Checked = false;
                            clientDataGridView.Columns[i].Visible = false;
                        }

                    }
                    sr.Close();
                    refreshClientHeaders();
                }

                else
                {
                    MessageBox.Show(path + " is emtpy.");
                }
            }
            else
            {
                MessageBox.Show(path + " does not exist.");
            }
        }

        private void saveClientHeaders()
        {
            //  Save client header presets
            try
            {
                string path = clientheaderfile;

                //Check if File Exists
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                FileInfo logfile = new FileInfo(path);
                FileStream fs = logfile.Open(FileMode.Append, FileAccess.Write, FileShare.None);
                StreamWriter sw = new StreamWriter(fs);
                //  If checked write 1, if not write 0
                for (int i = 0; i < clientDataGridView.ColumnCount; i++)
                {
                    if (((ToolStripMenuItem)client_Column_Chooser.Items[i]).Checked == true)
                    {
                        sw.WriteLine(1);
                    }
                    else
                    {
                        sw.WriteLine(0);
                    }

                }

                sw.Close();
                fs.Close();
                sw = null;
                fs = null;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void presetDataChkbx_CheckedChanged(object sender, EventArgs e)
        {
            if (presetDataChkbx.Checked)
            {
                loadClientPresetData();
            }
            else
            {
                clearClientData();
            }
        }

        private void clrdataBut_Click(object sender, EventArgs e)
        {
            clearClientData();
            updtClBut.Enabled = false;
        }

        private void clientDataGridView_DoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            clientupdaterow = clientDataGridView.CurrentRow.Index;
            int clID = Convert.ToInt32(clientDataGridView.CurrentRow.Cells[0].Value.ToString());
            LoadClientEdit(clID);
        }

        // This method loads the text boxes with the double clicked clients data
        private void LoadClientEdit(int cl)
        {
            Client client = new Client();
            try
            {
                // Clear text boxes
                clearClientData();
                // Get client data
                client = client.GetClientbyID(cl);
                // Populate client properties
                fNameTxtBx.Text = client.First;
                lNameTxtBx.Text = client.Last;
                cNameTxtBx.Text = client.Company;
                bNameTxtBx.Text = client.Billing;
                add1TxtBx.Text = client.Address1;
                add2TxtBx.Text = client.Address2;
                add3TxtBx.Text = client.Address3;
                cityTxtBx.Text = client.City;
                stateTxtBx.Text = client.State;
                if(client.Zip.Length > 0)
                {
                    zipTxtBx.Text = client.Zip.Substring(0, 5);
                    if (client.Zip.Length > 5)
                    {
                        zipETxtBx.Text = client.Zip.Substring(6, 4);
                    }
                }

                if (client.Phone.Length == 18)
                {
                    phone1TxtBx.Text = client.Phone.Substring(1, 3);
                    phone2TxtBx.Text = client.Phone.Substring(8, 3);
                    phone3TxtBx.Text = client.Phone.Substring(14, 4);
                }
                emailTxtBx.Text = client.Email;
                // Set public object to the client we want to update
                workingClient = client;
                // Make the update button visible
                updtClBut.Enabled = true;
                addClBut.Enabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error - LoadClientEdit: " + e.ToString());
            }

        }

        private void updtClBut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to Update this Client?", "Update Client?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Client client = new Client();
                client = workingClient;
                string clID = client.ID.ToString();
                // set workingClient to null
                workingClient = null;
                List<string> clData = buildClientData();
                updateHCSClient(clData, clID);
                updtClBut.Enabled = false;
            }
        }

        public void updateHCSClient(List<string> ci, string ID)
        {
            string fn = ci[0];
            string ln = ci[1];
            string cn = ci[2];
            string bn = ci[3];
            string ad1 = ci[4];
            string ad2 = ci[5];
            string ad3 = ci[6];
            string cty = ci[7];
            string st = ci[8];
            string zip = ci[9];
            string ph = ci[10];
            string em = ci[11];

            string query = "USE HCS UPDATE Client SET First_Name = '" + fn + "'," +
                                                      "Last_Name = '" + ln + "'," +
                                                      "Business_Name = '" + cn + "'," +
                                                      "Billing_Name = '" + bn + "'," +
                                                      "Address1 = '" + ad1 + "'," +
                                                      "Address2 = '" + ad2 + "'," +
                                                      "Address3 = '" + ad3 + "'," +
                                                      "City = '" + cty + "'," +
                                                      "[State] = '" + st + "'," +
                                                      "Zip = '" + zip + "'," +
                                                      "Phone = '" + ph + "'," +
                                                      "Email = '" + em + "'" +
                                                      " WHERE Client_ID = " + ID;
            executeQuery(query);
            clientDataGridView.FirstDisplayedScrollingRowIndex = clientDataGridView.RowCount - 1;
            clientDataGridView.CurrentCell = clientDataGridView.Rows[clientupdaterow].Cells[0];
            clientDataGridView.Rows[clientupdaterow].Selected = true;
            clearClientData();
            loadInvClClientCB();
            loadInvsClientCB();
            getClientCount();
        }
        #endregion

        #region         <<<<<<<<<<<<<<<<<<<----------  Create Invoice Tab  ---------->>>>>>>>>>>>>>>>>>>
        private void loadInvQty()
        {
            for (int i = 1; i < 10; i++)
            {
                invCQtyCmbBx.Items.Add(i);
            }
        }

        //  Load Invoice Items Combo Boxes
        public void loadInvDetails()
        {
            this.invCLItemCmbBx.SelectedIndexChanged -= new System.EventHandler(this.invCLItemCmbBx_SelectedIndexChanged);
            //this.actInvItemCmBx.SelectedIndexChanged -= new System.EventHandler(this.actInvItemCmBx_SelectedIndexChanged);
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                try
                {
                    string query = "SELECT GS_Details FROM GoodsServices WHERE GS_Active = 1";
                    SqlDataAdapter da1 = new SqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet ds1 = new DataSet();
                    da1.Fill(ds1, "GoodsServices");
                    invCLItemCmbBx.DataSource = ds1.Tables["GoodsServices"];
                    invCLItemCmbBx.DisplayMember = "GS_Details";
                    invCLItemCmbBx.ValueMember = "GS_Details";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured: " + ex.ToString());
                }
                invCLItemCmbBx.SelectedIndex = -1;
                this.invCLItemCmbBx.SelectedIndexChanged += new System.EventHandler(this.invCLItemCmbBx_SelectedIndexChanged);
            }
        }

        public void loadInvClClientCB()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                try
                {
                    string query = "SELECT BILLING_NAME FROM Client";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds, "Client");
                    invClCustCmbBX.DataSource = ds.Tables["Client"];
                    //invsClientCB.DataSource = ds.Tables["Client"];
                    invClCustCmbBX.DisplayMember = "Billing_Name";
                    //invsClientCB.DisplayMember = "Billing_Name";
                    invClCustCmbBX.ValueMember = "Billing_Name";
                    //invsClientCB.ValueMember = "Billing_Name";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured: " + ex.ToString());
                }
                invClCustCmbBX.SelectedIndex = -1;
                invsClientCB.SelectedIndex = -1;
            }
        }

        private void invCLItemCmbBx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(InvClcustTxtBx.Text.Length > 0)
            {
                InvClcustTxtBx.Text = "";
            }
            invCQtyCmbBx.SelectedIndex = 0;
            string gs_detail = invCLItemCmbBx.Text;
            decimal price = 0.00m;
            try
            {
                GoodsServices gs = new GoodsServices();
                price = gs.getGSPriceByDetail(gs_detail);
                // Make it pretty for the text box
                string p = price.ToString();
                p = p.Remove(p.Length - 2, 2);
                invCLPriceTxtBx.Text = "$ " + p;
                verifyAddToInvoice();
            }
            //string s = (Convert.ToDouble(textbox1.Text) / 100).ToString("0.00");

            catch (Exception ex)
            {
                MessageBox.Show("getGSPriceByDetail Error :" + ex.ToString());
            }
        }

        private void custTxtBx_TextChanged(object sender, EventArgs e)
        {
            if (invCLItemCmbBx.SelectedIndex != -1)
            {
                invCLItemCmbBx.SelectedIndex = -1;
            }
            if (invCLPriceTxtBx.Text != "")
            {
                invCLPriceTxtBx.Text = "";
            }
            verifyAddToInvoice();

        }

        private void custTxtBx_Leave(object sender, EventArgs e)
        {
            if (InvClcustTxtBx.Text.Length > 0 && invCQtyCmbBx.SelectedIndex == -1)
            {
                invCQtyCmbBx.SelectedIndex = 0;
            }
        }

        private void invCQtyCmbBx_SelectedIndexChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void invCLPriceTxtBx_TextChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void cmpclnpChkBx_CheckedChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void dataRecChkBx_CheckedChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void printstpChkBx_CheckedChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void compstpChkBx_CheckedChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void routerChkBx_CheckedChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void bootUSBChkBx_CheckedChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void inslOSChkBx_CheckedChanged(object sender, EventArgs e)
        {
            verifyAddToInvoice();
        }

        private void verifyAddToInvoice()
        {
            bool InvItem = false;
            bool Qty = false;
            bool Price = false;
            bool iiqp = false;
            bool checkbox = false;
            decimal Prc = 0.00m;
            string textPrice = invCLPriceTxtBx.Text.ToString();

            if (invCLItemCmbBx.SelectedIndex != -1)
            {
                InvItem = true;
            }

            if (InvClcustTxtBx.Text.Length > 0)
            {
                InvItem = true;
            }

            if (invCQtyCmbBx.SelectedIndex != -1)
            {
                Qty = true;
            }

            if (textPrice.Length > 0 && textPrice != ".")
            {
                //Prc = decimal.Parse(invCLPriceTxtBx.Text.ToString());
                if (Prc >= 0.00m)
                {
                    Price = true;
                }
            }

            if (InvItem && Qty && Price)
            {
                iiqp = true;
            }

            if (cmpclnpChkBx.Checked || dataRecChkBx.Checked || printstpChkBx.Checked ||
            compstpChkBx.Checked || routerChkBx.Checked || bootUSBChkBx.Checked ||
            inslOSChkBx.Checked)
            {
                checkbox = true;
            }

            if (iiqp || checkbox)
            {
                addInvBut.Enabled = true;
            }
            else
            {
                addInvBut.Enabled = false;
            }

        }

        public void addInvBut_Click(object sender, EventArgs e)
        {
            // Qty
            int Qty = 0;
            // Details
            string Details = "";
            // Price
            decimal Price = 0.00m;
            // Total
            decimal Total = 0.00m;
            // Tax
            decimal Tax = 0.00m;
            // Cost
            decimal Cost = 0.0m;

            if (invCLItemCmbBx.SelectedIndex != -1)
            {
                Details = invCLItemCmbBx.Text;
            }

            if (InvClcustTxtBx.Text.Length > 0)
            {
                Details = Details + " - " + InvClcustTxtBx.Text;
            }

            if (invCQtyCmbBx.SelectedIndex != -1)
            {
                Qty = int.Parse(invCQtyCmbBx.SelectedItem.ToString());
            }

            if (invCLPriceTxtBx.Text.Length > 0)
            {
                string price = invCLPriceTxtBx.Text;
                if(price.Contains("$"))
                {
                    price = price.Trim('$');
                }
                price.Trim();
                //Price = decimal.Parse(invCLPriceTxtBx.Text.Substring(2, invCLPriceTxtBx.Text.Length - 2));
                Price = decimal.Parse(price);
            }

            if(invCLCostTxtBx.Text.Length > 0)
            {
                string cost = invCLCostTxtBx.Text;
                if(cost.Contains("$"))
                {
                    cost = cost.Trim('$');
                }
                cost.Trim();
                Cost = decimal.Parse(cost);
            }

            Total = Qty * Price;
            //Tax = decimal.Round(Total * taxRate,2);
            Tax = Total * taxRate;
            if (Total >= 0 && Details.Length > 0)
            {
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }

            if (cmpclnpChkBx.Checked)
            {
                Qty = 1;
                Details = "General Computer Cleanup/Virus Remval";
                Price = 75.00m;
                Total = 75.00m;
                //Tax = decimal.Round(Total * taxRate, 2);
                Tax = Total * taxRate;
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }

            if (dataRecChkBx.Checked)
            {
                Qty = 1;
                Details = "Data Recovery";
                Price = 25.00m;
                Total = 25.00m;
                //Tax = decimal.Round(Total * taxRate, 2);
                Tax = Total * taxRate;
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }

            if (printstpChkBx.Checked)
            {
                Qty = 1;
                Details = "Printer  Installation/Setup";
                Price = 65.00m;
                Total = 65.00m;
                //Tax = decimal.Round(Total * taxRate, 2);
                Tax = Total * taxRate;
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }

            if (compstpChkBx.Checked)
            {
                Qty = 1;
                Details = "New Computer Setup";
                Price = 75.00m;
                Total = 75.00m;
                //Tax = decimal.Round(Total * taxRate, 2);
                Tax = Total * taxRate;
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }

            if (routerChkBx.Checked)
            {
                Qty = 1;
                Details = "Router Installation/Setup";
                Price = 65.00m;
                Total = 65.00m;
                //Tax = decimal.Round(Total * taxRate, 2);
                Tax = Total * taxRate;
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }

            if (bootUSBChkBx.Checked)
            {
                Qty = 1;
                Details = "32GB USB Recovery Drive with Image";
                Price = 35.00m;
                Total = 35.00m;
                //Tax = decimal.Round(Total * taxRate, 2);
                Tax = Total * taxRate;
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }

            if (inslOSChkBx.Checked)
            {
                Qty = 1;
                Details = "Install Operating System";
                Price = 125.00m;
                Total = 125.00m;
                //Tax = decimal.Round(Total * taxRate, 2);
                Tax = Total * taxRate;
                this.createInvoiceDataGridView.Rows.Add(Qty, Details, Price, Total, Tax, Cost);
            }
            invCLItemCmbBx.SelectedIndex = -1;
            clearCreateInvItems();
            calculateSummaryofCost();
            createInvClrItmsBut.Enabled = true;
            InvClcustTxtBx.Text = "";
            invCLCostTxtBx.Text = "";
        }

        public void clearCreateInvItems()
        {
            invCLItemCmbBx.Text = "";
            invCQtyCmbBx.SelectedIndex = -1;
            invCLPriceTxtBx.Text = "";
            cmpclnpChkBx.Checked = false;
            dataRecChkBx.Checked = false;
            printstpChkBx.Checked = false;
            compstpChkBx.Checked = false;
            routerChkBx.Checked = false;
            bootUSBChkBx.Checked = false;
            inslOSChkBx.Checked = false;
        }

        public void calculateSummaryofCost()
        {
            decimal qty = 0.00m;
            decimal price = 0.00m;
            decimal total = 0.00m;
            decimal tax = 0.00m;
            decimal cost = 0 - .00m;
            decimal pretaxtotal = 0.00m;
            decimal totalcost = 0.00m;
            decimal grandTotal = 0m;

            foreach (DataGridViewRow dr in createInvoiceDataGridView.Rows)
            {
                qty = Convert.ToDecimal(dr.Cells[0].Value);
                price = Convert.ToDecimal(dr.Cells[2].Value);
                cost = Convert.ToDecimal(dr.Cells[5].Value);
                total = qty * price;
                tax = total * taxRate;
                dr.Cells[2].Value = price.ToString("0.00##");
                dr.Cells[3].Value = total.ToString("0.00##");
                dr.Cells[4].Value = tax.ToString("0.0000");
                total = Convert.ToDecimal(dr.Cells[3].Value);
                pretaxtotal += total;
                totalcost += cost;
            }

            tax = decimal.Round(pretaxtotal * taxRate, 2);
            grandTotal = pretaxtotal + tax;
            invclTotalTxtBx.Text = "$ " + pretaxtotal.ToString();
            invclTaxlTxtBx.Text = "$ " + tax.ToString();
            invclGrandTotalTxtBx.Text = "$ " + grandTotal.ToString();
            invclTCostTxtBx.Text = "$ " + totalcost.ToString();

            if (grandTotal > 0 && invClCustCmbBX.SelectedIndex > -1)
            {
                createInvBut.Enabled = true;
                createEstBut.Enabled = true;
            }
            else
            {
                createInvBut.Enabled = false;
                createEstBut.Enabled = false;
            }
        }

        private void createInvoiceDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //string temp = createInvoiceDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
            calculateSummaryofCost();
        }

        private void createInvRmvItmBut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to remove this Item?", "Remove Item?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach(DataGridViewRow r in createInvoiceDataGridView.SelectedRows)
                {
                    if(!r.IsNewRow)
                    {
                        createInvoiceDataGridView.Rows.RemoveAt(r.Index);
                    }
                }
                createInvRmvItmBut.Enabled = false;
                if(createInvoiceDataGridView.Rows.Count < 1)
                {
                    createInvClrItmsBut.Enabled = false;
                }
                calculateSummaryofCost();
            }
        }

        private void createInvClrItmsBut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to remove all invoice Items?", "Clear Invoice?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                createInvoiceDataGridView.Rows.Clear();
                createInvClrItmsBut.Enabled = false;
                calculateSummaryofCost();
                invCLItemCmbBx.SelectedIndex = -1;
                InvClcustTxtBx.Text = "";
                invCQtyCmbBx.SelectedIndex = -1;
                invCLPriceTxtBx.Text = "";
                invCLCostTxtBx.Text = "";
            }
        }

        private void createInvoiceDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Checks if one row is highlighted, if so the delete button is activated.
            if (createInvoiceDataGridView.SelectedRows.Count > 0)
            {
                createInvRmvItmBut.Enabled = true;                
            }
            else
            {
                createInvRmvItmBut.Enabled = false;
            }
        }

        private void invClCustCmbBX_SelectedIndexChanged(object sender, EventArgs e)
        {
            decimal tot = 0.00m;
            if (invclGrandTotalTxtBx.Text.Length > 3)
            {
                tot = Convert.ToDecimal(invclGrandTotalTxtBx.Text.Substring(2));
            }
            
            if (tot > 0 && invClCustCmbBX.SelectedIndex > 0)
            {
                createInvBut.Enabled = true;
                createEstBut.Enabled = true;
            }
            else
            {
                createInvBut.Enabled = false;
                createEstBut.Enabled = false;
            }
        }

        private void createInvBut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Create this Invoice?", "Create Invoice?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string client = invClCustCmbBX.GetItemText(invClCustCmbBX.SelectedItem);
                workingClient = workingClient.GetClientbyName(client);

                int cid = workingClient.ID;
                int invNum = GetLastInvNum() + 1;
                DateTime dt = DateTime.Now;
                string bname = workingClient.Billing;
                int Qty = 0;
                string Det = "";
                decimal Price = 0.00m;
                decimal Tax = 0.00m;
                decimal Total = 0.0000m;
                decimal Cost = 0.0000m;
                string notes = "";

                foreach (DataGridViewRow dr in createInvoiceDataGridView.Rows)
                {
                    Qty = Convert.ToInt32(dr.Cells[0].Value);
                    Det = dr.Cells[1].Value.ToString();
                    Det = Det.Replace("'", "''");
                    Price = Convert.ToDecimal(dr.Cells[2].Value);
                    Tax = Convert.ToDecimal(dr.Cells[4].Value);
                    Cost = Convert.ToDecimal(dr.Cells[5].Value);

                    Total = Qty * Price * (1 + taxRate);

                    try
                    {
                                   string query = "USE HCS INSERT INTO Invoice (INV_Client_ID, INV_NUM, INV_Date, INV_Billing_Name, INV_Qty, " +
                                   "INV_Details, INV_Price, INV_Tax, INV_Total, INV_Notes, INV_Paid, Inv_Cost, Inv_GrossProfit, Inv_TaxPaid)" +
                                   "VALUES (" + cid + "," + invNum + ",'" + invClDTPicker.Value.ToString("yyyy-MM-dd") + "','" + bname + "'," +
                                   Qty + ",'" + Det + "'," + Price + "," + Tax + "," + Total + ",'" + notes + "'," + 0 + ',' + Cost + ',' + 0 + ',' + 0 + ")";

                        executeQuery(query);
                        //MessageBox.Show(query);

                    }
                    catch(Exception excp)
                    {
                        MessageBox.Show("Invoice couldn't be created.  Possible syntax Error." + Environment.NewLine +
                                         "Details: " + Environment.NewLine + excp);
                    }
                
                }

                createInvoiceDataGridView.Rows.Clear();
                createInvoiceDataGridView.Refresh();
                invClCustCmbBX.SelectedIndex = -1;
                invclTotalTxtBx.Text = "";
                invclTaxlTxtBx.Text = "";
                invclGrandTotalTxtBx.Text = "";
                invclTCostTxtBx.Text = "";
                executeProfit_SP();
                loadInvoices();
            }

            //MessageBox.Show("Client ID: " + cid.ToString() +
            //    Environment.NewLine + "New Inv Num: " + invNum.ToString() + 
            //    Environment.NewLine + "Date: " + dt.ToString("yyyy-MM-dd") +
            //    Environment.NewLine + "Bill Client: " + bname); 
        }

        public int GetLastInvNum()
        {
            //var con = ConfigurationManager.ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ToString();
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                int num = 0;
                string query = "USE HCS SELECT MAX(INV_Num) as INV_NUM FROM Invoice";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        num = (reader["INV_NUM"] as int? ?? default(int));
                    }
                }

                return num;
            }
        }

        private void invClDTPicker_ValueChanged(object sender, EventArgs e)
        {

        }
        #endregion

        #region         <<<<<<<<<<<<<<<<<<<----------  Invoices Tab  ---------->>>>>>>>>>>>>>>>>>>

        public void loadInvoices()
        {
            if (InvoicesInvsDG.Rows.Count > 0)
            { 
                InvoicesInvsDG.Rows.Clear();
            }
            if(invoiceItemsDG.Rows.Count > 0)
            {
                invoiceItemsDG.Rows.Clear();
            }
            var con = ConfigurationManager.ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ToString();
            //  Build list of Invoices
            List<Invoice> invs = new List<Invoice>();
            using (SqlConnection myCon = new SqlConnection(con))
            {
                string query = @"USE HCS
                                        SELECT INV_Num,
                                        INV_Date,
                                        INV_Billing_Name, 
                                        SUM(INV_Price * INV_Qty) AS 'Sub_Total',
                                        SUM(INV_Tax) AS 'Tax', 
                                        ROUND(SUM(INV_Total),2) AS 'Total', 
                                        ROUND(SUM(INV_Cost),2) AS 'Cost',  
                                        ROUND(SUM(INV_TaxPaid),2) AS 'Tax_Paid',
                                        ROUND(SUM(INV_GrossProfit),2) AS 'Gross_Profit',
                                        INV_Paid
                                        FROM INVOICE
                                        GROUP BY INV_NUM, INV_Billing_Name, INV_Date, INV_Paid
                                        ORDER BY INV_NUM";

                SqlCommand cmd = new SqlCommand(query, myCon);
                myCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                   
                    while (reader.Read())
                    {
                        Invoice inv = new Invoice();
                        inv.InvNumber = (int)reader["INV_Num"];
                        inv.Date = (DateTime)reader["INV_Date"];
                        inv.Billing_Name = reader["INV_Billing_Name"].ToString();
                        inv.Price = (decimal)reader["Sub_Total"];
                        inv.Tax = (decimal)reader["Tax"];
                        inv.Total = (decimal)reader["Total"];
                        inv.Cost = (decimal)reader["Cost"];
                        inv.TaxPaid = (decimal)reader["Tax_Paid"];
                        inv.InvProfit = (decimal)reader["Gross_Profit"];
                        if((bool)reader["Inv_Paid"])
                        {
                            inv.Paid = "Paid";
                        }
                        else
                        {
                            inv.Paid = "Owing";
                        }
                        
                        invs.Add(inv);
                    }
                }
            }

            foreach(Invoice inv in invs)
            {
                this.InvoicesInvsDG.Rows.Add(inv.InvNumber, inv.Date, inv.Billing_Name, Math.Round(inv.Price,2), Math.Round(inv.Tax,2), Math.Round(inv.Total, 2), Math.Round(inv.Cost, 2), Math.Round(inv.TaxPaid, 2), Math.Round(inv.InvProfit, 2), inv.Paid);
            }

            foreach(DataGridViewRow row in InvoicesInvsDG.Rows)
            {
                if(row.Cells[9].Value.ToString() != "Paid")
                {
                    row.Cells["Inv_Total"].Style.ForeColor = System.Drawing.Color.Red;
                    row.Cells["Inv_Paid"].Style.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    row.Cells["Inv_Total"].Style.ForeColor = System.Drawing.Color.Black;
                    row.Cells["Inv_Paid"].Style.ForeColor = System.Drawing.Color.Green;
                }
            }

            this.InvoicesInvsDG.Sort(this.InvoicesInvsDG.Columns["INV_Num"], System.ComponentModel.ListSortDirection.Descending);
        }

        private void loadInvsClientCB()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                try
                {
                    string query = "SELECT BILLING_NAME FROM Client";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds, "Client");
                    invsClientCB.DataSource = ds.Tables["Client"];
                    invsClientCB.DisplayMember = "Billing_Name";
                    invsClientCB.ValueMember = "Billing_Name";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured: " + ex.ToString());
                }
                invsClientCB.SelectedIndex = -1;
            }
        }

        private void loadInvsClientEditCB()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                try
                {
                    string query = "SELECT BILLING_NAME FROM Client";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds, "Client");
                    invsBillNameCB.DataSource = ds.Tables["Client"];
                    invsBillNameCB.DisplayMember = "Billing_Name";
                    invsBillNameCB.ValueMember = "Billing_Name";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured: " + ex.ToString());
                }
                //invsClientCB.SelectedIndex = -1;
            }
        }

        private void invsRefreshBut_Click(object sender, EventArgs e)
        {
            loadInvoices();
            invsClientCB.SelectedIndex = -1;
        }

        private void InvoicesInvsDG_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Checks if one row is highlighted, if so the items in that invoice are shown.
            if (InvoicesInvsDG.SelectedRows.Count > 0 && InvoicesInvsDG.SelectedRows.Count < 2)
            {
                if (invoiceItemsDG.Rows.Count > 0)
                {
                    invoiceItemsDG.Rows.Clear();
                }
                int row = InvoicesInvsDG.CurrentRow.Index;
                //  This is a hack
                //  Can't figure out why the Datagrid current row gets changed here when updating an invoice...
                if(invsDGRow > -1)
                {
                    row = invsDGRow;
                }
                Invoice inv = new Invoice();
                List<Invoice> invItems = new List<Invoice>();
                int invNum = Convert.ToInt32(InvoicesInvsDG.CurrentRow.Cells[0].Value.ToString());
                if(invsDGRow > -1)
                {
                    invNum = Convert.ToInt32(InvoicesInvsDG.Rows[invsDGRow].Cells[0].Value.ToString());
                }

                invItems = inv.GetInvoicebyInvNum(invNum);
                foreach (Invoice invItem in invItems)
                {

                    this.invoiceItemsDG.Rows.Add(invItem.ID, invItem.ClientID, invItem.InvNumber, invItem.Date, invItem.Billing_Name, 
                        invItem.Qty, invItem.Details, Math.Round(invItem.Price, 2), Math.Round(invItem.Tax, 2), Math.Round(invItem.Total, 2),
                        invItem.Notes, Math.Round(invItem.Cost, 2), Math.Round(invItem.TaxPaid, 2), Math.Round(invItem.InvProfit, 2), invItem.Paid);
                }

                foreach (DataGridViewRow rw in invoiceItemsDG.Rows)
                {
                    if (rw.Cells[14].Value.ToString() != "Paid")
                    {
                        rw.Cells["Inv_Pd"].Style.ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        rw.Cells["Inv_Pd"].Style.ForeColor = System.Drawing.Color.Green;
                    }
                }

                invsDeleteBut.Enabled = true;
            }

            else
            {
                invsDeleteBut.Enabled = false;
                invoiceItemsDG.Rows.Clear();
            }
        }

        private void invsClientCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // check if no client or empty indexes are choosen
            if (invsClientCB.SelectedIndex > 0)
            {
                loadInvoicesByClient(this.invsClientCB.GetItemText(this.invsClientCB.SelectedItem));
                invoiceItemsDG.Rows.Clear();
            }
            else
            {
                loadInvoices();
                invoiceItemsDG.Rows.Clear();
            }
        }

        private void loadInvoicesByClient(string cl)
        {
            if (InvoicesInvsDG.Rows.Count > 0)
            {
                InvoicesInvsDG.Rows.Clear();
            }

            if (invoiceItemsDG.Rows.Count > 0)
            {
                invoiceItemsDG.Rows.Clear();
            }

            var con = ConfigurationManager.ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ToString();
            //  Build list of Invoices
            List<Invoice> invs = new List<Invoice>();
            using (SqlConnection myCon = new SqlConnection(con))
            {
                string query = "USE HCS " +
                                    "SELECT INV_Num, " +
                                    "INV_Date, " +
                                    "INV_Billing_Name, " +
                                    "SUM(INV_Price * INV_Qty) AS 'Sub_Total', " +
                                    "SUM(INV_Tax) AS 'Tax', " +
                                    "ROUND(SUM(INV_Total),2) AS 'Total', " +
                                    "ROUND(SUM(INV_Cost),2) AS 'Cost', " +
                                    "ROUND(SUM(INV_TaxPaid),2) AS 'Tax_Paid', " +
                                    "ROUND(SUM(INV_GrossProfit), 2) AS 'Gross_Profit', " +
                                    "INV_Paid " +
                                    "FROM INVOICE " +
                                    "WHERE INV_Billing_Name = '" + cl + "' " +
                                    "GROUP BY INV_NUM, INV_Billing_Name, INV_Date, INV_Paid " +
                                    "ORDER BY INV_NUM";

                SqlCommand cmd = new SqlCommand(query, myCon);
                myCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        Invoice inv = new Invoice();
                        inv.InvNumber = (int)reader["INV_Num"];
                        inv.Date = (DateTime)reader["INV_Date"];
                        inv.Billing_Name = reader["INV_Billing_Name"].ToString();
                        inv.Price = (decimal)reader["Sub_Total"];
                        inv.Tax = (decimal)reader["Tax"];
                        inv.Total = (decimal)reader["Total"];
                        inv.Cost = (decimal)reader["Cost"];
                        inv.TaxPaid = (decimal)reader["Tax_Paid"];
                        inv.InvProfit = (decimal)reader["Gross_Profit"];
                        if ((bool)reader["Inv_Paid"])
                        {
                            inv.Paid = "Paid";
                        }
                        else
                        {
                            inv.Paid = "Owing";
                        }


                        invs.Add(inv);
                    }
                }
            }
            foreach (Invoice inv in invs)
            {
                this.InvoicesInvsDG.Rows.Add(inv.InvNumber, inv.Date, inv.Billing_Name, Math.Round(inv.Price, 2), Math.Round(inv.Tax, 2), Math.Round(inv.Total, 2), Math.Round(inv.Cost, 2), Math.Round(inv.TaxPaid, 2), Math.Round(inv.InvProfit, 2), inv.Paid);
            }

            foreach (DataGridViewRow row in InvoicesInvsDG.Rows)
            {
                if (row.Cells[9].Value.ToString() != "Paid")
                {
                    row.Cells["Inv_Total"].Style.ForeColor = System.Drawing.Color.Red;
                    row.Cells["Inv_Paid"].Style.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    row.Cells["Inv_Total"].Style.ForeColor = System.Drawing.Color.Black;
                    row.Cells["Inv_Paid"].Style.ForeColor = System.Drawing.Color.Green;
                }
            }

            this.InvoicesInvsDG.Sort(this.InvoicesInvsDG.Columns["INV_Num"], System.ComponentModel.ListSortDirection.Descending);


        }

        private void invoiceItemsDG_DoubleClick(object sender, EventArgs e)
        {
            invoicesupdaterow = InvoicesInvsDG.CurrentRow.Index;
            int invid = Convert.ToInt32(invoiceItemsDG.CurrentRow.Cells[0].Value.ToString());
            LoadInvoiceItemEdit(invid);
            enableInvEditFields(true);

            //  Set the invsDG row and column.
            //  Column is probably not needed here.
            invsDGRow = InvoicesInvsDG.CurrentRow.Index;
            invsDGColumn = InvoicesInvsDG.CurrentCell.ColumnIndex;
        }

        private void LoadInvoiceItemEdit(int id)
        {
            Invoice invItem = new Invoice();
            try
            {
                // Clear text boxes
                clearInvoiceItemData(false);
                // Get invoice data
                invItem = invItem.GetInvoiceItemByID(id);
                // Populate invoice properties
                loadInvsClientEditCB();
                invsInvNumTB.Text = invItem.InvNumber.ToString();
                invsDTPicker.Value = invItem.Date.Date;
                invsBillNameCB.Text = invItem.Billing_Name;
                invsQtyTB.Text = invItem.Qty.ToString();
                invsDetailsTB.Text = invItem.Details.ToString();
                invsSubTotalTB.Text = invItem.Price.ToString();
                invsNotesTB.Text = invItem.Notes;
                invsCostTB.Text = invItem.Cost.ToString();
                invsTaxPaidTB.Text = invItem.TaxPaid.ToString();
                // Set public object to the client we want to update
                workingInvoice = invItem;
                // Make the update button visible
                //updtClBut.Enabled = true;
                //addClBut.Enabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error - LoadClientEdit: " + e.ToString());
            }
        }

        private void clearInvoiceItemData(bool cbreset)
        {
            this.invsBillNameCB.DataSource = null;
            invsBillNameCB.Items.Clear();
            invsInvNumTB.Text = "";
            invsDTPicker.Value = DateTime.Today;
            if (cbreset)
            {
                invsBillNameCB.SelectedIndex = -1;
            }
            invsQtyTB.Text = "";
            invsDetailsTB.Text = "";
            invsSubTotalTB.Text = "";
            invsNotesTB.Text = "";
            invsCostTB.Text = "";
            invsTaxPaidTB.Text = "";

            enableInvEditFields(false);
        }

        private void invsClearEditItemsBut_Click(object sender, EventArgs e)
        {
            clearInvoiceItemData(true);
        }

        private void invsEditUpdateBut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to update this invoice item?", "Update Invoice?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Get Client Combo Box Position
                int clIndex = invsClientCB.SelectedIndex;

                //MessageBox.Show("Client Index#: " + clIndex);
                Invoice invEdit = new Invoice();
                invEdit = workingInvoice;
                string bn = this.invsBillNameCB.GetItemText(this.invsBillNameCB.SelectedItem);
                int invNum = int.Parse(this.invsInvNumTB.Text);
                DateTime dt = invsDTPicker.Value.Date;
                string date = dt.ToShortDateString();
                string dtls = invsDetailsTB.Text;
                //  Escape a single quote for SQL
                if(dtls.Contains("'"))
                {
                    dtls = dtls.Replace("'", "''");
                }
                //MessageBox.Show(dtls);
                string nts = invsNotesTB.Text;
                int qty = int.Parse(invsQtyTB.Text);
                decimal subT = decimal.Parse(invsSubTotalTB.Text);
                decimal tx = qty * subT * taxRate;
                decimal ttl = (qty * subT) + tx;
                decimal cst = decimal.Parse(invsCostTB.Text);
                decimal txpd = decimal.Parse(invsTaxPaidTB.Text);
                
                // Billing Name Check
                if (bn != invEdit.Billing_Name)
                {
                    if (MessageBox.Show("Change " + invEdit.Billing_Name + " to " + bn + " for all items in invoice " + invNum.ToString() + "?", "Change Client?",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        //Pull the new clients id
                        Client cl = new Client();
                        cl = cl.GetClientbyName(bn);
                        int clientID = cl.ID;
                        string query = "USE HCS UPDATE Invoice SET INV_Client_ID = " + clientID + ", INV_Billing_Name = '" + bn + "' WHERE INV_NUM = " + invNum;
                        try
                        {
                            executeQuery(query);
                            //MessageBox.Show(query);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.ToString());
                            return;
                        }
                        invEdit.ID = clientID;
                        invEdit.Billing_Name = bn;
                    }
                    else
                    {
                        // change billing name back
                        invsBillNameCB.SelectedIndex = invsBillNameCB.FindString(invEdit.Billing_Name);
                    }
                }
                // Date Check
                if (dt != invEdit.Date)
                {
                    string shortDate = invEdit.Date.ToShortDateString();
                    if (MessageBox.Show("Change " + shortDate + " to " + date + " for all items in invoice?", "Change Invoice Number?",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        string query = "USE HCS UPDATE Invoice SET INV_Date = '" + date + "' WHERE INV_NUM = " + invEdit.InvNumber;
                        try
                        {
                            executeQuery(query);
                            //MessageBox.Show(query);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.ToString());
                            return;
                        }
                        invEdit.Date = dt;
                    }
                    else
                    {
                        // Change the date back
                        invsDTPicker.Value = invEdit.Date;
                    }
                }
                // Invoice Check
                if (invNum != invEdit.InvNumber)
                {
                    // Get list of exiting invoice numbers
                    List<int> invNums = new List<int>();
                    string query = "USE HCS SELECT distinct INV_NUM FROM Invoice";
                    using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
                    {
                        conn.Open();
                        using (SqlCommand command = new SqlCommand(query, conn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    invNums.Add(reader.GetInt32(0));
                                }
                            }
                        }
                    }

                    if (invNums.Contains(invNum))
                    {
                        if (MessageBox.Show("This invoice number already exists. Do you want to add this item to invoice " + invNum.ToString(), "Change Invoice Number?",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            query = "USE HCS UPDATE Invoice SET INV_NUM = " + invNum + " WHERE INV_ID = " + invEdit.ID;
                            executeQuery(query);
                            //MessageBox.Show(query);
                        }
                        else
                        {
                            // Change invoice number back
                            invsInvNumTB.Text = invEdit.InvNumber.ToString();
                        }
                    }

                    else
                    {
                        if (MessageBox.Show("Change all invoice items from" + invEdit.InvNumber + " to " + invNum + "?", "Change Invoice Number?",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            query = "USE HCS UPDATE Invoice SET INV_NUM = " + invNum + " WHERE INV_NUM = " + invEdit.InvNumber;
                            try
                            {
                                executeQuery(query);
                                //MessageBox.Show(query);
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.ToString());
                                return;
                            }
                            invEdit.InvNumber = invNum;
                        }
                        else
                        {
                            // Change invoice number back
                            invsInvNumTB.Text = invEdit.InvNumber.ToString();
                        }
                    }
                }
                // Details Notes and Tax Paid Check
                if (dtls != invEdit.Details || nts != invEdit.Notes || txpd != invEdit.TaxPaid)
                {
                    string query = "USE HCS UPDATE Invoice SET INV_Details = '" + dtls + "'," +
                                                               "INV_Notes = '" + nts + "'," +
                                                               "INV_TaxPaid = " + txpd +
                                                               " WHERE INV_ID = " + invEdit.ID;
                    try
                    {
                        executeQuery(query);
                        //MessageBox.Show(query);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString());
                        return;
                    }
                }
                // Qty Price and Cost Check
                if(qty != invEdit.Qty || subT != invEdit.Price || cst != invEdit.Cost)
                {
                    string query = "USE HCS UPDATE Invoice SET INV_Qty = " + qty + "," +
                                                              "INV_Price = " + subT + "," +
                                                              "INV_Tax = " + tx + "," +
                                                              "INV_Total = " + ttl + "," +
                                                              "INV_Cost =" + cst +
                                                              " WHERE INV_ID = " + invEdit.ID;
                    try
                    {
                        executeQuery(query);
                        executeProfit_SP();
                        //MessageBox.Show(query);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString());
                        return;
                    }
                }

                //  Refresh invoice data grid
                if (clIndex == -1)
                {
                    loadInvoices();
                }
                else
                {
                    loadInvoicesByClient(bn);
                }
                // Clear Editable fields
                clearInvoiceItemData(false);

                InvoicesInvsDG.Rows[invsDGRow].Selected = true;
                InvoicesInvsDG_CellClick(this.InvoicesInvsDG, new DataGridViewCellEventArgs(invsDGColumn, invsDGRow));
                InvoicesInvsDG.CurrentCell = InvoicesInvsDG.Rows[invsDGRow].Cells[0];
                InvoicesInvsDG.SelectedRows.Equals(invsDGRow);
                // Reset these checks 
                invsDGRow = -1;                
                invsDGColumn = -1;
                workingInvoice = null;
            }
        }

        private void invsSubTotalTB_TextChanged(object sender, EventArgs e)
        {
            if (!invoiceEditTaxWarning)
            {
                MessageBox.Show("Be sure to check the tax rate before updating Quantity or Price fields.");
                invoiceEditTaxWarning = true;
            }
        }

        private void invsQtyTB_TextChanged(object sender, EventArgs e)
        {
            if (!invoiceEditTaxWarning)
            {
                MessageBox.Show("Be sure to check the tax rate before updating Quantity or Price fields.");
                invoiceEditTaxWarning = true;
            }
        }

        private void enableInvEditFields(bool e)
        {
            bilNameLbl.Enabled = e;
            invsBillNameCB.Enabled = e;
            dtlsLbl.Enabled = e;
            invsDetailsTB.Enabled = e;
            notesLbl.Enabled = e;
            invsNotesTB.Enabled = e;
            invsDTPicker.Enabled = e;
            QtyIPLbl.Enabled = e;
            invsQtyTB.Enabled = e;
            subTotLbl.Enabled = e;
            invsSubTotalTB.Enabled = e;
            invnumLbl.Enabled = e;
            invsInvNumTB.Enabled = e;
            costLbl.Enabled = e;
            invsCostTB.Enabled = e;
            taxpaidLbl.Enabled = e;
            invsTaxPaidTB.Enabled = e;
            invsClearEditItemsBut.Enabled = e;
            invsEditUpdateBut.Enabled = e;
            invsDeleteBut.Enabled = e;
        }

        private void executeProfit_SP()
        {
            using (var conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            using (var command = new SqlCommand("Profit_Calc", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                conn.Open();
                command.ExecuteNonQuery();
            }
        }

        private void setPaidChooserMenu()
        {
            invoice_Paid_Chooser.Items.Add("Paid").Name = "Paid";
            invoice_Paid_Chooser.Items.Add("-");
            invoice_Paid_Chooser.Items.Add("Done").Name = "Done";
            invoice_Paid_Chooser.ItemClicked += new ToolStripItemClickedEventHandler(invoice_Paid_Chooser_ItemClicked);
            invoice_Paid_Chooser.AutoClose = false;
        }

        private void InvoicesInvsDG_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Get the datagridview row that was clicked
                DataGridViewCell clickedCell = (sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex];
                InvoicesInvsDG.CurrentCell = clickedCell;
                workingInoviceRow = clickedCell.RowIndex;
                workingInvoiceNumber = Convert.ToInt32(InvoicesInvsDG.Rows[workingInoviceRow].Cells[0].Value);
                workingInvoiceClient = InvoicesInvsDG.Rows[workingInoviceRow].Cells[2].Value.ToString();

                if (InvoicesInvsDG.Rows[workingInoviceRow].Cells[9].Value.ToString() == "Paid")
                {
                    ((ToolStripMenuItem)invoice_Paid_Chooser.Items[0]).Checked = true;
                }
                else
                {
                    ((ToolStripMenuItem)invoice_Paid_Chooser.Items[0]).Checked = false;
                }
                invoice_Paid_Chooser.Show(this, InvoicesInvsDG.PointToClient(Cursor.Position));
            }
        }

        private void invoice_Paid_Chooser_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name.ToString())
            {
                case "Paid":
                    if (workingInvoiceNumber > -1)
                    {
                        int cPaid = 1;
                        if (((ToolStripMenuItem)invoice_Paid_Chooser.Items[0]).Checked)
                        {
                            cPaid = 0;
                            ((ToolStripMenuItem)invoice_Paid_Chooser.Items[0]).Checked = false;
                        }
                        else
                        {
                            ((ToolStripMenuItem)invoice_Paid_Chooser.Items[0]).Checked = true;
                        }

                        string query = "USE HCS UPDATE INVOICE SET INV_Paid = " + cPaid + " WHERE INV_NUM = " + workingInvoiceNumber;
                        executeQuery(query);
                        // Get Client Combo Box Position
                        int clIndex = invsClientCB.SelectedIndex;

                        if (clIndex == -1)
                        {
                            loadInvoices();
                        }
                        else
                        {
                            loadInvoicesByClient(workingInvoiceClient);
                        }
                        InvoicesInvsDG.FirstDisplayedScrollingRowIndex = workingInoviceRow;
                        InvoicesInvsDG.CurrentCell = InvoicesInvsDG.Rows[workingInoviceRow].Cells[9];
                        workingInvoiceNumber = -1;
                        workingInoviceRow = -1;
                        workingInvoiceClient = "";
                    }

                    invoice_Paid_Chooser.Close();
                    break;

                case "Done":
                    invoice_Paid_Chooser.Close();
                    break;
            }
        }

        private void invsDeleteBut_Click(object sender, EventArgs e)
        {
            int selectedrowindex = InvoicesInvsDG.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = InvoicesInvsDG.Rows[selectedrowindex];
            string invNum = Convert.ToString(selectedRow.Cells["Inv_Num"].Value);

            if (MessageBox.Show("Delete invoice " + invNum + " ?", "Delete Invoice",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string query = "USE HCS DELETE FROM Invoice WHERE INV_NUM = " + invNum;
                try
                {
                    executeQuery(query);
                    if (invsClientCB.SelectedIndex > -1)
                    {
                        loadInvoicesByClient(this.invsClientCB.GetItemText(this.invsClientCB.SelectedItem));
                        invoiceItemsDG.Rows.Clear();
                    }
                    else
                    {
                        loadInvoices();
                        invoiceItemsDG.Rows.Clear();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting invoice number: " + invNum +
                        Environment.NewLine + "Details: " +
                        Environment.NewLine + ex);
                }
            }
        }

        #endregion

        #region        <<<<<<<<<<<<<<<<<<<----------  Admin Tab  ---------->>>>>>>>>>>>>>>>>>>

        private void loadAdminCatDrpDwn()
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                try
                {
                    string query = "SELECT DISTINCT GS_Category FROM GoodsServices WHERE GS_Active = 1";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds, "Category");
                    adminCategoryCmbBx.DataSource = ds.Tables["Category"];
                    adminCategoryCmbBx.DisplayMember = "GS_Category";
                    adminCategoryCmbBx.ValueMember = "GS_Category";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured: " + ex.ToString());
                }
                adminCategoryCmbBx.SelectedIndex = -1;
            }
        }

        public void loadActiveInvItemDrpDwn()
        {
            this.actInvItemCmBx.SelectedIndexChanged -= new System.EventHandler(this.actInvItemCmBx_SelectedIndexChanged);
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                try
                {
                    string query = "SELECT GS_Details FROM GoodsServices";
                    SqlDataAdapter da2 = new SqlDataAdapter(query, conn);
                    DataSet ds2 = new DataSet();
                    da2.Fill(ds2, "GoodsServices");
                    actInvItemCmBx.DataSource = ds2.Tables["GoodsServices"];
                    actInvItemCmBx.DisplayMember = "GS_Details";
                    actInvItemCmBx.ValueMember = "GS_Details";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured: " + ex.ToString());
                }
                this.actInvItemCmBx.SelectedIndexChanged += new System.EventHandler(this.actInvItemCmBx_SelectedIndexChanged);
                actInvItemCmBx.SelectedIndex = -1;
            }
        }

        private void adminItemTxtBx_TextChanged(object sender, EventArgs e)
        {
            verifyGS_Cat_Item();
        }

        private void adminCategoryCmbBx_SelectedIndexChanged(object sender, EventArgs e)
        {
            verifyGS_Cat_Item();
            // Turn off event handler so it doens't fire, and then back on.
            this.adminCategoryTxtBx.TextChanged -= new System.EventHandler(this.adminCategoryTxtBx_TextChanged);
            adminCategoryTxtBx.Text = "";
            this.adminCategoryTxtBx.TextChanged += new System.EventHandler(this.adminCategoryTxtBx_TextChanged);
        }

        private void adminCategoryTxtBx_TextChanged(object sender, EventArgs e)
        {
            verifyGS_Cat_Item();
            // Turn off event handler so it doens't fire, and then back on.
            this.adminCategoryCmbBx.SelectedIndexChanged -= new System.EventHandler(this.adminCategoryCmbBx_SelectedIndexChanged);
            adminCategoryCmbBx.SelectedIndex = -1;
            this.adminCategoryCmbBx.SelectedIndexChanged += new System.EventHandler(this.adminCategoryCmbBx_SelectedIndexChanged);
        }

        private void adminPriceTxtBx_TextChanged(object sender, EventArgs e)
        {
            verifyGS_Cat_Item();
        }
        //  Only allows numbers and 2 decimal places to be entered.
        private void adminPriceTxtBx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            if (Regex.IsMatch(adminPriceTxtBx.Text, @"\.\d\d") && e.KeyChar != 8)
            {
                e.Handled = true;
            }

        }

        private void adminAddHSBut_Click(object sender, EventArgs e)
        {
            string name = adminItemTxtBx.Text;
            //  Use replace if a single quote exists, this can cause issues when creating the query otherwise.
            name = name.Replace("'", "''");
            string category = "";
            decimal prc = decimal.Parse(adminPriceTxtBx.Text);

            if (adminCategoryCmbBx.SelectedIndex == -1)
            {
                category = adminCategoryTxtBx.Text;
            }

            else
            {
                category = adminCategoryCmbBx.Text;
            }

            if (MessageBox.Show("Do you want to add this Item?" +
                Environment.NewLine + "Item: " + name +
                Environment.NewLine + "Category: " + category +
                Environment.NewLine + "Price: " + adminPriceTxtBx.Text
                , "Add Item",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    string query = "INSERT INTO GoodsServices (GS_Category,GS_Details, GS_Price, GS_Active)";
                    query += " VALUES ('" + category + "', '" + name + "', " + prc + ", " + 1 + ")";
                    //MessageBox.Show("Query = " + query);
                    executeQuery(query);
                }

                catch (Exception er)
                {
                    MessageBox.Show("adminAddHSBut_Click Failed: " + er.ToString());
                }

                loadInvDetails();
                loadAdminCatDrpDwn();
                loadActiveInvItemDrpDwn();
                adminPriceTxtBx.Text = "";
                adminItemTxtBx.Text = "";
                adminCategoryCmbBx.SelectedIndex = -1;
                adminCategoryTxtBx.Text = "";
            }

        }

        private void activateComItBut_Click(object sender, EventArgs e)
        {
            string details = actInvItemCmBx.Text;
            details = details.Replace("'", "''");
            if (MessageBox.Show("Do you want to activate item: " + details + "?", "Activate Item",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string query = "UPDATE GoodsServices SET GS_Active = 1 WHERE GS_Details = '" + details + "'";
                executeQuery(query);
            }
            loadInvDetails();
            loadActiveInvItemDrpDwn();
            activateComItBut.Enabled = false;
            adminActDelComItBut.Enabled = false;
        }

        private void deactivateComItBut_Click(object sender, EventArgs e)
        {
            string details = actInvItemCmBx.Text;
            details = details.Replace("'", "''");
            if (MessageBox.Show("Do you want to deactivate item: " + details + "?", "Activate Item",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string query = "UPDATE GoodsServices SET GS_Active = 0 WHERE GS_Details = '" + details + "'";
                executeQuery(query);
            }
            loadInvDetails();
            loadActiveInvItemDrpDwn();
            deactivateComItBut.Enabled = false;
            adminActDelComItBut.Enabled = false;
        }

        private void adminDelComItBut_Click(object sender, EventArgs e)
        {
            string details = actInvItemCmBx.Text;
            details = details.Replace("'", "''");
            if (MessageBox.Show("Do you want to DELETE item: " + details + "?", "DELETE Item",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string query = "DELETE FROM GoodsServices WHERE GS_Details = '" + details + "'";
                executeQuery(query);
            }
            loadInvDetails();
            loadAdminCatDrpDwn();
            loadActiveInvItemDrpDwn();
            activateComItBut.Enabled = false;
            deactivateComItBut.Enabled = false;
            adminActDelComItBut.Enabled = false;
        }

        private void actInvItemCmBx_SelectedIndexChanged(object sender, EventArgs e)
        {
            string details = actInvItemCmBx.Text;
            //details = details.Replace("'", "''");
            GoodsServices GS = new GoodsServices();
            GS.Active = GS.getIsActiveByDetail(details);
            if (GS.Active)
            {
                deactivateComItBut.Enabled = true;
                activateComItBut.Enabled = false;
            }
            else
            {
                activateComItBut.Enabled = true;
                deactivateComItBut.Enabled = false;
            }

            adminActDelComItBut.Enabled = true;
            
        }

        public void verifyGS_Cat_Item()
        {
            bool Name = false;
            bool Category = false;
            bool Price = false;
            decimal Prc = 0.00m;
            string textPrice = adminPriceTxtBx.Text.ToString();

            if (adminItemTxtBx.Text.Length > 1)
            {
                Name = true;
            }

            if (adminCategoryCmbBx.SelectedIndex != -1 || adminCategoryTxtBx.Text.Length > 0)
            {
                Category = true;
            }

            if (textPrice.Length > 0 && textPrice != ".")
            {
                Prc = decimal.Parse(adminPriceTxtBx.Text.ToString());
                if (Prc >= 0.00m)
                {
                    Price = true;
                }
            }

            if (Name && Category && Price)
            {
                adminAddHSBut.Enabled = true;
            }
            else
            {
                adminAddHSBut.Enabled = false;
            }

        }

        private void effectiveTaxRateTB_TextChanged(object sender, EventArgs e)
        {
            if(effectiveTaxRateTB.Text.Contains("0.0"))
            {
                if(effectiveTaxRateTB.Text.Length >3)
                {
                    setEffTxRateBut.Enabled = true;
                }
                else
                {
                    setEffTxRateBut.Enabled = false;
                }
            }
        }

        private void effectiveTaxRateTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && (e.KeyChar != 8 && e.KeyChar != 46))
            {
                e.Handled = true;
            }
        }

        private void resetDefaultTaxRate_Click(object sender, EventArgs e)
        {
            taxRate = Convert.ToDecimal(defaultTaxRateTB.Text);
            effectiveTaxRateTB.Text = taxRate.ToString();
            setEffTxRateBut.Enabled = false;
        }

        private void setEffTxRateBut_Click(object sender, EventArgs e)
        {
            taxRate = Convert.ToDecimal(effectiveTaxRateTB.Text);
            MessageBox.Show("The effective tax rate is: " + taxRate.ToString());
            setEffTxRateBut.Enabled = false;
        }
        
        #endregion
        
    }

}
