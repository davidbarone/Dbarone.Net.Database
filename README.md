# Dbarone.Net.Database
A NoSQL database written in .NET.

## Architecture

- https://www.guru99.com/sql-server-architecture.html
- https://sqlity.net/en/2414/dbcc-fileheader/#:~:text=Every%20database%20file%20contains%20a%20single%20page%20that,good%20way%20to%20dive%20into%20the%20page%27s%20content%3A
- https://www.c-sharpcorner.com/UploadFile/ff0d0f/how-sql-server-stores-data-in-data-pages-part-1/
- https://social.technet.microsoft.com/wiki/contents/articles/53223.sql-server-understanding-and-fixing-boot-page-corruption.aspx
- https://www.sqlskills.com/blogs/paul/inside-the-storage-engine-anatomy-of-a-page/
- https://etutorials.org/SQL/microsoft+sql+server+2000/Part+V+SQL+Server+Internals+and+Performance+Tuning/Chapter+33.+SQL+Server+Internals/Database+Pages/

## Data Page

https://www.dabrowski.space/posts/sql-server-page-types-list/

Foundation of Dbarone.Net.Database

Size: 8KB

Structure:
- Header
- Data
- RowOffset Array


https://docs.microsoft.com/en-us/sql/relational-databases/pages-and-extents-architecture-guide?view=sql-server-ver16
https://www.sqlservercentral.com/blogs/sql-server-understanding-the-data-page-structure
https://www.sqlskills.com/blogs/paul/inside-the-storage-engine-anatomy-of-a-page/
https://www.sqlskills.com/blogs/paul/inside-the-storage-engine-anatomy-of-a-record/

Default size = 8192 (change to 4K which is FileReader buffer default?)

Page HEader
Data Record(s)
Free area
Row offset (slots)
PageType
    - Data
    - Index
    - Boot (combine SQL boot (9) page + file header (0) page)
    - Text
SlotCount - number of records on page
Free - bytes free in page
Lsn - Log sequence number (last transaction / log record that changed the page)


Delete row in page - NOT compacted immediately - INSERTS deal with that
SLot array is ordered - gets reshuffled as records are inserted + deleted
1st slot entry should point to 1st record in page


/*
View all pages
--------------
DBCC IND ( { 'dbname' | dbid }, { 'objname' | objid }, { nonclustered indid | 1 | 0 | -1 | -2 });
nonclustered indid = non-clustered Index ID
1 = Clustered Index ID
0 = Displays information in-row data pages and in-row IAM pages (from Heap)
-1 = Displays information for all pages of all indexes including LOB (Large object binary) pages and row-overflow pages
-2 = Displays information for all IAM pages
*/
DBCC IND('dbarone','posts', -1)

/*
View single page
----------------
dbcc page ( {'dbname' | dbid}, filenum, pagenum [, printopt={0|1|2|3} ]);Printopt:
0 - print just the page header
1 - page header plus per-row hex dumps and a dump of the page slot array 
2 - page header plus whole page hex dump
3 - page header plus detailed per-row interpretation
*/
DBCC TRACEON(3604)	-- By default, output of DBCC TRACE sent to error log. 
GO
DBCC page('dbarone',1,9,3)



DECLARE @pageid INT
SET @pageid = 1
DBCC TRACEON(3604)	-- By default, output of DBCC TRACE sent to error log. 

while @pageid < 1000
BEGIN
	DBCC page('xxx',1,@pageid,2)
	SET @pageid = @pageid + 1
END




PageId UINT
ObjectID UINT
PageType tinyInt
NextPageId
PrevPageId


Data Pages
----------
All data and metadata is stored in Data Pages. Each page is 8K, and comprises of 2 parts:
- Page Header - Contains information about the page itself, and related pages (e.g. next page id)
- Page Data - Contain the actual data

Page Header
-----------

Page Data
---------
The Page Data section contains the actual data in the page. At the end of the page data section is a special area known as the offset or slot table. This is a table of 2-byte integers that point to the start offset of each record in the page data. This allows for variable length data to be stored in the page data area.
