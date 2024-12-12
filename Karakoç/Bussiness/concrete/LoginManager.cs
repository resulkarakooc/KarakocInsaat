using Karakoç.Bussiness.abstracts;
using Karakoç.Models;


namespace Karakoç.Bussiness.concrete
{
    public class LoginManager : LoginService
    {
        private readonly ResulContext _context;

        public LoginManager(ResulContext context)
        {
            _context = context;
        }

        public bool Reset(string email)
        {
            var user = _context.Calisans
               .FirstOrDefault(c => c.Email == email);
            if(user == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool Login(string username, string password, HttpContext httpContext)
        {
            var hashpass = Cryptography.ShaConverter.ComputeSha256Hash(password);

            var user = _context.Calisans
                .FirstOrDefault(c => c.Email == username && c.Password == hashpass);

            if (user != null)
            {
                httpContext.Session.SetInt32("CalisanId", user.CalısanId);
                httpContext.Session.SetString("UserName", user.Name);
                httpContext.Session.SetString("UserSurName", user.Surname);
                httpContext.Session.SetString("UserEmail", user.Email);
                httpContext.Session.SetInt32("Authority", Convert.ToInt32(user.Authority));
                if (user.KayıtTarihi.HasValue) // Nullable kontrolü
                {
                    httpContext.Session.SetString("KayitTarihi", user.KayıtTarihi.Value.ToString("yyyy-MM-dd"));
                }

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(17), // Çerez 20 gün boyunca geçerli
                    HttpOnly = true, // Çereze tarayıcıdan erişimi sınırla (güvenlik için)
                    IsEssential = true // Çerezin her zaman oluşturulmasını zorunlu kıl
                };
               

                // Çerezlerde kullanıcı bilgilerini sakla
                httpContext.Response.Cookies.Append("CalisanId", user.CalısanId.ToString(), cookieOptions);
                httpContext.Response.Cookies.Append("UserName", user.Name, cookieOptions);
                httpContext.Response.Cookies.Append("UserSurName", user.Surname, cookieOptions);
                httpContext.Response.Cookies.Append("UserEmail", user.Email, cookieOptions);
                httpContext.Response.Cookies.Append("Authority", user.Authority.ToString(), cookieOptions);
                httpContext.Response.Cookies.Append("isLogged", "true", cookieOptions);

                if (user.KayıtTarihi.HasValue)
                {
                    httpContext.Response.Cookies.Append("KayitTarihi", user.KayıtTarihi.Value.ToString("yyyy-MM-dd"), cookieOptions);
                }

                return true;
            }
            
            else
            {
                return false;
            }

            

        }

        public bool Register(string Rusername, string Rlastname, string Remail, string Rpassword)
        {
            var user = _context.Calisans.FirstOrDefault(c => c.Email == Remail);
            if (user == null) //mevcut kayıt yok ise 
            {
                string hashpass =  Cryptography.ShaConverter.ComputeSha256Hash(Rpassword);
                var newCalisan = new Calisan
                {
                    
                    //TcKimlik = TC,
                    Name = Rusername,
                    Surname = Rlastname,
                    Email = Remail,
                    KayıtTarihi = DateTime.Now,
                    //BirthDate = BirthDay,
                    Password = hashpass,
                    Authority = 1,
                    Verify = false

                };
                // Yeni çalışanı veritabanına ekle ve değişiklikleri kaydet
                _context.Calisans.Add(newCalisan);
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
