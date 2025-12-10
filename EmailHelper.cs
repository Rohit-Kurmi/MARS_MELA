using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Net;
using System.Net.Mail;

namespace MARS_MELA_PROJECT
{
    public class EmailHelper
    {

        public void sendmail(string to,string subject,string body)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(to);
            mail.Subject=subject;
            mail.Body=body;
            mail.IsBodyHtml = true;
            mail.From=new MailAddress("xyz@gmailcom");

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);

            smtp.Credentials = new NetworkCredential("","");
        }
    }
}
