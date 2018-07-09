USE HCS
BEGIN TRAN

INSERT INTO Invoice (INV_Client_ID, INV_NUM, INV_Date, INV_Billing_Name, INV_Qty, INV_Details, INV_Price, INV_Tax, INV_Total, INV_Notes, INV_Paid) 
VALUES (1, 1,'2018-07-05','Bob Frank', 1, 'Cable', 15.00, 1.23, 16.23, 'This is a test', 0)

--Commit
--Rollback


SELECT *FROM Invoice
USE HCS SELECT MAX(INV_Num) as INV_NUM FROM Invoice