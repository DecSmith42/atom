namespace DecSm.Atom.Hosting;

public interface IAtomConfiguration
{
    IHostApplicationBuilder Builder { get; }
}

public class AtomConfiguration(IHostApplicationBuilder builder) : IAtomConfiguration
{
    public IHostApplicationBuilder Builder => builder;
}