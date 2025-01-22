## Inventory System
A modern system for managing inventories, designed for small and medium-sized businesses. This application enables you to efficiently manage products, users, audit logs, and generate PDF reports. Developed using asp.net, with the code languages ​​c# and javascript for the backend, razor pages, css and bootstrap for the frontend and a database in sql server for storage
### Features

- Inventory Management: Record, view, and update your inventory information quickly and easily.
- User Management: View, register, update, and delete users who have access to the system.
- Audit Logs: Review a comprehensive history of all changes made to the records, ensuring accountability.
- PDF Reports: Generate reports of the records you need, whether products, users or changelogs.
- Security: Management of user accounts with specific permissions, use of recaptcha to control logins and forms validations in frontend and backen

## Setup
1.  Clone the repository:
   bash
   git clone https://github.com/al148506/InventorySystem
2. Open the project with Visual Studio 2022
3. Update the appsettings.json file with your database connection string.
4. Install the following NuGet packages:
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
5. Excecute in package manager console:
`Add-Migration "First Migration "`

## Database Schema  
This application uses Microsoft SQL Server. Use the following SQL script to create the database and its tables:

```sql
-- Create database
CREATE DATABASE [DB_Inventory];
GO

-- Use the datbase created
USE [DB_Inventory];
GO

-- Create tables
CREATE TABLE [dbo].[Category](
	[IdCategory] INT IDENTITY(1,1) PRIMARY KEY,
	[CategoryName] VARCHAR(50) NULL
);
GO

CREATE TABLE [dbo].[ChangeLog](
	[Id] INT IDENTITY(1,1) PRIMARY KEY,
	[UserId] VARCHAR(255) NOT NULL,
	[TypeAction] VARCHAR(255) NOT NULL,
	[TableName] VARCHAR(255) NOT NULL,
	[DateMod] DATETIME NOT NULL DEFAULT GETDATE(),
	[OldValues] VARCHAR(5000) NULL,
	[NewValues] VARCHAR(5000) NULL,
	[AffectedColumns] VARCHAR(255) NULL,
	[PrimaryKey] VARCHAR(255) NULL
);
GO

CREATE TABLE [dbo].[History](
	[IdHistory] INT IDENTITY(1,1) PRIMARY KEY,
	[IdProd] INT NULL,
	[IdUser] INT NULL,
	[DateMod] DATETIME NULL,
	[ChangeType] VARCHAR(50) NULL,
	[PreviousValue] TEXT NULL,
	[CurrentValue] TEXT NULL
);
GO

CREATE TABLE [dbo].[Location](
	[IdLocation] INT IDENTITY(1,1) PRIMARY KEY,
	[LocationName] VARCHAR(50) NULL
);
GO

CREATE TABLE [dbo].[Product](
	[IdProd] INT IDENTITY(1,1) PRIMARY KEY,
	[ProductName] VARCHAR(100) NULL,
	[Description] VARCHAR(250) NULL,
	[Quantity] INT NULL,
	[State] VARCHAR(50) NULL,
	[IdCategory] INT NULL,
	[CreationDate] DATETIME NULL,
	[LastModDate] DATETIME NULL,
	[ImageRoot] VARCHAR(250) NULL,
	[IdLocation] INT NULL,
	FOREIGN KEY (IdCategory) REFERENCES [dbo].[Category](IdCategory),
	FOREIGN KEY (IdLocation) REFERENCES [dbo].[Location](IdLocation)
);
GO

CREATE TABLE [dbo].[UserLogin](
	[IdUser] INT IDENTITY(1,1) PRIMARY KEY,
	[UserMail] VARCHAR(100) NULL,
	[UserPassword] VARCHAR(500) NULL,
	[UserName] VARCHAR(100) NULL,
	[CreationDate] DATETIME NULL,
	[IdRol] INT NULL,
	[LastModDate] DATETIME NULL
);
GO

CREATE TABLE [dbo].[UserRol](
	[IdRol] INT IDENTITY(1,1) PRIMARY KEY,
	[RolName] VARCHAR(50) NULL,
	[Description] VARCHAR(250) NULL
);
GO

-- Database table relationships
ALTER TABLE [dbo].[History] ADD FOREIGN KEY (IdProd) REFERENCES [dbo].[Product](IdProd);
ALTER TABLE [dbo].[History] ADD FOREIGN KEY (IdUser) REFERENCES [dbo].[UserLogin](IdUser);
ALTER TABLE [dbo].[UserLogin] ADD FOREIGN KEY (IdRol) REFERENCES [dbo].[UserRol](IdRol);
GO

-- Create Stored Procedures
CREATE PROCEDURE [dbo].[sp_RegisterUser]
	@Mail VARCHAR(100),
	@Password VARCHAR(500),
	@CreationDate DATETIME,
	@IdRol INT,
	@Registred BIT OUTPUT,
	@Message VARCHAR(100) OUTPUT
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM [dbo].[UserLogin] WHERE [UserMail] = @Mail)
	BEGIN
		INSERT INTO [dbo].[UserLogin] ([UserMail], [UserPassword], [CreationDate], [IdRol])
		VALUES (@Mail, @Password, @CreationDate, @IdRol);
		SET @Registred = 1;
		SET @Message = 'User registered';
	END
	ELSE
	BEGIN
		SET @Registred = 0;
		SET @Message = 'Email is already registered';
	END
END;
GO

CREATE PROCEDURE [dbo].[sp_UserValidation]
	@UserMail VARCHAR(100),
	@UserPassword VARCHAR(500)
AS
BEGIN
	IF EXISTS (SELECT * FROM [dbo].[UserLogin] WHERE [UserMail] = @UserMail AND [UserPassword] = @UserPassword)
	BEGIN
		SELECT [IdUser] FROM [dbo].[UserLogin]
		WHERE [UserMail] = @UserMail AND [UserPassword] = @UserPassword;
	END
	ELSE
	BEGIN
		SELECT CAST(0 AS INT) AS [IdUser];
	END
END;
GO>
```



