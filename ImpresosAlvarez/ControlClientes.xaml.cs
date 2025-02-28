using ImpresosAlvarez.Entity;
using iText.StyledXmlParser.Css.Resolve.Shorthand.Impl;
using Org.BouncyCastle.Asn1.Ocsp;
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
    /// Lógica de interacción para ControlClientes.xaml
    /// </summary>
    public partial class ControlClientes : Window
    {
        List<Clientes> _clientes;
        Clientes _clienteElegido;
        public ControlClientes()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                _clientes = dbContext.Clientes.ToList();
                tbClientes.AutoCompleteSource = _clientes;
            }
        }

        private void btnCorreos_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteElegido != null)
            {
                CorreosCliente correos = new CorreosCliente(_clienteElegido);
                correos.ShowDialog();
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteElegido != null)
            {
                if (MessageBox.Show("Desea guardar los cambios", "Atencion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            Clientes c = dbContext.Clientes.Where(T => T.id_cliente == _clienteElegido.id_cliente).First();

                            c.nombre = tbNombre.Text;
                            c.domicilio = tbDomicilio.Text;
                            c.colonia = tbColonia.Text;
                            c.numero_exterior = tbExterior.Text;
                            c.numero_interior = tbInterior.Text;
                            c.estado = tbEstado.Text;
                            c.codigo_postal = tbCodigoPostal.Text;
                            c.telefono1 = tbTelefono1.Text;
                            c.telefono2 = tbTelefono2.Text;
                            c.telefono3 = tbTelefono3.Text;
                            c.contacto = tbContacto.Text;
                            c.tipo_persona = cbTipoPersona.Text;
                            c.pseudonimo = tbNombreCorto.Text;
                            c.rfc = tbRFC.Text;
                            c.ciudad = tbCiudad.Text;
                            c.nombre_constancia = tbNombreConstancia.Text;  
                            if (cbRegimenFiscal.Text != "")
                            {
                                c.regimen_fiscal = cbRegimenFiscal.Text.Substring(0, 3);
                            }
                            
                            dbContext.SaveChanges();

                            MessageBox.Show("Se han guardado los cambios.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                if (MessageBox.Show("Desea guardar el nuevo cliente", "Atencion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
                        {
                            Clientes c = new Clientes();

                            c.nombre = tbNombre.Text;
                            c.domicilio = tbDomicilio.Text;
                            c.colonia = tbColonia.Text;
                            c.numero_exterior = tbExterior.Text;
                            c.numero_interior = tbInterior.Text;
                            c.estado = tbEstado.Text;
                            c.codigo_postal = tbCodigoPostal.Text;
                            c.telefono1 = tbTelefono1.Text;
                            c.telefono2 = tbTelefono2.Text;
                            c.telefono3 = tbTelefono3.Text;
                            c.telefono4 = "";
                            c.contacto = tbContacto.Text;
                            c.tipo_persona = cbTipoPersona.Text;
                            c.pseudonimo = tbNombreCorto.Text;
                            c.aplica_retencion = "SI";
                            c.rfc = tbRFC.Text;
                            c.ciudad = tbCiudad.Text;
                            c.nombre_constancia = tbNombreConstancia.Text;
                            if (cbRegimenFiscal.Text != "")
                            {
                                c.regimen_fiscal = cbRegimenFiscal.Text.Substring(0, 3);
                            }

                            dbContext.Clientes.Add(c);
                            dbContext.SaveChanges();

                            MessageBox.Show("Se han guardado los cambios.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbClientes_SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (tbClientes.SelectedItem != null)
            {
                _clienteElegido = (Clientes)tbClientes.SelectedItem;

                tbNombre.Text = _clienteElegido.nombre;
                tbDomicilio.Text = _clienteElegido.domicilio;
                tbColonia.Text = _clienteElegido.colonia;
                tbExterior.Text = _clienteElegido.numero_exterior;
                tbInterior.Text = _clienteElegido.numero_interior;
                tbEstado.Text = _clienteElegido.estado;
                tbCodigoPostal.Text = _clienteElegido.codigo_postal;
                tbTelefono1.Text = _clienteElegido.telefono1;
                tbTelefono2.Text = _clienteElegido.telefono2;
                tbTelefono3.Text = _clienteElegido.telefono3;
                tbContacto.Text = _clienteElegido.contacto;
                cbTipoPersona.Text = _clienteElegido.tipo_persona;
                tbRFC.Text = _clienteElegido.rfc;
                tbNombreConstancia.Text = _clienteElegido.nombre_constancia;
                if (_clienteElegido.regimen_fiscal != null)
                {
                    for (int i = 0; i < cbRegimenFiscal.Items.Count; i++)
                    {
                        if (cbRegimenFiscal.Items[i].ToString().Contains(_clienteElegido.regimen_fiscal))
                        {
                            cbRegimenFiscal.SelectedIndex = i;
                        }
                    }
                }
                tbNombreCorto.Text = _clienteElegido.pseudonimo;

            }
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            _clienteElegido = null;

            tbNombre.Text = "";
            tbDomicilio.Text = "";
            tbColonia.Text = "";
            tbExterior.Text = "";
            tbInterior.Text = "";
            tbCodigoPostal.Text = "";
            tbTelefono1.Text = "";
            tbTelefono2.Text = "";
            tbTelefono3.Text = "";
            tbContacto.Text = "";
            cbTipoPersona.SelectedIndex = 0;
            tbRFC.Text = "";
            tbNombreConstancia.Text = "";
            cbRegimenFiscal.SelectedIndex = 0;
            tbNombreCorto.Text = "";
        }
    }
}
