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
// </ImportedTypeAliases>

using go;
using static go.image_package;

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
[assembly: GoImplement<Alpha, Image>(Pointer = true)]
[assembly: GoImplement<Alpha, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Alpha16, Image>(Pointer = true)]
[assembly: GoImplement<Alpha16, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<CMYK, Image>(Pointer = true)]
[assembly: GoImplement<CMYK, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Gray, Image>(Pointer = true)]
[assembly: GoImplement<Gray, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Gray16, Image>(Pointer = true)]
[assembly: GoImplement<Gray16, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<NRGBA, Image>(Pointer = true)]
[assembly: GoImplement<NRGBA, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<NRGBA64, Image>(Pointer = true)]
[assembly: GoImplement<NRGBA64, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<NYCbCrA, Image>(Pointer = true)]
[assembly: GoImplement<Paletted, Image>(Pointer = true)]
[assembly: GoImplement<Paletted, PalettedImage>(Pointer = true)]
[assembly: GoImplement<Paletted, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<RGBA, Image>(Pointer = true)]
[assembly: GoImplement<RGBA, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<RGBA64, Image>(Pointer = true)]
[assembly: GoImplement<RGBA64, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Rectangle, Image>(Pointer = true)]
[assembly: GoImplement<Rectangle, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Rectangle, RGBA64Image>]
[assembly: GoImplement<Uniform, Image>(Pointer = true)]
[assembly: GoImplement<Uniform, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<Uniform, go.image.color_package.Model>(Pointer = true)]
[assembly: GoImplement<YCbCr, Image>(Pointer = true)]
[assembly: GoImplement<YCbCr, RGBA64Image>(Pointer = true)]
[assembly: GoImplement<bufio_package.Reader, reader>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// This package's string literals are hoisted to `static readonly` fields (the standard
// library's form, or -hoist-literals), so no converted file passes a bare literal for
// go2cs-gen's literal interceptor to box. The stamp below tells the generator to skip
// the assembly.
[assembly: GoHoistedLiterals]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("image/format.go", "format.cs", "AA8gAA4q4oKCggAJFJKClKiSgpSCgqaokoKCgoKmrsKCgoKUgq7CgoKClII=")]
[assembly: GoPositionMap("image/geom.go", "geom.cs", "ABUmkqiSqJKokqiSqJKsopKCgoKUgoKUqJIAChKSABAgkqiSqJKokt6S3pIABRKygoKUgpSCgpSClKqigpSClIKUgtyClKiSgpSClIKUgpSClIKUqJKqoqiSrJKCuKyigpSClKiSgpSokoKUqJKokgAKFrKClIKUqqKClIKClIKClIKClKqigpSCgpQ=")]
[assembly: GoPositionMap("image/image.go", "image.cs", "AFu+AfKCgpQADxyUlIKmgoKUgoKCgoKCAAcQgoKUgoKqoqaCgpSCgoKCgoKmgoKUgoKCgoKmgoKUgoKCgoKqoriClIIABhCSgpSSgoKCpoKUAAgIkgATJJSUgqaCgpSCggAHFKKmgoKUgoKCgoKCgoKCgqaCgpSCgoKCgoKCgoKqoriClIIABhCSgpSSgoKCpoKUAAgIkgATJJSUgqaCgqaCgpSCgqqipoKClIKCgoKCgqaCgpSCgoKClIKCgoKCpoKClIKCgoKCqqK4gpSCAAYQkoKUkoKCgqaClAAICJIAEySUlIKmgoKmgoKUgoIABxSipoKClIKCgoKCgoKCgoKmgoKUgoKCgpSCgoKCgoKCgoKmgoKUgoKCgoKCgoKCqqK4gpSCAAYQkoKUkoKCgqaClAAICJIAEySUlIKmgoKCpoKClIKqoqaCgpSCpoKClIKmgoKUgqqiuIKUggAGEJKClJKCgoKmgpQACAiSABMklJSCpoKCpoKClIKqoqaCgpSCgoKmgoKUgoKmgoKUgoKqoriClIIABhCSgpSSgoKCpoKUAAgIkgATJJSUgqaCgoKmgoKUgqqipoKClIKmgoKmgoKmgoKUgqqiuIKUggAGEJIACAiSABMklJSCpoKCpoKClIKqoqaCgpSCgoKmgoKmgoKCpoKClIKCqqK4gpSCAAYQkgAICJIAEySUlIKmgoKmgoKUgoKqoqaCgpSCgoKCgoKmgoKUgoKCgoKCpoKClIKCgoKCqqK4gpSCAAYQkgAICJIAFSiUlIKClIKUgqaCgpSCgpSClIIABxSipoKClIKmgoKUgqaCgpSCpoKClIKqoriCuIIABxKSgpKCgpSClIKClIKCpgAICqI=", "", "", "243=pixelBufferLength/1/1/1,Dx/1/1/2;369=pixelBufferLength/1/1/1,Dx/1/1/2;505=pixelBufferLength/1/1/1,Dx/1/1/2;658=pixelBufferLength/1/1/1,Dx/1/1/2;775=pixelBufferLength/1/1/1,Dx/1/1/2;895=pixelBufferLength/1/1/1,Dx/1/1/2;1000=pixelBufferLength/1/1/1,Dx/1/1/2;1108=pixelBufferLength/1/1/1,Dx/1/1/2;1225=pixelBufferLength/1/1/1,Dx/1/1/2;1333=Intersect/1/1/3;1374=pixelBufferLength/1/1/1,Dx/1/1/2")]
[assembly: GoPositionMap("image/names.go", "names.cs", "AAwalJSUAAkSgqaCpoKmlJSCgqiSgsiS")]
[assembly: GoPositionMap("image/ycbcr.go", "ycbcr.cs", "ACUugpSkpKSkpJQAHDKCpoKmgqaCgqaCgpSCggAGEqKqopSkpKSkpqqiuIK4goIAChaCxtKClIK0grSCtIK0gsaCpKqixqiCpoKCgoIAFSiCpoKmgoKmgoKUgoKCAAkYoqqiuILcgoKCAA4gkoKUkoKCgqaClMqitqiCpoKCgoKC", "", "", "227=mul3NonNeg/1/2/1,mul3NonNeg/2/2/2;356=mul3NonNeg/1/2/1,mul3NonNeg/2/2/2")]
// </GoSourcePositionMaps>

