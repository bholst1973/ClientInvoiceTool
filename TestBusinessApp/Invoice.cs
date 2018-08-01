using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBusinessApp
{
    class Invoice
    {
        public int ID { get; set; }
        public int ClientID { get; set; }
        public int InvNumber { get; set; }
        public DateTime Date { get; set; }
        public string Billing_Name { get; set; }
        public int Qty { get; set; }
        public string Details { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; }
        public string Paid { get; set; }
        public decimal Cost { get; set; }
        public decimal TaxPaid { get; set; }

        public List<Invoice> GetInvoicebyInvNum(int invn)
        {
            var con = ConfigurationManager.ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ToString();
            using (SqlConnection myCon = new SqlConnection(con))
            {
                List<Invoice> Invoices = new List<Invoice>();
                
                string query = "USE HCS SELECT * FROM Invoice WHERE INV_NUM = @invNum";
                SqlCommand cmd = new SqlCommand(query, myCon);
                cmd.Parameters.AddWithValue("@invNum", invn.ToString());
                myCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Invoice invs = new Invoice();
                        invs.ID = (int)reader["INV_ID"];
                        invs.ClientID = (int)reader["INV_Client_ID"];
                        invs.InvNumber = (int)reader["INV_NUM"];
                        invs.Date = reader.GetDateTime(3);
                        invs.Billing_Name = reader["INV_Billing_Name"].ToString();
                        invs.Qty = (int)reader["INV_Qty"];
                        invs.Details = reader["INV_Details"].ToString();
                        invs.Price = (decimal)reader["INV_Price"];
                        invs.Tax = (decimal)reader["INV_Tax"];
                        invs.Total = (decimal)reader["INV_Total"];
                        invs.Notes = reader["INV_Notes"].ToString();
                        if ((bool)reader["Inv_Paid"])
                        {
                            invs.Paid = "Paid";
                        }
                        else
                        {
                            invs.Paid = "Owing";
                        }
                        invs.Cost = (decimal)reader["INV_Cost"];
                        invs.TaxPaid = (decimal)reader["INV_TaxPaid"];
                        Invoices.Add(invs);
                    }
                }
                return Invoices;
            }
        }

        public Invoice GetInvoiceItemByID(int id)
        {
            var con = ConfigurationManager.ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ToString();
            Invoice invoiceItemByID = new Invoice();
            using (SqlConnection myCon = new SqlConnection(con))
            {
                string query = "USE HCS SELECT * FROM Invoice WHERE INV_ID = @InvoiceID";
                SqlCommand cmd = new SqlCommand(query, myCon);
                cmd.Parameters.AddWithValue("@InvoiceID", id);
                myCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        invoiceItemByID.ID = (int)reader["INV_ID"];
                        invoiceItemByID.ClientID = (int)reader["INV_Client_ID"];
                        invoiceItemByID.InvNumber = (int)reader["INV_NUM"];
                        invoiceItemByID.Date = (DateTime)reader["INV_Date"];
                        invoiceItemByID.Billing_Name = reader["INV_Billing_Name"].ToString();
                        invoiceItemByID.Qty = (int)reader["INV_Qty"];
                        invoiceItemByID.Details = reader["INV_Details"].ToString();
                        invoiceItemByID.Price = (decimal)reader["INV_Price"];
                        invoiceItemByID.Tax = (decimal)reader["INV_Tax"];
                        invoiceItemByID.Total = (decimal)reader["INV_Total"];
                        invoiceItemByID.Notes = reader["INV_Notes"].ToString();
                        invoiceItemByID.Paid = reader["INV_Paid"].ToString();
                        invoiceItemByID.Cost = (decimal)reader["INV_Cost"];
                        invoiceItemByID.TaxPaid = (decimal)reader["INV_TaxPaid"];
                    }
                }
            }
            return invoiceItemByID;
        }


    }
}
