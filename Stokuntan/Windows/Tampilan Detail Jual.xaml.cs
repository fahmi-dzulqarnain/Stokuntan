using SQLite;
using Stokuntan.Classes;
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
    public partial class Tampilan_Detail_Jual : Window
    {
        public Tampilan_Detail_Jual(string namaCustomer, int idTransaksi, SQLiteConnection db)
        {
            InitializeComponent();
            var detailJual = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan WHERE ID_JUAL = ?", idTransaksi).FirstOrDefault();
            txtCustomer.Text = namaCustomer;
            txtTglTransaksi.Text = detailJual.TANGGAL_JUAL;

            var columns = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Nama / Kategori Barang", BINDING = "NAMA_BARANG", WIDTH = 210 },
                new DataGridAttr() { HEADER = "Qty", BINDING = "JUMLAH_BELI", WIDTH = 80 },
                new DataGridAttr() { HEADER = "Harga Satuan", BINDING = "HARGA_SATUAN", WIDTH = 170 },
                new DataGridAttr() { HEADER = "Diskon", BINDING = "DISKON", WIDTH = 80 },
                new DataGridAttr() { HEADER = "Total", BINDING = "HARGA_TOTAL", WIDTH = 170 }
            };
            foreach (var value in columns)
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = value.HEADER,
                    Width = value.WIDTH,
                    Binding = new Binding(value.BINDING)
                };
                TableDetailTransaksi.Columns.Add(column);
            }

            var details = new List<DetailPenjualan>();
            var detail = detailJual.DETAIL_JUAL.Split('#');
            foreach (var value in detail) 
            {
                var final = value.Split(';');
                details.Add(new DetailPenjualan()
                {
                    NAMA_BARANG = final[0],
                    JUMLAH_BELI = final[1],
                    HARGA_SATUAN = final[2],
                    DISKON = final[3],
                    HARGA_TOTAL = final[4]
                });
            }
            TableDetailTransaksi.ItemsSource = null;
            TableDetailTransaksi.ItemsSource = details;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}