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
    public partial class Tambah_Kategori : Window
    {
        public Tambah_Kategori()
        {
            InitializeComponent();
        }

        private void BtnSimpanKategori_Click(object sender, RoutedEventArgs e)
        {
            var db = new IDatabase().Conn();
            try
            {
                if (txtBoxNamaKategori.Text != "") { db.Insert(new TabelKategori() { NAMA_KATEGORI = txtBoxNamaKategori.Text }); DialogResult = true; }
                else MessageBox.Show("Isi nama kategori untuk menambahkan kategori", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Kategori sudah ada", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBatalSimpan_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
