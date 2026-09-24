# c2-rva-dedup -- does Roslyn store identical u8 literal data once per module? (point-in-time record)

Input to [`DESIGN-string-literal-allocation.md`](../../DESIGN-string-literal-allocation.md) §8.1.x and
§8.1R2.2. The registration table keys on a literal's ADDRESS, so it depends on every `"n"u8` in a module
handing out the same address. The C# specification does not promise this; Roslyn does it.

**Pinned:**
- equal addresses across methods, a nested type, `Generic<int>`/`Generic<string>` instantiations and a
  second source file (`OtherFile.cs`);
- 1 B and 32 B literals;
- stable across calls;
- no prefix sharing (`"abc"` vs `"abcd"`), and a suffix slice is not the shorter literal.

**Run:** Debug, Release and a ReadyToRun publish (SDK 10.0.112, runtime 10.0.12, linux-x64), plus an
earlier NativeAOT publish. The outputs are in `../c2-literal-cache/output-round2-linux.txt` and
`output-round3-linux.txt`. The printed addresses differ per run (ASLR); the equalities are the result.

**Not pinned:**
- a literal emitted by a source generator;
- Windows.

```
dotnet run -c Release
dotnet publish -c Release -r linux-x64 -p:PublishReadyToRun=true --self-contained false -o out && out/c2-rva-dedup
```
