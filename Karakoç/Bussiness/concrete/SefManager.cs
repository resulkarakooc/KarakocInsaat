using Karakoç.Models;
using Microsoft.EntityFrameworkCore;

namespace Karakoç.Bussiness.concrete
{
    public class SefManager
    {
        private readonly ResulContext _resulContext;

        public SefManager(ResulContext resulContext)
        {
            _resulContext = resulContext;
        }

        public List<Calisan> GetCalisansVerify()
        {
            return _resulContext.Calisans.ToList();
        }

        public List<Calisan> GetCalisansVerifyTrue()
        {
            return _resulContext.Calisans.Where(x => x.Verify == true).ToList();
        }

        public async Task<List<Mesai>> GetMesai()
        {
            var yevmiyeList = await _resulContext.Mesais
                           .Include(o => o.Calisan)
                           .ToListAsync();
            return yevmiyeList;
        }
        public async Task<List<Odemeler>> GetOdeme()
        {
            // Veritabanından verileri al
            var odemeListesi = await _resulContext.Odemelers.Include(o => o.Calisan).ToListAsync();



            return odemeListesi;
        }

        public async Task<List<Yevmiyeler>> GetYevmiyeler()
        {
            var yevmiyeList = await _resulContext.Yevmiyelers
                            .Include(o => o.Calisan)
                            .ToListAsync();

            return yevmiyeList;
        }

        public List<Yevmiyeler> GetYevmiyelers()
        {
            var yevmiyeList = _resulContext.Yevmiyelers
                            .Include(o => o.Calisan)
                            .ToList();

            return yevmiyeList;
        }


        public List<GelirTablosu> GetGelir()
        {
            return _resulContext.GelirTablosus.ToList();
        }

        public Calisan? GetCalisanById(int id)
        {
            return _resulContext.Calisans
                      .Include(c => c.Yevmiyelers)
                      .Include(c => c.Mesais)
                      .FirstOrDefault(c => c.CalısanId == id);
        }

    }
}
