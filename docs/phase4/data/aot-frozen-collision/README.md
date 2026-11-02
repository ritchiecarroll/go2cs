# Native AOT frozen-object name collision

Native AOT failed to publish any darwin program that reaches `syscall`, inside ILC:

```
EXEC : error : An item with the same key has already been added. Key: __FrozenObj_syscall_go_syscall_package_IfmaMsghdr23
```

The BOARD entry of 2026-11-01 records the mechanism, the regression proof and the fix. This folder holds what it
measured with.

## The census

`frozen-census.pl` lists every same-package struct pair `X` and `X<digits>` per GOOS flavour, and predicts which can
collide. It models each struct's field accessor tokens, one allocation per non-blank field at interpreted
instructions 3, 7, 11 and so on, and reports a pair when a site id of `X` equals the digits followed by a site id of
`X<digits>`.

```
perl docs/phase4/data/aot-frozen-collision/frozen-census.pl src/core
```

At master `64d145b2c4` it read 158 pairs and one predicted collision: darwin `syscall`'s `IfmaMsghdr` and
`IfmaMsghdr2`, at ids 23 and 27.

## The standalone repro

`ilc-issue-DRAFT.md` carries a 30-line C# program that reproduces the ILC defect with no go2cs code. Build it with this
project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <PublishAot>true</PublishAot>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Then publish it with `dotnet publish -c Release -r win-x64`. It fails with `Key: __FrozenObj_FrozenRepro_A23`. Rename
`A2` to `AB` and it publishes, runs and exits 8.

## The ILC issue draft

`ilc-issue-DRAFT.md` is a draft for dotnet/runtime. It has not been posted; the owner decides whether it is filed.
