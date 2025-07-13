using Dbarone.Net.Database;
using Dbarone.Net.Database.Mapper;

public class TableResolver : AbstractMemberResolver, IMemberResolver
{
    public override bool HasMembers => false;

    public override bool IsEnumerable => true;

    public override bool DeferBuild => false;

    public override bool CanResolveMembersForType(Type type)
    {
        return type == typeof(Table);
    }
}