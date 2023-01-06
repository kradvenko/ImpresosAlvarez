using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpresosAlvarez.Clases
{
    public class Actualizaciones
    {
        public static void Actualizacion1()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImpresosBDConn"].ConnectionString))
                {
                    using (SqlCommand comm = new SqlCommand(" " +
                        "SELECT aplica_retencion " +
                        "FROM Clientes " +
                        "", con))
                    {
                        con.Open();

                        SqlDataReader reader = comm.ExecuteReader();
                        con.Close();
                    }
                }
            }
            catch
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImpresosBDConn"].ConnectionString))
                    {
                        using (SqlCommand comm = new SqlCommand(" " +
                            "ALTER TABLE Clientes " +
                            "ADD aplica_retencion NVARCHAR(3) NULL DEFAULT 'NO' WITH VALUES " +
                            "", con))
                        {
                            con.Open();

                            comm.ExecuteNonQuery();

                            con.Close();
                        }

                        using (SqlCommand comm = new SqlCommand(" " +
                            "UPDATE Clientes " +
                            "SET aplica_retencion = 'NO'" +
                            "", con))
                        {
                            con.Open();

                            comm.ExecuteNonQuery();

                            con.Close();
                        }
                    }
                }
                catch (Exception exc)
                {

                }
            }
        }
    }
}
