using Humanizer;
using Karakoç.Bussiness.abstracts;
using Karakoç.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security;

namespace Karakoç.Bussiness.concrete
{
    public class AdminManager : IAdminService
    {
        private readonly ResulContext _resulContext;

        public AdminManager(ResulContext resulContext)
        {
            _resulContext = resulContext;
        }

        public List<GelirTablosu> GetGelir()
        {
            return _resulContext.GelirTablosus.ToList();
        }

        public bool KaydetGelir(string aciklama, DateTime Tarih, decimal miktar)
        {
            try
            {
                GelirTablosu gelir = new GelirTablosu
                {
                    Aciklama = aciklama,
                    AlınanMiktar = miktar,
                    AlınanTarih = Tarih
                };

                _resulContext.GelirTablosus.Add(gelir);
                _resulContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }


        }
        public Calisan? GetCalisanById(int id)
        {
            return _resulContext.Calisans
                      .Include(c => c.Yevmiyelers)
                      .Include(c => c.Odemelers)
                      .Include(c => c.Mesais)
                      .FirstOrDefault(c => c.CalısanId == id);
        }




        public List<Calisan> GetCalisans()
        {
            return _resulContext.Calisans.ToList();
        }

        public List<Calisan> GetCalisansVerify()
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


        public bool KaydetOdeme(int CalisanId, string Aciklama, int tutar, DateTime tarih)
        {
            var odeme = new Odemeler
            {
                CalisanId = CalisanId,
                Amount = tutar,
                Description = Aciklama,
                Tarih = tarih
            };
            _resulContext.Odemelers.Add(odeme);
            _resulContext.SaveChanges();
            return true;
        }

        public bool KaydetMesai(DateTime Tarih, List<int> isWorked)
        {
            var calisanlar = _resulContext.Calisans.ToList();

            foreach (var calisan in calisanlar)
            {
                
                var mesai = _resulContext.Mesais.FirstOrDefault(y => y.CalisanId == calisan.CalısanId && y.Tarih == Tarih);

                // Eğer kayıt varsa güncelle
                if (mesai != null)
                {
                    
                    mesai.IsWorked = isWorked.Contains(calisan.CalısanId);
                }
               
                else
                {
                    
                    if (isWorked.Contains(calisan.CalısanId))
                    {
                        var yeniMesai = new Mesai
                        {
                            CalisanId = calisan.CalısanId,
                            Tarih = Tarih,
                            IsWorked = true
                        };
                        _resulContext.Mesais.Add(yeniMesai);
                    }
                }
            }

           
            _resulContext.SaveChanges();

            return true;
        }


        public bool KaydetYevmiye(DateTime Tarih, List<int> isWorked, List<int> isHalfWorked)
        {
            var calisanlar = _resulContext.Calisans.ToList();
            var workedSet = new HashSet<int>(isWorked);  // Daha hızlı erişim için HashSet
            var halfWorkedSet = new HashSet<int>(isHalfWorked);

            foreach (var calisan in calisanlar)
            {
                // Seçilen tarihte bu çalışanın bir kaydı olup olmadığını kontrol et
                var yevmiye = _resulContext.Yevmiyelers.FirstOrDefault(y => y.CalisanId == calisan.CalısanId && y.Tarih == Tarih);

                bool calisanIsWorked = workedSet.Contains(calisan.CalısanId);
                bool calisanIsHalfWorked = halfWorkedSet.Contains(calisan.CalısanId);

                // Kayıt var ise güncelle
                if (yevmiye != null)
                {
                    yevmiye.IsWorked = calisanIsWorked;
                    yevmiye.IsHalfWorked = calisanIsHalfWorked;
                }
                // Kayıt yoksa ve gelen veri `isWorked` veya `isHalfWorked` true ise yeni kayıt oluştur
                else if (calisanIsWorked || calisanIsHalfWorked)
                {
                    var yeniYevmiye = new Yevmiyeler
                    {
                        CalisanId = calisan.CalısanId,
                        Tarih = Tarih,
                        IsWorked = calisanIsWorked,
                        IsHalfWorked = calisanIsHalfWorked
                    };
                    _resulContext.Yevmiyelers.Add(yeniYevmiye);
                }
            }

            // Değişiklikleri tek seferde kaydet
            _resulContext.SaveChanges();
            return true;
        }



        public bool CalisanDelete(int id)
        {
            // Çalışanı veritabanından al
            var target = _resulContext.Calisans
                            .Include(c => c.Odemelers) // Çalışanın ödemelerini dahil et
                            .Include(c => c.Yevmiyelers)
                            .Include(c => c.Mesais)// Örneğin kayıtlar tablosuyla ilişkiliyse
                            .Include(c => c.Giderlers)
                            // Örneğin kayıtlar tablosuyla ilişkiliyse
                            .FirstOrDefault(x => x.CalısanId == id);

            if (target == null) return false;

            // İlişkili ödemeleri sil
            if (target.Odemelers != null)
            {
                _resulContext.Odemelers.RemoveRange(target.Odemelers);
            }

            // İlişkili kayıtları sil
            if (target.Giderlers != null)
            {
                _resulContext.Giderlers.RemoveRange(target.Giderlers);
            }
            
            if (target.Mesais != null)
            {
                _resulContext.Mesais.RemoveRange(target.Mesais);
            }
            
            if (target.Yevmiyelers != null)
            {
                _resulContext.Yevmiyelers.RemoveRange(target.Yevmiyelers);
            }

            // Çalışanı sil
            _resulContext.Calisans.Remove(target);

            // Değişiklikleri kaydet
            _resulContext.SaveChanges();
            return true;
        }


        public bool UpdateAuthority(int id, byte deger)
        {
            var personel = _resulContext.Calisans.FirstOrDefault(x => x.CalısanId == id);

            if (personel == null) return false;

            personel.Authority = deger;

            _resulContext.SaveChanges();

            return true;
        }

        public bool UpdateVerify(int id)
        {
           var personel =  _resulContext.Calisans.FirstOrDefault(x => x.CalısanId == id);

            if (personel == null) return false;

            if(personel.Verify != null)
            {
                if ((bool)personel.Verify)
                {
                    personel.Verify = false;
                }
                else
                {
                    personel.Verify = true;
                }
                _resulContext.SaveChanges();
            }

            return true;
        }


    }
}
