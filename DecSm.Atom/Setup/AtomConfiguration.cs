namespace DecSm.Atom.Setup;

public class AtomConfiguration(IHostApplicationBuilder builder) : IAtomConfiguration
{
    public IHostApplicationBuilder Builder => builder;
}