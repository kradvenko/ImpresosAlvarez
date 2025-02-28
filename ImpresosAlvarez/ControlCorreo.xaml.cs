using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImpresosAlvarez
{
    /// <summary>
    /// Lógica de interacción para ControlCorreo.xaml
    /// </summary>
    public partial class ControlCorreo : Window
    {
        Correos CorreoElegido;
        String Modo;
        CorreosCliente ParentCorreos;
        int IdCliente;
        public ControlCorreo(Correos CorreoElegido, String modo, CorreosCliente parentCorreos, int IdCliente)
        {
            InitializeComponent();
            this.CorreoElegido = CorreoElegido;
            Modo = modo;
            ParentCorreos = parentCorreos;
            this.IdCliente = IdCliente;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CorreoElegido != null)
            {
                tbCorreo.Text = CorreoElegido.correo;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (Modo == "NUEVO")
            {
                if (tbCorreo.Text != "")
                {
                    try
                    {
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            Correos Correo = new Correos();

                            Correo.id_cliente = IdCliente;
                            Correo.correo = tbCorreo.Text;

                            dbContext.Correos.Add(Correo);
                            dbContext.SaveChanges();

                            ParentCorreos.ActualizarListaCorreos();

                            MessageBox.Show("Se ha guardado el correo.");
                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else if (Modo == "MODIFICAR")
            {
                try
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        Correos Correo = dbContext.Correos.Where(T => T.id_correo == CorreoElegido.id_correo).First();
                        
                        Correo.correo = tbCorreo.Text;

                        dbContext.SaveChanges();

                        ParentCorreos.ActualizarListaCorreos();

                        MessageBox.Show("Se ha modificado el correo.");
                        this.Close();
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
