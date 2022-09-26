namespace Dbarone.Net.Database;

public class DictionaryPageData : Dictionary<string, object?>, IPageData {
    private IDictionary<string, object?> Row { get; set; }
    public DictionaryPageData(IDictionary<string, object?> row) {
        Row = row;
    }
}