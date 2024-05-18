namespace DecSm.Atom;

public class AtomConfigurator(IHostApplicationBuilder builder) : IAtomConfigurator
{
    public IHostApplicationBuilder Builder => builder;
}