using Karakoç.Bussiness.concrete;
using Karakoç.Models; 
using Microsoft.AspNetCore.Mvc;

namespace Karakoç.Controllers
{
    public class CalisanController : Controller
    {
        private readonly ResulContext _resulContext;
        private readonly CalisanManager manager;
        public CalisanController(CalisanManager calisanManager, ResulContext resulContext)
        {
            manager = calisanManager;
            _resulContext = resulContext;
        }

        [HttpGet]
        public IActionResult AnaSayfa()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            return View(manager.GetYevmiye(HttpContext)); // burası bir liste döndürücek geriye
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

            var yevmiyeListesi = _resulContext.Yevmiyelers.Where(y => y.CalisanId == CalisanId).ToList();
            var mesaiListesi = _resulContext.Mesais.Where(y => y.CalisanId == CalisanId).ToList();
            var avansListesi = _resulContext.Odemelers.Where(y => y.CalisanId == CalisanId && y.Tarih.Month == DateTime.Now.Month).ToList();
            // Çalışılan günleri sayma
            double calismaGunSayisi = yevmiyeListesi.Count(y => y.IsWorked == true && y.Tarih.Value.Month == DateTime.Now.Month);
            double calismaYarimSayisi = yevmiyeListesi.Count(y => y.IsHalfWorked == true && y.Tarih.Value.Month == DateTime.Now.Month);
            double mesaiGunSayisi = mesaiListesi.Count(y => y.IsWorked == true && y.Tarih.Month == DateTime.Now.Month);
            double miktar = avansListesi.Sum(y => y.Amount);
            ViewBag.WorkedDays = calismaGunSayisi + (calismaYarimSayisi/2);
            mesaiGunSayisi = mesaiGunSayisi / 2;
            ViewBag.WorkedDaysMesai = mesaiGunSayisi;
            ViewBag.Total = calismaGunSayisi+ mesaiGunSayisi + (calismaYarimSayisi / 2);
            ViewBag.Miktar = miktar;
            ViewBag.UserName = userName+" "+userSurName;

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
            var yevmiyeKayitlari = _resulContext.Yevmiyelers
                .Where(y => y.CalisanId == kullaniciId)
                .ToList();

            return View(yevmiyeKayitlari);
        }

        [HttpGet("/Calisan/GetMyPuantaj")]
        public IActionResult GetMyPuantaj()
        {
            var CalisanId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);
            
            var list = _resulContext.Yevmiyelers.Where(x => x.CalisanId == CalisanId).ToList();

            return  Json(list);
        }


        [HttpGet]
        public IActionResult Mesailerim()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var kullaniciId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);


            // Veritabanından bu kullanıcıya ait yevmiye kayıtlarını çek
            var yevmiyeKayitlari = _resulContext.Mesais
                .Where(y => y.CalisanId == kullaniciId)
                .ToList();

            return View(yevmiyeKayitlari);
            
        }

        [HttpGet("/Calisan/GetMyMesai")]
        public IActionResult GetMyMesai()
        {
            var CalisanId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);

            var list = _resulContext.Mesais.Where(x => x.CalisanId == CalisanId).ToList();

            return Json(list);
        }


        public IActionResult Odemeler() 
        {
            if (!Control())
            {
                 return RedirectToAction("Giris", "Login");
            }

            var kullaniciId = Convert.ToInt32(HttpContext.Request.Cookies["CalisanId"]);
            var liste = _resulContext.Odemelers.Where(y => y.CalisanId == kullaniciId).ToList(); 
            

            return View(liste); 
        }

        public bool Control()
        {
            if (HttpContext.Request.Cookies["Authority"] == "1"  && HttpContext.Request.Cookies["isLogged"] == "true") //calisan ise
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
