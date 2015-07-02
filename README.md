# MSPA-Reader
A parser for the website MSPA that will download comic content into a database for local, forms based, reading. Written in pure .NET.

Simple build the Release configuration with visual studio, or the MonoRelease configuration in mono. Both will require nuget package restore (Nuget is weird on my Arch Linux. I had to build using monodevelop the first time, but then I could use xbuild). You can safely ignore the missing references.

Reccommend using SQLLocalDB or SQLServer if possible. Sqlite has shown to run into deadlocks during archive operations. Support for MySQL coming soon.

Requirements:

.NET Framework 4.5/Mono equivalent: https://www.microsoft.com/en-ca/download/details.aspx?id=30653
Flashplayer browser plugin: https://get.adobe.com/flashplayer/

Other:

SQLLocalDB: https://www.microsoft.com/en-ca/download/details.aspx?id=29062 Choose SQLLocalDB.MSI for your architecture
SQL Server Express: https://www.microsoft.com/en-ca/download/details.aspx?id=42299

If using SQL Server you will need to setup a database named MSPAArchive and assign a user DBO permissions. Using only the server name paramater will attempt to connect with Windows Authentication.

I know it's poorly coded and documented. I did this for fun over the past week.

Dependancies:
HtmlAgilityPack
System.Data.Sqlite (on Windows)

Roxygen Karezi for endgame ships.