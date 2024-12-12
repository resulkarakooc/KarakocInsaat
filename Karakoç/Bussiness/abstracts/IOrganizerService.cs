using Karakoç.Models;

namespace Karakoç.Bussiness.abstracts
{
    public interface IOrganizerService
    {
        List<Calisan> GetCalisans();
        bool KaydetYevmiye(DateTime Tarih, List<int> isWorked);
        List<Giderler> GetGiderlers();
        bool KaydetGider(int CalisanId, string Aciklama, int tutar);

    }
}
