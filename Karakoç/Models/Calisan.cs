using System;
using System.Collections.Generic;

namespace Karakoç.Models
{
    public partial class Calisan
    {
        public Calisan()
        {
            Giderlers = new HashSet<Giderler>();
            Mesais = new HashSet<Mesai>();
            Odemelers = new HashSet<Odemeler>();
            Yevmiyelers = new HashSet<Yevmiyeler>();
        }

        public int CalısanId { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public DateTime? KayıtTarihi { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public long? TcKimlik { get; set; }
        public DateTime? BirthDate { get; set; }
        public byte? Authority { get; set; }
        public bool? Verify { get; set; }

        public virtual ICollection<Giderler> Giderlers { get; set; }
        public virtual ICollection<Mesai> Mesais { get; set; }
        public virtual ICollection<Odemeler> Odemelers { get; set; }
        public virtual ICollection<Yevmiyeler> Yevmiyelers { get; set; }
    }
}
