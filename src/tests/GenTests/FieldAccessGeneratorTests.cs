// FieldAccessGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Pins <see cref="FieldAccessGenerator"/>: the field properties it emits on <c>ж&lt;T&gt;</c> for the struct types
/// the <c>[GoFieldAccess]</c> records list, and the collision list it shares with the converter.
/// </summary>
/// <remarks>
/// The generated text is read as text: this Roslyn cannot compile a C# 14 extension block, and the end-to-end
/// compile and run is the FieldAccessReinterpret behavioral row.
/// </remarks>
[TestClass]
public class FieldAccessGeneratorTests
{
    private const string Stubs = """
        namespace go;

        public delegate ref TField FieldRefFunc<T, TField>(ref T instance);
        public class ж<T> { public ref TF field<TF>(FieldRefFunc<T, TF> f) => throw null!; public ж<TF> of<TF>(FieldRefFunc<T, TF> f) => throw null!; public ref T Value => throw null!; }
        public sealed class NativeBox<T> : ж<T> { }
        public struct array<T> { }
        public sealed class GoFieldAccessAttribute(params System.Type[] types) : System.Attribute { }
        public interface IUnsafePointer { }
        public static partial class unsafe_package { public class Pointer : IUnsafePointer { } }
        public struct uintptr { public nuint Value; }
        """;

    private const string Package = """
        namespace go;

        [GoFieldAccess(typeof(record))]
        partial class main_package
        {
            public partial struct tail { public ushort A, B; }

            public partial struct record
            {
                public uint Kind;
                public ushort @base;
                public array<ushort> Path;
                public uint Value;
                public ushort _;
                internal partial ref tail tail { get; }
            }

            public partial struct unrecorded { public uint X; }
        }

        [GoFieldAccess(typeof(main_package.pair<>))]
        partial class main_package
        {
            public partial struct pair<T> where T : struct { public T First; public int Count; }
        }
        """;

    // A FOREIGN struct, compiled to metadata the way a referenced package's assembly reaches the generator: its
    // Ꮡ accessors are visible and decide which fields get a property.
    private const string Foreign = """
        namespace go;

        public static partial class syscall_package
        {
            public partial struct Sockaddr
            {
                public ushort Family;
                public ushort Port;
                public uint NoAccessor;
                internal uint hidden;

                public static ref ushort ᏑFamily(ref Sockaddr instance) => ref instance.Family;
                public static ref ushort ᏑPort(ref Sockaddr instance) => ref instance.Port;
                internal static ref uint Ꮡhidden(ref Sockaddr instance) => ref instance.hidden;
            }
        }
        """;

    private const string User = """
        namespace go;

        [GoFieldAccess(typeof(syscall_package.Sockaddr))]
        partial class user_package { }
        """;

    private static string Generate(string source, params MetadataReference[] references) =>
        GenerateFiles([("", source)], references);

    // Each source is parsed under its file path, which decides whether a declaration is a converted Go type's.
    private static string GenerateFiles((string path, string source)[] sources, params MetadataReference[] references)
    {
        CSharpCompilation compilation = CSharpCompilation.Create("test",
            [.. sources.Select(file => CSharpSyntaxTree.ParseText(file.source, path: file.path)), CSharpSyntaxTree.ParseText(Stubs)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), .. references],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(compilation).GetRunResult();

        Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

        return result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "";
    }

