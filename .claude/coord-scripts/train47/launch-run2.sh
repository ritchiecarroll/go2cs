export TRAIN47_REQUIRE_ALL=1
bash coord-train47-assemble-run2.sh > coord-train47-assemble-run2.stdout 2>&1; rc=$?
echo "=== ASSEMBLY EXIT rc=$rc ==="
tail -40 coord-train47-assemble-run2.stdout
exit $rc
