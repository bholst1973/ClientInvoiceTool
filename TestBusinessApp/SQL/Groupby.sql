USE HCS
SELECT INV_Num,
INV_Date,
INV_Billing_Name, 
SUM(INV_Price * INV_Qty) AS 'Sub_Total',
SUM(INV_Tax) AS 'Tax', 
ROUND(SUM(INV_Total),2) AS 'Total', 
ROUND(SUM(INV_Cost),2) AS 'Cost',  
ROUND(SUM(INV_TaxPaid),2) AS 'Tax_Paid',
INV_Paid
FROM INVOICE
GROUP BY INV_NUM, INV_Billing_Name, INV_Date, INV_Paid
ORDER BY INV_NUM


