namespace DecSm.Atom.Util;

/// <summary>
///     Provides comprehensive type conversion utilities for the Atom framework.
///     Enables flexible conversion of string values to various target types including
///     arrays, generic collections, and custom types with optional custom converters.
/// </summary>
/// <remarks>
///     <para>
///         This utility class is designed to handle complex type conversions that go beyond
///         standard .NET type conversion capabilities. It provides special handling for:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>Arrays - Comma-separated string values converted to typed arrays</description>
///         </item>
///         <item>
///             <description>Generic Collections - Support for IReadOnlyList&lt;T&gt; and other generic types</description>
///         </item>
///         <item>
///             <description>Custom Converters - User-defined conversion functions for specialized types</description>
///         </item>
///         <item>
///             <description>Nullable Types - Proper handling of nullable reference and value types</description>
///         </item>
///     </list>
///     <para>
///         The conversion process follows this priority order:
///     </para>
///     <list type="number">
///         <item>
///             <description>Null Check - Returns default if input string is null</description>
///         </item>
///         <item>
///             <description>Custom Converter - Uses provided converter function if available</description>
///         </item>
///         <item>
///             <description>Array Conversion - Handles array types with comma-separated values</description>
///         </item>
///         <item>
///             <description>Generic Conversion - Processes generic types like collections</description>
///         </item>
///         <item>
///             <description>Standard Conversion - Falls back to TypeDescriptor conversion</description>
///         </item>
///     </list>
///     <para>
///         This class is thread-safe and designed for high-performance scenarios where
///         type conversion is performed frequently, such as parameter resolution systems.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Basic type conversion
/// var intValue = TypeUtil.Convert&lt;int&gt;("42", null);
/// 
/// // Array conversion
/// var intArray = TypeUtil.Convert&lt;int[]&gt;("1,2,3,4", null);
/// 
/// // Collection conversion
/// var stringList = TypeUtil.Convert&lt;IReadOnlyList&lt;string&gt;&gt;("a,b,c", null);
/// 
/// // Custom converter
/// var customValue = TypeUtil.Convert&lt;DateTime&gt;("2023-12-25",
///     str => DateTime.ParseExact(str, "yyyy-MM-dd", null));
/// 
/// // Nullable handling
/// var nullableInt = TypeUtil.Convert&lt;int?&gt;(null, null); // Returns null
/// </code>
/// </example>
[PublicAPI]
public static class TypeUtil
{
    /// <summary>
    ///     Converts a string value to the specified target type with optional custom conversion logic.
    /// </summary>
    /// <typeparam name="T">
    ///     The target type to convert the string value to. Can be any type including
    ///     arrays, generic collections, nullable types, and custom types.
    /// </typeparam>
    /// <param name="stringValue">
    ///     The string value to convert. If null, the method returns the default value for type T.
    ///     For array conversions, comma-separated values are expected.
    /// </param>
    /// <param name="converter">
    ///     Optional custom conversion function that takes precedence over built-in conversion logic.
    ///     When provided, this function is responsible for the entire conversion process.
    ///     Can be null to use automatic type conversion based on the target type.
    /// </param>
    /// <returns>
    ///     The converted value of type <typeparamref name="T" />, or the default value for the type
    ///     if conversion fails or the input string is null.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         The method handles conversion using the following strategy:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description><strong>Null Input</strong> - Returns default(T) immediately</description>
    ///         </item>
    ///         <item>
    ///             <description><strong>Custom Converter</strong> - If provided, delegates all conversion logic to the custom function</description>
    ///         </item>
    ///         <item>
    ///             <description><strong>Array Types</strong> - Splits input by comma and converts each element to the array's element type</description>
    ///         </item>
    ///         <item>
    ///             <description><strong>Generic Types</strong> - Special handling for supported generic types like IReadOnlyList&lt;T&gt;</description>
    ///         </item>
    ///         <item>
    ///             <description><strong>Standard Types</strong> - Uses TypeDescriptor.GetConverter() for built-in type conversion</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         For array conversions, the input string is split using comma (',') as the delimiter.
    ///         Each resulting substring is then converted to the array's element type.
    ///     </para>
    ///     <para>
    ///         Currently supported generic types include IReadOnlyList&lt;T&gt;. Additional generic
    ///         types will throw a NotSupportedException with details about the unsupported type.
    ///     </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">
    ///     Thrown when attempting to convert to an unsupported generic type.
    ///     Currently only IReadOnlyList&lt;T&gt; is supported among generic types.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the underlying TypeDescriptor conversion fails for standard types,
    ///     or when array element conversion fails.
    /// </exception>
    /// <example>
    ///     <code>
    /// // Converting to primitive types
    /// var number = TypeUtil.Convert&lt;int&gt;("123", null);
    /// var flag = TypeUtil.Convert&lt;bool&gt;("true", null);
    /// var date = TypeUtil.Convert&lt;DateTime&gt;("2023-12-25", null);
    /// 
    /// // Converting to arrays
    /// var numbers = TypeUtil.Convert&lt;int[]&gt;("1,2,3,4,5", null);
    /// var names = TypeUtil.Convert&lt;string[]&gt;("John,Jane,Bob", null);
    /// 
    /// // Converting to collections
    /// var readOnlyList = TypeUtil.Convert&lt;IReadOnlyList&lt;string&gt;&gt;("apple,banana,cherry", null);
    /// 
    /// // Using custom converters
    /// var customDate = TypeUtil.Convert&lt;DateTime&gt;("25-12-2023",
    ///     str => DateTime.ParseExact(str, "dd-MM-yyyy", CultureInfo.InvariantCulture));
    /// 
    /// var enumValue = TypeUtil.Convert&lt;MyEnum&gt;("Value1",
    ///     str => Enum.Parse&lt;MyEnum&gt;(str, ignoreCase: true));
    /// 
    /// // Handling null and empty values
    /// var nullResult = TypeUtil.Convert&lt;int?&gt;(null, null); // Returns null
    /// var emptyResult = TypeUtil.Convert&lt;string&gt;("", null); // Returns empty string
    /// </code>
    /// </example>
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
