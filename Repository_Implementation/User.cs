using MARS_MELA_PROJECT;
using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;


namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class User : IUser
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
           DateTime CreatedAt= DateTime.Now;

            // Create SQL connection
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Use stored procedure: AddUser
                using (SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT 1 FROM Users WHERE MobileNo = @MobileNo)BEGIN  " +
                    " SELECT -1 AS Result; END  ELSE  BEGIN  INSERT INTO Users (MobileNo,EmailID,EmailVerified,MobileVerified,Status," +
                    " FirstName,LastName,CreatedBy,CreatedAt) VALUES ( @MobileNo, @EmailID,@EmailVerified,@MobileVerified, @Status,@FirstName," +
                    "@LastName,@CreatedBy,@CreatedAt); SELECT 1 AS Result; END", conn))
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
                    cmd.Parameters.AddWithValue("@CreatedAt", CreatedAt);

                    // Open connection
                    conn.Open();

                    // Execute the stored procedure and get return value
                    int result = Convert.ToInt32(cmd.ExecuteScalar());

                    return result;
                }
            }
        }



        public int emailverificationcheck(string token, string email)
        {

            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("select * from Users Where EmailVerificationToken=@token and EmailID=@EmailID", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", email);
                cmd.Parameters.AddWithValue("@token", token);
                conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    DateTime tokenTime = Convert.ToDateTime(dr["TokenGeneratedAt"]);
                    DateTime now = DateTime.Now;
                    bool emailVerified = Convert.ToBoolean(dr["EmailVerified"]);

                    if (emailVerified)
                    {
                        return 2;   // Already verified
                    }


                    if ((now - tokenTime).TotalHours <= 1)
                        {
                            return 1;
                        }

                        else
                        {
                            return -1;
                        }
                    
                }
            }


            return 0;
        }


        public void updateEmailVerified(string email)
        {
            DateTime dot= DateTime.Now;
            using SqlConnection conn = new SqlConnection(cs);
            SqlCommand cmd = new SqlCommand(
                "UPDATE Users SET EmailVerified=1,EmailVerifieddAt=@date WHERE EmailID=@Email", conn);

            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@date", dot);
            conn.Open();
            cmd.ExecuteNonQuery();
        }




        public string GenerateAndSaveOTP(Mobileverification mob)
        {
            string otp = new Random().Next(100000, 999999).ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Users 
              SET OTPCode = @otp, 
                  OTPGeneratedAt = @OTPGeneratedAt
              WHERE MobileNo = @MobileNo",
                    conn
                );

                cmd.Parameters.AddWithValue("@MobileNo", mob.MobileNo);
                cmd.Parameters.AddWithValue("@otp", otp);
                cmd.Parameters.AddWithValue("@OTPGeneratedAt", DateTime.Now);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                return rows > 0 ? otp : "";
            }
        }



        public int verification(Mobileverification mob)
        {
            DateTime DOB = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT OTPCode, OTPGeneratedAt FROM Users WHERE MobileNo=@MobileNo",
                    conn
                );

                cmd.Parameters.AddWithValue("@MobileNo", mob.MobileNo);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string storedOtp = dr["OTPCode"].ToString();
                    DateTime generatedTime = Convert.ToDateTime(dr["OTPGeneratedAt"]);

                    dr.Close();

                    // OTP mismatch
                    if (storedOtp != mob.OTPCode)
                        return 0;

                    // OTP expired (valid for 2 minutes)
                    if ((DateTime.Now - generatedTime).TotalMinutes > 2)
                        return -1;

                    // Update mobile verified
                    SqlCommand updateCmd = new SqlCommand(
                        @"UPDATE Users SET MobileVerified = 1,MobileNoVerifiedAt=@MobileNoVerifiedAt WHERE MobileNo=@MobileNo",
                        conn
                    );

                    updateCmd.Parameters.AddWithValue("@MobileNo", mob.MobileNo);
                    updateCmd.Parameters.AddWithValue("@MobileNoVerifiedAt",DOB);
                    return updateCmd.ExecuteNonQuery() > 0 ? 1 : 0;
                }

                return 0;
            }
        }




        public int SavePassword(string mobileNo, string passwordHash)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // SQL command to update user's password
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Users 
              SET PasswordHash = @pwd 
              WHERE MobileNo = @MobileNo",
                    conn
                );

                // Adding parameters safely
                cmd.Parameters.AddWithValue("@pwd", passwordHash);
                cmd.Parameters.AddWithValue("@MobileNo", mobileNo);

                conn.Open();

                // Execute update and return affected rows
                return cmd.ExecuteNonQuery();
            }
        }


    }
}
