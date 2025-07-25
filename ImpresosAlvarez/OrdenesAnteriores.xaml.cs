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
    /// Lógica de interacción para OrdenesAnteriores.xaml
    /// </summary>
    public partial class OrdenesAnteriores : Window
    {
        List<Ordenes> _anteriores;
        Clientes ClienteElegido;
        NuevaOrden _Parent;

        public OrdenesAnteriores(Clientes ClienteElegido, NuevaOrden Parent)
        {
            InitializeComponent();
            this.ClienteElegido = ClienteElegido;
            this._Parent = Parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _anteriores = dbContext.Ordenes.Where(A => A.id_cliente == ClienteElegido.id_cliente).OrderByDescending(A => A.id_orden).ToList();
                dgOrdenesAnteriores.ItemsSource = null;
                dgOrdenesAnteriores.ItemsSource = _anteriores;
            }
        }

        private void dgOrdenesAnteriores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgOrdenesAnteriores.SelectedItem != null)
                {
                    Ordenes Anterior = (Ordenes)dgOrdenesAnteriores.SelectedItem;

                    _Parent.tbTelefono.Text = Anterior.telefono;
                    _Parent.tbSolicita.Text = Anterior.solicitante;
                    _Parent.tbNombreTrabajo.Text = Anterior.nombre_trabajo;
                    _Parent.tbCantidad.Text = Anterior.cantidad.ToString();
                    _Parent.tbColorTintas.Text = Anterior.color_tintas;
                    _Parent.tbTipoPapel.Text = Anterior.tipo_papel;
                    _Parent.cbTamaño.Text = Anterior.tamano;
                    _Parent.cbPrimeraCopia.Text = Anterior.copia_1;
                    _Parent.cbSegundaCopia.Text = Anterior.copia_2;
                    _Parent.cbTerceraCopia.Text = Anterior.copia_3;
                    _Parent.cbCuartaCopia.Text = Anterior.copia_4;
                    _Parent.tbCostoAnterior.Text = Anterior.costo_anterior.ToString();
                    _Parent.tbNotas.Text = Anterior.otros_4;
                    _Parent.tbDescripcion.Text = Anterior.especificaciones;
                    _Parent.tbOrdenAnterior.Text = Anterior.numero.ToString();
                    _Parent.dtpFechaNegativo.Text = Anterior.fecha_solicita.ToString();
                    

                    if (Anterior.pegado == "SI")
                    {
                        _Parent.chbPegado.IsChecked = true;
                    }
                    else
                    {
                        _Parent.chbPegado.IsChecked = false;
                    }
                    if (Anterior.engrapado == "SI")
                    {
                        _Parent.chbEngrapado.IsChecked = true;
                    }
                    else
                    {
                        _Parent.chbEngrapado.IsChecked = false;
                    }
                    if (Anterior.perforacion == "SI")
                    {
                        _Parent.chbPerforacion.IsChecked = true;
                    }
                    else
                    {
                        _Parent.chbPerforacion.IsChecked = false;
                    }
                    if (Anterior.rojo == "SI")
                    {
                        _Parent.chbRojo.IsChecked = true;
                    }
                    else
                    {
                        _Parent.chbRojo.IsChecked = false;
                    }
                    if (Anterior.blanco == "SI")
                    {
                        _Parent.chbBlanco.IsChecked = true;
                    }
                    else
                    {
                        _Parent.chbBlanco.IsChecked = false;
                    }
                    if (Anterior.con_folio == "SI")
                    {
                        _Parent.cbConFolio.Text = "SI";
                        int del = int.Parse(Anterior.al_numero);
                        int al = del + (int)Anterior.cantidad;
                        _Parent.tbDelNumero.Text = (del + 1).ToString();
                        _Parent.tbAlNumero.Text = al.ToString();
                    } 
                    else
                    {
                        _Parent.cbConFolio.Text = "NO";
                    }
                    _Parent.RutaOrden = Anterior.ruta;

                    this.Close();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR ORDEN ANTERIOR: " + exc.Message);
            }
        }

        private void tbBusqueda_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbBusqueda.Text.Length > 0)
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        _anteriores = dbContext.Ordenes.Where(A => A.nombre_trabajo.Contains(tbBusqueda.Text) && A.id_cliente == ClienteElegido.id_cliente).OrderByDescending(A => A.id_orden).ToList();
                        dgOrdenesAnteriores.ItemsSource = null;
                        dgOrdenesAnteriores.ItemsSource = _anteriores;
                    }
                    
                }
                else
                {
                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        _anteriores = dbContext.Ordenes.Where(A => A.id_cliente == ClienteElegido.id_cliente).OrderByDescending(A => A.id_orden).ToList();
                        dgOrdenesAnteriores.ItemsSource = null;
                        dgOrdenesAnteriores.ItemsSource = _anteriores;
                    }
                }
            }
        }
    }
}
