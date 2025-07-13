using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dbarone.Net.Extensions;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Builds the mapping graph between 2 types.
/// </summary>
public class MapperBuilder
{
    /// <summary>
    /// The input configuration for the map.
    /// </summary>
    internal MapperConfiguration Configuration { get; init; }

    /// <summary>
    /// Build-time type information.
    /// </summary>
    internal Dictionary<Type, BuildType> BuildTypes { get; set; } = new Dictionary<Type, BuildType>();

    #region Callbacks

    /// <summary>
    /// Optional logging callback.
    /// </summary>
    public MapperOperatorLogDelegate? OnLog = null;

    #endregion

    /// <summary>
    /// Creates a new <see cref="MapperBuilder"/> instance.
    /// </summary>
    /// <param name="configuration">The configuration used to create mapper objects.</param>
    public MapperBuilder(MapperConfiguration configuration)
    {
        this.Configuration = configuration;
        AddCoreResolvers();
    }

    #region Public Methods

    /// <summary>
    /// Returns boolean denoting whether a source-target mapping exists.
    /// </summary>
    /// <param name="sourceTarget">The source and target types.</param>
    /// <param name="parent">An optional parent operator.</param>
    /// <returns>Returns true if a mapper operator exists between source and target types.</returns>
    public bool CanMap(SourceTarget sourceTarget, MapperOperator? parent = null)
    {
        MapperOperator? mapperOperator = null;

        // source
        if (!this.BuildTypes.ContainsKey(sourceTarget.Source))
        {
            this.BuildType(sourceTarget.Source);
        }
        var sourceBuild = this.BuildTypes[sourceTarget.Source];

        // target
        if (!this.BuildTypes.ContainsKey(sourceTarget.Target))
        {
            this.BuildType(sourceTarget.Target);
        }
        var targetBuild = this.BuildTypes[sourceTarget.Target];

        // find mapper to handle source-target
        return MapperOperator.CanMap(this, sourceBuild, targetBuild, parent, this.OnLog);
    }

    /// <summary>
    /// Gets a mapper operator based on source and target types. Note this ONLY gets the basic operator that can
    /// be identified during build time. Operators that use dynamic / defer logic will get complete mapping
    /// information only during run (map) time.
    /// </summary>
    /// <param name="sourceTarget">The source and target types.</param>
    /// <param name="parent">An optional parent operator.</param>
    /// <returns>A mapper operator able to map source and target types.</returns>
    /// <exception cref="Exception">Throws an exception if no valid operator found.</exception>
    public MapperOperator GetMapperOperator(SourceTarget sourceTarget, MapperOperator? parent = null)
    {
        MapperOperator? mapperOperator = null;

        // source
        if (!this.BuildTypes.ContainsKey(sourceTarget.Source))
        {
            this.BuildType(sourceTarget.Source);
        }
        var sourceBuild = this.BuildTypes[sourceTarget.Source];

        // target
        if (!this.BuildTypes.ContainsKey(sourceTarget.Target))
        {
            this.BuildType(sourceTarget.Target);
        }
        var targetBuild = this.BuildTypes[sourceTarget.Target];

        // find mapper to handle source-target
        return MapperOperator.Create(this, sourceBuild, targetBuild, parent, this.OnLog);
    }

    /// <summary>
    /// Pre-emptively builds all type information provided by the configuration.
    /// </summary>
    public void Build()
    {
        foreach (var item in this.Configuration.Config.Types.Keys)
        {
            BuildType(item);
        }
    }

    /// <summary>
    /// Gets the build metadata for a single type.
    /// </summary>
    /// <param name="type">The type to get build information for.</param>
    /// <returns>Returns the build information.</returns>
    /// <exception cref="Exception">Throws an exception if the build type is not found.</exception>
    public BuildType GetBuildTypeFor(Type type)
    {
        if (!BuildTypes.ContainsKey(type))
        {
            throw new Exception("Type not found!");
        }
        return BuildTypes[type];
    }

