using SQLite;
using Stokuntan.Classes;
using Stokuntan.DatabaseModel;
using System;
using System.Collections.Generic;
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
    public partial class Tampilan_Reject : Window
    {
        public Tampilan_Reject(SQLiteConnection db)
        {
            InitializeComponent();
            var dataReject = db.Query<TabelReject>("SELECT * FROM TabelReject");
            if (dataReject.Count != 0)
            {
                var columnList = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Tanggal Reject", BINDING = "TANGGAL_REJECT", WIDTH = 110 },
                new DataGridAttr() { HEADER = "Nama Stok", BINDING = "NAMA_STOK", WIDTH = 210 },
                new DataGridAttr() { HEADER = "Harga Jual", BINDING = "HARGA_JUAL", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Qty (Gram)", BINDING = "STOK_DLM_GRAM", WIDTH = 100 },
                new DataGridAttr() { HEADER = "Total Rugi", BINDING = "TOTAL_RUGI", WIDTH = 130 },
                new DataGridAttr() { HEADER = "Sebab", BINDING = "SEBAB_REJECT", WIDTH = 80 }
            };
                TableReject.Columns.Clear();
                foreach (var value in columnList)
                {
                    DataGridTextColumn columns = new DataGridTextColumn
                    {
                        Header = value.HEADER,
                        Width = value.WIDTH,
                        Binding = new Binding(value.BINDING)
                    };
                    TableReject.Columns.Add(columns);
                }
                TableReject.ItemsSource = null;
                TableReject.ItemsSource = dataReject;
                TableReject.Visibility = Visibility.Visible;
                NoDataReject.Visibility = Visibility.Collapsed;
            }
            else 
            {
                TableReject.Visibility = Visibility.Collapsed;
                NoDataReject.Visibility = Visibility.Visible;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
