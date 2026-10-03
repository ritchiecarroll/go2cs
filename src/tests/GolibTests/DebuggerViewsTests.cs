using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The debugger views of golib's Go values (IDE-mode plan, Phase 1): each of slice, array, map, string,
/// pointer and channel carries a DebuggerDisplay and a DebuggerTypeProxy, so Visual Studio, VS Code
/// (vsdbg) and Rider show it as Go would rather than as its backing internals. The proxies are internal,
/// so every arm here reaches them the way a debugger does -- through the attribute -- and reads them by
/// member NAME. A wrong member name in a DebuggerDisplay is shown as an error in the IDE and nowhere
/// else, which is why one arm checks every expression against the type it decorates.
/// </summary>
[TestClass]
public class DebuggerViewsTests
{
    private static readonly Type[] s_viewed = [typeof(slice<>), typeof(array<>), typeof(map<,>), typeof(@string), typeof(ж<>), typeof(channel<>)];

    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [TestMethod]
    public void EveryGoValueTypeCarriesADisplayAndAProxy()
    {
        List<string> missing = [];

        foreach (Type type in s_viewed)
        {
            if (type.GetCustomAttribute<DebuggerDisplayAttribute>(inherit: false) is null)
                missing.Add($"{type.Name}: DebuggerDisplay");

            if (type.GetCustomAttribute<DebuggerTypeProxyAttribute>(inherit: false) is null)
                missing.Add($"{type.Name}: DebuggerTypeProxy");
        }

        Assert.AreEqual(0, missing.Count, "missing debugger views: " + string.Join(", ", missing));
    }

    [TestMethod]
    public void EveryDisplayExpressionNamesARealMember()
    {
        List<string> unknown = [];

        foreach (Type type in s_viewed)
        {
            if (type.GetCustomAttribute<DebuggerDisplayAttribute>(inherit: false) is not { } display)
                continue;

            foreach (Match match in Regex.Matches(display.Value, @"\{([^}]*)\}"))
            {
                // `{Member}`, `{Member,nq}` or `{Member()}`: the member is the leading identifier.
                string member = Regex.Match(match.Groups[1].Value, @"^\s*([A-Za-z_][A-Za-z0-9_]*)").Groups[1].Value;

                if (member.Length == 0 || type.GetMember(member, AnyInstance | BindingFlags.FlattenHierarchy).Length == 0)
                    unknown.Add($"{type.Name}: {{{match.Groups[1].Value}}}");
            }
        }

        Assert.AreEqual(0, unknown.Count, "DebuggerDisplay names no such member: " + string.Join(", ", unknown));
    }

    [TestMethod]
    public void ASliceShowsItsLengthWindowAndCapacity()
    {
        slice<nint> s = new nint[] { 1, 2, 3, 4, 5 };
        s = s[1..3];

        object view = ProxyOf(s);

        Assert.AreEqual((nint)2, Read(view, "Length"));
        Assert.AreEqual((nint)4, Read(view, "Capacity"));
        CollectionAssert.AreEqual(new nint[] { 2, 3 }, (nint[])Read(view, "Items")!);
    }

    [TestMethod]
    public void AnArrayShowsItsElements()
    {
        array<nint> a = new nint[] { 7, 8, 9 };

        CollectionAssert.AreEqual(new nint[] { 7, 8, 9 }, (nint[])Read(ProxyOf(a), "Items")!);
    }

    [TestMethod]
    public void AMapShowsItsPairs()
    {
        map<@string, nint> m = new() { [(@string)"a"] = 1, [(@string)"b"] = 2 };

        object view = ProxyOf(m);
        var items = (KeyValuePair<@string, nint>[])Read(view, "Items")!;

        Assert.AreEqual(2, Read(view, "Count"));
        Assert.AreEqual(0, Read(view, "More"));
        CollectionAssert.AreEquivalent(new[] { "a=1", "b=2" }, items.Select(pair => $"{pair.Key}={pair.Value}").ToArray());
    }

    [TestMethod]
    public void AHugeMapShowsAThousandPairsAndCountsTheRest()
    {
        map<nint, nint> m = new(1_000_000);

        for (nint i = 0; i < 1_000_000; i++)
            m[i] = i;

        Stopwatch watch = Stopwatch.StartNew();
        object view = ProxyOf(m);
        var items = (KeyValuePair<nint, nint>[])Read(view, "Items")!;
        watch.Stop();

        Assert.AreEqual(1000, items.Length);
        Assert.AreEqual(999_000, Read(view, "More"));
        Assert.IsTrue(watch.ElapsedMilliseconds < 1000, $"the view of a million-entry map took {watch.ElapsedMilliseconds} ms");
    }

    [TestMethod]
    public void AStringShowsItsTextAndItsBytes()
    {
        @string s = "héllo";

        object view = ProxyOf(s);

        Assert.AreEqual("héllo", Read(view, "Text"));
        CollectionAssert.AreEqual(System.Text.Encoding.UTF8.GetBytes("héllo"), (byte[])Read(view, "Bytes")!);
    }

    [TestMethod]
    public void APointerShowsItsPointee()
    {
        ж<nint> p = new go.StandardBox<nint>(42);

        Assert.AreEqual((nint)42, Read(ProxyOf(p, typeof(ж<nint>)), "Value"));
    }

    [TestMethod]
    public void AChannelShowsLengthCapacityAndClosed()
    {
        channel<nint> c = new(3);
        c.Send(1);
        c.Send(2);
        c.Close();

        object view = ProxyOf(c);

        Assert.AreEqual((nint)2, Read(view, "Length"));
        Assert.AreEqual((nint)3, Read(view, "Capacity"));
        Assert.AreEqual(true, Read(view, "Closed"));
    }

    [TestMethod]
    public void AChannelPrintsAsGoPrintsOne()
    {
        // Go's %v of a channel is its address, and of a nil channel <nil>; a channel's ToString
        // answers the same, so nothing that formats one through ToString disagrees with fmt.
        channel<nint> c = new(1);

        StringAssert.StartsWith(c.ToString(), "0x");
        Assert.AreEqual("<nil>", default(channel<nint>).ToString());
    }

    // The debugger's own route to a view: the type's DebuggerTypeProxy, closed over the value's type
    // arguments, constructed over the value.
    private static object ProxyOf(object value, Type? declared = null)
    {
        Type type = declared ?? value.GetType();
        Type definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

        DebuggerTypeProxyAttribute attribute = definition.GetCustomAttribute<DebuggerTypeProxyAttribute>(inherit: false)
            ?? throw new AssertFailedException($"{definition.Name} has no DebuggerTypeProxy");

        Type proxy = Type.GetType(attribute.ProxyTypeName, throwOnError: true)!;

        if (proxy.IsGenericTypeDefinition)
            proxy = proxy.MakeGenericType(type.GetGenericArguments());

        return Activator.CreateInstance(proxy, AnyInstance, null, [value], null)!;
    }

    private static object? Read(object view, string member) =>
        view.GetType().GetProperty(member, AnyInstance)?.GetValue(view)
        ?? view.GetType().GetField(member, AnyInstance)?.GetValue(view)
        ?? throw new AssertFailedException($"{view.GetType().Name} has no member {member}");
}
