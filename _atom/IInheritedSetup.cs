namespace Atom;

// TODO: Enforce extensions before other things? Allow specific order? Or enforce order regardless of definition order?
[TargetDefinition]
public partial interface IInheritedSetup : ISetup
{
    new Target Setup =>
        d => d
            .Extends<ISetup>(x => x.Setup)
            .Extends<IOtherThing>(x => x.DoOtherThing)
            .Extends<IOtherThing>(x => x.DoOtherThing)
            .Executes(() =>
            {
                Services
                    .GetRequiredService<ILogger<ISetup>>()
                    .LogInformation("Inherited setup");

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface IOtherThing
{
    Target DoOtherThing =>
        d => d.Executes(() =>
        {
            Services
                .GetRequiredService<ILogger<ISetup>>()
                .LogInformation("Doing other thing");

            return Task.CompletedTask;
        });
}