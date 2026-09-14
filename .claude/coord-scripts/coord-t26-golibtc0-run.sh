set -u
SP='C:/Projects/go2cs/.claude/coord-scripts'; ts="t26-$(date +%H%M%S)"
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
(cd src/tests/GolibTests && dotnet build -c Release -p:UseSharedCompilation=false -p:go2csPath="$(pwd -W | sed "s#/tests/GolibTests##")/" -clp:ErrorsOnly > "$SP/coord-t26union-golibtc0-build-$ts.log" 2>&1; echo "build exit=$?" >> "$SP/coord-t26union-golibtc0-build-$ts.log"; DOTNET_TieredCompilation=0 dotnet test --no-build -c Release -p:go2csPath="$(pwd -W | sed "s#/tests/GolibTests##")/" --logger "console;verbosity=minimal" > "$SP/coord-t26union-golibtc0-$ts.log" 2>&1; echo "tc0 exit=$?" >> "$SP/coord-t26union-golibtc0-$ts.log")
