namespace DecSm.Atom.Build.Definition;

#pragma warning disable CS9113 // Parameter is unread.

[AttributeUsage(AttributeTargets.Interface)]
public sealed class HelperAttribute(params Type[] types) : Attribute;

#pragma warning restore CS9113 // Parameter is unread.