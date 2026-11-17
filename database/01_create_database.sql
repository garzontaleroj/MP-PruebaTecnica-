IF DB_ID(N'TaskManagementDb') IS NULL
BEGIN
    CREATE DATABASE TaskManagementDb;
END;
GO

USE TaskManagementDb;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Name nvarchar(120) NOT NULL,
        Email nvarchar(254) NOT NULL,
        CreatedAt datetime2(7) NOT NULL
            CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END;
GO

IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tasks
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY,
        Title nvarchar(200) NOT NULL,
        Description nvarchar(2000) NULL,
        UserId uniqueidentifier NOT NULL,
        Status varchar(20) NOT NULL
            CONSTRAINT DF_Tasks_Status DEFAULT 'Pending',
        AdditionalData nvarchar(max) NULL,
        CreatedAt datetime2(7) NOT NULL
            CONSTRAINT DF_Tasks_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Tasks_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id),
        CONSTRAINT CK_Tasks_Status
            CHECK (Status IN ('Pending', 'InProgress', 'Done')),
        CONSTRAINT CK_Tasks_AdditionalData_IsJson
            CHECK (AdditionalData IS NULL OR ISJSON(AdditionalData) = 1)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Tasks_UserId_Status_CreatedAt'
      AND object_id = OBJECT_ID(N'dbo.Tasks')
)
BEGIN
    CREATE INDEX IX_Tasks_UserId_Status_CreatedAt
        ON dbo.Tasks (UserId, Status, CreatedAt DESC);
END;
GO

-- Filter and sort a user's tasks. Both filters are optional in the API.
DECLARE @UserId uniqueidentifier = NULL;
DECLARE @Status varchar(20) = NULL;

SELECT Id, Title, Description, UserId, Status, AdditionalData, CreatedAt
FROM dbo.Tasks
WHERE (@UserId IS NULL OR UserId = @UserId)
  AND (@Status IS NULL OR Status = @Status)
ORDER BY CreatedAt DESC;
GO

-- Read scalar and object values from task metadata using SQL Server JSON functions.
SELECT
    Id,
    JSON_VALUE(AdditionalData, '$.priority') AS Priority,
    JSON_VALUE(AdditionalData, '$.estimatedCompletionDate') AS EstimatedCompletionDate,
    JSON_QUERY(AdditionalData, '$.tags') AS Tags
FROM dbo.Tasks
WHERE AdditionalData IS NOT NULL
  AND ISJSON(AdditionalData) = 1;
GO

-- Filter by a value inside JSON and expand its tags array.
SELECT task.Id, task.Title, tag.[value] AS Tag
FROM dbo.Tasks AS task
CROSS APPLY OPENJSON(task.AdditionalData, '$.tags') AS tag
WHERE ISJSON(task.AdditionalData) = 1
  AND JSON_VALUE(task.AdditionalData, '$.priority') = N'High';
GO

-- Optional targeted JSON update example (SQL Server 2017+).
-- UPDATE dbo.Tasks
-- SET AdditionalData = JSON_MODIFY(AdditionalData, '$.priority', 'High')
-- WHERE Id = @TaskId AND ISJSON(AdditionalData) = 1;
