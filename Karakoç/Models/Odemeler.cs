using System;
using System.Collections.Generic;

namespace Karakoç.Models
{
    public partial class Odemeler
    {
        public int OdemeId { get; set; }
        public int CalisanId { get; set; }
        public string? Description { get; set; }
        public int Amount { get; set; }
        public DateTime Tarih { get; set; }

        public virtual Calisan Calisan { get; set; } = null!;
    }
}
