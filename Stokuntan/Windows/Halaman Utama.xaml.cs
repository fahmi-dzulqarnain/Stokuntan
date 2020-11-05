using SQLite;
using SQLiteNetExtensions.Extensions;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stokuntan.Windows
{
    public partial class Halaman_Utama : Window
    {
        readonly SQLiteConnection db = new IDatabase().Conn();
        List<TabelStok> DataStok; List<TabelRealStok> DataRealStok; List<TabelPenjualan> DataPenjualan;
        DataTable tablePenjualan;
        readonly List<DataGridAttr> DataGridAttrsStok, DataGridAttrsJual, DataGridAttrsPenjualan;
        readonly List<DetailPenjualan> Penjualans;
        readonly List<DataGridAttr> DataGridAttrs;
        readonly List<MinJualTemp> jualItemTemp;
        readonly CultureInfo culture = new CultureInfo("id-ID");
        int hargaJual = 0; int hargaBeli = 0; int jmlhJual = 0; double discount = 1;
        bool isUbahPembelian = false; object[] item;
        readonly Storyboard storyBoardIn, storyBoardOut, openPanel, closePanel;
        public static double ScreenWidth = SystemParameters.MaximizedPrimaryScreenWidth;

        public Halaman_Utama()
        {
            InitializeComponent();
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            ShowUI(GridBeranda);
            HideUI(GridPembelian);
            HideUI(GridStok);
            HideUI(GridPenjualan);
            HideUI(GridLaporan);
            DataGridAttrs = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "No", BINDING = "NO", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Tanggal Beli", BINDING = "TGL_MASUK", WIDTH = 130 },
                new DataGridAttr() { HEADER = "Kategori Barang", BINDING = "NAMA_KATEGORI", WIDTH = 170 },
                new DataGridAttr() { HEADER = "Merek", BINDING = "MEREK", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Qty", BINDING = "JUMLAH_STOK", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Satuan", BINDING = "SATUAN", WIDTH = 110 },
                new DataGridAttr() { HEADER = "Harga Beli", BINDING = "HARGA_BELI", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Harga Jual", BINDING = "HARGA_JUAL", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Deskripsi", BINDING = "DESKRIPSI", WIDTH = 210 },
                new DataGridAttr() { HEADER = "Ref / No. Nota", BINDING = "NO_NOTA_REF", WIDTH = 140 }
            };
            DataGridAttrsStok = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "No", BINDING = "NO", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Nama / Kategori Barang", BINDING = "KATEGORI_MEREK", WIDTH = 220 },
                new DataGridAttr() { HEADER = "Total Stok", BINDING = "TOTAL_STOK", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Satuan", BINDING = "SATUAN_STOK", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Stok Dlm Gram", BINDING = "STOK_DLM_GRAM", WIDTH = 160 },
                new DataGridAttr() { HEADER = "Harga Jual", BINDING = "HARGA_JUAL_PER", WIDTH = 200 },
                new DataGridAttr() { HEADER = "Lokasi", BINDING = "LOKASI", WIDTH = 210 }
            };
            DataGridAttrsJual = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Nama / Kategori Barang", BINDING = "NAMA_BARANG", WIDTH = 210 },
                new DataGridAttr() { HEADER = "Qty", BINDING = "JUMLAH_BELI", WIDTH = 80 },
                new DataGridAttr() { HEADER = "Harga Satuan", BINDING = "HARGA_SATUAN", WIDTH = 170 },
                new DataGridAttr() { HEADER = "Diskon", BINDING = "DISKON", WIDTH = 80 },
                new DataGridAttr() { HEADER = "Total", BINDING = "HARGA_TOTAL", WIDTH = 170 }
            };
            DataGridAttrsPenjualan = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "No", BINDING = "NO", WIDTH = 50 },
                new DataGridAttr() { HEADER = "Tanggal Jual", BINDING = "TANGGAL_JUAL", WIDTH = 100 },
                new DataGridAttr() { HEADER = "Customer", BINDING = "CUSTOMER", WIDTH = 160 },
                new DataGridAttr() { HEADER = "Total Penjualan", BINDING = "TOTAL_PENJUALAN", WIDTH = 180 }
            };
            Penjualans = new List<DetailPenjualan>();
            jualItemTemp = new List<MinJualTemp>();
            DataContext = new Classes.ComboBoxItem();
            storyBoardIn = (Storyboard)Resources["fadeIn"];
            storyBoardOut = (Storyboard)Resources["fadeOut"];
            openPanel = (Storyboard)Resources["TambahOpen"];
            closePanel = (Storyboard)Resources["TambahClose"];
            InitiateAddSaleAnimation();
            BacaDatabase();
        }

        #region FUNGSI UMUM

        private void KeluarButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Minimized) WindowState = WindowState.Minimized;
        }

        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                iconRestore.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                iconRestore.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
            }
        }

        private void BacaDatabase()
        {
            GenerateColumnDataGrid();
            DataStok = db.Query<TabelStok>("SELECT * FROM TabelStok");
            DataRealStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok");
            DataPenjualan = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan");

            var stokColumns = new List<string>() { "NO", "ID_STOK", "TGL_MASUK", "NAMA_KATEGORI", "DESKRIPSI", "MEREK", "JUMLAH_STOK", "SATUAN",
                "DALAM_GRAM", "NO_NOTA_REF", "HARGA_BELI", "HARGA_JUAL", "METODE_BAYAR", "NAMA_SUPPLIER", "SATUAN_JUAL" };
            var realColumns = new List<string>() { "NO", "ID_STOK_END", "KATEGORI_MEREK", "TOTAL_STOK", "SATUAN_STOK", "STOK_DLM_GRAM", "HARGA_JUAL_PER", "LOKASI" };
            var jualColumns = new List<string>() { "NO", "ID_JUAL", "TANGGAL_JUAL", "CUSTOMER", "TOTAL_PENJUALAN" };
            var tableStok = new DataTable();
            var tableRealStok = new DataTable();
            tablePenjualan = new DataTable();
            foreach (var value in stokColumns) { tableStok.Columns.Add(value); }
            foreach (var value in realColumns) { tableRealStok.Columns.Add(value); }
            foreach (var value in jualColumns) { tablePenjualan.Columns.Add(value); }
            var buttonColumn = (DataGridTemplateColumn)TableStok.Resources["BtnDetBeli"];
            TableStok.Columns.Add(buttonColumn);
            var buttonDetJual = (DataGridTemplateColumn)TablePenjualan.Resources["BtnDetJual"];
            TablePenjualan.Columns.Add(buttonDetJual);
            var buttonHapusJual = (DataGridTemplateColumn)TableTambahPenjualan.Resources["BtnHapusJual"];
            TableTambahPenjualan.Columns.Add(buttonHapusJual);

            int i = 1;
            foreach (var value in DataStok)
            {
                var satuan = db.Get<TabelSatuan>(value.SATUAN);
                var supplier = db.Get<TabelSupplier>(value.ID_SUPPLIER);
                var deskripsi = value.DESKRIPSI;
                if (deskripsi == "") deskripsi = $"{value.KATEGORI} {value.MEREK}";
                var row = tableStok.NewRow();
                row["NO"] = i;
                row["ID_STOK"] = value.ID_STOK;
                row["TGL_MASUK"] = value.TGL_MASUK;
                row["NAMA_KATEGORI"] = value.KATEGORI;
                row["DESKRIPSI"] = deskripsi;
                row["MEREK"] = value.MEREK;
                row["JUMLAH_STOK"] = value.JUMLAH_STOK;
                row["SATUAN"] = value.SATUAN;
                row["DALAM_GRAM"] = satuan.DALAM_GRAM;
                row["NO_NOTA_REF"] = value.NO_NOTA_REF;
                row["HARGA_BELI"] = string.Format(culture, "{0:C0}", value.HARGA_BELI);
                row["HARGA_JUAL"] = $"{string.Format(culture, "{0:C0}", value.HARGA_JUAL)}/{value.SATUAN_JUAL}";
                row["METODE_BAYAR"] = value.METODE_BAYAR;
                row["NAMA_SUPPLIER"] = supplier.NAMA_SUPPLIER;
                row["SATUAN_JUAL"] = value.SATUAN_JUAL;
                tableStok.Rows.Add(row);
                i++;
            }
            i = 1;
            foreach (var value in DataRealStok)
            {
                var row = tableRealStok.NewRow();
                var hrgJual = value.HARGA_JUAL_PER.Split('/');
                row["NO"] = i++;
                row["ID_STOK_END"] = value.ID_STOK_END;
                row["KATEGORI_MEREK"] = value.KATEGORI_MEREK;
                row["TOTAL_STOK"] = string.Format(culture, "{0:N0}", value.TOTAL_STOK);
                row["SATUAN_STOK"] = value.SATUAN_STOK;
                row["STOK_DLM_GRAM"] = string.Format(culture, "{0:N0}", value.STOK_DLM_GRAM);
                row["HARGA_JUAL_PER"] = $"{string.Format(culture, "{0:N0}", int.Parse(hrgJual[0]))}/{hrgJual[1]}";
                row["LOKASI"] = value.LOKASI;
                tableRealStok.Rows.Add(row);
            }
            i = 1;
            foreach (var value in DataPenjualan)
            {
                var row = tablePenjualan.NewRow();
                var customer = db.Query<TabelCustomer>("SELECT * FROM TabelCustomer WHERE ID_CUSTOMER = ?", value.CUSTOMER).FirstOrDefault();
                row["NO"] = i++;
                row["ID_JUAL"] = value.ID_JUAL;
                row["TANGGAL_JUAL"] = value.TANGGAL_JUAL;
                row["CUSTOMER"] = customer.NAMA_CUSTOMER;
                row["TOTAL_PENJUALAN"] = value.TOTAL_PENJUALAN;
                tablePenjualan.Rows.Add(row);
            }

            if (DataStok.Count == 0)
            {
                TablePembelian.ItemsSource = null;
                TablePembelian.Visibility = Visibility.Collapsed;
                NoDataPembelian.Visibility = Visibility.Visible;
            }
            else
            {
                TablePembelian.ItemsSource = null;
                TablePembelian.ItemsSource = tableStok.AsDataView();
                TablePembelian.Visibility = Visibility.Visible;
                NoDataPembelian.Visibility = Visibility.Collapsed;
            }

            if (DataRealStok.Count == 0)
            {
                TableStok.ItemsSource = null;
                TableStok.Visibility = Visibility.Collapsed;
                NoDataStok.Visibility = Visibility.Visible;
            }
            else
            {
                TableStok.ItemsSource = null;
                TableStok.ItemsSource = tableRealStok.AsDataView();
                TableStok.Visibility = Visibility.Visible;
                NoDataStok.Visibility = Visibility.Collapsed;
            }

            if (DataPenjualan.Count == 0)
            {
                TablePenjualan.ItemsSource = null;
                TablePenjualan.Visibility = Visibility.Collapsed;
                NoDataPenjualan.Visibility = Visibility.Visible;
            }
            else
            {
                TablePenjualan.ItemsSource = null;
                TablePenjualan.ItemsSource = tablePenjualan.AsDataView();
                TablePenjualan.Visibility = Visibility.Visible;
                NoDataPenjualan.Visibility = Visibility.Collapsed;
            }

            var akun = db.Query<TabelAkun>("SELECT * FROM TabelAkun").First();
            var saldoKas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First().TOTAL;
            var saldoUtang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First().TOTAL;
            var saldoPiutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First().TOTAL;
            txtSaldoKas.Text = string.Format(culture, "{0:C0}", saldoKas);
            txtSaldoUtang.Text = string.Format(culture, "{0:C0}", saldoUtang);
            txtSaldoPiutang.Text = string.Format(culture, "{0:C0}", saldoPiutang);
            txtNamaUsaha.Text = akun.NAMA_TOKO;
            txtAlamat.Text = akun.ALAMAT;
            txtKontakUsaha.Text = akun.KONTAK;
        }

        private void GenerateColumnDataGrid()
        {
            TablePembelian.Columns.Clear();
            TableStok.Columns.Clear();
            TablePenjualan.Columns.Clear();
            TableTambahPenjualan.Columns.Clear();
            foreach (var value in DataGridAttrs)
            {
                DataGridTextColumn columns = new DataGridTextColumn
                {
                    Header = value.HEADER,
                    Width = value.WIDTH,
                    Binding = new Binding(value.BINDING)
                };
                TablePembelian.Columns.Add(columns);
            }
            foreach (var value in DataGridAttrsStok)
            {
                DataGridTextColumn columns = new DataGridTextColumn
                {
                    Header = value.HEADER,
                    Width = value.WIDTH,
                    Binding = new Binding(value.BINDING)
                };
                TableStok.Columns.Add(columns);
            }
            foreach (var value in DataGridAttrsJual)
            {
                DataGridTextColumn columns = new DataGridTextColumn
                {
                    Header = value.HEADER,
                    Width = value.WIDTH,
                    Binding = new Binding(value.BINDING)
                };
                TableTambahPenjualan.Columns.Add(columns);
            }
            foreach (var value in DataGridAttrsPenjualan)
            {
                DataGridTextColumn columns = new DataGridTextColumn
                {
                    Header = value.HEADER,
                    Width = value.WIDTH,
                    Binding = new Binding(value.BINDING)
                };
                TablePenjualan.Columns.Add(columns);
            }
        }

        private void ShowUI(UIElement uIElement)
        {
            uIElement.Visibility = Visibility.Visible;
        }

        private void HideUI(UIElement uIElement)
        {
            uIElement.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region MENU SECTION

        private void BtnPembelian_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardOut.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardIn.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridPenjualan);
        }

        private void BtnStok_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardOut.Begin(GridBeranda);
            storyBoardIn.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridPenjualan);
        }

        private void BtnPenjualan_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardOut.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardIn.Begin(GridPenjualan);
        }

        private void BtnLaporan_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            storyBoardOut.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardIn.Begin(GridLaporan);
            storyBoardOut.Begin(GridPenjualan);
        }

        private void BtnBeranda_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardIn.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridPenjualan);
        }

        #endregion

        #region PEMBELIAN

        private void BtnUbahPembelian_Click(object sender, RoutedEventArgs e)
        {
            var rawItem = (DataRowView)TablePembelian.SelectedItem;
            var openPanel = (Storyboard)Resources["TambahOpen"];
            if (rawItem != null)
            {
                openPanel.Begin(GridTambahPembelian);
                item = rawItem.Row.ItemArray;
                var tgl = item[2].ToString().Split('/');
                txtTitle.Text = "Ubah Pembelian";
                txtBoxTglMasuk.SelectedDate = DateTime.Parse($"{tgl[1]}/{tgl[0]}/{tgl[2]}");
                cmbBoxKategori.SelectedItem = item[3].ToString();
                txtBoxDeskripsi.Text = item[4].ToString();
                txtBoxMerek.Text = item[5].ToString();
                txtBoxJumlahStok.Text = item[6].ToString();
                cmbBoxSatuan.SelectedItem = item[7].ToString();
                txtBoxRefNota.Text = item[9].ToString();
                txtBoxHargaBeli.Text = item[10].ToString().Replace("Rp", "");
                txtBoxHargaJual.Text = item[11].ToString().Split('/')[0].Replace("Rp", "");
                cmbBoxMetodeBayar.Text = item[12].ToString();
                cmbBoxSupplier.Text = item[13].ToString();
                cmbBoxSatuanJual.SelectedItem = item[14].ToString();
                btnTambahPembelian2.Content = "Ubah Pembelian";
                isUbahPembelian = true;
            }
            else MessageBox.Show("Pilih Data Yang Akan Diubah!", "Tidak ada data dipilih", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnTambahPembelian_Click(object sender, RoutedEventArgs e)
        {
            openPanel.Begin(GridTambahPembelian);
            isUbahPembelian = false;
            txtTitle.Text = "Tambah Pembelian";
            btnTambahPembelian2.Content = "Tambah Pembelian";
        }

        private void BtnTampilanTabel_Click(object sender, RoutedEventArgs e)
        {
            var window = new Tampilan_Tabel();
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                DataGridAttrs.Clear();

                foreach (var value in window.selectedItems)
                {
                    DataGridAttr columns = new DataGridAttr
                    {
                        HEADER = value.HEADER,
                        WIDTH = value.WIDTH,
                        BINDING = value.BINDING
                    };
                    DataGridAttrs.Add(columns);
                }

                BacaDatabase();
                window.Close();
            }
        }

        //private void BtnEksporExcel_Click(object sender, RoutedEventArgs e)
        //{

        //}

        private void BtnHapusPembelian_Click(object sender, RoutedEventArgs e)
        {
            var item = (DataRowView)TablePembelian.SelectedItem;
            if (item != null)
            {
                var result = MessageBox.Show("Yakin ingin menghapus data pembelian? Stok yang sebelumnya pernah dimasukkan akibat pembelian ini akan ikut terhapus, serta kas yang dikeluarkan juga akan dikembalikan", "Konfirmasi Penghapusan", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var itemDelete = db.Query<TabelStok>("SELECT * FROM TabelStok WHERE ID_STOK = ?", item.Row.ItemArray[1]).FirstOrDefault();
                    var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND SATUAN_STOK = ?", itemDelete.DESKRIPSI, itemDelete.SATUAN).FirstOrDefault();
                    var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", itemDelete.SATUAN).FirstOrDefault().DALAM_GRAM * itemDelete.JUMLAH_STOK;
                    isAvailable.TOTAL_STOK -= itemDelete.JUMLAH_STOK;
                    isAvailable.STOK_DLM_GRAM -= stokGram;
                    db.RunInTransaction(() => db.Update(isAvailable));
                    if (itemDelete.METODE_BAYAR.Contains("Cash"))
                    {
                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL += itemDelete.HARGA_BELI; db.Update(kas); });
                    }
                    else if (itemDelete.METODE_BAYAR.Contains("Utang"))
                    {
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        db.RunInTransaction(() => { utang.TOTAL -= itemDelete.HARGA_BELI; db.Update(utang); });
                    }
                    db.Delete(itemDelete);
                    BacaDatabase();
                }
            }
            else MessageBox.Show("Pilih Data Yang Akan Dihapus!", "Tidak ada data dipilih", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void TxtBoxCariPembelian_KeyUp(object sender, KeyEventArgs e)
        {
            var filtered = DataStok.Where(aset => aset.DESKRIPSI.ToLower().Contains(txtBoxCariPembelian.Text));
            var parameter = cmbBoxKategoriCari.SelectedItem.ToString();

            if (parameter.Equals("Tanggal Masuk")) filtered = DataStok.Where(aset => aset.TGL_MASUK.ToLower().Contains(txtBoxCariPembelian.Text));
            if (parameter.Equals("Kategori")) filtered = DataStok.Where(aset => aset.KATEGORI.ToLower().Contains(txtBoxCariPembelian.Text));
            if (parameter.Equals("Supplier")) {
                var idSupplier = db.Query<TabelSupplier>("SELECT * FROM TabelSupplier").Where(x => x.NAMA_SUPPLIER.ToLower().Contains(txtBoxCariPembelian.Text)).FirstOrDefault().ID_SUPPLIER;
                filtered = DataStok.Where(aset => aset.ID_SUPPLIER.ToString().Contains(idSupplier.ToString())); }
            if (parameter.Equals("Merek")) filtered = DataStok.Where(aset => aset.MEREK.ToLower().Contains(txtBoxCariPembelian.Text));
            if (parameter.Equals("Deskripsi")) filtered = DataStok.Where(aset => aset.DESKRIPSI.ToLower().Contains(txtBoxCariPembelian.Text));

            TablePembelian.ItemsSource = filtered;
        }

        private void CloseTambahButton_Click(object sender, RoutedEventArgs e)
        {
            closePanel.Begin(GridTambahPembelian);
            ResetTambahPembelian();
        }

        private void BtnTambahPembelian2_Click(object sender, RoutedEventArgs e)
        {
            var closePanel = (Storyboard)Resources["TambahClose"];
            if (!int.TryParse(txtBoxJumlahStok.Text, out int jumlah)) MessageBox.Show("Jumlah Stok harus berupa angka!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            if (cmbBoxKategori.SelectedItem != null && txtBoxTglMasuk.Text != "" && cmbBoxLokasi.SelectedItem != null &&
                txtBoxMerek.Text != "" && txtBoxJumlahStok.Text != "" && cmbBoxSatuan.SelectedItem != null &&
                txtBoxHargaBeli.Text != "" && txtBoxHargaJual.Text != "" && cmbBoxSupplier.SelectedItem != null)
            {
                var maxBeli = db.Query<TabelStok>("SELECT * FROM TabelStok");
                var maxStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok");
                var idStok = 1; var id = 1;
                if (maxStok.Count != 0) idStok = maxStok.Max(x => x.ID_STOK_END) + 1;
                if (maxBeli.Count != 0) id = maxBeli.Max(x => x.ID_STOK) + 1;
                var supplier = db.Query<TabelSupplier>("SELECT * FROM TabelSupplier WHERE NAMA_SUPPLIER = ?", cmbBoxSupplier.SelectedItem.ToString()).FirstOrDefault();
                var no_nota = "-"; var kategori = cmbBoxKategori.SelectedItem.ToString(); var merek = txtBoxMerek.Text; var deskripsi = $"{kategori} {merek}";
                var jmlhStok = int.Parse(txtBoxJumlahStok.Text); var satuanStok = cmbBoxSatuan.SelectedItem.ToString();
                var satuanJual = cmbBoxSatuanJual.SelectedItem.ToString();
                var metode = cmbBoxMetodeBayar.SelectedItem.ToString();
                var lokasi = cmbBoxLokasi.SelectedItem.ToString();
                if (txtBoxRefNota.Text != "") no_nota = txtBoxRefNota.Text;
                if (txtBoxDeskripsi.Text != "") deskripsi = txtBoxDeskripsi.Text;

                if (isUbahPembelian == false)
                {
                    db.Insert(new TabelStok()
                    {
                        ID_STOK = id,
                        DESKRIPSI = deskripsi,
                        KATEGORI = kategori,
                        TGL_MASUK = txtBoxTglMasuk.SelectedDate.Value.ToString("dd/MM/yyyy"),
                        MEREK = merek,
                        JUMLAH_STOK = jmlhStok,
                        HARGA_BELI = hargaBeli,
                        HARGA_JUAL = hargaJual,
                        SATUAN_JUAL = satuanJual,
                        SATUAN = satuanStok,
                        METODE_BAYAR = metode,
                        ID_SUPPLIER = supplier.ID_SUPPLIER,
                        NO_NOTA_REF = no_nota
                    });

                    var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND SATUAN_STOK = ?", deskripsi, satuanStok).FirstOrDefault();
                    var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", satuanStok).FirstOrDefault().DALAM_GRAM * jmlhStok;
                    if (isAvailable == null)
                    {
                        db.Insert(new TabelRealStok()
                        {
                            ID_STOK_END = idStok,
                            KATEGORI_MEREK = deskripsi,
                            TOTAL_STOK = jmlhStok,
                            SATUAN_STOK = satuanStok,
                            LOKASI = lokasi,
                            STOK_DLM_GRAM = stokGram,
                            HARGA_JUAL_PER = $"{hargaJual}/{satuanJual}"
                        });
                    }
                    else
                    {
                        isAvailable.TOTAL_STOK += jmlhStok;
                        isAvailable.STOK_DLM_GRAM += stokGram;
                        isAvailable.HARGA_JUAL_PER = $"{hargaJual}/{satuanJual}";
                        db.RunInTransaction(() => db.Update(isAvailable));
                    }

                    if (metode.Contains("Cash"))
                    {
                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL -= hargaBeli; db.Update(kas); });
                    }
                    else if (metode.Contains("Utang"))
                    {
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        db.RunInTransaction(() => { utang.TOTAL += hargaBeli; db.Update(utang); });
                    }
                }
                else
                {
                    var table = db.Get<TabelStok>(item[1]);
                    var jmlhStokUpdate = table.JUMLAH_STOK;
                    var jmlhKasUpdate = table.HARGA_BELI;
                    hargaBeli = int.Parse(txtBoxHargaBeli.Text.Replace(".", ""), NumberStyles.AllowCurrencySymbol);
                    hargaJual = int.Parse(txtBoxHargaJual.Text.Replace(".", ""), NumberStyles.AllowCurrencySymbol);
                    table.DESKRIPSI = deskripsi;
                    table.HARGA_BELI = hargaBeli;
                    table.HARGA_JUAL = hargaJual;
                    table.KATEGORI = kategori;
                    table.TGL_MASUK = txtBoxTglMasuk.SelectedDate.Value.ToString("dd/MM/yyyy");
                    table.MEREK = merek;
                    table.JUMLAH_STOK = jmlhStok;
                    table.SATUAN_JUAL = cmbBoxSatuanJual.SelectedItem.ToString();
                    table.SATUAN = satuanStok;
                    table.METODE_BAYAR = cmbBoxMetodeBayar.SelectedItem.ToString();
                    table.ID_SUPPLIER = supplier.ID_SUPPLIER;
                    table.NO_NOTA_REF = no_nota;  
                    db.RunInTransaction(() => db.Update(table));

                    var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND SATUAN_STOK = ?", deskripsi, satuanStok);
                    var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", satuanStok).FirstOrDefault().DALAM_GRAM;
                    if (isAvailable.Count == 0)
                    {
                        db.Insert(new TabelRealStok()
                        {
                            ID_STOK_END = idStok,
                            KATEGORI_MEREK = deskripsi,
                            TOTAL_STOK = jmlhStok,
                            SATUAN_STOK = satuanStok,
                            STOK_DLM_GRAM = stokGram,
                            LOKASI = lokasi,
                            HARGA_JUAL_PER = $"{hargaJual}/{satuanJual}"
                        });
                    }
                    else
                    {
                        var updated = isAvailable.FirstOrDefault();
                        updated.ID_STOK_END = updated.ID_STOK_END;
                        updated.TOTAL_STOK -= jmlhStokUpdate;
                        updated.STOK_DLM_GRAM -= stokGram * jmlhStokUpdate;
                        updated.TOTAL_STOK += jmlhStok;
                        updated.STOK_DLM_GRAM += stokGram * jmlhStok;
                        updated.HARGA_JUAL_PER = $"{hargaJual}/{satuanJual}";
                        db.RunInTransaction(() => db.Update(updated));
                    }

                    if (metode.Contains("Cash"))
                    {
                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL += jmlhKasUpdate; kas.TOTAL -= hargaBeli; db.Update(kas); });
                    }
                    else if (metode.Contains("Utang"))
                    {
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        db.RunInTransaction(() => { utang.TOTAL -= jmlhKasUpdate; utang.TOTAL += hargaBeli; db.Update(utang); });
                    }
                }

                ResetTambahPembelian();
                BacaDatabase();
                closePanel.Begin(GridTambahPembelian);
            }
            else MessageBox.Show("Ada Kolom yang belum diisi, Mohon Diisi terlebih dahulu", "Mohon Diperhatikan!");
        }

        private void ResetTambahPembelian()
        {
            txtBoxDeskripsi.Text = "";
            txtBoxTglMasuk.Text = "";
            txtBoxMerek.Text = "";
            txtBoxRefNota.Text = "";
            txtBoxHargaBeli.Text = "";
            txtBoxHargaJual.Text = "";
            txtBoxJumlahStok.Text = "";
            cmbBoxKategori.SelectedItem = null;
            cmbBoxMetodeBayar.SelectedItem = null;
            cmbBoxSatuan.SelectedItem = null;
            cmbBoxSatuanJual.SelectedItem = null;
            cmbBoxSupplier.SelectedItem = null;
        }

        private void BtnAddLocation_Click(object sender, RoutedEventArgs e)
        {
            var window = new Tambah_Lokasi();
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                DataContext = new Classes.ComboBoxItem();
                cmbBoxLokasi.SelectedItem = window.txtBoxNamaLokasi.Text;
                window.Close();
            }
        }

        private void BtnAddKategori_Click(object sender, RoutedEventArgs e)
        {
            var window = new Tambah_Kategori();
            window.ShowDialog();

            if (window.DialogResult == true) {
                DataContext = new Classes.ComboBoxItem();
                cmbBoxKategori.SelectedItem = window.txtBoxNamaKategori.Text;
                window.Close();
            }
        }

        private void TxtBoxHargaBeli_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxHargaBeli.Text != "")
            {
                try
                {
                    hargaBeli = int.Parse(txtBoxHargaBeli.Text.Replace(".", ""), NumberStyles.AllowCurrencySymbol);
                    if (!string.IsNullOrEmpty(txtBoxHargaBeli.Text))
                    {
                        txtBoxHargaBeli.Text = string.Format(culture, "{0:N0}", hargaBeli);
                        txtBoxHargaBeli.Select(txtBoxHargaBeli.Text.Length, 0);
                    }
                }
                catch (Exception) { MessageBox.Show("Harga Beli harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void TxtBoxHargaJual_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxHargaJual.Text != "")
            {
                try
                {
                    hargaJual = int.Parse(txtBoxHargaJual.Text.Replace(".", ""), NumberStyles.AllowCurrencySymbol);
                    if (!string.IsNullOrEmpty(txtBoxHargaJual.Text))
                    {
                        txtBoxHargaJual.Text = string.Format(culture, "{0:N0}", hargaJual);
                        txtBoxHargaJual.Select(txtBoxHargaJual.Text.Length, 0);
                    }
                }
                catch (Exception) { MessageBox.Show("Harga Beli harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnAddSatuan_Click(object sender, RoutedEventArgs e)
        {
            var window = new Tambah_Satuan();
            window.ShowDialog();

            if (window.DialogResult == true) {
                DataContext = new Classes.ComboBoxItem();
                cmbBoxSatuan.SelectedItem = window.txtBoxNamaSatuan.Text;
                window.Close();
            }
        }

        private void BtnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            var window = new Tambah_Supplier();
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                DataContext = new Classes.ComboBoxItem();
                cmbBoxSupplier.SelectedItem = window.txtBoxNamaSupplier.Text;
                window.Close();
            }
        }

        #endregion

        #region STOK

        private void CloseTambahRejectButton_Click(object sender, RoutedEventArgs e)
        {
            var closePanel = (Storyboard)Resources["TambahClose"];
            closePanel.Begin(GridTambahReject);
        }

        private void BtnTambahReject_Click(object sender, RoutedEventArgs e)
        {
            var openPanel = (Storyboard)Resources["TambahOpen"];
            openPanel.Begin(GridTambahReject);
        }

        private void BtnDetailPembelian_Click(object sender, RoutedEventArgs e)
        {
            var selected = ((DataRowView)TableStok.SelectedItem).Row.ItemArray;
            var window = new Tampilan_Pembelian(selected[2].ToString(), selected[4].ToString(), db);
            window.ShowDialog();
        }

        private void BtnDaftarBarangReject_Click(object sender, RoutedEventArgs e)
        {
            new Tampilan_Reject(db).ShowDialog();
        }

        private void BtnTambahReject2_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBoxNamaStok.SelectedItem != null && cmbBoxHargaJual.SelectedItem != null && txtBoxTglReject.Text != "" &&
                txtBoxStokRejectDlmGr.Text != "" && TxtTotalRugi.Text != "")
            {
                var id = 1; var sebab = "";
                var tglReject = txtBoxTglReject.SelectedDate.Value.ToString("dd/MM/yyyy");
                var deskripsi = cmbBoxNamaStok.SelectedItem.ToString();
                var hrgJualPer = cmbBoxHargaJual.SelectedItem.ToString();
                var stokGram = txtBoxStokRejectDlmGr.Text;
                var forId = db.Query<TabelReject>("SELECT * FROM TabelReject");
                var getStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND HARGA_JUAL_PER = ?", deskripsi, hrgJualPer.Replace(".", "")).FirstOrDefault();
                var satuanStok = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", getStok.SATUAN_STOK).FirstOrDefault();

                if (forId.Count != 0) id = forId.Max(x => x.ID_REJECT) + 1;
                if (cmbBoxSebabReject.SelectedItem != null) sebab = cmbBoxSebabReject.SelectedItem.ToString();

                db.Insert(new TabelReject() { ID_REJECT = id, HARGA_JUAL = hrgJualPer, NAMA_STOK = deskripsi, SEBAB_REJECT = sebab, STOK_DLM_GRAM = stokGram, TANGGAL_REJECT = tglReject, TOTAL_RUGI = TxtTotalRugi.Text });
                getStok.STOK_DLM_GRAM -= int.Parse(stokGram.Replace(".", ""));
                getStok.TOTAL_STOK = getStok.STOK_DLM_GRAM / satuanStok.DALAM_GRAM;
                db.RunInTransaction(() => db.Update(getStok));
                BacaDatabase();

                var closePanel = (Storyboard)Resources["TambahClose"];
                closePanel.Begin(GridTambahReject);
            }
            else MessageBox.Show("Field nama stok, harga jual, tanggal reject, atau stok reject tidak boleh kosong", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BtnAlihkanStok_Click(object sender, RoutedEventArgs e)
        {
            var window = new Alihkan_Stok();
            window.ShowDialog();
            if (window.DialogResult == true) BacaDatabase();
        }

        private void CmbBoxNamaStok_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxNamaStok.SelectedItem != null)
            {
                cmbBoxHargaJual.Items.Clear();
                var hargaJualItem = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ?", cmbBoxNamaStok.SelectedItem.ToString());
                if (hargaJualItem.Count != 0) foreach (var value in hargaJualItem)
                    {
                        var price = value.HARGA_JUAL_PER.Split('/');
                        cmbBoxHargaJual.Items.Add($"{string.Format(culture, "{0:N0}", int.Parse(price[0]))}/{price[1]}");
                    }
            }
        }

        private void TxtBoxStokRejectDlmGr_TextChanged(object sender, TextChangedEventArgs e)
        {
            var stokDlmGr = txtBoxStokRejectDlmGr.Text;
            if (stokDlmGr != "")
            {
                try
                {
                    var value = int.Parse(txtBoxStokRejectDlmGr.Text.Replace(".", ""));
                    txtBoxStokRejectDlmGr.Text = string.Format(culture, "{0:N0}", value);
                    txtBoxStokRejectDlmGr.Select(txtBoxStokRejectDlmGr.Text.Length, 0);
                    if (cmbBoxHargaJual.SelectedItem != null)
                    {
                        var price = cmbBoxHargaJual.SelectedItem.ToString().Split('/');
                        var satuan = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", price[1]).FirstOrDefault().DALAM_GRAM;
                        TxtTotalRugi.Text = string.Format(culture, "{0:C0}", (int.Parse(price[0].Replace(".", "")) / satuan * value));
                    }
                }
                catch (Exception) { MessageBox.Show("Stok dalam gram harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void TxtBoxCariStok_KeyUp(object sender, KeyEventArgs e)
        {
            var param = TxtBoxCariStok.Text;
            if (!param.Equals("")) TableStok.ItemsSource = DataRealStok.Where(x => x.KATEGORI_MEREK.ToLower().Contains(param));
            else BacaDatabase();
        }

        #endregion

        #region PENJUALAN

        private void TxtBoxCariPenjualan_KeyUp(object sender, KeyEventArgs e)
        {
            var searchString = txtBoxCariPenjualan.Text;
            if (searchString != "")
            {
                var filtered = tablePenjualan.AsEnumerable().Where(r => r.Field<string>("CUSTOMER").ToLower().Contains(searchString)); ;
                var parameter = "CUSTOMER";
                if (cmbBoxKategoriCariPenjualan.SelectedItem != null) parameter = cmbBoxKategoriCariPenjualan.SelectedItem.ToString();

                if (parameter.Equals("Tanggal Transaksi")) filtered = tablePenjualan.AsEnumerable().Where(r => r.Field<string>("TANGGAL_JUAL").ToLower().Contains(searchString));
                else if (parameter.Equals("Customer")) filtered = tablePenjualan.AsEnumerable().Where(r => r.Field<string>("CUSTOMER").ToLower().Contains(searchString));

                TablePenjualan.ItemsSource = null;
                TablePenjualan.ItemsSource = filtered.AsDataView();
            }
            else BacaDatabase();
        }

        private void BtnHapusPenjualan_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetTambahPenjualan()
        {
            Penjualans.Clear();
            txtBoxTglJual.Text = "";
            cmbBoxCustomer.SelectedItem = null;
            cmbBoxNamaBarang.SelectedItem = null;
            txtBoxJumlahBarang.Text = "";
            txtHargaSatuan.Text = "Rp0,00/Kilogram";
            txtBoxDiskon.Text = "";
            txtHargaTotal.Text = "";
            txtTotalHargaAll.Text = "Rp0,00";
        }

        private void BtnTambahPenjualan_Click(object sender, RoutedEventArgs e)
        {
            var openPanel = (Storyboard)Resources["JualOpen"];
            openPanel.Begin(GridTambahPenjualan);
        }

        private void CloseTambahJual_Click(object sender, RoutedEventArgs e)
        {
            var closePanel = (Storyboard)Resources["JualClose"];
            closePanel.Begin(GridTambahPenjualan);
            ResetTambahPenjualan();
        }

        private void InitiateAddSaleAnimation()
        {
            ((Storyboard)Resources["JualOpen"]).Begin(GridTambahPenjualan);
            ((Storyboard)Resources["JualClose"]).Begin(GridTambahPenjualan);
        }

        private void BtnTambahPenjualan2_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBoxCustomer.SelectedItem != null && txtBoxTglJual.Text != "" && cmbBoxLokasiJual.SelectedItem != null)
            {
                if (Penjualans.Count != 0)
                {
                    var detailJual = "";
                    var customer = db.Query<TabelCustomer>("SELECT * FROM TabelCustomer WHERE NAMA_CUSTOMER = ?", cmbBoxCustomer.SelectedItem.ToString()).FirstOrDefault();
                    var getJual = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan");
                    var metode = "Cash / Bank";
                    var totalJual = txtTotalHargaAll.Text;
                    var lokasi = cmbBoxLokasiJual.SelectedItem.ToString();
                    if (cmbBoxMetodeBayarJual.SelectedItem != null) metode = cmbBoxMetodeBayarJual.SelectedItem.ToString();
                    var id = 1;
                    if (getJual.Count != 0) id = getJual.Max(x => x.ID_JUAL) + 1;
                    foreach (var value in Penjualans)
                    {
                        if (detailJual == "") detailJual += $"{value}";
                        else detailJual += $"#{value}";
                    }
                    foreach (var value in jualItemTemp)
                    {
                        var getStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND LOKASI = ?", value.NAMA_BARANG, lokasi).FirstOrDefault();
                        getStok.TOTAL_STOK = value.JUMLAH_STOK;
                        getStok.STOK_DLM_GRAM = value.STOK_DLM_GRAM;
                        db.RunInTransaction(() => db.Update(getStok));
                    }
                    db.Insert(new TabelPenjualan() { ID_JUAL = id, CUSTOMER = customer.ID_CUSTOMER, DETAIL_JUAL = detailJual, TANGGAL_JUAL = txtBoxTglJual.SelectedDate.Value.ToString("dd/MM/yyyy"), 
                        TOTAL_PENJUALAN = totalJual, METODE_BAYAR = metode });

                    if (metode.Contains("Piutang"))
                    {
                        var piutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First();
                        db.RunInTransaction(() => { piutang.TOTAL += double.Parse(totalJual.Replace(".", "").Replace("Rp", "")); db.Update(piutang); });
                    }
                    else
                    {
                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL += double.Parse(totalJual.Replace(".", "").Replace("Rp", "")); db.Update(kas); });
                    }

                    BacaDatabase();
                    ((Storyboard)Resources["JualClose"]).Begin(GridTambahPenjualan);
                    ResetTambahPenjualan();
                }
                else MessageBox.Show("Tambahkan Item Dibeli Terlebih Dahulu Sebelum Menambahkan Penjualan", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("Anda harus Memilih Customer, Tanggal Jual, dan Lokasi Stok Untuk Melanjutkan", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAddSaleItem_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBoxNamaBarang.SelectedItem != null && txtBoxJumlahBarang.Text != "")
            {
                var namaBarang = cmbBoxNamaBarang.SelectedItem.ToString();
                var jumlahBeli = int.Parse(txtBoxJumlahBarang.Text);
                var hargaSatu = txtHargaSatuan.Text.Split('/');
                var getStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ?", namaBarang).FirstOrDefault();
                var satuan = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", hargaSatu[1]).FirstOrDefault();
                if (getStok.TOTAL_STOK >= jumlahBeli)
                {
                    var jualItem = new DetailPenjualan()
                    {
                        NAMA_BARANG = namaBarang,
                        HARGA_SATUAN = txtHargaSatuan.Text,
                        JUMLAH_BELI = jumlahBeli.ToString(),
                        DISKON = txtBoxDiskon.Text,
                        HARGA_TOTAL = txtHargaTotal.Text
                    };
                    Penjualans.Add(jualItem);

                    var totalStok = getStok.TOTAL_STOK -= jumlahBeli;
                    var stokGram = getStok.STOK_DLM_GRAM -= satuan.DALAM_GRAM * jumlahBeli;

                    if (jualItemTemp.Count != 0)
                    {
                        var stokTemp = jualItemTemp.Where(x => x.NAMA_BARANG == namaBarang);
                        if (stokTemp.Count() != 0)
                        {
                            var finalStok = stokTemp.FirstOrDefault();
                            if (finalStok.JUMLAH_STOK >= jumlahBeli)
                            {
                                finalStok.JUMLAH_STOK -= jumlahBeli;
                                finalStok.STOK_DLM_GRAM -= satuan.DALAM_GRAM * jumlahBeli;
                            }
                            else MessageBox.Show("Stok Item Tidak Mencukupi", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else jualItemTemp.Add(new MinJualTemp() { NAMA_BARANG = namaBarang, JUMLAH_STOK = totalStok, STOK_DLM_GRAM = stokGram });
                    }
                    else jualItemTemp.Add(new MinJualTemp() { NAMA_BARANG = namaBarang, JUMLAH_STOK = totalStok, STOK_DLM_GRAM = stokGram });

                    TableTambahPenjualan.ItemsSource = null;
                    TableTambahPenjualan.ItemsSource = Penjualans;
                    if (txtTotalHargaAll.Text == "Rp0,00") txtTotalHargaAll.Text = jualItem.HARGA_TOTAL;
                    else
                    {
                        var harTot = int.Parse(jualItem.HARGA_TOTAL.Replace(".", "").Replace("Rp", ""));
                        var harSem = int.Parse(txtTotalHargaAll.Text.Replace(".", "").Replace("Rp", ""));
                        txtTotalHargaAll.Text = string.Format(culture, "{0:C0}", harTot + harSem);
                    }
                }
                else MessageBox.Show("Stok Item Tidak Mencukupi", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("Data Belum Lengkap, Lengkapi Terlebih Dahulu", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CmbBoxNamaBarang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var namaBarang = cmbBoxNamaBarang.SelectedItem;
            if (namaBarang != null)
            {
                var barang = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ?", namaBarang);
                if (barang.Count != 0)
                {
                    var fix = barang.FirstOrDefault().HARGA_JUAL_PER.Split('/');
                    txtHargaSatuan.Text = $"{string.Format(culture, "{0:N0}", int.Parse(fix[0]))}/{fix[1]}";
                }
                else MessageBox.Show("Data Barang Yang Anda Masukkan Tidak Terdaftar", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TxtBoxJumlahBarang_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxJumlahBarang.Text != "")
            {
                try
                {
                    jmlhJual = int.Parse(txtBoxJumlahBarang.Text.Replace(".", ""));
                    if (!string.IsNullOrEmpty(txtBoxJumlahBarang.Text))
                    {
                        txtBoxJumlahBarang.Text = string.Format(culture, "{0:N0}", jmlhJual);
                        txtBoxJumlahBarang.Select(txtBoxJumlahBarang.Text.Length, 0);
                        if (txtBoxDiskon.Text != "") discount = 1 - (double.Parse(txtBoxDiskon.Text.Replace("%", "")) / 100);
                        var harTot = jmlhJual * int.Parse(txtHargaSatuan.Text.Split('/')[0].Replace(".", "")) * discount;
                        txtHargaTotal.Text = string.Format(culture, "{0:C0}", harTot);
                    }
                    else txtBoxJumlahBarang.Text = "0";
                }
                catch (Exception) { MessageBox.Show("Jumlah harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnDetailPenjualan_Click(object sender, RoutedEventArgs e)
        {
            var selected = ((DataRowView)TablePenjualan.SelectedItem).Row.ItemArray;
            new Tampilan_Detail_Jual(selected[3].ToString(), int.Parse(selected[1].ToString()), db).ShowDialog();
        }

        private void CmbBoxCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxCustomer.SelectedItem != null)
            {
                var getCustomer = db.Query<TabelCustomer>("SELECT * FROM TabelCustomer WHERE NAMA_CUSTOMER = ?", cmbBoxCustomer.SelectedItem.ToString()).FirstOrDefault();
                txtBoxDiskon.Text = getCustomer.DISKON_TETAP.ToString();
            }
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
                        if (txtHargaSatuan.Text != "Rp0,00/Kilogram")
                        {
                            double diskon = 1;
                            if (txtBoxDiskon.Text != "") diskon = 1 - (discount / 100);
                            var harTot = jmlhJual * int.Parse(txtHargaSatuan.Text.Split('/')[0].Replace(".", "").Replace("Rp", "")) * diskon;
                            txtHargaTotal.Text = string.Format(culture, "{0:C0}", harTot);
                        }
                    }
                    else txtBoxDiskon.Text = "0%";
                }
                catch (Exception) { MessageBox.Show("Diskon harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var window = new Tambah_Customer();
            window.ShowDialog();
            if (window.DialogResult == true) {
                DataContext = new Classes.ComboBoxItem();
                cmbBoxCustomer.SelectedItem = window.customerName;
            }
        }

        private void TxtBoxUangTunai_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtTunai = txtBoxUangTunai.Text;
            if (txtTunai != "")
            {
                try
                {
                    var value = int.Parse(txtBoxUangTunai.Text.Replace(".", ""));
                    txtBoxUangTunai.Text = string.Format(culture, "{0:N0}", value);
                    txtBoxUangTunai.Select(txtBoxUangTunai.Text.Length, 0);
                    if (txtBoxUangTunai.Text != "")
                    {
                        var nilai = txtBoxUangTunai.Text.Replace(".", "");
                        txtUangKembali.Text = string.Format(culture, "{0:C0}", value - int.Parse(txtTotalHargaAll.Text.Replace("Rp", "").Replace(".", "")));
                    }
                }
                catch (Exception) { MessageBox.Show("Uang Tunai harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnHapusItemJual_Click(object sender, RoutedEventArgs e)
        {
            var selected = (DetailPenjualan)TableTambahPenjualan.SelectedItem;
            Penjualans.Remove(selected);
            var harSem = int.Parse(txtTotalHargaAll.Text.Replace(".", "").Replace("Rp", ""));
            txtTotalHargaAll.Text = string.Format(culture, "{0:C0}", harSem - int.Parse(selected.HARGA_TOTAL.Replace(".", "").Replace("Rp", "")));
            TableTambahPenjualan.ItemsSource = null;
            TableTambahPenjualan.ItemsSource = Penjualans;
        }

        #endregion

        #region LAPORAN

        private void CloseTambahBebTrak_Click(object sender, RoutedEventArgs e)
        {
            closePanel.Begin(GridTambahBebTrak);
            ResetTambahBeban();
            ResetTambahTransaksi();
        }

        private void ResetTambahBeban()
        {
            txtBoxTglBeban.Text = "";
            txtBoxKetBeban.Text = "";
            cmbBoxJenisBeban.SelectedItem = null;
            txtBoxNilaiBeban.Text = "";
            TxtTotalRugi.Text = "Rp0,00";
        }

        private void ResetTambahTransaksi()
        {
            txtBoxTglTransaksi.Text = "";
            txtBoxKetTransaksi.Text = "";
            cmbBoxJenisTransaksi.SelectedItem = null;
            txtBoxNilaiTransaksi.Text = "";
            cmbBoxMetodeTransaksi.SelectedItem = null;
        }

        private void TxtBoxJumlahBeban_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtJumlahBeban = TxtBoxJumlahBeban.Text;
            if (txtJumlahBeban != "")
            {
                try
                {
                    var value = int.Parse(TxtBoxJumlahBeban.Text.Replace(".", ""));
                    TxtBoxJumlahBeban.Text = string.Format(culture, "{0:N0}", value);
                    TxtBoxJumlahBeban.Select(TxtBoxJumlahBeban.Text.Length, 0);
                    if (txtBoxNilaiBeban.Text != "")
                    {
                        var nilai = txtBoxNilaiBeban.Text.Replace(".", "");
                        TxtTotalBeban.Text = string.Format(culture, "{0:C0}", int.Parse(nilai) * value);
                    }
                }
                catch (Exception) { MessageBox.Show("Nilai beban dan jumlah harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void TxtBoxNilaiTransaksi_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtNilaiTrans = txtBoxNilaiTransaksi.Text;
            if (txtNilaiTrans != "")
            {
                try
                {
                    var value = int.Parse(txtBoxNilaiTransaksi.Text.Replace(".", ""));
                    txtBoxNilaiTransaksi.Text = string.Format(culture, "{0:N0}", value);
                    txtBoxNilaiTransaksi.Select(txtBoxNilaiTransaksi.Text.Length, 0);
                }
                catch (Exception) { MessageBox.Show("Nilai transaksi harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnTambahBeban2_Click(object sender, RoutedEventArgs e)
        {
            if (txtTitleBebTrak.Text == "Tambah Beban")
            {
                if (txtBoxTglBeban.Text != "" && txtBoxKetBeban.Text != "" && cmbBoxJenisBeban.SelectedItem != null && txtBoxNilaiBeban.Text != "" && TxtBoxJumlahBeban.Text != "")
                {
                    var id = 1;
                    var nilai = TxtTotalBeban.Text.Replace("Rp", "");
                    var getBeban = db.Query<TabelBeban>("SELECT * FROM TabelBeban");
                    if (getBeban.Count != 0) id = getBeban.Max(x => x.ID_BEBAN) + 1;
                    db.Insert(new TabelBeban()
                    {
                        ID_BEBAN = id,
                        JENIS = cmbBoxJenisBeban.SelectedItem.ToString(),
                        KETERANGAN = txtBoxKetBeban.Text,
                        TANGGAL_BEBAN = txtBoxTglBeban.SelectedDate.Value.ToString("dd/MM/yyyy"),
                        NILAI_BEBAN = txtBoxNilaiBeban.Text,
                        JUMLAH = TxtBoxJumlahBeban.Text,
                        TOTAL_BEBAN = nilai
                    });
                    var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                    db.RunInTransaction(() => { kas.TOTAL -= double.Parse(nilai.Replace(".", "")); db.Update(kas); });
                    ResetTambahBeban(); closePanel.Begin(GridTambahBebTrak);
                    BacaDatabase();
                }
                else MessageBox.Show("Ada Field yang belum Terisi. Semua Field Wajib Diisi!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (txtTitleBebTrak.Text == "Tambah Transaksi")
            {
                if (txtBoxTglTransaksi.Text != "" && txtBoxKetTransaksi.Text != "" && cmbBoxJenisTransaksi.SelectedItem != null && txtBoxNilaiTransaksi.Text != "")
                {
                    var id = 1;
                    var getTrans = db.Query<TabelTransaksi>("SELECT * FROM TabelTransaksi");
                    var field = cmbBoxJenisTransaksi.SelectedItem.ToString();
                    var nilai = double.Parse(txtBoxNilaiTransaksi.Text.Replace(".", ""));
                    var method = ""; if (cmbBoxMetodeTransaksi.SelectedItem != null) cmbBoxMetodeTransaksi.SelectedItem.ToString();
                    if (getTrans.Count != 0) id = getTrans.Max(x => x.ID_TRANSAKSI) + 1;
                    db.Insert(new TabelTransaksi()
                    {
                        ID_TRANSAKSI = id, TGL_TRANSAKSI = txtBoxTglTransaksi.SelectedDate.Value.ToString("dd/MM/yyyy"),
                        KETERANGAN = txtBoxKetTransaksi.Text, NAMA_FIELD_TOTAL = field, NILAI = nilai, METODE_BAYAR = method
                    });
                    //"Pengurangan Kas", "Penambahan Kas", "Penambahan Utang", "Pelunasan Utang", "Pelunasan Piutang"
                    var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                    if (field.Contains("Utang"))
                    {
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        if (field.Contains("Pelunasan")) {
                            if (method.Contains("Utang")) db.RunInTransaction(() => { utang.TOTAL -= nilai; utang.TOTAL += nilai; db.Update(utang); });
                            else db.RunInTransaction(() => { utang.TOTAL -= nilai; kas.TOTAL -= nilai; db.Update(utang); db.Update(kas); });
                        }
                        if (field.Contains("Penambahan")) db.RunInTransaction(() => { utang.TOTAL += nilai; kas.TOTAL += nilai; db.Update(utang); db.Update(kas); });
                    }
                    else if (field.Contains("Piutang"))
                    {
                        var piutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First();
                        if (field.Contains("Pelunasan")) db.RunInTransaction(() => { piutang.TOTAL -= nilai; kas.TOTAL += nilai; db.Update(piutang); db.Update(kas); });
                    }
                    else if (field.Contains("Kas"))
                    {
                        var ekuitas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Ekuitas'").First();
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        if (field.Contains("Penambahan"))
                        {
                            if (method.Contains("Utang")) db.RunInTransaction(() => { utang.TOTAL += nilai; kas.TOTAL += nilai; db.Update(utang); db.Update(kas); });
                            else db.RunInTransaction(() => { ekuitas.TOTAL += nilai; kas.TOTAL += nilai; db.Update(ekuitas); db.Update(kas); });
                        }
                        else if (field.Contains("Pengurangan"))
                        {
                            db.RunInTransaction(() => { ekuitas.TOTAL -= nilai; kas.TOTAL -= nilai; db.Update(ekuitas); db.Update(kas); });
                        }
                    }
                    ResetTambahTransaksi(); closePanel.Begin(GridTambahBebTrak);
                    BacaDatabase();
                }
                else MessageBox.Show("Ada Field yang belum Terisi. Semua Field Wajib Diisi!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtBoxNilaiBeban_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtNilaiBeban = txtBoxNilaiBeban.Text;
            if (txtNilaiBeban != "")
            {
                try
                {
                    var value = int.Parse(txtBoxNilaiBeban.Text.Replace(".", ""));
                    txtBoxNilaiBeban.Text = string.Format(culture, "{0:N0}", value);
                    txtBoxNilaiBeban.Select(txtBoxNilaiBeban.Text.Length, 0);
                    if (TxtBoxJumlahBeban.Text != "")
                    {
                        var jumlah = TxtBoxJumlahBeban.Text;
                        TxtTotalBeban.Text = string.Format(culture, "{0:C0}", (int.Parse(jumlah) * value));
                    }
                }
                catch (Exception) { MessageBox.Show("Nilai beban dan jumlah harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnTambahBeban_Click(object sender, RoutedEventArgs e)
        {
            txtTitleBebTrak.Text = "Tambah Beban";
            BtnTambahBeban2.Content = "Tambah Beban";
            panelBeban.Visibility = Visibility.Visible;
            panelTransaksi.Visibility = Visibility.Collapsed;
            openPanel.Begin(GridTambahBebTrak);
        }

        private void BtnTambahTransaksi_Click(object sender, RoutedEventArgs e)
        {
            txtTitleBebTrak.Text = "Tambah Transaksi";
            BtnTambahBeban2.Content = "Tambah Transaksi";
            panelBeban.Visibility = Visibility.Collapsed;
            panelTransaksi.Visibility = Visibility.Visible;
            openPanel.Begin(GridTambahBebTrak);
        }

        private void BtnTampilkanLaporan_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBoxBulanLaporan.SelectedItem != null)
            {
                var window = new Authentication_Windows();
                window.ShowDialog();

                if (window.DialogResult == true) 
                {
                    var bulan = GetIndexOfBulan(cmbBoxBulanLaporan.SelectedItem.ToString());
                    var laporan = GenerateLaporanLabaRugi(bulan, cmbBoxBulanLaporan.SelectedItem.ToString(), 13);
                    fdViewer.Document = laporan;
                    fdViewer.Visibility = Visibility.Visible;
                    NoDataLaporanSelected.Visibility = Visibility.Collapsed;
                }
            }
            else MessageBox.Show("Pilih Bulan Untuk Menampilkan Laporan", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetIndexOfBulan(string namaBulan) 
        {
            var index = "1";
            switch (namaBulan) 
            {
                case "Januari": index = "1"; break;
                case "Februari": index = "2"; break;
                case "Maret": index = "3"; break;
                case "April": index = "4"; break;
                case "Mei": index = "5"; break;
                case "Juni": index = "6"; break;
                case "Juli": index = "7"; break;
                case "Agustus": index = "8"; break;
                case "September": index = "9"; break;
                case "Oktober": index = "10"; break;
                case "November": index = "11"; break;
                case "Desember": index = "12"; break;
            }
            return index;
        }

        private TableCell NewCell(string paragraph, string fontWeight, TextAlignment alignment = TextAlignment.Left)
        {
            if (fontWeight == "SemiBold") return new TableCell(new Paragraph(new Run(paragraph)) { TextAlignment = alignment, FontWeight = FontWeights.SemiBold });
            return new TableCell(new Paragraph(new Run(paragraph)) { TextAlignment = alignment, FontWeight = FontWeights.Normal });
        }

        private FlowDocument GenerateLaporanLabaRugi(string nomorBulan, string namaBulan, int fontSize)
        {
            var size = 0.5;
            var laporan = new FlowDocument() { FontFamily = new FontFamily("Segoe UI"), Foreground = new SolidColorBrush(Colors.Black) };
            var header = new Paragraph(new Run($"Laporan Laba Rugi {namaBulan}")) { FontSize = fontSize + 3, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 0) };
            var tabelPendapatan = new Table() { CellSpacing = 0, Margin = new Thickness(0, 5, 0, 0), FontSize = fontSize };
            var tabelBeban = new Table() { CellSpacing = 0, Margin = new Thickness(0, 10, 0, 0), FontSize = fontSize };
            var tabelLaba = new Table() { CellSpacing = 0, Margin = new Thickness(0, 12, 0, 0), FontSize = fontSize };

            #region Pendapatan

            tabelPendapatan.RowGroups.Clear();
            tabelPendapatan.RowGroups.Add(new TableRowGroup());
            tabelPendapatan.Columns.Add(new TableColumn() { Width = new GridLength(240) });
            tabelPendapatan.Columns.Add(new TableColumn() { Width = new GridLength(200) });
            tabelPendapatan.Columns.Add(new TableColumn() { Width = new GridLength(200) });

            var intArray = new int[] { 0, 1 };
            var headerName = new string[] { "Pendapatan", "\tCash / Bank", "\tPiutang", "Pendapatan Total", "Pembelian Stok (HPP)" };
            var rawCash = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan WHERE METODE_BAYAR = 'Cash / Bank'");
            var rawPiutang = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan WHERE METODE_BAYAR = 'Piutang'");
            var pembelian = db.Query<TabelStok>("SELECT * FROM TabelStok");
            var cashBank = 0; var piutang = 0; var hpp = 0;
            if (rawCash.Count != 0) cashBank = rawCash.Where(x => x.TANGGAL_JUAL.Split('/')[1].Contains(nomorBulan)).Sum(x => int.Parse(x.TOTAL_PENJUALAN.Replace("Rp", "").Replace(".", "")));
            if (rawPiutang.Count != 0) piutang = rawPiutang.Where(x => x.TANGGAL_JUAL.Split('/')[1].Contains(nomorBulan)).Sum(x => int.Parse(x.TOTAL_PENJUALAN.Replace("Rp", "").Replace(".", "")));
            if (pembelian.Count != 0) hpp = pembelian.Where(x => x.TGL_MASUK.Split('/')[1].Contains(nomorBulan)).Sum(x => x.HARGA_BELI);
            var totalPendapatan = cashBank + piutang;

            for (int n = 0; n < headerName.Length; n++)
            {
                tabelPendapatan.RowGroups[0].Rows.Add(new TableRow());
                var rowContent = tabelPendapatan.RowGroups[0].Rows[n];

                if (n == 0) rowContent.Cells.Add(NewCell(headerName[n], "SemiBold"));
                else rowContent.Cells.Add(NewCell(headerName[n], "Normal"));

                if (n == 1) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", cashBank), "Normal", TextAlignment.Right));
                else if (n == 2) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", piutang), "Normal", TextAlignment.Right));
                else if (n == 3) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", totalPendapatan), "Normal", TextAlignment.Right));
                else if (n == 4) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", hpp), "Normal", TextAlignment.Right));
                else rowContent.Cells.Add(NewCell("", "Normal"));

                if (n == headerName.Length - 1) foreach (var i in intArray) { rowContent.Cells[i].BorderBrush = Brushes.Black; rowContent.Cells[i].BorderThickness = new Thickness(0, 0, 0, size); }

                rowContent.Cells[0].Padding = new Thickness(7, 10, 7, 10);
                rowContent.Cells[1].Padding = new Thickness(7, 10, 7, 10);
            }

            #endregion

            #region Beban

            tabelBeban.RowGroups.Add(new TableRowGroup());
            tabelBeban.Columns.Add(new TableColumn() { Width = new GridLength(240) });
            tabelBeban.Columns.Add(new TableColumn() { Width = new GridLength(200) });
            tabelBeban.Columns.Add(new TableColumn() { Width = new GridLength(200) });

            var bebanName = new string[] { "Beban", "\tBeban Gaji", "\tBeban Perlengkapan", "\tBeban Listrik dan Air", "\tBeban Telepon dan Internet", "Total Beban", "Laba / Rugi" };
            var rawGaji = db.Query<TabelBeban>("SELECT * FROM TabelBeban WHERE JENIS = 'Beban Gaji'");
            var rawPerlengkapan = db.Query<TabelBeban>("SELECT * FROM TabelBeban WHERE JENIS = 'Beban Perlengkapan'");
            var rawListrik = db.Query<TabelBeban>("SELECT * FROM TabelBeban WHERE JENIS = 'Beban Listrik / Air'");
            var rawTel = db.Query<TabelBeban>("SELECT * FROM TabelBeban WHERE JENIS = 'Beban Telepon / Internet'");
            var gaji = 0; var perlengkapan = 0; var listrik = 0; var telepon = 0;
            if (rawGaji.Count != 0) gaji = rawGaji.Where(x => x.TANGGAL_BEBAN.Split('/')[1].Contains(nomorBulan)).Sum(x => int.Parse(x.TOTAL_BEBAN.Replace("Rp", "").Replace(".", "")));
            if (rawPerlengkapan.Count != 0) perlengkapan = rawPerlengkapan.Where(x => x.TANGGAL_BEBAN.Split('/')[1].Contains(nomorBulan)).Sum(x => int.Parse(x.TOTAL_BEBAN.Replace("Rp", "").Replace(".", "")));
            if (rawListrik.Count != 0) listrik = rawListrik.Where(x => x.TANGGAL_BEBAN.Split('/')[1].Contains(nomorBulan)).Sum(x => int.Parse(x.TOTAL_BEBAN.Replace("Rp", "").Replace(".", "")));
            if (rawTel.Count != 0) telepon = rawTel.Where(x => x.TANGGAL_BEBAN.Split('/')[1].Contains(nomorBulan)).Sum(x => int.Parse(x.TOTAL_BEBAN.Replace("Rp", "").Replace(".", "")));
            var totalBeban = gaji + perlengkapan + listrik + telepon;

            for (int n = 0; n < bebanName.Length; n++)
            {
                tabelBeban.RowGroups[0].Rows.Add(new TableRow());
                var rowContent = tabelBeban.RowGroups[0].Rows[n];
                
                rowContent.Cells.Add(NewCell(bebanName[n], "Normal"));

                if (n == 1) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", gaji), "Normal", TextAlignment.Right));
                else if (n == 2) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", perlengkapan), "Normal", TextAlignment.Right));
                else if (n == 3) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", listrik), "Normal", TextAlignment.Right));
                else if (n == 4) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", telepon), "Normal", TextAlignment.Right));
                else if (n == 5) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", totalBeban), "Normal", TextAlignment.Right));
                else if (n == 6) rowContent.Cells.Add(NewCell(string.Format(culture, "{0:C0}", totalPendapatan - hpp - totalBeban), "Normal", TextAlignment.Right));
                else rowContent.Cells.Add(NewCell("", "Normal"));

                if (n == bebanName.Length - 2) foreach (var i in intArray) { rowContent.Cells[i].BorderBrush = Brushes.Black; rowContent.Cells[i].BorderThickness = new Thickness(0, 0, 0, size); }

                rowContent.Cells[0].Padding = new Thickness(7, 10, 7, 10);
                rowContent.Cells[1].Padding = new Thickness(7, 10, 7, 10);
            }

            #endregion

            laporan.Blocks.Add(header);
            laporan.Blocks.Add(tabelPendapatan);
            laporan.Blocks.Add(tabelBeban);
            laporan.Blocks.Add(tabelLaba);

            return laporan;
        }

        #endregion
    }
}