namespace Dbarone.Net.Database.Tests;
using System.Reflection;
using System.Diagnostics;

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
}