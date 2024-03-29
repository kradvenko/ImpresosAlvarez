﻿using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para TrabajosImpresionTerminado.xaml
    /// </summary>
    public partial class TrabajosImpresionTerminado : Window
    {
        List<Ordenes> TrabajosPendientes;
        public TrabajosImpresionTerminado()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                TrabajosPendientes = dbContext.Ordenes.Where(T => T.estado == "IMPRESION" || T.estado == "TERMINADO").ToList();
                dgTrabajos.ItemsSource = TrabajosPendientes;
            }
        }

        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTrabajos.SelectedItem != null)
            {
                if (MessageBox.Show("Desea enviar a entrega?", "ATENCION", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    InfoTaller info = new InfoTaller(this);
                    info.ShowDialog();
                }
            }
        }
        public void EnviarOrden(bool Aplica, int IdLamina, int IdNegativo, Usuarios UsuarioGuarda)
        {
            Ordenes elegida = (Ordenes)dgTrabajos.SelectedItem;
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                Ordenes orden = dbContext.Ordenes.Where(T => T.id_orden == elegida.id_orden).First();
                orden.estado = "POR ENTREGAR";
                orden.inicio_por_entregar = DateTime.Now.ToShortDateString();
                dbContext.SaveChanges();

                if (Aplica)
                {
                    Entity.InfoTaller info = new Entity.InfoTaller();
                    info.IdUsuarioLamina = IdLamina;
                    info.IdUsuarioNegativo = IdNegativo;
                    info.Fecha = DateTime.Now;
                    info.IdUsuarioGuarda = UsuarioGuarda.id_usuario;
                    dbContext.InfoTaller.Add(info);
                    dbContext.SaveChanges();
                }

                TrabajosPendientes = dbContext.Ordenes.Where(T => T.estado == "IMPRESION" || T.estado == "TERMINADO").ToList();
                dgTrabajos.ItemsSource = TrabajosPendientes;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
