using Karakoç.Models;
using Microsoft.EntityFrameworkCore;

namespace Karakoç.Bussiness.abstracts
{
    public interface IAdminService
    {
        List<Calisan> GetCalisans();
        bool KaydetOdeme(int CalisanId, string Aciklama, int tutar, DateTime Tarih);
        
     
        public List<Yevmiyeler> GetYevmiyelers();
        public bool KaydetYevmiye(DateTime Tarih, List<int> isWorked, List<int> isHalfWorked);

        

    }
}
