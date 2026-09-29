// NoUncountedBackingAllocationsTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The guard <c>AllocationCounter</c>'s documentation names: every raw array backing golib creates goes
/// through <c>AllocationCounter</c> (NewArray / CopyOf / Materialize / Utf8ToBytes), or it is on a NAMED allow-list.
/// </summary>
/// <remarks>
/// <para>
/// A counted allocation is only as honest as its census: a golib site that allocates a Go-visible backing
/// without charging it makes <c>testing.AllocsPerRun</c> read LOW, and nothing else notices. This guard scans
/// golib's own source for the raw forms (<c>new T[n]</c>, <c>.ToArray()</c>, <c>GC.Allocate*Array</c>,
/// <c>Encoding.UTF8.GetBytes</c>) outside <c>AllocationCounter.cs</c> and compares them, as a multiset keyed
/// by file and normalized source line, against two allow-list blocks:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <see cref="ByDesign"/>, the (b) block: allocations on a Go construct's path where Go's OWN construct
///     allocates nothing (a compiler stack temp, <c>runtime.zerobase</c>), so charging them would report a
///     malloc Go does not make. Each reason cites Go's source for the equivalent construct. The block began
///     as V-fix 11's 21 unclassified sites; the follow-on seat routed the real undercounts and the sites
///     whose charge the scan could not see, and these five are what remained.
///   </description></item>
///   <item><description>
///     <see cref="Infrastructure"/>, the (c) block: allocations that are not Go-visible backings at all
///     (reflection metadata, delegate and expression arrays, runtime bookkeeping), each with its reason.
///   </description></item>
/// </list>
/// <para>
/// A NEW raw site fails the guard until it is routed or allow-listed by name, and an allow-list entry that no
/// longer matches any site fails too, so each block stays one exact list. The scan is
/// textual on purpose: it reads the source the golib assembly is built from, located through this file's own
/// compile-time path.
/// </para>
/// </remarks>
[TestClass]
public class NoUncountedBackingAllocationsTests
{
    // (b) BY-DESIGN: Go's own construct allocates nothing here; the reason cites where Go keeps it instead.
    // Key: "<golib-relative path>|<normalized source line>". Duplicates are listed once per occurrence.
    private static readonly (string Site, string Reason)[] ByDesign =
    [
        ("GoReflect.TypeLayout.cs|s = new slice<T>(new T[0]);", "a zero-length backing: Go's mallocgc answers size 0 with &zerobase and no malloc (runtime/malloc.go); the fresh object exists only as the dims table's identity key"),
        ("builtin.cs|return (slice<T>)slice.Append(elems.Cast<object>().ToArray())!;", "the ISlice bridge's staging array: Go's append is a builtin that writes the elements in place, with no staging array"),
        ("channel.cs|ChanCore[] lockOrder = cores.ToArray();", "select scratch: Go's select statement keeps its lock/poll order in a compiler stack temp (cmd/compile/internal/walk/select.go, order TempAt)"),
        ("channel.cs|int[] pollOrder = new int[liveCount];", "select scratch: Go's select statement keeps its lock/poll order in a compiler stack temp (cmd/compile/internal/walk/select.go, order TempAt)"),
        ("map.cs|KeyValuePair<TKey, TValue>[] entries = count == 0 ? [] : new KeyValuePair<TKey, TValue>[count];", "the range snapshot: Go's range over a map keeps its iterator in a compiler stack temp (cmd/compile/internal/walk/order.go, Prealloc newTemp)"),
    ];

