// InterfaceImplTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Text;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InterfaceImpl;

internal class InterfaceImplTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string InterfaceName;

    // The name of the struct FIELD that embeds InterfaceName, when it differs from the interface's
    // simple name; null derives it from the name as before.
    //
    // The embed field is normally named after its interface type, so dropping the Δ collision prefix
    // is enough. A FUNCTION-LOCAL interface lift breaks that: the converter renames the TYPE to
    // `<Func>_<name>` while the field keeps `<name>`, so reflect's
    // `func TestCallPanic() { type T1 interface{…}; type T2 struct { T1 } }` emits field `T1` of type
    // `TestCallPanic_T1` and the derived name addresses nothing — CS1061 on every promoted twin,
    // CS0120 behind it. A name is the wrong oracle for a lifted type, so ImplementGenerator resolves
    // this where the SYMBOLS are and hands the answer down, exactly as it already does for
    // StructIsPublic / InterfaceIsPublic.
    public string? EmbedFieldName;

    // The embed field the promoted/forwarding bodies address: the resolved name when the generator
    // supplied one, else the interface's simple name with its Δ collision prefix dropped.
    private string EmbedField => EmbedFieldName ?? GetSimpleName(InterfaceName, dropCollisionPrefix: true);

    public required bool Promoted;
    public required HashSet<string> Overrides;
    public required List<MethodInfo> Methods;

    // Single embedded-pointer hop property (`Type` for rtype's `*abi.Type`): an interface member
    // with no direct struct method forwards through it (`this.Type.Value.M()`), matching the
    // converter's syntax-resolved promotion at Go call sites. Null when no (single) hop exists.
    public string? EmbedHop;

    // Hop-target methods that are direct-ж primaries (extensions on ж<X> with no ref twin) bind
    // the box field itself — `this.File.Read(p)`; deref'ing first strands the receiver (CS1929).
    public HashSet<string> EmbedHopBoxMethods = [];

    // Methods declared on a VALUE-embedded field of the hop type with a POINTER receiver bind a
    // projected field box — `this.TCPConn.of(TCPConn.Ꮡconn).Read(p)` (net CS1929 x2); the map
    // carries the `.of(…)` suffix per method name.
    public Dictionary<string, string>? EmbedHopDeepPaths;

    // With SEVERAL embedded pointers there is no single hop to name, so the receiver is decided per
    // member: each routes to the UNIQUE embed declaring it (`this.PipeReader.Read(p)`), Go's depth-1
    // promotion rule. Empty for one embed or none — that case keeps EmbedHop above.
    public Dictionary<string, string>? MultiEmbedHopPaths;

    // Single VALUE-embedded field (`addrPortUDPAddr struct { netip.AddrPort }`): an interface
    // member with no direct struct method promotes through it (`this.AddrPort.String()`).
    public string? ValueEmbedHop;

    // Non-null when the value embed's type is FOREIGN (dotted) - the extension lives in another
    // namespace segment the file only aliases, so the forwarding calls the package-class static
    // directly: `global::go.net.netip_package.String(this.AddrPort)`.
    public string? ValueEmbedHopStaticClass;

    // Both sides' effective accessibility, resolved by the GENERATOR from the SYMBOLS (see
    // ImplementGenerator.AdapterSidePublic / Common.EffectiveScopeIsPublic). The template cannot ask
    // that question itself — it holds only names — and a name is the wrong oracle for a lifted
    // function-local type, which is the very trap the twins below are `internal` to avoid.
    public bool StructIsPublic;
    public bool InterfaceIsPublic;

    // The promoted-method extension twins below are ALWAYS `internal`, and that is a decision,
    // not a default. The twin is reflective cargo only: the registry that builds the Go method
    // set discovers with BindingFlags.NonPublic, and no source-level call ever binds it — a
    // direct call always prefers the MEMBER the partial struct declares. So accessibility here
    // decides declaration form, never discoverability — and `internal` is the one form that can
    // never out-rank its own parameter type.
    //
    // The first cut scoped the twin to the struct via `DeclaredAccessibility == Public ||
    // GetScope(name) == "public"` — the adapterScope shape — and the name half is a trap this
    // repo has already paid for once (StructTypeTemplate's scope-follows-the-MEMBER note): a
    // FUNCTION-LOCAL type hoists to `<Func>_<name>`, whose leading case belongs to the enclosing
    // function. A test's local witness struct (`TestEncoderDecoder_r`, hoisted from `func
    // TestEncoderDecoder`) is declared `internal`, but the heuristic read the `T` and emitted a
    // `public` twin over it — CS0051, in every -tests host whose table-driven suite wraps an
    // embedded interface in a local type (encoding/hex, archive/zip, compress/flate,
    // net/rpc/jsonrpc). The behavioral corpus never produces that combination in a production
    // program, which is why 607 projects and the reflect canary compiled clean around it.

    public override string TemplateBody =>
        $$"""
             partial struct {{StructName}} : {{InterfaceName}}
             {
                 {{MethodsImplementation}}{{Comparisions}}
             }
         """;

    // The twins land in a SIBLING top-level static class, never inside the package class — and
    // that placement is load-bearing (Shape B of the JOB-010 consolidation, io/fs). A static
    // method in the package class participates in BARE-name lookup for every call in that class,
    // and enclosing-class members out-rank `using static` imports: io/fs's dot-imported test
    // calls `Sub(fsys, dir)` bare, `subOnly` embeds `SubFS` whose method is named `Sub`, and the
    // twin `Sub(subOnly, @string)` INTERCEPTED the package function — CS1503 on the call whose
    // argument was a different type, and a silent WRONG BINDING on the call whose argument
    // matched (the twin forwards to the embedded interface's method, skipping whatever the
    // package function does around it — fs.Sub validates the path before dispatching). The
    // compiling half was wronger than the failing half. A sibling class is invisible to bare
    // lookup, and costs the twins nothing: no source-level call ever binds them (the MEMBER
    // wins), and the registry scans every non-nested sealed static class in the assembly.
    public override string TemplateFooter =>
        $$"""

        }{{PromotedTwinsClass}}
        {{PackageFooter ?? ""}}
        """;

    private string PromotedTwinsClass
    {
        get
        {
            string twins = PromotedExtensionMethods;

            if (twins.Length == 0)
                return "";

            // Named for the OWNING PACKAGE plus the pair, so two generated files can never collide;
            // generic arity and keyword escapes cannot appear in a class name, so they are stripped —
            // the name only has to be unique and legal, never resolvable back to anything.
            //
            // The package half is what makes "can never collide" true rather than merely intended.
            // This class lands at NAMESPACE scope (see TemplateFooter — a sibling class, deliberately
            // outside the package class), while the pair it is named for is scoped to a package CLASS,
            // and one namespace holds several of those. A `-tests` conversion is where that bites: Go's
            // internal (`package http`) and external (`package http_test`) test variants are DISTINCT
            // packages that may each declare a type of the same name, and they emit correctly into
            // distinct classes — net/http declares `dumpConn` in both `requestwrite_test.go` and
            // `transport_test.go`. Each promotes io.Reader and io.Writer, so the pair name alone
            // produced `dumpConnᴛReaderᴛpromoted` and `dumpConnᴛWriterᴛpromoted` twice in namespace
            // `go.net` — CS0101 ×2, the last wall in front of net/http's test suite. Nothing about the
            // type model is wrong there; only this cargo name was under-qualified.
            //
            // Qualifying UNCONDITIONALLY rather than on a measured collision, which is the opposite of
            // the collision-conditional rule the pointer-ADAPTER names follow: an adapter name appears
            // at thousands of construction sites, so renaming it has real blast radius, whereas no
            // source-level call ever binds a twin (the struct MEMBER always wins) and the method-set
            // registry discovers these by scanning every non-nested static class, never by name. The
            // rename is therefore invisible everywhere except in this declaration.
            string packagePart = PackageName.Replace("@", "");
            string structPart = StructName.Split('<')[0].Replace("@", "");
            string interfacePart = GetSimpleName(InterfaceName).Replace("@", "");

            return $"\r\n\r\ninternal static class {packagePart}{TempVarMarker}{structPart}{TempVarMarker}{interfacePart}{TempVarMarker}promoted\r\n{{{twins}\r\n}}";
        }
    }

    // A PROMOTED interface method is a Go method of the struct, and it is the ONE kind of Go method
    // that never became an extension method — the member above satisfies the C# interface, but
    // go2cs's runtime method set is built from EXTENSION methods alone
    // (TypeExtensions.GetGoMethodSetCandidates), so a promoted method was invisible to every
    // structural question asked about the type.
    //
    // That is not a cosmetic gap. `builtin.Implements<T>` answers a direct assert with C# `is T`,
    // which succeeds for the embedded interface itself; ANY OTHER interface falls to the structural
    // probe, which counted only the directly-declared methods. A type embedding `net.Conn` and
    // adding ReadFrom/WriteTo therefore failed `c.(net.PacketConn)` — Go takes the UDP arm, the
    // conversion took TCP framing (F2). The twin below closes it by making the promoted method an
    // ordinary Go method: the probe finds it, AdapterBinder binds it through the same candidate
    // source, and reflect's NumMethod counts it, which is what Go does too.
    //
    // Emitting a member AND an extension of one name is legal and deliberate: C# prefers the member
    // at every direct call site, so behavior is unchanged there, while the extension exists for the
    // reflection-sourced registry. Accessibility follows the STRUCT, matching the converter's own
    // rule for that type's methods; the registry admits non-public extensions
    // (BindingFlags.NonPublic in ExtensionMethodRegistry), so an unexported type's twin is still
    // discovered by a foreign assembly's assert — which is precisely where F2 bites.
    //
    // Shadowing is Go's rule and is already decided: `Overrides` holds the struct's own declared
    // methods, and a member it declares itself needs no twin (it has one — the converter emitted
    // it). Only the !methodOverriden arm — the one that actually promotes — gets a twin.
    private string PromotedExtensionMethods
    {
        get
        {
            if (!Promoted)
                return "";

            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                string simpleMethodName = GetSimpleName(method.Name);

                if (Overrides.Contains(method.ForwardMemberName(simpleMethodName)))
                    continue;

                string receiver = $"recv{TempVarMarker}";

                // The twins live in a SIBLING class (see PromotedTwinsClass), so the struct —
                // nested in the package class — must be named through it.
                string qualifiedStruct = $"{PackageName}_package.{StructName}";

                string typedParameters = method.GetTypedParameters(false);
                string parameterList = string.IsNullOrEmpty(typedParameters) ?
                    $"this {qualifiedStruct} {receiver}" :
                    $"this {qualifiedStruct} {receiver}, {typedParameters}";

                string callParameters = method.GetCallParameters(false);
                string embedField = EmbedField;

                result.Append($"\r\n\r\n    // Go method set entry for the promoted '{GetSimpleName(InterfaceName)}.{simpleMethodName}()':\r\n");
                result.Append($"    internal static {method.ReturnType} {EscapeCsKeyword(simpleMethodName)}{method.GetGenericSignature()}({parameterList}){method.GetWhereConstraints()} => ");
                result.Append($"{receiver}.{embedField}.{simpleMethodName}{method.GetGenericSignature()}({callParameters});");
            }

            return result.ToString();
        }
    }

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                string simpleInterfaceName = GetSimpleName(InterfaceName);
                string simpleMethodName = GetSimpleName(method.Name);

                // Implemented under the interface's name, forwarded under the EMITTED one when the
                // collision pass renamed the implementation — see MethodInfo.ForwardName. `Overrides`
                // is keyed by the DECLARED names, so the resolved name is what decides whether this
                // member has a direct struct method at all; a Go-name miss read a renamed declaration
                // as absent and sent the member down the promotion path instead. Non-null only when
                // the struct itself declares the Δ name, which is exactly when `methodOverriden`
                // becomes true and every embed hop below is skipped — so the hop lookups keep reading
                // the interface member's own name, which is what the EMBEDDED type declares it under.
                string forwardName = method.ForwardMemberName(simpleMethodName);
                bool methodOverriden = Overrides.Contains(forwardName);

                if (result.Length > 0)
                    result.Append("\r\n\r\n        ");

                if (Promoted && !methodOverriden)
                {
                    // The forwarding receiver is the embedded interface FIELD, which carries the
                    // Go embed name — the Δ-stripped simple name when the interface TYPE was
                    // collision-renamed (bare `ΔHandler.Enabled(…)` binds nothing, CS0103 —
                    // slogtest's `wrapper` embeds slog.ΔHandler as field `Handler`).
                    result.Append($"// '{simpleInterfaceName}.{simpleMethodName}()' implicit implementation mapped to promoted interface receiver method:\r\n        ");
                    result.Append($"[{NonUserCodeAttribute}] public {method.ReturnType} {method.GetSignature()} => {EmbedField}.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
                }
                else
                {
                    if (Promoted && methodOverriden)
                    {
                        result.Append($"// '{simpleInterfaceName}.{simpleMethodName}()' explicit implementation mapped to direct struct receiver method,\r\n        ");
                        result.Append($"// this overrides promoted interface method '{GetSimpleName(InterfaceName)}.{simpleMethodName}':\r\n        ");
                    }
                    else
                    {
                        result.Append($"// '{simpleInterfaceName}.{simpleMethodName}()' explicit implementation mapped to direct struct receiver method:\r\n        ");
                    }

                    string receiver = "this";

                    if (!methodOverriden && EmbedHop is not null)
                    {
                        if (EmbedHopDeepPaths is not null && EmbedHopDeepPaths.TryGetValue(simpleMethodName, out string? deepPath))
                            receiver = $"this.{EmbedHop}{deepPath}";
                        else
                            receiver = EmbedHopBoxMethods.Contains(simpleMethodName) ? $"this.{EmbedHop}" : $"this.{EmbedHop}.Value";
                    }
                    else if (!methodOverriden && MultiEmbedHopPaths is not null && MultiEmbedHopPaths.TryGetValue(simpleMethodName, out string? multiHopPath))
                    {
                        receiver = $"this.{multiHopPath}";
                    }
                    else if (!methodOverriden && ValueEmbedHop is not null)
                    {
                        if (ValueEmbedHopStaticClass is not null)
                        {
                            string staticArgs = string.IsNullOrEmpty(method.CallParameters) ? $"this.{ValueEmbedHop}" : $"this.{ValueEmbedHop}, {method.CallParameters}";
                            result.Append($"[{NonUserCodeAttribute}] {method.ReturnType} {method.GetSignature()} => {ValueEmbedHopStaticClass}.{simpleMethodName}{method.GetGenericSignature()}({staticArgs});");
                            continue;
                        }

                        receiver = $"this.{ValueEmbedHop}";
                    }

                    result.Append($"[{NonUserCodeAttribute}] {method.ReturnType} {method.GetSignature()} => {receiver}.{forwardName}{method.GetGenericSignature()}({method.CallParameters});");
                }
            }

            return result.ToString();
        }
    }

    private string Comparisions
    {
        get
        {
            // Operators can only be public
            return OperatorScope != "public" ? 
                string.Empty : 
                $"""
                
                
                        // Handle comparisons between struct '{StructName}' and interface '{GetSimpleName(InterfaceName)}'
                        public static bool operator ==({StructName} src, {InterfaceName} iface) => iface is {StructName} val && val == src;
                        
                        public static bool operator !=({StructName} src, {InterfaceName} iface) => !(src == iface);
                        
                        public static bool operator ==({InterfaceName} iface, {StructName} src) => iface is {StructName} val && val == src;
                        
                        public static bool operator !=({InterfaceName} iface, {StructName} src) => !(iface == src);
                """;
        }
    }

    // The comparison operators take one of each side as parameters, so the pair's scope may not
    // out-rank either operand: public only when BOTH are, internal otherwise.
    private string OperatorScope => StructIsPublic && InterfaceIsPublic ? "public" : "internal";
}
