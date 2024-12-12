using Karakoç.Bussiness.concrete;
using Karakoç.MailService;
using MernisServiceReference;
using Microsoft.AspNetCore.Mvc;


public class LoginController : Controller
{
    private readonly LoginManager _loginManager;
    private readonly KPSPublicSoapClient _mernisClient;

    public LoginController(LoginManager loginManager)
    {
        _loginManager = loginManager;
        _mernisClient = new KPSPublicSoapClient(KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
    }

    //public async Task<bool> TCKimlikDogrula(long tckimlikNo, string ad, string soyad, int dogumYili)
    //{
    //    var requestBody = new TCKimlikNoDogrulaRequestBody(tckimlikNo, ad, soyad, dogumYili);
    //    var request = new TCKimlikNoDogrulaRequest(requestBody);

    //    try
    //    {
    //        var response = await _mernisClient.TCKimlikNoDogrulaAsync(request);
    //        return response.Body.TCKimlikNoDogrulaResult;
    //    }
    //    catch (Exception ex)
    //    {
    //        // Hata durumunu yönet
    //        Console.WriteLine("Mernis doğrulama hatası: " + ex.Message);
    //        return false;
    //    }
    //}



    [HttpPost]
    public IActionResult PasswordReset(string Reset_Mail)
    {
        Random random = new Random();

        if (_loginManager.Reset(Reset_Mail)) //bu e posta var mı
        {
            int sayi = random.Next(111111, 999999);
            if (MailService.SendEmail(Reset_Mail, "Şifre Sıfırlama", $"Şifre Sıfırlama Kodun: {sayi}"))
            {
                ViewBag.Sent = "Gönderildi";
                ViewBag.Sayi = sayi;
            }
            else
            {
                ViewBag.Sent = "Hata Oluştu ve Gönderilemedi";
            }
        }
        else
        {
            ViewBag.Sent = "Girdiğiniz E-Posta Sistemde Kayıtlı Değil";

        }

        return View();

    }

    public IActionResult PasswordReset()
    {
        return View();
    }


    [HttpPost]
    public IActionResult Login(string username, string password)
    {
       

        if (_loginManager.Login(username, password, HttpContext)) // HttpContexti doğrudan kullanıcam
        {
            if (HttpContext.Session.GetInt32("Authority") == 1) //Calisan
            {
                return RedirectToAction("Home", "Calisan");
            }
            else if (HttpContext.Session.GetInt32("Authority") == 2) //Organizer
            {
                return RedirectToAction("Giderler", "Organizer");
            }
            else if (HttpContext.Session.GetInt32("Authority") == 3) //Admin
            {
                return RedirectToAction("Index", "Admin");
            }
            else if(HttpContext.Session.GetInt32("Authority") == 4)
            {
                return RedirectToAction("Index", "Sef");
            }
            else
            {
                ViewBag.ErrorMessage = "Email veya Parola Yanlış";
                return View("Giris");
            }
        }
        else
        {
            ViewBag.ErrorMessage = "Email veya Parola Yanlış";
            return View("Giris");
        }
    }

    [HttpPost]
    public  IActionResult Register( string Rusername, string Rlastname, string Remail, string Rpassword)
    {
        // Mernis doğrulaması
       // bool dogrulamaSonucu = await TCKimlikDogrula(RTC, Rusername, Rlastname, RBirth.Year);  --- Geçici Olarak Devre Dışı ---

          // Kayıt işlemi geçerli mi kontrol ediliyor
            if (!ModelState.IsValid)
            {
                ViewBag.Info = "Geçersiz bilgi girdiniz.";
                return View("KayıtOl");
            }

            // Kayıt işlemi yapılmaya çalışılıyor
            if (_loginManager.Register(Rusername, Rlastname, Remail, Rpassword))
            {
                ViewBag.SuccessMessage = "Kayıt işlemi başarılı!";
                return View("Giris"); // Kayıt başarılıysa giriş sayfasına yönlendiriliyor
            }
            else
            {
                ViewBag.Info = "Mail zaten kullanılıyor";
                return View("KayıtOl"); // Kayıt başarısızsa tekrar kayıt sayfasına dönülüyor
            }
       
    }

    public IActionResult Giris(string Email, string password)
    {
        if (HttpContext.Request.Cookies["Authority"] == "4")
        {
            return RedirectToAction("Index", "Sef");
        }
        else if (HttpContext.Request.Cookies["Authority"] == "3")
        {
            return RedirectToAction("Index", "Admin");
        }
        else if (HttpContext.Request.Cookies["Authority"] == "2")
        {
            return RedirectToAction("Home", "Organizer");
        }
        else if (HttpContext.Request.Cookies["Authority"] == "1")
        {
            return RedirectToAction("Home", "Calisan");
        }

        return View();
    }

    public IActionResult KayıtOl()
    {
        return View();
    }

    public IActionResult LogOut()
    {
        HttpContext.Response.Cookies.Delete("CalisanId");
        HttpContext.Response.Cookies.Delete("UserName");
        HttpContext.Response.Cookies.Delete("UserSurName");
        HttpContext.Response.Cookies.Delete("UserEmail");
        HttpContext.Response.Cookies.Delete("Authority");
        HttpContext.Response.Cookies.Delete("KayitTarihi");
        HttpContext.Response.Cookies.Delete("isLogged");

        HttpContext.Session.Clear();
        return RedirectToAction("Giris", "Login");
    }
}
