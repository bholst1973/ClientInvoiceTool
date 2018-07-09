using System.Configuration;
using System.Data.SqlClient;

namespace TestBusinessApp
{
        class Client
    {
        public int ID { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string Company { get; set; }
        public string Billing { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        
        public Client GetClientbyID(int cl)
        {
            var con = ConfigurationManager.ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ToString();
            Client clinetbyID = new Client();
            using (SqlConnection myCon = new SqlConnection(con))
            {
                string query = "USE HCS SELECT * FROM Client WHERE Client_ID = @clientID";
                SqlCommand cmd = new SqlCommand(query, myCon);
                cmd.Parameters.AddWithValue("@clientID", cl.ToString());
                myCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clinetbyID.ID = (int)reader["Client_ID"];
                        clinetbyID.First = reader["First_Name"].ToString();
                        clinetbyID.Last = reader["Last_Name"].ToString();
                        clinetbyID.Company = reader["Business_Name"].ToString();
                        clinetbyID.Billing = reader["Billing_Name"].ToString();
                        clinetbyID.Address1 = reader["Address1"].ToString();
                        clinetbyID.Address2 = reader["Address2"].ToString();
                        clinetbyID.Address3 = reader["Address3"].ToString();
                        clinetbyID.City = reader["City"].ToString();
                        clinetbyID.State = reader["State"].ToString();
                        clinetbyID.Zip = reader["Zip"].ToString();
                        clinetbyID.Phone = reader["Phone"].ToString();
                        clinetbyID.Email = reader["Email"].ToString();
                    }
                }
            }
            return clinetbyID;
        }

        public Client GetClientbyName(string cl)
        {
            var con = ConfigurationManager.ConnectionStrings["TestBusinessApp.Properties.Settings.HCSConnectionString"].ToString();
            Client clinetbyName = new Client();
            using (SqlConnection myCon = new SqlConnection(con))
            {
                string query = "USE HCS SELECT * FROM Client WHERE Billing_Name = @clientName";
                SqlCommand cmd = new SqlCommand(query, myCon);
                cmd.Parameters.AddWithValue("@clientName", cl);
                myCon.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clinetbyName.ID = (int)reader["Client_ID"];
                        clinetbyName.First = reader["First_Name"].ToString();
                        clinetbyName.Last = reader["Last_Name"].ToString();
                        clinetbyName.Company = reader["Business_Name"].ToString();
                        clinetbyName.Billing = reader["Billing_Name"].ToString();
                        clinetbyName.Address1 = reader["Address1"].ToString();
                        clinetbyName.Address2 = reader["Address2"].ToString();
                        clinetbyName.Address3 = reader["Address3"].ToString();
                        clinetbyName.City = reader["City"].ToString();
                        clinetbyName.State = reader["State"].ToString();
                        clinetbyName.Zip = reader["Zip"].ToString();
                        clinetbyName.Phone = reader["Phone"].ToString();
                        clinetbyName.Email = reader["Email"].ToString();
                    }
                }
            }
            return clinetbyName;
        }
    }
    
}
