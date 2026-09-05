// InheritedTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal class InheritedTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string ObjectName;
    public string ObjectKind = "struct";
    public bool ReadOnlyValue = true;
    public required string TypeName;
    public required string TargetTypeName;
    public string? TargetValueTypeName = null;
    public string? TargetTypeSize = null;
    public required string TypeClass;

    // For the "Array" TypeClass: the construction expression for ONE element of the lazy backing, or
    // null when `default(element)` is already the element's Go zero value — which is nearly always.
    //
    // `new array<E>(N)` fills its backing with `default(E)`, and that is NOT the Go zero value for an
    // element that has to be built: a nested unnamed array (`type nn [2][3]int` — the inner length
    // lives only in the Go type, so every element keeps a length-ZERO backing) or a struct whose own
    // zero value needs construction (`type semTable [251]struct{ root semaRoot; pad [40]byte }` —
    // `default` skips the generated constructor that runs the `pad = new(40)` initializer). Measured
    // against `go run` before the fix: `var d nn` printed `2 0 [[] []]` where Go prints
    // `2 3 [[0 0 0] [0 0 0]]`, and the first indexed write PANICKED.
    //
    // This is the ONE place every such zero value converges. The wrapper's backing is allocated
    // lazily, so `default(nn)` — a `var`, a `new(T)`, a struct field, a named result, an element of
    // an `array<nn>`/`slice<nn>`, a map read — all reach the Go zero value THROUGH this property;
    // that is why the outer dimensions of `[2]nn` were already correct while only the innermost was
    // wrong, and why fixing it here fixes all of them at once rather than one declaration site at a
    // time. See TypeGenerator's Array arm for where the expression comes from.
    public string? ElementZeroFactory = null;

    // W3a wrapper-scaffolding (docs/phase4/DESIGN-w3a-wrapper-scaffolding.md). True (the existing,
    // unconditional-public behavior) unless the caller knows the wrapped type (TypeName) is itself
    // not effectively public — set only by the test-file-declared defined-type-over-struct case
    // today (TypeGenerator.cs). Governs MemberScope below, which the constructor/.Value/conversion
    // operators use instead of a bare `public`.
    public bool WrappedTypeIsPublic = true;

    // The accessibility for a member whose signature names TypeName directly (the constructor,
    // .Value, the underlying conversion operators) — public only when BOTH the wrapper itself
    // (Scope) and the wrapped type are, the same narrowest-wins rule ForwardedMembers already
    // applies per forwarded field. A public wrapper over an unexported production type (runtime's
    // white-box `MSpan` bridging `mspan`) still needs these members internal: IVT already makes
    // internal free for every consumer, all of which are sibling files in the same test assembly.
    // StartsWith, not ==: Scope can carry a trailing " readonly" (the "Numeric" TypeClass —
    // ArbitraryType, ArchFamilyType, ... — sets `Scope = $"{scope} readonly"`), and an exact-match
    // check against the bare word silently read every numeric wrapper as non-public regardless of
    // WrappedTypeIsPublic, which is exactly the corpus-wide CS0558 an earlier, unscoped attempt at
    // this fix measured (see TypeGenerator.cs's isTestFileDeclaration comment for the other half of
    // that same regression).
    private string MemberScope => Scope.StartsWith("public") && WrappedTypeIsPublic ? "public" : "internal";

    // For a defined type whose underlying is a STRUCT (`type winlibcall libcall`), the underlying
    // struct's fields are accessible on the named type in Go (`w.fn`). C# has no such access on the
    // wrapper, so the underlying struct's members are forwarded here as get/set properties over the
    // (mutable) `m_value`. Null/empty for every other inherited kind (slice/map/array/numeric/…).
    public List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)>? ForwardedStructMembers = null;

    // For a defined type whose underlying is ITSELF an array-backed [GoType] wrapper
    // (`type pallocBits pageBits`, `type pageBits [8]uint64`), the element type of that underlying
    // array — set to implement IArray<elem> on this wrapper as a view over `m_value` (golib `len()`
    // and indexing bind IArray). Null for every other inherited kind.
    public string? UnderlyingArrayElementType = null;

    // Set when the converter stamped [GoValueClone("Value")] on this wrapper — a defined type whose
    // underlying is a STRUCT carrying fixed-size ARRAY fields (`type IpMaskString IpAddressString`,
    // whose underlying holds a `[16]byte`). The wrapper's entire deep copy is its one Value member's,
    // so Clone() forwards to it. See GoValueCloneAttribute; the array-backed kinds already carry a
    // strongly-typed Clone() from their own templates and are excluded below.
    public bool ValueClone = false;

    private string ImplementedInterface => TypeClass switch
    {
        "Slice" => $" : ISlice<{TargetTypeName}>, ISupportMake<{ObjectName}>, ISliceWrap<{ObjectName}, {TargetTypeName}>",
        "Map" => $" : IMap<{TargetTypeName}, {TargetValueTypeName}>, ISupportMake<{ObjectName}>",
        "Channel" => $" : IChannel<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Array" => $" : IArray<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Pointer" => $" : IPointer<{TargetTypeName}>, INilPointer",
        // Generic-math declarations so a [GoType num:] wrapper satisfies converter-emitted
        // numeric constraints as a type ARGUMENT — slices.Sort's `cmp.Ordered` maps to
        // IAddition/IEquality/IComparisonOperators (time.Duration in runtime/debug's
        // SetGCPercent sort was CS0315 ×3); the operators below already exist for every
        // numeric kind, only the interface declarations were missing (the golib uintptr
        // struct precedent).
        "Numeric" => NumericInterfaces,
        _ => UnderlyingArrayElementType is null ? "" : $" : IArray<{UnderlyingArrayElementType}>"
    };

    // A numeric wrapper over an INTEGER underlying (not float/complex) — the modulus, bitwise and
    // shift operators exist for it (NumericTypeTemplate.GetComplementOperator, same kind-gate), so
    // it can ALSO satisfy a converter-emitted `~integer` operator constraint that lifts to
    // IModulus/IBitwise/IShiftOperators. internal/trace's `type dataTable[EI ~uint64, E]`
    // instantiated with `type stringID uint64` was CS0315 ×48 on exactly these three interfaces.
    // Float/complex keep only the common set below (they have no %/&/<<). IShiftOperators uses the
    // BCL shape <T, int, T> — the shift count is int (see the converter's lifted-shift note).
    private bool IsIntegerNumeric => TypeClass == "Numeric" && !TypeName.StartsWith("float") && !TypeName.StartsWith("complex");

    private string NumericInterfaces
    {
        get
        {
            // Complex kinds have no ordered comparisons (Go spec: == / != only; C# complex has no
            // <//<=/>/>= either) — the operators are gated in NumericTypeTemplate, so declaring
            // IComparisonOperators here would be CS0535.
            //
            // IComparable<T> rides the SAME gate and is the ordering interface's other half: the
            // operators satisfy a constraint that lifts to IComparisonOperators, but the BCL's own
            // ordering surface — Array/List.Sort, SortedSet, Comparer<T>.Default, and golib's
            // N-argument `min`/`max` (`where T : IComparable<T>`) — binds IComparable<T> instead,
            // which a named numeric could not satisfy. Go's `min(a-got, got-a, a-got+q, got-a+q)`
            // over `type fieldElement uint16` (crypto/internal/mlkem768's TestDecompressCompress)
            // was CS0315 for exactly that reason. The wrapper is IEquatable<T> already; this makes
            // it ordered as well, matching the golib `uintptr`/`@string` structs, which are both.
            string comparisonInterface = TypeName.StartsWith("complex") ? "" : $" global::System.IComparable<{TargetTypeName}>, global::System.Numerics.IComparisonOperators<{TargetTypeName}, {TargetTypeName}, bool>,";

            string interfaces = $" : global::System.IEquatable<{TargetTypeName}>, global::System.Numerics.IAdditionOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.ISubtractionOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IMultiplyOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IDivisionOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IEqualityOperators<{TargetTypeName}, {TargetTypeName}, bool>,{comparisonInterface} global::System.Numerics.IIncrementOperators<{TargetTypeName}>, global::System.Numerics.IDecrementOperators<{TargetTypeName}>, global::System.Numerics.IUnaryNegationOperators<{TargetTypeName}, {TargetTypeName}>";

            if (IsIntegerNumeric)
                interfaces += $", global::System.Numerics.IModulusOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IBitwiseOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IShiftOperators<{TargetTypeName}, int, {TargetTypeName}>";

            return interfaces;
        }
    }

    private string InterfaceImplementation => TypeClass switch
    {
        "Slice" => ISliceTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName),
        "Map" => IMapTypeTemplate.Generate(ObjectName, TargetTypeName, TargetValueTypeName),
        "Channel" => IChannelTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName),
        "Array" => IArrayTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName, TargetTypeSize),
        "Numeric" => NumericTypeTemplate.Generate(TypeName, TargetTypeName),
        "Pointer" => PointerTypeTemplate.Generate(ObjectName, TargetTypeName),
        _ => UnderlyingArrayElementType is null ? "" : IArrayViewTypeTemplate.Generate(ObjectName, TypeName, UnderlyingArrayElementType)
    };

    // Only the plain (struct-underlying) wrapper needs this: every array/slice-backed kind already
    // declares its own Clone()/ICloneable through IArrayTypeTemplate, IArrayViewTypeTemplate or
    // ISliceTypeTemplate, and re-declaring would be CS0102/CS0111.
    private bool EmitsValueClone => ValueClone && UnderlyingArrayElementType is null &&
        TypeClass is not ("Array" or "Slice" or "Map" or "Channel" or "Pointer" or "Numeric");

    private string ValueCloneInterface => EmitsValueClone ?
        string.IsNullOrEmpty(ImplementedInterface) ? " : IGoValueClone" : ", IGoValueClone" : "";

    private string ValueCloneImplementation => EmitsValueClone ?
        $$"""

                // Go by-value copy of {{ObjectKind}} '{{ObjectName}}'
                public {{EscapeCsTypeName(ObjectName)}} {{ValueCloneMethod}}() => new {{ObjectName}}({{Value}}.{{ValueCloneMethod}}());

                object global::System.ICloneable.Clone() => {{ValueCloneMethod}}();

        """ : "";

    private string ValueGetter => TypeClass switch
    {
        // The Array kind supplies its own BLOCK-bodied Value (see ValueProperty): its lazy backing is
        // published with an interlocked CAS, which an expression-bodied `??=` cannot express.
        // The array-view wrapper's `Value` must route through `view` (ensure the underlying's lazy
        // backing materializes on THIS wrapper's own m_value, then return a copy sharing that T[]):
        // the converter emits `b.Value[i] = v` inside the wrapper's pointer-receiver methods, and a
        // plain by-value `m_value` would lazily allocate on the returned temp — silently dropping
        // every write on virgin storage (the pallocBits fill-loop shape).
        _ => UnderlyingArrayElementType is null ? "m_value" : "view"
    };

    private string Value => TypeClass switch
    {
        "Array" => "Value", // Null-coalescing property auto-creates array on first reference
        _ => "m_value"
    };

    // The Pointer class supplies its own ref-returning Value (PointerTypeTemplate); emitting the
    // base value property too is a duplicate member (CS0102).
    private string EqualityExpression => TypeClass switch
    {
        // A nil named pointer is a NULL reference (class) — left.Equals would NRE; Equals(a, b)
        // is null-safe (null == null is true, matching Go's nil == nil).
        "Pointer" => "Equals(left, right)",
        _ => "left.Equals(right)"
    };

    // A named slice/map/channel's `s == nil` — the ONLY comparison Go permits on these kinds — renders
    // as `s == default!` (nil literal in value context) or `s == nil` (pointer context), and BOTH bind
    // THIS `operator ==(S, NilType)` overload, not the same-type operator above (verified against the
    // emitted C# — reverting EqualityExpression has no effect; reverting this flips the result). The
    // SLICE wrapper's default `value.Equals(default(S))` is structural CONTENT equality (its
    // ISliceTypeTemplate Equals(ISlice<T>?) compares the backing arrays element-wise), so a non-nil
    // EMPTY named slice (`S{}`, `make(S, 0)`, an `s[len(s):]` tail) was misclassified as nil. Delegate
    // to slice<T>'s own == NilType — REPRESENTATION nilness (null backing array, R13) — so `IntSlice{}
    // == nil` is false while the zero value stays nil. Map/channel keep the structural default and are
    // already correct: they declare no structural Equals, so `Equals(default)` falls back to reference
    // identity on the backing field (map<K,V>.Equals is ReferenceEquals(m_map); channel<T> compares its
    // queue). Pointer uses its reference-identity Equals override, and array/numeric/string/struct/any
    // are not nil-comparable in Go — all keep the default.
    private string NilComparisonExpression => TypeClass switch
    {
        "Slice" => "value.m_value == nil",
        // STRUCTURAL nil for pointers (null-safe): a null reference and the canonical typed nil
        // instance are the same Go nil pointer; the old `value.Equals(default)` NRE'd on a null
        // reference and reported the canonical instance non-nil.
        "Pointer" => "value is null || value.IsNilPointer",
        _ => $"value.Equals(default({ObjectName}))"
    };

    // A nil→named-POINTER conversion yields the type's CANONICAL typed nil instance (mirrors
    // ж<T>.NilBox): the boxed value keeps its Go type (`%T`, reflect.TypeOf((*T)(nil)), typed-nil
    // interface semantics) and reference-compares equal across conversion sites. Every other kind
    // keeps its plain zero value.
    private string NilConversionExpression => TypeClass switch
    {
        "Pointer" => "NilInstance",
        _ => $"default({ObjectName})!"
    };

    // A named fixed-size Go array's backing is allocated LAZILY, and the lazy slot is shared mutable
    // state: `m_value ??= new array<E>(N)` is a read-modify-write, so two threads that first-touch the
    // SAME zero-valued wrapper each allocate a backing and the second store ORPHANS the first — along
    // with every element pointer already derived from it. The loss is silent (no fault, no exception),
    // bounded to a microsecond-wide start-up window, and measured at ~97% of concurrent first-touch
    // trials (`src/tests/ElemAliasProbe`, arm7 — 872 of 900 at the `??=` emission, 0 of 900 here).
    //
    // The publish is therefore an interlocked CAS: every racing thread allocates, exactly one wins the
    // slot, and the losers DISCARD their allocation before anything can derive an element address from
    // it — which is what makes the fix correct rather than merely narrower. The cold path pays one CAS,
    // once per wrapper for the life of the value; the warm path pays a reference load and a null test.
    //
    // The slot is a REFERENCE rather than `array<E>?` because an interlocked publish needs ONE machine
    // word. `array<E>` is a 3-field readonly struct (backing + window low/length), so `array<E>?` is 24
    // bytes: it can neither be CAS'd nor even READ without tearing while another thread writes it. The
    // holder carries the WHOLE value, which the narrower alternative — holding the bare `E[]` — would
    // not: a constructor-supplied array may be an ALIAS WINDOW (`array<E>.Alias`, Go's `(*[N]E)(s)`)
    // whose `Source` is WIDER than the array, and flattening it to its backing would silently widen the
    // named array to the whole allocation and shift its origin. It is allocated only where a value was
    // already being allocated (the lazy backing, `Clone()`, an explicit conversion); `default(T)`
    // allocates nothing, and the wrapper struct itself SHRINKS from 24 bytes to 8.
    //
    // `StrongBox<array<E>>` and not plain `object`: both are one word and both preserve the value, but
    // an `object` slot makes every warm read an `unbox.any` — a type-check helper CALL in the hot loop
    // — while the typed holder reads a field at a fixed offset. Measured on the element-address path
    // (`ref Value[i]`, the form converted element reads and writes emit), 64 cold tables: 1.97 ns/op at
    // the `??=` emission, 4.13 with an `object` slot, 2.45 with the holder.
    //
    // The cost that remains is one dependent load, and the probe reports it honestly (arm9, both
    // emissions measured in one process): the raw `Value` getter gets FASTER (1.03 -> 0.87 ns/op — the
    // wrapper struct shrank from 24 bytes to 8, so every Go by-value array copy moved too), the element
    // path over ONE long-lived table — what the corpus's named arrays actually are, and where the JIT
    // hoists the loop-invariant getter — moves between -1% and +12% run to run (1.70..1.77 ->
    // 1.79..1.93 ns/op), and the pathological shape of 64 separate non-resident tables costs +17..25%.
    // That is the price of closing a silent lost write.
    //
    // This closes DOOR 2 of the element-aliasing family (`docs/phase4/INVESTIGATION-element-aliasing.md`):
    // the wrapper's own getter, reached by ref with no golib on the path, which door 1's per-box publish
    // gate in `ж<T>.at()` structurally cannot see. It does NOT close a materialization that happens on a
    // by-value COPY of the wrapper (the copy's field is the one published) — that is the mpallocbits
    // operator-copy door, tracked separately.
    // The lazy backing's constructor arguments: the length alone when `default(element)` is already
    // the element's Go zero value, and the length plus a construction lambda when it is not. golib's
    // `array<T>(nint length, Func<T> elementFactory)` overload is the same one the CONVERTER's
    // arrayLengthArgs renders for every zero-value site it can spell; this is that argument list
    // reached from the generator side, so a named array's zero value is built the same way whichever
    // half of the toolchain gets there first.
    private string LazyBackingArguments => ElementZeroFactory is { Length: > 0 } factory ?
        $"{TargetTypeSize ?? "0"}, static () => {factory}" :
        TargetTypeSize ?? "0";

    private string ValueProperty => TypeClass switch
    {
        "Pointer" => "",
        "Array" =>
            $$"""
                      public {{TypeName}} Value
                      {
                          get
                          {
                              {{ValueFieldType}}? value = m_value;

                              if (value is null)
                              {
                                  {{ValueFieldType}} created = new {{ValueFieldType}}(new {{TypeName}}({{LazyBackingArguments}}));
                                  value = global::System.Threading.Interlocked.CompareExchange(ref m_value, created, null) ?? created;
                              }

                              return value.Value;
                          }
                      }
              """,
        _ => $"        {MemberScope} {TypeName} Value => {ValueGetter};"
    };

    // A wrapper over golib's `uintptr` STRUCT needs its own bridges to the plain numeric world:
    // `gclinkptr x = 0` / `(gclinkptr)someNuint` would otherwise chain TWO user-defined
    // conversions (nuint→uintptr, uintptr→gclinkptr), which C# never composes. The nuint bridge
    // (plus UntypedInt for named untyped consts) restores the reachability the old
    // System.UIntPtr alias provided for free.
    // A named type over `any` (`type Symbol any` — plugin; also crypto.PublicKey/PrivateKey,
    // driver.Value, json.Token) cannot declare user-defined conversions to or from its
    // underlying: `any` is System.Object, the base type of everything (CS0553 ×2). No bridge is
    // lost — boxing already converts any value to object, `(Symbol)obj` is the unbox-cast, and
    // the `new Symbol(value)` constructor plus the NilType operators are unaffected.
    // C# requires a user-defined conversion operator to be public — there is no legal non-public
    // form (CS0558), so the MemberScope narrowing every OTHER wrapped-type-facing member takes
    // (the constructor, .Value) is not an option here. When the wrapper is public but the wrapped
    // type is not, NEITHER modifier is legal — public would expose the type (CS0053), internal
    // would be CS0558 — so the pair is omitted outright rather than weakened. This costs nothing Go
    // promised: TypeName here is always a Go DEFINED type's underlying (never a true alias — see
    // the design doc), and Go itself requires an EXPLICIT conversion between a defined type and its
    // underlying, never an implicit one. The constructor and `.Value` (now correctly internal in
    // this same case) remain the explicit path for every consumer, all of which are sibling files
    // in the one test assembly under the whitebox-reference model.
    private bool OmitUnderlyingConversionOperators => Scope.StartsWith("public") && !WrappedTypeIsPublic;

    private string UnderlyingConversionOperators => TypeName is "any" or "object" || OmitUnderlyingConversionOperators ? "" :
        $$"""
                // Handle implicit conversions between '{{TypeName}}' and {{ObjectKind}} '{{ObjectName}}'
                public static implicit operator {{ObjectName}}({{TypeName}} value) => new {{ObjectName}}(value);

                public static implicit operator {{TypeName}}({{ObjectName}} value) => value.{{Value}};
        """;

    private string UintptrBridgeOperators => TypeName != "uintptr" ? "" :
        $"""

                public static implicit operator {ObjectName}(nuint value) => new {ObjectName}((uintptr)value);

                public static implicit operator nuint({ObjectName} value) => ((uintptr)value.{Value}).Value;

                public static implicit operator {ObjectName}(UntypedInt value) => new {ObjectName}((uintptr)(nuint)value);

        """;

    // A named type over a plain INTEGER needs the UntypedInt bridge for named untyped
    // consts: `(token)(endBlockMarker)` (compress/flate, CS0030 ×2) would otherwise chain
    // two user-defined conversions (UntypedInt→uint32, uint32→token), which C# never
    // composes.
    private string UntypedIntBridgeOperator => TypeName is "byte" or "uint8" or "uint16" or "uint32" or "uint64" or "int8" or "int16" or "int32" or "int64" or "nint" or "nuint" or "rune" ?
        $"""

                public static implicit operator {ObjectName}(UntypedInt value) => new {ObjectName}(({TypeName})value);

        """ : "";

    // The float twin of the bridge above — needed because a Go literal with an EXPONENT (`const
    // gcCPULimiterUpdatePeriod = 10e6`) is a FLOAT literal syntactically even when its value is a
    // whole number, so go/types classifies the untyped constant as UntypedFloat, not UntypedInt.
    // A BARE reference to it (or arithmetic over it, `x / 2`) renders as the golib UntypedFloat
    // WRAPPER struct exactly like a named untyped INT constant renders as UntypedInt
    // (exprRendersUntypedConstWrapper) — go/types only accepts this at an integer-typed call site
    // (runtime's `advance(gcCPULimiterUpdatePeriod)`, a time.Duration parameter) when the value IS
    // exactly representable as one, so by the time this operator is ever reached the cast through
    // UntypedFloat's own existing conversion to the primitive underlying type can never truncate a
    // fraction Go itself would have rejected.
    private string UntypedFloatBridgeOperator => TypeName is "byte" or "uint8" or "uint16" or "uint32" or "uint64" or "int8" or "int16" or "int32" or "int64" or "nint" or "nuint" or "rune" ?
        $"""

                public static implicit operator {ObjectName}(UntypedFloat value) => new {ObjectName}(({TypeName})value);

        """ : "";

    // A named type over `string` is indexed and sub-sliced in Go (`tag[i]`, `tag[i:j]` -
    // reflect StructTag.Get); C# indexing never applies user-defined conversions, so the
    // wrapper forwards the @string surface: element indexers, a Range indexer returning the
    // WRAPPER (a Go sub-slice of a named string keeps the named type), Length for len(), and
    // the u8-literal bridge so span comparisons/assignments bind (census F5, CS0021 x14 +
    // CS0019 x2).
    private string StringSurfaceMembers => TypeName != "@string" ? "" :
        $$"""

                public byte this[int index] => {{Value}}[index];

                public byte this[nint index] => {{Value}}[index];

                public {{ObjectName}} this[global::System.Range range] => new {{ObjectName}}({{Value}}[range]);

                public nint Length => {{Value}}.Length;

                public static implicit operator {{ObjectName}}(global::System.ReadOnlySpan<byte> value) => new {{ObjectName}}(new @string(value));

                // Comparisons directly against a `ReadOnlySpan<byte>` — a `u8` string literal, which is
                // zero-allocation static ROM. Without them `v != ""u8` (go/types' goVersion checks) bound
                // the same-type operators through the implicit span conversion above, materializing a
                // fresh @string on EVERY comparison; @string itself carries the identical set, so the
                // named wrapper now has the same literal-comparison cost model as its underlying. Exact
                // match beats the user-defined conversion, so these bind ahead of `==({{ObjectName}}, {{ObjectName}})`;
                // both operand orders are declared because the literal can be written on either side.
                public static bool operator ==({{ObjectName}} left, global::System.ReadOnlySpan<byte> right) => left.{{Value}} == right;

                public static bool operator !=({{ObjectName}} left, global::System.ReadOnlySpan<byte> right) => left.{{Value}} != right;

                public static bool operator ==(global::System.ReadOnlySpan<byte> left, {{ObjectName}} right) => left == right.{{Value}};

                public static bool operator !=(global::System.ReadOnlySpan<byte> left, {{ObjectName}} right) => left != right.{{Value}};

                public static bool operator <({{ObjectName}} left, global::System.ReadOnlySpan<byte> right) => left.{{Value}} < right;

                public static bool operator <=({{ObjectName}} left, global::System.ReadOnlySpan<byte> right) => left.{{Value}} <= right;

                public static bool operator >({{ObjectName}} left, global::System.ReadOnlySpan<byte> right) => left.{{Value}} > right;

                public static bool operator >=({{ObjectName}} left, global::System.ReadOnlySpan<byte> right) => left.{{Value}} >= right;

                public static bool operator <(global::System.ReadOnlySpan<byte> left, {{ObjectName}} right) => left < right.{{Value}};

                public static bool operator <=(global::System.ReadOnlySpan<byte> left, {{ObjectName}} right) => left <= right.{{Value}};

                public static bool operator >(global::System.ReadOnlySpan<byte> left, {{ObjectName}} right) => left > right.{{Value}};

                public static bool operator >=(global::System.ReadOnlySpan<byte> left, {{ObjectName}} right) => left >= right.{{Value}};

                // CONCATENATION, the twin of the comparison set above. Without these the wrapper had no
                // `+` candidate at all, so C# fell back to converting both operands to a C# `string` and
                // using string.Concat — which yields a `string`, not the named type, so every use site
                // that needed the wrapper back failed (`[]S{q + "-e"}`, CS0029), and a `u8` operand had
                // no candidate to reach at all (`S b = q + "-b"u8`, CS0019). Go keeps the named type
                // across a concat (`type S string; s + "x"` is an S), so the same-type overload returns
                // {{ObjectName}}. The span overload forwards to @string's own, whose bytes block-copy
                // straight into the result buffer — no intermediate @string, no UTF-16 transcode; exact
                // match beats the user-defined span conversion, so it binds ahead of the same-type form.
                public static {{ObjectName}} operator +({{ObjectName}} left, {{ObjectName}} right) => new {{ObjectName}}(left.{{Value}} + right.{{Value}});

                public static {{ObjectName}} operator +({{ObjectName}} left, global::System.ReadOnlySpan<byte> right) => new {{ObjectName}}(left.{{Value}} + right);

                public static {{ObjectName}} operator +(global::System.ReadOnlySpan<byte> left, {{ObjectName}} right) => new {{ObjectName}}(left + right.{{Value}});

        """;

    private string ToStringImplementation => TypeClass switch
    {
        "bool" => $"{Value}.ToString().ToLowerInvariant()",
        _ => $"{Value}.ToString()"
    };

    // A Go fixed-size array is COMPARABLE and legal as a map key, so equal arrays must be equal and
    // hash alike. With no overrides a C# struct inherits ValueType.Equals/GetHashCode, and BOTH read
    // the single m_value FIELD — which for the Array kind is now a publish holder (see ValueProperty),
    // i.e. a REFERENCE. Two distinct wrappers over equal content then compared unequal and hashed
    // differently, so they missed each other in a map and in reflect.DeepEqual: precisely the silent
    // wrong answer door 2 exists to remove, traded for a different one.
    //
    // Both overrides are needed and the `==` operator is neither of them: `EqualityExpression` binds
    // the wrapper's own `Equals(IArray<E>)` at COMPILE time, which was structural all along and stayed
    // so — which is exactly why the regression is invisible from the source and had to be measured
    // (ElemAliasProbe arm10 checks the compile-time and the runtime overload separately: structural
    // before, `object.Equals` false after, structural again with these).
    //
    // The `is {ObjectName}` test keeps ValueType.Equals's type-strictness (a different Go named array
    // over the same underlying is a different Go type and never equal), while the comparison itself
    // delegates to array<E>'s element-wise Equals/GetHashCode — so neither depends on the slot's shape
    // any more, and a future change of carrier cannot move them again.
    private string EqualityOverrides => TypeClass == "Array" ?
        $"""

                public override bool Equals(object? obj) => obj is {ObjectName} other && {Value}.Equals(other.{Value});

                public override int GetHashCode() => {Value}.GetHashCode();

        """ : "";

    private string ReadOnly => ReadOnlyValue ? "readonly " : "";

    // Only the lazily-allocated Array backing needs a nullable slot (null == "not yet materialized").
    // Other mutable cases (a struct-forwarding named type) keep a non-nullable value slot — decoupled
    // from ReadOnlyValue so struct forwarding can be mutable yet non-nullable.
    private string Nullable => TypeClass == "Array" ? "?" : "";

    // The Array kind's slot holds its `array<E>` inside a StrongBox, so the lazy backing can be
    // published with an interlocked CAS — see ValueProperty for why one machine word is the
    // requirement, why the holder (not the bare `E[]`) is what preserves the value, and why the holder
    // is TYPED rather than a plain `object`.
    private string ValueFieldType => TypeClass == "Array" ? $"global::System.Runtime.CompilerServices.StrongBox<{TypeName}>" : TypeName;

    // The Array kind's constructor wraps its incoming value in the publish holder; every other kind
    // stores it directly.
    private string ValueConstructorArgument => TypeClass == "Array" ? $"new {ValueFieldType}(value)" : "value";

    // Forwarding properties for a defined-type-over-struct, exposing the underlying struct's fields on
    // the wrapper. `m_value` is mutable (ReadOnlyValue=false) so a write through a ж<T>.Value ref —
    // `box.Value.fn = x`, where `box.Value` is `ref winlibcall` — reaches the real storage and persists.
    // A blank `_` field is unaddressable/unselectable in Go and would collide.
    //
    // REF-RETURNING, not get/set: in Go the selection IS the underlying field, so it is a variable —
    // addressable, and usable as the receiver of a method whose receiver the converter emits `this ref`
    // (every value-receiver method). A get/set property yields a VALUE, so `x.sa.len()` and
    // `x.sa.get(i)` — index/suffixarray's `type index Index`, whose `sa` is an `ints` with `len`/`get`
    // methods — were CS0206 ("a non ref-returning property may not be used as a ref value"). The ref
    // property keeps every use the setter form supported (`w.fn = v` assigns through the ref) and adds
    // the variable-requiring ones, so it is a strict superset.
    //
    // [UnscopedRef] is what makes it legal at all: a struct member returning a ref to instance state is
    // CS8170 by default, because the receiver could be a temporary. The attribute states the ref's
    // lifetime is the RECEIVER's, which is exactly the guarantee Go gives — the selection aliases the
    // wrapper's own storage — and it moves the burden to the call site, where C#'s ref-safety rules
    // then reject precisely the cases Go also rejects (taking the address of a non-variable).
    private string ForwardedMembers
    {
        get
        {
            if (ForwardedStructMembers is null || ForwardedStructMembers.Count == 0)
                return "";

            // A forwarded accessor is public only when BOTH the wrapper itself is public AND the
            // forwarded member's own type is — the narrowest-wins rule ReceiverMethodTemplate.
            // TargetScope already applies to a receiver mismatch. Without it, a deliberately PUBLIC
            // bridge wrapping an unexported production type (runtime's white-box `MSpan`, `type MSpan
            // = mspan` in export_test.go, exposed so external tests can name it) forwarded `mspan`'s
            // still-internal field types — `gcBits`, `mutex`, `special`, `addrRange` — through
            // unconditionally `public` accessors: the wrapper's OWN publicization says nothing about
            // its FIELDS' accessibility, which stays whatever production emitted per field (W3a).
            string MemberScope((string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic) member) =>
                Scope == "public" && member.isPublic ? "public" : "internal";

            IEnumerable<string> props = ForwardedStructMembers
                .Where(member => GetSimpleName(member.memberName) != "_")
                .Select(member => $"\r\n        [global::System.Diagnostics.CodeAnalysis.UnscopedRef] {MemberScope(member)} ref {member.typeName} {member.memberName} => ref m_value.{member.memberName};");

            // The field-box accessors (`Ꮡfield`) that a plain struct's partial generates (used by the
            // converter's `receiver.of(Type.Ꮡfield)` field-address form — `&p.x` on a *pinnerBits, where
            // `type pinnerBits gcBits`) must exist on the WRAPPER type too, since the accessor names the
            // wrapper (`pinnerBits.Ꮡx`, CS0117 otherwise). Forward them as true refs THROUGH `m_value`
            // into the underlying struct's field — `ref instance.m_value.x` is a genuine ref chain into
            // the wrapper's own storage (no copy, so writes through the resulting box persist; `m_value`
            // is mutable whenever members are forwarded). Property members cannot be ref'd and get no
            // accessor (matching the plain-struct template, which only emits them for fields).
            IEnumerable<string> fieldRefs = ForwardedStructMembers
                .Where(member => GetSimpleName(member.memberName) != "_" && !member.isProperty)
                .Select(member => $"\r\n        {MemberScope(member)} static ref {member.typeName} {AddressPrefix}{GetUnsanitizedIdentifier(member.memberName)}(ref {ObjectName} instance) => ref instance.m_value.{member.memberName};");

            return $"\r\n\r\n        // Forwarded fields of the underlying '{TypeName}'{string.Concat(props)}{string.Concat(fieldRefs)}";
        }
    }

    // The nil-constructed value of a defined-type-over-STRUCT wrapper must construct the wrapped
    // struct through its own NilType constructor: the wrapped struct may carry promoted-embed
    // boxes (readonly `ж<T>` fields only its constructors allocate), so a `default!` m_value
    // would NRE on the first forwarded promoted-member access. Every other inherited kind
    // (slice/map/array/numeric/…) keeps the plain default — its zero value is already correct.
    private string NilValueExpression => ForwardedStructMembers is null || ForwardedStructMembers.Count == 0 ?
        "default!" : $"new {TypeName}(nil)";

    // A C# constructor name must not carry the type's generic parameters (e.g. the constructor for
    // a generic named array type `vec<T>` is `vec(...)`, not `vec<T>(...)`). Non-generic types have
    // no '<' so ConstructorName equals ObjectName — emitting byte-identical output.
    private string ConstructorName
    {
        get
        {
            int angle = ObjectName.IndexOf('<');
            return angle < 0 ? ObjectName : ObjectName.Substring(0, angle);
        }
    }

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial {{ObjectKind}} {{ObjectName}}{{ImplementedInterface}}{{ValueCloneInterface}}
            {
                // Value of the {{ObjectKind}} '{{ObjectName}}'
                private {{ReadOnly}}{{ValueFieldType}}{{Nullable}} m_value;
                {{InterfaceImplementation}}{{ForwardedMembers}}{{ValueCloneImplementation}}

                {{MemberScope}} {{ConstructorName}}({{TypeName}} value) => m_value = {{ValueConstructorArgument}};

                public {{ConstructorName}}(NilType _) => m_value = {{NilValueExpression}};

        {{ValueProperty}}
                public override string ToString() => {{ToStringImplementation}};
        {{EqualityOverrides}}
                public static bool operator ==({{ObjectName}} left, {{ObjectName}} right) => {{EqualityExpression}};
        
                public static bool operator !=({{ObjectName}} left, {{ObjectName}} right) => !(left == right);
        
        {{UnderlyingConversionOperators}}
                    {{UintptrBridgeOperators}}{{UntypedIntBridgeOperator}}{{UntypedFloatBridgeOperator}}{{StringSurfaceMembers}}
                // Handle comparisons between 'nil' and {{ObjectKind}} '{{ObjectName}}'
                public static bool operator ==({{ObjectName}} value, NilType nil) => {{NilComparisonExpression}};
        
                public static bool operator !=({{ObjectName}} value, NilType nil) => !(value == nil);
        
                public static bool operator ==(NilType nil, {{ObjectName}} value) => value == nil;
        
                public static bool operator !=(NilType nil, {{ObjectName}} value) => value != nil;
        
                public static implicit operator {{ObjectName}}(NilType nil) => {{NilConversionExpression}};
            }
        """;
}
