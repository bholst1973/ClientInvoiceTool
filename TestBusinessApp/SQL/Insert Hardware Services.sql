USE HCS
BEGIN TRAN

INSERT INTO GoodsServices(GS_Category, GS_Details,GS_Price, GS_Active)
VALUES ('Custom PC','Custom PC', 0.00, 1),
	   ('Router','WRT610N-RM',125.00, 1),
	   ('Printer','Canon Pixma MX922',120.00, 1),
	   ('Service','PC-Cleanup', 75.00, 1),
	   ('Service','OS Installation', 125.00, 1),
	   ('Service','Printer Installation', 65.00, 1),
	   ('Service','Router Installation', 65.00, 1),
	   ('Service','Virus Removal', 75.00, 1),
	   ('Service','New Computer Setup', 75.00, 1),
	   ('Desktop','Acer ATC-780-UR61, Intel Core i5-6400, 8GB RAM, 1TB HDD, Windows 10 Home', 650.00, 1),
	   ('Laptop','Acer E5-573G 15.6", Intel Core i5 5200U, 8GB RAM, 1TB HDD,  940M 2GB GPU, Windows 10 Home',650.00, 1)


--COMMIT
--ROLLBACK