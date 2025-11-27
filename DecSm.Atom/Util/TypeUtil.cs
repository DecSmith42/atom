using System.Globalization;

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
/// // Array conversion
/// var intArray = TypeUtil.Convert&lt;int[]&gt;("1,2,3,4", null);
/// // Collection conversion
/// var stringList = TypeUtil.Convert&lt;IReadOnlyList&lt;string&gt;&gt;("a,b,c", null);
/// // Custom converter
/// var customValue = TypeUtil.Convert&lt;DateTime&gt;("2023-12-25",
///     str => DateTime.ParseExact(str, "yyyy-MM-dd", null));
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
    ///             <description>
    ///                 <strong>Custom Converter</strong> - If provided, delegates all conversion logic to the custom
    ///                 function
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <strong>Array Types</strong> - Splits input by comma and converts each element to the array's
    ///                 element type
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <strong>Generic Types</strong> - Special handling for supported generic types like
    ///                 IReadOnlyList&lt;T&gt;
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <strong>Standard Types</strong> - Uses TypeDescriptor.GetConverter() for built-in type
    ///                 conversion
    ///             </description>
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
    /// // Converting to arrays
    /// var numbers = TypeUtil.Convert&lt;int[]&gt;("1,2,3,4,5", null);
    /// var names = TypeUtil.Convert&lt;string[]&gt;("John,Jane,Bob", null);
    /// // Converting to collections
    /// var readOnlyList = TypeUtil.Convert&lt;IReadOnlyList&lt;string&gt;&gt;("apple,banana,cherry", null);
    /// // Using custom converters
    /// var customDate = TypeUtil.Convert&lt;DateTime&gt;("25-12-2023",
    ///     str => DateTime.ParseExact(str, "dd-MM-yyyy", CultureInfo.InvariantCulture));
    /// var enumValue = TypeUtil.Convert&lt;MyEnum&gt;("Value1",
    ///     str => Enum.Parse&lt;MyEnum&gt;(str, ignoreCase: true));
    /// // Handling null and empty values
    /// var nullResult = TypeUtil.Convert&lt;int?&gt;(null, null); // Returns null
    /// var emptyResult = TypeUtil.Convert&lt;string&gt;("", null); // Returns empty string
    /// </code>
    /// </example>
    [UnconditionalSuppressMessage("Trimming",
        "IL2026",
        Justification =
            "May fall back to the TypeDescriptor-based Convert(string, Type) helper (marked RequiresUnreferencedCode) when no custom converter, array/generic path, or AOT-safe TryConvert path can handle the input. This is a last-resort conversion; in trimmed/AOT builds callers should prefer passing explicit converters for unsupported types.")]
    public static T? Convert<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string? stringValue,
        Func<string?, T?>? converter) =>
        stringValue is null
            ? default
            : converter is not null
                ? converter(stringValue)
                : typeof(T).IsArray
                    ? ConvertArray<T>(stringValue)
                    : typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() != typeof(Nullable<>)
                        ? ConvertGeneric<T>(stringValue, typeof(T))
                        : TryConvert(stringValue, typeof(T), out var simple) && simple is T simpleT
                            ? simpleT
                            : Convert(stringValue, typeof(T)) is T tValue
                                ? tValue
                                : default;

    [RequiresUnreferencedCode(
        "TypeDescriptor.GetConverter(Type) is not trimming-safe. Converters for some types may be removed during trimming.")]
    [UnconditionalSuppressMessage("Trimming",
        "IL2067",
        Justification =
            "TypeDescriptor.GetConverter(Type) uses runtime-discovered converters that can be trimmed away. The 'type' parameter is annotated with DynamicallyAccessedMembers(All), but the linker cannot guarantee presence of external converter types, so IL2067 can still be produced. This method is an explicit best-effort fallback for non-AOT-friendly conversions; AOT callers should avoid it or provide explicit converters.")]
    private static object? Convert(
        string value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type) =>
        TypeDescriptor
            .GetConverter(type)
            .ConvertFromInvariantString(value);

    // Best-effort AOT-friendly conversion that avoids TypeDescriptor where possible.
    private static bool TryConvert(string value, Type type, out object? result)
    {
        // Handle Nullable<T> by unwrapping the underlying type
        if (IsNullableValueType(type, out var underlying))
        {
            if (TryConvert(value, underlying!, out var inner))
            {
                result = inner;

                return true;
            }

            result = null;

            return false;
        }

        if (TryConvertSimple(value, type, out result))
            return true;

        // Enums (non-flag) and flags
        if (type.IsEnum)
        {
            try
            {
                result = Enum.Parse(type, value, true);

                return true;
            }
            catch
            {
                // ignored
            }
        }

        result = null;

        return false;
    }

    private static bool IsNullableValueType(Type type, out Type? underlying)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            underlying = Nullable.GetUnderlyingType(type);

            return true;
        }

        underlying = null;

        return false;
    }

    private static bool TryConvertSimple(string value, Type type, out object? result)
    {
        // String is already simple
        if (type == typeof(string))
        {
            result = value;

            return true;
        }

        // Char
        if (type == typeof(char))
        {
            if (value.Length == 1)
            {
                result = value[0];

                return true;
            }

            result = null;

            return false;
        }

        var styleInteger = NumberStyles.Integer;
        var styleFloat = NumberStyles.Float | NumberStyles.AllowThousands;
        var ci = CultureInfo.InvariantCulture;

        // Boolean
        if (type == typeof(bool))
        {
            if (bool.TryParse(value, out var b))
            {
                result = b;

                return true;
            }

            result = null;

            return false;
        }

        // Integral types
        if (type == typeof(int))
        {
            if (int.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(long))
        {
            if (long.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(short))
        {
            if (short.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(byte))
        {
            if (byte.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(sbyte))
        {
            if (sbyte.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(uint))
        {
            if (uint.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(ulong))
        {
            if (ulong.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(ushort))
        {
            if (ushort.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        // Floating point / decimal
        if (type == typeof(float))
        {
            if (float.TryParse(value, styleFloat, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(double))
        {
            if (double.TryParse(value, styleFloat, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(decimal))
        {
            if (decimal.TryParse(value, styleFloat, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        // Guid
        if (type == typeof(Guid))
        {
            if (Guid.TryParse(value, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        // Date/time types
        if (type == typeof(DateTime))
        {
            if (DateTime.TryParse(value, ci, DateTimeStyles.None, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(TimeSpan))
        {
            if (TimeSpan.TryParse(value, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        #if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
        {
            if (DateOnly.TryParse(value, ci, DateTimeStyles.None, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(TimeOnly))
        {
            if (TimeOnly.TryParse(value, ci, DateTimeStyles.None, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }
        #endif

        result = null;

        return false;
    }

    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2026:RequiresUnreferencedCode",
        Justification =
            "ConvertArray<T> may fall back to Convert(string, Type), which is marked RequiresUnreferencedCode. We first attempt AOT-safe TryConvert for each element and only use the TypeDescriptor-based fallback if necessary. Generic parameter T is annotated with DynamicallyAccessedMembers(All), improving linker preservation of required members; nonetheless callers targeting AOT should prefer element types supported by TryConvert or supply explicit converters.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2072:UnrecognizedReflectionPattern",
        Justification =
            "The analyzer cannot prove at compile time the safety of using typeof(T).GetElementType() and Array.CreateInstance for arbitrary T. This method only operates on array types (T is TElement[]), and the reflection usage is limited to creating an array and passing the element Type into narrowly-scoped conversion helpers. The remaining risky path is the explicit RUC-marked fallback, which is documented above.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL3050:RequiresUnreferencedCode",
        Justification =
            "May fall back to the TypeDescriptor-based Convert(string, Type) helper when no AOT-safe TryConvert path can handle the input. This is a last-resort conversion; in trimmed/AOT builds callers should prefer passing explicit converters for unsupported types.")]
    private static T ConvertArray<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string value)
    {
        var elementType = typeof(T).GetElementType()!;
        var values = value.Split(',');

        var array = Array.CreateInstance(elementType, values.Length);

        for (var i = 0; i < values.Length; i++)
        {
            if (TryConvert(values[i], elementType, out var elem))
                array.SetValue(elem, i);
            else
                array.SetValue(Convert(values[i], elementType), i);
        }

        return (T)(object)array;
    }

    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2026:RequiresUnreferencedCode",
        Justification =
            "ConvertGeneric<T> can fall back to Convert(string, Type) for each element if AOT-safe TryConvert fails. That helper is intentionally marked RequiresUnreferencedCode. Here we only support IReadOnlyList<TElement> and implement it by constructing a TElement[]; all conversions attempt AOT-safe parsing first.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2062:UnrecognizedReflectionPattern",
        Justification =
            "This method inspects the generic type (GetGenericTypeDefinition/GetGenericArguments) and creates an array via Array.CreateInstance. The usage is bounded to supported shape IReadOnlyList<TElement>. The generic parameter T is annotated with DynamicallyAccessedMembers(All) at the call site, improving member preservation. Any remaining reflection risk exists only on the documented RUC-marked fallback path.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL3050:RequiresUnreferencedCode",
        Justification =
            "May fall back to the TypeDescriptor-based Convert(string, Type) helper when no AOT-safe TryConvert path can handle the input. This is a last-resort conversion; in trimmed/AOT builds callers should prefer passing explicit converters for unsupported types.")]
    private static T ConvertGeneric<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string value,
        Type type)
    {
        var genericType = type.GetGenericTypeDefinition();

        var genericArgument = type
            .GetGenericArguments()[0];

        if (genericType == typeof(IReadOnlyList<>))
        {
            var values = value.Split(',');

            // Build a TElement[] which implements IReadOnlyList<TElement>
            var array = Array.CreateInstance(genericArgument, values.Length);

            for (var i = 0; i < values.Length; i++)
                if (TryConvert(values[i], genericArgument, out var elem))
                    array.SetValue(elem, i);
                else
                    array.SetValue(Convert(values[i], genericArgument), i);

            return (T)(object)array;
        }

        throw new NotSupportedException($"Generic type '{type}' is not supported.");
    }
}
