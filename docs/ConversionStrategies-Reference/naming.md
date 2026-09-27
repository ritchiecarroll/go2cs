# Names and Glyphs

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#reading-converted-code-names-and-glyphs)

This page covers how the converter spells names in C#: the keyword escaping of Go identifiers, and the synthetic names the converter introduces beside the user's own.

## Keyword names

### A keyword-named addressed global's heap-box field strips the escape after the Ꮡ prefix
An address-taken package-level var is backed by a heap-box FIELD plus a ref-returning property
(`writeAddressedGlobalDecl`). A keyword-named such global (`var null = json.RawMessage([]byte("null"))`,
net/rpc/jsonrpc) arrives keyword-escaped (`@null`), and composing the box as `Ꮡ` + `@null` places the
escape INTERIOR to the identifier token — `Ꮡ@null` lexes as two tokens (a whole-file syntax cascade).
The `Ꮡ` prefix already de-keywords the composed name (the keyword + affix rule the adapter compositions
above rely on), so the field declaration strips the escape — matching every `&null` use site, which
already composed `Ꮡnull` through `boxBaseName`:
```csharp
internal static ж<slice<byte>> Ꮡnull = new(slice<byte>((@string)"null"));
internal static ref slice<byte> @null => ref Ꮡnull.ValueSlot;   // the var itself keeps its escape

var p = Ꮡnull;                                                  // use site, unchanged
```
Guarded by `HeapKeywordVar` (a package-level `var null` written through its pointer and read back both
ways), alongside its existing keyword-named LOCAL coverage.

## Synthetic names

### Known exposure: marker-shaped USER identifiers can collide with synthetic names

The converter's synthetic-name markers — `ᴛ` (TempVarMarker, U+1D1B: `selᴛ1`, `tupleᴛ2`,
`elemᴛ0`, `iᴛ1`, `initᴛ<name>`, lifted-type `<name>ᴛ1`), `ʗ` (CapturedVarMarker), `Δ`
(ShadowVarMarker), and the rest of the Symbols.cs family — are exotic Unicode LETTERS, legal in
Go identifiers. A Go program that itself declares an identifier matching a generated shape (e.g.
`selᴛ1` used in a `select`, emitting `var selᴛ1 = selᴛ1;`) collides with the synthetic name —
loudly, at C# compile time (CS0128/CS0102), never silently. A general Δ-rename of user
identifiers matching the numbered-temp shape was attempted at the sanitizer choke point
(`getCoreSanitizedIdentifier`) and REJECTED: that choke point also renders the converter's own
synthetic names (loop temps `iᴛ1`, lifted anonymous/named-value types `main_MyBoolᴛ1`,
cross-file anon-struct names), so the blanket rule Δ-renamed synthetic names too and churned
non-select goldens; distinguishing user from synthetic identifiers requires threading origin
through many naming call sites — deliberate sprawl for a trigger that demands typing U+1D1B in
Go source. Accepted as a documented family-wide exposure: the failure mode is a compile error
naming the colliding identifier, and the workaround is renaming the pathological identifier in
the Go source.

---

[Index](README.md)