    /// <summary>
    /// Returns the member resolver for the type given the current configuration.
    /// </summary>
    /// <param name="type">The type to get the resolver for.</param>
    /// <returns>The member resolver for the type.</returns>
    /// <exception cref="MapperBuildException">Throws an exception if no resolver found for the type.</exception>
    public IMemberResolver GetResolver(Type type)
    {
        if (!this.Configuration.Config.Types.ContainsKey(type) && this.Configuration.Config.AutoRegisterTypes)
        {
            this.Configuration.RegisterType(type);
        }

        var configType = this.Configuration.Config.Types[type];

        if (configType == null)
        {
            throw new MapperBuildException(type, MapperEndPoint.None, null, $"Type: {type.Name} not registered in configuration.");
        }

        // Get resolver
        var resolvers = this.Configuration.Config.Resolvers.Union(this.Configuration.Config.DefaultResolvers);
        foreach (var configResolver in resolvers)
        {
            if (configResolver.CanResolveMembersForType(configType.Type))
            {
                return configResolver;
            }
        }
        throw new MapperBuildException(type, MapperEndPoint.None, string.Empty, $"No resolver found for type: {type.Name}.");
    }

    #endregion

    /// <summary>
    /// Returns the creator delegate for a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Returns a delegate that when invoked will create an instance of the object.</returns>
    internal CreateInstance GetCreatorFor(Type type)
    {
        if (!BuildTypes.ContainsKey(type))
        {

        }
        return BuildTypes[type].MemberResolver.CreateInstance(type, new object[] { });
    }

    #region Private Core Build Methods

    private void BuildType(Type type)
    {
        IMemberResolver? resolver = this.GetResolver(type);
        var configType = this.Configuration.Config.Types[type];

        BuildType buildType = new BuildType
        {
            Type = configType.Type,
            Options = configType.Options,
            MemberResolver = resolver,
        };

        // Add to metadata
        this.BuildTypes[type] = buildType;

        if (!buildType.isOpenGeneric)
        {
            // Add members
            if (resolver.HasMembers)
            {
                AddMembers(type);
            }

            // Add calculations
            AddCalculations(type);

            // Validate Type
            ValidateType(buildType);
        }
    }

    /// <summary>
    /// Adds members to a dynamic type at runtime.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    internal void AddDynamicMembers(Type type, string path, object? obj)
    {
        List<MapperBuildError> errors = new List<MapperBuildError>();
        var buildType = this.GetBuildTypeFor(type);

        var members = buildType.MemberResolver.GetInstanceMembers(obj);

        List<BuildMember> buildMembers = new List<BuildMember>();
        foreach (var member in members)
        {
            var dataType = buildType.MemberResolver.GetMemberType(type, member, buildType.Options);
            var getter = buildType.MemberResolver.GetGetter(type, member, buildType.Options);
            var setter = buildType.MemberResolver.GetSetter(type, member, buildType.Options);
            var ignore = GetMemberInclusionStatus(type, member);
            var internalName = GetInternalName(type, member, buildType.Options.MemberRenameStrategy);

            // validations
            if (dataType == null)
            {
                errors.Add(new MapperBuildError(type, MapperEndPoint.None, member, "Data type not known for member."));
            }
            else if (getter == null)
            {
                errors.Add(new MapperBuildError(type, MapperEndPoint.None, member, "No getter for member."));
            }
            else if (setter == null)
            {
                errors.Add(new MapperBuildError(type, MapperEndPoint.None, member, "No setter for member."));
            }
            else
            {
                // add member to build
                buildMembers.Add(new BuildMember
                {
                    MemberName = member,
                    DataType = dataType,
                    Getter = getter,
                    Setter = setter,
                    Ignore = ignore,
                    InternalMemberName = internalName
                });
            }
        }
        if (errors.Any())
        {
            throw new MapperBuildException(errors);
        }
        buildType.Members = buildMembers;
    }

    private MethodInfo? GetImplicitCast(Type fromType, Type toType)
    {
        var method = fromType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(
                            m => m.ReturnType == toType &&
                            m.Name == "op_Implicit"
                        );
        return method;
    }


    #endregion

    #region Helper Methods
    private void AddCoreResolvers()
    {
    }

