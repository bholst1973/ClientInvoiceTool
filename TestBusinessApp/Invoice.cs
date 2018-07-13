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


    }
}
