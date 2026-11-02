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
    /// Lógica de interacción para OrdenesDeNota.xaml
    /// </summary>
    public partial class OrdenesDeNota : System.Windows.Window
    {
        int _idNota;

        public OrdenesDeNota(int idNota)
        {
            InitializeComponent();
            _idNota = idNota;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (ImpresosBDEntities dbContext = new ImpresosBDEntities())
            {
                var ordenes = dbContext.NotaOrden
                    .Where(no => no.id_nota == _idNota)
                    .Join(
                        dbContext.Ordenes,
                        no => no.id_orden,
                        o => o.id_orden,
                        (no, o) => new
                        {
                            Numero = o.numero
                        }
                    )
                    .ToList();

                dgOrdenes.ItemsSource = ordenes;
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}