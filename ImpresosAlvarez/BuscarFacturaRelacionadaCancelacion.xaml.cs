﻿using ImpresosAlvarez.Entity;
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
    /// Lógica de interacción para BuscarFacturaRelacionadaCancelacion.xaml
    /// </summary>
    public partial class BuscarFacturaRelacionadaCancelacion : Window
    {
        MotivosCancelacionFactura ParentForm;
        public BuscarFacturaRelacionadaCancelacion(MotivosCancelacionFactura ParentForm)
        {
            InitializeComponent();
            this.ParentForm = ParentForm;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnElegirFactura_Click(object sender, RoutedEventArgs e)
        {
            if (dgFacturas.SelectedItem != null)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    try
                    {
                        String idfactura = dgFacturas.SelectedItem.GetType().GetProperty("id_factura").GetValue(dgFacturas.SelectedItem, null).ToString();
                        Entity.FacturaDigital f = dbContext.FacturaDigital.Where(F => F.id_factura.ToString() == idfactura).First();

                        ParentForm.ElegirFactura(f);
                        this.Close();
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                //_facturas = dbContext.Facturas.Where(F => F.IdCliente == _clienteElegido.IdCliente).ToList();

                var facts = dbContext.Facturas
                    .Join(
                        dbContext.Contribuyentes,
                        f => f.id_contribuyente,
                        c => c.id_contribuyente,
                        (f, c) => new
                        {
                            f.id_cliente,
                            f.id_factura,
                            f.numero,
                            Fecha = f.fecha.Substring(0, 10),
                            f.estado,
                            Total = Math.Round((double)f.total, 2),
                            c.nombre
                        }
                    )
                    .Join(
                        dbContext.Clientes,
                        comb => comb.id_cliente,
                        cli => cli.id_cliente,
                        (comb, cli) => new
                        {
                            comb.id_cliente,
                            comb.id_factura,
                            comb.numero,
                            comb.Fecha,
                            comb.estado,
                            comb.nombre,
                            comb.Total,
                            Cliente = cli.nombre
                        }
                    )
                    .Join(
                        dbContext.FacturaDigital,
                        comb => comb.id_factura,
                        fd => fd.id_factura,
                        (comb, fd) => new
                        {
                            comb.id_cliente,
                            comb.id_factura,
                            comb.numero,
                            comb.Fecha,
                            comb.estado,
                            comb.nombre,
                            comb.Total,
                            comb.Cliente,
                            fd.para_cancelacion
                        }
                    )
                    .Where(F => F.para_cancelacion == "SI")
                    .OrderByDescending(F => F.id_factura)
                    .ToList();

                dgFacturas.ItemsSource = facts;
            }
        }
    }
}
