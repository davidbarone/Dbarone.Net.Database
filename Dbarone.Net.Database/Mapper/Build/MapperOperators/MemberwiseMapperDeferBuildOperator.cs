using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Extensions;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Mapper operator that is able to map class and struct types on a member level, where the source type has a DeferBuild set to True (i.e. Dictionary and Dynamic types).
/// The operator build occurs when the first object is mapped. It is assumed that all subsequent objects have the same schema as the first object.
/// </summary>
public class MemberwiseMapperDeferBuildOperator : MapperOperator
{
    private List<BuildMember> runTimeMembers { get; set; }

    /// <summary>
    /// Creates a new <see cref="MemberwiseMapperDeferBuildOperator"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public MemberwiseMapperDeferBuildOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog)
    {
    }

    /// <summary>
    /// GetChildren implementation for <see cref="MemberwiseMapperDeferBuildOperator"/>.
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
            members = this.runTimeMembers
                    .Where(mc => mc.Ignore == false)
                    .Select(mc => mc.InternalMemberName);
        }
        else if (this.runTimeMembers == null)
        {
            // runTimeMembers only populated when source object instance
            // provided (e.g. during mapping process). If just getting
            // the initial mapper operator, then cannot do full check
            members = new List<string>();
        }
        else
        {
            members = TargetType
                    .Members
                    .Where(mc => mc.Ignore == false)
                    .Select(mc => mc.InternalMemberName).Intersect(
                        this.runTimeMembers
                        .Where(mc => mc.Ignore == false)
                        .Select(mc => mc.InternalMemberName)
                    );
        }

        foreach (var member in members)
        {
            var mSourceType = this.runTimeMembers.First(m => m.MemberName == member).DataType;

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
    /// The <see cref="MemberwiseMapperDeferBuildOperator"/> operator is able to map when the source and target types have members, and the source type is a defer build type. 
    /// </summary>
    /// <returns>Returns true when the source and target types have members, and the source type is a defer build type.</returns>
    public override bool CanMap()
    {
        // Note that dynamic types generally behave like object types.
        return SourceType.MemberResolver.HasMembers
        && TargetType.MemberResolver.HasMembers
        && (SourceType.MemberResolver.DeferBuild/* || SourceType.Type == typeof(object)*/);
    }

    private void EndPointValidation()
    {
        List<MapperBuildError> errors = new List<MapperBuildError>();
        if ((SourceType.Options.EndPointValidation & MapperEndPoint.Source) == MapperEndPoint.Source)
        {
            // check all source member rules map to target rules.
            var unmappedSourceMembers = this.runTimeMembers
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
                .Where(m => this.runTimeMembers
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

    private void GetRuntimeMembers(object? source)
    {
        if (this.runTimeMembers == null)
        {
            this.runTimeMembers = new List<BuildMember>();
            var members = this.SourceType.MemberResolver.GetInstanceMembers(source);
            foreach (var member in members)
            {
                var getter = this.SourceType.MemberResolver.GetGetter(this.SourceType.Type, member, this.SourceType.Options);

                var buildMember = new BuildMember
                {
                    MemberName = member,
                    InternalMemberName = member,
                    Ignore = false,
                    Getter = getter,
                    Setter = this.SourceType.MemberResolver.GetSetter(this.SourceType.Type, member, this.SourceType.Options),
                    DataType = (getter(source) is null) ? typeof(object) : getter(source)!.GetType(),
                    Calculation = null
                };
                this.runTimeMembers.Add(buildMember);
            }
        }
    }

    /// <summary>
    /// Mapping implementation for <see cref="MemberwiseMapperDeferBuildOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        GetRuntimeMembers(source);

        EndPointValidation();

        var creator = TargetType.MemberResolver.CreateInstance(TargetType.Type, null);
        var instance = creator();

        foreach (var member in Children.Keys)
        {
            var sourceGetter = SourceType.MemberResolver.GetGetter(SourceType.Type, member, SourceType.Options);
            var targetSetter = TargetType.MemberResolver.GetSetter(TargetType.Type, member, TargetType.Options);

            var memberFrom = sourceGetter(source);
            var memberOperator = this.Children[member];
            var memberMapped = memberOperator.Map(memberFrom);
            targetSetter(instance, memberMapped);
        }
        return instance;
    }
}