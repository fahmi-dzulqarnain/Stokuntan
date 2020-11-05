using Stokuntan.DatabaseModel;
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

namespace Stokuntan.Windows
{
    public partial class Tambah_Lokasi : Window
    {
        public Tambah_Lokasi()
        {
            InitializeComponent();
        }

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            var db = new IDatabase().Conn();
            try
            {
                var id = 1;
                var getLokasi = db.Query<TabelLokasi>("SELECT * FROM TabelLokasi");
                if (getLokasi.Count != 0) id = getLokasi.Max(x => x.ID_LOKASI) + 1;
                if (txtBoxNamaLokasi.Text != "") { db.Insert(new TabelLokasi() { ID_LOKASI = id, NAMA_LOKASI = txtBoxNamaLokasi.Text }); DialogResult = true; }
                else MessageBox.Show("Isi nama lokasi untuk menambahkan lokasi", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Ada yang Salah, terjadi tabrakan Data", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBatal_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
