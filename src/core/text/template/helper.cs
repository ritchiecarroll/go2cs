// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Helper functions to make constructing templates easier.
namespace go.text;

using fmt = fmt_package;
using fs = go.io.fs_package;
using os = os_package;
using path = path_package;
using filepath = go.path.filepath_package;
using go.io;
using go.path;
using ꓸꓸꓸstring = Span<@string>;

partial class template_package {

// Functions and methods to parse templates.

// Must is a helper that wraps a call to a function returning ([*Template], error)
// and panics if the error is non-nil. It is intended for use in variable
// initializations such as
//
//	var t = template.Must(template.New("name").Parse("text"))
public static ж<Template> Must(ж<Template> Ꮡt, error err) {
    if (err != default!) {
        throw panic(err);
    }
    return Ꮡt;
}

// ParseFiles creates a new [Template] and parses the template definitions from
// the named files. The returned template's name will have the base name and
// parsed contents of the first file. There must be at least one file.
// If an error occurs, parsing stops and the returned *Template is nil.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
// For instance, ParseFiles("a/foo", "b/foo") stores "b/foo" as the template
// named "foo", while "a/foo" is unavailable.
public static (ж<Template>, error) ParseFiles(params ꓸꓸꓸstring filenamesʗp) {
    var filenames = filenamesʗp.sslice();

    return parseFiles(nil, readFileOS, filenames.ꓸꓸꓸ);
}

// ParseFiles parses the named files and associates the resulting templates with
// t. If an error occurs, parsing stops and the returned template is nil;
// otherwise it is t. There must be at least one file.
// Since the templates created by ParseFiles are named by the base
// (see [filepath.Base]) names of the argument files, t should usually have the
// name of one of the (base) names of the files. If it does not, depending on
// t's contents before calling ParseFiles, t.Execute may fail. In that
// case use t.ExecuteTemplate to execute a valid template.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
public static (ж<Template>, error) ParseFiles(this ж<Template> Ꮡt, params ꓸꓸꓸstring filenamesʗp) {
    var filenames = filenamesʗp.sslice();
    ref var t = ref Ꮡt.DerefOrNull();

    t.init();
    return parseFiles(Ꮡt, readFileOS, filenames.ꓸꓸꓸ);
}

// parseFiles is the helper for the method and function. If the argument
// template is nil, it is created from the first file.
internal static (ж<Template>, error) parseFiles(ж<Template> Ꮡt, Func<@string, (@string, slice<byte>, error)> readFile, params ꓸꓸꓸstring filenamesʗp) {
    var filenames = filenamesʗp.sslice();
    ref var t = ref Ꮡt.DerefOrNull();

    if (len(filenames) == 0) {
        // Not really a problem, but be consistent.
        return (default!, fmt.Errorf("template: no files named in call to ParseFiles"u8));
    }
    foreach (var (_, filename) in filenames) {
        var (name, b, err) = readFile(filename);
        if (err != default!) {
            return (default!, err);
        }
        @string s = (@string)b;
        // First template becomes return value if not already defined,
        // and we use that one for subsequent New calls to associate
        // all the templates together. Also, if this file has the same name
        // as t, this file becomes the contents of t, so
        //  t, err := New(name).Funcs(xxx).ParseFiles(name)
        // works. Otherwise we create a new template associated with t.
        ж<Template> tmpl = default!;
        if (Ꮡt == nil) {
            Ꮡt = New(name); t = ref Ꮡt.DerefOrNull();
        }
        if (name == t.Name()){
            tmpl = Ꮡt;
        } else {
            tmpl = t.New(name);
        }
        (_, err) = tmpl.Parse(s);
        if (err != default!) {
            return (default!, err);
        }
    }
    return (Ꮡt, default!);
}

// ParseGlob creates a new [Template] and parses the template definitions from
// the files identified by the pattern. The files are matched according to the
// semantics of [filepath.Match], and the pattern must match at least one file.
// The returned template will have the [filepath.Base] name and (parsed)
// contents of the first file matched by the pattern. ParseGlob is equivalent to
// calling [ParseFiles] with the list of files matched by the pattern.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
public static (ж<Template>, error) ParseGlob(@string pattern) {
    return parseGlob(nil, pattern);
}

// ParseGlob parses the template definitions in the files identified by the
// pattern and associates the resulting templates with t. The files are matched
// according to the semantics of [filepath.Match], and the pattern must match at
// least one file. ParseGlob is equivalent to calling [Template.ParseFiles] with
// the list of files matched by the pattern.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
public static (ж<Template>, error) ParseGlob(this ж<Template> Ꮡt, @string pattern) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.init();
    return parseGlob(Ꮡt, pattern);
}

// parseGlob is the implementation of the function and method ParseGlob.
internal static (ж<Template>, error) parseGlob(ж<Template> Ꮡt, @string pattern) {
    var (filenames, err) = filepath.Glob(pattern);
    if (err != default!) {
        return (default!, err);
    }
    if (len(filenames) == 0) {
        return (default!, fmt.Errorf("template: pattern matches no files: %#q"u8, pattern));
    }
    return parseFiles(Ꮡt, readFileOS, filenames.ꓸꓸꓸ);
}

// ParseFS is like [Template.ParseFiles] or [Template.ParseGlob] but reads from the file system fsys
// instead of the host operating system's file system.
// It accepts a list of glob patterns (see [path.Match]).
// (Note that most file names serve as glob patterns matching only themselves.)
public static (ж<Template>, error) ParseFS(fs.FS fsys, params ꓸꓸꓸstring patternsʗp) {
    var patterns = patternsʗp.slice();

    return parseFS(nil, fsys, patterns);
}

// ParseFS is like [Template.ParseFiles] or [Template.ParseGlob] but reads from the file system fsys
// instead of the host operating system's file system.
// It accepts a list of glob patterns (see [path.Match]).
// (Note that most file names serve as glob patterns matching only themselves.)
public static (ж<Template>, error) ParseFS(this ж<Template> Ꮡt, fs.FS fsys, params ꓸꓸꓸstring patternsʗp) {
    var patterns = patternsʗp.slice();
    ref var t = ref Ꮡt.DerefOrNull();

    t.init();
    return parseFS(Ꮡt, fsys, patterns);
}

internal static (ж<Template>, error) parseFS(ж<Template> Ꮡt, fs.FS fsys, slice<@string> patterns) {
    slice<@string> filenames = default!;
    foreach (var (_, pattern) in patterns) {
        var (list, err) = fs.Glob(fsys, pattern);
        if (err != default!) {
            return (default!, err);
        }
        if (len(list) == 0) {
            return (default!, fmt.Errorf("template: pattern matches no files: %#q"u8, pattern));
        }
        filenames = appendꓸꓸꓸ(filenames, list);
    }
    return parseFiles(Ꮡt, readFileFS(fsys), filenames.ꓸꓸꓸ);
}

internal static (@string name, slice<byte> b, error err) readFileOS(@string @file) {
    @string name = default!;
    slice<byte> b = default!;
    error err = default!;

    name = filepath.Base(@file);
    (b, err) = os.ReadFile(@file);
    return (name, b, err);
}

internal static Func<@string, (@string, slice<byte>, error)> readFileFS(fs.FS fsys) {
    return (@string @file) => {
        @string name = default!;
        slice<byte> b = default!;
        error err = default!;
        name = path.Base(@file);
        (b, err) = fs.ReadFile(fsys, @file);
        return (name, b, err);
    };
}

} // end template_package
