USE HCS
GO
CREATE PROCEDURE Profit_Calc
AS
SET NOCOUNT ON
UPDATE Invoice
SET INV_GrossProfit = ISNULL(INV_Price, 0) * isnull(INV_Qty,0) - ISNULL(INV_Cost,0)