    private static MetadataReference ForeignReference(string source = Foreign)
    {
        CSharpCompilation foreign = CSharpCompilation.Create("syscall",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream image = new();
        Assert.IsTrue(foreign.Emit(image).Success, "the foreign fixture compiles");

        return MetadataReference.CreateFromImage(image.ToArray());
    }

    // Roslyn 4.14 (go2cs-gen's floor) displays a type named `record`, a contextual keyword, escaped; 4.10 printed it
    // bare. Both spellings compile.
    private const string Record = "global::go.main_package.@record";

    // Every accessor's attributes: go2cs machinery, inlined, and no Go frame where the JIT keeps one (design Q4).
    private static readonly string Getter =
        $"[{Common.GeneratedCodeAttribute}, global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

    // A field's property, with the specialized body (design Q1): the field in place on a managed box, at its Go offset
    // through the accessor on a NativeBox<T>. `field` is the field's own name where the property's is escaped.
    private static string Ref(string fieldType, string name, string type, string? field = null) =>
        $"public ref {fieldType} {name} {{ {Getter} get => ref ʗp is global::go.NativeBox<{type}> ? ref ʗp.field({type}.Ꮡ{field ?? name}) : ref ʗp.Value.{name}; }}";

    // A field's ADDRESS property (parity seat #32, P2: `&p.f` reads `p.Ꮡf`): the field's view over the struct's own
    // per-field token `T.Ꮡf‿` (FieldAccessorTokenTests), one delegate object for every site in every assembly, so
    // golib's view cache compares by reference. `token` is the accessor itself where no token is visible.
    private static string Address(string fieldType, string name, string type, string? token = null) =>
        $"public global::go.ж<{fieldType}> Ꮡ{name} {{ {Getter} get => ʗp.of({type}.{token ?? $"Ꮡ{name}‿"}); }}";

    // A header-shaped struct's field: get/set over the accessor, the setter committing the header.
    private static string Header(string fieldType, string name, string type) =>
        $"public {fieldType} {name} {{ {Getter} get => ʗp.field({type}.Ꮡ{name}); {Getter} set {{ ʗp.field({type}.Ꮡ{name}) = value; ʗp.OnHeaderStore(); }} }}";

    [TestMethod]
    public void ARecordedStructGetsARefPropertyPerField()
    {
        string generated = Generate(Package);

        StringAssert.Contains(generated, $"extension(global::go.ж<{Record}> ʗp)");
        StringAssert.Contains(generated, Ref("uint", "Kind", Record));
        StringAssert.Contains(generated, Ref("global::go.array<ushort>", "Path", Record));
        Assert.IsFalse(generated.Contains("unrecorded"), "a struct no record lists gets no properties");
    }

    [TestMethod]
    public void AKeywordFieldKeepsItsEscapeAndItsAccessorDropsIt()
    {
        StringAssert.Contains(Generate(Package), Ref("ushort", "@base", Record, field: "base"));
    }

    [TestMethod]
    public void ACollisionNameAndABlankFieldGetNoProperty()
    {
        string generated = Generate(Package);

        // The control: the shape these negatives look for is the one every property takes.
        StringAssert.Contains(generated, " Kind { ");
        Assert.IsFalse(generated.Contains(" Value { "), "a field named like a ж<T> member would silently bind the member");
        Assert.IsFalse(generated.Contains(" _ { "), "a blank field has no address and no accessor");
    }

    [TestMethod]
    public void AnEmbeddedStructGetsItsRefPropertySoAPromotedPathReadsByName()
    {
        StringAssert.Contains(Generate(Package), Ref("global::go.main_package.tail", "tail", Record));
    }

    [TestMethod]
    public void AGenericStructGetsAGenericBlockWithItsConstraints()
    {
        string generated = Generate(Package);

        StringAssert.Contains(generated, "extension<T>(global::go.ж<global::go.main_package.pair<T>> ʗp) where T : struct");
        StringAssert.Contains(generated, Ref("T", "First", "global::go.main_package.pair<T>"));
    }

    // A type another generator emits into the package class (the ImplementGenerator's constraint proxy, SelfRefGenerics'
    // `gnodeжGraphNode`) is unresolved where this generator runs, since generators do not see each other's output.
    // The field names it bare from inside the package class; the extension class sits outside it, so the property's
    // type roots every unresolved name at the struct's package class, at any depth of the type (go2cs.slnx: CS0246).
    private const string Proxied = """
        namespace go;

        [GoFieldAccess(typeof(gnode))]
        partial class main_package
        {
            public partial struct Graph<N> { public N Root; }

            public partial struct gnode { public ж<Graph<gnodeжGraphNode>> g; public gnodeжGraphNode self; public int id; }
        }
        """;

    [TestMethod]
    public void AFieldTypeNamingAnotherGeneratorsTypeIsRootedAtThePackageClass()
    {
        string generated = Generate(Proxied);
        const string Gnode = "global::go.main_package.gnode";

        StringAssert.Contains(generated, Ref("global::go.ж<global::go.main_package.Graph<global::go.main_package.gnodeжGraphNode>>", "g", Gnode));
        StringAssert.Contains(generated, Ref("global::go.main_package.gnodeжGraphNode", "self", Gnode));
        StringAssert.Contains(generated, Ref("int", "id", Gnode), "a resolved field type is rendered as before");
    }

    // A READONLY field is a zero-size layout field, laid over its neighbour at Go's offset: it has no writable slot of
    // its own, and its accessor answers golib's shared zero-size slot. Its property keeps the accessor's body, since
    // `ref ʗp.Value.f` would not compile (CS8329).
    private const string ZeroSize = """
        namespace go;

        [GoFieldAccess(typeof(counter))]
        partial class atomic_package
        {
            public partial struct counter { internal readonly noCopy _noCopy; public readonly noCopy pad; public int v; }

            public partial struct noCopy { }
        }
        """;

    [TestMethod]
    public void AReadonlyFieldKeepsTheAccessorsBody()
    {
        string generated = Generate(ZeroSize);
        const string Counter = "global::go.atomic_package.counter";

        StringAssert.Contains(generated, $"public ref global::go.atomic_package.noCopy pad {{ {Getter} get => ref ʗp.field({Counter}.Ꮡpad); }}");
        StringAssert.Contains(generated, Ref("int", "v", Counter), "the writable sibling takes the specialized body");
    }

    [TestMethod]
    public void EveryAccessorIsMachineryAndInlined()
    {
        string generated = Generate(Package);
        int properties = generated.Split(" get => ").Length - 1;

        Assert.IsTrue(properties > 0, "the fixture generates properties");
        Assert.AreEqual(properties, generated.Split($"{Getter} get => ").Length - 1, "every getter carries [GeneratedCode] and [MethodImpl(AggressiveInlining)]");
    }

    // abi.ArrayType's shape: a field that shadows a method an embedded type promotes. Go selects the field, but the box
    // carries the promoted method as an extension member, and a property beside it would make every `p.Len` ambiguous;
    // the converter keeps the box form for such a field (shadowsPromotedMethod), so the property would serve nothing.
    // The promoted method sits one embed down (Type), two embeds down (through outer's value embed of ArrayType's
    // shape), and behind a POINTER embed; a sibling field that shadows nothing keeps its property, and so does a field
    // whose name matches a method of an unrelated type.
    private const string Shadows = """
        namespace go;

        [GoFieldAccess(typeof(ArrayType), typeof(deeper), typeof(viaPointer), typeof(unrelated))]
        partial class abi_package
        {
            public partial struct Type { public nint Size_; }

            public partial struct ArrayType
            {
                /*embed*/ public Type Type;
                public nint Len;
                public nint Elem;
            }

            public partial struct deeper
            {
                /*embed*/ public ArrayType ArrayType;
                public nint Kind;
                public nint Cap;
            }

            public partial struct viaPointer
            {
                /*embed*/ public ж<Type> Type;
                public nint Len;
            }

            public partial struct other { }

            public partial struct unrelated { public nint Len; }

            public static nint Len(this ж<Type> t) => 0;

            public static nint Kind(this ref Type t) => 0;

            public static nint Len(this ж<other> o) => 0;
        }
        """;

    // internal/poll's fdMutex: a hand-written companion partial (`fd_mutex_impl.cs`) adds fields the Go type does not
    // have. The type generator gives `Ꮡ` accessors only to the fields of its target declaration (the one carrying
    // [GoType], else the converted one), so a companion's field has no accessor to reach it through, and no property.
    private const string ConvertedMutex = """
        namespace go;

        [GoFieldAccess(typeof(fdMutex))]
        partial class poll_package
        {
            internal partial struct fdMutex { internal ulong state; internal uint rsema; }
        }
        """;

    private const string CompanionMutex = """
        namespace go;

        partial class poll_package
        {
            partial struct fdMutex { internal ulong rgate; }
        }
        """;

    [TestMethod]
    public void AFieldOfAHandWrittenCompanionPartialGetsNoProperty()
    {
        string generated = GenerateFiles([("fd_mutex.cs", ConvertedMutex), ("fd_mutex_impl.cs", CompanionMutex)]);
        const string Mutex = "global::go.poll_package.fdMutex";

        StringAssert.Contains(generated, Ref("ulong", "state", Mutex), "the converted declaration's fields keep their properties");
        StringAssert.Contains(generated, Ref("uint", "rsema", Mutex));
        Assert.IsFalse(generated.Contains("rgate"), "a companion's field has no Ꮡ accessor: its property would not compile (CS0117)");
    }

    [TestMethod]
    public void AFieldThatShadowsAPromotedMethodGetsNoProperty()
    {
        string generated = Generate(Shadows);
        const string Package = "global::go.abi_package";

        Assert.IsFalse(generated.Contains($"{Package}.ArrayType.ᏑLen"), "ArrayType.Len shadows the Len its embedded Type promotes");
        Assert.IsFalse(generated.Contains($"{Package}.deeper.ᏑKind"), "deeper.Kind shadows the Kind promoted two embeds down");
        Assert.IsFalse(generated.Contains($"{Package}.viaPointer.ᏑLen"), "a pointer embed promotes its type's methods too");

        StringAssert.Contains(generated, Ref("nint", "Elem", $"{Package}.ArrayType"), "a sibling that shadows nothing keeps its property");
        StringAssert.Contains(generated, Ref("nint", "Cap", $"{Package}.deeper"));
        StringAssert.Contains(generated, Ref("nint", "Len", $"{Package}.unrelated"), "a method of an unrelated type shadows nothing");
    }

    // HEADER-SHAPED structs: an unsafe.Pointer field, then one or two ints, in declaration order. Go's slice and string
    // headers (internal/unsafeheader's Slice and String, runtime's slice and stringStruct, internal/runtime/maps'
    // stringStruct), which golib's header boxes serve over a slice or string variable. Beside them, a struct that only
    // resembles one (a third field that is not an int) and one whose first field is a typed pointer.
    private const string Headers = """
        namespace go;

        [GoFieldAccess(typeof(Slice), typeof(String), typeof(notHeader), typeof(typedHeader))]
        partial class unsafeheader_package
        {
            public partial struct Slice { public unsafe_package.Pointer Data; public nint Len; public nint Cap; }

            public partial struct String { public unsafe_package.Pointer Data; public nint Len; }

            public partial struct notHeader { public unsafe_package.Pointer Data; public nint Len; public uint Cap; }

            public partial struct typedHeader { public ж<byte> Data; public nint Len; public nint Cap; }
        }
        """;

    [TestMethod]
    public void AHeaderShapedStructGetsGetSetPropertiesWhoseSetterCommitsTheHeader()
    {
        // A ref cannot be intercepted, and the header's last store is followed by no access of the box, so the setter is
        // the one point at which golib's header box sees the finished header (ж<T>.OnHeaderStore).
        string generated = Generate(Headers);
        const string Slice = "global::go.unsafeheader_package.Slice";
        const string String = "global::go.unsafeheader_package.String";

        StringAssert.Contains(generated, Header("global::go.unsafe_package.Pointer", "Data", Slice));
        StringAssert.Contains(generated, Header("nint", "Len", Slice));
        StringAssert.Contains(generated, Header("nint", "Cap", Slice));
        StringAssert.Contains(generated, Header("nint", "Len", String));
    }

    // The predicate's arity and order checks (G, review of 94ec9ea487): a header is exactly two or three fields, the
    // pointer FIRST, and every later field a native int. golib's uintptr is a struct, not nint, so a Go uintptr word
    // does not make a header either.
    private const string NearMisses = """
        namespace go;

        [GoFieldAccess(typeof(fourWords), typeof(pointerSecond), typeof(pointerAlone), typeof(uintptrWord))]
        partial class nearmiss_package
        {
            public partial struct fourWords { public unsafe_package.Pointer Data; public nint Len; public nint Cap; public nint Extra; }

            public partial struct pointerSecond { public nint Len; public unsafe_package.Pointer Data; }

            public partial struct pointerAlone { public unsafe_package.Pointer Data; }

            public partial struct uintptrWord { public unsafe_package.Pointer Data; public uintptr Len; }
        }
        """;

    [TestMethod]
    public void ANearMissOfTheHeaderShapeKeepsItsRefProperties()
    {
        string generated = Generate(NearMisses);
        const string Package = "global::go.nearmiss_package";

        StringAssert.Contains(generated, Ref("nint", "Extra", $"{Package}.fourWords"), "four words: one too many");
        StringAssert.Contains(generated, Ref("global::go.unsafe_package.Pointer", "Data", $"{Package}.pointerSecond"), "the pointer second: the order is checked");
        StringAssert.Contains(generated, Ref("global::go.unsafe_package.Pointer", "Data", $"{Package}.pointerAlone"), "the pointer alone: one too few");
        StringAssert.Contains(generated, Ref("global::go.uintptr", "Len", $"{Package}.uintptrWord"), "a uintptr word is golib's struct, not an int");
        Assert.IsFalse(generated.Contains("OnHeaderStore"), "no near miss of the header shape commits anything");
    }

    [TestMethod]
    public void AStructThatIsNotHeaderShapedKeepsItsRefProperties()
    {
        string generated = Generate(Headers);

        StringAssert.Contains(generated, Ref("uint", "Cap", "global::go.unsafeheader_package.notHeader"));
        StringAssert.Contains(generated, Ref("nint", "Len", "global::go.unsafeheader_package.notHeader"));
        StringAssert.Contains(generated, Ref("global::go.ж<byte>", "Data", "global::go.unsafeheader_package.typedHeader"));

        // And every property of an ordinary record is still a ref.
        Assert.IsFalse(Generate(Package).Contains("OnHeaderStore"), "no ordinary struct's property commits anything");
    }

    [TestMethod]
    public void AForeignStructGetsAPropertyOnlyWhereItsAccessorIsVisible()
    {
        string generated = Generate(User, ForeignReference());
        const string Sockaddr = "global::go.syscall_package.Sockaddr";

        StringAssert.Contains(generated, Ref("ushort", "Port", Sockaddr));
        StringAssert.Contains(generated, Ref("ushort", "Family", Sockaddr));
        Assert.IsFalse(generated.Contains("NoAccessor"), "a foreign field with no Ꮡ accessor has nothing to reach it through");
        Assert.IsFalse(generated.Contains("hidden"), "an internal foreign field is not accessible here");
    }

    [TestMethod]
    public void EachPropertyFieldGetsAnAddressPropertyOverItsToken()
    {
        string generated = Generate(Package);

        StringAssert.Contains(generated, Address("uint", "Kind", Record));
        StringAssert.Contains(generated, Address("global::go.array<ushort>", "Path", Record));
        StringAssert.Contains(generated, Address("ushort", "base", Record), "the accessor's name drops the field's escape");
        StringAssert.Contains(generated, Address("global::go.main_package.tail", "tail", Record), "an embedded struct's address too");
        StringAssert.Contains(generated, Address("T", "First", "global::go.main_package.pair<T>"), "a generic struct's token is its instantiation's");
        Assert.IsFalse(generated.Contains(" ᏑValue { "), "a field the converter keeps on the box form gets neither property");
        Assert.IsFalse(generated.Contains(" Ꮡ_ { "), "a blank field has no address");
    }

    [TestMethod]
    public void AReadonlyOrHeaderFieldsAddressIsItsTokensViewToo()
    {
        StringAssert.Contains(Generate(ZeroSize), Address("global::go.atomic_package.noCopy", "pad", "global::go.atomic_package.counter"));
        StringAssert.Contains(Generate(Headers), Address("nint", "Len", "global::go.unsafeheader_package.Slice"));
    }

    // A Go field named `elem` keeps its `p.elem` property, but `p.Ꮡelem` would bind ж<T>'s own Ꮡelem method (an
    // instance member wins over an extension), so the collision list's `Ꮡelem` denies the address property alone.
    private const string Elem = """
        namespace go;

        [GoFieldAccess(typeof(node))]
        partial class list_package
        {
            public partial struct node { public int elem; public int next; }
        }
        """;

    [TestMethod]
    public void AnAddressPropertyNamedLikeABoxMemberIsNotDeclared()
    {
        string generated = Generate(Elem);
        const string Node = "global::go.list_package.node";

        StringAssert.Contains(generated, Ref("int", "elem", Node), "the field property is not the collision");
        StringAssert.Contains(generated, Address("int", "next", Node));
        Assert.IsFalse(generated.Contains(" Ꮡelem { "), "`p.Ꮡelem` would bind ж<T>.Ꮡelem");
    }

    [TestMethod]
    public void AForeignStructsAddressUsesItsTokenOnlyWhereItsMetadataHasOne()
    {
        // An assembly built before seat #32 carries the accessors but no tokens: its address property passes the
        // accessor's method group, today's `.of(T.Ꮡf)` delegate, which golib's cache still matches by its method.
        string generated = Generate(User, ForeignReference());
        const string Sockaddr = "global::go.syscall_package.Sockaddr";

        StringAssert.Contains(generated, Address("ushort", "Port", Sockaddr, token: "ᏑPort"));

        string tokened = Generate(User, ForeignReference(Foreign.Replace(
            "public static ref ushort ᏑPort(ref Sockaddr instance) => ref instance.Port;",
            "public static ref ushort ᏑPort(ref Sockaddr instance) => ref instance.Port;\n" +
            "                public static readonly FieldRefFunc<Sockaddr, ushort> ᏑPort‿ = ᏑPort;") +
            "\npublic delegate ref TField FieldRefFunc<T, TField>(ref T instance);\n"));

        StringAssert.Contains(tokened, Address("ushort", "Port", Sockaddr));
        StringAssert.Contains(tokened, Address("ushort", "Family", Sockaddr, token: "ᏑFamily"), "token presence is read per field");
    }

    // A test assembly that REFERENCES its package's assembly sees the package's internal GoFieldAccess class through
    // InternalsVisibleTo. Two extension properties of one name on one receiver are ambiguous, which C# reports as no
    // member at all (CS9286): runtime's export_test.cs MemclrBytes, `s.Δarray` on a ж<Δsliceᴛ> that alg.cs records
    // too. The package's class is the metadata a C# 14 `extension(ж<slice> ʗp) { public ref nint len => … }` block
    // leaves behind, its implementation method, since this Roslyn cannot compile the block itself.
    private static string GenerateBeside(string package, string user)
    {
        CSharpCompilation packageCompilation = CSharpCompilation.Create("runtime",
            [CSharpSyntaxTree.ParseText(package), CSharpSyntaxTree.ParseText(Stubs)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream image = new();
        Assert.IsTrue(packageCompilation.Emit(image).Success, "the package fixture compiles");

        CSharpCompilation compilation = CSharpCompilation.Create("test",
            [CSharpSyntaxTree.ParseText(user)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromImage(image.ToArray())],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(compilation).GetRunResult();

        Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

        return result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "";
    }

    private const string RuntimePackage = """
        [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("test")]

        namespace go;

        public static partial class runtime_package
        {
            internal partial struct slice
            {
                internal nint len;
                internal nint cap;

                internal static ref nint Ꮡlen(ref slice instance) => ref instance.len;
                internal static ref nint Ꮡcap(ref slice instance) => ref instance.cap;
            }
        }

        internal static class GoFieldAccess
        {
            public static ref nint get_len(ж<runtime_package.slice> ʗp) => throw null!;
        }
        """;

    private const string RuntimeTest = """
        namespace go;

        [GoFieldAccess(typeof(runtime_package.slice))]
        partial class runtime_test_package { }
        """;

    [TestMethod]
    public void APropertyTheReferencedPackageAlreadyGivesIsNotDeclaredAgain()
    {
        string generated = GenerateBeside(RuntimePackage, RuntimeTest);

        StringAssert.Contains(generated, Ref("nint", "cap", "global::go.runtime_package.slice"));
        Assert.IsFalse(generated.Contains(" len { "), "a second `len` on ж<slice> makes the package's own ambiguous: CS9286 at every site");
    }

    [TestMethod]
    public void AnAddressPropertyTheReferencedPackageAlreadyGivesIsNotDeclaredAgain()
    {
        string package = RuntimePackage.Replace(
            "public static ref nint get_len(ж<runtime_package.slice> ʗp) => throw null!;",
            "public static ref nint get_len(ж<runtime_package.slice> ʗp) => throw null!;\n" +
            "    public static ж<nint> get_Ꮡcap(ж<runtime_package.slice> ʗp) => throw null!;");

        string generated = GenerateBeside(package, RuntimeTest);

        StringAssert.Contains(generated, Address("nint", "len", "global::go.runtime_package.slice", token: "Ꮡlen"));
        Assert.IsFalse(generated.Contains(" Ꮡcap { "), "a second `Ꮡcap` on ж<slice> would be CS9286 at every `&s.cap`");
    }

    [TestMethod]
    public void APropertyTheReferencedPackageHidesIsDeclared()
    {
        // The control: with no InternalsVisibleTo grant the package's class is out of reach, so nothing is shadowed.
        // The fixture's struct and accessors are internal too, so they take the grant's place as public.
        string package = RuntimePackage
            .Replace("[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"test\")]", "")
            .Replace("internal partial struct slice", "public partial struct slice")
            .Replace("internal nint", "public nint")
            .Replace("internal static ref", "public static ref");

        string generated = GenerateBeside(package, RuntimeTest);

        StringAssert.Contains(generated, Ref("nint", "len", "global::go.runtime_package.slice"));
        StringAssert.Contains(generated, Ref("nint", "cap", "global::go.runtime_package.slice"));
    }

    // The real thing, end to end: the package's block is a C# 14 extension compiled by the SDK's own compiler, the
    // generator runs over a test assembly that references it, and the SDK compiler then compiles that assembly with
    // its generated class and a `p.len` site. The arms above read the generated text; this one shows CS9286 itself
    // when the property is declared twice. Inconclusive only where no SDK compiler resolves.
    [TestMethod]
    public void ASiteOnAPropertyThePackageGivesCompilesInTheTestAssembly()
    {
        if (SdkCompiler() is not { } compiler)
        {
            Assert.Inconclusive("no SDK csc.dll resolves (DOTNET_HOST_PATH, DOTNET_ROOT, PATH): nothing here can compile a C# 14 extension block");
            return;
        }

        string directory = Directory.CreateTempSubdirectory("go2cs-fieldaccess-").FullName;

        try
        {
            string package = RuntimePackage.Replace(
                "public static ref nint get_len(ж<runtime_package.slice> ʗp) => throw null!;",
                "extension(ж<runtime_package.slice> ʗp) { public ref nint len => ref ʗp.field(runtime_package.slice.Ꮡlen); }");

            Assert.AreNotEqual(RuntimePackage, package, "the fixture's metadata stand-in was replaced by a real block");

            File.WriteAllText(Path.Combine(directory, "package.cs"), package);
            File.WriteAllText(Path.Combine(directory, "stubs.cs"), Stubs);

            (int code, string output) = Compile(compiler, directory, "runtime.dll", ["package.cs", "stubs.cs"], []);
            Assert.AreEqual(0, code, $"the package fixture compiles under the SDK compiler:\n{output}");

            CSharpCompilation compilation = CSharpCompilation.Create("test",
                [CSharpSyntaxTree.ParseText(RuntimeTest)],
                [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(Path.Combine(directory, "runtime.dll"))],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(compilation).GetRunResult();
            Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

            File.WriteAllText(Path.Combine(directory, "test.cs"), RuntimeTest);
            File.WriteAllText(Path.Combine(directory, "generated.cs"), result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "");
            File.WriteAllText(Path.Combine(directory, "site.cs"), """
                namespace go;

                static class site
                {
                    static nint read(ж<runtime_package.slice> s) => s.len + s.cap + s.Ꮡcap.Value;
                }
                """);

            (code, output) = Compile(compiler, directory, "test.dll", ["test.cs", "generated.cs", "site.cs"], ["runtime.dll"]);
            Assert.AreEqual(0, code, $"`s.len`, `s.cap` and `s.Ꮡcap` bind in the test assembly:\n{output}");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    // The SDK's csc.dll beside the dotnet host, newest SDK first: (host, csc.dll).
    private static (string Host, string Csc)? SdkCompiler()
    {
        string exe = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
        List<string> hosts = [];

        if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host)
            hosts.Add(host);

        if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is { Length: > 0 } root)
            hosts.Add(Path.Combine(root, exe));

        hosts.AddRange((Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator).Where(dir => dir.Length > 0).Select(dir => Path.Combine(dir, exe)));

        foreach (string candidate in hosts.Where(File.Exists))
        {
            string sdk = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(candidate))!, "sdk");

            if (!Directory.Exists(sdk))
                continue;

            string? csc = Directory.GetDirectories(sdk)
                .Select(version => Path.Combine(version, "Roslyn", "bincore", "csc.dll"))
                .Where(File.Exists)
                .OrderByDescending(path => Version.TryParse(Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path)))!.Split('-')[0]), out Version? version) ? version : new Version())
                .FirstOrDefault();

            if (csc is not null)
                return (candidate, csc);
        }

