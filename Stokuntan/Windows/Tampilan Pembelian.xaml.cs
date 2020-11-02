using SQLite;
using Stokuntan.Classes;
using Stokuntan.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
    public partial class Tampilan_Pembelian : Window
    {
        public Tampilan_Pembelian(string deskripsi, string satuan, SQLiteConnection db)
        {
            InitializeComponent();
            var culture = new CultureInfo("id-ID");
            var dataStok = db.Query<TabelStok>("SELECT * FROM TabelStok WHERE DESKRIPSI = ? AND SATUAN = ?", deskripsi, satuan);
            var columnList = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Tanggal Beli", BINDING = "TGL_MASUK", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Kategori Barang", BINDING = "NAMA_KATEGORI", WIDTH = 160 },
                new DataGridAttr() { HEADER = "Merek", BINDING = "MEREK", WIDTH = 130 },
                new DataGridAttr() { HEADER = "Qty", BINDING = "JUMLAH_STOK", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Satuan", BINDING = "SATUAN", WIDTH = 110 },
                new DataGridAttr() { HEADER = "Harga Beli", BINDING = "HARGA_BELI", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Harga Jual", BINDING = "HARGA_JUAL", WIDTH = 155 },
                new DataGridAttr() { HEADER = "Bayar", BINDING = "METODE_BAYAR", WIDTH = 125 },
                new DataGridAttr() { HEADER = "Ref / No. Nota", BINDING = "NO_NOTA_REF", WIDTH = 120 }
            };
            var tableStok = new DataTable();
            var stokColumns = new List<string>() { "TGL_MASUK", "NAMA_KATEGORI", "MEREK", "JUMLAH_STOK", "SATUAN",
               "NO_NOTA_REF", "HARGA_BELI", "HARGA_JUAL", "METODE_BAYAR" };
            TableDetailBeli.Columns.Clear();
            foreach (var value in columnList)
            {
                DataGridTextColumn columns = new DataGridTextColumn
                {
                    Header = value.HEADER,
                    Width = value.WIDTH,
                    Binding = new Binding(value.BINDING)
                };
                TableDetailBeli.Columns.Add(columns);
            }
            foreach (var value in stokColumns) { tableStok.Columns.Add(value); }
            foreach (var value in dataStok)
            {
                if (deskripsi == "") deskripsi = $"{value.KATEGORI} {value.MEREK}";
                var row = tableStok.NewRow();
                row["TGL_MASUK"] = value.TGL_MASUK;
                row["NAMA_KATEGORI"] = value.KATEGORI;
                row["MEREK"] = value.MEREK;
                row["JUMLAH_STOK"] = value.JUMLAH_STOK;
                row["SATUAN"] = satuan;
                row["NO_NOTA_REF"] = value.NO_NOTA_REF;
                row["HARGA_BELI"] = string.Format(culture, "{0:C0}", value.HARGA_BELI);
                row["HARGA_JUAL"] = $"{string.Format(culture, "{0:C0}", value.HARGA_JUAL)}/{value.SATUAN_JUAL}";
                row["METODE_BAYAR"] = value.METODE_BAYAR;
                tableStok.Rows.Add(row);
            }
            TableDetailBeli.ItemsSource = null;
            TableDetailBeli.ItemsSource = tableStok.AsDataView();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}