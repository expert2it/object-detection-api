
# Adding MySQL Entity Framework to asp .net core project
https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html


# Check the dafault .net framework version
dotnet --version

# List all installed .net framework versions:
dotnet --info

# Change .net framework version of the project: it will create a "global.json" file which is stored in the folder of the current version of your project's SDK. 
dotnet new globaljson --sdk-version 3.0.100-preview-010184 --force

# Adding EF Scaffold to the project

dotnet add package MySql.Data.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef dbcontext scaffold "server=IPADDRESS;port=3007;user=<USERNAME>;password=<PASSWORD>;database=cili_20190904;Allow Zero Datetime=True;Convert Zero Datetime=True;" MySql.Data.EntityFrameworkCore -o FolderName -f

# Or you can use Package Manager Console in Visual Studio:
Scaffold-DbContext "server=localhost;port=3306;user=root;password=mypass;database=dbname" MySql.Data.EntityFrameworkCore -OutputDir Sakila -Schemas db1,db2 -f

# Scaffolding a Database by Filtering Tables
dotnet ef dbcontext scaffold "server=localhost;port=3306;user=root;password=mypass;database=sakila" MySql.Data.EntityFrameworkCore -o sakila -t actor -t film -t film_actor -t language -f
OR
Scaffold-DbContext "server=localhost;port=3306;user=root;password=mypass;database=sakila" MySql.Data.EntityFrameworkCore -OutputDir Sakila -Tables actor,film,film_actor,language -f

