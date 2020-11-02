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
    public partial class Tampilan_Tabel : Window
    {
        readonly List<ItemSelect> selectItems = new List<ItemSelect>();
        public List<ItemSelect> selectedItems = new List<ItemSelect>();

        public Tampilan_Tabel()
        {
            InitializeComponent();
            GenerateHeader();
        }

        public void GenerateHeader()
        {
            var listHeader = new string[] { "No", "Merek", "Kategori", "Tanggal Masuk", "Qty", "Satuan",
                                            "Dalam Gram", "Deskripsi", "Ref. / No. Nota", "Harga Beli", "Harga Jual", "Metode Bayar", "Nama Supplier" };
            var listBinding = new string[] { "NO", "MEREK", "NAMA_KATEGORI", "TGL_MASUK", "JUMLAH_STOK", "SATUAN", "DALAM_GRAM", "DESKRIPSI",
                                             "NO_NOTA_REF", "HARGA_BELI", "HARGA_JUAL", "METODE_BAYAR", "NAMA_SUPPLIER" };
            var listWidth = new int[] { 50, 150, 150, 140, 80, 120, 100, 210, 100, 120, 170, 120, 200 };

            for (int i = 0; i < listHeader.Length; i++)
            {
                selectItems.Add(new ItemSelect { IsSelected = false, HEADER = listHeader[i], BINDING = listBinding[i], WIDTH = listWidth[i] });
            }

            dataGridTampilanTabel.ItemsSource = null;
            dataGridTampilanTabel.ItemsSource = selectItems;
        }

        private void BtnOke_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count != 0) DialogResult = true;
            else MessageBox.Show("Pilih Kolom Yang Akan Ditampilkan!", "Tidak Ada Kolom Dipilih", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnBatal_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Yakin Tidak Jadi Mengatur Tabel?", "Konfirmasi Pembatalan", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var chk = (CheckBox)sender;
            var newValue = !chk.IsChecked.GetValueOrDefault();

            chk.IsChecked = newValue;

            if (newValue) selectedItems.Add((ItemSelect)chk.DataContext);
            else selectedItems.Remove((ItemSelect)chk.DataContext);

            RefreshDisplay();
            e.Handled = true;
        }

        private void RefreshDisplay()
        {
            string kolomRaw = "";
            if (selectedItems.Count != 0)
            {
                int i = 0;
                foreach (var kolom in selectedItems)
                {
                    if (i == 0) kolomRaw = kolom.HEADER;
                    else kolomRaw += ", " + kolom.HEADER;
                    i++;
                }

                txtKolomDipilih.Text = kolomRaw;
            }
        }
    }
    public class ItemSelect
    {
        public bool IsSelected { get; set; }
        public string HEADER { get; set; }
        public string BINDING { get; set; }
        public int WIDTH { get; set; }

        public override string ToString()
        {
            return $"{HEADER}{BINDING}{WIDTH}";
        }
    }
}
