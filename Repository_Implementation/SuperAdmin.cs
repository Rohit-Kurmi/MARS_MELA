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



        public void MelaAdmin(MelaAdmin MEAL,string session)
        {


            DateTime dot = DateTime.Now;
            int verified = 1;

            string querry = "INSERT INTO Users\r\n (FirstName,LastName,PasswordHash,MobileNo,MobileNoVerifiedAt,MobileVerified ,EmailID,EmailVerified,EmailVerifieddAt,Status,CreatedAt,CreatedBy)\r\n   " +
                " VALUES(@FirstName,@LastName,@PasswordHash,@MobileNo,@MobileNoVerifiedAt,@MobileVerified,@EmailID,@EmailVerified,@EmailVerifieddAt,@Status,@CreatedAt,@CreatedBy);\r\n";
            string password = PortalEncryption.GetSHA512(MEAL.PasswordHash);
            using(SqlConnection conn=new SqlConnection(cs))
            {
                conn.Open();
                // 🔹 1️⃣ Get UserId (transaction ke andar)
                long createdByUserId;

                string userQuery = "SELECT UserID FROM Users WHERE MobileNo = @MobileNo";

                using (SqlCommand cmd1 = new SqlCommand(userQuery, conn))
                {
                    cmd1.Parameters.AddWithValue("@MobileNo", session);
                    object result = cmd1.ExecuteScalar();

                    if (result == null)
                        throw new Exception("User not found");

                    createdByUserId = Convert.ToInt64(result);
                }




                SqlCommand cmd = new SqlCommand(querry, conn);
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@FirstName",MEAL.FirstName);
                cmd.Parameters.AddWithValue("@LastName",MEAL.LastName);
                cmd.Parameters.AddWithValue("@PasswordHash", password);
                cmd.Parameters.AddWithValue("@MobileNo",MEAL.MobileNo);
                cmd.Parameters.AddWithValue("@MobileNoVerifiedAt", dot);
                cmd.Parameters.AddWithValue("@MobileVerified", verified);
                cmd.Parameters.AddWithValue("@EmailID",MEAL.EmailID);
                cmd.Parameters.AddWithValue("@EmailVerified", verified);
                cmd.Parameters.AddWithValue("@EmailVerifieddAt",dot);
                cmd.Parameters.AddWithValue("@Status", verified);
                cmd.Parameters.AddWithValue("@CreatedAt", dot);
                cmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);

                
                cmd.ExecuteNonQuery();

            }

        }




    }
}
