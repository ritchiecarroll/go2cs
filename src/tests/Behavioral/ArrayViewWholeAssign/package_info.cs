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
using static go.main_package;

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
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

// The struct types whose fields this package reaches through a pointer, as Go spells it, `p.f`:
// go2cs-gen emits a field property on the pointer for each of their fields.
[assembly: GoFieldAccess(typeof(holder))]
[assembly: GoFieldAccess(typeof(outerHolder))]

// Go source positions are recorded here, one `GoPositionMap` attribute per converted
// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it
// can name the GO file and line a frame was converted from rather than the emitted C# one.
// Each record carries the Go file's identity and an encoded C#-line to Go-line table
// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,
// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.

// <GoSourcePositionMaps>
[assembly: GoPositionMap("main.go", "main.cs", "ABhElJSCgoKCgoKmgoKCgoKmgoKCgoKmgoKCgpKCpoKCgoKigqaCgoKCgqaCgoKCgqaCgoKCgqaCgoKCgqaCgoKCgqaCgoKCpoKCgoKCpoKCgoKCgpSmgoKCgoKCgrOagqaCgoKCgIKUpoKCgoKCkpSmgoKCgoK0tKaCgoKCkoKmgoKCgqKUpoKCgoKCpoKCgoKCpoKCgoKCpoKCgoKC1oKmgoKCgoKCkoKm0oKAkoLWgoKCgoKClKaCgoKCgoKUgqaCgoKCgoKCgoLFroKm0oKAkoLW0oKAkoLcsoKQkubSgpCS5tKCkJLmsoKQkuaygpCS5sKCkJLmwoKQkoKU5rKCkoKU5rKCgpCAkuaygpCS5oKygJKCkJLUpsKCgJKC5sKCkJLmwoKQkujCgqCAkoLmmMKCkoKU6MKCkJLmgoKCgoKCgoKCgoKSgoKCgoKCgoKCgoKCgoKCgpKCgoKSgpKmgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgg==", "262-262:1;309-309:1;316-316:1;326-326:1;332-332:1;338-338:1;344-344:1;350-350:1;356-356:1;362-362:1;371-374:1;381-381:1;392-397:1;393-393:1.1;395-395:1.2;416-416:1;423-423:1;434-437:1;444-444:1", "", "", "", "277:56,278:13,338:62,339:13,355:58,356:13,371:48,372:13,389:50,390:13,407:49,408:13,423:49,424:13,439:48,440:13,456:48,457:13,473:49,474:13,492:50,493:13,510:50,511:13,527:46,528:13,542:60,543:17,547:60,548:17,565:46,566:13,583:46,584:13,600:50,601:13,618:50,619:13,639:53,640:13,657:53,658:13")]
// </GoSourcePositionMaps>

namespace go;

[GoPackage("main")]
[GoTestMatchingConsoleOutput]
public static partial class main_package
{
    // C# nested types declared with no access modifier are always private, and the
    // Go type declarations in this package's converted sources are deliberately
    // bare so they read more like the original Go code. The real accessibility for
    // the types - public for a Go-exported name, internal otherwise - are defined
    // via declarations below.

    // <TypeAccessibility>
    [GoValueClone("x")] internal partial struct holder {}
    internal partial struct named {}
    [GoValueClone("h")] internal partial struct outerHolder {}
    // </TypeAccessibility>

    // Go initializes an imported package before the importing package, for every import
    // form - not only the blank one. .NET would never load an assembly nothing has touched
    // yet, so each import that initializes anything is forced below: once per assembly, and
    // ahead of this package's own `init` functions, which this file being the first compile
    // item of the project guarantees.

    // <ImportInitializers>
    [GoInit] internal static void initᴛᴛimportꓸfmt() => builtin.initPackage(typeof(fmt_package));
    // </ImportInitializers>
}
