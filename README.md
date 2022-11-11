# Dbarone.Net.Database
A NoSQL database written in .NET.

This is a simple database engine that can be used for very basic data storage use-cases (for example configurations, small prototype applications etc). It is written entirely in .NET Core, and should be portable to any system that can run .NET Core.

*NOTE: This project is still in very early development, and is not useable yet.*

## Basics

Data is stored in a single file. There are no rules about the naming of the file. All data and metadata is stored within the file in 8K units called data pages.

## Data Page

A data page is the foundation unit of the database - a database file comprises of a number of data pages, and each data page is 8KB (8192 bytes) in size. This default page size can be changed if required. For example, the .NET FileRead buffer default size is 4K. All data pages have a similar structure:

| Section     | Size       | Purpose                                        |
| ----------- | ---------- | ---------------------------------------------- |
| Page Header | 96 bytes   | Header information for the current page.       |
| Page Data   | 8096 bytes | Data contained in the page (multiple records). |


Disk IO is performed at a data page level. Disk IO is also buffered to improve performance. If the database engine needs to read a single row from a table, it will read the entire page that the row is stored in.

### Header

The header section stores key parameters for the current page. The parameters vary depending on the page type. However, all pages share some basic header parameters:

| Name          | Type     | Purpose                                                                     |
| ------------- | -------- | --------------------------------------------------------------------------- |
| PageType      | PageType | The type of the page.                                                       |
| PageId        | int      | The id / position of the current page.                                      |
| PrevPageId    | int      | The page id of the previous related page in a doubly-linked list.           |
| NextPageId    | int      | The page id of the next related page in a doubly-linked list.               |
| SlotsUsed     | int      | The number of rows / slots used in the current data page.                   |
| TransactionId | int      | The transaction id that made changes to the page.                           |
| IsDirty       | boolean  | Set to true if the page has been modified, and needs to be flushed to disk. |
| FreeOffset    | int      | The next free offset in the page data section.                              |
| Lsn???        | int      | Last transaction / log record that changed the page?                        |

#### Page Types
There are a number of page types:

| Page Type    | Purpose                                                                                    |
| ------------ | ------------------------------------------------------------------------------------------ |
| Boot         | The database header page. Always page id 0. Only 1 per database.                           |
| SystemTable  | Stores table metadata. Starts at page id 1. Can span multiple pages.                       |
| SystemColumn | Stores column metadata. At least 1 per table. Can span multiple pages.                     |
| Data         | Stores row data for a table. At least 1 per table. Can span multiple pages.                |
| Index        | Stores index data for a table. At least 1 per table. Can span multiple pages.              |
| Text         | Stores large text (CLOB) data for a table. At least 1 per table. Can span multiple pages.  |
| LOB          | Stores large object data (LOB) for a table. At least 1 per table. Can span multiple pages. |
| Overflow     | Stores CLOB and BLOB data that cannot fit onto a single page.                              |

### Data Section

The page data section can be broken down further:

| Section    | Size                | Purpose                                                                                                                                 |
| ---------- | ------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| Data Rows  | (variable length)   | Data is written as variable-length buffers starting at the top of the page data section.                                                |
| Free Area  |                     | Remaining free area where data rows can be written.                                                                                     |
| Slot Array | 2 bytes * row count | 2 byte offset pointing to the start of each row in the Page Data section. The slot array grows in reverse order. Also known as 'slots'. |

The slot array table allows for variable length data to be stored in each data row.

Rules around adding data rows to page data section:
- Rows are always added to data pages (except index page types) in sequential order.
- Each row has implicit `rowid` value which is the global position of the row.
- rowid is only changed during a database rebuild process

### Buffering

Performing disk IO for every read and write operation on a database would result in incredibly poor performance. For this reason, page buffering is performed. Basically, when any data is read or written, the database engine will read in page chunks. Once a page is read it will be cached so that subsequent reads will happen from memory. When a write operation (insert, updated, delete) occurs, it is done to the page in memory first. The data is periodically flushed to the disk via a `CheckPoint` operation, which can be executed manually, and also occurs periodically and automatically from within the engine.

Disk IO buffering is managed using the `BufferManager` class:
- When a page is read from disk, it is cached. Subsequent accesses of the page are read from the memory cache.
- When a page is modified in memory, an 'IsDirty' flag is set on the page.
- Periodically, and when the `CheckPoint` command is manually executed, all dirty pages are written back to disk.

## Serialisation

In order to move data from disk to memory and vice versa, it must be serialised and deserialised. Serialisation is the process of converting data structures and objects to byte streams. Deserialisation is the opposite process. A custom serialiser has been implemented to perform both tasks. The serialiser is able to serialise any object, and stores the serialised output in the following format:

| Field                 | Size (bytes)              | Row Overhead | Description                                                             |
| --------------------- | ------------------------- | ------------ | ----------------------------------------------------------------------- |
| Buffer Length         | 4                         | Yes          | The total size of the serialised output including row overhead.         |
| Row Status            | 1                         | Yes          | Various row status flags                                                |
| Fixed Column Count    | 1                         | Yes          | The number of fixed length columns in the object.                       |
| Variable Column Count | 1                         | Yes          | The number of variable length columns in the object.                    |
| Null Bitmap           | Total Column Count / 8    | Yes          | Stores 1 bit for every column. bit set to true if column value is null. |
| Fixed Data Length     | 2                         | Yes          | The size of fixed length data.                                          |
| Fixed Data            | n                         | No           | The fixed length data.                                                  |
| Variable Length Table | 4 * variable column count | Yes          | Table of Int16 values denoting length of each variable length field.    |
| Variable Data         | n                         | No           | The variable length data                                                |

