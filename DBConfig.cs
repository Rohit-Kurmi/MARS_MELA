namespace MARS_MELA_PROJECT
{
    public class DBConfig
    {
        public string DefaultConnection { get; set; }
    }
}

//CREATE TABLE dbo.UserRoles (
//    RoleID INT IDENTITY(1,1) PRIMARY KEY,
//    RoleName NVARCHAR(100) NOT NULL UNIQUE -- e.g., Citizen, SuperAdmin, MelaAdmin, Finance, GateStaff, Inspector
//);
//GO

//CREATE TABLE dbo.UserRoleMapping (
//    MapID BIGINT IDENTITY(1,1) PRIMARY KEY,
//    UserID BIGINT NOT NULL,
//    RoleID INT NOT NULL,
//    AssignedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
//    AssignedBy BIGINT NULL,
//    CONSTRAINT FK_UserRoleMapping_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
//    CONSTRAINT FK_UserRoleMapping_Role FOREIGN KEY (RoleID) REFERENCES dbo.UserRoles(RoleID)
//);


//insert into UserRoles(RoleName) Values('Inspector');

//delete from Users

//DBCC CHECKIDENT('Users', RESEED,0);



//select* from Users


//select * from UserRoles

//select * from UserRoleMapping