    private void ValidateType(BuildType buildType)
    {
        List<MapperBuildError> errors = new List<MapperBuildError>();

        // Check no duplicate internal names
        var duplicates = buildType.Members
            .GroupBy(g => g.InternalMemberName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key).ToList();

        foreach (var duplicate in duplicates)
        {
            var members = buildType.Members.Where(m => m.InternalMemberName == duplicate);
            foreach (var member in members)
            {
                errors.Add(new MapperBuildError(buildType.Type, MapperEndPoint.None, member.MemberName, "Member internal name not unique."));
            }
        }

        if (errors.Any())
        {
            throw new MapperBuildException(errors);
        }
    }

    private void AddMembers(Type type)
    {
        var buildType = this.GetBuildTypeFor(type);
        var resolver = buildType.MemberResolver;
        var configType = this.Configuration.Config.Types[type];

        // Get members
        string[] members = new string[] { };
        List<BuildMember> buildMembers = new List<BuildMember>();
        if (!resolver.DeferBuild)
        {
            members = resolver.GetTypeMembers(configType.Type, configType.Options);

            foreach (var member in members)
            {
                var dataType = resolver.GetMemberType(configType.Type, member, configType.Options);
                var getter = resolver.GetGetter(configType.Type, member, configType.Options);
                var setter = resolver.GetSetter(configType.Type, member, configType.Options);
                var ignore = GetMemberInclusionStatus(configType.Type, member);
                var internalName = GetInternalName(configType.Type, member, configType.Options.MemberRenameStrategy);

                // validations
                if (dataType == null)
                {
                    throw new MapperBuildException(type, MapperEndPoint.None, member, "Data type not known for member.");
                }
                else
                {
                    // add member to build
                    buildMembers.Add(new BuildMember
                    {
                        MemberName = member,
                        DataType = dataType,
                        Getter = getter,
                        Setter = setter,
                        Ignore = ignore,
                        InternalMemberName = internalName
                    });
                }
            }
        }
        buildType.Members = buildMembers;
    }

    private bool GetMemberInclusionStatus(Type type, string member)
    {
        MemberFilterDelegate? memberFilterRuleA = null;
        MemberFilterDelegate? memberFilterRuleB = null;
        MemberFilterDelegate? memberFilterRule = null;

        if (!Configuration.Config.Types.ContainsKey(type))
        {
            throw new Exception($"GetIgnoreStatus. Invalid type: {type}.");
        }

        // Get member filter function if exists
        if (Configuration.Config.MemberFilters.ContainsKey(type))
        {
            memberFilterRuleA = Configuration.Config.MemberFilters[type];
        }

        memberFilterRuleB = Configuration.Config.Types[type].Options.MemberFilterRule;
        memberFilterRule = memberFilterRuleA != null ? memberFilterRuleA : memberFilterRuleB;

        var isIncluded = (memberFilterRule != null) ? memberFilterRule(member) : false;
        foreach (var configMemberInclusion in Configuration.Config.MemberInclusions.Where(c => c.Type == type && c.Member.Equals(member, StringComparison.Ordinal)))
        {
            isIncluded = configMemberInclusion.IncludeExclude == IncludeExclude.Include ? true : false;
        }
        return isIncluded;
    }

    private string GetInternalName(Type type, string memberName, IMemberRenameStrategy? memberRenameStrategy = null)
    {
        var internalMemberName = memberName;

        // rename strategy present?
        if (memberRenameStrategy != null)
        {

            internalMemberName = memberRenameStrategy!.RenameMember(memberName);
        }

        // loop through any specific rename rules. Last one wins. 
        foreach (var rename in Configuration.Config.MemberRenames.Where(c => c.Type == type && c.MemberName.Equals(memberName, StringComparison.Ordinal)))
        {
            internalMemberName = rename.InternalMemberName;
        }
        return internalMemberName;
    }

    private void AddCalculations(Type type)
    {
        // Add calculations to existing type
        foreach (var calculation in this.Configuration.Config.Calculations.Where(c => c.SourceType == type))
        {
            var buildType = BuildTypes[type];

            buildType.Members.Add(new BuildMember
            {
                MemberName = calculation.MemberName,
                DataType = calculation.MemberType,
                InternalMemberName = calculation.MemberName,
                Ignore = false,
                Getter = calculation.Calculation.Convert,
                Setter = null,
                Calculation = calculation.Calculation
            });
        }
    }

    #endregion

}