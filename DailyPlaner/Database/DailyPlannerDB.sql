USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'DailyPlannerDB')
BEGIN
    DROP DATABASE DailyPlannerDB;
END
GO

CREATE DATABASE DailyPlannerDB;
GO

USE DailyPlannerDB;
GO

CREATE TABLE Genders (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Roles (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(64) NOT NULL,
    Email NVARCHAR(100) NULL,
    GenderId INT NULL,
    RoleId INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (GenderId) REFERENCES Genders(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

CREATE TABLE Tasks (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    DueDate DATETIME NOT NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT N'Средний',
    Status NVARCHAR(50) NOT NULL DEFAULT N'Ожидает',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CHK_Tasks_Priority CHECK (Priority IN (N'Низкий', N'Средний', N'Высокий')),
    CONSTRAINT CHK_Tasks_Status CHECK (Status IN (N'Ожидает', N'Выполнена'))
);
GO

CREATE TABLE Events (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Location NVARCHAR(200) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Запланировано',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CHK_Events_Status CHECK (Status IN (N'Запланировано', N'Завершено')),
    CONSTRAINT CHK_Events_Dates CHECK (EndDate >= StartDate)
);
GO

CREATE TABLE Notes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(2000) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE TABLE Reminders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TaskId INT NULL,
    EventId INT NULL,
    ReminderTime DATETIME NOT NULL,
    Message NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    FOREIGN KEY (EventId) REFERENCES Events(Id),
    CONSTRAINT CHK_Reminders_Source CHECK (
        (TaskId IS NOT NULL AND EventId IS NULL) OR
        (TaskId IS NULL AND EventId IS NOT NULL))
);
GO

INSERT INTO Genders (Id, Name) VALUES
    (1, N'Мужской'),
    (2, N'Женский');
GO

INSERT INTO Roles (Id, Name) VALUES
    (1, N'Пользователь'),
    (2, N'Администратор');
GO

INSERT INTO Users (Username, PasswordHash, Email, GenderId, RoleId)
VALUES (
    N'admin',
    '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4',
    N'admin@example.com',
    1,
    2);
GO