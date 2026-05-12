using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Release.Tests.TestSupport;

internal static class PublicApiSnapshotGenerator
{
    public static string Generate(Assembly assembly)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Public API snapshot for {assembly.GetName().Name}");
        foreach (var type in assembly.GetTypes().Where(IsVisibleType).Where(t => !IsCompilerGenerated(t))
                     .OrderBy(FormatTypeName, StringComparer.Ordinal))
        {
            builder.AppendLine(FormatType(type));
            foreach (var member in Members(type)) builder.AppendLine("  " + member);
            builder.AppendLine();
        }

        return builder.ToString().Replace("\r\n", "\n").TrimEnd() + "\n";
    }

    private static bool IsVisibleType(Type type)
    {
        return type.IsPublic || type.IsNestedPublic;
    }

    private static bool IsCompilerGenerated(MemberInfo member)
    {
        return member.GetCustomAttribute<CompilerGeneratedAttribute>() is not null ||
               member.Name.Contains("<", StringComparison.Ordinal);
    }

    private static IEnumerable<string> Members(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
                                   BindingFlags.DeclaredOnly;
        foreach (var ctor in type.GetConstructors(flags).Where(c => !IsCompilerGenerated(c))
                     .OrderBy(FormatConstructor, StringComparer.Ordinal)) yield return FormatConstructor(ctor);
        foreach (var field in type.GetFields(flags).Where(f => !f.IsSpecialName && !IsCompilerGenerated(f))
                     .OrderBy(FormatField, StringComparer.Ordinal)) yield return FormatField(field);
        foreach (var prop in type.GetProperties(flags).Where(p => !IsCompilerGenerated(p))
                     .OrderBy(FormatProperty, StringComparer.Ordinal)) yield return FormatProperty(prop);
        foreach (var evt in type.GetEvents(flags).Where(e => !IsCompilerGenerated(e))
                     .OrderBy(FormatEvent, StringComparer.Ordinal)) yield return FormatEvent(evt);
        foreach (var method in type.GetMethods(flags).Where(m => !m.IsSpecialName && !IsCompilerGenerated(m))
                     .OrderBy(FormatMethod, StringComparer.Ordinal)) yield return FormatMethod(method);
        if (type.IsEnum)
            foreach (var name in Enum.GetNames(type).OrderBy(n => n, StringComparer.Ordinal))
                yield return "enum " + name;
    }

    private static string FormatType(Type type)
    {
        var kind = type.IsEnum ? "enum" :
            type.IsInterface ? "interface" :
            type.IsValueType ? "struct" :
            type.IsAbstract && type.IsSealed ? "static class" :
            type.IsClass ? "class" : "type";
        var bases = new List<string>();
        if (type.BaseType is not null && type.BaseType != typeof(object) && !type.IsEnum && !type.IsValueType)
            bases.Add(FormatTypeName(type.BaseType));
        bases.AddRange(type.GetInterfaces().Select(FormatTypeName).OrderBy(x => x, StringComparer.Ordinal));
        return bases.Count == 0
            ? $"{kind} {FormatTypeName(type)}"
            : $"{kind} {FormatTypeName(type)} : {string.Join(", ", bases)}";
    }

    private static string FormatConstructor(ConstructorInfo ctor)
    {
        return $"ctor {FormatParameters(ctor.GetParameters())}";
    }

    private static string FormatField(FieldInfo field)
    {
        return $"field {FormatTypeName(field.FieldType)} {field.Name}";
    }

    private static string FormatProperty(PropertyInfo prop)
    {
        var accessors =
            new[] { prop.GetMethod is not null ? "get" : null, prop.SetMethod is not null ? "set" : null }.Where(x =>
                x is not null);
        return $"property {FormatTypeName(prop.PropertyType)} {prop.Name} {{{string.Join(";", accessors!)};}}";
    }

    private static string FormatEvent(EventInfo evt)
    {
        return $"event {FormatTypeName(evt.EventHandlerType!)} {evt.Name}";
    }

    private static string FormatMethod(MethodInfo method)
    {
        return
            $"method {FormatTypeName(method.ReturnType)} {method.Name}{FormatGenericParameters(method.GetGenericArguments())}{FormatParameters(method.GetParameters())}";
    }

    private static string FormatParameters(IEnumerable<ParameterInfo> parameters)
    {
        return "(" + string.Join(", ", parameters.Select(p => FormatTypeName(p.ParameterType) + " " + p.Name)) + ")";
    }

    private static string FormatGenericParameters(Type[] args)
    {
        return args.Length == 0 ? string.Empty : "<" + string.Join(", ", args.Select(a => a.Name)) + ">";
    }

    private static string FormatTypeName(Type type)
    {
        if (type.IsGenericParameter) return type.Name;
        if (type.IsArray) return FormatTypeName(type.GetElementType()!) + "[]";
        if (type.IsByRef) return FormatTypeName(type.GetElementType()!) + "&";
        if (Nullable.GetUnderlyingType(type) is { } nullable) return FormatTypeName(nullable) + "?";
        if (type.IsGenericType)
        {
            var definitionName = (type.FullName ?? type.Name).Split('`')[0].Replace('+', '.');
            return definitionName + "<" + string.Join(", ", type.GetGenericArguments().Select(FormatTypeName)) + ">";
        }

        return (type.FullName ?? type.Name).Replace('+', '.');
    }
}