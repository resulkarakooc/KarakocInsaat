using ClosedXML.Excel;
using Karakoç.Bussiness.concrete;
using Karakoç.Models;
using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karakoç.Controllers
{
    public class AdminController : Controller
    {
        private AdminManager _adminManager;
        private ResulContext _context;

        public AdminController(AdminManager adminManager, ResulContext context)
        {
            _adminManager = adminManager;
            _context = context;
        }

        public IActionResult Index()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }
            // Tüm çalışanları al
            var calisanlar = _adminManager.GetCalisansVerify();
            var yevmiyeler = _adminManager.GetYevmiyelers();
            var mesailer = _context.Mesais.ToList();
            var giderler = _context.Giderlers.ToList();

            // Çalışan sayısını ViewBag ile gönder
            ViewBag.CalisanSayisi = calisanlar.Count;

            // Mevcut ayın yılı ve ayı
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            // Bu ayki true olan yevmiye sayısını hesapla
            var trueYevmiyeCount = yevmiyeler.Count(y => y.Tarih.HasValue && // Nullable kontrolü
                                                        y.Tarih.Value.Year == currentYear &&
                                                        y.Tarih.Value.Month == currentMonth &&
                                                        y.IsWorked == true);
            double trueHalfYevmiyeCount = yevmiyeler.Count(y => y.Tarih.HasValue && // Nullable kontrolü
                                                        y.Tarih.Value.Year == currentYear &&
                                                        y.Tarih.Value.Month == currentMonth &&
                                                        y.IsHalfWorked == true);

            var mesaiYevmiyeCount = mesailer.Count(y => y.Tarih.Year == currentYear && y.Tarih.Month == currentMonth && y.IsWorked == true);

            // Bu ayki giderlerin toplamını hesapla
            var totalGiderAmount = giderler
                .Where(g => g.Tarih.Year == currentYear && g.Tarih.Month == currentMonth)
                .Sum(g => g.Amount);

            // ViewBag'e gönderimler
            ViewBag.TrueYevmiyeSayisi = trueYevmiyeCount + (trueHalfYevmiyeCount / 2);
            ViewBag.MesaiYevmiyeleri = mesaiYevmiyeCount / 2;
            ViewBag.ToplamYevmiye = trueYevmiyeCount + (trueHalfYevmiyeCount / 2) + (mesaiYevmiyeCount / 2) ;
            ViewBag.TotalGiderAmount = totalGiderAmount;

            return View(); // Çalışanları da model olarak gönder
        }




        public async Task<IActionResult> GetYevmiye()
        {
            var yevmiyeler = await _adminManager.GetYevmiyeler();

            var veriler = yevmiyeler.Select(c => new
            {

                c.Tarih,
                c.IsWorked,
                c.IsHalfWorked,
                CalisanID = c.CalisanId,
                CalisanAd = c.Calisan.Name,
                CalisanSoyad = c.Calisan.Surname


            }).ToList();
            return Json(veriler);
        }

        public IActionResult YevmiyeGor()
        {

            return View();
        }


        public IActionResult MesaiGor()
        {

            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            return View();
        }
        public async Task<IActionResult> GetMesai()
        {
            var yevmiyeler = await _adminManager.GetMesai();

            var veriler = yevmiyeler.Select(c => new
            {

                c.Tarih,
                c.IsWorked,
                CalisanID = c.CalisanId,
                CalisanAd = c.Calisan.Name,
                CalisanSoyad = c.Calisan.Surname

            }).ToList();

            // Normal olarak view döndür
            return Json(veriler);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                // Veritabanında sorgu yapılıyor
                var bul = _context.Calisans
                                  .Where(x => x.Name.Contains(query)) // Tam eşleşme yerine parça eşleşme
                                  .ToList();

                return View(bul); // Sonuçları View'e gönder
            }

            // Arama boşsa sonuçsuz bir View dön
            return View(new List<Calisan>());
        }

        

        [Route("/Admin/Calisan/{id}")]
        public IActionResult Calisan(int id)
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var calisan = _adminManager.GetCalisanById(id);

            if (calisan == null)
            {
                return NotFound();
            }

            return View(calisan);
        }

        public IActionResult CalisanList()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            return View(_adminManager.GetCalisans());
        }

        [Route("/Admin/calisanDelete/{id}")]
        public IActionResult CalisanDelete(int id)
        {
            bool isDeleted = _adminManager.CalisanDelete(id);

            if (isDeleted)
            {
                // Başarılı silme işlemi sonrası ana sayfaya veya çalışan listesine yönlendirin
                return RedirectToAction("CalisanList", "Admin");
            }
            else
            {
                // Silme işlemi başarısız olduysa bir hata mesajı gösterin
                ModelState.AddModelError("", "Çalışan silinemedi. Çalışan bulunamadı veya ilişkili veriler silinemedi.");
                return View();
            }
        }

        [Route("/Admin/UpdateAuthority/{id}/{deger}")]
        public IActionResult UpdateAuthority(int id, byte deger)
        {

            _adminManager.UpdateAuthority(id, deger);
            return RedirectToAction("CalisanList", "Admin");
        }

        [Route("/Admin/UpdateVerify/{id}")]
        public IActionResult UpdateVerify(int id)
        {
            _adminManager.UpdateVerify(id);
            return RedirectToAction("CalisanList", "Admin");
        }

        public IActionResult YevmiyeGiris()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var calisanList = _adminManager.GetCalisansVerify();
            return View(calisanList);
        }

        public IActionResult MesaiGiris()
        {

            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var calisanList = _adminManager.GetCalisansVerify();
            return View(calisanList);
        }

        [HttpPost]
        public IActionResult MesaiKaydet(DateTime Tarih, List<int> isWorked)
        {
            // Öncelikle tüm çalışanları alın
            if (_adminManager.KaydetMesai(Tarih, isWorked))
            {
                ViewBag.Onay = "Mesailer Kaydedildi";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Onay = "bir hata oluştu";
                return RedirectToAction("MesaiGiris", "Admin");
            }
        }

        public IActionResult YevmiyeKaydet(DateTime Tarih, List<int> isWorked, List<int> isHalfWorked)
        {
            // Öncelikle tüm çalışanları alın
            if (_adminManager.KaydetYevmiye(Tarih, isWorked, isHalfWorked))
            {
                ViewBag.Onay = "Yevmiyeler Kaydedildi";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Onay = "bir hata oluştu";
                return RedirectToAction("YevmiyeGiris", "Admin");
            }
        }

        public IActionResult OdemeGiris()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            return View(_adminManager.GetCalisansVerify());
        }

        public class OdemeDto
        {
            public int OdemeId { get; set; }
            public int CalisanId { get; set; }
            public string Description { get; set; }
            public decimal Amount { get; set; }
            public string CalisanAd { get; set; } // Calisan ismi
            public string CalisanSoyad { get; set; } // Calisan ismi
            public DateTime Tarih { get; set; } // Tarih alanı
        }

        // Eğer ihtiyaç varsa
        public class CalisanDto
        {
            public int Id { get; set; }
            public string? Ad { get; set; }
            public string? Soyad { get; set; }
        }

        [HttpGet("/Admin/Odeme")]
        public async Task<IActionResult> GetOdeme()
        {
            var odemeler = await _adminManager.GetOdeme();

            var odemeDtos = odemeler.Select(o => new OdemeDto
            {
                OdemeId = o.OdemeId,
                CalisanId = o.CalisanId,
                Description = o.Description,
                Amount = o.Amount,
                CalisanAd = o.Calisan.Name,
                CalisanSoyad = o.Calisan.Surname,
                Tarih = o.Tarih // Tarih alanını ekleyin
            }).ToList();

            return Json(odemeDtos); // DTO listesini JSON olarak döndür
        }

        public IActionResult OdemeGor()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }


            return View(); // View'a model olarak geçin
        }

        [HttpPost]
        public IActionResult KaydetOdeme(int CalisanId, string Aciklama, int tutar, DateTime Tarih)
        {
            _adminManager.KaydetOdeme(CalisanId, Aciklama, tutar, Tarih);
            ViewBag.Bilgi = "Kayıt Edildi";
            return RedirectToAction("OdemeGiris", "Admin");
        }

        public IActionResult Alınan()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }
            return View(_adminManager.GetGelir());
        }

        [HttpPost]
        public IActionResult KaydetGelir(string aciklama, DateTime Tarih, decimal miktar)
        {
            _adminManager.KaydetGelir(aciklama, Tarih, miktar);
            ViewBag.Info = "İşlem Kaydedildi";
            return View("GelirGiris");
        }

        public IActionResult GelirGiris()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }
            return View();
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




        public IActionResult GiderGir()
        {
            var calisan = _context.Calisans.ToList();
           return View(calisan);   
        }

        [HttpPost]
        public IActionResult UpdateYevmiye(int yevmiyeId, int calisanId, DateTime tarih, string workedStatus)
        {
            var yevmiye = _context.Yevmiyelers.FirstOrDefault(y => y.YevmiyeId == yevmiyeId);
            if (workedStatus == "Çalışmadı")
            {
                if (yevmiye != null)
                {
                    _context.Yevmiyelers.Remove(yevmiye);
                    _context.SaveChanges();  // Değişiklikleri kaydet
                }
            }
            else
            {
                if (yevmiye == null)
                {
                    yevmiye = new Yevmiyeler
                    {
                        CalisanId = calisanId,
                        Tarih = tarih
                    };
                }

                if (workedStatus == "TamGün")
                {
                    yevmiye.IsWorked = true;
                    yevmiye.IsHalfWorked = false;
                }
                else if (workedStatus == "YarımGün")
                {
                    yevmiye.IsWorked = false;
                    yevmiye.IsHalfWorked = true;
                }
                else
                {
                    yevmiye.IsWorked = false;
                    yevmiye.IsHalfWorked = false;
                }

                if (yevmiyeId == 0)
                {
                    _context.Yevmiyelers.Add(yevmiye);
                }
                else
                {
                    _context.Yevmiyelers.Update(yevmiye);
                }

                _context.SaveChanges();  // Değişiklikleri kaydet
            }

            // İşlem tamamlandığında, yönlendirmek için çalışan sayfasına dön
            return RedirectToAction("Calisan", "Admin", new { id = calisanId });
        }

        [HttpPost]
        public IActionResult UpdateMesai(int mesaiId, int calisanId, DateTime tarih, string workedStatus)
        {
            var mesai = _context.Mesais.FirstOrDefault(m => m.MesaiId == mesaiId);
            if (workedStatus == "Çalışmadı")
            {
                if (mesai != null)
                {
                    // Mesai kaydını sil
                    _context.Mesais.Remove(mesai);
                    _context.SaveChanges();  
                }
            }
            else
            {
                if (mesai == null)
                {

                    mesai = new Mesai
                    {
                        CalisanId = calisanId,
                        Tarih = tarih,
                    };

                    if (workedStatus == "Yarım")
                    {
                        mesai.IsWorked = true;
                    }
                    else
                    {
                        mesai.IsWorked = false;
                    }
                    _context.Mesais.Add(mesai);
                }
                _context.SaveChanges();  
            }
            return RedirectToAction("Calisan", "Admin", new { id = calisanId });
        }

        [HttpGet]
        public IActionResult ExportHakedis(int ay, int yil)
        {
            var calisanlar = _context.Calisans.ToList();
            var yevmiyeler = _context.Yevmiyelers.Where(y => y.Tarih.Value.Year == yil && y.Tarih.Value.Month == ay).ToList();
            var mesailer = _context.Mesais.Where(m => m.Tarih.Year == yil && m.Tarih.Month == ay).ToList();

            using (var workbook = new XLWorkbook())
            {
                DateTime now = DateTime.Now;

                void ApplyCommonStyles(IXLWorksheet sheet)
                {
                    // Başlıklar
                    var titleCell = sheet.Range("A1:AJ1");
                    titleCell.Merge();
                    titleCell.Value = "TURYEM YAPI ALÇI-SIVA PUNTAJ LİSTESİ";
                    titleCell.Style.Font.Bold = true;
                    titleCell.Style.Font.FontSize = 18;
                    titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var periodCell = sheet.Range("A2:AJ2");
                    periodCell.Merge();
                    periodCell.Value = $"AİT OLDUĞU ÇALIŞMA DÖNEMİ: {ay}/{yil}";
                    periodCell.Style.Font.FontSize = 18;
                    periodCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Çalıştığı Günler Başlığı
                    var calistigiGunlerCell = sheet.Range("A3:AJ3");
                    calistigiGunlerCell.Merge();
                    calistigiGunlerCell.Value = "ÇALIŞTIĞI GÜNLER";
                    calistigiGunlerCell.Style.Font.Bold = true;
                    calistigiGunlerCell.Style.Font.FontSize = 18;
                    calistigiGunlerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Dosyanın oluşturulma tarihi
                    var dateCell = sheet.Range("AK2:AL2");
                    dateCell.Merge();
                    dateCell.Value = now.ToString("dd.MM.yyyy");
                    dateCell.Style.Font.Bold = true;
                    dateCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Günlerin olduğu satır
                    sheet.Cell(4, 1).Value = "PERSONELİN ADI SOYADI";
                    sheet.Cell(4, 1).Style.Font.Bold = true;
                    sheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    sheet.Cell(4, 1).Style.Alignment.WrapText = true;

                    sheet.Cell(4, 2).Value = "Görev";
                    sheet.Cell(4, 2).Style.Font.Bold = true;
                    sheet.Cell(4, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    for (int gun = 1; gun <= DateTime.DaysInMonth(yil, ay); gun++)
                    {
                        var cell = sheet.Cell(4, gun + 2);
                        cell.Value = gun;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontSize = 14;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.WrapText = true;
                    }

                    var toplamCell = sheet.Cell(4, DateTime.DaysInMonth(yil, ay) + 3);
                    toplamCell.Value = "ÇALIŞTIĞI GÜN TOPLAM";
                    toplamCell.Style.Font.Bold = true;
                    toplamCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    toplamCell.Style.Alignment.WrapText = true;

                    // kenarlıklar**
                    var lastCol = DateTime.DaysInMonth(yil, ay) + 3;
                    sheet.Range(1, 1, 4 + calisanlar.Count, lastCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Range(1, 1, 4 + calisanlar.Count, lastCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                void FillTableMesai(IXLWorksheet sheet, List<Mesai> data, bool isMesai = false)
                {
                    int startRow = 5;

                    foreach (var calisan in calisanlar)
                    {
                        if (calisan.Authority == 4) { continue; }
                        sheet.Cell(startRow, 1).Value = $"{calisan.Name} {calisan.Surname}";
                        sheet.Cell(startRow, 1).Style.Font.FontSize = 14;
                        sheet.Cell(startRow, 2).Value = "ALÇI-SIVA";

                        double amberCellsCount = 0;
                        for (int gun = 1; gun <= DateTime.DaysInMonth(yil, ay); gun++)
                        {
                            var record = data.FirstOrDefault(y => y.CalisanId == calisan.CalısanId && y.Tarih.Day == gun);
                            if (record != null)
                            {
                                var cell = sheet.Cell(startRow, gun + 2);
                                if (record.IsWorked == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Amber;
                                    amberCellsCount = amberCellsCount + (0.5);
                                }
                            }
                        }

                        // Çalıştığı Gün Toplam sütunu
                        var totalCell = sheet.Cell(startRow, DateTime.DaysInMonth(yil, ay) + 3);
                        totalCell.Value = isMesai ? amberCellsCount : "Hata";
                        totalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        startRow++;
                    }

                    // Sütun genişliklerini ayarla
                    sheet.Column(1).AdjustToContents();
                    sheet.Column(2).AdjustToContents();
                    for (int gun = 1; gun <= DateTime.DaysInMonth(yil, ay) + 1; gun++)
                    {
                        sheet.Column(gun + 2).Width = 8.11; // Günlerin sütun genişliği
                    }
                    sheet.Row(4).Height = 40.5; // Günlerin satır yüksekliği (54 piksel)
                }

                void FillTableYevmiye(IXLWorksheet sheet, List<Yevmiyeler> data, bool isMesai = false)
                {
                    int startRow = 5;

                    foreach (var calisan in calisanlar)
                    {
                        if (calisan.Authority == 4) { continue; }
                        sheet.Cell(startRow, 1).Value = $"{calisan.Name} {calisan.Surname.ToUpper()}";
                        sheet.Cell(startRow, 2).Value = "ALÇI-SIVA";

                        double greyCellsCount = 0;
                        double amberCellCount = 0;
                        for (int gun = 1; gun <= DateTime.DaysInMonth(yil, ay); gun++)
                        {
                            var record = data.FirstOrDefault(y => y.CalisanId == calisan.CalısanId && y.Tarih.Value.Day == gun);
                            if (record != null)
                            {
                                var cell = sheet.Cell(startRow, gun + 2);
                                if (record.IsWorked == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Gray;
                                    greyCellsCount++;
                                }
                                else if (record.IsHalfWorked == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Amber;
                                    amberCellCount = amberCellCount + (0.5);
                                }
                            }
                        }

                        // Çalıştığı Gün Toplam sütunu
                        var totalCell = sheet.Cell(startRow, DateTime.DaysInMonth(yil, ay) + 3);
                        totalCell.Value = greyCellsCount + amberCellCount;
                        totalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        startRow++;
                    }


                    sheet.Column(1).AdjustToContents();
                    sheet.Column(2).AdjustToContents();
                    for (int gun = 1; gun <= DateTime.DaysInMonth(yil, ay) + 1; gun++)
                    {
                        sheet.Column(gun + 2).Width = 8.11;
                    }
                    sheet.Row(4).Height = 40.5; // Günlerin satır yüksekliği (54 piksel)
                }

                // Yevmiyeler Sayfası
                var yevmiyeSheet = workbook.Worksheets.Add("Yevmiyeler");
                ApplyCommonStyles(yevmiyeSheet);
                FillTableYevmiye(yevmiyeSheet, yevmiyeler);

                // Mesailer Sayfası ekle
                var mesaiSheet = workbook.Worksheets.Add("Mesailer");
                ApplyCommonStyles(mesaiSheet);
                FillTableMesai(mesaiSheet, mesailer, isMesai: true);

                // Excel dosyasını bellek üzerine kaydet
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Hakedis_{ay}_{yil}.xlsx");
                }
            }
        }

        public bool Control()
        {
            if (HttpContext.Request.Cookies["Authority"] == "3") //admin ise al
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