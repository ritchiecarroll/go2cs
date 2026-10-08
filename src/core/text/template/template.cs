// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using maps = maps_package;
using reflect = reflect_package;
using sync = sync_package;
using parse = go.text.template.parse_package;
using go.text.template;

partial class template_package {

// common holds the information shared by related templates.
partial struct common {
    internal map<@string, ж<Template>> tmpl; // Map from name to defined templates.
    internal sync.RWMutex muTmpl;         // protects tmpl
    internal option option;
    // We use two maps, one for parsing and one for execution.
    // This separation makes the API cleaner since it doesn't
    // expose reflection to the client.
    internal sync.RWMutex muFuncs; // protects parseFuncs and execFuncs
    internal FuncMap parseFuncs;
    internal map<@string, reflectꓸValue> execFuncs;
}

// Template is the representation of a parsed template. The *parse.Tree
// field is exported only for use by [html/template] and should be treated
// as unexported by all other clients.
partial struct Template {
    internal @string name;
    public partial ref ж<text.template.parse_package.Tree> Tree { get; }
    internal partial ref ж<common> common { get; }
    internal @string leftDelim;
    internal @string rightDelim;
}

// New allocates a new, undefined template with the given name.
public static ж<Template> New(@string name) {
    var t = Ꮡ(new Template(
        name: name
    ));
    t.init();
    return t;
}

// Name returns the name of the template.
public static @string Name(this ref Template t) {
    return t.name;
}

// New allocates a new, undefined template associated with the given one and with the same
// delimiters. The association, which is transitive, allows one template to
// invoke another with a {{template}} action.
//
// Because associated templates share underlying data, template construction
// cannot be done safely in parallel. Once the templates are constructed, they
// can be executed in parallel.
public static ж<Template> New(this ref Template t, @string name) {
    t.init();
    var nt = Ꮡ(new Template(
        name: name,
        common: t.common,
        leftDelim: t.leftDelim,
        rightDelim: t.rightDelim
    ));
    return nt;
}

// init guarantees that t has a valid common structure.
internal static void init(this ref Template t) {
    if (t.common == nil) {
        var c = @new<common>();
        c.Value.tmpl = new map<@string, ж<Template>>();
        c.Value.parseFuncs = new FuncMap(0);
        c.Value.execFuncs = new map<@string, reflectꓸValue>();
        t.common = c;
    }
}

// Clone returns a duplicate of the template, including all associated
// templates. The actual representation is not copied, but the name space of
// associated templates is, so further calls to [Template.Parse] in the copy will add
// templates to the copy but not to the original. Clone can be used to prepare
// common templates and use them with variant definitions for other templates
// by adding the variants after the clone is made.
public static (ж<Template>, error) Clone(this ж<Template> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var nt = t.copy(nil);
        nt.init();
        if (t.common == nil) {
            return (nt, default!);
        }
        Ꮡt.of(Template.ᏑmuTmpl).RLock();
        ᒐd1 = true;
        foreach (var (k, v) in t.tmpl) {
            if (k == t.name) {
                nt.Value.tmpl[t.name] = nt;
                continue;
            }
            // The associated templates share nt's common structure.
            var tmpl = v.copy((~nt).common);
            nt.Value.tmpl[k] = tmpl;
        }
        Ꮡt.of(Template.ᏑmuFuncs).RLock();
        ᒐd2 = true;
        maps.Copy<FuncMap, FuncMap, @string, any>((~nt).parseFuncs, t.parseFuncs);
        maps.Copy<map<@string, reflectꓸValue>, map<@string, reflectꓸValue>, @string, reflectꓸValue>((~nt).execFuncs, t.execFuncs);
        return (nt, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd2) Ꮡt.of(Template.ᏑmuFuncs).RUnlock(); if (ᒐd1) Ꮡt.of(Template.ᏑmuTmpl).RUnlock(); ᒐ.Run(); }
}

// copy returns a shallow copy of t, with common set to the argument.
internal static ж<Template> copy(this ref Template t, ж<common> Ꮡc) {
    return Ꮡ(new Template(
        name: t.name,
        Tree: t.Tree,
        common: Ꮡc,
        leftDelim: t.leftDelim,
        rightDelim: t.rightDelim
    ));
}

