using ImpresosAlvarez.Clases;
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
    /// Lógica de interacción para CorteDiarioDetalle.xaml
    /// </summary>
    public partial class CorteDiarioDetalle : Window
    {
        FacturaViewModel Pago;
        string Tipo;
        CorteDiario ParentWindow;
        List<Usuarios> ListaEntrega;
        string Modo;
        public CorteDiarioDetalle(FacturaViewModel Pago, string Tipo, CorteDiario ParentWindow, string modo)
        {
            InitializeComponent();
            this.Pago = Pago;
            this.Tipo = Tipo;
            this.ParentWindow = ParentWindow;
            Modo = modo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListaEntrega = new List<Usuarios>();
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaEntrega = dbContext.Usuarios.Where(x => x.estado == "ACTIVO").ToList();
            }
            cbEntrega.ItemsSource = ListaEntrega; 
            cbEntrega.DisplayMemberPath = "nombre";
            cbEntrega.SelectedValuePath = "id_usuario";
            cbEntrega.SelectedIndex = 0;
            if (Modo == "EDITAR")
            {
                cbEntrega.SelectedValue = Pago.id_entrega;
                cbReferencia.Text = Pago.referencia;
                txtObservaciones.Text = Pago.observaciones;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Pago.entrego = cbEntrega.Text;
            Pago.id_entrega = Convert.ToInt32(cbEntrega.SelectedValue);
            Pago.referencia = cbReferencia.Text;
            Pago.observaciones = txtObservaciones.Text;

            if (Tipo == "Factura")
            {
                ParentWindow.ActualizarFactura(Pago);
            }
            else if (Tipo == "Cotizacion")
            {
                ParentWindow.ActualizarNota(Pago);
            }

            this.Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
