// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
using io = go.io_package;
using textproto = go.net.textproto_package;
// </ImportedTypeAliases>

using go;
using static go.mime.multipart_package;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). Resolving each duck-typed cast at compile time
// this way is what keeps startup free of reflection.

// <InterfaceImplementations>
[assembly: GoImplement<Part, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<bytes_package.Reader, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<go.mime.quotedprintable_package.Reader, io_package.Reader>(Pointer = true)]
[assembly: GoImplement<os_package.File, File>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.Closer>(Pointer = true)]
[assembly: GoImplement<os_package.File, io_package.ReaderAt>(Pointer = true)]
[assembly: GoImplement<part, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<partReader, io_package.Reader>]
[assembly: GoImplement<readForm_writerOnly, io_package.Writer>(Promoted = true)]
[assembly: GoImplement<readForm_writerOnly, io_package.Writer>]
[assembly: GoImplement<sectionReadCloser, File>]
[assembly: GoImplement<sectionReadCloser, io_package.Closer>(Promoted = true)]
[assembly: GoImplement<stickyErrorReader, io_package.Reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(FileHeader))]
[assembly: GoFieldAccess(typeof(Form))]
[assembly: GoFieldAccess(typeof(Part))]
[assembly: GoFieldAccess(typeof(Reader))]
[assembly: GoFieldAccess(typeof(Writer))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("mime/multipart/formdata.go", "formdata.cs", "ABQoAAEYAAoCqIIACgbigpiCgoKmgoKCgqaUkoKCpoKCgriCgoIAESSClIKClIKClIKCgoKUgpSClJSCgpSasoKCpqaUlIKClIKClIKokoKCgoKUgpS4goKUgoKCgqaCgpSCloSClIKCgoKCgpSmgoKClKbmooKCgoKCpgAMHJKCgoKCgoLKAA0gkoKClIKCgpSClAAVKIKClA==", "62-81:1", "", "", "", "75:21,76:13")]
[assembly: GoPositionMap("mime/multipart/multipart.go", "multipart.cs", "AC9YAB1AxoKUgpTssoKUgoK45oKCgoKCAAMU4oIAEiaigpSCAAoGgriClKaCgoKCpqaCgoKCpoKUqqIACBKCgriCgoKUgoLcgpSCgpSCgoKClAACGgAKApSClKSkpoLKgpSkpKaC7oKClAACHgAMAoKClJSUgriCgpSUlIK4poKCABc0lIKCgoKmAAIS4gACENKmooKUgpSCgpTclIKmgoKCgpSCppSmgqaU7oKCpr6ygpSCgqbugpSCyoKClK7CgpQ=", "", "", "133=NewReaderSize/1/1/1")]
[assembly: GoPositionMap("mime/multipart/readmimeheader.go", "readmimeheader.cs", "AAse")]
[assembly: GoPositionMap("mime/multipart/writer.go", "writer.cs", "ABs4ot6SAAoS4oKmgpSCgoKUlLSCtpSCqqKmgpSmgoKCgpSu4oKCpoKClKaCgqaCgoKUpoKmAAEcAAwCAAgKooKCgqqigpSqorqSgoKUgqqigoKUlIIACBKCguaygpSCgpQ=", "", "", "31=randomBoundary/1/1/2;180=escapeQuotes/1/1/1,Sprintf/1/1/1;188=escapeQuotes/1/2/1,escapeQuotes/2/2/1")]
// </GoSourcePositionMaps>

namespace go.mime;

using static go.mime.multipart_package;
using io = go.io_package;
using textproto = go.net.textproto_package;

