using MARS_MELA_PROJECT.Repository_Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PortalLib.Framework.Utilities;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class EmailHelper : IEmailHelper
    {
        private readonly string _cs;

        public EmailHelper(IOptions<DBConfig> options)
        {
            _cs = options.Value.DefaultConnection;
        }



        public void ClearEmailToken(string email,string mobile)
        {
            using (SqlConnection conn = new SqlConnection(_cs))
            {
                SqlCommand cmd = new SqlCommand(@"
UPDATE Users
SET EmailVerificationToken = NULL,
    TokenGeneratedAt = NULL
WHERE EmailID = @Email And MobileNo=@@MobileNo", conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@@MobileNo", mobile);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
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
        public void SendVerificationMail(string email,string mobile)
        {
            string Email = email;
            string token = Guid.NewGuid().ToString();
            DateTime generatedAt = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(_cs))
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
            email = PortalEncryption.EncryptPasswordNew(email);
            mobile = PortalEncryption.EncryptPasswordNew(mobile);
            string verifyUrl = $"https://localhost:7114/Account/EmailVerify?token={token}&email={email}&mobile={mobile}";



            string body = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Email Verification</title>
</head>
<body style='margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif;'>

    <table width='100%' cellpadding='0' cellspacing='0'>
        <tr>
            <td align='center'>

                <table width='600' cellpadding='0' cellspacing='0' 
                       style='background:#ffffff; margin:40px auto; border-radius:8px; overflow:hidden;'>

                    <!-- Header -->
                    <tr>
                        <td style='background:#0d6efd; padding:20px; text-align:center; color:#ffffff;'>
                            <h1 style='margin:0; font-size:24px;'>MARS MELA</h1>
                            <p style='margin:5px 0 0;'>Email Verification</p>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style='padding:30px; color:#333333;'>
                            <h2 style='margin-top:0;'>Verify Your Email Address</h2>

                            <p>
                                Thank you for registering with <b>MARS MELA</b>.
                                Please confirm your email address by clicking the button below.
                            </p>

                            <div style='text-align:center; margin:30px 0;'>
                                <a href='{verifyUrl}' 
                                   style='background:#0d6efd; color:#ffffff; padding:12px 30px;
                                          text-decoration:none; border-radius:5px; font-size:16px;'>
                                    Verify Email
                                </a>
                            </div>

                            <p>
                                If the button doesn’t work, copy and paste the link below into your browser:
                            </p>

                            <p style='word-break:break-all; color:#0d6efd;'>
                                {verifyUrl}
                            </p>

                            <p style='margin-top:30px; font-size:14px; color:#777777;'>
                                This verification link will expire in <b>24 minutes</b>.
                                If you did not create this account, please ignore this email.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background:#f1f1f1; padding:15px; text-align:center; font-size:13px; color:#666666;'>
                            © {DateTime.Now.Year} MARS MELA. All rights reserved.
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";

           
            SendMail(Email, "Verify Your Email", body);
        }

    




        public void SendForgotpasswordMail(string email,string Mobile)
        {
            string Email = email;
            string token = Guid.NewGuid().ToString();
            DateTime generatedAt = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(_cs))
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
            email = PortalEncryption.EncryptPasswordNew(email);
            Mobile = PortalEncryption.EncryptPasswordNew(Mobile);
            string resetUrl = $"https://localhost:7114/Account/EmailVerify?token={token}&email={email}&mobile={Mobile}";

            

            string body = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Reset Your Password</title>
</head>
<body style='margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0'>
<tr>
<td align='center'>

<table width='600' cellpadding='0' cellspacing='0'
       style='background:#ffffff; margin:40px auto; border-radius:8px; overflow:hidden;'>

<!-- Header -->
<tr>
<td style='background:#dc3545; padding:20px; text-align:center; color:#ffffff;'>
    <h1 style='margin:0; font-size:24px;'>MARS MELA</h1>
    <p style='margin:5px 0 0;'>Password Reset Request</p>
</td>
</tr>

<!-- Body -->
<tr>
<td style='padding:30px; color:#333333;'>

<h2 style='margin-top:0;'>Reset Your Password</h2>

<p>
We received a request to reset the password for your <b>MARS MELA</b> account.
</p>

<p>
Click the button below to create a new password.
</p>

<div style='text-align:center; margin:30px 0;'>
<a href='{resetUrl}'
   style='background:#dc3545; color:#ffffff; padding:12px 30px;
          text-decoration:none; border-radius:5px; font-size:16px;'>
Reset Password
</a>
</div>

<p>
If the button does not work, copy and paste the link below into your browser:
</p>

<p style='word-break:break-all; color:#dc3545;'>
{resetUrl}
</p>

<p style='margin-top:30px; font-size:14px; color:#777777;'>
⏱ This password reset link will expire in <b>15 minutes</b>.<br/>
If you did not request a password reset, please ignore this email.
</p>

</td>
</tr>

<!-- Footer -->
<tr>
<td style='background:#f1f1f1; padding:15px; text-align:center; font-size:13px; color:#666666;'>
© {DateTime.Now.Year} MARS MELA. All rights reserved.
</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>";

           

            SendMail(Email, "Reset Your Password - MARS MELA", body);
        }

    }



}
