    CREATE TABLE Users(
        UserID BIGINT IDENTITY(1,1) PRIMARY KEY,
         FirstName NVARCHAR(100) NULL,
LastName NVARCHAR(100) NULL,
CreatedBy VARCHAR(50) NULL,
PasswordHash NVARCHAR(512) NULL,
 LastLoginAt DATETIME2 NULL,
 MobileNo NVARCHAR(15) NULL,
  OTPCode NVARCHAR(20) NULL,
OTPGeneratedAt DATETIME2 NULL,
MobileNoVerifiedAt DATETIME null,
 MobileVerified BIT NOT NULL CONSTRAINT DF_Users_MobileVerified DEFAULT(0),

EmailID NVARCHAR(150) NULL,
EmailVerified BIT NOT NULL CONSTRAINT DF_Users_EmailVerified DEFAULT(0),
EmailVerificationToken nvarchar(200) null,
TokenGeneratedAt DATETIME null,
EmailVerifieddAt datetime null,
Status TINYINT NOT NULL CONSTRAINT DF_Users_Status DEFAULT(1), -- 1=Active,0=Inactive
CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
);


drop table Users

select * from Users

delete from Users