// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using fmt = fmt_package;
using parse = text.template.parse_package;
using text.template;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// Error describes a problem encountered during template Escaping.
partial struct ΔError {
    // ErrorCode describes the kind of error.
    public ErrorCode ErrorCode;
    // Node is the node that caused the problem, if known.
    // If not nil, it overrides Name and Line.
    public parse.Node Node;
    // Name is the name of the template in which the error was encountered.
    public @string Name;
    // Line is the line number of the error in the template source or 0.
    public nint Line;
    // Description is a human-readable description of the problem.
    public @string Description;
}

// ErrorCode is a code for a kind of error.
partial struct ErrorCode /*num:nint*/;

// We define codes for each error that manifests while escaping templates, but
// escaped templates may also fail at runtime.
//
// Output: "ZgotmplZ"
// Example:
//
//	<img src="{{.X}}">
//	where {{.X}} evaluates to `javascript:...`
//
// Discussion:
//
//	"ZgotmplZ" is a special value that indicates that unsafe content reached a
//	CSS or URL context at runtime. The output of the example will be
//	  <img src="#ZgotmplZ">
//	If the data comes from a trusted source, use content types to exempt it
//	from filtering: URL(`javascript:...`).
// OK indicates the lack of an error.
public static ErrorCode OK => /* iota */ 0;

// ErrAmbigContext: "... appears in an ambiguous context within a URL"
// Example:
//   <a href="
//      {{if .C}}
//        /path/
//      {{else}}
//        /search?q=
//      {{end}}
//      {{.X}}
//   ">
// Discussion:
//   {{.X}} is in an ambiguous URL context since, depending on {{.C}},
//  it may be either a URL suffix or a query parameter.
//   Moving {{.X}} into the condition removes the ambiguity:
//   <a href="{{if .C}}/path/{{.X}}{{else}}/search?q={{.X}}">
public static ErrorCode ErrAmbigContext => 1;

// ErrBadHTML: "expected space, attr name, or end of tag, but got ...",
//   "... in unquoted attr", "... in attribute name"
// Example:
//   <a href = /search?q=foo>
//   <href=foo>
//   <form na<e=...>
//   <option selected<
// Discussion:
//   This is often due to a typo in an HTML element, but some runes
//   are banned in tag names, attribute names, and unquoted attribute
//   values because they can tickle parser ambiguities.
//   Quoting all attributes is the best policy.
public static ErrorCode ErrBadHTML => 2;

// ErrBranchEnd: "{{if}} branches end in different contexts"
// Examples:
//   {{if .C}}<a href="{{end}}{{.X}}
//   <script {{with .T}}type="{{.}}"{{end}}>
// Discussion:
//   Package html/template statically examines each path through an
//   {{if}}, {{range}}, or {{with}} to escape any following pipelines.
//   The first example is ambiguous since {{.X}} might be an HTML text node,
//   or a URL prefix in an HTML attribute. The context of {{.X}} is
//   used to figure out how to escape it, but that context depends on
//   the run-time value of {{.C}} which is not statically known.
//   The second example is ambiguous as the script type attribute
//   can change the type of escaping needed for the script contents.
//
//   The problem is usually something like missing quotes or angle
//   brackets, or can be avoided by refactoring to put the two contexts
//   into different branches of an if, range or with. If the problem
//   is in a {{range}} over a collection that should never be empty,
//   adding a dummy {{else}} can help.
public static ErrorCode ErrBranchEnd => 3;

// ErrEndContext: "... ends in a non-text context: ..."
// Examples:
//   <div
//   <div title="no close quote>
//   <script>f()
// Discussion:
//   Executed templates should produce a DocumentFragment of HTML.
//   Templates that end without closing tags will trigger this error.
//   Templates that should not be used in an HTML context or that
//   produce incomplete Fragments should not be executed directly.
//
//   {{define "main"}} <script>{{template "helper"}}</script> {{end}}
//   {{define "helper"}} document.write(' <div title=" ') {{end}}
//
//   "helper" does not produce a valid document fragment, so should
//   not be Executed directly.
public static ErrorCode ErrEndContext => 4;

// ErrNoSuchTemplate: "no such template ..."
// Examples:
//   {{define "main"}}<div {{template "attrs"}}>{{end}}
//   {{define "attrs"}}href="{{.URL}}"{{end}}
// Discussion:
//   Package html/template looks through template calls to compute the
//   context.
//   Here the {{.URL}} in "attrs" must be treated as a URL when called
//   from "main", but you will get this error if "attrs" is not defined
//   when "main" is parsed.
public static ErrorCode ErrNoSuchTemplate => 5;

// ErrOutputContext: "cannot compute output context for template ..."
// Examples:
//   {{define "t"}}{{if .T}}{{template "t" .T}}{{end}}{{.H}}",{{end}}
// Discussion:
//   A recursive template does not end in the same context in which it
//   starts, and a reliable output context cannot be computed.
//   Look for typos in the named template.
//   If the template should not be called in the named start context,
//   look for calls to that template in unexpected contexts.
//   Maybe refactor recursive templates to not be recursive.
public static ErrorCode ErrOutputContext => 6;

