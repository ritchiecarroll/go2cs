// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using FuncMap = go.text.template_package.FuncMap;

namespace go.html;

using fmt = fmt_package;
using io = io_package;
using fs = go.io.fs_package;
using os = os_package;
using path = path_package;
using filepath = go.path.filepath_package;
using sync = sync_package;
using template = text.template_package;
using parse = text.template.parse_package;
using go.io;
using go.path;
using text;
using text.template;
using ꓸꓸꓸstring = Span<@string>;

partial class template_package {

// Template is a specialized Template from "text/template" that produces a safe
// HTML document fragment.
partial struct Template {
    // Sticky error if escaping fails, or escapeOK if succeeded.
    internal error escapeErr;
    // We could embed the text/template field, but it's safer not to because
    // we need to keep our version of the name space and the underlying
    // template's in sync.
    internal ж<template.Template> text;
    // The underlying template's parse tree, updated to be HTML-safe.
    public ж<parse.Tree> Tree;
    internal partial ref ж<nameSpace> nameSpace { get; } // common to all associated templates
}

// escapeOK is a sentinel value used to indicate valid escaping.
internal static error escapeOK = fmt.Errorf("template escaped correctly"u8);

// nameSpace is the data structure shared by all templates in an association.
partial struct nameSpace {
    internal sync.Mutex mu;
    internal map<@string, ж<Template>> set;
    internal bool escaped;
    internal escaper esc;
}

// Templates returns a slice of the templates associated with t, including t
// itself.
public static slice<ж<Template>> Templates(this ж<Template> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var ns = t.nameSpace;
        ns.of(nameSpace.Ꮡmu).Lock();
        var nsʗ1 = ns;
        defer(nsʗ1.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        // Return a slice so we don't expose the map.
        var m = new slice<ж<Template>>(0, len((~ns).set));
        foreach (var (_, v) in (~ns).set) {
            m = append(m, v);
        }
        return m;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Option sets options for the template. Options are described by
// strings, either a simple string or "key=value". There can be at
// most one equals sign in an option string. If the option string
// is unrecognized or otherwise invalid, Option panics.
//
// Known options:
//
// missingkey: Control the behavior during execution if a map is
// indexed with a key that is not present in the map.
//
//	"missingkey=default" or "missingkey=invalid"
//		The default behavior: Do nothing and continue execution.
//		If printed, the result of the index operation is the string
//		"<no value>".
//	"missingkey=zero"
//		The operation returns the zero value for the map type's element.
//	"missingkey=error"
//		Execution stops immediately with an error.
public static ж<Template> Option(this ж<Template> Ꮡt, params ꓸꓸꓸstring optʗp) {
    var opt = optʗp.sslice();

    ref var t = ref Ꮡt.DerefOrNull();
    t.text.Option(opt.ꓸꓸꓸ);
    return Ꮡt;
}

// checkCanParse checks whether it is OK to parse templates.
// If not, it returns an error.
internal static error checkCanParse(this ж<Template> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Ꮡt == nil) {
            return default!;
        }
        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        if ((~t.nameSpace).escaped) {
            return fmt.Errorf("html/template: cannot Parse after Execute"u8);
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// escape escapes all associated templates.
internal static error escape(this ж<Template> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        t.nameSpace.Value.escaped = true;
        if (t.escapeErr == default!){
            if (t.Tree == nil) {
                return fmt.Errorf("template: %q is an incomplete or empty template"u8, t.Name());
            }
            {
                var err = escapeTemplate(Ꮡt, new parse.ListNodeжNode((~t.text).Root), t.Name()); if (err != default!) {
                    return err;
                }
            }
        } else 
        if (!AreEqual(t.escapeErr, escapeOK)) {
            return t.escapeErr;
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Execute applies a parsed template to the specified data object,
// writing the output to wr.
// If an error occurs executing the template or writing its output,
// execution stops, but partial results may already have been written to
// the output writer.
// A template may be executed safely in parallel, although if parallel
// executions share a Writer the output may be interleaved.
public static error Execute(this ж<Template> Ꮡt, io.Writer wr, any data) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var err = Ꮡt.escape(); if (err != default!) {
            return err;
        }
    }
    return t.text.Execute(wr, data);
}

// ExecuteTemplate applies the template associated with t that has the given
// name to the specified data object and writes the output to wr.
// If an error occurs executing the template or writing its output,
// execution stops, but partial results may already have been written to
// the output writer.
// A template may be executed safely in parallel, although if parallel
// executions share a Writer the output may be interleaved.
public static error ExecuteTemplate(this ж<Template> Ꮡt, io.Writer wr, @string name, any data) {
    var (tmpl, err) = Ꮡt.lookupAndEscapeTemplate(name);
    if (err != default!) {
        return err;
    }
    return (~tmpl).text.Execute(wr, data);
}

// lookupAndEscapeTemplate guarantees that the template with the given name
// is escaped, or returns an error if it cannot be. It returns the named
// template.
internal static (ж<Template> tmpl, error err) lookupAndEscapeTemplate(this ж<Template> Ꮡt, @string name) {
    ж<Template> tmpl = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        t.nameSpace.Value.escaped = true;
        tmpl = t.set[name];
        if (tmpl == nil) {
            (tmpl, err) = (default!, fmt.Errorf("html/template: %q is undefined"u8, name)); goto ᒐdone;
        }
        if ((~tmpl).escapeErr != default! && !AreEqual((~tmpl).escapeErr, escapeOK)) {
            (tmpl, err) = (default!, (~tmpl).escapeErr); goto ᒐdone;
        }
        if ((~(~tmpl).text).Tree == nil || (~(~tmpl).text).Root == nil) {
            (tmpl, err) = (default!, fmt.Errorf("html/template: %q is an incomplete template"u8, name)); goto ᒐdone;
        }
        if (t.text.Lookup(name) == nil) {
            throw panic("html/template internal error: template escaping out of sync");
        }
        if ((~tmpl).escapeErr == default!) {
            err = escapeTemplate(tmpl, new parse.ListNodeжNode((~(~tmpl).text).Root), name);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (tmpl, err);
}

// DefinedTemplates returns a string listing the defined templates,
// prefixed by the string "; defined templates are: ". If there are none,
// it returns the empty string. Used to generate an error message.
public static @string DefinedTemplates(this ref Template t) {
    return t.text.DefinedTemplates();
}

// Parse parses text as a template body for t.
// Named template definitions ({{define ...}} or {{block ...}} statements) in text
// define additional templates associated with t and are removed from the
// definition of t itself.
//
// Templates can be redefined in successive calls to Parse,
// before the first use of [Template.Execute] on t or any associated template.
// A template definition with a body containing only white space and comments
// is considered empty and will not replace an existing template's body.
// This allows using Parse to add new named template definitions without
// overwriting the main template body.
public static (ж<Template>, error) Parse(this ж<Template> Ꮡt, @string text) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        {
            var errΔ1 = Ꮡt.checkCanParse(); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        var (ret, err) = t.text.Parse(text);
        if (err != default!) {
            return (default!, err);
        }
        // In general, all the named templates might have changed underfoot.
        // Regardless, some new ones may have been defined.
        // The template.Template set has been updated; update ours.
        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        foreach (var (_, v) in ret.Templates()) {
            @string name = v.Name();
            var tmpl = t.set[name];
            if (tmpl == nil) {
                tmpl = t.@new(name);
            }
            tmpl.Value.text = v;
            tmpl.Value.Tree = v.Value.Tree;
        }
        return (Ꮡt, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// AddParseTree creates a new template with the name and parse tree
// and associates it with t.
//
// It returns an error if t or any associated template has already been executed.
public static (ж<Template>, error) AddParseTree(this ж<Template> Ꮡt, @string name, ж<parse.Tree> Ꮡtree) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        {
            var errΔ1 = Ꮡt.checkCanParse(); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        var (text, err) = t.text.AddParseTree(name, Ꮡtree);
        if (err != default!) {
            return (default!, err);
        }
        var ret = Ꮡ(new Template(
            default!,
            text,
            (~text).Tree,
            t.nameSpace
        ));
        t.set[name] = ret;
        return (ret, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Clone returns a duplicate of the template, including all associated
// templates. The actual representation is not copied, but the name space of
// associated templates is, so further calls to [Template.Parse] in the copy will add
// templates to the copy but not to the original. [Template.Clone] can be used to prepare
// common templates and use them with variant definitions for other templates
// by adding the variants after the clone is made.
//
// It returns an error if t has already been executed.
public static (ж<Template>, error) Clone(this ж<Template> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        if (t.escapeErr != default!) {
            return (default!, fmt.Errorf("html/template: cannot Clone %q after it has executed"u8, t.Name()));
        }
        var (textClone, err) = t.text.Clone();
        if (err != default!) {
            return (default!, err);
        }
        var ns = Ꮡ(new nameSpace(set: new map<@string, ж<Template>>()));
        ns.Value.esc = makeEscaper(ns);
        var ret = Ꮡ(new Template(
            default!,
            textClone,
            (~textClone).Tree,
            ns
        ));
        ret.Value.set[ret.Name()] = ret;
        foreach (var (_, x) in textClone.Templates()) {
            @string name = x.Name();
            var src = t.set[name];
            if (src == nil || (~src).escapeErr != default!) {
                return (default!, fmt.Errorf("html/template: cannot Clone %q after it has executed"u8, t.Name()));
            }
            x.Value.Tree = (~x).Tree.Copy();
            ret.Value.set[name] = Ꮡ(new Template(
                default!,
                x,
                (~x).Tree,
                (~ret).nameSpace
            ));
        }
        // Return the template associated with the name of this template.
        return ((~ret).set[ret.Name()], default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// New allocates a new HTML template with the given name.
public static ж<Template> New(@string name) {
    var ns = Ꮡ(new nameSpace(set: new map<@string, ж<Template>>()));
    ns.Value.esc = makeEscaper(ns);
    var tmpl = Ꮡ(new Template(
        default!,
        text.template_package.New(name),
        nil,
        ns
    ));
    tmpl.Value.set[name] = tmpl;
    return tmpl;
}

// New allocates a new HTML template associated with the given one
// and with the same delimiters. The association, which is transitive,
// allows one template to invoke another with a {{template}} action.
//
// If a template with the given name already exists, the new HTML template
// will replace it. The existing template will be reset and disassociated with
// t.
public static ж<Template> New(this ж<Template> Ꮡt, @string name) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        return t.@new(name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// new is the implementation of New, without the lock.
internal static ж<Template> @new(this ref Template t, @string name) {
    var tmpl = Ꮡ(new Template(
        default!,
        t.text.New(name),
        nil,
        t.nameSpace
    ));
    {
        var (existing, ok) = (~tmpl).set[name, ꟷ]; if (ok) {
            var emptyTmpl = New(existing.Name());
            existing.Value = emptyTmpl.Value;
        }
    }
    tmpl.Value.set[name] = tmpl;
    return tmpl;
}

// Name returns the name of the template.
public static @string Name(this ref Template t) {
    return t.text.Name();
}

// Funcs adds the elements of the argument map to the template's function map.
// It must be called before the template is parsed.
// It panics if a value in the map is not a function with appropriate return
// type. However, it is legal to overwrite elements of the map. The return
// value is the template, so calls can be chained.
public static ж<Template> Funcs(this ж<Template> Ꮡt, FuncMap funcMap) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.text.Funcs(((template.FuncMap)funcMap));
    return Ꮡt;
}

// Delims sets the action delimiters to the specified strings, to be used in
// subsequent calls to [Template.Parse], [ParseFiles], or [ParseGlob]. Nested template
// definitions will inherit the settings. An empty delimiter stands for the
// corresponding default: {{ or }}.
// The return value is the template, so calls can be chained.
public static ж<Template> Delims(this ж<Template> Ꮡt, @string left, @string right) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.text.Delims(left, right);
    return Ꮡt;
}

// Lookup returns the template with the given name that is associated with t,
// or nil if there is no such template.
public static ж<Template> Lookup(this ж<Template> Ꮡt, @string name) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.nameSpace.of(nameSpace.Ꮡmu).Lock();
        defer(Ꮡt.Value.nameSpace.of(nameSpace.Ꮡmu).Unlock, ref ᒐ);
        return t.set[name];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Must is a helper that wraps a call to a function returning ([*Template], error)
// and panics if the error is non-nil. It is intended for use in variable initializations
// such as
//
//	var t = template.Must(template.New("name").Parse("html"))
public static ж<Template> Must(ж<Template> Ꮡt, error err) {
    if (err != default!) {
        throw panic(err);
    }
    return Ꮡt;
}

// ParseFiles creates a new [Template] and parses the template definitions from
// the named files. The returned template's name will have the (base) name and
// (parsed) contents of the first file. There must be at least one file.
// If an error occurs, parsing stops and the returned [*Template] is nil.
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
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
//
// ParseFiles returns an error if t or any associated template has already been executed.
public static (ж<Template>, error) ParseFiles(this ж<Template> Ꮡt, params ꓸꓸꓸstring filenamesʗp) {
    var filenames = filenamesʗp.sslice();

    return parseFiles(Ꮡt, readFileOS, filenames.ꓸꓸꓸ);
}

// parseFiles is the helper for the method and function. If the argument
// template is nil, it is created from the first file.
internal static (ж<Template>, error) parseFiles(ж<Template> Ꮡt, Func<@string, (@string, slice<byte>, error)> readFile, params ꓸꓸꓸstring filenamesʗp) {
    var filenames = filenamesʗp.sslice();

    ref var t = ref Ꮡt.DerefOrNull();
    {
        var err = Ꮡt.checkCanParse(); if (err != default!) {
            return (default!, err);
        }
    }
    if (len(filenames) == 0) {
        // Not really a problem, but be consistent.
        return (default!, fmt.Errorf("html/template: no files named in call to ParseFiles"u8));
    }
    foreach (var (_, filename) in filenames) {
        var (name, b, err) = readFile(filename);
        if (err != default!) {
            return (default!, err);
        }
        @string s = ((@string)b);
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
            tmpl = Ꮡt.New(name);
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
// semantics of filepath.Match, and the pattern must match at least one file.
// The returned template will have the (base) name and (parsed) contents of the
// first file matched by the pattern. ParseGlob is equivalent to calling
// [ParseFiles] with the list of files matched by the pattern.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
public static (ж<Template>, error) ParseGlob(@string pattern) {
    return parseGlob(nil, pattern);
}

// ParseGlob parses the template definitions in the files identified by the
// pattern and associates the resulting templates with t. The files are matched
// according to the semantics of filepath.Match, and the pattern must match at
// least one file. ParseGlob is equivalent to calling t.ParseFiles with the
// list of files matched by the pattern.
//
// When parsing multiple files with the same name in different directories,
// the last one mentioned will be the one that results.
//
// ParseGlob returns an error if t or any associated template has already been executed.
public static (ж<Template>, error) ParseGlob(this ж<Template> Ꮡt, @string pattern) {
    return parseGlob(Ꮡt, pattern);
}

// parseGlob is the implementation of the function and method ParseGlob.
internal static (ж<Template>, error) parseGlob(ж<Template> Ꮡt, @string pattern) {
    {
        var errΔ1 = Ꮡt.checkCanParse(); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var (filenames, err) = filepath.Glob(pattern);
    if (err != default!) {
        return (default!, err);
    }
    if (len(filenames) == 0) {
        return (default!, fmt.Errorf("html/template: pattern matches no files: %#q"u8, pattern));
    }
    return parseFiles(Ꮡt, readFileOS, filenames.ꓸꓸꓸ);
}

// IsTrue reports whether the value is 'true', in the sense of not the zero of its type,
// and whether the value has a meaningful truth value. This is the definition of
// truth used by if and other such actions.
public static (bool truth, bool ok) IsTrue(any val) {
    return text.template_package.IsTrue(val);
}

// ParseFS is like [ParseFiles] or [ParseGlob] but reads from the file system fs
// instead of the host operating system's file system.
// It accepts a list of glob patterns.
// (Note that most file names serve as glob patterns matching only themselves.)
public static (ж<Template>, error) ParseFS(fs.FS fsΔ1, params ꓸꓸꓸstring patternsʗp) {
    var patterns = patternsʗp.slice();

    return parseFS(nil, fsΔ1, patterns);
}

// ParseFS is like [Template.ParseFiles] or [Template.ParseGlob] but reads from the file system fs
// instead of the host operating system's file system.
// It accepts a list of glob patterns.
// (Note that most file names serve as glob patterns matching only themselves.)
public static (ж<Template>, error) ParseFS(this ж<Template> Ꮡt, fs.FS fsΔ1, params ꓸꓸꓸstring patternsʗp) {
    var patterns = patternsʗp.slice();

    return parseFS(Ꮡt, fsΔ1, patterns);
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
