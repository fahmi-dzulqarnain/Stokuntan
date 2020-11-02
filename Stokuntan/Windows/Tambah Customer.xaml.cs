using SQLite;
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
    public partial class Tambah_Customer : Window
    {
        SQLiteConnection db = new IDatabase().Conn();
        int discount = 0;
        public string customerName = "";
        public Tambah_Customer()
        {
            InitializeComponent();
        }

        private void BtnSimpanCustomer_Click(object sender, RoutedEventArgs e)
        {
            var namaCustomer = txtBoxNamaCustomer.Text;
            if (namaCustomer != "")
            {
                var id = 1;
                var getCustomer = db.Query<TabelCustomer>("SELECT * FROM TabelCustomer");
                if (getCustomer.Count != 0) id = getCustomer.Max(x => x.ID_CUSTOMER) + 1;
                customerName = namaCustomer;
                db.Insert(new TabelCustomer() { ID_CUSTOMER = id, NAMA_CUSTOMER = namaCustomer, DISKON_TETAP = discount, ALAMAT_CUSTOMER = txtBoxAlamat.Text, KONTAK_CUSTOMER = txtBoxKontak.Text });
                DialogResult = true;
                Close();
            }
        }

        private void BtnBatalSimpan_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TxtBoxDiskon_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxDiskon.Text != "")
            {
                try
                {
                    discount = int.Parse(txtBoxDiskon.Text.Replace("%", ""));
                    if (!string.IsNullOrEmpty(txtBoxDiskon.Text))
                    {
                        txtBoxDiskon.Text = $"{discount}%";
                        txtBoxDiskon.Select(txtBoxDiskon.Text.Length, 0);
                    }
                    else txtBoxDiskon.Text = "0%";
                }
                catch (Exception) { MessageBox.Show("Diskon harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
