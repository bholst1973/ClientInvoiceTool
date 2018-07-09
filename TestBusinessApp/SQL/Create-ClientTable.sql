USE HCS
GO

CREATE TABLE Client
(
Client_ID int IDENTITY(1,1) PRIMARY KEY,
First_Name VARCHAR (50),
Last_Name VARCHAR (50),
Business_Name VARCHAR (50),
Billing_Name VARCHAR (50),
Address1 VARCHAR (50),
Address2 VARCHAR (50),
Address3 VARCHAR (50),
City VARCHAR (50),
[State]  VARCHAR (2),
Zip VARCHAR(10),
Phone VARCHAR (25),
Email NVARCHAR(320)
);
