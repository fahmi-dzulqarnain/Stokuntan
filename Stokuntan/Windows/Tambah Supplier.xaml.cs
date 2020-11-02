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
    public partial class Tambah_Supplier : Window
    {
        public Tambah_Supplier()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnSimpanSupplier_Click(object sender, RoutedEventArgs e)
        {
            var db = new IDatabase().Conn();
            var id = 1;
            var raw = db.Query<TabelSupplier>("SELECT * FROM TabelSupplier");
            if (raw.Count != 0) id = raw.Max(x => x.ID_SUPPLIER) + 1;
            var nama = txtBoxNamaSupplier.Text;
            try
            {
                if (nama != "") { 
                    db.Insert(new TabelSupplier() { ID_SUPPLIER = id, NAMA_SUPPLIER = nama, KONTAK_SUPPLIER = txtBoxKontak.Text, ALAMAT_SUPPLIER = txtBoxAlamat.Text }); 
                    DialogResult = true; 
                }
                else MessageBox.Show("Isi nama supplier untuk menambahkan supplier", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Ada tabrakan data, Hubungi kami", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBatalSimpan_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
