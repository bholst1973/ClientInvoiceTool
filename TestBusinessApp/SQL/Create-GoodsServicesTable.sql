USE HCS
GO

CREATE TABLE GoodsServices
(
GS_ID int IDENTITY(1,1) PRIMARY KEY,
GS_Category VARCHAR (25),
GS_Details VARCHAR (250),
GS_Price Decimal (19, 4),
GS_Active BIT
);
