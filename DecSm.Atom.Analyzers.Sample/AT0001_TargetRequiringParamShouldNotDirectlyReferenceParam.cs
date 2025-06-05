using DecSm.Atom.Build;
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Params;

namespace DecSm.Atom.Analyzers.Sample;

public interface IMyTarget : IBuildAccessor
{
    [ParamDefinition("my-param-1", "My Param 1")]
    string MyParam1 => GetParam(() => MyParam1)!;

    [ParamDefinition("my-param-2", "My Param 2")]
    string MyParam2 => GetParam(() => MyParam2)!;

    string NotParam1 { get; }

    string NotParam2 { get; }

    Target MyTarget =>
        t => t
            .RequiresParam(MyParam1)
            .RequiresParam(NotParam1)
            .RequiresParam(nameof(MyParam2));

    Target MyTarget2 =>
        t => t
            .RequiresParam(NotParam1)
            .RequiresParam(MyParam1)
            .RequiresParam(NotParam2);
}
