namespace MARS_MELA_PROJECT.Repository_Interface
{
    public interface IEmailHelper
    {
        public void SendMail(string to, string subject, string body);
        public void SendVerificationMail(string email);
    }
}
