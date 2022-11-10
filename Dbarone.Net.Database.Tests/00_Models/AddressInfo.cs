namespace Dbarone.Net.Database;

public class AddressInfo
{
    public int AddressId { get; set; }
    public string Address1 { get; set; } = default!;
    public string Address2 { get; set; } = default!;
    public string Country { get; set; } = default!;
}