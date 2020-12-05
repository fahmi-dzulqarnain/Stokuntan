using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stokuntan.Classes
{
    class ManajemenPiutang
    {
        public int ID { get; set; }
        public string BULAN { get; set; }
        public string TANGGAL_JUAL { get; set; }
        public string CUSTOMER { get; set; }
        public string TOTAL_PENJUALAN { get; set; }
        public string TOTAL_BAYAR { get; set; }
        public string SISA_BAYAR { get; set; }
    }
}
