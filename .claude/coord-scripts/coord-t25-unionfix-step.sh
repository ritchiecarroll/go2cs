# UNION FIX (pre-resolved at the rehearsal, coord-t25-dryrun @ 3edac6aba): SUBQ36 (net/http 1343 -> 1345) and SUBQ29 (testing 35 -> 37) each moved the
# roster header from 27,772 / 169 to 27,774 / 167 for DIFFERENT rows -- the same number on both branches merges cleanly and the union's truth is the
# guard's, 27,776 / 165. The artifact is the rehearsal's recomposed roster; it must differ from the assembled roster by EXACTLY the header line.
stamp "UNION FIX: roster header recomposition from the rehearsal artifact"
UF="$SP/coord-t25-resolutions/union-fix/docs/ValidatedTestPackages.md"
[ -f "$UF" ] || { stamp "union-fix artifact MISSING -- ABORT"; exit 1; }
cp "$UF" docs/ValidatedTestPackages.md
ufns=$(git diff --numstat -- docs/ValidatedTestPackages.md | cut -f1,2 | tr '\t' '/')
if [ "$ufns" != "1/1" ]; then stamp "union-fix numstat '$ufns' != 1/1 -- the assembled roster differs from the rehearsal's; ABORT"; git checkout -- docs/ValidatedTestPackages.md; exit 1; fi
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train25-unionfix-guard-$ts.log" 2>&1; ufg=$?
[ "$ufg" = "0" ] || { stamp "union-fix roster guard exit=$ufg -- ABORT: $(grep -aiE 'expected|FAIL' "$SP/coord-train25-unionfix-guard-$ts.log" | head -2 | tr '\n' ' ')"; git checkout -- docs/ValidatedTestPackages.md; exit 1; }
git add docs/ValidatedTestPackages.md && git commit -S -q -F "$SP/coord-t25-resolutions/union-fix/message.txt" || { stamp "union-fix commit FAILED -- ABORT"; exit 1; }
stamp "UNION FIX committed $(git rev-parse --short HEAD) :: $(grep -a 'checks pass' "$SP/coord-train25-unionfix-guard-$ts.log" | tail -1)"
