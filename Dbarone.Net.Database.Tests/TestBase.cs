namespace Dbarone.Net.Database.Tests;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;

public class TestBase
{
    protected string GetDatabaseFileNameFromMethod()
    {
        // Gets the method name, and returns a database file name based off that
        var st = new StackTrace();
        StackFrame? sf = st.GetFrame(1);
        if (sf != null)
        {
            MethodBase? method = sf.GetMethod();
            if (method != null)
            {
                var caller = method.Name;
                return $"Test_{caller}.db";
            }
        }
        throw new System.Exception("Unable to get database filename from calling method.");
    }

    protected IEngine CreateDatabase(string filename)
    {
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }
        return Engine.Create(filename);
    }

    protected TableInfo CreateTable(IEngine db, Type tableType)
    {
        var tableName = tableType.Name;
        var columns = GetColumnsForType(tableType);
        return db.CreateTable(tableName, columns);
    }

    protected TableInfo CreateTableFromEntity<T>(IEngine db) {
        var tableName = typeof(T).Name;
        return db.CreateTable<T>(tableName);
    }

    protected IEnumerable<ColumnInfo> GetColumnsForType(Type type){
        var cols = new List<ColumnInfo>();
        var props = type.GetProperties();
        foreach (var prop in props){
            var col = new ColumnInfo(prop.Name, prop.PropertyType);
            cols.Add(col);
        }
        return cols;
    }
}