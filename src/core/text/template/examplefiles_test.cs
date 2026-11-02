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
internal static ref readonly @string templateˢ => ref templateˢᵇ;
private static readonly @string templateˢᵇ = "template"u8;

internal static @string createTestDir(slice<templateFile> files) {
    GoFrame ᒐ = default;
    try {
        var (dir, err) = os.MkdirTemp(""u8, templateˢ);
        if (err != nil) {
            log.Fatal(err);
        }
        foreach (var (_, @file) in files) {
            var (f, errΔ1) = os.Create(filepath.Join(dir, @file.name));
            if (errΔ1 != nil) {
                log.Fatal(errΔ1);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            (_, errΔ1) = io.WriteString(new os.FileжWriter(f), @file.contents);
            if (errΔ1 != nil) {
                log.Fatal(errΔ1);
            }
        }
        return dir;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmplˢ => ref tmplˢᵇ;
private static readonly @string tmplˢᵇ = "*.tmpl"u8;

// Here we demonstrate loading a set of templates from a directory.
public static void ExampleTemplate_glob() {
    GoFrame ᒐ = default;
    try {
        // Here we create a temporary directory and populate it with our sample
        // template definition files; usually the template files would already
        // exist in some location known to the program.
        @string dir = createTestDir(slice<templateFile>([
            // T0.tmpl is a plain template file that just invokes T1.
            new("T0.tmpl", @"T0 invokes T1: ({{template ""T1""}})"),
            // T1.tmpl defines a template, T1 that invokes T2.
            new("T1.tmpl", @"{{define ""T1""}}T1 invokes T2: ({{template ""T2""}}){{end}}"),
            // T2.tmpl defines a template T2.
            new("T2.tmpl", @"{{define ""T2""}}This is T2{{end}}")
        ]));
        // Clean up after the test; another quirk of running as an example.
        defer(os.RemoveAll, dir, ref ᒐ);

        // pattern is the glob pattern used to find all the template files.
        @string pattern = filepath.Join(dir, tmplˢ);

        // Here starts the example proper.
        // T0.tmpl is the first name matched, so it becomes the starting template,
        // the value returned by ParseGlob.
        var (ᴛ1, ᴛ2) = Δtemplate.ParseGlob(pattern);
        var tmpl = Δtemplate.Must(ᴛ1, ᴛ2);

        var err = tmpl.Execute(new os.FileжWriter(os.Stdout), default);
        if (err != nil) {
            log.Fatalf("template execution: %s", err);
        }
        // Output:
        // T0 invokes T1: (T1 invokes T2: (This is T2))
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string defineDriver1Driver1ˢ => ref defineDriver1Driver1ˢᵇ;
private static readonly @string defineDriver1Driver1ˢᵇ = "{{define `driver1`}}Driver 1 calls T1: ({{template `T1`}})\n{{end}}"u8;
internal static readonly object parsingDriver1ˢ = (@string)"parsing driver1: "u8;
internal static ref readonly @string defineDriver2Driver2ˢ => ref defineDriver2Driver2ˢᵇ;
private static readonly @string defineDriver2Driver2ˢᵇ = "{{define `driver2`}}Driver 2 calls T2: ({{template `T2`}})\n{{end}}"u8;
internal static readonly object parsingDriver2ˢ = (@string)"parsing driver2: "u8;
internal static ref readonly @string driver1ˢ => ref driver1ˢᵇ;
private static readonly @string driver1ˢᵇ = "driver1"u8;
internal static ref readonly @string driver2ˢ => ref driver2ˢᵇ;
private static readonly @string driver2ˢᵇ = "driver2"u8;

// This example demonstrates one way to share some templates
// and use them in different contexts. In this variant we add multiple driver
// templates by hand to an existing bundle of templates.
public static void ExampleTemplate_helpers() {
    GoFrame ᒐ = default;
    try {
        // Here we create a temporary directory and populate it with our sample
        // template definition files; usually the template files would already
        // exist in some location known to the program.
        @string dir = createTestDir(slice<templateFile>([
            // T1.tmpl defines a template, T1 that invokes T2.
            new("T1.tmpl", @"{{define ""T1""}}T1 invokes T2: ({{template ""T2""}}){{end}}"),
            // T2.tmpl defines a template T2.
            new("T2.tmpl", @"{{define ""T2""}}This is T2{{end}}")
        ]));
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
        if (err != nil) {
            log.Fatal(parsingDriver1ˢ, err);
        }
        // Add another driver template.
        (_, err) = templates.Parse(defineDriver2Driver2ˢ);
        if (err != nil) {
            log.Fatal(parsingDriver2ˢ, err);
        }
        // We load all the templates before execution. This package does not require
        // that behavior but html/template's escaping does, so it's a good habit.
        err = templates.ExecuteTemplate(new os.FileжWriter(os.Stdout), driver1ˢ, default);
        if (err != nil) {
            log.Fatalf("driver1 execution: %s", err);
        }
        err = templates.ExecuteTemplate(new os.FileжWriter(os.Stdout), driver2ˢ, default);
        if (err != nil) {
            log.Fatalf("driver2 execution: %s", err);
        }
        // Output:
        // Driver 1 calls T1: (T1 invokes T2: (This is T2))
        // Driver 2 calls T2: (This is T2)
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cloningHelpersˢ = (@string)"cloning helpers: "u8;
internal static ref readonly @string defineT2T2VersionAEndˢ => ref defineT2T2VersionAEndˢᵇ;
private static readonly @string defineT2T2VersionAEndˢᵇ = "{{define `T2`}}T2, version A{{end}}"u8;
internal static readonly object parsingT2ˢ = (@string)"parsing T2: "u8;
internal static readonly object cloningDriversˢ = (@string)"cloning drivers: "u8;
internal static ref readonly @string defineT2T2VersionBEndˢ => ref defineT2T2VersionBEndˢᵇ;
private static readonly @string defineT2T2VersionBEndˢᵇ = "{{define `T2`}}T2, version B{{end}}"u8;
internal static ref readonly @string t0Tmplˢ => ref t0Tmplˢᵇ;
private static readonly @string t0Tmplˢᵇ = "T0.tmpl"u8;
internal static readonly object secondˢ = (@string)"second"u8;
internal static readonly object firstˢ = (@string)"first"u8;

// This example demonstrates how to use one group of driver
// templates with distinct sets of helper templates.
public static void ExampleTemplate_share() {
    GoFrame ᒐ = default;
    try {
        // Here we create a temporary directory and populate it with our sample
        // template definition files; usually the template files would already
        // exist in some location known to the program.
        @string dir = createTestDir(slice<templateFile>([
            // T0.tmpl is a plain template file that just invokes T1.
            new("T0.tmpl", "T0 ({{.}} version) invokes T1: ({{template `T1`}})\n"),
            // T1.tmpl defines a template, T1 that invokes T2. Note T2 is not defined
            new("T1.tmpl", @"{{define ""T1""}}T1 invokes T2: ({{template ""T2""}}){{end}}")
        ]));
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
        if (err != nil) {
            log.Fatal(cloningHelpersˢ, err);
        }
        // 2. Define T2, version A, and parse it.
        (_, err) = first.Parse(defineT2T2VersionAEndˢ);
        if (err != nil) {
            log.Fatal(parsingT2ˢ, err);
        }

        // Now repeat the whole thing, using a different version of T2.
        // 1. Clone the drivers.
        (var second, err) = drivers.Clone();
        if (err != nil) {
            log.Fatal(cloningDriversˢ, err);
        }
        // 2. Define T2, version B, and parse it.
        (_, err) = second.Parse(defineT2T2VersionBEndˢ);
        if (err != nil) {
            log.Fatal(parsingT2ˢ, err);
        }

        // Execute the templates in the reverse order to verify the
        // first is unaffected by the second.
        err = second.ExecuteTemplate(new os.FileжWriter(os.Stdout), t0Tmplˢ, secondˢ);
        if (err != nil) {
            log.Fatalf("second execution: %s", err);
        }
        err = first.ExecuteTemplate(new os.FileжWriter(os.Stdout), t0Tmplˢ, firstˢ);
        if (err != nil) {
            log.Fatalf("first: execution: %s", err);
        }

        // Output:
        // T0 (second version) invokes T1: (T1 invokes T2: (T2, version B))
        // T0 (first version) invokes T1: (T1 invokes T2: (T2, version A))
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end template_test_package
