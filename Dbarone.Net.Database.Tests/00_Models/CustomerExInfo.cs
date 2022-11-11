namespace Dbarone.Net.Database;
using System;

public class CustomerExInfo
{
    public int CustomerId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string CustomerName { get; set; } = default!;
    public bool IsActive { get; set; }
    public CustomerExInfo(int customerId, DateTime dateOfBirth, string customerName, bool isActive)
    {
        this.CustomerId = customerId;
        this.DateOfBirth = dateOfBirth;
        this.CustomerName = customerName;
        this.IsActive = isActive;
    }
    public CustomerExInfo()
    {

    }
}