In order to serialise or deserialise, the column information (`IEnumerable<ColumnInfo>`) must also be provided to the Serialiser class. The serialised data does not include column names, so this must be stored and provided separated in order for the serialisation and deserialisation to occur.

## Data Types

The following data types are supported in Dbarone.Net.Database, each mapping to a .NET type:

### Fixed Length Data Types:

| Type     | .NET Type | Size |
| -------- | --------- | ---- |
| Boolean  | bool      | 1    |
| Byte     | byte      | 1    |
| SByte    | sbyte     | 1    |
| Char     | char      | 1    |
| Decimal  | Decimal   | 16   |
| Double   | double    | 8    |
| Single   | float     | 4    |
| Int16    | Int16     | 2    |
| UInt16   | UInt16    | 2    |
| Int32    | Int32     | 4    |
| UInt32   | UInt32    | 4    |
| Int64    | Int64     | 8    |
| UInt64   | UInt64    | 8    |
| Guid     | Guid      | 16   |
| DateTime | DateTime  | 8    |

## Variable Length Data Types

| Type   | .NET Core Type |
| ------ | -------------- |
| string | string         |
| Blob   | byte[]         |

## LOB Data

In most cases, a row must fit inside a single page (TODO: what are specific row limits?). However, a special page type called an `OverFlowPage` allows for large string data to be stored. If a large string value is stored, then the original page stores a special `OverflowPointer` record to the starting page number where the actual data is stored. From there multiple pages can be doubly-linked to store the entire large string value.

## Database Rebuild

A database rebuild rebuilds the entire data file. A database rebuild includes the following:
- Checks all data pages are reachable and linked correctly
- Checks the internal integrity of all pages
- Physically removes any logically deleted data rows
- Reassigns new rowid to all rows (due to removal of logically deleted rows.)
- Rebuilds all pages, compacts, removes any unused pages
- Rebuilds all indexes
- Rows are never reordered in a data page
- If row cannot be added to current page, a new page is created, and it is added there
- Index Pages
  - Index rows can be inserted in the middle of a page
  - If a page becomes full, it is split into 2 pages - half onto each page.


Delete row in page - NOT compacted immediately - INSERTS deal with that
SLot array is ordered - gets reshuffled as records are inserted + deleted
1st slot entry should point to 1st record in page

## Research SQL Queries

### Viewing All Pages in a SQL Server Database

``` sql
/*
DBCC IND ( { 'dbname' | dbid }, { 'objname' | objid }, { nonclustered indid | 1 | 0 | -1 | -2 });
nonclustered indid = non-clustered Index ID
1 = Clustered Index ID
0 = Displays information in-row data pages and in-row IAM pages (from Heap)
-1 = Displays information for all pages of all indexes including LOB (Large object binary) pages and row-overflow pages
-2 = Displays information for all IAM pages
*/
DBCC IND('mydatabase','mytable', -1)
```

### Viewing a Single Page in a SQL Server Database

``` sql
/*
dbcc page ( {'dbname' | dbid}, filenum, pagenum [, printopt={0|1|2|3} ]);Printopt:
0 - print just the page header
1 - page header plus per-row hex dumps and a dump of the page slot array 
2 - page header plus whole page hex dump
3 - page header plus detailed per-row interpretation
*/
DBCC TRACEON(3604)	-- By default, output of DBCC TRACE sent to error log. 
GO
DBCC page('mydatabase',1,9,3)
```

## Bibiography

### Architecture

- SQL Server Architecture (Guru99): https://www.guru99.com/sql-server-architecture.html

### Data Pages
- How SQL Server stores data in data pages (c-sharpcorner): https://www.c-sharpcorner.com/UploadFile/ff0d0f/how-sql-server-stores-data-in-data-pages-part-1/
- Reading SQL Server File Header Page (sqlity.net): https://sqlity.net/en/2414/dbcc-fileheader/#:~:text=Every%20database%20file%20contains%20a%20single%20page%20that,good%20way%20to%20dive%20into%20the%20page%27s%20content%3A
- Fixing boot page corruption (technet.microsoft): https://social.technet.microsoft.com/wiki/contents/articles/53223.sql-server-understanding-and-fixing-boot-page-corruption.aspx
- Anatomy of a data page (sqlskills): https://www.sqlskills.com/blogs/paul/inside-the-storage-engine-anatomy-of-a-page/
- Data page internals (etutorials): https://etutorials.org/SQL/microsoft+sql+server+2000/Part+V+SQL+Server+Internals+and+Performance+Tuning/Chapter+33.+SQL+Server+Internals/Database+Pages/
- Data page types: https://www.dabrowski.space/posts/sql-server-page-types-list/
- Page & Architecture Guide (learn.microsoft): https://learn.microsoft.com/en-us/sql/relational-databases/pages-and-extents-architecture-guide?view=sql-server-ver16
- Understanding data page structure (sqlservercentral): https://www.sqlservercentral.com/blogs/sql-server-understanding-the-data-page-structure
- Storage engine anatomy of a page (sqlskills): https://www.sqlskills.com/blogs/paul/inside-the-storage-engine-anatomy-of-a-page/
- Storage engine anatomy of a record (sqlskills): https://www.sqlskills.com/blogs/paul/inside-the-storage-engine-anatomy-of-a-record/

### Row Overflow
- https://sqlity.net/en/1051/blob-and-row-overflow-storage-internals-row-overflow-data/

## Naming Conventions

Naming conventions
- Alpha-numeric
- Alpha first character
- Case Insensitive
- 128 characters max
- Keywords (cannot be used as column name)
  - rowid