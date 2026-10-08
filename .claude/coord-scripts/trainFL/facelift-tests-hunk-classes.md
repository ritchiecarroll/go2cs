# Face-lift train: -tests hunk classes, one per step (input of the landing refresh classifier)

Ruled 2026-10-07 22:49 (ledger): the committed -tests sources refresh at the LANDING from the battery's sweeps and the
i9's shard; every face-lift step names the class of its -tests hunks; anything outside these classes is read by name.

## C2's steps A, F, E, D, D2, S
Verbatim in C2's mailbox post 62566dca42 (claude/mailbox, docs/phase4/inbox/COORD/20261008T041931Z-C2.md).
Section 11 rows 5 and 1 (production half): C2's post 02ade5ea32. Rows 1+2 (bridge), 4 and 6: in their step posts.

## G's B, C and section 11 row 3 (claude/g-facelift-bc-converter-q 5e822b6f20; G, direct message 2026-10-08 00:5x)
- C1 PLAIN [GoType] REMOVED: the `[GoType] ` token and its one trailing space are deleted wherever they sit; the line
  is otherwise byte-identical. `[GoType] public partial struct hmacDRBG {` -> `public partial struct hmacDRBG {`
- C2 DEFINITION MOVES TO A COMMENT: `[GoType("D")] ` deleted, ` /*D*/` (D verbatim) right after the name, or after the
  `>` of its type-parameter list; the tail is ` {`, `;` or ` where ...`.
  `[GoType("num:nint")] partial struct Format;` -> `partial struct Format /*num:nint*/;`
  `[GoType("dyn")] [GoLocalName("result")] internal partial struct lookupProtocol_result {` ->
  `[GoLocalName("result")] internal partial struct lookupProtocol_result /*dyn*/ {`
- C3 ARRAY LENGTH ANNOTATION (2-line hunk): the attribute leaves the annotation line and the definition joins the next.
  `[GoType("[512]byte")] /* [blockSize]byte */` + `partial struct block;` -> `/* [blockSize]byte */` + `partial struct block /*[512]byte*/;`
- C4 OPERATOR-SET INTERFACE (2 lines become 1): the attribute line deleted, the definition after the CRTP list.
  `[GoType("operators = Sum, Comparable, Ordered")]` + `partial interface Ordered<ΔT> {` ->
  `partial interface Ordered<ΔT> /*operators = Sum, Comparable, Ordered*/ {`
- C5 POSITION-MAP RE-ENCODE, only in a compilation that carries C4: one `[assembly: go.GoPositionMap(...)]` line whose table changes.
- C6 TYPE-ACCESSIBILITY PROSE, one line per info file carrying the section:
  "// `[GoType]` declarations in this package's converted sources are deliberately" ->
  "// Go type declarations in this package's converted sources are deliberately"
- C7 ROW 3, DIRECTIONAL CHANNEL (outermost single direction only): both attribute tokens deleted, Go's spelling in the comment.
  `[GoType("chan nint")] [GoChanDir(GoChanDir.Recv)] partial struct IntChanRecv;` -> `partial struct IntChanRecv /*<-chan nint*/;`
  Population: reflect 5 sites, reflectlite 1; production 0.
- NEVER A HUNK, by design: a definition containing `/*` or `*/` keeps `[GoType("...")]`; a nested direction chain keeps [GoChanDir(a, b)].
- Behavioral goldens carry the same C1-C7.
