namespace DecSm.Atom.Tools.Nuget;

internal class CancellationTokenValueSource : IValueDescriptor<CancellationToken>, IValueSource
{
    public object GetDefaultValue() =>
        throw new NotImplementedException();

    public string ValueName => throw new NotImplementedException();

    public Type ValueType => throw new NotImplementedException();

    public bool HasDefaultValue => throw new NotImplementedException();

    public bool TryGetValue(IValueDescriptor valueDescriptor, BindingContext bindingContext, out object? boundValue)
    {
        boundValue = (CancellationToken)bindingContext.GetService(typeof(CancellationToken))!;

        return true;
    }
}