namespace go;

// <GlyphForwarderUsings> (scratch prototype: the using directives the forwarders' signatures read)
using color = image.color_package;
using image;
using math;
using static image_package;
// </GlyphForwarderUsings>

[GoPackage("image")]
public static partial class image_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    internal partial interface reader {}
    internal partial struct format {}
    public partial interface Image {}
    public partial interface PalettedImage {}
    public partial interface RGBA64Image {}
    public partial struct Alpha {}
    public partial struct Alpha16 {}
    public partial struct CMYK {}
    public partial struct Config {}
    public partial struct Gray {}
    public partial struct Gray16 {}
    public partial struct NRGBA {}
    public partial struct NRGBA64 {}
    public partial struct NYCbCrA {}
    public partial struct Paletted {}
    public partial struct Point {}
    public partial struct RGBA {}
    public partial struct RGBA64 {}
    public partial struct Rectangle {}
    public partial struct Uniform {}
    public partial struct YCbCr {}
    public partial struct YCbCrSubsampleRatio {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸbufio() => builtin.initPackage(typeof(bufio_package));
    [GoInit] internal static void initᴛᴛimportꓸerrors() => builtin.initPackage(typeof(errors_package));
    [GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() => builtin.initPackage(typeof(image.color_package));
    [GoInit] internal static void initᴛᴛimportꓸio() => builtin.initPackage(typeof(io_package));
    [GoInit] internal static void initᴛᴛimportꓸmathꓸbits() => builtin.initPackage(typeof(math.bits_package));
    [GoInit] internal static void initᴛᴛimportꓸstrconv() => builtin.initPackage(typeof(strconv_package));
    [GoInit] internal static void initᴛᴛimportꓸsync() => builtin.initPackage(typeof(sync_package));
    [GoInit] internal static void initᴛᴛimportꓸsyncꓸatomic() => builtin.initPackage(typeof(go.sync.atomic_package));
    // </ImportInitializers>
}

// A Go method is an extension method on its receiver type. The methods of one type are
// in a class of their own beside the package class, so that a method and a package-level
// type, constant or variable can share a name as they do in Go. Each class is declared
// below with the type it holds the methods of; its blocks are in the converted sources,
// in Go's declaration order.

// <MethodHolders>
[GoMethodHost(typeof(image_package.Alpha)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛAlpha;
[GoMethodHost(typeof(image_package.Alpha16)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛAlpha16;
[GoMethodHost(typeof(image_package.CMYK)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛCMYK;
[GoMethodHost(typeof(image_package.Gray)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛGray;
[GoMethodHost(typeof(image_package.Gray16)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛGray16;
[GoMethodHost(typeof(image_package.NRGBA)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛNRGBA;
[GoMethodHost(typeof(image_package.NRGBA64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛNRGBA64;
[GoMethodHost(typeof(image_package.NYCbCrA)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛNYCbCrA;
[GoMethodHost(typeof(image_package.Paletted)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛPaletted;
[GoMethodHost(typeof(image_package.Point)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛPoint;
[GoMethodHost(typeof(image_package.RGBA)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛRGBA;
[GoMethodHost(typeof(image_package.RGBA64)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛRGBA64;
[GoMethodHost(typeof(image_package.Rectangle)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛRectangle;
[GoMethodHost(typeof(image_package.Uniform)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛUniform;
[GoMethodHost(typeof(image_package.YCbCr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛYCbCr;
[GoMethodHost(typeof(image_package.YCbCrSubsampleRatio)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class imageᴛYCbCrSubsampleRatio;
// </MethodHolders>

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies in the package class)
partial class imageᴛPoint {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Point p) => image_package.Stringᴍ(p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Point Add(this Point p, Point q) => image_package.Addᴍ(p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Point Sub(this Point p, Point q) => image_package.Subᴍ(p, q);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Point Mul(this Point p, nint k) => image_package.Mulᴍ(p, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Point Div(this Point p, nint k) => image_package.Divᴍ(p, k);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool In(this Point p, Rectangle r) => image_package.Inᴍ(p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Point Mod(this Point p, Rectangle r) => image_package.Modᴍ(p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Eq(this Point p, Point q) => image_package.Eqᴍ(p, q);
}
partial class imageᴛRectangle {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Rectangle r) => image_package.Stringᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Dx(this Rectangle r) => image_package.Dxᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Dy(this Rectangle r) => image_package.Dyᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Point Size(this Rectangle r) => image_package.Sizeᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Add(this Rectangle r, Point p) => image_package.Addᴍ(r, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Sub(this Rectangle r, Point p) => image_package.Subᴍ(r, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Inset(this Rectangle r, nint n) => image_package.Insetᴍ(r, n);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Intersect(this Rectangle r, Rectangle s) => image_package.Intersectᴍ(r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Union(this Rectangle r, Rectangle s) => image_package.Unionᴍ(r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Empty(this Rectangle r) => image_package.Emptyᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Eq(this Rectangle r, Rectangle s) => image_package.Eqᴍ(r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Overlaps(this Rectangle r, Rectangle s) => image_package.Overlapsᴍ(r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool In(this Rectangle r, Rectangle s) => image_package.Inᴍ(r, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Canon(this Rectangle r) => image_package.Canonᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this Rectangle r, nint x, nint y) => image_package.Atᴍ(r, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this Rectangle r, nint x, nint y) => image_package.RGBA64Atᴍ(r, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this Rectangle r) => image_package.Boundsᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this Rectangle r) => image_package.ColorModelᴍ(r);
}
partial class imageᴛRGBA {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref RGBA p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref RGBA p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref RGBA p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref RGBA p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA RGBAAt(this ref RGBA p, nint x, nint y) => image_package.RGBAAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref RGBA p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref RGBA p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref RGBA p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA(this ref RGBA p, nint x, nint y, color.RGBA c) => image_package.SetRGBAᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref RGBA p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref RGBA p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛRGBA64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref RGBA64 p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref RGBA64 p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref RGBA64 p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref RGBA64 p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref RGBA64 p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref RGBA64 p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref RGBA64 p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref RGBA64 p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref RGBA64 p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛNRGBA {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref NRGBA p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref NRGBA p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref NRGBA p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref NRGBA p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.NRGBA NRGBAAt(this ref NRGBA p, nint x, nint y) => image_package.NRGBAAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref NRGBA p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref NRGBA p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref NRGBA p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetNRGBA(this ref NRGBA p, nint x, nint y, color.NRGBA c) => image_package.SetNRGBAᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref NRGBA p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref NRGBA p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛNRGBA64 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref NRGBA64 p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref NRGBA64 p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref NRGBA64 p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref NRGBA64 p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.NRGBA64 NRGBA64At(this ref NRGBA64 p, nint x, nint y) => image_package.NRGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref NRGBA64 p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref NRGBA64 p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref NRGBA64 p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetNRGBA64(this ref NRGBA64 p, nint x, nint y, color.NRGBA64 c) => image_package.SetNRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref NRGBA64 p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref NRGBA64 p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛAlpha {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref Alpha p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref Alpha p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref Alpha p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref Alpha p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Alpha AlphaAt(this ref Alpha p, nint x, nint y) => image_package.AlphaAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref Alpha p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref Alpha p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref Alpha p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetAlpha(this ref Alpha p, nint x, nint y, color.Alpha c) => image_package.SetAlphaᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref Alpha p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref Alpha p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛAlpha16 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref Alpha16 p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref Alpha16 p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref Alpha16 p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref Alpha16 p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Alpha16 Alpha16At(this ref Alpha16 p, nint x, nint y) => image_package.Alpha16Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref Alpha16 p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref Alpha16 p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref Alpha16 p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetAlpha16(this ref Alpha16 p, nint x, nint y, color.Alpha16 c) => image_package.SetAlpha16ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref Alpha16 p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref Alpha16 p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛGray {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref Gray p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref Gray p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref Gray p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref Gray p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Gray GrayAt(this ref Gray p, nint x, nint y) => image_package.GrayAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref Gray p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref Gray p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref Gray p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetGray(this ref Gray p, nint x, nint y, color.Gray c) => image_package.SetGrayᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref Gray p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref Gray p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛGray16 {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref Gray16 p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref Gray16 p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref Gray16 p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref Gray16 p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Gray16 Gray16At(this ref Gray16 p, nint x, nint y) => image_package.Gray16Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref Gray16 p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref Gray16 p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref Gray16 p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetGray16(this ref Gray16 p, nint x, nint y, color.Gray16 c) => image_package.SetGray16ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref Gray16 p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref Gray16 p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛCMYK {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref CMYK p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref CMYK p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref CMYK p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref CMYK p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.CMYK CMYKAt(this ref CMYK p, nint x, nint y) => image_package.CMYKAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref CMYK p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref CMYK p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref CMYK p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetCMYK(this ref CMYK p, nint x, nint y, color.CMYK c) => image_package.SetCMYKᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref CMYK p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref CMYK p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛPaletted {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref Paletted p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref Paletted p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref Paletted p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref Paletted p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint PixOffset(this ref Paletted p, nint x, nint y) => image_package.PixOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref Paletted p, nint x, nint y, color.Color c) => image_package.Setᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetRGBA64(this ref Paletted p, nint x, nint y, color.RGBA64 c) => image_package.SetRGBA64ᴍ(ref p, x, y, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uint8 ColorIndexAt(this ref Paletted p, nint x, nint y) => image_package.ColorIndexAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetColorIndex(this ref Paletted p, nint x, nint y, uint8 index) => image_package.SetColorIndexᴍ(ref p, x, y, index);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref Paletted p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref Paletted p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛUniform {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this ref Uniform c) => image_package.RGBAᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ж<Uniform> Ꮡc) => image_package.ColorModelᴍ(Ꮡc);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color Convert(this ref Uniform c, color.Color _) => image_package.Convertᴍ(ref c, _);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref Uniform c) => image_package.Boundsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref Uniform c, nint x, nint y) => image_package.Atᴍ(ref c, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref Uniform c, nint x, nint y) => image_package.RGBA64Atᴍ(ref c, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref Uniform c) => image_package.Opaqueᴍ(ref c);
}
partial class imageᴛYCbCrSubsampleRatio {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this YCbCrSubsampleRatio s) => image_package.Stringᴍ(s);
}
partial class imageᴛYCbCr {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref YCbCr p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Rectangle Bounds(this ref YCbCr p) => image_package.Boundsᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref YCbCr p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref YCbCr p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.YCbCr YCbCrAt(this ref YCbCr p, nint x, nint y) => image_package.YCbCrAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint YOffset(this ref YCbCr p, nint x, nint y) => image_package.YOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint COffset(this ref YCbCr p, nint x, nint y) => image_package.COffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref YCbCr p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref YCbCr p) => image_package.Opaqueᴍ(ref p);
}
partial class imageᴛNYCbCrA {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Model ColorModel(this ref NYCbCrA p) => image_package.ColorModelᴍ(ref p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.Color At(this ref NYCbCrA p, nint x, nint y) => image_package.Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.RGBA64 RGBA64At(this ref NYCbCrA p, nint x, nint y) => image_package.RGBA64Atᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static color.NYCbCrA NYCbCrAAt(this ref NYCbCrA p, nint x, nint y) => image_package.NYCbCrAAtᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint AOffset(this ref NYCbCrA p, nint x, nint y) => image_package.AOffsetᴍ(ref p, x, y);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Image SubImage(this ref NYCbCrA p, Rectangle r) => image_package.SubImageᴍ(ref p, r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Opaque(this ref NYCbCrA p) => image_package.Opaqueᴍ(ref p);
}
// </GlyphForwarders>
