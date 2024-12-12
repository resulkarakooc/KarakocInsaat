using System;
using System.Collections.Generic;

namespace Karakoç.Models
{
    public partial class GelirTablosu
    {
        public long AlınanId { get; set; }
        public string? Aciklama { get; set; }
        public decimal AlınanMiktar { get; set; }
        public DateTime? AlınanTarih { get; set; }
    }
}
