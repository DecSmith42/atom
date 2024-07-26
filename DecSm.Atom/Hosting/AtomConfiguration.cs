namespace DecSm.Atom.Hosting;

public class AtomConfiguration(IHostApplicationBuilder builder) : IAtomConfiguration
{
    public IHostApplicationBuilder Builder => builder;
}