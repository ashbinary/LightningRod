namespace System.Runtime.CompilerServices
{
#if !NET5_0_OR_GREATER
    internal static class IsExternalInit;
#endif

#if !NET7_0_OR_GREATER
    [AttributeUsage(
        AttributeTargets.Class
            | AttributeTargets.Field
            | AttributeTargets.Property
            | AttributeTargets.Struct,
        Inherited = false
    )]
    internal sealed class RequiredMemberAttribute : Attribute;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
    {
        public string FeatureName { get; set; } = featureName;

        public bool IsOptional { get; init; }

        public const string RefStructs = nameof(RefStructs);

        public const string RequiredMembers = nameof(RequiredMembers);
    }
#endif
}

namespace System.Diagnostics.CodeAnalysis
{
#if !NET7_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute;
#endif
}
