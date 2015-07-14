# MSPA-Reader
A parser for the website MSPA that will download comic content into a database for local, forms based, reading. Written in pure .NET.

This is an unofficial, fan-made program that is in no way associated with MS Paint Adventures, What Pumpkin?, or Andrew Hussie.

Simply build the Release configuration with visual studio, or in mono. Both will require nuget package restore (Make sure your ssl ceritificates won't block you from nuget.org). You can safely ignore the missing references.


Reccommend using SQLLocalDB or SQLServer if possible. Sqlite has shown to run into deadlocks during archive operations. Support for MySQL coming soon.


Requirements:

.NET Framework 4.5/Mono equivalent: https://www.microsoft.com/en-ca/download/details.aspx?id=30653

Flashplayer browser plugin: https://get.adobe.com/flashplayer/


You will probably also want a database that isn't SQLite:


SQLLocalDB (Recommended for Windows users. Choose SQLLocalDB.MSI for your architecture, WINDOWS ONLY): https://www.microsoft.com/en-ca/download/details.aspx?id=29062 

SQL Server Express (WINDOWS ONLY): https://www.microsoft.com/en-ca/download/details.aspx?id=42299

MySQL Community (Recommended for *nix users. Though you will need to install it from your distro's package manager, this is the windows link): http://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-web-community-5.6.25.0.msi

If using SQL Server or MySQL you will need to assign a user schema creation permissions and use it in the application. Using only the hostname paramater will attempt to connect with Windows Authentication (SQLServer only).

NOTE: CURRENTLY ONLY MYSQL WORKS ON LINUX

I know it's poorly documented. I did this for fun.


Dependancies:

HtmlAgilityPack

System.Data.Sqlite

Roxygen Karezi for endgame ships.