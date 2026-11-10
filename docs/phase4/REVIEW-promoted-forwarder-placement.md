# REVIEW — DESIGN-promoted-forwarder-placement (P1, claude/p1-forwarder-placement 123e7002b1), adversarial

> **Status: RECORD.** COORD requested this adversarial review before the seat is cut (Fable 5.2, 2026-11-10). Read at
> master 66a2e023ca, plus dotnet/runtime v10.0.0's method-table builder. **Verdict: approve with changes.**
> The problem: tsc's converted ast package fails to load (TypeLoadException), because `ast_package` carries 116,102
> methods, 93% of them go2cs-gen `[GoPromoted]` forwarders, against CoreCLR's per-type limit.

## Verified

- **The limit is 65,524** (§1): `methodtablebuilder.h` `MAX_SLOT_INDEX = (UINT16)-1 - 10`, and `EnumerateClassMethods`
  throws `IDS_CLASSLOAD_TOO_MANY_METHODS` at `MAX_SLOT_INDEX <= cMethAndGaps` over every MethodDef, statics
  included, before any filter. Generic instantiations add no rows; nested compiler types carry their own limit.
- **Lookup** (§2): the converter reaches cross-package methods through namespace usings, never `using static`
  (`diagnosticOutput.go:38-74`, `convSelectorExpr.go:1276-1288`), so a holder in the package namespace sits at the same
  lookup level as the package class. Precedent: the xpkg holder class (`StructTypeTemplate.cs:1912`, via
  `DeclareExtensionHost`, `TemplateBase.cs:108`).
- **The extension-host scan** (`TypeExtensions.ExtensionMethodRegistry.cs:467-488`) requires sealed, non-nested,
  non-generic hosts. That rules out nested static classes (CS1109 as well), and a hash split of one partial class is
  still one type. **A per-STRUCT holder is necessary**: a per-package holder in ast would hold 107,835.
- **Instance members rejected correctly** (§3): C# binds an instance member before an extension, so silently
  inverting Go's shadowing where today the result is CS0111/CS0121. The ж form needs a static class regardless, and
  "forwarders only when referenced" breaks the runtime method set.
- **The runtime.Caller parity claim** (§6) is plausible: `managed_impl.cs:4416-4461` counts a frame only when the
  type name ends `_package` and the method lacks go2cs-gen `[GeneratedCode]`. The `[GoPromoted]` reader
  (`GoReflect.MethodSets.cs:462-471`) reads the method attribute, which moves with it.

## Must fix

1. **The binary break is not addressed.** Every public same-package forwarder leaves `<pkg>_package`. A compiled
   consumer (any .3-built nugetgo package or app) binds `go.X_package::M(ref T)` in IL and gets MissingMethodException
   at run time. The binary-compat guard will red with one CP0002 per public forwarder (about 37 std packages). The
   note must state:
   - the break class;
   - who mints the suppressions (generated from the census, never by hand);
   - that push-nuget refuses until the break floor moves;
   - ApiCompat in §9's gates.
   **Owner decision pending** (see the ruling).
2. **GO2CS0011 is already earmarked** (a BOARD row reserves it for the inaccessible-record diagnostic). Take the next
   free ID at cut time, with its DiagnosticCodes.md row (TestGo2csDiagnosticCodeRegistry gates it). The analyzer
   premise is right: analyzers run on the post-generator compilation. Exclude an unimplemented partial method, which
   is a symbol with no MethodDef.

## Should fix

3. §9's invariants are counts, so a forwarder re-minted with the wrong receiver form conserves them. Diff
   (name, signature, attributes) as a set, modulo the declaring type.
4. `ConsumingGoFromCSharp.md:172-173` says `using static` on a package class brings its methods; after the move it
   no longer brings promoted ones. Amend the doc, and add one consumer-shaped test (a foreign namespace, `using static`
   only, a promoted call).
5. Census the hand-owned static forms now (`_package\.\w+\((ref )?` against the promoted-name list), not at build.
6. Find a holder by an attribute naming its struct, not by name derivation (the stem premise GO2CS0001 only warns on).

## COORD ruling (2026-11-10)

Approved with changes: the per-struct holder is the design. Findings 2-6 are binding on the seat. Finding 1 is the
owner's call between (a) a deliberate .4 break, with GoBreakFloor moving to 1.26.10.4, generated suppressions and a
nugetgo rebuild wave after .4, and (b) a compatible hybrid that keeps every existing forwarder in place and spills only
a type that would exceed the limit. Before that decision P1 measures the real reach: the IL references from the
published nugetgo packages to std promoted forwarders.
