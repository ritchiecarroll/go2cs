export TRAIN47_REQUIRE_ALL=1
cd "$(dirname "$0")"
bash coord-train47-assemble-run5.sh > coord-train47-assemble-run5.stdout 2>&1; rc=$?
echo "=== ASSEMBLY EXIT rc=$rc ==="
tail -40 coord-train47-assemble-run5.stdout
exit $rc
