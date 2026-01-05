using Humanizer;
using MARS_MELA_PROJECT.Models;
using MARS_MELA_PROJECT.Repository_Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PortalLib.Framework.Utilities;
using System.Data;

namespace MARS_MELA_PROJECT.Repository_Implementation
{
    public class SuperAdmin:ISuperAdmin
    {
        private readonly IWebHostEnvironment _env;
        private readonly string cs;

        public SuperAdmin(IOptions<DBConfig> options, IWebHostEnvironment env)
        {
            cs = options.Value.DefaultConnection;
            _env = env;
           
        }


        public void AddTraid(CreateTradeFair CTF, string session)
        {
            string filename = string.Empty;
            string fullpath = string.Empty;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // 🔹 1️⃣ Get UserId (transaction ke andar)
                    long createdByUserId;

                    string userQuery = "SELECT UserID FROM Users WHERE MobileNo = @MobileNo";

                    using (SqlCommand cmd = new SqlCommand(userQuery, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@MobileNo", session);
                        object result = cmd.ExecuteScalar();

                        if (result == null)
                            throw new Exception("User not found");

                        createdByUserId = Convert.ToInt64(result);
                    }

                    // 🔹 2️⃣ Prepare image details (NO SAVE YET)
                    string token = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(CTF.FairLogo.FileName);
                    filename = token + extension;

                    string folderpath = Path.Combine(_env.WebRootPath, "uploads", "Fairlogos");
                    if (!Directory.Exists(folderpath))
                        Directory.CreateDirectory(folderpath);

                    fullpath = Path.Combine(folderpath, filename);
                    string dbPath = "/uploads/Fairlogos/" + filename;

                    // 🔹 3️⃣ Insert TradeFair (inside transaction)
                    string insertQuery = @"INSERT INTO TradeFair
                (FairName, Division, District, Tehsil, City,
                 StartDate, EndDate, ApplyStartDate, ApplyEndDate,
                 FairLogoPath, ContactMobile1, ContactMobile2, ContactEmail,
                 Status, CreatedBy, CreatedAt)
                VALUES
                (@FairName, @Division, @District, @Tehsil, @City,
                 @StartDate, @EndDate, @ApplyStartDate, @ApplyEndDate,
                 @FairLogoPath, @ContactMobile1, @ContactMobile2, @ContactEmail,
                 @Status, @CreatedBy, @CreatedAt)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@FairName", CTF.FairName);
                        cmd.Parameters.AddWithValue("@Division", CTF.Division);
                        cmd.Parameters.AddWithValue("@District", CTF.District);
                        cmd.Parameters.AddWithValue("@Tehsil", CTF.Tehsil);
                        cmd.Parameters.AddWithValue("@City", CTF.City);

                        cmd.Parameters.AddWithValue("@StartDate", CTF.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", CTF.EndDate);
                        cmd.Parameters.AddWithValue("@ApplyStartDate", CTF.ApplyStartDate);
                        cmd.Parameters.AddWithValue("@ApplyEndDate", CTF.ApplyEndDate);

                        cmd.Parameters.AddWithValue("@FairLogoPath", dbPath);
                        cmd.Parameters.AddWithValue("@ContactMobile1", CTF.ContactMobile1);
                        cmd.Parameters.AddWithValue("@ContactMobile2", CTF.ContactMobile2);
                        cmd.Parameters.AddWithValue("@ContactEmail", CTF.ContactEmail);

                        cmd.Parameters.AddWithValue("@Status", 1);
                        cmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }

                    // 🔹 4️⃣ Save image AFTER DB success
                    using (FileStream fs = new FileStream(fullpath, FileMode.Create))
                    {
                        CTF.FairLogo.CopyTo(fs);
                    }

                    // ✅ 5️⃣ Commit transaction
                    tran.Commit();
                }
                catch
                {
                    // ❌ Rollback DB
                    tran.Rollback();

                    // ❌ Delete image if created
                    if (!string.IsNullOrEmpty(fullpath) && File.Exists(fullpath))
                        File.Delete(fullpath);

                    throw;
                }
            }
        }



