using Karakoç.Bussiness.concrete;
using Karakoç.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karakoç.Controllers
{
	public class OrganizerController : Controller
	{
		private readonly ResulContext _context;
		private readonly OrganizerManager _organizerManager;

		public OrganizerController(ResulContext context, OrganizerManager manager)
		{
			_context = context;
			_organizerManager = manager;
		}


        public IActionResult Home()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var CalisanId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);
            var userName = HttpContext.Request.Cookies["UserName"];
            var userSurName = HttpContext.Request.Cookies["UserSurName"];
            var userEmail = HttpContext.Request.Cookies["UserEmail"];
            var KayitTarihi = HttpContext.Request.Cookies["KayitTarihi"];



            ViewBag.UserName = userName;
            ViewBag.UserSurName = userSurName;
            ViewBag.UserEmail = userEmail;
            ViewBag.KayitTarihi = KayitTarihi;

            var yevmiyeListesi = _context.Yevmiyelers.Where(y => y.CalisanId == CalisanId).ToList();
            var mesaiListesi = _context.Mesais.Where(y => y.CalisanId == CalisanId).ToList();
            var avansListesi = _context.Odemelers.Where(y => y.CalisanId == CalisanId && y.Tarih.Month == DateTime.Now.Month).ToList();
            // Çalışılan günleri sayma
            double calismaGunSayisi = yevmiyeListesi.Count(y => y.IsWorked == true && y.Tarih.Value.Month == DateTime.Now.Month);
            double calismaYarimSayisi = yevmiyeListesi.Count(y => y.IsHalfWorked == true && y.Tarih.Value.Month == DateTime.Now.Month);
            double mesaiGunSayisi = mesaiListesi.Count(y => y.IsWorked == true && y.Tarih.Month == DateTime.Now.Month);
            double miktar = avansListesi.Sum(y => y.Amount);
            ViewBag.WorkedDays = calismaGunSayisi + (calismaYarimSayisi / 2);
            mesaiGunSayisi = mesaiGunSayisi / 2;
            ViewBag.WorkedDaysMesai = mesaiGunSayisi;
            ViewBag.Total = calismaGunSayisi + mesaiGunSayisi + (calismaYarimSayisi / 2);
            ViewBag.Miktar = miktar;
            ViewBag.UserName = userName + " " + userSurName;

            return View();
        }


        public IActionResult Puantajım()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var kullaniciId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);


            // Veritabanından bu kullanıcıya ait yevmiye kayıtlarını çek
            var yevmiyeKayitlari = _context.Yevmiyelers
                .Where(y => y.CalisanId == kullaniciId)
                .ToList();

            return View(yevmiyeKayitlari);
        }

        [HttpGet("/Organizer/GetMyPuantaj")]
        public IActionResult GetMyPuantaj()
        {
            var CalisanId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);

            var list = _context.Yevmiyelers.Where(x => x.CalisanId == CalisanId).ToList();

            return Json(list);
        }


        [HttpGet]
        public IActionResult Mesailerim()
        {
            //if (!Control())
            //{
            //    return RedirectToAction("Giris", "Login");
            //}

            var kullaniciId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);


            // Veritabanından bu kullanıcıya ait yevmiye kayıtlarını çek
            var yevmiyeKayitlari = _context.Mesais
                .Where(y => y.CalisanId == kullaniciId)
                .ToList();

            return View(yevmiyeKayitlari);

        }

        [HttpGet("/Organizer/GetMyMesai")]
        public IActionResult GetMyMesai()
        {
            var CalisanId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);

            var list = _context.Mesais.Where(x => x.CalisanId == CalisanId).ToList();

            return Json(list);
        }


        [HttpGet]
        public IActionResult Giderler(int? year, int? month)
        {
            // Eğer year veya month null ise mevcut yıl ve ayı kullan
            year ??= DateTime.Now.Year;
            month ??= DateTime.Now.Month;

            // Giderleri filtrele
            var giderler = _context.Giderlers
                .Where(g => g.Tarih.Year == year && g.Tarih.Month == month)
                .Include(g => g.Calisan)  // İlişkili Calisan (Çalışan) verilerini dahil et
                .ToList();

            // Seçili yıl ve ayı View'e gönder
            ViewBag.SelectedYear = year;
            ViewBag.SelectedMonth = month;

            return View(giderler);
        }



        [HttpGet]
		public IActionResult Yevmiyeler()  //yevmiye girme ekranı
		{
			return View(_organizerManager.GetCalisans());
		}

        public class CalisanViewModel 
        {
            public List<Calisan> Calisanlar { get; set; }
        }

        [HttpPost]
		public IActionResult Kaydet(DateTime Tarih, List<int> isWorked) //yevmiyeleri kaydet
		{
			// Öncelikle tüm çalışanları alın
			if (_organizerManager.KaydetYevmiye(Tarih, isWorked)){
                ViewBag.Onay = "Yevmiyeler Kaydedildi";
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
		}

        [Route("/Organizer/GiderDelete/{id}")]
        public IActionResult GiderDelete(int id)
        {
            bool isDeleted = _organizerManager.GiderDelete(id);

            if (isDeleted)
            {
                // Başarılı silme işlemi sonrası ana sayfaya veya çalışan listesine yönlendirin
                return RedirectToAction("Giderler", "Organizer");
            }
            else
            {
                // Silme işlemi başarısız olduysa bir hata mesajı gösterin
                ModelState.AddModelError("", "Çalışan silinemedi. Çalışan bulunamadı veya ilişkili veriler silinemedi.");
                return View();
            }
        }

        public IActionResult GiderGir()
        {
            var calisan = _context.Calisans.ToList();
            return View(calisan);
        }

        [HttpPost]
		public IActionResult KaydetGider(int CalisanId, string Aciklama,int tutar)
		{
			if(_organizerManager.KaydetGider(CalisanId, Aciklama, tutar))
            {
                return RedirectToAction("Giris", "Login");
            }
            return RedirectToAction("Giris", "Login");
        }

        public class GiderViewModel
        {
            public List<Giderler> Giderler { get; set; }
            public List<Calisan> Calisanlar { get; set; } // Çalışanlar listesini ekliyoruz
            public decimal ToplamTutar { get; set; } // Toplam tutar
        }


        public IActionResult Odemeler()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var kullaniciId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);
            var liste = _context.Odemelers.Where(y => y.CalisanId == kullaniciId).ToList();


            return View(liste);
        }

        public bool Control()
        {
            if (HttpContext.Request.Cookies["Authority"] == "2"  && HttpContext.Request.Cookies["isLogged"] == "true") //calisan ise
            {
                return true;
            }
            else
            {
                return false;
            }


        }


    }
}