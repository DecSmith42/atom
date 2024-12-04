namespace DecSm.Atom.Util;

public static class TypeUtils
{
    public static T? Convert<T>(string? stringValue, Func<string?, T?>? converter) =>
        stringValue is null
            ? default
            : converter is not null
                ? converter(stringValue)
                : typeof(T).IsArray
                    ? ConvertArray<T>(stringValue)
                    : typeof(T).IsGenericType
                        ? ConvertGeneric<T>(stringValue, typeof(T))
                        : Convert(stringValue, typeof(T)) is T tValue
                            ? tValue
                            : default;

    private static object? Convert(string value, Type type) =>
        TypeDescriptor
            .GetConverter(type)
            .ConvertFromInvariantString(value);

    private static T ConvertArray<T>(string value)
    {
        var elementType = typeof(T).GetElementType()!;
        var values = value.Split(',');

        var array = Array.CreateInstance(elementType, values.Length);

        for (var i = 0; i < values.Length; i++)
            array.SetValue(Convert(values[i], elementType), i);

        return (T)(object)array;
    }

    private static T ConvertGeneric<T>(string value, Type type)
    {
        var genericType = type.GetGenericTypeDefinition();

        var genericArgument = type
            .GetGenericArguments()[0];

        if (genericType == typeof(IReadOnlyList<>))
        {
            var values = value.Split(',');
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArgument))!;

            foreach (var item in values)
                list.Add(Convert(item, genericArgument));

            return (T)list;
        }

        throw new NotSupportedException($"Generic type '{type}' is not supported.");
    }
}
