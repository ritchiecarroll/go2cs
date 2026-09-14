#!/usr/bin/env bash
# coord-bank-legs.sh <assemble.log> <battery-lines.txt> -- append every "[ts] LEG ..." stamp of the log (except suite+cnr, banked by hand) that the battery file does not already carry. Idempotent.
log="$1"; out="$2"; n=0
grep -a '^\[.*\] LEG ' "$log" | grep -av 'LEG suite+cnr' | while IFS= read -r l; do
  body="${l#\[*\] }"; body="$(echo "$body" | sed 's/  */ /g' | cut -c1-600)"
  key="$(echo "$body" | cut -c1-40)"
  grep -qF -- "$key" "$out" || { echo "$body" >> "$out"; echo "banked: $(echo "$body" | cut -c1-90)"; }
done