        return null;
    }

    // Compiles a library with the SDK compiler against this runtime's own framework assemblies.
    private static (int Code, string Output) Compile((string Host, string Csc) compiler, string directory, string output, string[] sources, string[] references)
    {
        string framework = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        List<string> arguments = [compiler.Csc, "-nologo", "-noconfig", "-nostdlib+", "-target:library", "-langversion:14", "-nullable:annotations", $"-out:{output}",
            $"-r:{Path.Combine(framework, "System.Private.CoreLib.dll")}", $"-r:{Path.Combine(framework, "System.Runtime.dll")}"];

        arguments.AddRange(references.Select(reference => $"-r:{reference}"));
        arguments.AddRange(sources);

        ProcessStartInfo start = new(compiler.Host, arguments)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using Process process = Process.Start(start)!;
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(120_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"csc {output} did not finish in 120 s");
        }

        return (process.ExitCode, stdout + stderr);
    }

    // The converter writes the records in package_info.cs, on the assembly (parity seat #31); the class form above is
    // the committed corpus's until its regeneration. Both are read, and a type both list gets one block.
    private const string AssemblyRecords = """
        [assembly: global::go.GoFieldAccess(typeof(global::go.main_package.point))]
        [assembly: global::go.GoFieldAccess(typeof(global::go.main_package.pair<>))]

        namespace go;

        partial class main_package
        {
            public partial struct point { public int X, Y; }

            public partial struct pair<T> where T : struct { public T First; public int Count; }
        }

        [GoFieldAccess(typeof(main_package.point))]
        partial class main_package { }
        """;

    [TestMethod]
    public void AnAssemblyRecordGetsItsPropertiesBesideTheClassForm()
    {
        string generated = Generate(AssemblyRecords);

        StringAssert.Contains(generated, Ref("int", "X", "global::go.main_package.point"));
        StringAssert.Contains(generated, Ref("T", "First", "global::go.main_package.pair<T>"));
        Assert.AreEqual(1, generated.Split("extension(global::go.ж<global::go.main_package.point> ʗp)").Length - 1, "a type both forms list gets one block");

        string assemblyOnly = AssemblyRecords.Replace("[GoFieldAccess(typeof(main_package.point))]", "");
        Assert.AreNotEqual(AssemblyRecords, assemblyOnly, "the fixture's class record was removed");
        StringAssert.Contains(Generate(assemblyOnly), Ref("int", "Y", "global::go.main_package.point"), "the assembly record alone generates");
    }

    [TestMethod]
    public void TheClassIsInternalSoTwoPackagesExtendingOneTypeNeverMeet()
    {
        StringAssert.Contains(Generate(Package), "internal static class GoFieldAccess");
    }

    [TestMethod]
    public void NoRecordGeneratesNothing()
    {
        Assert.AreEqual("", Generate(Package.Replace("[GoFieldAccess(typeof(record))]", "").Replace("[GoFieldAccess(typeof(main_package.pair<>))]", "")));
    }

    /// <summary>
    /// The guard on the collision list: every public instance member of golib's <c>ж&lt;T&gt;</c>, and every golib
    /// extension member whose receiver is a <c>ж&lt;…&gt;</c>, must be on it. C# binds an instance member before an
    /// extension property, so a Go field named like a member the list lacks would silently bind the member.
    /// </summary>
    [TestMethod]
    public void TheCollisionListNamesEveryMemberOfTheBox()
    {
        Type box = typeof(golib::go.ж<>);
        HashSet<string> names = [];

        foreach (MemberInfo member in box.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            if (member is System.Reflection.MethodInfo { IsSpecialName: true })
                continue;

            names.Add(member.Name);
        }

        foreach (Type type in box.Assembly.GetExportedTypes().Where(type => type.IsAbstract && type.IsSealed))
        {
            foreach (System.Reflection.MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!method.IsDefined(typeof(ExtensionAttribute), false) || method.GetParameters() is not [var receiver, ..])
                    continue;

                if (IsBox(receiver.ParameterType))
                    names.Add(method.Name);
            }
        }

        string[] missing = names.Where(name => !FieldAccessGenerator.CollisionNames.Contains(name)).OrderBy(name => name, StringComparer.Ordinal).ToArray();

        Assert.AreEqual(0, missing.Length, $"src/go2cs/fieldAccessCollisions.txt lacks: {string.Join(", ", missing)}");
        Assert.IsTrue(names.Contains("Value") && names.Contains("of"), "the guard must see the box's own members");

        static bool IsBox(Type type)
        {
            for (Type? current = type; current is not null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(golib::go.ж<>))
                    return true;
            }

            return false;
        }
    }

    // Seat #45 (class B): a FOREIGN struct whose field is typed with a THIRD assembly's struct, which this compilation
    // does not reference. encoding/json's tests record httptest.Server, whose TLS field is a *tls.Config, and the json
    // test assembly references net/http/httptest but not crypto/tls: the field's type is unresolved there.
    private const string Tls = """
        namespace go;

        public static partial class tls_package
        {
            public partial struct Config { public int MinVersion; }
        }
        """;

    private const string Httptest = """
        namespace go;

        public static partial class httptest_package
        {
            public partial struct Server
            {
                public int Port;
                public ж<tls_package.Config> TLS;
                public array<tls_package.Config> Configs;

                public static ref int ᏑPort(ref Server instance) => ref instance.Port;
                public static ref ж<tls_package.Config> ᏑTLS(ref Server instance) => ref instance.TLS;
                public static ref array<tls_package.Config> ᏑConfigs(ref Server instance) => ref instance.Configs;
            }
        }
        """;

    private const string HttptestUser = """
        namespace go;

        [GoFieldAccess(typeof(httptest_package.Server))]
        partial class json_package { }
        """;

    // The user compiled against golib's stubs and httptest, and against tls only when referenceTls is set.
    private static string GenerateOverThreeAssemblies(bool referenceTls)
    {
        MetadataReference corlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        MetadataReference Emit(string name, string source, params MetadataReference[] references)
        {
            CSharpCompilation compilation = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source)], [corlib, .. references],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using MemoryStream image = new();
            Microsoft.CodeAnalysis.Emit.EmitResult emitted = compilation.Emit(image);
            Assert.IsTrue(emitted.Success, $"the {name} fixture compiles: {string.Join("\n", emitted.Diagnostics)}");

            return MetadataReference.CreateFromImage(image.ToArray());
        }

        MetadataReference stubs = Emit("golib", Stubs);
        MetadataReference tls = Emit("tls", Tls, stubs);
        MetadataReference httptest = Emit("httptest", Httptest, stubs, tls);

        CSharpCompilation user = CSharpCompilation.Create("json",
            [CSharpSyntaxTree.ParseText(HttptestUser)],
            referenceTls ? [corlib, stubs, httptest, tls] : [corlib, stubs, httptest],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(user).GetRunResult();

        Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

        return result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "";
    }

    [TestMethod]
    public void AForeignFieldTypedFromAnUnreferencedAssemblyGetsNoProperty()
    {
        const string Server = "global::go.httptest_package.Server";
        string generated = GenerateOverThreeAssemblies(referenceTls: false);

        StringAssert.Contains(generated, Ref("int", "Port", Server));

        // The field's type cannot be named here at all: rooted at the struct's own package class it read
        // `global::go.httptest_package.Config` (CS0426), and spelled right it would be CS0012. The field is unreachable
        // as typed, so it gets no property.
        Assert.IsFalse(generated.Contains("httptest_package.Config"), $"an unresolved foreign field type is never rooted at the struct's package:\n{generated}");
        Assert.IsFalse(generated.Contains(" TLS "), $"a field typed from an unreferenced assembly gets no property:\n{generated}");
        Assert.IsFalse(generated.Contains(" Configs "), $"nor does one whose element type is:\n{generated}");

        // The control: with the third assembly referenced, the same field gets its property, its type qualified by
        // its own package.
        string referenced = GenerateOverThreeAssemblies(referenceTls: true);

        StringAssert.Contains(referenced, Ref("global::go.ж<global::go.tls_package.Config>", "TLS", Server));
        StringAssert.Contains(referenced, Ref("global::go.array<global::go.tls_package.Config>", "Configs", Server));
    }

    // The same three assemblies under the SDK's compiler, with the converter's spelling of a consumer's `s.TLS != nil`
    // (P1, fieldPropertySelector: it reads the field through the property whatever the consumer imports). A production
    // project references its packages transitively, so the third assembly is in view and the property serves the site.
    // A -tests project does not (DisableTransitiveProjectReferences): there the generated class must still compile, and
    // the site compiles under no spelling at all, the whole value's `s.Value.TLS` included (CS0012), so no converter
    // fallback could serve it; such a consumer needs the reference. Inconclusive only where no SDK compiler resolves.
    [TestMethod]
    public void AConsumerOfAForeignFieldCompilesWhereItsTypeIsReferenced()
    {
        if (SdkCompiler() is not { } compiler)
        {
            Assert.Inconclusive("no SDK csc.dll resolves (DOTNET_HOST_PATH, DOTNET_ROOT, PATH): nothing here can compile a C# 14 extension block");
            return;
        }

        string directory = Directory.CreateTempSubdirectory("go2cs-fieldaccess-").FullName;

        try
        {
            void Library(string output, string source, params string[] references)
            {
                File.WriteAllText(Path.Combine(directory, $"{output}.cs"), source);
                (int code, string log) = Compile(compiler, directory, $"{output}.dll", [$"{output}.cs"], references);
                Assert.AreEqual(0, code, $"the {output} fixture compiles under the SDK compiler:\n{log}");
            }

            Library("golib", Stubs);
            Library("tls", Tls, "golib.dll");
            Library("httptest", Httptest, "golib.dll", "tls.dll");

            (int Code, string Output) Consumer(bool referenceTls, string site)
            {
                string[] references = referenceTls ? ["golib.dll", "httptest.dll", "tls.dll"] : ["golib.dll", "httptest.dll"];

                CSharpCompilation compilation = CSharpCompilation.Create("json",
                    [CSharpSyntaxTree.ParseText(HttptestUser)],
                    [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), .. references.Select(reference => MetadataReference.CreateFromFile(Path.Combine(directory, reference)))],
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(new FieldAccessGenerator()).RunGenerators(compilation).GetRunResult();
                Assert.AreEqual(0, result.Diagnostics.Length, string.Join("\n", result.Diagnostics));

                File.WriteAllText(Path.Combine(directory, "user.cs"), HttptestUser);
                File.WriteAllText(Path.Combine(directory, "generated.cs"), result.GeneratedTrees.SingleOrDefault()?.ToString() ?? "");
                File.WriteAllText(Path.Combine(directory, "site.cs"), $$"""
                    namespace go;

                    static class site
                    {
                        static bool read(ж<httptest_package.Server> s) => {{site}};
                    }
                    """);

                return Compile(compiler, directory, $"json-{referenceTls}.dll", ["user.cs", "generated.cs", "site.cs"], references);
            }

            (int code, string output) = Consumer(referenceTls: true, "s.TLS != null && s.Port > 0");
            Assert.AreEqual(0, code, $"with the field's assembly referenced, `s.TLS` binds the generated property:\n{output}");

            (code, output) = Consumer(referenceTls: false, "s.Port > 0");
            Assert.AreEqual(0, code, $"without it, the generated class still compiles and serves the struct's other fields:\n{output}");

            (code, output) = Consumer(referenceTls: false, "s.TLS != null");
            Assert.AreNotEqual(0, code, "without it, the field has no property");
            StringAssert.Contains(output, "CS1061");

            (code, output) = Consumer(referenceTls: false, "s.Value.TLS != null");
            Assert.AreNotEqual(0, code, "nor does the whole value's field compile: its type is in no referenced assembly");
            StringAssert.Contains(output, "CS0012");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    // Seat #31b: a PROMOTED field reads `p.f` too. Go promotes a name from the shallowest embed depth at which exactly one
    // field carries it, unless the struct declares it itself; an ambiguity at that depth promotes nothing. The property
    // walks the embed hops itself, over members this generator can see, never T's own generated promoted members (the
    // type generator's set is not Go's: it does not chase a cross-package embed's own embeds).
    private const string Promoted = """
        namespace go;

        [GoFieldAccess(typeof(top), typeof(viaPtr), typeof(clash), typeof(shallow))]
        partial class main_package
        {
            public partial struct leaf { public nint n; public nint shared; }

            public partial struct mid
            {
                /*embed*/ public leaf leaf;
                public nint m;
            }

            public partial struct top
            {
                /*embed*/ public mid mid;
                public nint t;
            }

            public partial struct viaPtr
            {
                /*embed*/ public ж<leaf> leaf;
                public nint k;
            }

            public partial struct other { public nint shared; public nint only; }

            public partial struct clash
            {
                /*embed*/ public leaf leaf;
                /*embed*/ public other other;
            }

            public partial struct shallow
            {
                /*embed*/ public leaf leaf;
                public nint n;
            }
        }
        """;

    // A value chain: the managed box reaches the field through the embeds in place; a NativeBox<T> takes each hop's view
    // at its Go offset and the field at its own.
    private static string PromotedRef(string fieldType, string name, string type, string native, string managed) =>
        $"public ref {fieldType} {name} {{ {Getter} get => ref ʗp is global::go.NativeBox<{type}> ? ref {native} : ref ʗp.Value.{managed}; }}";

    [TestMethod]
    public void APromotedFieldWalksItsValueEmbedsToItsGoOffset()
    {
        string generated = Generate(Promoted);
        const string Main = "global::go.main_package";

        StringAssert.Contains(generated, PromotedRef("nint", "n", $"{Main}.top",
            $"ʗp.of({Main}.top.Ꮡmid).of({Main}.mid.Ꮡleaf).field({Main}.leaf.Ꮡn)", "mid.leaf.n"), "two embeds down");
        StringAssert.Contains(generated, PromotedRef("nint", "m", $"{Main}.top",
            $"ʗp.of({Main}.top.Ꮡmid).field({Main}.mid.Ꮡm)", "mid.m"), "one embed down");
        StringAssert.Contains(generated, PromotedRef($"{Main}.leaf", "leaf", $"{Main}.top",
            $"ʗp.of({Main}.top.Ꮡmid).field({Main}.mid.Ꮡleaf)", "mid.leaf"), "an embed is a field, and is promoted by its name");
        StringAssert.Contains(generated, Ref("nint", "t", $"{Main}.top"), "the struct's own field keeps its property");
    }

    [TestMethod]
    public void APromotedFieldThroughAPointerEmbedReadsTheManagedPathAlone()
    {
        // The field lives in another allocation, which T's memory holds no offset into: the property is exactly today's
        // `p.Value.f` storage, through the embed's own box and its nil check.
        StringAssert.Contains(Generate(Promoted),
            $"public ref nint n {{ {Getter} get => ref ʗp.Value.leaf.Value.n; }}");
    }

    [TestMethod]
    public void APromotedFieldFollowsGosAmbiguityAndShadowingRules()
    {
        string generated = Generate(Promoted);
        const string Main = "global::go.main_package";

        Assert.IsFalse(generated.Contains($"NativeBox<{Main}.clash> ? ref ʗp.of({Main}.clash.Ꮡleaf).field({Main}.leaf.Ꮡshared)") ||
                       generated.Contains($"NativeBox<{Main}.clash> ? ref ʗp.of({Main}.clash.Ꮡother).field({Main}.other.Ꮡshared)"),
            "two embeds at one depth both carry `shared`: Go promotes neither");
        StringAssert.Contains(generated, PromotedRef("nint", "only", $"{Main}.clash",
            $"ʗp.of({Main}.clash.Ꮡother).field({Main}.other.Ꮡonly)", "other.only"), "a name unique at its depth is promoted");
        StringAssert.Contains(generated, Ref("nint", "n", $"{Main}.shallow"), "the struct's own `n` is the one Go selects");
        Assert.IsFalse(generated.Contains($"ʗp.of({Main}.shallow.Ꮡleaf).field({Main}.leaf.Ꮡn)"), "an own field shadows the promoted one");
        StringAssert.Contains(generated, PromotedRef("nint", "shared", $"{Main}.shallow",
            $"ʗp.of({Main}.shallow.Ꮡleaf).field({Main}.leaf.Ꮡshared)", "leaf.shared"));
    }

    // The same shapes compiled to metadata: a FOREIGN owner's promoted field is walked over its metadata embeds, by the
    // Ꮡ accessors its assembly carries.
    private const string ForeignPromoted = """
        namespace go;

        public static partial class inner_package
        {
            public partial struct Base
            {
                public nint ID;

                public static ref nint ᏑID(ref Base instance) => ref instance.ID;
            }

            public partial struct Outer
            {
                [GoEmbedded] public Base Base;
                public nint Name;

                public static ref Base ᏑBase(ref Outer instance) => ref instance.Base;
                public static ref nint ᏑName(ref Outer instance) => ref instance.Name;
            }
        }

        public sealed class GoEmbeddedAttribute : System.Attribute { }
        """;

    private const string ForeignPromotedUser = """
        namespace go;

        [GoFieldAccess(typeof(inner_package.Outer))]
        partial class main_package { }
        """;

    [TestMethod]
    public void AForeignOwnersPromotedFieldWalksItsMetadataEmbeds()
    {
        string generated = Generate(ForeignPromotedUser, ForeignReference(ForeignPromoted));
        const string Inner = "global::go.inner_package";

        StringAssert.Contains(generated, PromotedRef("nint", "ID", $"{Inner}.Outer",
            $"ʗp.of({Inner}.Outer.ᏑBase).field({Inner}.Base.ᏑID)", "Base.ID"));
    }
}
