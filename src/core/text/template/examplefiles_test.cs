// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using io = io_package;
using log = log_package;
using os = os_package;
using filepath = path.filepath_package;
using Δtemplate = go.text.template_package;
using go.text;
using path;
using static go.text.template_internal_test_package;

partial class template_test_package {

// templateFile defines the contents of a template to be stored in a file, for testing.
partial struct templateFile {
    internal @string name;
    internal @string contents;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string templateˢ = "template"u8;

internal static @string createTestDir(slice<templateFile> files) {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = os.MkdirTemp(""u8, templateˢ);
        if (err != default!) {
            log.Fatal(err);
        }
        foreach (var (_, @file) in files) {
            var (f, errΔ1) = os.Create(filepath.Join(dir, @file.name));
            if (errΔ1 != default!) {
                log.Fatal(errΔ1);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            (_, errΔ1) = io.WriteString(new os.FileжWriter(f), @file.contents);
            if (errΔ1 != default!) {
                log.Fatal(errΔ1);
            }
        }
        return dir;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmplˢ = "*.tmpl"u8;

// Here we demonstrate loading a set of templates from a directory.
public static void ExampleTemplate_glob() {
    GoFrame ᒐ = default;
    try {
        // Here we create a temporary directory and populate it with our sample
        // template definition files; usually the template files would already
        // exist in some location known to the program.
        @string dir = createTestDir(new templateFile[]{ // T0.tmpl is a plain template file that just invokes T1.

            new("T0.tmpl"u8, @"T0 invokes T1: ({{template ""T1""}})"u8), // T1.tmpl defines a template, T1 that invokes T2.

            new("T1.tmpl"u8, @"{{define ""T1""}}T1 invokes T2: ({{template ""T2""}}){{end}}"u8), // T2.tmpl defines a template T2.

            new("T2.tmpl"u8, @"{{define ""T2""}}This is T2{{end}}"u8)
        }.slice());
        // Clean up after the test; another quirk of running as an example.
        defer(os.RemoveAll, dir, ref ᒐ);
        // pattern is the glob pattern used to find all the template files.
        @string pattern = filepath.Join(dir, tmplˢ);
        // Here starts the example proper.
        // T0.tmpl is the first name matched, so it becomes the starting template,
        // the value returned by ParseGlob.
        var (ᴛ1, ᴛ2) = Δtemplate.ParseGlob(pattern);
        var tmpl = Δtemplate.Must(ᴛ1, ᴛ2);
        var err = tmpl.Execute(new os.FileжWriter(os.Stdout), default!);
        if (err != default!) {
            log.Fatalf("template execution: %s"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defineDriver1Driver1ˢ = "{{define `driver1`}}Driver 1 calls T1: ({{template `T1`}})\n{{end}}"u8;
internal static readonly object parsingDriver1ˢ = (@string)"parsing driver1: "u8;
internal static readonly @string defineDriver2Driver2ˢ = "{{define `driver2`}}Driver 2 calls T2: ({{template `T2`}})\n{{end}}"u8;
internal static readonly object parsingDriver2ˢ = (@string)"parsing driver2: "u8;
internal static readonly @string driver1ˢ = "driver1"u8;
internal static readonly @string driver2ˢ = "driver2"u8;

// Output:
// T0 invokes T1: (T1 invokes T2: (This is T2))

// This example demonstrates one way to share some templates
// and use them in different contexts. In this variant we add multiple driver
// templates by hand to an existing bundle of templates.
public static void ExampleTemplate_helpers() {
    GoFrame ᒐ = default;
    try {
        // Here we create a temporary directory and populate it with our sample
        // template definition files; usually the template files would already
        // exist in some location known to the program.
        @string dir = createTestDir(new templateFile[]{ // T1.tmpl defines a template, T1 that invokes T2.

            new("T1.tmpl"u8, @"{{define ""T1""}}T1 invokes T2: ({{template ""T2""}}){{end}}"u8), // T2.tmpl defines a template T2.

            new("T2.tmpl"u8, @"{{define ""T2""}}This is T2{{end}}"u8)
        }.slice());
        // Clean up after the test; another quirk of running as an example.
        defer(os.RemoveAll, dir, ref ᒐ);
        // pattern is the glob pattern used to find all the template files.
        @string pattern = filepath.Join(dir, tmplˢ);
        // Here starts the example proper.
        // Load the helpers.
        var (ᴛ3, ᴛ4) = Δtemplate.ParseGlob(pattern);
        var templates = Δtemplate.Must(ᴛ3, ᴛ4);
        // Add one driver template to the bunch; we do this with an explicit template definition.
        var (_, err) = templates.Parse(defineDriver1Driver1ˢ);
        if (err != default!) {
            log.Fatal(parsingDriver1ˢ, err);
        }
        // Add another driver template.
        (_, err) = templates.Parse(defineDriver2Driver2ˢ);
        if (err != default!) {
            log.Fatal(parsingDriver2ˢ, err);
        }
        // We load all the templates before execution. This package does not require
        // that behavior but html/template's escaping does, so it's a good habit.
        err = templates.ExecuteTemplate(new os.FileжWriter(os.Stdout), driver1ˢ, default!);
        if (err != default!) {
            log.Fatalf("driver1 execution: %s"u8, err);
        }
        err = templates.ExecuteTemplate(new os.FileжWriter(os.Stdout), driver2ˢ, default!);
        if (err != default!) {
            log.Fatalf("driver2 execution: %s"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cloningHelpersˢ = (@string)"cloning helpers: "u8;
internal static readonly @string defineT2T2VersionAEndˢ = "{{define `T2`}}T2, version A{{end}}"u8;
internal static readonly object parsingT2ˢ = (@string)"parsing T2: "u8;
internal static readonly object cloningDriversˢ = (@string)"cloning drivers: "u8;
internal static readonly @string defineT2T2VersionBEndˢ = "{{define `T2`}}T2, version B{{end}}"u8;
internal static readonly @string t0Tmplˢ = "T0.tmpl"u8;
internal static readonly object secondˢ = (@string)"second"u8;
internal static readonly object firstˢ = (@string)"first"u8;

// Output:
// Driver 1 calls T1: (T1 invokes T2: (This is T2))
// Driver 2 calls T2: (This is T2)

// This example demonstrates how to use one group of driver
// templates with distinct sets of helper templates.
public static void ExampleTemplate_share() {
    GoFrame ᒐ = default;
    try {
        // Here we create a temporary directory and populate it with our sample
        // template definition files; usually the template files would already
        // exist in some location known to the program.
        @string dir = createTestDir(new templateFile[]{ // T0.tmpl is a plain template file that just invokes T1.

            new("T0.tmpl"u8, "T0 ({{.}} version) invokes T1: ({{template `T1`}})\n"u8), // T1.tmpl defines a template, T1 that invokes T2. Note T2 is not defined

            new("T1.tmpl"u8, @"{{define ""T1""}}T1 invokes T2: ({{template ""T2""}}){{end}}"u8)
        }.slice());
        // Clean up after the test; another quirk of running as an example.
        defer(os.RemoveAll, dir, ref ᒐ);
        // pattern is the glob pattern used to find all the template files.
        @string pattern = filepath.Join(dir, tmplˢ);
        // Here starts the example proper.
        // Load the drivers.
        var (ᴛ5, ᴛ6) = Δtemplate.ParseGlob(pattern);
        var drivers = Δtemplate.Must(ᴛ5, ᴛ6);
        // We must define an implementation of the T2 template. First we clone
        // the drivers, then add a definition of T2 to the template name space.
        // 1. Clone the helper set to create a new name space from which to run them.
        var (first, err) = drivers.Clone();
        if (err != default!) {
            log.Fatal(cloningHelpersˢ, err);
        }
        // 2. Define T2, version A, and parse it.
        (_, err) = first.Parse(defineT2T2VersionAEndˢ);
        if (err != default!) {
            log.Fatal(parsingT2ˢ, err);
        }
        // Now repeat the whole thing, using a different version of T2.
        // 1. Clone the drivers.
        (var second, err) = drivers.Clone();
        if (err != default!) {
            log.Fatal(cloningDriversˢ, err);
        }
        // 2. Define T2, version B, and parse it.
        (_, err) = second.Parse(defineT2T2VersionBEndˢ);
        if (err != default!) {
            log.Fatal(parsingT2ˢ, err);
        }
        // Execute the templates in the reverse order to verify the
        // first is unaffected by the second.
        err = second.ExecuteTemplate(new os.FileжWriter(os.Stdout), t0Tmplˢ, secondˢ);
        if (err != default!) {
            log.Fatalf("second execution: %s"u8, err);
        }
        err = first.ExecuteTemplate(new os.FileжWriter(os.Stdout), t0Tmplˢ, firstˢ);
        if (err != default!) {
            log.Fatalf("first: execution: %s"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Output:
// T0 (second version) invokes T1: (T1 invokes T2: (T2, version B))
// T0 (first version) invokes T1: (T1 invokes T2: (T2, version A))

} // end template_test_package
