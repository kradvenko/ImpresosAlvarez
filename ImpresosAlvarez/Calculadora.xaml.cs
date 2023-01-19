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
    /// Lógica de interacción para Calculadora.xaml
    /// </summary>
    public partial class Calculadora : Window
    {
        List<CalcElement> elementos;
        public Calculadora()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbCantidad.Focus();
            elementos = new List<CalcElement>();
        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int cantidad = 0;
                if (int.TryParse(tbCantidad.Text, out cantidad))
                {
                    tbUnitario.Focus();
                }
                else
                {
                    tbCantidad.Text = "0";
                }
            }
        }

        private void tbUnitario_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float unitario = 0;
                if (float.TryParse(tbUnitario.Text, out unitario))
                {
                    tbImporte.Focus();
                }
                else
                {
                    tbUnitario.Text = "0";
                }
            }
        }

        private void tbImporte_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float importe = 0;
                if (float.TryParse(tbImporte.Text, out importe))
                {
                    Calcular();
                }
                else
                {
                    tbImporte.Text = "0";
                }
            }
        }

        private void Calcular()
        {
            try
            {
                int cantidad;
                float unitario;
                float importe;

                cantidad = int.Parse(tbCantidad.Text);
                unitario = float.Parse(tbUnitario.Text);
                importe = float.Parse(tbImporte.Text);

                if (cantidad <= 0)
                {
                    MessageBox.Show("No ha escrito una cantidad correcta.");
                    tbCantidad.Focus();
                    return;
                }
                if (unitario <= 0)
                {
                    if (importe <= 0)
                    {
                        MessageBox.Show("No ha escrito un precio unitario correcto ni un importe correcto.");
                        tbUnitario.Focus();
                        return;
                    }
                    else
                    {
                        unitario = importe / cantidad;
                    }
                }
                else
                {
                    if (importe <= 0)
                    {
                        importe = unitario * cantidad;
                    }
                    else
                    {                        
                        MessageBox.Show("Hay cantidades para unitario e importe al mismo tiempo.");
                        tbImporte.Focus();
                        return;
                    }
                }
                Agregar(cantidad, unitario, importe);
            }
            catch (Exception exc)
            {

            }
        }

        private void Agregar(int cantidad, float unitario, float importe)
        {
            CalcElement ele = new CalcElement();
            ele.Cantidad = cantidad;
            ele.Unitario = unitario;
            ele.Importe = importe;

            float subtotal = 0;
            float iva = 0;
            float isr = 0;

            elementos.Add(ele);
            dgConceptos.ItemsSource = null;
            dgConceptos.ItemsSource = elementos;

            float t1;

            foreach (CalcElement item in elementos)
            {
                subtotal += item.Importe;
                t1 = item.Importe * 0.16f;
                iva += float.Parse(Math.Round(t1, 2).ToString());
                if (chbAplicarISR.IsChecked == true)
                {
                    t1 = item.Importe * 0.012500f;
                    isr += float.Parse(Math.Round(t1, 2).ToString());
                }
            }

            tbCantidad.Text = "";
            tbUnitario.Text = "";
            tbImporte.Text = "";

            lblSubtotal.Content = "$ " + subtotal.ToString();
            lblIVA.Content = "$ " + (subtotal * 0.16).ToString();

            lblISR.Content = "$ " + isr.ToString();

            lblTotal.Content = "$ " + (subtotal + (iva) - (isr)).ToString();
            tbCantidad.Focus();
        }

        private void dgConceptos_KeyUp(object sender, KeyEventArgs e)
        {
            if (dgConceptos.SelectedItem != null)
            {
                CalcElement rem = (CalcElement)dgConceptos.SelectedItem;
                elementos.Remove(rem);
                dgConceptos.ItemsSource = null;
                dgConceptos.ItemsSource = elementos;
            }
        }

        private void btnAplicarISR_Click(object sender, RoutedEventArgs e)
        {
            float subtotal = 0;
            float iva = 0;
            float isr = 0;

            float t1;

            foreach (CalcElement item in elementos)
            {
                subtotal += item.Importe;
                t1 = item.Importe * 0.16f;
                iva += float.Parse(Math.Round(t1, 2).ToString());
                if (chbAplicarISR.IsChecked == true)
                {
                    t1 = item.Importe * 0.012500f;
                    isr += float.Parse(Math.Round(t1, 2).ToString());
                }
            }

            tbCantidad.Text = "";
            tbUnitario.Text = "";
            tbImporte.Text = "";

            lblSubtotal.Content = "$ " + subtotal.ToString();
            lblIVA.Content = "$ " + (subtotal * 0.16).ToString();

            lblISR.Content = "$ " + isr.ToString();

            lblTotal.Content = "$ " + (subtotal + (iva) - (isr)).ToString();
            tbCantidad.Focus();
        }
    }
}
