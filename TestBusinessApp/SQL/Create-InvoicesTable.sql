USE HCS
GO

CREATE TABLE Invoice
(
INV_ID int IDENTITY(1,1),
INV_Client_ID INT,
INV_NUM INT,
INV_Date DATE,
INV_Billing_Name VarChar(50),
INV_Qty INT,
INV_Details VARCHAR (250),
INV_Price Decimal (19,4),
INV_Tax Decimal (19,4),
INV_Total Decimal (19,4),
INV_Notes VARCHAR (250),
INV_Paid Bit,
INV_Cost Decimal(19,4),
INV_TaxPaid Decimal(19,4),
INV_GrossProfit Decimal(19,4)
);
