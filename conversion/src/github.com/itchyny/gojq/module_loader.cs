global using ModuleLoader = object;

namespace go.github.com.itchyny;

using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using encoding;
using fs = go.io.fs_package;
using go.io;
using path;

partial class gojq_package {

public static ModuleLoader NewModuleLoader(slice<@string> paths) {
    var ps = new slice<@string>(0, len(paths));
    foreach (var (_, vᴛ1) in range(paths)) {
        var path = vᴛ1;

        path = resolvePath(path, ""); if (path != ""u8) {
            ps = builtin.append(ps, path);
        }
    }
    return Ꮡ(new moduleLoader(ps));
}

partial struct moduleLoader {
    internal slice<@string> paths;
}

internal static (slice<ж<Query>>, error) LoadInitModules(this ref moduleLoader l) {
    var qsᴷ = new stackAppend<ж<Query>>(moves: true);

    slice<ж<Query>> qs = default;
    foreach (var (_, vᴛ1) in range(l.paths)) {
        ref var path = ref heap(new @string(), out var Ꮡpath);
        path = vᴛ1;

        if (filepath.Base(path) != ".jq"u8) {
            continue;
        }
        var (fi, err) = os.Stat(path);
        if (err != nil) {
            if (os.IsNotExist(err)) {
                continue;
            }
            return (default, err);
        }
        if (fi.IsDir()) {
            continue;
        }
        (var cnt, err) = os.ReadFile(path);
        if (err != nil) {
            return (default, err);
        }
        (var q, err) = parseModule((@string)cnt, filepath.Dir(path));
        if (err != nil) {
            return (default, new queryParseErrorжerror(Ꮡ(new queryParseError(path, (@string)cnt, err))));
        }
        qs = builtin.append(qsᴷ, qs, q);
    }
    qs = move2heap(qsᴷ, qs);
    return (qs, default);
}

internal static (ж<Query>, error) LoadModuleWithMeta(this ref moduleLoader l, @string name, map<@string, any> meta) {
    ref var path = ref heap<@string>(out var Ꮡpath);
    (path, var err) = l.lookupModule(name, ".jq", meta);
    if (err != nil) {
        return (default, err);
    }
    (var cnt, err) = os.ReadFile(path);
    if (err != nil) {
        return (default, err);
    }
    (var q, err) = parseModule((@string)cnt, filepath.Dir(path));
    if (err != nil) {
        return (default, new queryParseErrorжerror(Ꮡ(new queryParseError(path, (@string)cnt, err))));
    }
    return (q, default);
}

internal static (any, error) LoadJSONWithMeta(this ж<moduleLoader> Ꮡl, @string name, map<@string, any> meta) {
    GoFrame ᒐ = default;
    try {
        ref var l = ref Ꮡl.DerefOrNull();

        ref var path = ref heap<@string>(out var Ꮡpath);
        (path, var err) = l.lookupModule(name, ".json", meta);
        if (err != nil) {
            return (default, err);
        }
        (var f, err) = os.Open(path);
        if (err != nil) {
            return (default, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var vals = slice<any>([]);
        var dec = json.NewDecoder(new os_FileжReader(f));
        dec.UseNumber();
        for (;;) {
            ref var val = ref heap<any>(out var Ꮡval);
            var errΔ1 = dec.Decode(Ꮡval); if (errΔ1 != nil) {
                if (AreEqual(errΔ1, io.EOF)) {
                    break;
                }
                var (_, errΔ2) = f.Seek(0, io.SeekStart); if (errΔ2 != nil) {
                    return (default, errΔ2);
                }
                var (cnt, er) = io.ReadAll(new os_FileжReader(f));
                if (er != nil) {
                    return (default, er);
                }
                return (default, new jsonParseErrorжerror(Ꮡ(new jsonParseError(path, (@string)cnt, errΔ1))));
            }
            vals = builtin.append(vals, val);
        }
        return (vals, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static (@string, error) lookupModule(this ref moduleLoader l, @string name, @string extension, map<@string, any> meta) {
    var paths = l.paths;
    {
        var (path, ok) = meta["search"]._<@string>(ᐧ); if (ok) {
            path = resolvePath(path, ""); if (path != ""u8) {
                paths = builtin.appendꓸꓸꓸ(slice<@string>([path]), paths);
            }
        }
    }
    foreach (var (_, @base) in range(paths)) {
        @string path = filepath.Join(@base, name + extension);
        {
            var (_, err) = os.Stat(path); if (err == nil) {
                return (path, err);
            }
        }
        path = filepath.Join(@base, name, filepath.Base(name) + extension);
        {
            var (_, err) = os.Stat(path); if (err == nil) {
                return (path, err);
            }
        }
    }
    return ("", fmt.Errorf("module not found: %q"u8, name));
}

internal static (ж<Query>, error) parseModule(@string cnt, @string dir) {
    var (q, err) = Parse(cnt);
    if (err != nil) {
        return (default, err);
    }
    foreach (var (_, i) in range(q.Imports)) {
        if (i.Meta != nil) {
            foreach (var (_, e) in range(i.Meta.KeyVals)) {
                if (e.Key == "search"u8 || e.KeyString == "search"u8) {
                    var (path, ok) = e.Val.toString(); if (ok) {
                        path = resolvePath(path, dir); if (path != ""u8) {
                            e.Val.Str = path;
                        } else {
                            e.Val.Null = true;
                        }
                    }
                }
            }
        }
    }
    return (q, default);
}

internal static @string resolvePath(@string path, @string dir) {
    switch (ᐧ) {
    case {} when filepath.IsAbs(path): {
        return path;
    }
    case {} when strings.HasPrefix(path, "~/"): {
        var (dirΔ2, err) = os.UserHomeDir();
        if (err != nil) {
            return "";
        }
        return filepath.Join(dirΔ2, path[2..]);
    }
    case {} when strings.HasPrefix(path, "$ORIGIN/"): {
        var (exe, err) = os.Executable();
        if (err != nil) {
            return "";
        }
        (exe, err) = filepath.EvalSymlinks(exe);
        if (err != nil) {
            return "";
        }
        return filepath.Join(filepath.Dir(exe), path[8..]);
    }
    default: {
        return filepath.Join(dir, path);
    }}

}

} // end gojq_package
