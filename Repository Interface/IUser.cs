using MARS_MELA_PROJECT.Models;


namespace MARS_MELA_PROJECT.Repository
{
    public interface IUser
    {
        public int AddUser(SignUP Sign);
        public int emailverificationcheck(string token, string email);

        public void updateEmailVerified(string email);


    }
}
