-- 1. Creación y Uso de la Database

CREATE DATABASE ETL_Opinion;
GO

USE ETL_Opinion;
GO

-- 2. Tablas de catalogo

CREATE TABLE Category (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,

    CONSTRAINT UQ_Category_Name UNIQUE (Name)
);
GO

CREATE TABLE Source_Type (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Type NVARCHAR(50) NOT NULL,

    CONSTRAINT UQ_Source_Type_Type UNIQUE (Type)
);
GO

CREATE TABLE Classification (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Type NVARCHAR(50) NOT NULL,

    CONSTRAINT UQ_Classification_Type UNIQUE (Type)
);
GO

-- 3. tablas Maestras

CREATE TABLE Client (
    ID INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,

    CONSTRAINT UQ_Client_Email UNIQUE (Email)
);
GO

CREATE TABLE Product (
    ID INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(75) NOT NULL,
    Category_ID INT NOT NULL
);
GO

CREATE TABLE Data_Source (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    External_ID VARCHAR(30) NULL,
    Name NVARCHAR(50) NOT NULL,
    Load_Date DATE NOT NULL,
    Source_Type_ID INT NOT NULL
);
GO


-- 4. Tabla Staging

CREATE TABLE Stage_Opinion (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    Origin VARCHAR(30) NOT NULL,
    Original_Opinion_ID VARCHAR(30) NOT NULL,
    Original_Client_ID VARCHAR(30) NULL,
    Original_Product_ID VARCHAR(30) NULL,

    Comment NVARCHAR(MAX) NULL,
    Original_Date VARCHAR(30) NULL,
    Original_Classification NVARCHAR(30) NULL,
    Original_Score VARCHAR(20) NULL,
    Source_Name NVARCHAR(50) NULL,

    Status VARCHAR(20) NOT NULL
        CONSTRAINT DF_StageOpinion_Status DEFAULT 'Pending',

    Error_Message NVARCHAR(500) NULL,

    Loaded_At DATETIME2(3) NOT NULL
        CONSTRAINT DF_StageOpinion_LoadedAt
        DEFAULT SYSUTCDATETIME()
)
GO

-- 5. Tabla central de opiniones

CREATE TABLE Opinion (
    ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    Comment NVARCHAR(MAX) NOT NULL,
    Satisfaction_Score INT NULL,
    [Date] DATE NOT NULL,

    Client_ID INT NULL,
    Product_ID INT NULL,

    Data_Source_ID INT NOT NULL,
    Classification_ID INT NOT NULL,

    Stage_Opinion_ID INT NOT NULL,

    CONSTRAINT UQ_Opinion_StageOpinion
        UNIQUE (Stage_Opinion_ID)
);
GO

-- 6. CLAVES FORÁNEAS


ALTER TABLE Product
ADD CONSTRAINT FK_Product_Category
FOREIGN KEY (Category_ID)
REFERENCES Category(ID);
GO

ALTER TABLE Data_Source
ADD CONSTRAINT FK_DataSource_SourceType
FOREIGN KEY (Source_Type_ID)
REFERENCES Source_Type(ID);
GO

ALTER TABLE Opinion
ADD CONSTRAINT FK_Opinion_Client
FOREIGN KEY (Client_ID)
REFERENCES Client(ID);
GO

ALTER TABLE Opinion
ADD CONSTRAINT FK_Opinion_Product
FOREIGN KEY (Product_ID)
REFERENCES Product(ID);
GO

ALTER TABLE Opinion
ADD CONSTRAINT FK_Opinion_DataSource
FOREIGN KEY (Data_Source_ID)
REFERENCES Data_Source(ID);
GO

ALTER TABLE Opinion
ADD CONSTRAINT FK_Opinion_Classification
FOREIGN KEY (Classification_ID)
REFERENCES Classification(ID);
GO

