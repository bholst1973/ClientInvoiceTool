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
        public bool Paid { get; set; }
        public decimal Cost { get; set; }
        public decimal TaxPaid { get; set; }


        public List<Invoice> getInvoiceTotals()
        {
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
                                    INV_Paid

                                    FROM INVOICE

                                    GROUP BY INV_NUM, INV_Billing_Name, INV_Date, INV_Paid
                                    ORDER BY INV_NUM";


                SqlCommand cmd = new SqlCommand(query, myCon);
                myCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Invoice inv = new Invoice();
                    while (reader.Read())
                    {

                        //INV_Num
                        //INV_Date
                        //INV_Billing_Name 
                        //Sub_Total
                        //Tax
                        //Total
                        //Cost 
                        //Tax_Paid
                        //INV_Paid

                        inv.InvNumber = (int)reader["INV_Num"];
                        inv.Date = (DateTime)reader["INV_Date"];
                        inv.Billing_Name = reader["INV_Billing_Name "].ToString();
                        inv.Price = (decimal)reader["Sub_Total"];
                        //clinetbyID.ID = (int)reader["Client_ID"];
                        //clinetbyID.First = reader["First_Name"].ToString();
                        //clinetbyID.Last = reader["Last_Name"].ToString();
                        //clinetbyID.Company = reader["Business_Name"].ToString();
                        //clinetbyID.Billing = reader["Billing_Name"].ToString();
                        //clinetbyID.Address1 = reader["Address1"].ToString();
                        //clinetbyID.Address2 = reader["Address2"].ToString();
                        //clinetbyID.Address3 = reader["Address3"].ToString();
                        //clinetbyID.City = reader["City"].ToString();
                        //clinetbyID.State = reader["State"].ToString();
                        //clinetbyID.Zip = reader["Zip"].ToString();
                        //clinetbyID.Phone = reader["Phone"].ToString();
                        //clinetbyID.Email = reader["Email"].ToString();
                    }
                }
            }

            return invs;
        }
    }


}