        public List<RolesdropDown> GetRoles()
        {
            List<RolesdropDown> roles =new List<RolesdropDown>();

            using(SqlConnection conn =new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("SELECT RoleID, RoleName FROM UserRoles", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    roles.Add(new RolesdropDown
                    {
                        RoleID = Convert.ToInt32(dr["RoleID"]),
                        RoleName = dr["RoleName"].ToString()
                    });
                }
            }


            return roles;

        }


        public void MelaAdmin(MelaAdmin MEAL, string session)
        {
            DateTime dot = DateTime.Now;
            int verified = 1;

            string password = PortalEncryption.GetSHA512(MEAL.PasswordHash);

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    // 🔹 1️⃣ Get SuperAdmin UserID (AssignedBy)
                    long createdByUserId;

                    string userQuery = "SELECT UserID FROM Users WHERE MobileNo = @MobileNo";

                    using (SqlCommand cmd = new SqlCommand(userQuery, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@MobileNo", session);
                        createdByUserId = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    // 🔹 2️⃣ Check if user already exists by MobileNo or EmailID
                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE MobileNo = @MobileNo OR EmailID = @EmailID";
                    using (SqlCommand cmd = new SqlCommand(checkUserQuery, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@MobileNo", MEAL.MobileNo);
                        cmd.Parameters.AddWithValue("@EmailID", MEAL.EmailID);

                        int userExists = (int)cmd.ExecuteScalar();
                        if (userExists > 0)
                        {
                            throw new Exception("User with this MobileNo or EmailID already exists.");
                        }
                    }

                    // 🔹 3️⃣ Insert into Users & get NEW UserID
                    string insertUserQuery = @"
                INSERT INTO Users
                (FirstName, LastName, PasswordHash, MobileNo,
                 MobileNoVerifiedAt, MobileVerified,
                 EmailID, EmailVerified, EmailVerifieddAt,
                 Status, CreatedAt, CreatedBy)
                VALUES
                (@FirstName, @LastName, @PasswordHash, @MobileNo,
                 @MobileNoVerifiedAt, @MobileVerified,
                 @EmailID, @EmailVerified, @EmailVerifieddAt,
                 @Status, @CreatedAt, @CreatedBy);

                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                    long newUserId;

                    using (SqlCommand cmd = new SqlCommand(insertUserQuery, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", MEAL.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", MEAL.LastName);
                        cmd.Parameters.AddWithValue("@PasswordHash", password);
                        cmd.Parameters.AddWithValue("@MobileNo", MEAL.MobileNo);
                        cmd.Parameters.AddWithValue("@MobileNoVerifiedAt", dot);
                        cmd.Parameters.AddWithValue("@MobileVerified", verified);
                        cmd.Parameters.AddWithValue("@EmailID", MEAL.EmailID);
                        cmd.Parameters.AddWithValue("@EmailVerified", verified);
                        cmd.Parameters.AddWithValue("@EmailVerifieddAt", dot);
                        cmd.Parameters.AddWithValue("@Status", verified);
                        cmd.Parameters.AddWithValue("@CreatedAt", dot);
                        cmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);

                        newUserId = (long)cmd.ExecuteScalar();
                    }

                    // 🔹 4️⃣ Insert into UserRoleMapping
                    string roleMapQuery = @"
                INSERT INTO UserRoleMapping
                (UserID, RoleID, AssignedAt, AssignedBy)
                VALUES
                (@UserID, @RoleID, @AssignedAt, @AssignedBy)";

                    using (SqlCommand cmd = new SqlCommand(roleMapQuery, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@UserID", newUserId);
                        cmd.Parameters.AddWithValue("@RoleID", MEAL.RoleID);
                        cmd.Parameters.AddWithValue("@AssignedAt", DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@AssignedBy", createdByUserId);

                        cmd.ExecuteNonQuery();
                    }

                    // ✅ 5️⃣ Commit transaction
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }





    }
}
