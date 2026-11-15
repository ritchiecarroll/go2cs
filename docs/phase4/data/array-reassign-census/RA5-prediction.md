# RA-5, written before CNR runs (R, 2026-11-15)

The cut: claude/r-array-reassign b6ef01b76720d0f896c279ca2c87135c9ea51e26, the converter at 93fb26d9f5.

**The census.** Census v2 ran over all 992 behavioral programs (each folder is its own module), host
windows/amd64 (`readings/v2/behavioral-windows-amd64.tsv`). The five linux-only programs that do not load on
windows (NativeFieldPortAlias, NativeViewPointerEquality, ScmRightsSeam, SetegidBroadcastSeam, WritevIovecSeam)
were read under GOOS=linux, and each has 0 sites. 12 programs hold 48 sites.

| Program | Sites |
|---|---|
| ArrayReassignForms | 19 |
| ArrayReassignUnsafe | 9 |
| ArrayReassignAliases | 7 |
| ArrayPassByValue | 4 |
| MapArrayValueZero | 2 |
| NamedPointerParamArgument | 1 |
| NamedArrayWrapper | 1 |
| JsonFixedArrayUnmarshal | 1 |
| DeferMultiValueSpread | 1 |
| ArrayValueCopySites | 1 |
| ArraySwitchCases | 1 |
| ArrayCastDerefClone | 1 |

**The prediction.** CNR, which re-transpiles every program and compares it with its committed golden, moves the
goldens of exactly these 9 programs:

ArrayPassByValue, MapArrayValueZero, NamedPointerParamArgument, NamedArrayWrapper, JsonFixedArrayUnmarshal,
DeferMultiValueSpread, ArrayValueCopySites, ArraySwitchCases, ArrayCastDerefClone.

The cut's own three kits are already re-baselined in the cut, so they read equal.

- A site the census counts can still emit unchanged when its struct holds an array only through an embed, or only
  as a blank zero-length field (the std reconciliation's two over-count classes). Such a program would read equal.
- N1 (a conversion between array types now clones) is NOT in this census. Any golden outside the 9 that moves must
  be an N1 conversion site in its diff. A moved golden whose diff is neither a reassign site nor an N1 conversion
  falsifies the prediction.
- Package_info.cs moves only with a tuple site; the census lists none outside the cut's own kits.
