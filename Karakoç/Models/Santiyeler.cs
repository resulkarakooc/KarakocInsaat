using System;
using System.Collections.Generic;

namespace Karakoç.Models
{
    public partial class Santiyeler
    {
        public Santiyeler()
        {
            Yevmiyelers = new HashSet<Yevmiyeler>();
        }

        public int SantiyeId { get; set; }
        public string SantiyeAdi { get; set; } = null!;
        public string SantiyeAdres { get; set; } = null!;
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }

        public virtual ICollection<Yevmiyeler> Yevmiyelers { get; set; }
    }
}
