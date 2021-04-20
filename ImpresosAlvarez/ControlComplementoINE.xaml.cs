using ImpresosAlvarez.Clases;
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
    /// Lógica de interacción para ControlComplementoINE.xaml
    /// </summary>
    public partial class ControlComplementoINE : Window
    {
        FacturaDigital Parent;
        Factura datosFacturaElectronica;
        public ControlComplementoINE(FacturaDigital Parent, Factura datosFacturaElectronica)
        {
            InitializeComponent();
            this.Parent = Parent;
            this.datosFacturaElectronica = datosFacturaElectronica;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbTipoProceso.Text.Length > 0)
                {
                    datosFacturaElectronica.IneTipoProceso = cbTipoProceso.Text;

                    if (cbTipoProceso.Text == "Ordinario")
                    {
                        if (cbTipoComite.Text == "Ejecutivo Nacional")
                        {
                            datosFacturaElectronica.IneTipoComite = cbTipoComite.Text;
                            datosFacturaElectronica.IneClaveContabilidad = tbClaveContabilidad.Text;
                            datosFacturaElectronica.IneIdContabilidad = "";
                            datosFacturaElectronica.IneEntidad = "";
                            datosFacturaElectronica.IneAmbito = "";
                        }
                        else
                        {
                            datosFacturaElectronica.IneTipoComite = cbTipoComite.Text;
                            datosFacturaElectronica.IneClaveContabilidad = tbClaveContabilidad.Text;
                            datosFacturaElectronica.IneIdContabilidad = "";
                            datosFacturaElectronica.IneEntidad = cbEntidades.Text;
                            datosFacturaElectronica.IneAmbito = "";
                        }
                    }
                    else
                    {
                        datosFacturaElectronica.IneTipoComite = "";
                        datosFacturaElectronica.IneClaveContabilidad = "";
                        datosFacturaElectronica.IneIdContabilidad = tbIdContabilidad.Text;
                        datosFacturaElectronica.IneEntidad = cbEntidades.Text;
                        datosFacturaElectronica.IneAmbito = cbAmbito.Text;
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No ha elegido un Tipo de Proceso");
                    cbTipoProceso.Focus();
                }
            }
            catch
            {

            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbContribuyentes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void cbTipoProceso_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbTipoProceso != null && cbTipoComite != null)
                {
                    string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
                    if (text == "Ordinario")
                    {
                        cbTipoComite.IsEnabled = true;
                        cbAmbito.IsEnabled = false;
                        tbClaveContabilidad.IsEnabled = true;
                        tbIdContabilidad.IsEnabled = false;
                        if (cbTipoComite.Text == "Ejecutivo Nacional")
                        {
                            cbEntidades.IsEnabled = false;
                        }
                        else
                        {
                            cbEntidades.IsEnabled = true;
                        }
                    }
                    else
                    {
                        cbTipoComite.IsEnabled = false;
                        cbAmbito.IsEnabled = true;
                        tbClaveContabilidad.IsEnabled = false;
                        tbIdContabilidad.IsEnabled = true;
                        cbEntidades.IsEnabled = true;
                    }
                }
            }
            catch
            {

            }
        }

        private void cbTipoComite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbTipoProceso != null && cbTipoComite != null)
                {
                    string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
                    if (text == "Ejecutivo Nacional")
                    {
                        cbEntidades.IsEnabled = false;
                    }
                    else
                    {
                        cbEntidades.IsEnabled = true;
                    }
                }
            }
            catch
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (datosFacturaElectronica.IneTipoProceso != null)
            {
                cbTipoProceso.Text = datosFacturaElectronica.IneTipoProceso;
                cbTipoComite.Text = datosFacturaElectronica.IneTipoComite;
                tbClaveContabilidad.Text = datosFacturaElectronica.IneClaveContabilidad;
                cbEntidades.Text = datosFacturaElectronica.IneEntidad;
                cbAmbito.Text = datosFacturaElectronica.IneAmbito;
                tbIdContabilidad.Text = datosFacturaElectronica.IneIdContabilidad;
            }

            if (cbTipoProceso.Text == "Ordinario")
            {
                cbTipoComite.IsEnabled = true;
                cbAmbito.IsEnabled = false;
                tbClaveContabilidad.IsEnabled = true;
                tbIdContabilidad.IsEnabled = false;
                if (cbTipoComite.Text == "Ejecutivo Nacional")
                {
                    cbEntidades.IsEnabled = false;
                }
                else
                {
                    cbEntidades.IsEnabled = true;
                }
            }
            else
            {
                cbTipoComite.IsEnabled = false;
                cbAmbito.IsEnabled = true;
                tbClaveContabilidad.IsEnabled = false;
                tbIdContabilidad.IsEnabled = true;
                cbEntidades.IsEnabled = true;
            }
        }
    }
}
