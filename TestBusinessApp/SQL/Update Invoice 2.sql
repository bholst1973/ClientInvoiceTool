USE HCS
Begin TRAN
SELECT * FROM Invoice
 where INV_Client_ID = 8


update Invoice set inv_Num = 14 where inv_Num = 15
update Invoice set INV_Date = '2009-08-01' where INV_NUM = 22
Update invoice set INV_Tax = 3.25 where INV_ID = 32

Update invoice set INV_Cost = 0.00 where INV_Cost is null
update invoice set INV_Paid = 1

select * from Invoice where INV_TaxPaid is null

--USE HCS
--Begin TRAN
--delete from Invoice where INV_NUM = 27
--commit
--rollback
