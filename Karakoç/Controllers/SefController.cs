using Karakoç.Bussiness.concrete;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Karakoç.Models;

namespace Karakoç.Controllers
{
    public class SefController : Controller
    {
        private SefManager _sefManager;
        private ResulContext _resulContext;

        public SefController(SefManager sefManager, ResulContext resulContext)
        {
            _sefManager = sefManager;
            _resulContext = resulContext;
        }


        public IActionResult Index() // çalışan sayısı  //Hosgeldiniz
        {

            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            // Tüm çalışanları al
            var calisanlar = _sefManager.GetCalisansVerify();
            var Aktifcalisanlar = _sefManager.GetCalisansVerifyTrue();


            ViewBag.UserName = HttpContext.Request.Cookies["UserName"];
            ViewBag.UserSurName = HttpContext.Request.Cookies["UserSurName"];

            // Çalışan sayısını ViewBag ile gönder
            ViewBag.CalisanSayisi = calisanlar.Count;
            ViewBag.AktifCalisanSayisi = Aktifcalisanlar.Count;


            return View(); // Çalışanları da model olarak gönder
        }

        public IActionResult ExportCalisan()
        {
            var employees = _resulContext.Calisans.ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Çalışanlar");

                // Başlık "Karakoç İnşaat"
                worksheet.Range("C1:G1").Merge().Value = "Karakoç İnşaat";
                worksheet.Range("C1:G1").Style.Font.Bold = true;
                worksheet.Range("C1:G1").Style.Font.FontSize = 16;
                worksheet.Range("C1:G1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Sağ üst köşeye tarih
                worksheet.Cell(2, 7).Value = DateTime.Now.ToString("dd.MM.yyyy");

                // Üçüncü satır boş bırak ve başlıklar
                worksheet.Cell(4, 3).Value = "No";
                worksheet.Cell(4, 4).Value = "TC Kimlik";
                worksheet.Cell(4, 5).Value = "Ad";
                worksheet.Cell(4, 6).Value = "Soyad";
                worksheet.Cell(4, 7).Value = "Durumu";

                // No sütunu genişliği
                worksheet.Column(3).Width = 5;
                worksheet.Column(4).Width = 15;

                // Verileri doldur ve numaralandır
                for (int i = 0, row = 5; i < employees.Count; i++)
                {
                    if (employees[i].Authority == 4) continue;

                    worksheet.Cell(row, 3).Value = row - 4; // No sütunu
                    worksheet.Cell(row, 4).Value = employees[i].TcKimlik;
                    worksheet.Cell(row, 5).Value = employees[i].Name;
                    worksheet.Cell(row, 6).Value = employees[i].Surname;
                    worksheet.Cell(row, 7).Value = employees[i].Verify.HasValue
                                                    ? (employees[i].Verify.Value ? "Çalışıyor" : "Çalışmıyor")
                                                    : "Belirsiz";

                    row++;
                }

                // Tablo oluşturma ve stil
                var tableRange = worksheet.Range(4, 3, employees.Count + 4, 7);
                var table = tableRange.CreateTable();
                table.Theme = XLTableTheme.TableStyleMedium9;

                // Başlıkları ortala ve kalın yap
                var headerRow = worksheet.Range(4, 3, 4, 7);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Calisanlar.xlsx");
                }
            }
        }
        public async Task<IActionResult> GetYevmiye()
        {
            var yevmiyeler = await _sefManager.GetYevmiyeler();

            var veriler = yevmiyeler.Select(c => new
            {

                c.Tarih,
                c.IsWorked,
                c.IsHalfWorked,
                c.IsWorkedCompany,
                CalisanID = c.CalisanId,
                CalisanAd = c.Calisan.Name,
                CalisanSoyad = c.Calisan.Surname

            }).ToList();

            // Normal olarak view döndür
            return Json(veriler);
        }

        public IActionResult YevmiyeGor()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }


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
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }


            var yevmiyeler = await _sefManager.GetMesai();

            var veriler = yevmiyeler.Select(c => new
            {

                c.Tarih,
                c.IsWorked,
                c.IsFullWorked,
                c.IsWorkedCompany,
                CalisanID = c.CalisanId,
                CalisanAd = c.Calisan.Name,
                CalisanSoyad = c.Calisan.Surname

            }).ToList();

            // Normal olarak view döndür
            return Json(veriler);
        }



        public IActionResult Ödemeler()
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }


            return View(_sefManager.GetGelir());
        }

        [Route("/Sef/Calisan/{id}")]
        public IActionResult Calisan(int id)
        {
            if (!Control())
            {
                return RedirectToAction("Giris", "Login");
            }

            var calisan = _sefManager.GetCalisanById(id);

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

            return View(_sefManager.GetCalisansVerify());
        }

        public bool Control()
        {
            if (HttpContext.Request.Cookies["Authority"] == "4") //admin veya  mimar ise
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet]
        public IActionResult ExportHakedis(int ay, int yil)
        {
            var calisanlar = _resulContext.Calisans.ToList();
            var yevmiyeler = _resulContext.Yevmiyelers.Where(y => y.Tarih.Value.Year == yil && y.Tarih.Value.Month == ay).ToList();
            var mesailer = _resulContext.Mesais.Where(m => m.Tarih.Year == yil && m.Tarih.Month == ay).ToList();

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
                        if(calisan.Authority == 4) { continue; }
                        sheet.Cell(startRow, 1).Value = $"{calisan.Name} {calisan.Surname}";
                        sheet.Cell(startRow, 1).Style.Font.FontSize = 14;
                        sheet.Cell(startRow, 2).Value = "ALÇI-SIVA";

                        double amberCellsCount = 0;
                        double fullCellsCount = 0;
                        for (int gun = 1; gun <= DateTime.DaysInMonth(yil, ay); gun++)
                        {
                            var record = data.FirstOrDefault(y => y.CalisanId == calisan.CalısanId && y.Tarih.Day == gun);
                            if (record != null)
                            {
                                var cell = sheet.Cell(startRow, gun + 2);
                                if(record.IsFullWorked == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Gray;
                                    fullCellsCount++;
                                }
                                else if(record.IsWorkedCompany == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Amber;
                                    amberCellsCount = amberCellsCount + (0.5);
                                }
                                else if (record.IsWorked == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Amber;
                                    amberCellsCount = amberCellsCount + (0.5);
                                }
                            }
                        }

                        // Çalıştığı Gün Toplam sütunu
                        var totalCell = sheet.Cell(startRow, DateTime.DaysInMonth(yil, ay) + 3);
                        totalCell.Value = isMesai ? amberCellsCount + fullCellsCount : amberCellsCount + fullCellsCount;
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
                                else if(record.IsWorkedCompany == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Gray;
                                    greyCellsCount++;
                                }
                                else if(record.IsHalfWorked == true)
                                {
                                    cell.Style.Fill.BackgroundColor = XLColor.Gray;
                                    amberCellCount = amberCellCount + 1;
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
    }
}
