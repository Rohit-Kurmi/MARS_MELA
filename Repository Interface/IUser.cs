using MARS_MELA_PROJECT.Models;


namespace MARS_MELA_PROJECT.Repository
{
    public interface IUser
    {
        public int AddUser(SignUP Sign);
        public string CheckOnSignIN(SignIn sign);
        public int emailverificationcheck(string token, string email);

        public void updateEmailVerified(string email);
        public string GenerateAndSaveOTP(Mobileverification mob);
        public int verification(Mobileverification mob);
        public int SavePassword(string mobileNo, string passwordHash);
       

    }
}