ALTER TABLE Opinion
ADD CONSTRAINT FK_Opinion_StageOpinion
FOREIGN KEY (Stage_Opinion_ID)
REFERENCES Stage_Opinion(ID);
GO


-- 7. Índices

-- Consultas por producto y rango de fechas
CREATE INDEX IX_Opinion_Product_Date
ON Opinion(Product_ID, [Date]);
GO

-- Búsqueda de opiniones pendientes de procesamiento
CREATE INDEX IX_StageOpinion_Status
ON Stage_Opinion(Status);
GO

-- Evitar IDs externos duplicados en las fuentes
-- Permite múltiples NULL
CREATE UNIQUE INDEX UX_DataSource_ExternalId
ON Data_Source(External_ID)
WHERE External_ID IS NOT NULL;
GO

-- Views

CREATE VIEW vw_OpinionDetails
AS
SELECT
    o.ID,
    o.Comment,
    o.Satisfaction_Score,
    o.Date,
    c.Name AS Client,
    p.Name AS Product,
    cl.Type AS Classification,
    ds.Name AS DataSource,
    st.Type AS SourceType
FROM Opinion o
LEFT JOIN Client c ON o.Client_ID = c.ID
LEFT JOIN Product p ON o.Product_ID = p.ID
INNER JOIN Classification cl
    ON o.Classification_ID = cl.ID
INNER JOIN Data_Source ds
    ON o.Data_Source_ID = ds.ID
INNER JOIN Source_Type st
    ON ds.Source_Type_ID = st.ID;
GO

-- procedures

CREATE PROCEDURE sp_OpinionSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cl.Type AS Classification,
        COUNT(*) AS Total
    FROM Opinion o
    INNER JOIN Classification cl
        ON o.Classification_ID = cl.ID
    GROUP BY cl.Type;
END;
GO


-- Consultas de Validación
-- ==================================================
SELECT 'Client' AS Tabla, COUNT(*) AS Total FROM Client
UNION ALL
SELECT 'Product', COUNT(*) FROM Product
UNION ALL
SELECT 'Category', COUNT(*) FROM Category
UNION ALL
SELECT 'Source_Type', COUNT(*) FROM Source_Type
UNION ALL
SELECT 'Classification', COUNT(*) FROM Classification
UNION ALL
SELECT 'Data_Source', COUNT(*) FROM Data_Source
UNION ALL
SELECT 'Stage_Opinion', COUNT(*) FROM Stage_Opinion
UNION ALL
SELECT 'Opinion', COUNT(*) FROM Opinion;
-- ==================================================

-- ==================================================
SELECT
    COUNT(*) AS RelacionesInvalidas
FROM Opinion o
LEFT JOIN Client c ON o.Client_ID = c.ID
LEFT JOIN Product p ON o.Product_ID = p.ID
WHERE
    (o.Client_ID IS NOT NULL AND c.ID IS NULL)
    OR
    (o.Product_ID IS NOT NULL AND p.ID IS NULL);
-- ==================================================

-- ==================================================
SELECT
    COUNT(*) AS RelacionesInvalidas
FROM Opinion o
LEFT JOIN Classification c
    ON o.Classification_ID = c.ID
LEFT JOIN Data_Source ds
    ON o.Data_Source_ID = ds.ID
WHERE
    c.ID IS NULL
    OR ds.ID IS NULL;
-- ==================================================

-- ==================================================
SELECT
    s.Status,
    COUNT(*) AS TotalStaging,
    COUNT(o.ID) AS TotalEnOpinion
FROM Stage_Opinion s
LEFT JOIN Opinion o
    ON s.ID = o.Stage_Opinion_ID
GROUP BY s.Status;
-- ==================================================

-- ==================================================
SELECT
    c.Type AS Clasificacion,
    COUNT(o.ID) AS TotalOpiniones
FROM Classification c
LEFT JOIN Opinion o
    ON c.ID = o.Classification_ID
GROUP BY c.Type
ORDER BY c.Type;
-- ==================================================