    // (c) INFRASTRUCTURE: not a Go-visible backing; the reason names what the array is.
    private static readonly (string Site, string Reason)[] Infrastructure =
    [
        ("AdapterBinder.cs|MethodInvoker[] invokers = new MethodInvoker[methodNames.Length];", "adapter binder plumbing: per-binding invoker tables and the reflection-invoke argument array of its fallback path"),
        ("AdapterBinder.cs|bool[] dereference = new bool[methodNames.Length];", "adapter binder plumbing: per-binding invoker tables and the reflection-invoke argument array of its fallback path"),
        ("AdapterBinder.cs|object?[] arguments = new object?[args.Length + 1];", "adapter binder plumbing: per-binding invoker tables and the reflection-invoke argument array of its fallback path"),
        ("GoReflect.FieldAccess.cs|GoFieldInfo[] ordered = new GoFieldInfo[count];", "reflection metadata: the cached, ordered Go field table of a struct type"),
        ("GoReflect.FieldAccess.cs|return result.ToArray();", "reflection metadata: the cached, ordered Go field table of a struct type"),
        ("GoReflect.MakeVariadicDelegate.cs|Type[] typeArguments = new Type[fixedCount + (hasResult ? 2 : 1)];", "reflection metadata: generic type arguments for a delegate type"),
        ("GoReflect.MethodSets.cs|Expression[] call = new Expression[ps.Length];", "expression-tree compilation of a method-set thunk (compiled once per method)"),
        ("GoReflect.MethodSets.cs|ParameterExpression[] args = new ParameterExpression[ps.Length];", "expression-tree compilation of a method-set thunk (compiled once per method)"),
        ("GoReflect.TypeLayout.cs|Type[] flattened = new Type[count];", "type-layout metadata: field offsets, array dims and signature types, computed per type"),
        ("GoReflect.TypeLayout.cs|Type[] typeArguments = new Type[fixedCount + (hasResult ? 2 : 1)];", "type-layout metadata: field offsets, array dims and signature types, computed per type"),
        ("GoReflect.TypeLayout.cs|byte[] mask = new byte[size / (nuint)GoWordSize];", "type-layout metadata: the pointer mask of a Go type, computed once per type"),
        ("GoReflect.TypeLayout.cs|ins = new Type[parameters.Length];", "type-layout metadata: field offsets, array dims and signature types, computed per type"),
        ("GoReflect.TypeLayout.cs|nint[] offsets = new nint[fields.Length];", "type-layout metadata: field offsets, array dims and signature types, computed per type"),
        ("GoReflect.TypeLayout.cs|nint[] result = new nint[dims.Length];", "type-layout metadata: field offsets, array dims and signature types, computed per type"),
        ("GoReflect.TypeLayout.cs|nint[] result = new nint[dims.Length];", "type-layout metadata: field offsets, array dims and signature types, computed per type"),
        ("GoReflect.ValueMarshalling.cs|long[] dims = new long[arrayDims.Length];", "reflection metadata: the dims of a nested array type"),
        ("GoStructSynthesis.cs|Type[] signature = new Type[targetParameters.Length - skip + 1];", "runtime struct-type synthesis: signature and dims metadata of a synthesized type"),
        ("GoStructSynthesis.cs|int[] result = new int[dims.Length];", "runtime struct-type synthesis: signature and dims metadata of a synthesized type"),
        ("GoStructSynthesis.cs|long[] result = new long[dims.Length];", "runtime struct-type synthesis: signature and dims metadata of a synthesized type"),
        ("string.cs|byte[][] table = new byte[256][];", "the one-byte string table: Go's runtime.staticuint64s, static data every one-byte string([]byte) views (runtime/string.go:144-150), built once, not an allocation of any conversion"),
        ("GoZeroSize.cs|internal static readonly T[] Storage = IsZeroSize ? new T[1] : [];", "a per-type static singleton: the one storage slot every zero-size value of T shares"),
        ("GoFrame.cs|Sequence[] entries = state.Sequences ??= new Sequence[16];", "runtime bookkeeping: the per-thread panic-sequence entries captureCallers pairs with GoFrame.Run frames (held in the thread's GoThreadState), allocated on the thread's first panic and reused"),
        ("ж.SliceHeaderBox.cs|internal static readonly object Element = new ElemRefBox<X>(new slice<X>(new X[1]), 0);", "a per-type static singleton: the zerobase a zero-capacity slice's array word names, never an element any slice holds"),
        ("GoZeroSize.cs|private static T[] Backing() => GoZeroSizeFacts<T>.IsZeroSize ? GoZeroSizeFacts<T>.Storage : new T[1];", "a per-type static singleton: the one slot a non-nil zero-capacity slice's data pointer names (GoZeroCapacityElement)"),
        ("runtime/BoringCaches.cs|Action[] updated = new Action[s_caches.Length + 1];", "runtime bookkeeping: the registered cache-cleanup callbacks (copy-on-register)"),
        ("runtime/CrashReport.cs|byte[] bytes = Encoding.UTF8.GetBytes(report);", "crash-report output: the UTF-8 text of a fatal report, written once as the process dies"),
        ("runtime/ExecutionTracer.cs|byte[] bytes = Encoding.UTF8.GetBytes(text);", "execution-trace output: a strings-table entry of the trace stream, written once per trace at stop; Go's tracer buffers are off-heap and never reach MemStats.Mallocs, so counting them would perturb AllocsPerRun under a running trace"),
        ("runtime/GcPauseRecorder.cs|private static readonly ulong[] s_pauseEndUnixNs = new ulong[RingLength];", "runtime bookkeeping: the static GC pause ring buffers behind ReadMemStats"),
        ("runtime/GcPauseRecorder.cs|private static readonly ulong[] s_pauseNs = new ulong[RingLength];", "runtime bookkeeping: the static GC pause ring buffers behind ReadMemStats"),
        ("runtime/TypeExtensions.ExtensionMethodRegistry.cs|return Delegate.CreateDelegate(getMethodType(types.ToArray()), methodInfo);", "runtime metadata: the extension-method registry and delegate signature types"),
        ("runtime/TypeExtensions.ExtensionMethodRegistry.cs|return methods.ToArray();", "runtime metadata: the extension-method registry and delegate signature types"),
        ("runtime/TypeExtensions.ExtensionMethodRegistry.cs|s_extensionMethods = extensionMethods.ToArray();", "runtime metadata: the extension-method registry and delegate signature types"),
        ("runtime/WaitReason.cs|WaitReason[] parked = new WaitReason[all.Length - 1];", "runtime metadata: the static table of wait reasons"),
        ("ж.HeaderSliceBox.cs|.ToArray();", "reflection metadata: the ordered fields of a slice-header struct type"),
        ("ж.PointerExtensions.cs|Type[] fieldTypes = new Type[fields.Length];", "reflection metadata: the field types of a reinterpreted struct"),
        ("ж.SliceHeaderBox.cs|.ToArray();", "reflection metadata: the ordered fields of a slice-header struct type"),
    ];

