using MARS_MELA_PROJECT.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using MARS_MELA_PROJECT;
using Microsoft.AspNetCore.Http.HttpResults;


namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class User
    {



        private readonly string cs;

        public User(IOptions<DBConfig> options)
        {
            cs = options.Value.DefaultConnection;
        }



        public int AddUser(SignUP Sign,string token)
        {
            // Default values for new user
            int EmailVerified = 0;
            int MobileVerified = 0;
            int Status = 1;
           DateTime CreatedAt= DateTime.Now;

            // Create SQL connection
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Use stored procedure: AddUser
                using (SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT 1 FROM Users WHERE MobileNo = @MobileNo)BEGIN  " +
                    " SELECT -1 AS Result; END  ELSE  BEGIN  INSERT INTO Users (MobileNo,EmailID,EmailVerified,MobileVerified,Status," +
                    " FirstName,LastName,CreatedBy,EmailVerificationToken,CreatedAt) VALUES ( @MobileNo, @EmailID,@EmailVerified,@MobileVerified, @Status,@FirstName," +
                    "@LastName,@CreatedBy,@EmailVerificationToken,@CreatedAt); SELECT 1 AS Result; END", conn))
                {
                    cmd.CommandType = CommandType.Text;

                    // Parameters for stored procedure
                    cmd.Parameters.AddWithValue("@MobileNo", Sign.MobileNo);
                    cmd.Parameters.AddWithValue("@EmailID", Sign.EmailID);
                    cmd.Parameters.AddWithValue("@EmailVerified", EmailVerified);
                    cmd.Parameters.AddWithValue("@MobileVerified", MobileVerified);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.Parameters.AddWithValue("@FirstName", Sign.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", Sign.LastName);
                    cmd.Parameters.AddWithValue("@CreatedBy", Sign.CreatedBy);
                    cmd.Parameters.AddWithValue("@EmailVerificationToken", token);
                    cmd.Parameters.AddWithValue("@CreatedAt", CreatedAt);

                    // Open connection
                    conn.Open();

                    // Execute the stored procedure and get return value
                    int result = Convert.ToInt32(cmd.ExecuteScalar());

                    return result;
                }
            }
        }



    }
}
