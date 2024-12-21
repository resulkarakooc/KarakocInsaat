using System;
using System.Collections.Generic;

namespace Karakoç.Models
{
    public partial class Mesai
    {
        public long MesaiId { get; set; }
        public int CalisanId { get; set; }
        public DateTime Tarih { get; set; }
        public bool IsWorked { get; set; } // yarım
        public bool IsFullWorkedCalisan { get; set; } // Tam
        
        public bool? IsWorkedCompany { get; set; } // şirket yarım
        public bool? IsFullWorked { get; set; } //tam şirket

        public virtual Calisan Calisan { get; set; } = null!;
    }
}
