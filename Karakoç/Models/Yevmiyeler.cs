﻿using System;
using System.Collections.Generic;

namespace Karakoç.Models
{
    public partial class Yevmiyeler
    {
        public int YevmiyeId { get; set; }
        public int CalisanId { get; set; }
        public DateTime? Tarih { get; set; }
        public bool? IsWorked { get; set; }
        public bool? IsHalfWorked { get; set; }
        public bool? IsWorkedCompany { get; set; }
        public int? SantiyeId { get; set; }

        public virtual Calisan Calisan { get; set; } = null!;
        public virtual Santiyeler? Santiye { get; set; }
    }
}
