using System;
using System.Collections.Generic;

namespace Karakoç.Models
{
    public partial class Mesai
    {
        public long MesaiId { get; set; }
        public int CalisanId { get; set; }
        public DateTime Tarih { get; set; }
        public bool IsWorked { get; set; }

        public virtual Calisan Calisan { get; set; } = null!;
    }
}
