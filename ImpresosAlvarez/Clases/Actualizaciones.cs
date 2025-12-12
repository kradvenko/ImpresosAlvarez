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
        public static void Actualizacion2()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImpresosBDConn"].ConnectionString))
                {
                    using (SqlCommand comm = new SqlCommand(" " +
                        "SELECT solicita " +
                        "FROM Notas " +
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
                            "ALTER TABLE Notas " +
                            "ADD solicita NVARCHAR(50) NULL DEFAULT '-' WITH VALUES " +
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
        public static void Actualizacion3()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImpresosBDConn"].ConnectionString))
                {
                    using (SqlCommand comm = new SqlCommand(" " +
                        "SELECT lona_medida " +
                        "FROM Ordenes " +
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
                            "ALTER TABLE Ordenes " +
                            "ADD tipo_orden NVARCHAR(30) NULL DEFAULT 'NORMAL' WITH VALUES, " +
                            "lona_medida NVARCHAR(30) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_normal NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_traslucida NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_impresion_uv NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_impresion_ecosolvente NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_acabado_byo NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_acabado_bastilla NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_acabado_sobrante NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_acabado_bolsa NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_acabado_otro NVARCHAR(20) NULL DEFAULT ' ' WITH VALUES, " +
                            "lona_acabado_observaciones NVARCHAR(100) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_medida NVARCHAR(30) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_tipo_brillante NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_tipo_mate NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_impresion_uv NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_impresion_ecosolvente NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_econo NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_ecogris NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +                            
                            "vinil_alta NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_micro NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_trans NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_est_blanco NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_est_trans NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_reflejante NVARCHAR(5) NULL DEFAULT ' ' WITH VALUES, " +
                            "vinil_observaciones NVARCHAR(100) NULL DEFAULT ' ' WITH VALUES, " +
                            "otro_material NVARCHAR(50) NULL DEFAULT ' ' WITH VALUES, " +
                            "envio_a NVARCHAR(20) NULL DEFAULT ' ' WITH VALUES " +
                            " " +
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
        public static void Actualizacion4()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImpresosBDConn"].ConnectionString))
                {
                    using (SqlCommand comm = new SqlCommand(" " +
                        "SELECT id_orden " +
                        "FROM DetalleFactura " +
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
                            "ALTER TABLE DetalleFactura " +
                            "ADD id_orden int NULL DEFAULT 0 WITH VALUES " +
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
