using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository_Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;


namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class SignInCheckcs: ISignInCheck
    {
        private readonly string cs;

        public SignInCheckcs(IOptions<DBConfig> options)
        {
            cs = options.Value.DefaultConnection;
        }
        public string CheckOnSignIN(SignIn sign)
        {   

            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Query to get verification flags and password hash for given MobileNo
                SqlCommand cmd = new SqlCommand(
                    "SELECT EmailVerified, MobileVerified, PasswordHash FROM Users WHERE MobileNo = @MobileNo",
                    conn
                );

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@MobileNo", sign.MobileNo);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                // If user exists
                if (dr.Read())
                {
                    bool mobileVerified = Convert.ToBoolean(dr["MobileVerified"]);
                    bool emailVerified = Convert.ToBoolean(dr["EmailVerified"]);
                    bool hasPassword = dr["PasswordHash"] != DBNull.Value;

                    // Step 1: Mobile OR Email not verified
                    if (!emailVerified)
                    {
                        return "NEED_EMAIL_VERIFICATION";
                    }

                    if (!mobileVerified)
                    {
                        return "NEED_MOBILE_VERIFICATION";
                    }

                    // Step 2: Password is not created yet
                    if (!hasPassword)
                    {
                        return "CREATE_PASSWORD";
                    }

                    // Step 3: All good → allow login
                    return "LOGIN_ALLOWED";
                }
                else
                {
                    // No user found with this MobileNo
                    return "USER_NOT_FOUND";
                }
            }









            return ""; 
        }

    }
}
