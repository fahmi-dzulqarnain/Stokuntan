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
    public partial class Tambah_Satuan : Window
    {
        public Tambah_Satuan()
        {
            InitializeComponent();
        }

        private void BtnSimpanSatuan_Click(object sender, RoutedEventArgs e)
        {
            var db = new IDatabase().Conn();
            var nama = txtBoxNamaSatuan.Text;
            var raws = txtBoxDalamGram.Text;
            try
            {
                if (nama != "" && int.TryParse(raws, out int gram)) { db.Insert(new TabelSatuan() { NAMA_SATUAN = nama, DALAM_GRAM = gram }); DialogResult = true; }
                else MessageBox.Show("Isi nama kategori untuk menambahkan kategori, Atau pastikan gram hanya menggunakan nomor", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Satuan sudah ada", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
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
