USE HCS
--BEGIN TRAN

ALTER TABLE Invoice
ADD INV_GrossProfit Decimal(19,4)

--Commit
--Rollback