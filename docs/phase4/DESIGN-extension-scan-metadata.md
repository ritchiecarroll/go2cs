# DESIGN — the extension-method scan reads metadata tables, not every type

> **State: RULED (COORD, 2026-11-06), short by ruling.** The measurements behind it are in
> [`DATA-bogo-startup.md`](DATA-bogo-startup.md) §8, and in §9, which this seat appends.

## The problem

The first time a program asks a dynamic method-set question, golib's `GetExtensionMethods`
(`src/core/golib/runtime/TypeExtensions.ExtensionMethodRegistry.cs`) builds its index by calling `Assembly.GetTypes()`
on every loaded go2cs assembly. It then keeps the sealed, non-nested, non-generic classes and reads each one's
extension methods.

The crypto/tls test host asks such a question during package init. Cold, its 162 go2cs assemblies cost:

| step | cost |
|:--|:--|
| `GetTypes()` (loads 16,185 types) | 171 to 200 ms |
| the method filter (204 classes, 10,352 methods) | 26 to 29 ms |

`GetTypes()` is about 85% of the scan. Only 204 of those 16,185 types matter.

## The decision

Find the classes in the assembly's metadata tables instead, and load only those:

1. `Assembly.TryGetRawMetadata` gives the assembly's metadata blob. `System.Reflection.Metadata.MetadataReader`
   walks its TypeDef table without loading any type.
2. Keep a TypeDef that meets all of these:
   - it is sealed, not nested and not generic, the same shape filter as before;
   - it carries `System.Runtime.CompilerServices.ExtensionAttribute`. C# emits that attribute on every class that
     declares an extension method, as well as on the method.
3. `Module.ResolveType(token)` loads each kept class. The unchanged method filter (`GetMethods` +
   `IsDefined(ExtensionAttribute)`) then reads its methods.
4. An assembly without raw metadata falls back to `GetTypes()`. That is Native AOT, where `TryGetRawMetadata`
   returns false.

The order is unchanged. Metadata order is the order `GetTypes()` returns types in, and the per-class step is the
same code, so the index holds the same methods in the same order. `GetExtensionMethod`'s tie-break depends on that
order.

Measured on the crypto/tls host's closure: the table walk takes 7 to 12 ms and the resolve plus filter about 50 ms.
It finds the same 10,352 methods.

## Why not a build-time list from go2cs-gen

A list emitted into each converted assembly would replace only the 7 to 12 ms table walk. It would still need the
same resolve and filter. It would also:
- change generated output in every converted assembly;
- give each assembly a list that can disagree with the code it describes. That is a staleness surface the metadata
  tables do not have, because they are the code.

## Guard

`ExtensionScanMetadataTests` checks every non-dynamic assembly loaded in GolibTests, the framework included. For
each one, the classes the metadata walk names must yield exactly the methods `GetTypes()` yields, in the same order.

## Not changed

- Which assemblies are scanned. golib is now skipped by identity (step a of this seat).
- The per-type caches and their clearing on `AssemblyLoad`.
- The lock-free publish.
- The emitted surface. No converter or go2cs-gen change; the corpus does not move.
