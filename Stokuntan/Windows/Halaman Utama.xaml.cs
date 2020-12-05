using SQLite;
using Stokuntan.Classes;
using Stokuntan.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Stokuntan.Windows
{
    public partial class Halaman_Utama : Window
    {
        readonly SQLiteConnection db = new IDatabase().Conn();
        List<TabelStok> DataStok; List<TabelRealStok> DataRealStok;
        List<TabelPembelian> DataPembelian;  List<TabelPenjualan> DataPenjualan;
        DataTable tablePenjualan; DataTable tablePembelian;
        readonly List<DataGridAttr> DataGridAttrsJual, DataGridAttrsPenjualan, DataGridAttrsStok;
        List<DetailPenjualan> Penjualans = new List<DetailPenjualan>();
        List<TabelStok> Pembelians = new List<TabelStok>();
        readonly List<MinJualTemp> jualItemTemp;
        readonly CultureInfo culture = new CultureInfo("id-ID");
        int hargaJual = 0; int hargaBeli = 0; int jmlhJual = 0; double discount = 1;
        string idPembelianRoot = ""; bool isMigrasi = false; bool isUtang = false; bool isPiutang = false;
        bool isUbahPembelian = false; bool isUbahPenjualan = false; object[] item;
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
            HideUI(GridTransaksi);
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
                new DataGridAttr() { HEADER = "Total Penjualan", BINDING = "TOTAL_PENJUALAN", WIDTH = 180 },
                new DataGridAttr() { HEADER = "Total Dibayar", BINDING = "TOTAL_BAYAR", WIDTH = 180 },
                new DataGridAttr() { HEADER = "Sisa Bayar", BINDING = "SISA_BAYAR", WIDTH = 180 }
            };
            jualItemTemp = new List<MinJualTemp>();
            DataContext = new Classes.ComboBoxItem();
            storyBoardIn = (Storyboard)Resources["fadeIn"];
            storyBoardOut = (Storyboard)Resources["fadeOut"];
            openPanel = (Storyboard)Resources["JualOpen"];
            closePanel = (Storyboard)Resources["JualClose"];
            InitiateAddSaleAnimation();
            GenerateColumnPembelian();
            GenerateColumnDataGrid();
            GenerateColumnTransaksi();
            BacaBeranda();

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("id-ID");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("id-ID");
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

        public DataGridTemplateColumn ButtonInDataGrid(string content, RoutedEventHandler eventHandler)
        {
            var column = new DataGridTemplateColumn();
            var template = new DataTemplate();
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(ContentProperty, content);
            buttonFactory.SetValue(FontSizeProperty, 10D);
            buttonFactory.SetValue(HeightProperty, 23D);
            buttonFactory.SetValue(StyleProperty, FindResource("MaterialDesignFlatButton"));
            buttonFactory.AddHandler(ButtonBase.ClickEvent, eventHandler);
            template.VisualTree = buttonFactory;
            column.CellTemplate = template;

            return column;
        }

        private void BacaBeranda() 
        {
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

        private void BacaPenjualan() 
        {
            DataPenjualan = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan WHERE DETAIL_JUAL != 'PIUTANG_MIGRASI'");
            tablePenjualan = new DataTable();
            var jualColumns = new List<string>() { "NO", "ID_JUAL", "TANGGAL_JUAL", "CUSTOMER", "DETAIL_JUAL", "TOTAL_PENJUALAN", "TOTAL_BAYAR", "SISA_BAYAR", "LOKASI_STOK", "METODE_BAYAR" };
            foreach (var value in jualColumns) { tablePenjualan.Columns.Add(value); }            

            int i = 1;
            foreach (var value in DataPenjualan)
            {
                var row = tablePenjualan.NewRow();
                var customer = db.Query<TabelCustomer>("SELECT * FROM TabelCustomer WHERE ID_CUSTOMER = ?", value.CUSTOMER).FirstOrDefault();
                row["NO"] = i++;
                row["ID_JUAL"] = value.ID_JUAL;
                row["TANGGAL_JUAL"] = value.TANGGAL_JUAL;
                row["CUSTOMER"] = customer.NAMA_CUSTOMER;
                row["DETAIL_JUAL"] = value.DETAIL_JUAL;
                row["TOTAL_PENJUALAN"] = value.TOTAL_PENJUALAN;
                row["TOTAL_BAYAR"] = value.TOTAL_BAYAR;
                row["SISA_BAYAR"] = value.SISA_BAYAR;
                row["LOKASI_STOK"] = value.LOKASI_STOK;
                row["METODE_BAYAR"] = value.METODE_BAYAR;
                tablePenjualan.Rows.Add(row);
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
        }

        private void BacaPembelian() 
        {
            var DataPembelian = db.Query<TabelPembelian>("SELECT * FROM TabelPembelian WHERE DAFTAR_STOK != 'UTANG_MIGRASI'");
            tablePembelian = new DataTable();
            var columns = new List<string>() { "NO", "REF_NOTA", "TGL_BELI", "SUPPLIER", "DAFTAR_STOK", "METODE_BAYAR", "TOTAL_TAGIHAN", "TOTAL_BAYAR", "SISA_BAYAR", "LOKASI" };
            foreach (var value in columns) { tablePembelian.Columns.Add(value); }
            int i = 1;
            foreach (var value in DataPembelian)
            {
                var supplier = db.Get<TabelSupplier>(value.ID_SUPPLIER);
                var row = tablePembelian.NewRow();
                row["NO"] = i++;
                row["REF_NOTA"] = value.REF_NOTA;
                row["TGL_BELI"] = value.TGL_BELI;
                row["SUPPLIER"] = supplier;
                row["DAFTAR_STOK"] = value.DAFTAR_STOK;
                row["METODE_BAYAR"] = value.METODE_BAYAR;
                row["TOTAL_TAGIHAN"] = string.Format(culture, "{0:C0}", value.TOTAL_TAGIHAN);
                row["TOTAL_BAYAR"] = string.Format(culture, "{0:C0}", value.TOTAL_BAYAR);
                row["SISA_BAYAR"] = string.Format(culture, "{0:C0}", value.SISA_BAYAR);
                row["LOKASI"] = value.LOKASI;
                tablePembelian.Rows.Add(row);
            }

            if (DataPembelian.Count == 0)
            {
                TablePembelian.ItemsSource = null;
                TablePembelian.Visibility = Visibility.Collapsed;
                NoDataPembelian.Visibility = Visibility.Visible;
            }
            else
            {
                TablePembelian.ItemsSource = null;
                TablePembelian.ItemsSource = tablePembelian.AsDataView();
                TablePembelian.Visibility = Visibility.Visible;
                NoDataPembelian.Visibility = Visibility.Collapsed;
            }
        }

        private void BacaDatabase()
        {
            DataStok = db.Query<TabelStok>("SELECT * FROM TabelStok");
            DataRealStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok");
            
            var realColumns = new List<string>() { "NO", "ID_STOK_END", "KATEGORI_MEREK", "TOTAL_STOK", "SATUAN_STOK", "STOK_DLM_GRAM", "HARGA_JUAL_PER", "LOKASI" };
            var tableRealStok = new DataTable();
            foreach (var value in realColumns) { tableRealStok.Columns.Add(value); }            

            int i = 1;
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
        }

        private void GenerateColumnPembelian()
        {
            TablePembelian.Columns.Clear();
            TableTambahPembelian.Columns.Clear();
            TableDetailPembelian.Columns.Clear();
            int i = 0;
            var DataGridAttrDetailBeli = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "No", BINDING = "NO", WIDTH = 60 },
                new DataGridAttr() { HEADER = "Deskripsi", BINDING = "DESKRIPSI", WIDTH = 210 },
                new DataGridAttr() { HEADER = "Harga Beli", BINDING = "HARGA_BELI", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Qty", BINDING = "JUMLAH_STOK", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Satuan", BINDING = "SATUAN", WIDTH = 110 },
                new DataGridAttr() { HEADER = "Total Harga", BINDING = "TOTAL_HARGA", WIDTH = 130 },
                new DataGridAttr() { HEADER = "Harga Jual", BINDING = "HARGA_JUAL", WIDTH = 170 }
            };
            var DataGridAttrPembelian = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "No", BINDING = "NO", WIDTH = 50 },
                new DataGridAttr() { HEADER = "Ref. / Nota", BINDING = "REF_NOTA", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Tgl Beli", BINDING = "TGL_BELI", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Supplier", BINDING = "SUPPLIER", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Detail Beli", BINDING = "DAFTAR_STOK", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Metode Bayar", BINDING = "METODE_BAYAR", WIDTH = 110 },
                new DataGridAttr() { HEADER = "Total Tagihan", BINDING = "TOTAL_TAGIHAN", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Total Bayar", BINDING = "TOTAL_BAYAR", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Sisa Bayar", BINDING = "SISA_BAYAR", WIDTH = 210 },
                new DataGridAttr() { HEADER = "Lokasi", BINDING = "LOKASI", WIDTH = 210 }
            };
            var DataGridAttrBeli = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Kategori Barang", BINDING = "KATEGORI", WIDTH = 170 },
                new DataGridAttr() { HEADER = "Merek", BINDING = "MEREK", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Qty", BINDING = "JUMLAH_STOK", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Satuan", BINDING = "SATUAN", WIDTH = 110 },
                new DataGridAttr() { HEADER = "Harga Satuan", BINDING = "HARGA_BELI", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Harga Total", BINDING = "TOTAL_HARGA", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Harga Jual", BINDING = "HARGA_JUAL", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Satuan Jual", BINDING = "SATUAN_JUAL", WIDTH = 110 }
            };
            foreach (var value in DataGridAttrBeli)
            {
                TableTambahPembelian.Columns.Add(new DataGridTextColumn
                {
                    Header = value.HEADER,
                    Width = value.WIDTH,
                    Binding = new Binding(value.BINDING)
                });
            }
            foreach (var value in DataGridAttrPembelian) 
            {
                if (i == 4) TablePembelian.Columns.Add(ButtonInDataGrid("Detail Pembelian", new RoutedEventHandler(BtnDetailBeli_Click)));
                else 
                {
                    TablePembelian.Columns.Add(new DataGridTextColumn
                    {
                        Header = value.HEADER,
                        Width = value.WIDTH,
                        Binding = new Binding(value.BINDING)
                    });
                }
                i++;
            }
            foreach (var value in DataGridAttrDetailBeli) 
            { 
                TableDetailPembelian.Columns.Add(new DataGridTextColumn() { Binding = new Binding(value.BINDING), Header = value.HEADER, Width = value.WIDTH }); 
            }

            TableTambahPembelian.Columns.Add(ButtonInDataGrid("HAPUS", new RoutedEventHandler(BtnHapusItemBeli_Click)));
            TableTambahPembelian.Columns.Add(ButtonInDataGrid("UBAH", new RoutedEventHandler(BtnUbahItemBeli_Click)));
        }

        private void GenerateColumnDataGrid()
        {
            TableStok.Columns.Clear();
            TablePenjualan.Columns.Clear();
            TableTambahPenjualan.Columns.Clear();
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
            var buttonColumn = (DataGridTemplateColumn)TableStok.Resources["BtnDetBeli"];
            TableStok.Columns.Add(buttonColumn);
            TablePenjualan.Columns.Add((DataGridTemplateColumn)TablePenjualan.Resources["BtnDetJual"]);
            TableTambahPenjualan.Columns.Add(ButtonInDataGrid("HAPUS", new RoutedEventHandler(BtnHapusItemJual_Click)));
            TableTambahPenjualan.Columns.Add(ButtonInDataGrid("UBAH", new RoutedEventHandler(BtnUbahItemJual_Click)));
        }

        private void GenerateColumnTransaksi()
        {
            TableUtang.Columns.Clear();
            TablePiutang.Columns.Clear();
            TableRiwayatUtang.Columns.Clear();
            TableRiwayatPiutang.Columns.Clear();
            TableBeban.Columns.Clear(); TableKas.Columns.Clear();

            var DataGridAttrUtang = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Ref / No. Nota", BINDING = "REF_NOTA", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Tgl Beli", BINDING = "TGL_BELI", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Supplier", BINDING = "SUPPLIER", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Total Tagihan", BINDING = "TOTAL_TAGIHAN", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Total Bayar", BINDING = "TOTAL_BAYAR", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Sisa Bayar", BINDING = "SISA_BAYAR", WIDTH = 210 }
            };
            var DataGridAttrPiutang = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Tgl Jual", BINDING = "TANGGAL_JUAL", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Customer", BINDING = "CUSTOMER", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Total Penjualan", BINDING = "TOTAL_PENJUALAN", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Total Bayar", BINDING = "TOTAL_BAYAR", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Sisa Bayar", BINDING = "SISA_BAYAR", WIDTH = 210 }
            };
            var AttrRiwayatUtang = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Ref / No. Nota", BINDING = "REF_NOTA", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Tgl Bayar", BINDING = "TGL_BELI", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Supplier", BINDING = "SUPPLIER", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Total Tagihan", BINDING = "TOTAL_TAGIHAN", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Total Bayar", BINDING = "TOTAL_BAYAR", WIDTH = 150 }
            };
            var AttrRiwayatPiutang = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Tgl Bayar", BINDING = "TANGGAL_JUAL", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Customer", BINDING = "CUSTOMER", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Total Penjualan", BINDING = "TOTAL_PENJUALAN", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Total Bayar", BINDING = "TOTAL_BAYAR", WIDTH = 150 }
            };
            var AttrBeban = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Tgl Beban", BINDING = "TANGGAL_BEBAN", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Keterangan", BINDING = "KETERANGAN", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Jenis Beban", BINDING = "JENIS", WIDTH = 140 },
                new DataGridAttr() { HEADER = "Harga / Nominal Satuan", BINDING = "NILAI_BEBAN", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Qty", BINDING = "JUMLAH", WIDTH = 70 },
                new DataGridAttr() { HEADER = "Total", BINDING = "TOTAL_BEBAN", WIDTH = 160 },
            };
            var AttrKas = new List<DataGridAttr>
            {
                new DataGridAttr() { HEADER = "Tgl Transaksi", BINDING = "TGL_TRANSAKSI", WIDTH = 120 },
                new DataGridAttr() { HEADER = "Keterangan", BINDING = "KETERANGAN", WIDTH = 150 },
                new DataGridAttr() { HEADER = "Jenis Transaksi", BINDING = "NAMA_FIELD_TOTAL", WIDTH = 170 },
                new DataGridAttr() { HEADER = "Nominal", BINDING = "NILAI", WIDTH = 160 }
            };

            foreach (var value in DataGridAttrUtang)
            {
                TableUtang.Columns.Add(new DataGridTextColumn() { Binding = new Binding(value.BINDING), Header = value.HEADER, Width = value.WIDTH });
            }
            foreach (var value in DataGridAttrPiutang)
            {
                TablePiutang.Columns.Add(new DataGridTextColumn() { Binding = new Binding(value.BINDING), Header = value.HEADER, Width = value.WIDTH });
            }
            foreach (var value in AttrRiwayatUtang)
            {
                TableRiwayatUtang.Columns.Add(new DataGridTextColumn() { Binding = new Binding(value.BINDING), Header = value.HEADER, Width = value.WIDTH });
            }
            foreach (var value in AttrRiwayatPiutang)
            {
                TableRiwayatPiutang.Columns.Add(new DataGridTextColumn() { Binding = new Binding(value.BINDING), Header = value.HEADER, Width = value.WIDTH });
            }
            foreach (var value in AttrBeban)
            {
                TableBeban.Columns.Add(new DataGridTextColumn() { Binding = new Binding(value.BINDING), Header = value.HEADER, Width = value.WIDTH });
            }
            foreach (var value in AttrKas)
            {
                TableKas.Columns.Add(new DataGridTextColumn() { Binding = new Binding(value.BINDING), Header = value.HEADER, Width = value.WIDTH });
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

        private async void HideView(UIElement uIElement)
        {
            await Task.Delay(2000);
            HideUI(uIElement);
        }

        private static string AddPrefix(string s, int digitCount = 4)
        {
            return s.PadLeft(digitCount, '0');
        }

        #endregion

        #region MENU SECTION

        private void BtnPembelian_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnTransaksi.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardOut.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardIn.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridTransaksi);
            storyBoardOut.Begin(GridPenjualan);
            BacaPembelian(); isUtang = false;
        }

        private void BtnStok_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnTransaksi.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardOut.Begin(GridBeranda);
            storyBoardIn.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridPenjualan);
            storyBoardOut.Begin(GridTransaksi);
            BacaDatabase();
        }

        private void BtnPenjualan_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnTransaksi.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardOut.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridTransaksi);
            storyBoardIn.Begin(GridPenjualan);
            BacaPenjualan(); isPiutang = false;
            DataContext = new Classes.ComboBoxItem();
        }

        private void BtnTransaksi_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnTransaksi.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardOut.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardIn.Begin(GridTransaksi);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridPenjualan);
            BacaUtangPiutang();
        }

        private void BtnLaporan_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnTransaksi.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            storyBoardOut.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardIn.Begin(GridLaporan);
            storyBoardOut.Begin(GridTransaksi);
            storyBoardOut.Begin(GridPenjualan);
            BacaBeban(); BacaKas();
        }

        private void BtnBeranda_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BtnBeranda.Background = new SolidColorBrush(Color.FromRgb(134, 88, 64));
            BtnPembelian.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnStok.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnPenjualan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnTransaksi.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            BtnLaporan.Background = new SolidColorBrush(Color.FromRgb(100, 43, 9));
            storyBoardIn.Begin(GridBeranda);
            storyBoardOut.Begin(GridStok);
            storyBoardOut.Begin(GridPembelian);
            storyBoardOut.Begin(GridLaporan);
            storyBoardOut.Begin(GridTransaksi);
            storyBoardOut.Begin(GridPenjualan);
            BacaBeranda();
        }

        #endregion

        #region PEMBELIAN

        private void BtnUbahPembelian_Click(object sender, RoutedEventArgs e)
        {
            var rawItem = (DataRowView)TablePembelian.SelectedItem;
            if (rawItem != null)
            {
                GridTambahPembelian.Visibility = Visibility.Visible;
                openPanel.Begin(GridTambahPembelian);
                var item = rawItem.Row.ItemArray;
                var listItem = new List<TabelStok>();

                foreach (var value in rawItem.Row.ItemArray[4].ToString().Split(';'))
                {
                    if (value != "") listItem.Add(db.Get<TabelStok>(value));
                }

                txtTitle.Text = "Ubah Pembelian";
                idPembelianRoot = item[1].ToString();
                txtBoxTglMasuk.SelectedDate = DateTime.Parse(item[2].ToString());
                txtBoxRefNota.Text = item[1].ToString();
                cmbBoxMetodeBayar.Text = item[5].ToString();
                cmbBoxSupplier.Text = item[3].ToString();
                Pembelians = listItem; 
                TableTambahPembelian.ItemsSource = null;
                TableTambahPembelian.ItemsSource = Pembelians;
                txtTotalHargaBeli.Text = item[6].ToString();
                TxtBoxJmlhBayarBeli.Text = GetNumFromCurr(item[7].ToString()).ToString();
                btnTambahPembelian2.Content = "Ubah Pembelian";
                isUbahPembelian = true;
            }
            else MessageBox.Show("Pilih Data Yang Akan Diubah!", "Tidak ada data dipilih", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnTambahPembelian_Click(object sender, RoutedEventArgs e)
        {
            GridTambahPembelian.Visibility = Visibility.Visible;
            var openPanel = (Storyboard)Resources["JualOpen"];
            openPanel.Begin(GridTambahPembelian);
            Pembelians.Clear();
            isUbahPembelian = false;
            isMigrasi = false; isUtang = false;
            txtTitle.Text = "Tambah Pembelian";
            btnAddItem_Beli.Content = "Tambah Item";
            btnTambahPembelian2.Content = "Tambah Pembelian";
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
                    var itemPembelian = db.Get<TabelPembelian>(item.Row.ItemArray[1].ToString());

                    foreach (var value in item.Row.ItemArray[4].ToString().Split(';')) 
                    {
                        if (value != "") 
                        {
                            var itemDelete = db.Query<TabelStok>("SELECT * FROM TabelStok WHERE ID_STOK = ?", value).FirstOrDefault();
                            var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND SATUAN_STOK = ? AND LOKASI = ?", itemDelete.DESKRIPSI, itemDelete.SATUAN, itemPembelian.LOKASI).FirstOrDefault();
                            var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", itemDelete.SATUAN).FirstOrDefault().DALAM_GRAM * itemDelete.JUMLAH_STOK;
                            isAvailable.TOTAL_STOK -= itemDelete.JUMLAH_STOK;
                            isAvailable.STOK_DLM_GRAM -= stokGram;
                            db.Update(isAvailable);
                            db.Delete(itemDelete);
                        }
                    }                    

                    if (itemPembelian.METODE_BAYAR.Contains("Cash"))
                    {
                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL += itemPembelian.TOTAL_TAGIHAN; db.Update(kas); });
                    }
                    else if (itemPembelian.METODE_BAYAR.Contains("Utang"))
                    {
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        db.RunInTransaction(() => { utang.TOTAL -= itemPembelian.SISA_BAYAR; db.Update(utang); });
                    }
                    else if (itemPembelian.METODE_BAYAR.Contains("Cicilan"))
                    {
                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL += itemPembelian.TOTAL_BAYAR; db.Update(kas); });
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        db.RunInTransaction(() => { utang.TOTAL -= itemPembelian.SISA_BAYAR; db.Update(utang); });
                    }

                    db.Delete(itemPembelian);
                    BacaPembelian();
                }
            }
            else MessageBox.Show("Pilih Data Yang Akan Dihapus!", "Tidak ada data dipilih", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void TxtBoxCariPembelian_KeyUp(object sender, KeyEventArgs e)
        {
            var searchString = txtBoxCariPembelian.Text;
            if (searchString != "")
            {
                var filtered = tablePembelian.AsEnumerable().Where(r => r.Field<string>("SUPPLIER").ToLower().Contains(searchString)); ;
                var parameter = "SUPPLIER";
                if (cmbBoxKategoriCari.SelectedItem != null) parameter = cmbBoxKategoriCari.SelectedItem.ToString();

                if (parameter.Equals("Tanggal Beli")) filtered = tablePembelian.AsEnumerable().Where(r => r.Field<string>("TGL_BELI").ToLower().Contains(searchString));
                else if (parameter.Equals("Lokasi")) filtered = tablePembelian.AsEnumerable().Where(r => r.Field<string>("LOKASI").ToLower().Contains(searchString));
                else if (parameter.Equals("Ref. / No. Nota")) filtered = tablePembelian.AsEnumerable().Where(r => r.Field<string>("REF_NOTA").ToLower().Contains(searchString));

                TablePembelian.ItemsSource = null;
                TablePembelian.ItemsSource = filtered.AsDataView();
            }
            else BacaPembelian();
        }

        private void TxtBoxJmlhBayarBeli_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtTunai = TxtBoxJmlhBayarBeli.Text;
            if (txtTunai != "")
            {
                try
                {
                    var value = int.Parse(TxtBoxJmlhBayarBeli.Text.Replace(".", ""));
                    TxtBoxJmlhBayarBeli.Text = string.Format(culture, "{0:N0}", value);
                    TxtBoxJmlhBayarBeli.Select(TxtBoxJmlhBayarBeli.Text.Length, 0);
                }
                catch (Exception) { MessageBox.Show("Uang Tunai harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void CloseTambahButton_Click(object sender, RoutedEventArgs e)
        {
            closePanel.Begin(GridTambahPembelian);
            ResetTambahPembelian();
            HideView(GridTambahPembelian);
        }

        private void BtnAddItem_Beli_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtBoxJumlahStok.Text, out int jumlah)) MessageBox.Show("Jumlah Stok harus berupa angka!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            if (cmbBoxKategori.SelectedItem != null && txtBoxTglMasuk.Text != "" && cmbBoxLokasi.SelectedItem != null &&
                txtBoxMerek.Text != "" && txtBoxJumlahStok.Text != "" && cmbBoxSatuan.SelectedItem != null &&
                txtBoxHargaBeli.Text != "" && txtBoxHargaJual.Text != "" && cmbBoxSupplier.SelectedItem != null)
            {
                var supplier = db.Query<TabelSupplier>("SELECT * FROM TabelSupplier WHERE NAMA_SUPPLIER = ?", cmbBoxSupplier.SelectedItem.ToString()).FirstOrDefault();
                var no_nota = "-"; var kategori = cmbBoxKategori.SelectedItem.ToString(); var merek = txtBoxMerek.Text; var deskripsi = $"{kategori} {merek}";
                var jmlhStok = int.Parse(txtBoxJumlahStok.Text); var satuanStok = cmbBoxSatuan.SelectedItem.ToString();
                var satuanJual = cmbBoxSatuanJual.SelectedItem.ToString();
                var lokasi = cmbBoxLokasi.SelectedItem.ToString();
                hargaBeli = GetNumFromCurr(txtBoxHargaBeli.Text);
                hargaJual = GetNumFromCurr(txtBoxHargaJual.Text);
                var totalHarga = hargaBeli * jmlhStok;
                if (txtBoxRefNota.Text != "") no_nota = txtBoxRefNota.Text;

                if (btnAddItem_Beli.Content.ToString() == "Ubah Item")
                {
                    var selected = (TabelStok)TableTambahPembelian.SelectedItem;
                    var index = Pembelians.FindIndex(c => c.KATEGORI == selected.KATEGORI && c.MEREK == selected.MEREK);

                    var item = new TabelStok()
                    {
                        ID_STOK = selected.ID_STOK, DESKRIPSI = deskripsi, KATEGORI = kategori,
                        TGL_MASUK = txtBoxTglMasuk.SelectedDate.Value.ToString("dd/MM/yyyy"),
                        MEREK = merek, JUMLAH_STOK = jmlhStok,
                        HARGA_BELI = hargaBeli, HARGA_JUAL = hargaJual,
                        SATUAN_JUAL = satuanJual, SATUAN = satuanStok,
                        TOTAL_HARGA = totalHarga, ID_SUPPLIER = supplier.ID_SUPPLIER, NO_NOTA_REF = no_nota
                    };

                    Pembelians[index] = item;
                    TableTambahPembelian.ItemsSource = null;
                    TableTambahPembelian.ItemsSource = Pembelians;

                    if (txtTotalHargaBeli.Text == "Rp0,00") txtTotalHargaBeli.Text = string.Format(culture, "{0:C0}", totalHarga);
                    else
                    {
                        var harSem = GetNumFromCurr(txtTotalHargaBeli.Text);
                        txtTotalHargaBeli.Text = string.Format(culture, "{0:C0}", harSem - (selected.HARGA_BELI * selected.JUMLAH_STOK) + totalHarga);
                    }
                    btnAddItem_Beli.Content = "Tambah Item";
                }
                else
                {
                    var item = new TabelStok()
                    {
                        DESKRIPSI = deskripsi,
                        KATEGORI = kategori,
                        TGL_MASUK = txtBoxTglMasuk.SelectedDate.Value.ToString("dd/MM/yyyy"),
                        MEREK = merek,
                        JUMLAH_STOK = jmlhStok,
                        HARGA_BELI = hargaBeli,
                        HARGA_JUAL = hargaJual,
                        SATUAN_JUAL = satuanJual,
                        SATUAN = satuanStok,
                        TOTAL_HARGA = totalHarga,
                        ID_SUPPLIER = supplier.ID_SUPPLIER,
                        NO_NOTA_REF = no_nota
                    };
                    Pembelians.Add(item);

                    TableTambahPembelian.ItemsSource = null;
                    TableTambahPembelian.ItemsSource = Pembelians;
                    if (txtTotalHargaBeli.Text == "Rp0,00") txtTotalHargaBeli.Text = string.Format(culture, "{0:C0}", totalHarga);
                    else
                    {
                        var harSem = GetNumFromCurr(txtTotalHargaBeli.Text);
                        txtTotalHargaBeli.Text = string.Format(culture, "{0:C0}", totalHarga + harSem);
                    }
                }

                ResetTambahItemBeli();
            }
            else MessageBox.Show("Belum bisa menambah item karena data Belum Lengkap, Lengkapi Terlebih Dahulu", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BtnUbahItemBeli_Click(object sender, RoutedEventArgs e)
        {
            var selected = (TabelStok)TableTambahPembelian.SelectedItem;
            btnAddItem_Beli.Content = "Ubah Item";
            cmbBoxKategori.SelectedItem = selected.KATEGORI;
            cmbBoxSatuan.SelectedItem = selected.SATUAN;
            cmbBoxSatuanJual.SelectedItem = selected.SATUAN_JUAL;
            txtBoxMerek.Text = selected.MEREK;
            txtBoxJumlahStok.Text = selected.JUMLAH_STOK.ToString();
            txtBoxHargaBeli.Text = selected.HARGA_BELI.ToString();
            txtBoxHargaJual.Text = selected.HARGA_JUAL.ToString();
        }

        private void BtnHapusItemBeli_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Apakah Anda yakin akan menghapus item pembelian?", "Konfirmasi Penghapusan", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) 
            {
                var selected = (TabelStok)TableTambahPembelian.SelectedItem;
                var harSem = GetNumFromCurr(txtTotalHargaBeli.Text);
                txtTotalHargaBeli.Text = string.Format(culture, "{0:C0}", harSem - (selected.HARGA_BELI * selected.JUMLAH_STOK));
                Pembelians.Remove(selected);
                TableTambahPembelian.ItemsSource = null;
                TableTambahPembelian.ItemsSource = Penjualans;
            }
        }

        private void BtnTambahPembelian2_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBoxSupplier.SelectedItem != null && cmbBoxMetodeBayar.SelectedItem != null &&
                cmbBoxLokasi.SelectedItem != null && txtBoxTglMasuk.Text != "")
            {
                var supplier = db.Query<TabelSupplier>("SELECT * FROM TabelSupplier WHERE NAMA_SUPPLIER = ?", cmbBoxSupplier.SelectedItem.ToString()).FirstOrDefault();
                var no_nota = "-"; var idPembelian = "1";
                var getIDpembelian = db.Query<TabelPembelian>("SELECT * FROM TabelPembelian");
                if (getIDpembelian.Count() != 0) idPembelian = getIDpembelian.Max(x => int.Parse(x.REF_NOTA.Split('-')[1]) + 1).ToString();
                if (txtBoxRefNota.Text != "") no_nota = txtBoxRefNota.Text;
                else no_nota = $"{txtBoxTglMasuk.SelectedDate.Value:yyyyMMdd}{AddPrefix(supplier.ID_SUPPLIER.ToString(), 2)}-{AddPrefix(idPembelian, 5)}";
                var metode = cmbBoxMetodeBayar.SelectedItem.ToString();
                var lokasi = cmbBoxLokasi.SelectedItem.ToString();
                var tanggal = txtBoxTglMasuk.SelectedDate.Value.ToString("dd/MM/yyyy");
                var listID = "";

                if (isUbahPembelian == false)
                {
                    foreach (var value in Pembelians)
                    {
                        var maxBeli = db.Query<TabelStok>("SELECT * FROM TabelStok");
                        var maxStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok");
                        var idStok = 1; var id = 1;
                        if (maxStok.Count != 0) idStok = maxStok.Max(x => x.ID_STOK_END) + 1;
                        if (maxBeli.Count != 0) id = maxBeli.Max(x => x.ID_STOK) + 1; var kategori = value.KATEGORI; var merek = value.MEREK; var deskripsi = $"{kategori} {merek}";
                        listID += $"{id};";
                        var satuanStok = value.SATUAN;
                        var satuanJual = value.SATUAN_JUAL;
                        var totalHarga = value.TOTAL_HARGA;

                        db.Insert(new TabelStok()
                        {
                            ID_STOK = id, DESKRIPSI = deskripsi,
                            KATEGORI = kategori, TGL_MASUK = tanggal, MEREK = merek,
                            TOTAL_HARGA = totalHarga, JUMLAH_STOK = value.JUMLAH_STOK,
                            HARGA_BELI = value.HARGA_BELI, HARGA_JUAL = value.HARGA_JUAL,
                            SATUAN_JUAL = satuanJual, SATUAN = satuanStok,
                            NO_NOTA_REF = no_nota, ID_SUPPLIER = supplier.ID_SUPPLIER
                        });

                        var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND SATUAN_STOK = ? AND HARGA_BELI = ? AND LOKASI = ?", deskripsi, satuanStok, value.HARGA_BELI, lokasi).FirstOrDefault();
                        var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", satuanStok).FirstOrDefault().DALAM_GRAM * value.JUMLAH_STOK;
                        if (isAvailable == null)
                        {
                            db.Insert(new TabelRealStok()
                            {
                                ID_STOK_END = idStok,
                                KATEGORI_MEREK = deskripsi,
                                TOTAL_STOK = value.JUMLAH_STOK,
                                SATUAN_STOK = satuanStok,
                                LOKASI = lokasi, HARGA_BELI = value.HARGA_BELI,
                                STOK_DLM_GRAM = stokGram,
                                HARGA_JUAL_PER = $"{value.HARGA_JUAL}/{satuanJual}"
                            });
                        }
                        else
                        {
                            isAvailable.TOTAL_STOK += value.JUMLAH_STOK;
                            isAvailable.STOK_DLM_GRAM += stokGram;
                            isAvailable.HARGA_JUAL_PER = $"{value.HARGA_JUAL}/{satuanJual}";
                            db.RunInTransaction(() => db.Update(isAvailable));
                        }
                    }

                    if (TxtBoxJmlhBayarBeli.Text != "" && TxtBoxJmlhBayarBeli.IsEnabled == true)
                    {
                        var totalBayar = GetNumFromCurr(TxtBoxJmlhBayarBeli.Text);
                        var totalTagihan = GetNumFromCurr(txtTotalHargaBeli.Text);
                        var sisaBayar = totalTagihan - totalBayar;
                        db.Insert(new TabelPembelian()
                        {
                            REF_NOTA = no_nota,
                            DAFTAR_STOK = listID,
                            METODE_BAYAR = metode,
                            ID_SUPPLIER = supplier.ID_SUPPLIER,
                            TGL_BELI = tanggal, TOTAL_BAYAR = totalBayar,
                            SISA_BAYAR = sisaBayar, LOKASI = lokasi,
                            TOTAL_TAGIHAN = totalTagihan
                        });

                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL -= totalBayar; db.Update(kas); });
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        db.RunInTransaction(() => { utang.TOTAL += sisaBayar; db.Update(utang); });
                    }
                    else
                    {
                        var totalTagihan = GetNumFromCurr(txtTotalHargaBeli.Text);
                        if (metode.Contains("Cash"))
                        {
                            db.Insert(new TabelPembelian()
                            {
                                REF_NOTA = no_nota,
                                DAFTAR_STOK = listID,
                                METODE_BAYAR = metode,
                                ID_SUPPLIER = supplier.ID_SUPPLIER,
                                TGL_BELI = tanggal,
                                TOTAL_BAYAR = totalTagihan,
                                SISA_BAYAR = 0, LOKASI = lokasi,
                                TOTAL_TAGIHAN = totalTagihan
                            });
                            var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                            db.RunInTransaction(() => { kas.TOTAL -= totalTagihan; db.Update(kas); });
                        }
                        else if (metode.Contains("Utang"))
                        {
                            db.Insert(new TabelPembelian()
                            {
                                REF_NOTA = no_nota,
                                DAFTAR_STOK = listID,
                                METODE_BAYAR = metode,
                                ID_SUPPLIER = supplier.ID_SUPPLIER,
                                TGL_BELI = tanggal,
                                TOTAL_BAYAR = 0, LOKASI = lokasi,
                                SISA_BAYAR = totalTagihan,
                                TOTAL_TAGIHAN = totalTagihan
                            });
                            var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                            db.RunInTransaction(() => { utang.TOTAL += totalTagihan; db.Update(utang); });
                        }
                    }
                }
                else
                {
                    foreach (var value in Pembelians)
                    {
                        listID += $"{value.ID_STOK};";
                        var kategori = value.KATEGORI; var merek = value.MEREK; var deskripsi = $"{kategori} {merek}";
                        var satuanStok = value.SATUAN;
                        var satuanJual = value.SATUAN_JUAL;
                        var totalHarga = value.TOTAL_HARGA;
                        var jmlhStokUpdate = value.JUMLAH_STOK;

                        value.DESKRIPSI = deskripsi;
                        value.KATEGORI = kategori;
                        value.TGL_MASUK = tanggal;
                        value.MEREK = merek;
                        value.TOTAL_HARGA = totalHarga;
                        value.JUMLAH_STOK = value.JUMLAH_STOK;
                        value.HARGA_BELI = value.HARGA_BELI;
                        value.HARGA_JUAL = value.HARGA_JUAL;
                        value.SATUAN_JUAL = satuanJual;
                        value.SATUAN = satuanStok;
                        value.NO_NOTA_REF = no_nota;
                        value.ID_SUPPLIER = supplier.ID_SUPPLIER;

                        db.RunInTransaction(() => db.Update(value));

                        var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND SATUAN_STOK = ? AND HARGA_BELI = ? AND LOKASI = ?", deskripsi, satuanStok, value.HARGA_BELI, lokasi);
                        var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", satuanStok).FirstOrDefault().DALAM_GRAM * value.JUMLAH_STOK;
                        if (isAvailable == null)
                        {
                            var maxStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok");
                            var idStok = 1;
                            if (maxStok.Count != 0) idStok = maxStok.Max(x => x.ID_STOK_END) + 1;
                            db.Insert(new TabelRealStok()
                            {
                                ID_STOK_END = idStok,
                                KATEGORI_MEREK = deskripsi,
                                TOTAL_STOK = value.JUMLAH_STOK,
                                SATUAN_STOK = satuanStok, HARGA_BELI = value.HARGA_BELI,
                                LOKASI = lokasi,
                                STOK_DLM_GRAM = stokGram,
                                HARGA_JUAL_PER = $"{value.HARGA_JUAL}/{satuanJual}"
                            });
                        }
                        else
                        {
                            var updated = isAvailable.FirstOrDefault();
                            updated.ID_STOK_END = updated.ID_STOK_END;
                            updated.TOTAL_STOK -= jmlhStokUpdate;
                            updated.STOK_DLM_GRAM -= stokGram * jmlhStokUpdate;
                            updated.TOTAL_STOK += value.JUMLAH_STOK;
                            updated.STOK_DLM_GRAM += stokGram * value.JUMLAH_STOK;
                            updated.HARGA_JUAL_PER = $"{hargaJual}/{satuanJual}";
                            db.RunInTransaction(() => db.Update(updated));
                        }
                    }

                    if (TxtBoxJmlhBayarBeli.Text != "" && TxtBoxJmlhBayarBeli.IsEnabled == true)
                    {
                        var totalBayar = GetNumFromCurr(TxtBoxJmlhBayarBeli.Text);
                        var totalTagihan = GetNumFromCurr(txtTotalHargaBeli.Text);
                        var sisaBayar = totalTagihan - totalBayar;
                        var pembelian = db.Get<TabelPembelian>(idPembelianRoot);
                        var sisaBayarBefore = pembelian.SISA_BAYAR;
                        var bayarBefore = pembelian.TOTAL_BAYAR;

                        pembelian.REF_NOTA = no_nota;
                        pembelian.DAFTAR_STOK = listID;
                        pembelian.METODE_BAYAR = metode;
                        pembelian.ID_SUPPLIER = supplier.ID_SUPPLIER;
                        pembelian.TGL_BELI = tanggal;
                        pembelian.TOTAL_BAYAR = totalBayar;
                        pembelian.SISA_BAYAR = sisaBayar;
                        pembelian.TOTAL_TAGIHAN = totalTagihan;
                        pembelian.LOKASI = lokasi;

                        db.RunInTransaction(() => db.Update(pembelian));

                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                        var saldoKas = kas.TOTAL;
                        var saldoUtang = utang.TOTAL;

                        saldoKas += bayarBefore;
                        saldoKas -= totalBayar;

                        saldoUtang -= sisaBayarBefore;
                        saldoKas += sisaBayarBefore;
                        saldoUtang += sisaBayar;
                        saldoKas -= sisaBayar;

                        kas.TOTAL = saldoKas; utang.TOTAL = saldoUtang;
                        db.Update(kas); db.Update(utang);
                    }
                    else
                    {
                        var totalTagihan = GetNumFromCurr(txtTotalHargaBeli.Text);
                        var pembelian = db.Get<TabelPembelian>(idPembelian);
                        var tagihanBefore = pembelian.TOTAL_TAGIHAN;

                        if (metode.Contains("Cash"))
                        {
                            pembelian.REF_NOTA = no_nota;
                            pembelian.DAFTAR_STOK = listID;
                            pembelian.METODE_BAYAR = metode;
                            pembelian.ID_SUPPLIER = supplier.ID_SUPPLIER;
                            pembelian.TGL_BELI = tanggal;
                            pembelian.TOTAL_BAYAR = totalTagihan;
                            pembelian.SISA_BAYAR = 0;
                            pembelian.TOTAL_TAGIHAN = totalTagihan;
                            pembelian.LOKASI = lokasi;

                            db.RunInTransaction(() => db.Update(pembelian));

                            var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                            var saldoKas = kas.TOTAL;

                            saldoKas += tagihanBefore;
                            saldoKas -= totalTagihan;
                            kas.TOTAL = saldoKas;
                            db.Update(kas);
                        }
                        else if (metode.Contains("Utang"))
                        {
                            pembelian.REF_NOTA = no_nota;
                            pembelian.DAFTAR_STOK = listID;
                            pembelian.METODE_BAYAR = metode;
                            pembelian.ID_SUPPLIER = supplier.ID_SUPPLIER;
                            pembelian.TGL_BELI = tanggal;
                            pembelian.TOTAL_BAYAR = 0;
                            pembelian.SISA_BAYAR = totalTagihan;
                            pembelian.TOTAL_TAGIHAN = totalTagihan;
                            pembelian.LOKASI = lokasi;

                            db.RunInTransaction(() => db.Update(pembelian));

                            var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                            var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                            var saldoKas = kas.TOTAL;
                            var saldoUtang = utang.TOTAL;

                            saldoUtang -= tagihanBefore;
                            saldoKas += tagihanBefore;
                            saldoUtang += totalTagihan;
                            saldoKas -= totalTagihan;

                            db.Update(utang); db.Update(kas);
                        }
                    }
                }

                ResetTambahPembelian();
                BacaPembelian();
                closePanel.Begin(GridTambahPembelian);
            }
            else MessageBox.Show("Supplier, Metode Bayar, Lokasi Stok, dan Tanggal Masuk Waji Diisi!", "Masih Ada Kolom Kosong", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ResetTambahItemBeli()
        {
            cmbBoxKategori.SelectedItem = null;
            cmbBoxSatuan.SelectedItem = null;
            cmbBoxSatuanJual.SelectedItem = null;
            txtBoxMerek.Text = "";
            txtBoxJumlahStok.Text = "0";
            txtBoxHargaBeli.Text = "";
            txtBoxHargaJual.Text = "";
        }

        private void ResetTambahPembelian()
        {
            Pembelians.Clear();
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
                if (!isMigrasi) cmbBoxLokasi.SelectedItem = window.txtBoxNamaLokasi.Text;
                else CmbBoxLokasiStok.SelectedItem = window.txtBoxNamaLokasi.Text;
                window.Close();
            }
        }

        private void BtnAddKategori_Click(object sender, RoutedEventArgs e)
        {
            var window = new Tambah_Kategori();
            window.ShowDialog();

            if (window.DialogResult == true) {
                DataContext = new Classes.ComboBoxItem();
                if (!isMigrasi) cmbBoxKategori.SelectedItem = window.txtBoxNamaKategori.Text;
                else CmbBoxKategoriStok.SelectedItem = window.txtBoxNamaKategori.Text;
                window.Close();
            }
        }

        private void BtnDetailBeli_Click(object sender, RoutedEventArgs e)
        {
            var selected = ((DataRowView)TablePembelian.SelectedItem).Row.ItemArray;
            dialogDetailPembelian.IsOpen = true;
            
            var columns = new List<string>() { "NO", "ID_STOK", "TGL_MASUK", "DESKRIPSI", "JUMLAH_STOK", "SATUAN",
                "DALAM_GRAM", "TOTAL_HARGA", "NO_NOTA_REF", "HARGA_BELI", "HARGA_JUAL", "METODE_BAYAR", "SATUAN_JUAL" };
            var dataTable = new DataTable();
            foreach (var value in columns) { dataTable.Columns.Add(value); }
            int i = 1;
            foreach (var value in selected[4].ToString().Split(';'))
            {
                if (value != "") 
                {
                    var item = db.Get<TabelStok>(value);
                    var satuan = db.Get<TabelSatuan>(item.SATUAN);
                    var deskripsi = item.DESKRIPSI;
                    if (deskripsi == "") deskripsi = $"{item.KATEGORI} {item.MEREK}";

                    var row = dataTable.NewRow();
                    row["NO"] = i++;
                    row["ID_STOK"] = value;
                    row["TGL_MASUK"] = item.TGL_MASUK;
                    row["DESKRIPSI"] = deskripsi;
                    row["JUMLAH_STOK"] = item.JUMLAH_STOK;
                    row["SATUAN"] = item.SATUAN;
                    row["DALAM_GRAM"] = satuan.DALAM_GRAM;
                    row["HARGA_BELI"] = string.Format(culture, "{0:C0}", item.HARGA_BELI);
                    row["TOTAL_HARGA"] = string.Format(culture, "{0:C0}", item.TOTAL_HARGA);
                    row["HARGA_JUAL"] = $"{string.Format(culture, "{0:C0}", item.HARGA_JUAL)}/{item.SATUAN_JUAL}";
                    row["SATUAN_JUAL"] = item.SATUAN_JUAL;
                    dataTable.Rows.Add(row);
                }
            }

            if (dataTable.Rows.Count == 0)
            {
                TableDetailPembelian.ItemsSource = null;
                TableDetailPembelian.Visibility = Visibility.Collapsed;
            }
            else
            {
                TableDetailPembelian.ItemsSource = null;
                TableDetailPembelian.ItemsSource = dataTable.AsDataView();
                TableDetailPembelian.Visibility = Visibility.Visible;
                TxtRefNumber.Text = $"Ref. / No. Nota : {selected[1]}";
                TxtTglMasuk.Text = $"Tanggal Beli : {selected[2]}";
                TxtTagihan.Text = $"Total Tagihan : {selected[6]}";
                TxtSisaBayar.Text = $"Sisa Bayar : {selected[8]}";
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            dialogDetailPembelian.IsOpen = false;
            TableDetailPembelian.ItemsSource = null;
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
                if (!isMigrasi) cmbBoxSatuan.SelectedItem = window.txtBoxNamaSatuan.Text;
                else CmbBoxSatuanStok.SelectedItem = window.txtBoxNamaSatuan.Text;
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
                if (isUtang) CmbBoxSupplierUtang.SelectedItem = window.txtBoxNamaSupplier.Text;
                else cmbBoxSupplier.SelectedItem = window.txtBoxNamaSupplier.Text;
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
            var window = new Tampilan_Pembelian(selected[2].ToString(), selected[4].ToString(), selected[7].ToString(), db);
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

        private void BtnHapusStok_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Menghapus Stok hanya digunakan untuk stok yang tidak digunakan lagi. Menghapus stok tidak akan menghapus riwayat pembelian, dan tidak akan mengembalikan kas. Apakah Anda yakin ingin menghapus stok?", 
                "Konfirmasi Penghapusan", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) 
            {
                if (MessageBox.Show("Pikirkan kembali, apakah Anda perlu menghapus stok ini?", "Konfirmasi Penghapusan", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
                {
                    var selected = ((DataRowView)TableStok.SelectedItem).Row.ItemArray;
                    db.Delete<TabelRealStok>(int.Parse(selected[1].ToString()));
                }
            }
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

        private void CmbBoxMetodeBayar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBoxMetodeBayar.SelectedItem != null) 
            {
                if (cmbBoxMetodeBayar.SelectedItem.ToString() == "Cicilan") TxtBoxJmlhBayarBeli.IsEnabled = true;
                else TxtBoxJmlhBayarBeli.IsEnabled = false;
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

        private void ResetMigrasiStok() 
        {
            CmbBoxLokasiStok.SelectedItem = null;
            CmbBoxKategoriStok.SelectedItem = null;
            CmbBoxSatuanStok.SelectedItem = null;
            CmbBoxSatuanJualStok.SelectedItem = null;
            TxtBoxMerekStok.Text = "";
            TxtBoxJumlahStokMigrasi.Text = "";
            TxtBoxHargaBeliStok.Text = "";
            TxtBoxHargaJualStok.Text = "";
        }

        private void BtnMigrasiStok_Click(object sender, RoutedEventArgs e)
        {
            DialogMigrasiStok.IsOpen = true;
            ResetMigrasiStok();
            isMigrasi = true;
        }

        private void BtnAddStokMigrasi_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBoxLokasiStok.SelectedItem != null && CmbBoxKategoriStok.SelectedItem != null &&
                CmbBoxSatuanStok.SelectedItem != null && CmbBoxSatuanJualStok.SelectedItem != null &&
                TxtBoxMerekStok.Text != "" && TxtBoxJumlahStokMigrasi.Text != "" &&
                TxtBoxHargaBeliStok.Text != "" && TxtBoxHargaJualStok.Text != "")
            {
                var beliPrice = GetNumFromCurr(TxtBoxHargaBeliStok.Text);
                var jualPrice = GetNumFromCurr(TxtBoxHargaJualStok.Text);
                var jmlhStok = GetNumFromCurr(TxtBoxJumlahStokMigrasi.Text);
                var satuanStok = CmbBoxSatuanStok.SelectedItem.ToString();
                var satuanJual = CmbBoxSatuanJualStok.SelectedItem.ToString();
                var lokasi = CmbBoxLokasiStok.SelectedItem.ToString();
                var deskripsi = $"{CmbBoxKategoriStok.SelectedItem} {TxtBoxMerekStok.Text}";
                var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND SATUAN_STOK = ? AND HARGA_BELI = ? AND LOKASI = ?", deskripsi, satuanStok, beliPrice, lokasi).FirstOrDefault();
                var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", satuanStok).FirstOrDefault().DALAM_GRAM * jmlhStok;
                if (isAvailable == null)
                {
                    var maxStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok");
                    var idStok = 1;
                    if (maxStok.Count != 0) idStok = maxStok.Max(x => x.ID_STOK_END) + 1;

                    db.Insert(new TabelRealStok()
                    {
                        ID_STOK_END = idStok,
                        KATEGORI_MEREK = deskripsi,
                        TOTAL_STOK = jmlhStok,
                        SATUAN_STOK = satuanStok,
                        LOKASI = lokasi,
                        HARGA_BELI = beliPrice,
                        STOK_DLM_GRAM = stokGram,
                        HARGA_JUAL_PER = $"{jualPrice}/{satuanJual}"
                    });
                }
                else
                {
                    isAvailable.TOTAL_STOK += jmlhStok;
                    isAvailable.STOK_DLM_GRAM += stokGram;
                    isAvailable.HARGA_JUAL_PER = $"{jualPrice}/{satuanJual}";
                    db.RunInTransaction(() => db.Update(isAvailable));
                }
                BacaDatabase(); DialogMigrasiStok.IsOpen = false;
            }
            else MessageBox.Show("Ada data yang belum diinput, pastikan semua kolom telah diisi!", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void TxtBoxHargaJualStok_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtBoxHargaJualStok.Text != "")
            {
                try
                {
                    var hargaJual = GetNumFromCurr(TxtBoxHargaJualStok.Text);
                    if (!string.IsNullOrEmpty(TxtBoxHargaJualStok.Text))
                    {
                        TxtBoxHargaJualStok.Text = string.Format(culture, "{0:N0}", hargaJual);
                        TxtBoxHargaJualStok.Select(TxtBoxHargaJualStok.Text.Length, 0);
                    }
                }
                catch (Exception) { MessageBox.Show("Harga Jual harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void TxtBoxHargaBeliStok_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtBoxHargaBeliStok.Text != "")
            {
                try
                {
                    var hargaBeli = GetNumFromCurr(TxtBoxHargaBeliStok.Text);
                    if (!string.IsNullOrEmpty(TxtBoxHargaBeliStok.Text))
                    {
                        TxtBoxHargaBeliStok.Text = string.Format(culture, "{0:N0}", hargaBeli);
                        TxtBoxHargaBeliStok.Select(TxtBoxHargaBeliStok.Text.Length, 0);
                    }
                }
                catch (Exception) { MessageBox.Show("Harga Beli harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
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
            else BacaPenjualan();
        }

        private void BtnHapusPenjualan_Click(object sender, RoutedEventArgs e)
        {
            var item = (DataRowView)TablePenjualan.SelectedItem;
            if (item != null)
            {
                var result = MessageBox.Show("Yakin ingin menghapus data penjualan? Stok yang sebelumnya pernah dijual akan dikembalikan, serta kas yang diterima juga akan dikembalikan", "Konfirmasi Penghapusan", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var value in item.Row.ItemArray[4].ToString().Split('#'))
                    {
                        var itemDelete = value.Split(';');
                        var isAvailable = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND HARGA_JUAL_PER = ? AND LOKASI = ?", 
                            itemDelete[0], itemDelete[2].Replace(".", ""), item.Row.ItemArray[8].ToString()).FirstOrDefault();

                        if (isAvailable != null)
                        {
                            var satuanStok = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", isAvailable.SATUAN_STOK).FirstOrDefault();
                            var stokGram = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", itemDelete[2].Split('/')[1]).FirstOrDefault().DALAM_GRAM * int.Parse(itemDelete[1]);

                            double jmlhHapus = stokGram / satuanStok.DALAM_GRAM;
                            
                            isAvailable.TOTAL_STOK += jmlhHapus;
                            isAvailable.STOK_DLM_GRAM += stokGram;
                            db.Update(isAvailable);
                        }
                        else 
                        {
                            var results = MessageBox.Show("Sepertinya stok sudah dihapus dan tidak dapat dikembalikan, menghapus penjualan ini tidak akan mengembalikan stok! Apakah Anda ingin tetap melnajutkan?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                            if (results == MessageBoxResult.No) return;
                        }
                    }

                    var itemPenjualan = db.Get<TabelPenjualan>(item.Row.ItemArray[1].ToString());
                    var metode = itemPenjualan.METODE_BAYAR;

                    if (metode.Contains("Piutang") || metode.Contains("Cicilan"))
                    {
                        var piutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First();
                        var saldoPiutang = piutang.TOTAL;
                        saldoPiutang -= GetNumFromCurr(itemPenjualan.SISA_BAYAR);
                        piutang.TOTAL = saldoPiutang;
                        db.Update(piutang);

                        if (itemPenjualan.TOTAL_BAYAR != "Rp0")
                        {
                            var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                            kas.TOTAL -= GetNumFromCurr(itemPenjualan.TOTAL_BAYAR);
                            db.RunInTransaction(() => db.Update(kas));
                        }
                    }
                    else
                    {
                        var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                        db.RunInTransaction(() => { kas.TOTAL -= GetNumFromCurr(itemPenjualan.TOTAL_BAYAR); db.Update(kas); });
                    }

                    db.Delete(itemPenjualan);
                    BacaPenjualan();
                }
            }
            else MessageBox.Show("Pilih Data Yang Akan Dihapus!", "Tidak ada data dipilih", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            GridTambahPenjualan.Visibility = Visibility.Visible;
            var openPanel = (Storyboard)Resources["JualOpen"];
            openPanel.Begin(GridTambahPenjualan);
        }

        private void BtnUbahPenjualan_Click(object sender, RoutedEventArgs e)
        {
            var selected = (DataRowView)TablePenjualan.SelectedItem;

            if (selected != null) 
            {
                GridTambahPenjualan.Visibility = Visibility.Visible;
                var openPanel = (Storyboard)Resources["JualOpen"];
                openPanel.Begin(GridTambahPenjualan);
                isUbahPenjualan = true;
                item = selected.Row.ItemArray;

                txtTitleJual.Text = "Ubah Penjualan";
                BtnTambahPenjualan2.Content = "Ubah Penjualan";
                Penjualans.Clear();
                var tgl = item[2].ToString().Split('/');

                txtBoxTglJual.SelectedDate = DateTime.Parse($"{tgl[2]}/{tgl[1]}/{tgl[0]}"); ;
                cmbBoxCustomer.SelectedItem = item[3].ToString();

                var details = new List<DetailPenjualan>();
                var detail = item[4].ToString().Split('#');
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

                TableTambahPenjualan.ItemsSource = null;
                TableTambahPenjualan.ItemsSource = details;
                cmbBoxNamaBarang.SelectedItem = null;
                txtBoxJumlahBarang.Text = "";
                txtHargaSatuan.Text = "Rp0,00/Kilogram";
                txtBoxDiskon.Text = "";
                txtHargaTotal.Text = "";
                txtTotalHargaAll.Text = item[5].ToString();
                txtBoxUangTunai.Text = GetNumFromCurr(item[6].ToString()).ToString();
                cmbBoxLokasi.SelectedItem = item[8].ToString();
                cmbBoxMetodeBayarJual.SelectedItem = item[9].ToString();
                Penjualans = details;
                isUbahPenjualan = true;
            }
            else MessageBox.Show("Pilih Data Yang Akan Diubah!", "Tidak ada data dipilih", MessageBoxButton.OK, MessageBoxImage.Warning);            
        }

        private void CloseTambahJual_Click(object sender, RoutedEventArgs e)
        {
            var closePanel = (Storyboard)Resources["JualClose"];
            closePanel.Begin(GridTambahPenjualan);
            ResetTambahPenjualan();
            HideView(GridTambahPenjualan);
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
                    var metode = "Cash / Bank";
                    if (cmbBoxMetodeBayarJual.SelectedItem != null) metode = cmbBoxMetodeBayarJual.SelectedItem.ToString();
                    if (GetNumFromCurr(txtUangKembali.Text) < 0 && metode == "Cash / Bank") 
                    {
                        MessageBox.Show("Pembayaran Tidak Full, Pilih Metode Pembayaran selain Cash / Bank", "Pilih Metode Lain", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    
                    var detailJual = "";
                    var customer = db.Query<TabelCustomer>("SELECT * FROM TabelCustomer WHERE NAMA_CUSTOMER = ?", cmbBoxCustomer.SelectedItem.ToString()).FirstOrDefault();
                    var getJual = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan");                    
                    var totalJual = txtTotalHargaAll.Text;
                    var lokasi = cmbBoxLokasiJual.SelectedItem.ToString();                    

                    var id = 1;
                    if (getJual.Count != 0) id = getJual.Max(x => x.ID_JUAL) + 1;
                    foreach (var value in Penjualans)
                    {
                        if (detailJual == "") detailJual += $"{value}";
                        else detailJual += $"#{value}";
                    }

                    // Update Data Stok

                    foreach (var value in jualItemTemp)
                    {
                        var getStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND LOKASI = ?", value.NAMA_BARANG, lokasi).FirstOrDefault();
                        if (getStok == null)
                        {
                            MessageBox.Show($"Tidak ada {value.NAMA_BARANG} di {lokasi}. Pastikan Anda memilih stok dan lokasi yang sesuai", "Stok Tidak Ada", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        else 
                        {
                            getStok.TOTAL_STOK = value.JUMLAH_STOK;
                            getStok.STOK_DLM_GRAM = value.STOK_DLM_GRAM;                            
                            db.RunInTransaction(() => db.Update(getStok));
                        }
                    }

                    #region PENGATURAN METODE
                    
                    var totBayar = "Rp0";
                    var sisaBayar = "Rp0";

                    #endregion

                    if (BtnTambahPenjualan2.Content.ToString() == "Ubah Penjualan" && isUbahPenjualan == true)
                    {
                        var selected = (DataRowView)TablePenjualan.SelectedItem;
                        var sisaBayarBefore = "Rp0";
                        var bayarBefore = "Rp0";
                        if (selected != null)
                        {
                            var fix = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan WHERE ID_JUAL = ?", selected.Row.ItemArray[1]).FirstOrDefault();
                            sisaBayarBefore = fix.SISA_BAYAR;
                            bayarBefore = fix.TOTAL_BAYAR;

                            if (metode.Contains("Piutang") || metode.Contains("Cicilan") || fix.METODE_BAYAR != "Cash / Bank")
                            {
                                if (txtBoxUangTunai.Text != "") totBayar = string.Format(txtBoxUangTunai.Text);
                                sisaBayar = string.Format(culture, "{0:C0}", GetNumFromCurr(totalJual) - GetNumFromCurr(totBayar));
                                var piutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First();
                                var saldoPiutang = piutang.TOTAL;
                                saldoPiutang -= GetNumFromCurr(sisaBayarBefore);
                                saldoPiutang += GetNumFromCurr(sisaBayar);
                                piutang.TOTAL = saldoPiutang;
                                db.Update(piutang);

                                if (totBayar != "Rp0" && totBayar != "")
                                {
                                    var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                                    var saldoKas = kas.TOTAL;
                                    saldoKas -= GetNumFromCurr(bayarBefore);
                                    saldoKas += GetNumFromCurr(totBayar);
                                    kas.TOTAL = saldoKas;
                                    db.RunInTransaction(() => db.Update(kas));
                                }
                            }
                            else
                            {
                                var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                                db.RunInTransaction(() => { kas.TOTAL += double.Parse(totalJual.Replace(".", "").Replace("Rp", "")); db.Update(kas); });
                                totBayar = totalJual;
                            }
                            
                            fix.CUSTOMER = customer.ID_CUSTOMER;
                            fix.DETAIL_JUAL = detailJual;
                            fix.TANGGAL_JUAL = txtBoxTglJual.SelectedDate.Value.ToString("dd/MM/yyyy");
                            fix.TOTAL_PENJUALAN = totalJual;
                            fix.METODE_BAYAR = metode;
                            fix.TOTAL_BAYAR = totBayar;
                            fix.SISA_BAYAR = sisaBayar;
                            fix.LOKASI_STOK = lokasi;
                            db.RunInTransaction(() => db.Update(fix));
                        }                        
                    }
                    else 
                    {
                        if (metode.Contains("Piutang") || metode.Contains("Cicilan"))
                        {
                            if (txtBoxUangTunai.Text != "") totBayar = string.Format(txtBoxUangTunai.Text);
                            sisaBayar = string.Format(culture, "{0:C0}", GetNumFromCurr(totalJual) - GetNumFromCurr(totBayar));
                            var piutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First();
                            db.RunInTransaction(() => { piutang.TOTAL += double.Parse(GetNumFromCurr(sisaBayar).ToString()); db.Update(piutang); });

                            if (totBayar != "Rp0" && totBayar != "")
                            {
                                var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                                db.RunInTransaction(() => { kas.TOTAL += GetNumFromCurr(totBayar); db.Update(kas); });
                            }
                        }
                        else
                        {
                            var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                            db.RunInTransaction(() => { kas.TOTAL += double.Parse(totalJual.Replace(".", "").Replace("Rp", "")); db.Update(kas); });
                            totBayar = totalJual;
                        }

                        db.Insert(new TabelPenjualan()
                        {
                            ID_JUAL = id,
                            CUSTOMER = customer.ID_CUSTOMER,
                            DETAIL_JUAL = detailJual,
                            TANGGAL_JUAL = txtBoxTglJual.SelectedDate.Value.ToString("dd/MM/yyyy"),
                            TOTAL_PENJUALAN = totalJual,
                            METODE_BAYAR = metode,
                            TOTAL_BAYAR = totBayar,
                            SISA_BAYAR = sisaBayar,
                            LOKASI_STOK = lokasi
                        });
                    }

                    BacaPenjualan();
                    jualItemTemp.Clear();
                    ((Storyboard)Resources["JualClose"]).Begin(GridTambahPenjualan);
                    ResetTambahPenjualan();
                }
                else MessageBox.Show("Tambahkan Item Dibeli Terlebih Dahulu Sebelum Menambahkan Penjualan", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("Anda harus Memilih Customer, Tanggal Jual, dan Lokasi Stok Untuk Melanjutkan", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private int GetNumFromCurr(string rawCurrency) 
        {
            try
            {
                return int.Parse(rawCurrency.Replace("Rp", "").Replace(".", ""));
            }
            catch 
            {
                return 0;
            }
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
                    if (BtnAddSaleItem.Content.ToString() == "Ubah Item")
                    {
                        var selected = (DetailPenjualan)TableTambahPenjualan.SelectedItem;
                        var index = Penjualans.FindIndex(c => c.NAMA_BARANG == selected.NAMA_BARANG && c.HARGA_TOTAL == selected.HARGA_TOTAL);

                        var jualItem = new DetailPenjualan()
                        {
                            NAMA_BARANG = namaBarang, HARGA_BELI = getStok.HARGA_BELI,
                            HARGA_SATUAN = txtHargaSatuan.Text,
                            JUMLAH_BELI = jumlahBeli.ToString(),
                            DISKON = txtBoxDiskon.Text,
                            HARGA_TOTAL = txtHargaTotal.Text
                        };

                        var totalStok = getStok.TOTAL_STOK + int.Parse(selected.JUMLAH_BELI) - jumlahBeli;
                        var stokGram = getStok.STOK_DLM_GRAM + (satuan.DALAM_GRAM * int.Parse(selected.JUMLAH_BELI)) - (satuan.DALAM_GRAM * jumlahBeli);

                        if (jualItemTemp.Count == 0) jualItemTemp.Add(new MinJualTemp() { NAMA_BARANG = namaBarang, JUMLAH_STOK = totalStok, STOK_DLM_GRAM = stokGram });

                        var stokTemp = jualItemTemp.Where(x => x.NAMA_BARANG == namaBarang);
                        if (stokTemp.Count() == 0)
                        {
                            jualItemTemp.Add(new MinJualTemp() { NAMA_BARANG = namaBarang, JUMLAH_STOK = totalStok, STOK_DLM_GRAM = stokGram });
                            stokTemp = jualItemTemp.Where(x => x.NAMA_BARANG == namaBarang);
                        }

                        var finalStok = stokTemp.FirstOrDefault();
                        if (finalStok.JUMLAH_STOK >= jumlahBeli)
                        {
                            finalStok.JUMLAH_STOK = finalStok.JUMLAH_STOK + int.Parse(selected.JUMLAH_BELI) - jumlahBeli;
                            finalStok.STOK_DLM_GRAM = finalStok.STOK_DLM_GRAM + (satuan.DALAM_GRAM * int.Parse(selected.JUMLAH_BELI)) - (satuan.DALAM_GRAM * jumlahBeli);

                            Penjualans[index] = jualItem;
                            TableTambahPenjualan.ItemsSource = null;
                            TableTambahPenjualan.ItemsSource = Penjualans;

                            if (txtTotalHargaAll.Text == "Rp0,00") txtTotalHargaAll.Text = jualItem.HARGA_TOTAL;
                            else
                            {
                                var harTot = GetNumFromCurr(jualItem.HARGA_TOTAL);
                                var harSem = GetNumFromCurr(txtTotalHargaAll.Text);
                                txtTotalHargaAll.Text = string.Format(culture, "{0:C0}", harSem - GetNumFromCurr(selected.HARGA_TOTAL) + harTot);
                            }
                        }
                        else MessageBox.Show("Stok Item Tidak Mencukupi", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        var jualItem = new DetailPenjualan()
                        {
                            NAMA_BARANG = namaBarang, HARGA_BELI = getStok.HARGA_BELI,
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
                    if (BtnAddSaleItem.Content.ToString() == "Ubah Item") BtnAddSaleItem.Content = "Tambah Item";

                    cmbBoxNamaBarang.SelectedItem = null;
                    txtBoxJumlahBarang.Text = "";
                    txtHargaSatuan.Text = "Rp0,00/Kilogram";
                    txtHargaTotal.Text = "Rp0,00";
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
                if (isPiutang) CmbBoxCustomerPiutang.SelectedItem = window.customerName;
                else cmbBoxCustomer.SelectedItem = window.customerName;
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
                        txtUangKembali.Text = string.Format(culture, "{0:C0}", value - GetNumFromCurr(txtTotalHargaAll.Text));
                    }
                }
                catch (Exception) { MessageBox.Show("Uang Tunai harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnHapusItemJual_Click(object sender, RoutedEventArgs e)
        {
            var selected = (DetailPenjualan)TableTambahPenjualan.SelectedItem;
            var harSem = GetNumFromCurr(txtTotalHargaAll.Text);
            txtTotalHargaAll.Text = string.Format(culture, "{0:C0}", harSem - GetNumFromCurr(selected.HARGA_TOTAL));
            Penjualans.Remove(selected);
            TableTambahPenjualan.ItemsSource = null;
            TableTambahPenjualan.ItemsSource = Penjualans;
        }

        private void BtnUbahItemJual_Click(object sender, RoutedEventArgs e)
        {
            var selected = (DetailPenjualan)TableTambahPenjualan.SelectedItem;            
            BtnAddSaleItem.Content = "Ubah Item";
            cmbBoxNamaBarang.SelectedItem = selected.NAMA_BARANG;
            txtBoxJumlahBarang.Text = selected.JUMLAH_BELI;
            txtHargaSatuan.Text = selected.HARGA_SATUAN;
            txtBoxDiskon.Text = selected.DISKON;
            txtHargaTotal.Text = selected.HARGA_TOTAL;
        }

        #endregion

        #region TRANSAKSI LAIN

        private string GetNamaBulan(string angka) 
        {
            switch (int.Parse(angka)) 
            {
                case 1: return "Januari";
                case 2: return "Februari";
                case 3: return "Maret";
                case 4: return "April";
                case 5: return "Mei";
                case 6: return "Juni";
                case 7: return "Juli";
                case 8: return "Agustus";
                case 9: return "September";
                case 10: return "Oktober";
                case 11: return "November";
                case 12: return "Desember";
                default: return "";
            }
        }

        private void BacaUtangPiutang() 
        {
            var getUtang = db.Query<TabelPembelian>("SELECT * FROM TabelPembelian WHERE SISA_BAYAR != '0' AND SISA_BAYAR != 'Rp0'");
            var getPiutang = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan WHERE SISA_BAYAR != '0' AND SISA_BAYAR != 'Rp0'");
            var getRiwUtang = db.Query<RiwayatBayarUtang>("SELECT * FROM RiwayatBayarUtang");
            var getRiwPiutang = db.Query<RiwayatBayarPiutang>("SELECT * FROM RiwayatBayarPiutang");

            var dataUtang = new ObservableCollection<ManajemenUtang>();
            var dataPiutang = new ObservableCollection<ManajemenPiutang>();
            var riwUtang = new ObservableCollection<ManajemenUtang>();
            var riwPiutang = new ObservableCollection<ManajemenPiutang>(); 
            foreach (var value in getUtang) {
                var supplier = db.Get<TabelSupplier>(value.ID_SUPPLIER);
                dataUtang.Add(new ManajemenUtang() 
                {
                    REF_NOTA = value.REF_NOTA,
                    BULAN = GetNamaBulan(value.TGL_BELI.Split('/')[1]),
                    TGL_BELI = value.TGL_BELI,
                    SUPPLIER = supplier.NAMA_SUPPLIER,
                    TOTAL_TAGIHAN = string.Format(culture, "{0:C0}", value.TOTAL_TAGIHAN),
                    TOTAL_BAYAR = string.Format(culture, "{0:C0}", value.TOTAL_BAYAR),
                    SISA_BAYAR = string.Format(culture, "{0:C0}", value.SISA_BAYAR)
                }); 
            }
            foreach (var value in getPiutang)
            {
                var customer = db.Get<TabelCustomer>(value.CUSTOMER);
                dataPiutang.Add(new ManajemenPiutang()
                {
                    ID = value.ID_JUAL,
                    BULAN = GetNamaBulan(value.TANGGAL_JUAL.Split('/')[1]),
                    TANGGAL_JUAL = value.TANGGAL_JUAL,
                    CUSTOMER = customer.NAMA_CUSTOMER,
                    TOTAL_PENJUALAN = string.Format(culture, "{0:C0}", value.TOTAL_PENJUALAN),
                    TOTAL_BAYAR = string.Format(culture, "{0:C0}", value.TOTAL_BAYAR),
                    SISA_BAYAR = string.Format(culture, "{0:C0}", value.SISA_BAYAR)
                });
            }
            foreach (var value in getRiwUtang)
            {
                var pembelian = db.Get<TabelPembelian>(value.REF_NOTA_BELI);
                var supplier = db.Get<TabelSupplier>(pembelian.ID_SUPPLIER);
                riwUtang.Add(new ManajemenUtang()
                {
                    REF_NOTA = value.REF_NOTA_BELI,
                    BULAN = GetNamaBulan(value.TGL_BAYAR.Split('/')[1]),
                    TGL_BELI = value.TGL_BAYAR,
                    SUPPLIER = supplier.NAMA_SUPPLIER,
                    TOTAL_TAGIHAN = string.Format(culture, "{0:C0}", pembelian.TOTAL_TAGIHAN),
                    TOTAL_BAYAR = string.Format(culture, "{0:C0}", value.TOTAL_BAYAR)
                });
            }
            foreach (var value in getRiwPiutang)
            {
                var penjualan = db.Get<TabelPenjualan>(value.ID_BELI);
                var customer = db.Get<TabelCustomer>(penjualan.CUSTOMER);
                riwPiutang.Add(new ManajemenPiutang()
                {
                    ID = value.ID_BELI,
                    BULAN = GetNamaBulan(value.TGL_BAYAR.Split('/')[1]),
                    TANGGAL_JUAL = value.TGL_BAYAR,
                    CUSTOMER = customer.NAMA_CUSTOMER,
                    TOTAL_PENJUALAN = string.Format(culture, "{0:C0}", penjualan.TOTAL_PENJUALAN),
                    TOTAL_BAYAR = string.Format(culture, "{0:C0}", value.TOTAL_BAYAR)
                });
            }

            ListCollectionView collection1 = new ListCollectionView(dataUtang);
            collection1.GroupDescriptions.Add(new PropertyGroupDescription("BULAN"));
            TableUtang.ItemsSource = null;
            TableUtang.ItemsSource = collection1;

            ListCollectionView collection2 = new ListCollectionView(dataPiutang);
            collection2.GroupDescriptions.Add(new PropertyGroupDescription("BULAN"));
            TablePiutang.ItemsSource = null;
            TablePiutang.ItemsSource = collection2;

            ListCollectionView collection3 = new ListCollectionView(riwUtang);
            collection3.GroupDescriptions.Add(new PropertyGroupDescription("BULAN"));
            TableRiwayatUtang.ItemsSource = null;
            TableRiwayatUtang.ItemsSource = collection3;

            ListCollectionView collection4 = new ListCollectionView(riwPiutang);
            collection4.GroupDescriptions.Add(new PropertyGroupDescription("BULAN"));
            TableRiwayatPiutang.ItemsSource = null;
            TableRiwayatPiutang.ItemsSource = collection4;
        }

        private void BtnInputUtang_Click(object sender, RoutedEventArgs e)
        {
            DialogInputUtang.IsOpen = true;
            isUtang = true;
        }

        private void ResetAddUtang() 
        {
            CmbBoxSupplierUtang.SelectedItem = null;
            TxtBoxTglMasukUtang.Text = "";
            TxtBoxTagihanUtang.Text = "";
            TxtBoxRefNotaUtang.Text = "";
        }

        private void BtnAddUtang_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBoxSupplierUtang.SelectedItem != null && TxtBoxTglMasukUtang.Text != "" &&
                TxtBoxTagihanUtang.Text != "") 
            {
                var no_nota = "-"; var idPembelian = "1";
                var getIDpembelian = db.Query<TabelPembelian>("SELECT * FROM TabelPembelian");
                if (getIDpembelian.Count() != 0) idPembelian = getIDpembelian.Max(x => int.Parse(x.REF_NOTA.Split('-')[1]) + 1).ToString();
                var supplier = db.Query<TabelSupplier>("SELECT * FROM TabelSupplier WHERE NAMA_SUPPLIER = ?", CmbBoxSupplierUtang.SelectedItem.ToString()).FirstOrDefault();
                if (TxtBoxRefNotaUtang.Text != "") no_nota = TxtBoxRefNotaUtang.Text;
                else no_nota = $"{TxtBoxTglMasukUtang.SelectedDate.Value:yyyyMMdd}{AddPrefix(supplier.ID_SUPPLIER.ToString(), 2)}-{AddPrefix(idPembelian, 5)}";
                var tagihan = GetNumFromCurr(TxtBoxTagihanUtang.Text);

                db.Insert(new TabelPembelian()
                {
                    REF_NOTA = no_nota,
                    DAFTAR_STOK = "UTANG_MIGRASI",
                    METODE_BAYAR = "Utang",
                    ID_SUPPLIER = supplier.ID_SUPPLIER,
                    TGL_BELI = TxtBoxTglMasukUtang.SelectedDate.Value.ToString("dd/MM/yyyy"),
                    TOTAL_BAYAR = 0,
                    SISA_BAYAR = tagihan,
                    LOKASI = "UTANG_MIGRASI",
                    TOTAL_TAGIHAN = tagihan
                });

                var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                db.RunInTransaction(() => { utang.TOTAL += tagihan; db.Update(utang); });

                BacaUtangPiutang(); ResetAddUtang(); DialogInputUtang.IsOpen = false;
            }
            else MessageBox.Show("Ada data yang belum diinput, pastikan semua kolom telah diisi kecuali Ref. / No. Nota!", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        //private void TxtBoxJumlahBayar_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var txtNilaiBayar = TxtBoxJumlahBayar.Text;
        //    if (txtNilaiBayar != "")
        //    {
        //        try
        //        {
        //            var value = int.Parse(TxtBoxJumlahBayar.Text.Replace(".", ""));
        //            if (value > GetNumFromCurr(TxtSisaBayarUtang.Text))
        //            {
        //                txtBoxBayarUtang.Text = string.Format(culture, "{0:N0}", value);
        //                txtBoxBayarUtang.Select(txtBoxBayarUtang.Text.Length, 0);
        //                txtSisaUtang.Text = string.Format(culture, "{0:C0}", GetNumFromCurr(TxtSisaBayarUtang.Text.Replace("Sisa Bayar : ", "")) - value);
        //            }
        //            else
        //            {
        //                txtBoxBayarUtang.Text = "0";
        //                MessageBox.Show("Pembayaran melebihi sisa pembayaran!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            }
        //        }
        //        catch (Exception) { MessageBox.Show("Utang yang dibayar harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
        //    }
        //}

        private void TxtBoxTagihanUtang_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtTotalUtang = TxtBoxTagihanUtang.Text;
            if (txtTotalUtang != "")
            {
                try
                {
                    var value = int.Parse(TxtBoxTagihanUtang.Text.Replace(".", ""));
                    TxtBoxTagihanUtang.Text = string.Format(culture, "{0:N0}", value);
                    TxtBoxTagihanUtang.Select(TxtBoxTagihanUtang.Text.Length, 0);
                }
                catch (Exception) { MessageBox.Show("Utang harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnInputPiutang_Click(object sender, RoutedEventArgs e)
        {
            DialogInputPiutang.IsOpen = true;
            isPiutang = true;
        }

        private void ResetAddPiutang()
        {
            CmbBoxCustomerPiutang.SelectedItem = null;
            TxtBoxTglPiutang.Text = "";
            TxtBoxTotalPiutang.Text = "";
        }

        private void BtnAddPiutang_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBoxCustomerPiutang.SelectedItem != null && TxtBoxTglPiutang.Text != "" &&
                TxtBoxTotalPiutang.Text != "")
            {
                var ID = 1;
                var getJual = db.Query<TabelPenjualan>("SELECT * FROM TabelPenjualan");
                if (getJual.Count != 0) ID = getJual.Max(x => x.ID_JUAL) + 1;
                var customer = db.Query<TabelCustomer>("SELECT * FROM TabelCustomer WHERE NAMA_CUSTOMER = ?", CmbBoxCustomerPiutang.SelectedItem.ToString()).FirstOrDefault();
                var tagihan = TxtBoxTotalPiutang.Text;

                db.Insert(new TabelPenjualan()
                {
                    ID_JUAL = ID,
                    CUSTOMER = customer.ID_CUSTOMER,
                    DETAIL_JUAL = "PIUTANG_MIGRASI",
                    TANGGAL_JUAL = TxtBoxTglPiutang.SelectedDate.Value.ToString("dd/MM/yyyy"),
                    TOTAL_PENJUALAN = tagihan,
                    METODE_BAYAR = "Piutang",
                    TOTAL_BAYAR = "0",
                    SISA_BAYAR = tagihan,
                    LOKASI_STOK = "PIUTANG_MIGRASI"
                });

                var piutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First();
                db.RunInTransaction(() => { piutang.TOTAL += double.Parse(GetNumFromCurr(tagihan).ToString()); db.Update(piutang); });

                BacaUtangPiutang(); ResetAddPiutang(); DialogInputPiutang.IsOpen = false;
            }
            else MessageBox.Show("Ada data yang belum diinput, pastikan semua kolom telah diisi", "Mohon Diperhatikan", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void TxtBoxTotalPiutang_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtTotalPiutang = TxtBoxTotalPiutang.Text;
            if (txtTotalPiutang != "")
            {
                try
                {
                    var value = int.Parse(TxtBoxTotalPiutang.Text.Replace(".", ""));
                    TxtBoxTotalPiutang.Text = string.Format(culture, "{0:N0}", value);
                    TxtBoxTotalPiutang.Select(TxtBoxTotalPiutang.Text.Length, 0);
                }
                catch (Exception) { MessageBox.Show("Piutang harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnBayarUtang_Click(object sender, RoutedEventArgs e)
        {
            var selected = (ManajemenUtang)TableUtang.SelectedItem;
            if (selected != null)
            {
                DialogBayarUtang.IsOpen = true;
                TxtRefNumberUtang.Text = $"No. Ref / Nota : {selected.REF_NOTA}";
                TxtTglMasukUtang.Text = $"Tanggal Masuk : {selected.TGL_BELI}";
                TxtTagihanUtang.Text = $"Total Tagihan : {selected.TOTAL_TAGIHAN}";
                TxtSisaBayarUtang.Text = $"Sisa Bayar : {selected.SISA_BAYAR}";
            }
            else MessageBox.Show("Tidak Ada Data Utang yang dipilih, Silakan pIlih Terlebiyh Dahulu", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void TxtBoxBayarUtang_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtNilaiBayar = txtBoxBayarUtang.Text;
            if (txtNilaiBayar != "")
            {
                try
                {
                    var value = int.Parse(txtBoxBayarUtang.Text.Replace(".", ""));
                    if (value > GetNumFromCurr(TxtSisaBayarUtang.Text))
                    {
                        txtBoxBayarUtang.Text = string.Format(culture, "{0:N0}", value);
                        txtBoxBayarUtang.Select(txtBoxBayarUtang.Text.Length, 0);
                        txtSisaUtang.Text = string.Format(culture, "{0:C0}", GetNumFromCurr(TxtSisaBayarUtang.Text.Replace("Sisa Bayar : ", "")) - value);
                    }
                    else 
                    {
                        txtBoxBayarUtang.Text = "0";
                        MessageBox.Show("Pembayaran melebihi sisa pembayaran!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);                        
                    }
                }
                catch (Exception) { MessageBox.Show("Utang yang dibayar harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnOkeBayarUtang_Click(object sender, RoutedEventArgs e)
        {
            var jumlahBayar = txtBoxBayarUtang.Text;
            if (jumlahBayar != "" || jumlahBayar != "0")
            {
                var refNota = TxtRefNumberUtang.Text.Split(':')[1].Replace(" ", "");
                var dataBeli = db.Get<TabelPembelian>(refNota);
                var tglBayar = DateTime.Today.ToString("dd/MM/yyyy");
                if (txtBoxTglBayarUtang.Text != "") tglBayar = txtBoxTglBayarUtang.SelectedDate.Value.ToString("dd/MM/yyyy");

                dataBeli.TOTAL_BAYAR += GetNumFromCurr(jumlahBayar);
                dataBeli.SISA_BAYAR -= GetNumFromCurr(jumlahBayar);
                db.Update(dataBeli);

                var id = 1;
                var getRiwayat = db.Table<RiwayatBayarUtang>();
                if (getRiwayat.Count() != 0) id = getRiwayat.Max(x => x.ID) + 1;
                db.Insert(new RiwayatBayarUtang()
                {
                    ID = id,
                    REF_NOTA_BELI = refNota,
                    TGL_BAYAR = tglBayar,
                    TOTAL_BAYAR = jumlahBayar,                    
                });

                var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();

                kas.TOTAL -= GetNumFromCurr(jumlahBayar);
                utang.TOTAL -= GetNumFromCurr(jumlahBayar);
                db.Update(kas); db.Update(utang);

                BacaUtangPiutang();
                DialogBayarUtang.IsOpen = false;
            }
            else MessageBox.Show("Jumlah dibayar tidak boleh kosong!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnBayarPiutang_Click(object sender, RoutedEventArgs e)
        {
            var selected = (ManajemenPiutang)TablePiutang.SelectedItem;
            if (selected != null)
            {
                DialogBayarPiutang.IsOpen = true;
                TxtTglBeli.Text = $"Tanggal Jual : {selected.TANGGAL_JUAL}";
                TxtCustomer.Text = $"Customer : {selected.CUSTOMER}";
                TxtPenjualanPiutang.Text = $"Total Penjualan : {selected.TOTAL_PENJUALAN}";
                TxtSisaBayarPiutang.Text = $"Sisa Bayar : {selected.SISA_BAYAR}";
            }
            else MessageBox.Show("Tidak Ada Data Utang yang dipilih, Silakan pIlih Terlebiyh Dahulu", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void TxtBoxBayarPiutang_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtNilaiBayar = txtBoxBayarPiutang.Text;
            if (txtNilaiBayar != "")
            {
                try
                {
                    var value = int.Parse(txtBoxBayarPiutang.Text.Replace(".", ""));
                    if (value > GetNumFromCurr(TxtSisaBayarPiutang.Text))
                    {
                        txtBoxBayarPiutang.Text = string.Format(culture, "{0:N0}", value);
                        txtBoxBayarPiutang.Select(txtBoxBayarPiutang.Text.Length, 0);
                        txtSisaPiutang.Text = string.Format(culture, "{0:C0}", GetNumFromCurr(TxtSisaBayarPiutang.Text.Replace("Sisa Bayar : ", "")) - value);
                    }
                    else
                    {
                        txtBoxBayarPiutang.Text = "0";
                        MessageBox.Show("Pembayaran melebihi sisa pembayaran!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception) { MessageBox.Show("Piutang yang dibayar harus berupa angka dan Tidak Boleh melebihi 5 M!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnOkeBayarPiutang_Click(object sender, RoutedEventArgs e)
        {
            var jumlahBayar = txtBoxBayarPiutang.Text;
            if (jumlahBayar != "" || jumlahBayar != "0")
            {
                var selected = ((ManajemenPiutang)TablePiutang.SelectedItem).ID;
                var dataJual = db.Get<TabelPenjualan>(selected);
                var tglBayar = DateTime.Today.ToString("dd/MM/yyyy");
                if (txtBoxTglBayarPiutang.Text != "") tglBayar = txtBoxTglBayarPiutang.SelectedDate.Value.ToString("dd/MM/yyyy");

                dataJual.TOTAL_BAYAR = string.Format(culture, "{0:N0}", GetNumFromCurr(dataJual.TOTAL_BAYAR) + GetNumFromCurr(jumlahBayar));
                dataJual.SISA_BAYAR = string.Format(culture, "{0:C0}", GetNumFromCurr(dataJual.SISA_BAYAR) - GetNumFromCurr(jumlahBayar));
                db.Update(dataJual);

                var id = 1;
                var getRiwayat = db.Table<RiwayatBayarPiutang>();
                if (getRiwayat.Count() != 0) id = getRiwayat.Max(x => x.ID) + 1;
                db.Insert(new RiwayatBayarPiutang()
                {
                    ID = id, ID_BELI = selected,
                    TGL_BAYAR = tglBayar,
                    TOTAL_BAYAR = jumlahBayar,
                });

                var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                var piutang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Piutang'").First();

                kas.TOTAL += GetNumFromCurr(jumlahBayar);
                piutang.TOTAL -= GetNumFromCurr(jumlahBayar);
                db.Update(kas); db.Update(piutang);

                BacaUtangPiutang();
                DialogBayarPiutang.IsOpen = false;
            }
            else MessageBox.Show("Jumlah dibayar tidak boleh kosong!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion

        #region LAPORAN

        private void BacaBeban() 
        {
            var getBeban = db.Query<TabelBeban>("SELECT * FROM TabelBeban");

            if (getBeban.Count != 0)
            {
                var dataBeban = new ObservableCollection<ManajemenBeban>();

                foreach (var value in getBeban)
                {
                    dataBeban.Add(new ManajemenBeban()
                    {
                        ID_BEBAN = value.ID_BEBAN,
                        BULAN = GetNamaBulan(value.TANGGAL_BEBAN.Split('/')[1]),
                        TANGGAL_BEBAN = value.TANGGAL_BEBAN,
                        JENIS = value.JENIS,
                        KETERANGAN = value.KETERANGAN,
                        JUMLAH = value.JUMLAH,
                        NILAI_BEBAN = value.NILAI_BEBAN,
                        TOTAL_BEBAN = value.TOTAL_BEBAN
                    });
                }

                ListCollectionView collection1 = new ListCollectionView(dataBeban);
                collection1.GroupDescriptions.Add(new PropertyGroupDescription("BULAN"));
                TableBeban.ItemsSource = null;
                PanelDaftarBeban.Visibility = Visibility.Visible;
                NoRiwayatBeban.Visibility = Visibility.Collapsed;
                TableBeban.ItemsSource = collection1;
            }
            else 
            {
                TableBeban.ItemsSource = null;
                PanelDaftarBeban.Visibility = Visibility.Collapsed;
                NoRiwayatBeban.Visibility = Visibility.Visible;
            }
        }

        private void BacaKas()
        {
            var getTransaksi = db.Query<TabelTransaksi>("SELECT * FROM TabelTransaksi");

            if (getTransaksi.Count != 0)
            {
                TableKas.ItemsSource = null;
                PanelDaftarKas.Visibility = Visibility.Visible;
                NoRiwayatKas.Visibility = Visibility.Collapsed;
                TableKas.ItemsSource = getTransaksi;
            }
            else
            {
                TableKas.ItemsSource = null;
                PanelDaftarKas.Visibility = Visibility.Collapsed;
                PanelDaftarKas.Visibility = Visibility.Visible;
            }
        }

        private void CloseTambahBebTrak_Click(object sender, RoutedEventArgs e)
        {
            DrawerBeban.IsRightDrawerOpen = false;
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
            txtTitleBebTrak.Text = "Tambah Beban";
            BtnTambahBeban2.Content = "Tambah Beban";
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
                    ResetTambahBeban(); DrawerBeban.IsRightDrawerOpen = false;
                    BacaBeban();
                }
                else MessageBox.Show("Ada Field yang belum Terisi. Semua Field Wajib Diisi!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (txtTitleBebTrak.Text == "Ubah Beban")
            {
                var selected = (ManajemenBeban)TableBeban.SelectedItem;
                if (txtBoxTglBeban.Text != "" && txtBoxKetBeban.Text != "" && cmbBoxJenisBeban.SelectedItem != null && txtBoxNilaiBeban.Text != "" && TxtBoxJumlahBeban.Text != "")
                {
                    var nilai = TxtTotalBeban.Text.Replace("Rp", "");
                    var updated = db.Get<TabelBeban>(selected.ID_BEBAN);

                    updated.JENIS = cmbBoxJenisBeban.SelectedItem.ToString();
                    updated.KETERANGAN = txtBoxKetBeban.Text;
                    updated.TANGGAL_BEBAN = txtBoxTglBeban.SelectedDate.Value.ToString("dd/MM/yyyy");
                    updated.NILAI_BEBAN = txtBoxNilaiBeban.Text;
                    updated.JUMLAH = TxtBoxJumlahBeban.Text;
                    updated.TOTAL_BEBAN = nilai;

                    db.Update(updated);

                    var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();
                    var saldo = kas.TOTAL;
                    saldo += GetNumFromCurr(selected.TOTAL_BEBAN);
                    saldo -= GetNumFromCurr(nilai);
                    kas.TOTAL = saldo;
                    db.Update(kas);
                    ResetTambahBeban(); DrawerBeban.IsRightDrawerOpen = false;
                    BacaBeban();
                }
                else MessageBox.Show("Ada Field yang belum Terisi. Semua Field Wajib Diisi!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (txtTitleBebTrak.Text == "Tambah Transaksi Kas")
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
                        ID_TRANSAKSI = id,
                        TGL_TRANSAKSI = txtBoxTglTransaksi.SelectedDate.Value.ToString("dd/MM/yyyy"),
                        KETERANGAN = txtBoxKetTransaksi.Text,
                        NAMA_FIELD_TOTAL = field,
                        NILAI = nilai,
                        METODE_BAYAR = method
                    });

                    var kas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Kas'").First();

                    var ekuitas = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Ekuitas'").First();
                    var utang = db.Query<TabelTotal>("SELECT * FROM TabelTotal WHERE NAMA_FIELD = 'Utang'").First();
                    if (field.Contains("Penambahan"))
                    {
                        if (method.Contains("Utang")) db.RunInTransaction(() => { ekuitas.TOTAL += nilai; utang.TOTAL += nilai; kas.TOTAL += nilai; db.Update(utang); db.Update(kas); db.Update(ekuitas); });
                        else db.RunInTransaction(() => { ekuitas.TOTAL += nilai; kas.TOTAL += nilai; db.Update(ekuitas); db.Update(kas); });
                    }
                    else if (field.Contains("Pengurangan")) db.RunInTransaction(() => { ekuitas.TOTAL -= nilai; kas.TOTAL -= nilai; db.Update(ekuitas); db.Update(kas); });                    

                    ResetTambahTransaksi(); DrawerBeban.IsRightDrawerOpen = false;
                    BacaBeban();
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
            DrawerBeban.IsRightDrawerOpen = true;
        }

        private void BtnUbahBeban_Click(object sender, RoutedEventArgs e)
        {
            var selected = (ManajemenBeban)TableBeban.SelectedItem;
            if (selected != null) 
            {
                txtBoxTglBeban.SelectedDate = DateTime.Parse(selected.TANGGAL_BEBAN);
                txtBoxKetBeban.Text = selected.KETERANGAN;
                cmbBoxJenisBeban.SelectedItem = selected.JENIS;
                txtBoxNilaiBeban.Text = selected.NILAI_BEBAN;
                TxtBoxJumlahBeban.Text = selected.JUMLAH;
                TxtTotalBeban.Text = selected.TOTAL_BEBAN;

                txtTitleBebTrak.Text = "Ubah Beban";
                BtnTambahBeban2.Content = "Ubah Beban";
                DrawerBeban.IsRightDrawerOpen = true;
            }
            else MessageBox.Show("Tidak Ada Beban Dipilih!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnHapusBeban_Click(object sender, RoutedEventArgs e)
        {
            var selected = (ManajemenBeban)TableBeban.SelectedItem;
            if (selected != null && MessageBox.Show("Apakah Anda yakin ingin menghapus beban dipilih?", "Konfirmasi Penghapusan", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            { 
                db.Delete<TabelBeban>(selected.ID_BEBAN);
                BacaBeban();
            }
            else MessageBox.Show("Tidak Ada Beban Dipilih!", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnTambahTransaksi_Click(object sender, RoutedEventArgs e)
        {
            txtTitleBebTrak.Text = "Tambah Transaksi Kas";
            BtnTambahBeban2.Content = "Tambah Transaksi Kas";
            panelBeban.Visibility = Visibility.Collapsed;
            panelTransaksi.Visibility = Visibility.Visible;
            DrawerBeban.IsRightDrawerOpen = true;
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
                case "Januari": index = "01"; break;
                case "Februari": index = "02"; break;
                case "Maret": index = "03"; break;
                case "April": index = "04"; break;
                case "Mei": index = "05"; break;
                case "Juni": index = "06"; break;
                case "Juli": index = "07"; break;
                case "Agustus": index = "08"; break;
                case "September": index = "09"; break;
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
            tabelPendapatan.Columns.Add(new TableColumn() { Width = new GridLength(170) });
            tabelPendapatan.Columns.Add(new TableColumn() { Width = new GridLength(170) });

            var intArray = new int[] { 0, 1, 2 };
            var headerName = new string[] { "Pendapatan", "\tCash / Bank", "\tPiutang", "Pendapatan Total", "Pembelian Stok (HPP)" };
            var penjualan = db.Query<TabelPenjualan>($"SELECT * FROM TabelPenjualan WHERE TANGGAL_JUAL LIKE '%/{nomorBulan}/%'");

            var cashBank = 0; var piutang = 0; var hpp = 0D;
            if (penjualan.Count != 0)
            {
                cashBank = penjualan.Sum(x => GetNumFromCurr(x.TOTAL_BAYAR));
                piutang = penjualan.Sum(x => GetNumFromCurr(x.SISA_BAYAR));

                var listHargaJual = new List<double>();
                foreach (var value in penjualan) 
                {
                    foreach (var nilai in value.DETAIL_JUAL.Split('#')) 
                    {
                        var detailJual = nilai.Split(';');
                        listHargaJual.Add(int.Parse(detailJual[1]) * int.Parse(detailJual[2]));
                    }
                }

                hpp = listHargaJual.Sum();
            } 
            var totalPendapatan = cashBank + piutang;

            for (int n = 0; n < headerName.Length; n++)
            {
                tabelPendapatan.RowGroups[0].Rows.Add(new TableRow());
                var rowContent = tabelPendapatan.RowGroups[0].Rows[n];

                if (n == 0) rowContent.Cells.Add(NewCell(headerName[n], "SemiBold"));
                else rowContent.Cells.Add(NewCell(headerName[n], "Normal"));

                if (n == 1)
                {
                    rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", cashBank), "Normal", TextAlignment.Right));
                    rowContent.Cells.Add(NewCell("", "Normal"));
                }
                else if (n == 2)
                {
                    rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", piutang), "Normal", TextAlignment.Right));
                    rowContent.Cells.Add(NewCell("", "Normal"));
                }
                else if (n == 3)
                {
                    rowContent.Cells.Add(NewCell("", "Normal"));
                    rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", totalPendapatan), "Normal", TextAlignment.Right));
                }
                else if (n == 4)
                {
                    rowContent.Cells.Add(NewCell("", "Normal"));
                    rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", hpp), "Normal", TextAlignment.Right));
                }
                else { rowContent.Cells.Add(NewCell("", "Normal")); rowContent.Cells.Add(NewCell("", "Normal")); }

                if (n == headerName.Length - 1) foreach (var i in intArray) { rowContent.Cells[i].BorderBrush = Brushes.Black; rowContent.Cells[i].BorderThickness = new Thickness(0, 0, 0, size); }

                rowContent.Cells[0].Padding = new Thickness(7, 10, 7, 10);
                rowContent.Cells[1].Padding = new Thickness(7, 10, 7, 10);
                rowContent.Cells[2].Padding = new Thickness(7, 10, 7, 10);
            }

            #endregion

            #region Beban

            tabelBeban.RowGroups.Add(new TableRowGroup());
            tabelBeban.Columns.Add(new TableColumn() { Width = new GridLength(240) });
            tabelBeban.Columns.Add(new TableColumn() { Width = new GridLength(170) });
            tabelBeban.Columns.Add(new TableColumn() { Width = new GridLength(170) });

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

                if (n == 1) { rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", gaji), "Normal", TextAlignment.Right)); rowContent.Cells.Add(NewCell("", "Normal")); }
                else if (n == 2) { rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", perlengkapan), "Normal", TextAlignment.Right)); rowContent.Cells.Add(NewCell("", "Normal")); }
                else if (n == 3) { rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", listrik), "Normal", TextAlignment.Right)); rowContent.Cells.Add(NewCell("", "Normal")); }
                else if (n == 4) { rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", telepon), "Normal", TextAlignment.Right)); rowContent.Cells.Add(NewCell("", "Normal")); }
                else if (n == 5) { rowContent.Cells.Add(NewCell("", "Normal")); rowContent.Cells.Add(NewCell(string.Format(culture, "{0:N0}", totalBeban), "Normal", TextAlignment.Right)); }
                else if (n == 6) { rowContent.Cells.Add(NewCell("", "Normal")); rowContent.Cells.Add(NewCell(string.Format(culture, "{0:C0}", totalPendapatan - hpp - totalBeban), "Normal", TextAlignment.Right)); }
                else { rowContent.Cells.Add(NewCell("", "Normal")); rowContent.Cells.Add(NewCell("", "Normal")); }

                if (n == bebanName.Length - 2) foreach (var i in intArray) { rowContent.Cells[i].BorderBrush = Brushes.Black; rowContent.Cells[i].BorderThickness = new Thickness(0, 0, 0, size); }

                rowContent.Cells[0].Padding = new Thickness(7, 10, 7, 10);
                rowContent.Cells[1].Padding = new Thickness(7, 10, 7, 10);
                rowContent.Cells[2].Padding = new Thickness(7, 10, 7, 10);
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