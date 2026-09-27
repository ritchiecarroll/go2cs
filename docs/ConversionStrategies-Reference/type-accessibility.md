# Type Accessibility and Publicization

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#functions-and-methods)

This page covers the C# access modifier each converted declaration receives, and when an unexported Go type is emitted `public` because an exported signature exposes it.

## Publicization

### Publicization decides WHAT a type's modifier is; the test-bridge arm only decides WHERE

`visitTypeSpec` writes a `[GoType]` declaration's access modifier from one of two sources, and they
answer different questions. `packagePublicizedTypes` answers *what* the modifier must be — an
unexported type reached by an exported field, var, or callable signature has to be `public` or C#
rejects the referrer (CS0050/CS0051/CS0052). `testInlineTypeAccess` answers *where* it is written: a
white-box bridge type carries its modifier inline rather than through `package_info.cs`'s
`<TypeAccessibility>` section, because its metadata anchor can be a different test class.

Asking the inline arm FIRST made it answer both — from the name alone — so a publicized bridge type
stayed `internal`. `context`'s internal test file declares `type testingT interface{…}` and the
exported `func XTestParentFinishesChild(t testingT)` that `x_test.go` calls; the publicization pre-pass
records `testingT` (it runs over the test-augmented package, so the exported `*types.Func` arm fires),
but the emission ignored it: `internal partial interface testingT` under a `public` method, CS0051 ×4.
Publicization now outranks, and the inline arm supplies the DEFAULT:

```csharp
[GoType] public partial interface testingT {   // was: internal
```

This is why `signatureReferencesUnexportedProductionType` (which downgrades an exported *test-file*
function whose signature names an unexported PRODUCTION type) is correctly restricted to production
types: a test-declared type is meant to be handled by publicization, and now is.

---

[Index](README.md)
