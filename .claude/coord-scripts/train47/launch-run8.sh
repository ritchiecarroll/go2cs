export TRAIN47_REQUIRE_ALL=1
cd "$(dirname "$0")"
bash coord-train47-assemble-run8.sh > coord-train47-assemble-run8.stdout 2>&1; rc=$?
# the wrapper's OWN trailing line -- the land's exit scan requires it as the record's last non-empty line (2026-09-13 run 8)
printf 'assembly exit=%s\n' "$rc" >> coord-train47-assemble-run8.stdout
echo "=== ASSEMBLY EXIT rc=$rc ==="
tail -40 coord-train47-assemble-run8.stdout
exit $rc
