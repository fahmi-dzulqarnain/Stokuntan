using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stokuntan.Classes
{
    class ManajemenUtang
    {
        public string REF_NOTA { get; set; }
        public string BULAN { get; set; }
        public string TGL_BELI { get; set; }
        public string SUPPLIER { get; set; }
        public string TOTAL_TAGIHAN { get; set; }
        public string TOTAL_BAYAR { get; set; }
        public string SISA_BAYAR { get; set; }
    }
}
