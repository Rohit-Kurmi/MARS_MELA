using Humanizer;
using MARS_MELA_PROJECT;
using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository;
using MARS_MELA_PROJECT.Repository_Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;


namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class User : IUser
    {
        private readonly EmailHelper _emailHelper;
        private readonly string cs;

        public User(IOptions<DBConfig> options, EmailHelper emailHelper)
        {
            cs = options.Value.DefaultConnection;
            _emailHelper = emailHelper;
        }



        public int AddUser(SignUP Sign)
        {
            int EmailVerified = 0;
            int MobileVerified = 0;
            int Status = 1;
            int result = 0;
            int userId = 0;
            DateTime CreatedAt = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                // START TRANSACTION
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM Users WHERE MobileNo = @MobileNo OR EmailID=@EmailID)
BEGIN
    SELECT -1 AS Result, NULL AS UserId;
END
ELSE
BEGIN
    INSERT INTO Users 
    (MobileNo, EmailID, EmailVerified, MobileVerified, Status,
     FirstName, LastName, CreatedAt)
    VALUES 
    (@MobileNo, @EmailID, @EmailVerified, @MobileVerified, @Status,
     @FirstName, @LastName, @CreatedAt);

    DECLARE @NewUserId BIGINT = SCOPE_IDENTITY();

    INSERT INTO UserRoleMapping (UserID, RoleID, AssignedAt)
    VALUES (@NewUserId, 1, GETDATE());

    SELECT 1 AS Result, @NewUserId AS UserId;
END
", conn, tran))
                    {
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@MobileNo", Sign.MobileNo);
                        cmd.Parameters.AddWithValue("@EmailID", Sign.EmailID);
                        cmd.Parameters.AddWithValue("@EmailVerified", EmailVerified);
                        cmd.Parameters.AddWithValue("@MobileVerified", MobileVerified);
                        cmd.Parameters.AddWithValue("@Status", Status);
                        cmd.Parameters.AddWithValue("@FirstName", Sign.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", Sign.LastName);
                        cmd.Parameters.AddWithValue("@CreatedAt", CreatedAt);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                result = Convert.ToInt32(dr["Result"]);

                                if (result == 1 && dr["UserId"] != DBNull.Value)
                                {
                                    userId = Convert.ToInt32(dr["UserId"]);
                                }
                            }
                        }
                    }
                    if (result == -1)
                    {
                        tran.Rollback();
                        return -1;
                    }

                   

                    // COMMIT
                    tran.Commit();
                   
                    return result;
                }



                catch
                {
                    // ROLLBACK if anything fails
                    tran.Rollback();
                    throw;
                }
            }
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


                    if ((now - tokenTime).TotalHours <= 0.0167)
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
            DateTime dot = DateTime.Now;
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
            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                SqlCommand checkCmd = new SqlCommand(
                @"SELECT OTPGeneratedAt, OTPAttempts 
          FROM Users WHERE MobileNo=@MobileNo", conn);

                checkCmd.Parameters.AddWithValue("@MobileNo", mob.MobileNo);

                DateTime? lastGenerated = null;
                int attempts = 0;

                using (SqlDataReader dr = checkCmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lastGenerated = dr["OTPGeneratedAt"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(dr["OTPGeneratedAt"]);

                        attempts = dr["OTPAttempts"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(dr["OTPAttempts"]);
                    }
                }

                // ⏱️ 1 MINUTE RESEND BLOCK
                if (lastGenerated != null &&
                    (DateTime.Now - lastGenerated.Value).TotalMinutes < 1)
                {
                    return "WAIT_1_MIN"; // special flag
                }

                // 🔴 3 OTP → BLOCK FOR 1 HOUR
                if (attempts >= 3 && lastGenerated != null &&
                    (DateTime.Now - lastGenerated.Value).TotalHours < 1)
                {
                    return "BLOCK_1_HOUR";
                }

                // 🔁 1 HOUR PASSED → RESET ATTEMPTS
                if (attempts >= 3 && lastGenerated != null &&
                    (DateTime.Now - lastGenerated.Value).TotalHours >= 1)
                {
                    attempts = 0;
                }

                // ✅ GENERATE OTP
                string otp = new Random(Guid.NewGuid().GetHashCode())
                                .Next(100000, 999999).ToString();

                SqlCommand genCmd = new SqlCommand(
                @"UPDATE Users 
          SET OTPCode=@OTP,
              OTPGeneratedAt=@GeneratedAt,
              OTPAttempts=@Attempts
          WHERE MobileNo=@MobileNo", conn);

                genCmd.Parameters.AddWithValue("@OTP", otp);
                genCmd.Parameters.AddWithValue("@GeneratedAt", DateTime.Now);
                genCmd.Parameters.AddWithValue("@Attempts", attempts + 1);
                genCmd.Parameters.AddWithValue("@MobileNo", mob.MobileNo);

                int rows = genCmd.ExecuteNonQuery();
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

                    // OTP expired (valid for 1 minutes)
                    if ((DateTime.Now - generatedTime).TotalMinutes > 1)
                        return -1;

                    // Update mobile verified
                    SqlCommand updateCmd = new SqlCommand(
                        @"UPDATE Users SET MobileVerified = 1,MobileNoVerifiedAt=@MobileNoVerifiedAt WHERE MobileNo=@MobileNo",
                        conn
                    );

                    updateCmd.Parameters.AddWithValue("@MobileNo", mob.MobileNo);
                    updateCmd.Parameters.AddWithValue("@MobileNoVerifiedAt", DOB);
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


        public int SignIn(EnterPassword pass)
        {
            DateTime dot = DateTime.Now;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    // 1️⃣ Get Password
                    string dbPassword = null;
                    SqlCommand cmd = new SqlCommand(
                        "SELECT PasswordHash FROM Users WHERE MobileNo=@MobileNo",
                        con, trans);

                    cmd.Parameters.AddWithValue("@MobileNo", pass.MobileNo);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dbPassword = reader["PasswordHash"]?.ToString();
                        }
                        else
                        {
                            return 0; // user not found
                        }
                    }

                    // 2️⃣ Compare password
                    if (dbPassword != pass.PasswordHash)
                    {
                        return -1; // wrong password
                    }

                    // 3️⃣ Update last login
                    SqlCommand updateCmd = new SqlCommand(
                        "UPDATE Users SET LastLoginAt=@LastLoginAt WHERE MobileNo=@MobileNo",
                        con, trans);

                    updateCmd.Parameters.AddWithValue("@MobileNo", pass.MobileNo);
                    updateCmd.Parameters.AddWithValue("@LastLoginAt", dot);
                    updateCmd.ExecuteNonQuery();

                    // 4️⃣ Get RoleID
                    int roleId = 0;
                    SqlCommand roleCmd = new SqlCommand(
                        @"SELECT UR.RoleID 
                  FROM UserRoleMapping UR
                  INNER JOIN Users U ON U.UserID = UR.UserID
                  WHERE U.MobileNo=@MobileNo",
                        con, trans);

                    roleCmd.Parameters.AddWithValue("@MobileNo", pass.MobileNo);

                    using (SqlDataReader roleReader = roleCmd.ExecuteReader())
                    {
                        if (roleReader.Read())
                        {
                            roleId = Convert.ToInt32(roleReader["RoleID"]);
                        }
                    }

                    // 5️⃣ Commit transaction
                    trans.Commit();

                    return roleId; // ✅ success
                }
                catch (Exception)
                {
                    trans.Rollback();
                    return 0;
                }
            }
        }




        public bool IsUserExist(string email, string mobile)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT 1 FROM Users WHERE EmailID=@Email AND MobileNo=@Mobile",
                    conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Mobile", mobile);

                conn.Open();
                var result = cmd.ExecuteScalar();

                return result != null;
            }
        }



        public void MarkUserUnverifiedForForgot(string email, string mobile)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
            UPDATE Users
            SET 
                EmailVerified = 0,
                MobileVerified = 0,
                Status = 0
               
            WHERE EmailID = @Email AND MobileNo = @Mobile
        ", conn);

                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Mobile", mobile);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }



    }
}
