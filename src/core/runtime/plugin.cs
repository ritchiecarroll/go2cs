// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeNoPluginModuleˢ = "runtime: no plugin module data"u8;
internal static readonly @string runtimePluginHasEmptyˢ = "runtime: plugin has empty pluginpath"u8;
internal static readonly @string pluginAlreadyLoadedˢ = "plugin already loaded"u8;
internal static readonly @string pluginNewModuleDataˢ = "plugin: new module data overlaps with previous moduledata"u8;

//go:linkname plugin_lastmoduleinit plugin.lastmoduleinit
internal static (@string path, map<@string, any> syms, slice<ж<initTask>> initTasks, @string errstr) plugin_lastmoduleinit() {
    map<@string, any> syms = default!;

    ж<moduledata> md = default!;
    for (var pmd = firstmoduledata.next; pmd != nil; pmd = pmd.Value.next) {
        if ((~pmd).bad) {
            md = default!; // we only want the last module
            continue;
        }
        md = pmd;
    }
    if (md == nil) {
        @throw(runtimeNoPluginModuleˢ);
    }
    if ((~md).pluginpath == ""u8) {
        @throw(runtimePluginHasEmptyˢ);
    }
    if ((~md).typemap != default!) {
        return ("", default!, default!, pluginAlreadyLoadedˢ);
    }
    foreach (var (_, pmd) in activeModules()) {
        if ((~pmd).pluginpath == (~md).pluginpath) {
            md.Value.bad = true;
            return ("", default!, default!, pluginAlreadyLoadedˢ);
        }
        if (inRange((~pmd).text, (~pmd).etext, (~md).text, (~md).etext) || inRange((~pmd).bss, (~pmd).ebss, (~md).bss, (~md).ebss) || inRange((~pmd).data, (~pmd).edata, (~md).data, (~md).edata) || inRange((~pmd).types, (~pmd).etypes, (~md).types, (~md).etypes)) {
            println((@string)"plugin: new module data overlaps with previous moduledata"u8);
            println((@string)"\tpmd.text-etext="u8, ((Δhex)(uint64)(~pmd).text), (@string)"-"u8, ((Δhex)(uint64)(~pmd).etext));
            println((@string)"\tpmd.bss-ebss="u8, ((Δhex)(uint64)(~pmd).bss), (@string)"-"u8, ((Δhex)(uint64)(~pmd).ebss));
            println((@string)"\tpmd.data-edata="u8, ((Δhex)(uint64)(~pmd).data), (@string)"-"u8, ((Δhex)(uint64)(~pmd).edata));
            println((@string)"\tpmd.types-etypes="u8, ((Δhex)(uint64)(~pmd).types), (@string)"-"u8, ((Δhex)(uint64)(~pmd).etypes));
            println((@string)"\tmd.text-etext="u8, ((Δhex)(uint64)(~md).text), (@string)"-"u8, ((Δhex)(uint64)(~md).etext));
            println((@string)"\tmd.bss-ebss="u8, ((Δhex)(uint64)(~md).bss), (@string)"-"u8, ((Δhex)(uint64)(~md).ebss));
            println((@string)"\tmd.data-edata="u8, ((Δhex)(uint64)(~md).data), (@string)"-"u8, ((Δhex)(uint64)(~md).edata));
            println((@string)"\tmd.types-etypes="u8, ((Δhex)(uint64)(~md).types), (@string)"-"u8, ((Δhex)(uint64)(~md).etypes));
            @throw(pluginNewModuleDataˢ);
        }
    }
    foreach (var (_, pkghash) in (~md).pkghashes) {
        if (pkghash.linktimehash != pkghash.runtimehash.Value) {
            md.Value.bad = true;
            return ("", default!, default!, "plugin was built with a different version of package " + pkghash.modulename);
        }
    }
    // Initialize the freshly loaded module.
    modulesinit();
    typelinksinit();
    pluginftabverify(md);
    moduledataverify1(md);
    @lock(ᏑitabLock);
    foreach (var (_, i) in (~md).itablinks) {
        itabAdd(i);
    }
    unlock(ᏑitabLock);
    // Build a map of symbol names to symbols. Here in the runtime
    // we fill out the first word of the interface, the type. We
    // pass these zero value interfaces to the plugin package,
    // where the symbol value is filled in (usually via cgo).
    //
    // Because functions are handled specially in the plugin package,
    // function symbol names are prefixed here with '.' to avoid
    // a dependency on the reflect package.
    syms = new map<@string, any>(len((~md).ptab));
    foreach (var (_, ptab) in (~md).ptab) {
        var symName = resolveNameOff((@unsafe.Pointer)(~md).types, ptab.name);
        var t = toRType((ж<_type>)(uintptr)((@unsafe.Pointer)(~md).types)).typeOff(ptab.typ); // TODO can this stack of conversions be simpler?
        ref var val = ref heap<any>(out var Ꮡval);
        var valp = (ж<array<@unsafe.Pointer>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡval));
        (valp.Value)[0] = @unsafe.Pointer.FromPinnedBox(t);
        @string name = symName.Name();
        if ((abiꓸKind)((~t).Kind_ & abi.KindMask) == abi.Func) {
            name = "."u8 + name;
        }
        syms[name] = val;
    }
    return ((~md).pluginpath, syms, (~md).inittasks, "");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noneˢ = "none"u8;
internal static readonly @string runtimePluginHasBadˢ = "runtime: plugin has bad symbol table"u8;

internal static void pluginftabverify(ж<moduledata> Ꮡmd) {
    ref var md = ref Ꮡmd.DerefOrNull();

    var badtable = false;
    for (nint i = 0; i < len(md.ftab); i++) {
        var entry = md.textAddr(md.ftab[i].entryoff);
        if (md.minpc <= entry && entry <= md.maxpc) {
            continue;
        }
        var f = new ΔfuncInfo(Ꮡ(md.pclntable, md.ftab[i].funcoff).Reinterpret<byte, _func>(), Ꮡmd);
        @string name = funcname(f);
        // A common bug is f.entry has a relocation to a duplicate
        // function symbol, meaning if we search for its PC we get
        // a valid entry with a name that is useful for debugging.
        @string name2 = noneˢ;
        var entry2 = (uintptr)0;
        var f2 = findfunc(entry);
        if (f2.valid()) {
            name2 = funcname(f2);
            entry2 = f2.entry();
        }
        badtable = true;
        println((@string)"ftab entry"u8, ((Δhex)(uint64)entry), (@string)"/"u8, ((Δhex)(uint64)entry2), (@string)": "u8,
            name, (@string)"/"u8, name2, (@string)"outside pc range:["u8, ((Δhex)(uint64)md.minpc), (@string)","u8, ((Δhex)(uint64)md.maxpc), (@string)"], modulename="u8, md.modulename, (@string)", pluginpath="u8, md.pluginpath);
    }
    if (badtable) {
        @throw(runtimePluginHasBadˢ);
    }
}

// inRange reports whether v0 or v1 are in the range [r0, r1].
internal static bool inRange(uintptr r0, uintptr r1, uintptr v0, uintptr v1) {
    return (v0 >= r0 && v0 <= r1) || (v1 >= r0 && v1 <= r1);
}

// A ptabEntry is generated by the compiler for each exported function
// and global variable in the main package of a plugin. It is used to
// initialize the plugin module's symbol map.
partial struct ptabEntry {
    internal nameOff name;
    internal typeOff typ;
}

} // end runtime_package
