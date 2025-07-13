using Dbarone.Net.Extensions;
using Dbarone.Net.Database.Mapper;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Operator for mapping classes and structs with members.
/// </summary>
public class MemberwiseMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new <see cref="MemberwiseMapperOperator"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public MemberwiseMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog)
    {

    }

    /// <summary>
    /// GetChildren implementation for <see cref="MemberwiseMapperOperator"/>.
    /// </summary>
    /// <returns>Returns the children operators.</returns>
    /// <exception cref="MapperBuildException"></exception>
    protected override IDictionary<string, MapperOperator> GetChildren()
    {
        Dictionary<string, MapperOperator> children = new Dictionary<string, MapperOperator>();

        // member-wise mapping
        // Get internal member names matching on source + target
        IEnumerable<string> members;

        if (TargetType.MemberResolver.DeferBuild)
        {
            members = SourceType
                    .Members
                    .Where(mc => mc.Ignore == false)
                    .Select(mc => mc.InternalMemberName);

        }
        else
        {
            members = TargetType
                    .Members
                    .Where(mc => mc.Ignore == false)
                    .Select(mc => mc.InternalMemberName).Intersect(
                        SourceType
                        .Members
                        .Where(mc => mc.Ignore == false)
                        .Select(mc => mc.InternalMemberName)
                    );
        }

        foreach (var member in members)
        {
            var mSourceType = SourceType.Members.First(m => m.MemberName == member).DataType;
            Type mTargetType;

            if (TargetType.MemberResolver.DeferBuild)
            {
                mTargetType = TargetType.MemberResolver.GetMemberType(TargetType.Type, member, TargetType.Options);
            }
            else
            {
                mTargetType = TargetType.Members.First(m => m.MemberName == member).DataType;
            }

            var memberMapperOperator = Builder.GetMapperOperator(new SourceTarget(mSourceType, mTargetType), this);
            children[member] = memberMapperOperator;
        }
        return children;
    }

    /// <summary>
    /// The <see cref="MemberwiseMapperOperator"/> operator is able to map when the source and target types have members, and the source type is a defer build type. 
    /// </summary>
    /// <returns>Returns true when the source and target types have members, and the source type is a defer build type.</returns>
    public override bool CanMap()
    {
        return SourceType.MemberResolver.HasMembers && TargetType.MemberResolver.HasMembers;
    }

    private void EndPointValidation()
    {
        List<MapperBuildError> errors = new List<MapperBuildError>();
        if ((SourceType.Options.EndPointValidation & MapperEndPoint.Source) == MapperEndPoint.Source)
        {
            // check all source member rules map to target rules.
            var unmappedSourceMembers = SourceType
                .Members
                .Where(m => TargetType
                    .Members
                    .Select(d => d.InternalMemberName).Contains(m.InternalMemberName) == false);

            foreach (var item in unmappedSourceMembers)
            {
                errors.Add(new MapperBuildError(SourceType.Type, MapperEndPoint.Source, item.MemberName, "Source end point validation enabled, but source member is not mapped to target."));
            }
        }

        if ((TargetType.Options.EndPointValidation & MapperEndPoint.Target) == MapperEndPoint.Target)
        {
            // check all source member rules map to target rules.
            var unmappedTargetMembers = TargetType
                .Members
                .Where(m => SourceType
                    .Members
                    .Select(d => d.InternalMemberName).Contains(m.InternalMemberName) == false);

            foreach (var item in unmappedTargetMembers)
            {
                errors.Add(new MapperBuildError(TargetType.Type, MapperEndPoint.Target, item.MemberName, "Target end point validation enabled, but target member is not mapped from source."));
            }
        }
        if (errors.Any())
        {
            throw new MapperBuildException("Error occurred during end point validation. See inner errors collection for more information.", errors);
        }
    }

    /// <summary>
    /// Mapping implementation for <see cref="MemberwiseMapperOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        EndPointValidation();

        var creator = TargetType.MemberResolver.CreateInstance(TargetType.Type, null);
        var instance = creator();

        foreach (var member in Children.Keys)
        {
            var memberFrom = SourceType.MemberResolver.GetGetter(SourceType.Type, member, SourceType.Options)(source);
            var memberOperator = this.Children[member];
            var memberMapped = memberOperator.Map(memberFrom);
            TargetType.MemberResolver.GetSetter(TargetType.Type, member, SourceType.Options)(instance, memberMapped);
        }
        return instance;
    }
}