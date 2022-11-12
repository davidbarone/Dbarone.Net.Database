namespace Dbarone.Net.Database;
using System;

public class PersonInfo
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = default!;
    public string Comments { get; set; } = default!;
    public DateTime DoB { get; set; }

    public PersonInfo(int personId, string personName, string comments, DateTime dob)
    {
        this.PersonId = personId;
        this.PersonName = personName;
        this.Comments = comments;
        this.DoB = dob;
    }

    public PersonInfo() { }

    public static PersonInfo[] CreateTestData()
    {
        var people = new PersonInfo[] {
                new PersonInfo(0, "Fred", "XXX", DateTime.Today),
                new PersonInfo(1, "John", "XXX", DateTime.Today),
                new PersonInfo(2, "Tony", "XXX", DateTime.Today),
                new PersonInfo(3, "Ian", "YYY", DateTime.Today),
                new PersonInfo(4, "Paul", "YYY", DateTime.Today),
                new PersonInfo(5, "Stuart", "YYY", DateTime.Today),
                new PersonInfo(6, "Colin", "ZZZ", DateTime.Today),
                new PersonInfo(7, "Malcolm", "ZZZ", DateTime.Today),
                new PersonInfo(8, "David", "ZZZ", DateTime.Today),
                new PersonInfo(9, "Mark", "ZZZ", DateTime.Today)
            };
        return people;
    }
}
