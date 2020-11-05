using Stokuntan.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class Authentication_Windows : Window
    {
        public Authentication_Windows()
        {
            InitializeComponent();
        }

        private void BtnOke_Click(object sender, RoutedEventArgs e)
        {
            var db = new IDatabase().Conn();
            var password = txtBoxKataSandi.Password;

            new Thread(() =>
            {
                if (password != "")
                {
                    var akun = db.Query<TabelAkun>("SELECT * FROM TabelAkun").Where(x => x.JENIS_AKUN == "ADMIN" && x.KATA_SANDI == password);

                    if (akun.Count() != 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DialogResult = true;
                            Close();
                        });
                    }
                    else Dispatcher.Invoke(() => { MessageBox.Show("Kata Sandi Admin yang Anda Masukkan Salah", "Terjadi Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error); });
                }
                else Dispatcher.Invoke(() => { MessageBox.Show("Isi Kolom Kata Sandi", "Lengkapi Kolom", MessageBoxButton.OK, MessageBoxImage.Warning); });
            }).Start();
        }

        private void BtnBatal_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
