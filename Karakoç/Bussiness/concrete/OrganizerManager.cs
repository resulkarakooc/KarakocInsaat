using Karakoç.Bussiness.abstracts;
using Karakoç.Models;
using Microsoft.AspNetCore.Components.Web;

namespace Karakoç.Bussiness.concrete
{
    public class OrganizerManager : IOrganizerService
    {
        private readonly ResulContext _context;

        public OrganizerManager(ResulContext context)
        {
            _context = context;
        }
        public List<Calisan> GetCalisans()
        {
            var calisanlar = _context.Calisans.Where(x => x.Verify == true).ToList();
            return calisanlar;
        }

        public bool KaydetYevmiye(DateTime Tarih, List<int> isWorked)
        {
            var calisanlar = _context.Calisans.ToList();

            foreach (var calisan in calisanlar)
            {
                // Bu çalışanın seçilen tarihte bir kaydı olup olmadığını kontrol et
                var yevmiye = _context.Yevmiyelers.FirstOrDefault(y => y.CalisanId == calisan.CalısanId && y.Tarih == Tarih);

                // Eğer kayıt varsa güncelle
                if (yevmiye != null)
                {
                    yevmiye.IsWorked = isWorked.Contains(calisan.CalısanId);
                }
                // Eğer kayıt yoksa yeni bir yevmiye kaydı oluştur
                else
                {
                    var yeniYevmiye = new Yevmiyeler
                    {
                        CalisanId = calisan.CalısanId,
                        Tarih = Tarih,
                        IsWorked = isWorked.Contains(calisan.CalısanId)
                    };
                    _context.Yevmiyelers.Add(yeniYevmiye);
                }
                _context.SaveChanges();
            }
            return true;

        }

        public bool KaydetGider(int CalisanId, string Aciklama, int tutar)
        {
            var newGider = new Giderler
            {
                CalisanId = CalisanId,
                Description = Aciklama,
                Amount = tutar,
                Tarih = DateTime.Now
            };

            _context.Giderlers.Add(newGider);
            _context.SaveChanges();

            return true;
        }

        public List<Giderler> GetGiderlers()
        {
            throw new NotImplementedException();
        }

        public bool GiderDelete(int id)
        {
            var target = _context.Giderlers.FirstOrDefault(x => x.GiderId == id);
            if (target != null)
            {
                _context.Giderlers.Remove(target);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

            
        }


    }
}