    private static readonly Regex[] s_rawForms =
    [
        new(@"\bnew\s+[A-Za-z_][\w.<>, ?]*\[\s*[^\]\s][^\]]*\]", RegexOptions.Compiled),
        new(@"\.ToArray\(\)", RegexOptions.Compiled),
        new(@"\bGC\.Allocate(?:Uninitialized)?Array\b", RegexOptions.Compiled),
        new(@"\bEncoding\.UTF8\.GetBytes\(", RegexOptions.Compiled),
    ];

    private static string GolibRoot([CallerFilePath] string thisFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", "..", "core", "golib"));

    [TestMethod]
    public void NoUncountedBackingAllocations()
    {
        string root = GolibRoot();

        if (!Directory.Exists(root))
            Assert.Inconclusive($"golib source not found at {root}; this guard reads the source tree it was built from");

        List<string> found = ScanGolib(root);
        List<string> allowed = ByDesign.Concat(Infrastructure).Select(entry => entry.Site).ToList();

        List<string> unlisted = MultisetMinus(found, allowed);
        List<string> stale = MultisetMinus(allowed, found);

        StringBuilder message = new();

        if (unlisted.Count > 0)
        {
            message.AppendLine($"{unlisted.Count} raw backing allocation(s) in golib bypass AllocationCounter and are not allow-listed; " +
                "route each through AllocationCounter.NewArray/CopyOf/Materialize/Utf8ToBytes, or list it BY NAME, with its reason, in ByDesign (b) or Infrastructure (c):");

            foreach (string site in unlisted)
                message.AppendLine($"    \"{site}\",");
        }

        if (stale.Count > 0)
        {
            message.AppendLine($"{stale.Count} allow-list entr(y/ies) match no site any more (routed, moved or edited); remove or update them:");

            foreach (string site in stale)
                message.AppendLine($"    \"{site}\",");
        }

        if (message.Length > 0)
            Assert.Fail(message.ToString());
    }

    [TestMethod]
    public void AllowListEntriesCarryAReason()
    {
        foreach ((string site, string reason) in ByDesign.Concat(Infrastructure))
            Assert.IsFalse(string.IsNullOrWhiteSpace(reason), $"allow-list entry without a reason: {site}");
    }

    // The scanner must be able to fail: each raw form is found in live code, and none is found inside a
    // comment or a string literal.
    [TestMethod]
    public void ScannerSeesEveryRawFormAndIgnoresCommentsAndStrings()
    {
        string source = string.Join("\n",
            "byte[] a = new byte[n];",
            "var b = span.ToArray();",
            "var c = GC.AllocateUninitializedArray<int>(4);",
            "var d = Encoding.UTF8.GetBytes(s);",
            "// byte[] e = new byte[n];",
            "/* var f = x.ToArray();",
            "   still a comment */ int g = 1;",
            "string h = \"new byte[n] .ToArray()\";",
            "public new ref T this[nint index] => ref x;");

        List<string> found = ScanSource("probe.cs", source);

        Assert.AreEqual(4, found.Count, "the scanner must find exactly the four live raw forms: " + string.Join(" | ", found));
    }

    private static List<string> ScanGolib(string root)
    {
        List<string> found = [];

        foreach (string file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');

            if (relative.StartsWith("bin/") || relative.StartsWith("obj/") || relative.Contains("/Generated/") ||
                relative.StartsWith("Generated/") || relative == "AllocationCounter.cs")
                continue;

            found.AddRange(ScanSource(relative, File.ReadAllText(file)));
        }

        found.Sort(StringComparer.Ordinal);

        return found;
    }

    // Strips comments, string literals and char literals (keeping line structure), then reports one key per
    // raw form found on a line.
    internal static List<string> ScanSource(string relative, string source)
    {
        List<string> found = [];
        string[] originalLines = source.Replace("\r\n", "\n").Split('\n');
        string[] codeLines = StripNonCode(source.Replace("\r\n", "\n")).Split('\n');

        for (int i = 0; i < codeLines.Length; i++)
        {
            foreach (Regex form in s_rawForms)
            {
                foreach (Match match in form.Matches(codeLines[i]))
                {
                    // `public new ref T this[nint index]` is an indexer declaration with a `new` modifier.
                    if (match.Value.Contains("this["))
                        continue;

                    found.Add($"{relative}|{Normalize(originalLines[i])}");
                }
            }
        }

        return found;
    }

    private static string Normalize(string line) => Regex.Replace(line.Trim(), @"\s+", " ");

    private static string StripNonCode(string source)
    {
        StringBuilder code = new(source.Length);
        int i = 0;

        while (i < source.Length)
        {
            char c = source[i];

            if (c == '/' && i + 1 < source.Length && source[i + 1] == '/')
            {
                while (i < source.Length && source[i] != '\n')
                    i++;
            }
            else if (c == '/' && i + 1 < source.Length && source[i + 1] == '*')
            {
                i += 2;

                while (i < source.Length && !(source[i] == '*' && i + 1 < source.Length && source[i + 1] == '/'))
                {
                    if (source[i] == '\n')
                        code.Append('\n');

                    i++;
                }

                i += 2;
            }
            else if (c == '@' && i + 1 < source.Length && source[i + 1] == '"')
            {
                i += 2;

                while (i < source.Length && !(source[i] == '"' && (i + 1 >= source.Length || source[i + 1] != '"')))
                {
                    if (source[i] == '"')
                        i++;

                    if (source[i] == '\n')
                        code.Append('\n');

                    i++;
                }

                code.Append("\"\"");
                i++;
            }
            else if (c == '"' || c == '\'')
            {
                char quote = c;
                i++;

                while (i < source.Length && source[i] != quote && source[i] != '\n')
                {
                    if (source[i] == '\\')
                        i++;

                    i++;
                }

                code.Append(quote).Append(quote);
                i++;
            }
            else
            {
                code.Append(c);
                i++;
            }
        }

        return code.ToString();
    }

    private static List<string> MultisetMinus(List<string> left, List<string> right)
    {
        Dictionary<string, int> counts = right.GroupBy(site => site).ToDictionary(group => group.Key, group => group.Count());
        List<string> result = [];

        foreach (string site in left)
        {
            if (counts.TryGetValue(site, out int remaining) && remaining > 0)
                counts[site] = remaining - 1;
            else
                result.Add(site);
        }

        return result;
    }
}
