using MARS_MELA_PROJECT.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using MARS_MELA_PROJECT;


namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class User
    {



        private readonly string cs;

        public User(IOptions<DBConfig> options)
        {
            cs = options.Value.DefaultConnection;
        }



        public int AddUser(SignUP Sign)
        {
            // Default values for new user
            int EmailVerified = 0;
            int MobileVerified = 0;
            int Status = 1;
            int IsUsed = 0;

            // Create SQL connection
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Use stored procedure: AddUser
                using (SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT 1 FROM Users WHERE MobileNo = @MobileNo)BEGIN  " +
                    " SELECT -1 AS Result; END  ELSE  BEGIN  INSERT INTO Users (MobileNo,EmailID,EmailVerified,MobileVerified,Status," +
                    " FirstName,LastName,CreatedBy, CreatedAt,IsUsed) VALUES ( @MobileNo, @EmailID,@EmailVerified,@MobileVerified, @Status,@FirstName," +
                    "@LastName,@CreatedBy,GETDATE(),@IsUsed); SELECT 1 AS Result; END", conn))
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
                    cmd.Parameters.AddWithValue("@IsUsed",IsUsed);

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
