using MARS_MELA_PROJECT.Repository_Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class EmailHelper : IEmailHelper
    {
        private readonly string _cs;

        public EmailHelper(IOptions<DBConfig> options)
        {
            _cs = options.Value.DefaultConnection;
        }

        // SMTP SEND EMAIL
        public void SendMail(string to, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            mail.From = new MailAddress("rohitkurmi200304@gmail.com");

            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("rohitkurmi200304@gmail.com", "emxe acfb zlhb kyxo");
            smtp.Send(mail);
        }

        // SEND VERIFICATION EMAIL
        public void SendVerificationMail(string email)
        {
            string token = Guid.NewGuid().ToString();
            DateTime generatedAt = DateTime.Now;

            using(SqlConnection conn = new SqlConnection(_cs))
                    {
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Users 
                  SET EmailVerificationToken=@token,
                      TokenGeneratedAt=@dt 
                  WHERE EmailID=@Email", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@dt", generatedAt);
                cmd.Parameters.AddWithValue("@Email", email);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            string verifyUrl =$"https://localhost:7114/Account/EmailVerify?token={token}&email={email}";


            string body = $@"
                <h2>Email Verification</h2>
                <p>Click the link below to verify your email:</p>
                <a href='{verifyUrl}'>Verify Email</a>";

            SendMail(email, "Verify Your Email", body);
        }
    }
}
