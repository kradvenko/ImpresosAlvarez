using ImpresosAlvarez.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Lógica de interacción para ComplementoPagoControl.xaml
    /// </summary>
    public partial class ComplementoPagoControl : Window
    {
        ComplementoPagoData _cpd;
        ComplementoPago _parent;
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        String insoluto;
        public ComplementoPagoControl(ComplementoPagoData CPD, ComplementoPago Parent)
        {
            InitializeComponent();
            this._cpd = CPD;
            this._parent = Parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbCantidad.Text = _cpd.Pagado;
            tbInsoluto.Text = _cpd.SaldoInsoluto;
            tbAnterior.Text = _cpd.SaldoAnterior;
        }

        private void tbCantidad_KeyUp(object sender, KeyEventArgs e)
        {
            CalcularTotal();
        }

        private void CalcularTotal()
        {
            try
            {
                float ins = float.Parse(_cpd.SaldoAnterior) - float.Parse(tbCantidad.Text);
                ins = (float)Math.Round(ins, 2);
                tbInsoluto.Text = ins.ToString();
                insoluto = ins.ToString();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //CalcularTotal();
            _cpd.Pagado = tbCantidad.Text;
            _cpd.SaldoInsoluto = tbInsoluto.Text;
            _cpd.SaldoAnterior = tbAnterior.Text;

            if (_cpd.SaldoInsoluto != "0")
            {
                _parent.PagoCompleto = false;
            }
            _parent.ActualizarComplemento();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
    }
}
