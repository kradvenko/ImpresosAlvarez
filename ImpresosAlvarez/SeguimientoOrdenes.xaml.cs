using ImpresosAlvarez.Clases;
using ImpresosAlvarez.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Lógica de interacción para SeguimientoOrdenes.xaml
    /// </summary>
    /// 
    public class FacturaNota
    {
        public String Tipo { get; set; }
        public int Id { get; set; }
        public String Numero { get; set; }
        public FacturaNota() { }
    }
    public partial class SeguimientoOrdenes : Window
    {
        List<Ordenes> ListaOrdenes;
        List<Clientes> ListaClientes;
        Ordenes OrdenElegida;
        Usuarios CurrentUser;
        List<Usuarios> UsuariosEntrega;
        List<FacturaNota> FacturasNotas;
        public SeguimientoOrdenes(Usuarios CurrentUser)
        {
            InitializeComponent();
            this.CurrentUser = CurrentUser;
        }

        private void cbAreas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String Area;
            ListaOrdenes = new List<Ordenes>();

            Area = cbAreas.SelectedValue.ToString();

            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaOrdenes = dbContext.Ordenes.Where(A => A.estado == Area).OrderByDescending(A => A.id_orden).ToList();                
                dgOrdenes.ItemsSource = null;
                dgOrdenes.ItemsSource = ListaOrdenes;
            }
        }

        private void dgOrdenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrdenes.SelectedItem != null)
            {
                OrdenElegida = new Ordenes();

                OrdenElegida = (Ordenes)dgOrdenes.SelectedItem;

                String Cliente;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Cliente = dbContext.Clientes.Where(C => C.id_cliente == OrdenElegida.id_cliente).First().nombre;
                }

                tbNombreCliente.Text = Cliente;
                tbTrabajo.Text = OrdenElegida.nombre_trabajo;
                tbFechaSolicito.Text = OrdenElegida.fecha_solicita;
                tbHoraSolicito.Text = OrdenElegida.hora_solicita;
                tbRecibio.Text = OrdenElegida.quien_recibio;
                tbCantidad.Text = OrdenElegida.cantidad.ToString();
                tbTintas.Text = OrdenElegida.color_tintas;
                tbPapel.Text = OrdenElegida.tipo_papel;
                cbTamanio.Text = OrdenElegida.tamano;
                if (OrdenElegida.con_folio == "SI")
                {
                    cbConFolio.IsChecked = true;
                } else
                {
                    cbConFolio.IsChecked = false;
                }
                tbDel.Text = OrdenElegida.del_numero;
                tbAl.Text = OrdenElegida.al_numero;
                tbTotal.Text = OrdenElegida.total.ToString();
                tbAnticipo.Text = OrdenElegida.anticipo.ToString();
                tbCostoAnterior.Text = OrdenElegida.costo_anterior.ToString();
                tbOtros1.Text = OrdenElegida.otros_1;
                tbOtros2.Text = OrdenElegida.otros_2;
                tbOtros3.Text = OrdenElegida.otros_3;
                tbOtros4.Text = OrdenElegida.otros_4;
                tbTelefono.Text = OrdenElegida.telefono;
                cbCopias1.Text = OrdenElegida.copia_1;
                cbCopias2.Text = OrdenElegida.copia_2;
                cbCopias3.Text = OrdenElegida.copia_3;
                cbCopias4.Text = OrdenElegida.copia_4;
                if (OrdenElegida.pegado == "SI")
                {
                    cbPegado.IsChecked = true;
                }
                else
                {
                    cbPegado.IsChecked = false;
                }
                if (OrdenElegida.engrapado == "SI")
                {
                    cbEngrapado.IsChecked = true;
                }
                else
                {
                    cbEngrapado.IsChecked = false;
                }
                if (OrdenElegida.perforacion == "SI")
                {
                    cbPerforacion.IsChecked = true;
                }
                else
                {
                    cbPerforacion.IsChecked = false;
                }
                if (OrdenElegida.rojo == "SI")
                {
                    cbRojo.IsChecked = true;
                }
                else
                {
                    cbRojo.IsChecked = false;
                }
                if (OrdenElegida.blanco == "SI")
                {
                    cbBlanco.IsChecked = true;
                }
                else
                {
                    cbBlanco.IsChecked = false;
                }
                tbNotas.Text = OrdenElegida.especificaciones;

                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    var dis = dbContext.Disenos
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                FechaInicio = orden.fecha_inicio,
                                FechaFin = orden.fecha_fin,
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgDiseno.ItemsSource = dis;

                    var imp = dbContext.Impresion
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                FechaInicio = orden.fecha_inicio,
                                FechaFin = orden.fecha_fin,
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgImpresion.ItemsSource = imp;

                    var ter = dbContext.Terminado
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                FechaInicio = orden.fecha_inicio,
                                FechaFin = orden.fecha_fin,
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgTerminado.ItemsSource = ter;

                    List<Ordenes> PorEntregar = new List<Ordenes>();
                    PorEntregar.Add(OrdenElegida);
                    dgPorEntregar.ItemsSource = PorEntregar;
                    
                    var ent = dbContext.Entrega
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                Fecha = orden.fecha,                                
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgEntrega.ItemsSource = ent;

                    if (OrdenElegida.estado == "DISEÑO")
                    {
                        imgDiseno.Visibility = Visibility.Visible;
                        imgImpresion.Visibility = Visibility.Hidden;
                        imgTerminado.Visibility = Visibility.Hidden;
                        imgPorEntregar.Visibility = Visibility.Hidden;
                        imgEntrega.Visibility = Visibility.Hidden;
                    }
                    else if (OrdenElegida.estado == "IMPRESION")
                    {
                        imgDiseno.Visibility = Visibility.Hidden;
                        imgImpresion.Visibility = Visibility.Visible;
                        imgTerminado.Visibility = Visibility.Hidden;
                        imgPorEntregar.Visibility = Visibility.Hidden;
                        imgEntrega.Visibility = Visibility.Hidden;
                    }
                    else if (OrdenElegida.estado == "IMPRESION")
                    {
                        imgDiseno.Visibility = Visibility.Hidden;
                        imgImpresion.Visibility = Visibility.Hidden;
                        imgTerminado.Visibility = Visibility.Visible;
                        imgPorEntregar.Visibility = Visibility.Hidden;
                        imgEntrega.Visibility = Visibility.Hidden;
                    }
                    else if (OrdenElegida.estado == "POR ENTREGAR")
                    {
                        imgDiseno.Visibility = Visibility.Hidden;
                        imgImpresion.Visibility = Visibility.Hidden;
                        imgTerminado.Visibility = Visibility.Hidden;
                        imgPorEntregar.Visibility = Visibility.Visible;
                        imgEntrega.Visibility = Visibility.Hidden;
                    }
                    else if (OrdenElegida.estado == "ENTREGADO")
                    {
                        imgDiseno.Visibility = Visibility.Hidden;
                        imgImpresion.Visibility = Visibility.Hidden;
                        imgTerminado.Visibility = Visibility.Hidden;
                        imgPorEntregar.Visibility = Visibility.Hidden;
                        imgEntrega.Visibility = Visibility.Visible;
                    }

                    try
                    {
                        FacturasNotas = new List<FacturaNota>();

                        NotaOrden no = dbContext.NotaOrden.Where(N => N.id_orden == OrdenElegida.id_orden).FirstOrDefault();
                        if (no != null)
                        {
                            Notas n = dbContext.Notas.Where(N => N.id_nota == no.id_nota).FirstOrDefault();
                            FacturaNota fn = new FacturaNota();
                            fn.Tipo = "NOTA";
                            fn.Id = n.id_nota;
                            fn.Numero = n.numero;
                            FacturasNotas.Add(fn);
                        }

                        FacturaOrden fo = dbContext.FacturaOrden.Where(N => N.id_orden == OrdenElegida.id_orden).FirstOrDefault();
                        if (fo != null)
                        {
                            Facturas f = dbContext.Facturas.Where(N => N.id_factura == fo.id_factura).FirstOrDefault();
                            FacturaNota fn = new FacturaNota();
                            fn.Tipo = "FACTURA";
                            fn.Id = f.id_factura;
                            fn.Numero = f.numero;
                            FacturasNotas.Add(fn);
                        }

                        dgFacturasNotas.ItemsSource = null;
                        dgFacturasNotas.ItemsSource = FacturasNotas;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ERROR TIPO 2: " + ex.Message);
                    }
                }
            }            
        }

        private void tbNumeroOrden_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double NumeroOrden;

                if (double.TryParse(tbNumeroOrden.Text, out NumeroOrden))
                {
                    NumeroOrden = double.Parse(tbNumeroOrden.Text);

                    using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                    {
                        ListaOrdenes = dbContext.Ordenes.Where(A => A.numero == NumeroOrden).OrderByDescending(A => A.id_orden).ToList();
                        dgOrdenes.ItemsSource = null;
                        dgOrdenes.ItemsSource = ListaOrdenes;
                    }
                }
            }
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                Clientes ClienteElegido = (Clientes)tbClientes.SelectedItem;
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    ListaOrdenes = dbContext.Ordenes.Where(A => A.id_cliente == ClienteElegido.id_cliente).OrderByDescending(A => A.id_orden).ToList();
                    dgOrdenes.ItemsSource = null;
                    dgOrdenes.ItemsSource = ListaOrdenes;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                ListaClientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = ListaClientes;

                UsuariosEntrega = dbContext.Usuarios.Where(U => (U.tipo == "ENTREGA" || U.tipo == "ADMIN" || U.tipo == "RECEPCION") && U.estado == "ACTIVO").ToList();
                cbPersonaEntrega.ItemsSource = UsuariosEntrega;
                cbPersonaEntrega.DisplayMemberPath = "nombre";
                cbPersonaEntrega.SelectedValuePath = "id_usuario";
                cbPersonaEntrega.SelectedIndex = 0;
            }

            FacturasNotas = new List<FacturaNota>();
        }

        private void btnEnviarDiseno_Click(object sender, RoutedEventArgs e)
        {
            if (OrdenElegida.estado == "DISEÑO")
            {
                return;
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Ordenes Orden = dbContext.Ordenes.Where(O => O.id_orden == OrdenElegida.id_orden).FirstOrDefault();

                    Orden.estado = "DISEÑO";
                    Orden.inicio_diseno = DateTime.Now.Date.ToShortDateString();
                    /*
                    Disenos diseno =  new Disenos();

                    diseno.id_orden = OrdenElegida.id_orden;
                    diseno.id_usuario = CurrentUser.id_usuario;
                    diseno.fecha_inicio = DateTime.Now.Date.ToShortDateString();
                    diseno.hora_inicio = DateTime.Now.Date.ToShortTimeString();

                    dbContext.Disenos.Add(diseno);
                    */
                    dbContext.SaveChanges();

                    OrdenElegida.estado = "DISEÑO";
                    OrdenElegida.inicio_diseno = DateTime.Now.Date.ToShortDateString();

                    VerificarEstadoOrden();

                    var dis = dbContext.Disenos
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                FechaInicio = orden.fecha_inicio,
                                FechaFin = orden.fecha_fin,
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgDiseno.ItemsSource = dis;
                }
            }
        }

        private void btnEnviarImpresion_Click(object sender, RoutedEventArgs e)
        {
            if (OrdenElegida.estado == "IMPRESION")
            {
                return;
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Ordenes Orden = dbContext.Ordenes.Where(O => O.id_orden == OrdenElegida.id_orden).FirstOrDefault();

                    Orden.estado = "IMPRESION";
                    Orden.inicio_impresion = DateTime.Now.Date.ToShortDateString();
                    /*
                    Impresion impresion = new Impresion();

                    impresion.id_orden = OrdenElegida.id_orden;
                    impresion.id_usuario = CurrentUser.id_usuario;
                    impresion.fecha_inicio = DateTime.Now.Date.ToShortDateString();
                    impresion.hora_inicio = DateTime.Now.Date.ToShortTimeString();

                    dbContext.Impresion.Add(impresion);
                    */
                    dbContext.SaveChanges();

                    OrdenElegida.estado = "IMPRESION";
                    OrdenElegida.inicio_impresion = DateTime.Now.Date.ToShortDateString();

                    VerificarEstadoOrden();

                    var imp = dbContext.Impresion
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                FechaInicio = orden.fecha_inicio,
                                FechaFin = orden.fecha_fin,
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgImpresion.ItemsSource = imp;
                }
            }
        }

        private void btnEnviarTerminado_Click(object sender, RoutedEventArgs e)
        {
            if (OrdenElegida.estado == "TERMINADO")
            {
                return;
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Ordenes Orden = dbContext.Ordenes.Where(O => O.id_orden == OrdenElegida.id_orden).FirstOrDefault();

                    Orden.estado = "TERMINADO";
                    Orden.inicio_terminado = DateTime.Now.Date.ToShortDateString();
                    /*
                    Terminado terminado = new Terminado();

                    terminado.id_orden = OrdenElegida.id_orden;
                    terminado.id_usuario = CurrentUser.id_usuario;
                    terminado.fecha_inicio = DateTime.Now.Date.ToShortDateString();
                    terminado.hora_inicio = DateTime.Now.Date.ToShortTimeString();

                    dbContext.Terminado.Add(terminado);
                    */
                    dbContext.SaveChanges();

                    OrdenElegida.estado = "TERMINADO";
                    OrdenElegida.inicio_terminado = DateTime.Now.Date.ToShortDateString();

                    VerificarEstadoOrden();

                    var ter = dbContext.Terminado
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                FechaInicio = orden.fecha_inicio,
                                FechaFin = orden.fecha_fin,
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgTerminado.ItemsSource = ter;
                }
            }
        }

        private void btnEnviarPorEntregar_Click(object sender, RoutedEventArgs e)
        {
            if (OrdenElegida.estado == "POR ENTREGAR")
            {
                return;
            }
            else
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Ordenes Orden = dbContext.Ordenes.Where(O => O.id_orden == OrdenElegida.id_orden).FirstOrDefault();

                    Orden.estado = "POR ENTREGAR";
                    Orden.inicio_por_entregar = DateTime.Now.Date.ToShortDateString();

                    dbContext.SaveChanges();

                    OrdenElegida.estado = "POR ENTREGAR";
                    OrdenElegida.inicio_por_entregar = DateTime.Now.Date.ToShortDateString();

                    VerificarEstadoOrden();

                    List<Ordenes> PorEntregar = new List<Ordenes>();
                    PorEntregar.Add(OrdenElegida);
                    dgPorEntregar.ItemsSource = PorEntregar;
                }
            }
        }

        private void btnEnviarEntrega_Click(object sender, RoutedEventArgs e)
        {
            if (OrdenElegida.estado == "ENTREGADO")
            {
                return;
            }
            else
            {
                gEntrega.Visibility = Visibility.Visible;
            }
        }

        private void VerificarEstadoOrden()
        {
            if (OrdenElegida.estado == "DISEÑO")
            {
                imgDiseno.Visibility = Visibility.Visible;
                imgImpresion.Visibility = Visibility.Hidden;
                imgTerminado.Visibility = Visibility.Hidden;
                imgPorEntregar.Visibility = Visibility.Hidden;
                imgEntrega.Visibility = Visibility.Hidden;
            }
            else if (OrdenElegida.estado == "IMPRESION")
            {
                imgDiseno.Visibility = Visibility.Hidden;
                imgImpresion.Visibility = Visibility.Visible;
                imgTerminado.Visibility = Visibility.Hidden;
                imgPorEntregar.Visibility = Visibility.Hidden;
                imgEntrega.Visibility = Visibility.Hidden;
            }
            else if (OrdenElegida.estado == "IMPRESION")
            {
                imgDiseno.Visibility = Visibility.Hidden;
                imgImpresion.Visibility = Visibility.Hidden;
                imgTerminado.Visibility = Visibility.Visible;
                imgPorEntregar.Visibility = Visibility.Hidden;
                imgEntrega.Visibility = Visibility.Hidden;
            }
            else if (OrdenElegida.estado == "POR ENTREGAR")
            {
                imgDiseno.Visibility = Visibility.Hidden;
                imgImpresion.Visibility = Visibility.Hidden;
                imgTerminado.Visibility = Visibility.Hidden;
                imgPorEntregar.Visibility = Visibility.Visible;
                imgEntrega.Visibility = Visibility.Hidden;
            }
            else if (OrdenElegida.estado == "ENTREGADO")
            {
                imgDiseno.Visibility = Visibility.Hidden;
                imgImpresion.Visibility = Visibility.Hidden;
                imgTerminado.Visibility = Visibility.Hidden;
                imgPorEntregar.Visibility = Visibility.Hidden;
                imgEntrega.Visibility = Visibility.Visible;
            }
        }

        private void btnCancelarEntregar_Click(object sender, RoutedEventArgs e)
        {
            gEntrega.Visibility = Visibility.Hidden;
        }

        private void btnEntregar_Click(object sender, RoutedEventArgs e)
        {
            if (cbPersonaEntrega.SelectedIndex >= 0)
            {
                using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                {
                    Ordenes Orden = dbContext.Ordenes.Where(O => O.id_orden == OrdenElegida.id_orden).FirstOrDefault();

                    Orden.estado = "ENTREGADO";
                    Orden.fecha_entregado = DateTime.Now.Date.ToShortDateString();

                    Usuarios uEntrega = (Usuarios)cbPersonaEntrega.SelectedItem;

                    Entrega entrega = new Entrega();

                    entrega.id_orden = OrdenElegida.id_orden;
                    entrega.id_usuario = uEntrega.id_usuario;
                    entrega.fecha = DateTime.Now.Date.ToShortDateString();
                    entrega.hora = DateTime.Now.Date.ToShortTimeString();
                    entrega.descripcion = "NUEVO SISTEMA";

                    dbContext.Entrega.Add(entrega);

                    dbContext.SaveChanges();

                    OrdenElegida.estado = "ENTREGADO";
                    OrdenElegida.fecha_entregado = DateTime.Now.Date.ToShortDateString();

                    VerificarEstadoOrden();

                    var ent = dbContext.Entrega
                        .Join(
                            dbContext.Usuarios,
                            orden => orden.id_usuario,
                            usu => usu.id_usuario,
                            (orden, usu) => new
                            {
                                Fecha = orden.fecha,
                                orden.id_orden,
                                Usuario = usu.nombre
                            }
                        )
                        .Where(F => F.id_orden == OrdenElegida.id_orden)
                        .ToList();

                    dgEntrega.ItemsSource = ent;

                    gEntrega.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
