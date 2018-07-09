using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBusinessApp
{
    class GoodsServices
    {
        public int ID { get; set; }
        public string Category { get; set; }
        public string Details { get; set; }
        public decimal Price { get; set; }
        public bool Active { get; set; }

        public decimal getGSPriceByDetail(string dtl)
        {
            decimal price = 0.00m;
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                string query = @"SELECT GS_Price FROM GoodsServices WHERE GS_Details = @detail";
                using(SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = dtl;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            price = (decimal)reader[0];
                    }
                }
            }

            return price;
        }

        public int getIDByDetail(string dtl)
        {
            int ID = -1;
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                string query = @"SELECT GS_ID From GoodsServices WHERE GS_Details = @detail";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = dtl;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            ID = (int)reader[0];
                    }
                }
            }

            return ID;
        }

        public bool getIsActiveByDetail(string dtl)
        {
            bool Active = false;
            using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HCS;Integrated Security=True"))
            {
                string query = @"SELECT GS_Active From GoodsServices WHERE GS_Details = @detail";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@detail", SqlDbType.VarChar).Value = dtl;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            Active = (bool)reader[0];
                    }
                }
            }

            return Active;
        }
    }


}
