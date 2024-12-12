using System;
using System.Net;
using System.Net.Mail;


namespace Karakoç.MailService
{
    public static class MailService
    {
        public static bool SendEmail(string recipientEmail, string konu, string mesaj)
        {

            try
            {
                string smtpServer = "smtp.gmail.com"; //SMTP sunucusu adresi
                int port = 587; //SMTP sunucusu port numarası
                string senderEmail = "resul.krkoc@gmail.com"; //Gönderen e-posta adresi
                string password = "ecugpjsuhqbrvnwy"; //Gönderen e-posta hesabının şifresi gerçek şifre değil

                MailMessage mail = new MailMessage();  //mail oluştur ve içeriği ekle 
                mail.From = new MailAddress(senderEmail);
                mail.To.Add(recipientEmail);
                mail.Subject = konu; //E-posta konusu
                mail.Body = mesaj; //E-posta içeriği

                SmtpClient smtpClient = new SmtpClient(smtpServer, port); //protokolü oluştur
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderEmail, password);
                smtpClient.EnableSsl = true; //güvenliği aç

                smtpClient.Send(mail); //gönder
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
