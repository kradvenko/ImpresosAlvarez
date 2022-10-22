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
    /// Lógica de interacción para Llenado4.xaml
    /// </summary>
    public partial class Llenado4 : Window
    {
        FacturaDigital ParentForm;
        Clientes clienteElegido;
        public Llenado4(FacturaDigital ParentForm, Clientes clienteElegido)
        {
            InitializeComponent();
            this.ParentForm = ParentForm;
            this.clienteElegido = clienteElegido;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbNombreConstancia.Text = clienteElegido.nombre_constancia;
            cbRegimenFiscal.Text = clienteElegido.regimen_fiscal;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {            
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                Clientes c = dbContext.Clientes.Where(T => T.id_cliente == clienteElegido.id_cliente).First();
                c.nombre_constancia = tbNombreConstancia.Text;
                c.regimen_fiscal = cbRegimenFiscal.Text.Substring(0, 3);
                dbContext.SaveChanges();
                ParentForm.CargarDatosCliente();
                this.Close();
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