[GoPackage("multipart")]
public static partial class multipart_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial struct part {}
    internal partial struct partReader {}
    [GoLocalName("writerOnly")] internal partial struct readForm_writerOnly {}
    internal partial struct sectionReadCloser {}
    internal partial struct stickyErrorReader {}
    public partial interface File {}
    public partial struct FileHeader {}
    public partial struct Form {}
    public partial struct Part {}
    public partial struct Reader {}
    public partial struct Writer {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸbytes() => builtin.initPackage(typeof(bytes_package));
    [GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() => builtin.initPackage(typeof(crypto.rand_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    [GoInit] internal static void initᴛᴛimportꓸinternalꓸgodebug() => builtin.initPackage(typeof(@internal.godebug_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmaps() => builtin.initPackage(typeof(maps_package));
    [GoInit] internal static void initᴛᴛimportꓸmath() => builtin.initPackage(typeof(math_package));
    [GoInit] internal static void initᴛᴛimportꓸmime() => builtin.initPackage(typeof(mime_package));
    [GoInit] internal static void initᴛᴛimportꓸmimeꓸquotedprintable() => builtin.initPackage(typeof(go.mime.quotedprintable_package));
    [GoInit] internal static void initᴛᴛimportꓸnetꓸtextproto() => builtin.initPackage(typeof(net.textproto_package));
    [GoInit] internal static void initᴛᴛimportꓸos() => builtin.initPackage(typeof(os_package));
    [GoInit] internal static void initᴛᴛimportꓸpathꓸfilepath() => builtin.initPackage(typeof(path.filepath_package));
    [GoInit] internal static void initᴛᴛimportꓸslices() => builtin.initPackage(typeof(slices_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸstrings() => builtin.initPackage(typeof(strings_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(multipart_package.FileHeader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛFileHeader;
[GoMethodHost(typeof(multipart_package.Form)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛForm;
[GoMethodHost(typeof(multipart_package.Part)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛPart;
[GoMethodHost(typeof(multipart_package.Reader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛReader;
[GoMethodHost(typeof(multipart_package.Writer)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛWriter;
[GoMethodHost(typeof(multipart_package.part)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛpart;
[GoMethodHost(typeof(multipart_package.partReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛpartReader;
[GoMethodHost(typeof(multipart_package.sectionReadCloser)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛsectionReadCloser;
[GoMethodHost(typeof(multipart_package.stickyErrorReader)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class multipartᴛstickyErrorReader;
// </MethodHolders>

// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
// the body's frame as the Go method.
// <MethodForwarders>
partial class multipartᴛFileHeader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (File, error) Open(this ref FileHeader fh) => multipart_package.Open⁔(ref fh);
}
partial class multipartᴛForm {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error RemoveAll(this ref Form f) => multipart_package.RemoveAll⁔(ref f);
}
partial class multipartᴛPart {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FormName(this ref Part p) => multipart_package.FormName⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FileName(this ref Part p) => multipart_package.FileName⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static void parseContentDisposition(this ref Part p) => multipart_package.parseContentDisposition⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error populateHeaders(this ref Part p, int64 maxMIMEHeaderSize, int64 maxMIMEHeaders) => multipart_package.populateHeaders⁔(ref p, maxMIMEHeaderSize, maxMIMEHeaders);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (nint n, error err) Read(this ref Part p, slice<byte> d) => multipart_package.Read⁔(ref p, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ж<Part> Ꮡp) => multipart_package.Close⁔(Ꮡp);
}
partial class multipartᴛReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Form>, error) ReadForm(this ж<Reader> Ꮡr, int64 maxMemory) => multipart_package.ReadForm⁔(Ꮡr, maxMemory);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Form>, error err) readForm(this ж<Reader> Ꮡr, int64 maxMemory) => multipart_package.readForm⁔(Ꮡr, maxMemory);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Part>, error) NextPart(this ж<Reader> Ꮡr) => multipart_package.NextPart⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (ж<Part>, error) NextRawPart(this ж<Reader> Ꮡr) => multipart_package.NextRawPart⁔(Ꮡr);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (ж<Part>, error) nextPart(this ж<Reader> Ꮡr, bool rawPart, int64 maxMIMEHeaderSize, int64 maxMIMEHeaders) => multipart_package.nextPart⁔(Ꮡr, rawPart, maxMIMEHeaderSize, maxMIMEHeaders);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool isFinalBoundary(this ref Reader r, slice<byte> line) => multipart_package.isFinalBoundary⁔(ref r, line);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static bool /*ret*/ isBoundaryDelimiterLine(this ref Reader r, slice<byte> line) => multipart_package.isBoundaryDelimiterLine⁔(ref r, line);
}
partial class multipartᴛWriter {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string Boundary(this ref Writer w) => multipart_package.Boundary⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error SetBoundary(this ref Writer w, @string boundary) => multipart_package.SetBoundary⁔(ref w, boundary);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static @string FormDataContentType(this ref Writer w) => multipart_package.FormDataContentType⁔(ref w);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.Writer, error) CreatePart(this ж<Writer> Ꮡw, textproto.MIMEHeader header) => multipart_package.CreatePart⁔(Ꮡw, header);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.Writer, error) CreateFormFile(this ж<Writer> Ꮡw, @string fieldname, @string filename) => multipart_package.CreateFormFile⁔(Ꮡw, fieldname, filename);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static (io.Writer, error) CreateFormField(this ж<Writer> Ꮡw, @string fieldname) => multipart_package.CreateFormField⁔(Ꮡw, fieldname);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error WriteField(this ж<Writer> Ꮡw, @string fieldname, @string value) => multipart_package.WriteField⁔(Ꮡw, fieldname, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static error Close(this ref Writer w) => multipart_package.Close⁔(ref w);
}
partial class multipartᴛpart {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error close(this ref part p) => multipart_package.close⁔(ref p);
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error err) Write(this ref part p, slice<byte> d) => multipart_package.Write⁔(ref p, d);
}
partial class multipartᴛpartReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint, error) Read(this partReader pr, slice<byte> d) => multipart_package.Read⁔(pr, d);
}
partial class multipartᴛsectionReadCloser {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static error Close(this sectionReadCloser rc) => multipart_package.Close⁔(rc);
}
partial class multipartᴛstickyErrorReader {
    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] internal static (nint n, error) Read(this ref stickyErrorReader r, slice<byte> p) => multipart_package.Read⁔(ref r, p);
}
// </MethodForwarders>