// ErrPartialCharset: "unfinished JS regexp charset in ..."
// Example:
//     <script>var pattern = /foo[{{.Chars}}]/</script>
// Discussion:
//   Package html/template does not support interpolation into regular
//   expression literal character sets.
public static ErrorCode ErrPartialCharset => 7;

// ErrPartialEscape: "unfinished escape sequence in ..."
// Example:
//   <script>alert("\{{.X}}")</script>
// Discussion:
//   Package html/template does not support actions following a
//   backslash.
//   This is usually an error and there are better solutions; for
//   example
//     <script>alert("{{.X}}")</script>
//   should work, and if {{.X}} is a partial escape sequence such as
//   "xA0", mark the whole sequence as safe content: JSStr(`\xA0`)
public static ErrorCode ErrPartialEscape => 8;

// ErrRangeLoopReentry: "on range loop re-entry: ..."
// Example:
//   <script>var x = [{{range .}}'{{.}},{{end}}]</script>
// Discussion:
//   If an iteration through a range would cause it to end in a
//   different context than an earlier pass, there is no single context.
//   In the example, there is missing a quote, so it is not clear
//   whether {{.}} is meant to be inside a JS string or in a JS value
//   context. The second iteration would produce something like
//
//     <script>var x = ['firstValue,'secondValue]</script>
public static ErrorCode ErrRangeLoopReentry => 9;

// ErrSlashAmbig: '/' could start a division or regexp.
// Example:
//   <script>
//     {{if .C}}var x = 1{{end}}
//     /-{{.N}}/i.test(x) ? doThis : doThat();
//   </script>
// Discussion:
//   The example above could produce `var x = 1/-2/i.test(s)...`
//   in which the first '/' is a mathematical division operator or it
//   could produce `/-2/i.test(s)` in which the first '/' starts a
//   regexp literal.
//   Look for missing semicolons inside branches, and maybe add
//   parentheses to make it clear which interpretation you intend.
public static ErrorCode ErrSlashAmbig => 10;

// ErrPredefinedEscaper: "predefined escaper ... disallowed in template"
// Example:
//   <div class={{. | html}}>Hello<div>
// Discussion:
//   Package html/template already contextually escapes all pipelines to
//   produce HTML output safe against code injection. Manually escaping
//   pipeline output using the predefined escapers "html" or "urlquery" is
//   unnecessary, and may affect the correctness or safety of the escaped
//   pipeline output in Go 1.8 and earlier.
//
//   In most cases, such as the given example, this error can be resolved by
//   simply removing the predefined escaper from the pipeline and letting the
//   contextual autoescaper handle the escaping of the pipeline. In other
//   instances, where the predefined escaper occurs in the middle of a
//   pipeline where subsequent commands expect escaped input, e.g.
//     {{.X | html | makeALink}}
//   where makeALink does
//     return `<a href="`+input+`">link</a>`
//   consider refactoring the surrounding template to make use of the
//   contextual autoescaper, i.e.
//     <a href="{{.X}}">link</a>
//
//   To ease migration to Go 1.9 and beyond, "html" and "urlquery" will
//   continue to be allowed as the last command in a pipeline. However, if the
//   pipeline occurs in an unquoted attribute value context, "html" is
//   disallowed. Avoid using "html" and "urlquery" entirely in new templates.
public static ErrorCode ErrPredefinedEscaper => 11;

// ErrJSTemplate: "... appears in a JS template literal"
// Example:
//     <script>var tmpl = `{{.Interp}}`</script>
// Discussion:
//   Package html/template does not support actions inside of JS template
//   literals.
//
// Deprecated: ErrJSTemplate is no longer returned when an action is present
// in a JS template literal. Actions inside of JS template literals are now
// escaped as expected.
public static ErrorCode ErrJSTemplate => 12;

public static @string Error(this ref ΔError e) {
    switch (ᐧ) {
    case {} when e.Node != default!: {
        var (loc, _) = (((ж<parse.Tree>)nil)).ErrorContext(e.Node);
        return fmt.Sprintf("html/template:%s: %s"u8, loc, e.Description);
    }
    case {} when e.Line is not 0: {
        return fmt.Sprintf("html/template:%s:%d: %s"u8, e.Name, e.Line, e.Description);
    }
    case {} when e.Name != ""u8: {
        return fmt.Sprintf("html/template:%s: %s"u8, e.Name, e.Description);
    }}
    return "html/template: "u8 + e.Description;
}

// errorf creates an error given a format string f and args.
// The template Name still needs to be supplied.
internal static ж<ΔError> errorf(ErrorCode k, parse.Node node, nint line, @string f, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return Ꮡ(new ΔError(k, node, ""u8, line, fmt.Sprintf(f, args.ꓸꓸꓸ)));
}

} // end template_package