// AddParseTree associates the argument parse tree with the template t, giving
// it the specified name. If the template has not been defined, this tree becomes
// its definition. If it has been defined and already has that name, the existing
// definition is replaced; otherwise a new template is created, defined, and returned.
public static (ж<Template>, error) AddParseTree(this ж<Template> Ꮡt, @string name, ж<parse.Tree> Ꮡtree) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.init();
        Ꮡt.of(Template.ᏑmuTmpl).Lock();
        ᒐd1 = true;
        var nt = Ꮡt;
        if (name != t.name) {
            nt = t.New(name);
        }
        // Even if nt == t, we need to install it in the common.tmpl map.
        if (t.associate(nt, Ꮡtree) || (~nt).Tree == nil) {
            nt.Value.Tree = Ꮡtree;
        }
        return (nt, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.of(Template.ᏑmuTmpl).Unlock(); ᒐ.Run(); }
}

// Templates returns a slice of defined templates associated with t.
public static slice<ж<Template>> Templates(this ж<Template> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (t.common == nil) {
            return default!;
        }
        // Return a slice so we don't expose the map.
        Ꮡt.of(Template.ᏑmuTmpl).RLock();
        ᒐd1 = true;
        var m = new slice<ж<Template>>(0, len(t.tmpl));
        foreach (var (_, v) in t.tmpl) {
            m = append(m, v);
        }
        return m;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.of(Template.ᏑmuTmpl).RUnlock(); ᒐ.Run(); }
}

// Delims sets the action delimiters to the specified strings, to be used in
// subsequent calls to [Template.Parse], [Template.ParseFiles], or [Template.ParseGlob]. Nested template
// definitions will inherit the settings. An empty delimiter stands for the
// corresponding default: {{ or }}.
// The return value is the template, so calls can be chained.
public static ж<Template> Delims(this ж<Template> Ꮡt, @string left, @string right) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.init();
    t.leftDelim = left;
    t.rightDelim = right;
    return Ꮡt;
}

// Funcs adds the elements of the argument map to the template's function map.
// It must be called before the template is parsed.
// It panics if a value in the map is not a function with appropriate return
// type or if the name cannot be used syntactically as a function in a template.
// It is legal to overwrite elements of the map. The return value is the template,
// so calls can be chained.
public static ж<Template> Funcs(this ж<Template> Ꮡt, FuncMap funcMap) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.init();
        Ꮡt.of(Template.ᏑmuFuncs).Lock();
        ᒐd1 = true;
        addValueFuncs(t.execFuncs, funcMap);
        addFuncs(t.parseFuncs, funcMap);
        return Ꮡt;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.of(Template.ᏑmuFuncs).Unlock(); ᒐ.Run(); }
}

// Lookup returns the template with the given name that is associated with t.
// It returns nil if there is no such template or the template has no definition.
public static ж<Template> Lookup(this ж<Template> Ꮡt, @string name) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (t.common == nil) {
            return default!;
        }
        Ꮡt.of(Template.ᏑmuTmpl).RLock();
        ᒐd1 = true;
        return t.tmpl[name];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.of(Template.ᏑmuTmpl).RUnlock(); ᒐ.Run(); }
}

// Parse parses text as a template body for t.
// Named template definitions ({{define ...}} or {{block ...}} statements) in text
// define additional templates associated with t and are removed from the
// definition of t itself.
//
// Templates can be redefined in successive calls to Parse.
// A template definition with a body containing only white space and comments
// is considered empty and will not replace an existing template's body.
// This allows using Parse to add new named template definitions without
// overwriting the main template body.
public static (ж<Template>, error) Parse(this ж<Template> Ꮡt, @string text) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.init();
    Ꮡt.of(Template.ᏑmuFuncs).RLock();
    var (trees, err) = parse.Parse(t.name, text, t.leftDelim, t.rightDelim, t.parseFuncs, builtins());
    Ꮡt.of(Template.ᏑmuFuncs).RUnlock();
    if (err != default!) {
        return (default!, err);
    }
    // Add the newly parsed trees, including the one for t, into our common structure.
    foreach (var (name, tree) in trees) {
        {
            var (_, errΔ1) = Ꮡt.AddParseTree(name, tree); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }
    return (Ꮡt, default!);
}

// associate installs the new template into the group of templates associated
// with t. The two are already known to share the common structure.
// The boolean return value reports whether to store this tree as t.Tree.
internal static bool associate(this ref Template t, ж<Template> Ꮡnew, ж<parse.Tree> Ꮡtree) {
    ref var @new = ref Ꮡnew.DerefOrNull();
    ref var tree = ref Ꮡtree.DerefOrNull();

    if (@new.common != t.common) {
        throw panic("internal error: associate not common");
    }
    {
        var old = t.tmpl[@new.name]; if (old != nil && parse.IsEmptyTree(new parse.ListNodeжNode(tree.Root)) && (~old).Tree != nil) {
            // If a template by that name exists,
            // don't replace it with an empty template.
            return false;
        }
    }
    t.tmpl[@new.name] = Ꮡnew;
    return true;
}

} // end template_package
