using Dbarone.Net.Database;
using Xunit;

public class PageTests
{

    [Fact]
    public void TestHeadersGetSet()
    {
        var page = new Page();
        page.InitialiseHeader();
        page.TableCount = 2;
        page.PageId = 123;
        page.NextPageId = 456;
        page.PageType = PageType.Empty;

        Assert.Equal(2, page.TableCount);
        Assert.Equal(123, page.PageId);
        Assert.Equal(456, page.NextPageId);
        Assert.Equal(PageType.Empty, page.PageType);
    }
}