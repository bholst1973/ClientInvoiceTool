CREATE DATABASE HCS
ON(
Name = 'MyTestDB_data',
Filename = 'C:\C#Projects\TestBusinessApp\TestBusinessApp\Data\HCS.mdf',
size = 20MB,
Maxsize = 100MB,
Filegrowth = 5MB)
Log ON(
Name = 'MyTestDB_log',
Filename = 'C:\C#Projects\TestBusinessApp\TestBusinessApp\Data\HCS.ldf',
Size = 10MB,
Maxsize = 50MB,
Filegrowth = 5MB)



