namespace DecSm.Atom.Setup;

public class AtomConfigurator(IHostApplicationBuilder builder) : IAtomConfigurator
{
    public IHostApplicationBuilder Builder => builder;
}