# Glyph exhibit: candidate suffixes for method bodies

This is SCRATCH material from the global ᴍ cost test (COORD 20261120T202904Z, owner items of 11-21). It is never for master.

Each variant folder holds the same converted C#, spelled with that variant's suffix, so the files can be opened side by side in Visual Studio or VS Code:
- `strings/builder.cs`: strings.Builder. Its method bodies sit in `strings_package` as `internal static <Name><glyph>(…)`, stamped `[GoMethodBody("<Name>")]`.
- `strings/package_info.Builder.cs`: an excerpt of strings' package_info.cs holding the converter-written, Go-named forwarders in the holder `stringsᴛBuilder`.
- `vertex/`: the Tour's Vertex (`Abs`, `Scale`) as a LIBRARY package, under the final form. Package main keeps P1's plain form and uses the suffix only on a rare real name clash (owner ruling 11-21).

`master/` holds today's form for comparison. `U+1D0D-small-capital-m/builder-exhibit.png` is the rendered side-by-side of the current choice: Go | master | ᴍ form, plus the forwarders.

The text is identical across variants, byte for byte, except for the suffix. It was generated from the prototype's emission at the scratch tip (f0d12f49fc, base master f604d2ae89).

## The candidates

| Folder | Glyph | Codepoint | Category | Can Go spell it? | Escape + stamp needed? | Cascadia Code | Cascadia Mono | JetBrains Mono | Consolas | Why it is here |
|---|---|---|---|---|---|---|---|---|---|---|
| `U+1D0D-small-capital-m` | ᴍ | U+1D0D LATIN LETTER SMALL CAPITAL M | Ll | yes | yes | no | no | no | not measured | The current choice: "m" for method, and the same small-capital family as the `ᴛ` in every holder name (`stringsᴛBuilder`). |
| `U+0280-small-capital-r` | ʀ | U+0280 LATIN LETTER SMALL CAPITAL R | Ll | yes | yes | no | no | no | not measured | "r" for receiver. |
| `U+2040-character-tie` | ⁀ | U+2040 CHARACTER TIE | Pc | NO | no escape. The stamp is still wanted for frame naming. | no | no | no | not measured | Connector punctuation that a C# identifier accepts and Go cannot spell, so no Go name can collide with a body. |
| `U+2054-inverted-undertie` | ⁔ | U+2054 INVERTED UNDERTIE | Pc | NO | no escape (as ⁀) | no | no | no | not measured | ⁀'s sibling. It sits on the baseline like an underscore, so it reads quieter mid-line. |
| `U+02BC-modifier-apostrophe` | ʼ | U+02BC MODIFIER LETTER APOSTROPHE | Lm | yes | yes | **yes** | **yes** | **yes** | not measured | The only quiet identifier-legal mark found in all three measured fonts, so it needs no fallback. Risk: it reads as a quote or prime. |
| `U+141F-canadian-final-acute` | ᐟ | U+141F CANADIAN SYLLABICS FINAL ACUTE | Lo | yes | yes | no | no | no | not measured | A small raised tick that reads like a prime without being an apostrophe. |

Notes on the columns:
- **Can Go spell it?** A Go identifier admits Unicode letters (categories L*) and `_`. Pc other than `_` is not allowed.
  - Where Go CAN spell the glyph, a Go identifier ending in it would collide with a body name. The converter then needs the doubling escape for such identifiers, and the runtime must name frames from the `[GoMethodBody]` stamp rather than the spelling.
  - Where Go cannot spell it, the suffix alone is unambiguous. The stamp stays the single discriminator either way.
- **C# acceptance.** All six are identifier characters under C#'s rules: Ll, Lm, Lo and Pc are all admitted.
  - Compiled on this lane: ᴍ (the whole corpus), and ⁀, ⁔ and ᐟ (GolibTests' GlyphForwarderPrototypeTests).
  - ʀ and ʼ are admitted by category but were not compiled here.

## Font coverage

### How it was measured
- Read with fontTools from each font's cmap, in the Regular weight:
  - Cascadia Code and Cascadia Mono, version 2407.024, from the Google Fonts build in `@expo-google-fonts/cascadia-code@0.4.1` and `@expo-google-fonts/cascadia-mono@0.4.1`. 2,426 codepoints each.
  - JetBrains Mono 2.211, from `@expo-google-fonts/jetbrains-mono`. 976 codepoints.
- "no" means the font has no glyph, so the editor draws the character from a FALLBACK font. On Windows that is usually Segoe UI Symbol or Cambria Math, which changes its width and weight.

### The corpus already relies on fallback
These three fonts cover none of the corpus's existing marks:

| Mark | Codepoint | Where it appears |
|---|---|---|
| `ᴛ` | U+1D1B | every holder name |
| `Ꮡ` | U+13D1 | address-of names |
| `ꓸ` | U+A4F8 | package dots |
| `ˢ`, `ᶜ`, `ᵇ` | U+02E2, U+1D9C, U+1D47 | hoist names |

`ж` (U+0436) and `Δ` (U+0394) are covered. So ᴍ renders exactly as `ᴛ` already does today.

### Consolas
Consolas is proprietary and could not be measured on this box. To read it on a Windows box, run this in Windows PowerShell 5.1:

    Add-Type -AssemblyName PresentationCore
    $f = New-Object Windows.Media.GlyphTypeface ([uri]'C:\Windows\Fonts\consola.ttf')
    0x1D0D,0x0280,0x2040,0x2054,0x02BC,0x141F,0x1D1B | % { '{0:X4} {1}' -f $_, $f.CharacterToGlyphMap.ContainsKey($_) }

The last code, 0x1D1B, is today's `ᴛ`, as a control.
