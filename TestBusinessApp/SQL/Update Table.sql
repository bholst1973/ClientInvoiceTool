USE HCS

BEGIN TRAN

UPDATE GoodsServices

SET GS_Category = 'Service' WHERE GS_Category = 'Serice'

--Commit
--Rollback