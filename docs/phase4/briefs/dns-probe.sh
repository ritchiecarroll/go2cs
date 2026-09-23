#!/usr/bin/env bash
# dns-probe.sh -- READ-ONLY DNS lane probe, WSL/Linux leg: does this box's DNS answer NXDOMAIN the way Go 1.24.13
# net's TestLookupNoSuchHost needs? Changes NO setting; no go2cs, no dotnet, no worktree. Output is ADDRESS-FREE:
# resolvers print as KINDS, the raw logs as leaf names; no home path, host name or account name is printed.
# SHOW_ADDRESSES=1 is OWNER-CONSOLE-ONLY (stderr): a Claude lane's tool captures stderr too, so a lane never sets it.
# The raw logs (/tmp) can hold resolver addresses and the host name: they stay local, never on a pushed surface.
# Run it as /tmp/dns-probe.sh inside the distro (never from a share): a shell error prints the script's own path.
# Usage: GO124=<linux go1.24.13>/bin/go PUBLIC_RESOLVER=<v4>[,<v6>] bash /tmp/dns-probe.sh [lookups-only]
# Exit: 0 = ran to its VERDICT line (ANY verdict: the VERDICT line is the result), 2 = ABORT (nothing measured).
# Sourcing (the unit tests) defines the functions and returns before anything runs. A CRLF-saved copy self-heals:
if [ "${BASH_SOURCE[0]}" = "$0" ] && [ "$0" != "$DNS_PROBE_LF" ] && [ "$(tr -cd '\r' <"$0" | wc -c)" -gt 0 ]; then f=$(mktemp "${TMPDIR:-/tmp}/dns-probe-heal.XXXXXX") && tr -d '\r' <"$0" >"$f" && DNS_PROBE_LF=$f exec bash "$f" "$@"; fi #
[ "${BASH_SOURCE[0]}" = "$0" ] && [ "$0" = "$DNS_PROBE_LF" ] && case "$0" in */dns-probe-heal.*) trap 'rm -f -- "$0"' EXIT;; esac  # ONLY the healed temp copy removes itself
MARKERS=('panic: test timed out' 'unexpected success' 'IsNotFound is set to false' 'server misbehaving' 'DNS server failure'
  'i/o timeout' 'timeout period expired' 'temporary error')
NOTE='NOTE: DNS-CONFORMING is NOT host qualification: banking net still needs go test -count=1 -timeout 40m net, its failing set judged by the ledger.'
canon(){ local a=${1%%\%*}; a=${a,,}  # an address compared AS an address: zone dropped, lowercase, IPv6 in RFC 5952 form
  case "$a" in *:*) ;; *) printf '%s\n' "$a"; return;; esac
  awk -v a="$a" 'function hx(s,  v, i, c) { if (s == "" || length(s) > 4) return -1; v = 0
      for (i = 1; i <= length(s); i++) { c = index("0123456789abcdef", substr(s, i, 1)); if (!c) return -1; v = v * 16 + c - 1 }; return v }
    BEGIN { n = split(a, h, "::"); if (n > 2) { print a; exit }
      nl = (h[1] == "") ? 0 : split(h[1], l, ":"); nr = (n == 2 && h[2] != "") ? split(h[2], r, ":") : 0
      if (nr && r[nr] ~ /\./) { split(r[nr], o, "."); r[nr] = sprintf("%x", o[1] * 256 + o[2]); r[++nr] = sprintf("%x", o[3] * 256 + o[4]) }
      else if (!nr && nl && l[nl] ~ /\./) { split(l[nl], o, "."); l[nl] = sprintf("%x", o[1] * 256 + o[2]); l[++nl] = sprintf("%x", o[3] * 256 + o[4]) }
      if ((n == 1 && nl != 8) || (n == 2 && nl + nr > 7)) { print a; exit }
      k = 0; for (i = 1; i <= nl; i++) g[++k] = hx(l[i]); if (n == 2) for (i = nl + nr; i < 8; i++) g[++k] = 0
      for (i = 1; i <= nr; i++) g[++k] = hx(r[i]); for (i = 1; i <= 8; i++) if (g[i] < 0) { print a; exit }
      bs = 0; bl = 1; cl = 0; for (i = 1; i <= 8; i++) if (g[i] == 0) { if (++cl > bl) { bl = cl; bs = i - cl + 1 } } else cl = 0
      out = ""; for (i = 1; i <= 8; i++) { if (i == bs) { out = out "::"; i += bl - 1; continue }
        out = out ((out == "" || out ~ /:$/) ? "" : ":") sprintf("%x", g[i]) }; print out }'; }
pubs=$(for p in $(printf '%s' "$PUBLIC_RESOLVER" | tr ',;' '  '); do canon "$p"; done)
kind(){ local a p; a=$(canon "$1"); for p in $pubs; do [ "$a" = "$p" ] && { echo ctl-match; return; }; done
  case "$a" in fec0:*) echo fec0;; fe[89ab]?:*) echo ll6;; f[cd]*) echo ula6;; 127.*|::1) echo loop;;
    10.*|192.168.*|172.1[6-9].*|172.2[0-9].*|172.3[01].*) echo lan4;;
    100.6[4-9].*|100.[7-9][0-9].*|100.1[01][0-9].*|100.12[0-7].*) echo cgnat;; 169.254.*) echo ll4;; *) echo public;; esac; }
digclass(){ awk '/status: /{s=$0; sub(/.*status: /,"",s); sub(/,.*/,"",s)} /ANSWER: /{a=$0; sub(/.*ANSWER: /,"",a); sub(/,.*/,"",a)}
  /Query time: /{t=$4} /timed out/{to=1}
  END{ if (s=="") print (to ? "TIMEOUT" : "no-reply"); else { if (s=="NOERROR") s=(a+0>0 ? "ANSWERED" : "NODATA"); print s "/" t "ms" } }'; }
look(){ python3 -c 'import socket,sys,time
t=time.time()
try: socket.getaddrinfo(sys.argv[1],None); r="ANSWERED"
except socket.gaierror as e: r={-2:"NXDOMAIN",-3:"TEMPFAIL",-5:"NODATA"}.get(e.errno,"gai%d"%e.errno)
print("%s/%.2fs"%(r,time.time()-t))' "$1" 2>/dev/null || echo python3-absent; }
arm(){ local lab=$1 a=$2 nx=$3 q out=''  # example.com control, a fresh nx .com, then every type the NoSuchHost leaves query (host = A + AAAA)
  for q in 'ex.com A example.com' "nx.com A $nx" 'host A invalid.invalid.' 'AAAA AAAA invalid.invalid.' 'CNAME CNAME invalid.invalid.' \
      'MX MX invalid.invalid.' 'NS NS invalid.invalid.' 'TXT TXT invalid.invalid.' 'SRV SRV _unknown._tcp.invalid.invalid.'; do
    set -- $q; out+=" $1=$(dig +time=5 +tries=1 @"$a" -t "$2" "$3" 2>&1 | digclass)"; done
  printf '%-52s%s\n' "$lab" "$out"; }
cnt(){ local c; c=$(grep -c "$@" 2>/dev/null); echo "${c:-0}"; }  # a missing log counts 0, never a shell error
testline(){ local log=$1 n=$2 m  # one test's root result, read from ITS OWN call's log
  m=$(grep -m1 -o -E "^--- (PASS|FAIL|SKIP): $n \([0-9.]+s\)" "$log" 2>/dev/null | sed -E 's/^--- ([A-Z]+): [^ ]+ \((.*)\)$/\1 \2/')
  if [ -n "$m" ]; then echo "$m"; elif [ "$(cnt -E "^=== RUN[[:space:]]+$n\$" "$log")" = 0 ]; then echo 'NOT REACHED'
  elif [ "$(cnt -F 'panic: test timed out' "$log")" != 0 ]; then echo 'KILLED (-timeout 40m)'; else echo 'DIED (no result line; read the raw log locally)'; fi; }
summarize(){ local l1=$1 log=$2 rc=$3 n k root par leaf ran pass rootl skip to sf cls v; declare -A C
  # pure: l1 = the CNAME/LocalPTR call's go test -v log, log = the NoSuchHost call's -> readout + VERDICT (never echoes a log line).
  # Everything that judges NoSuchHost (totals, modes, markers, the kill, the class) reads the NoSuchHost call's log ONLY.
  for n in TestLookupCNAME TestLookupLocalPTR; do printf '%-22s %s\n' "$n" "$(testline "$l1" "$n")"; done
  printf '%-22s %s\n' TestLookupNoSuchHost "$(testline "$log" TestLookupNoSuchHost)"
  [ "$(cnt -F 'panic: test timed out' "$l1")" != 0 ] && echo 'CNAME/LocalPTR call killed by -timeout 40m: its two tests are partial (not a NoSuchHost verdict)'
  root=$(cnt -E '^--- FAIL: TestLookupNoSuchHost ' "$log"); par=$(cnt -E '^[[:space:]]+--- FAIL: TestLookupNoSuchHost/[^/[:space:]]+ \(' "$log")
  leaf=$(cnt -E '^[[:space:]]+--- FAIL: TestLookupNoSuchHost/[^/[:space:]]+/' "$log")
  echo "NoSuchHost failing verdicts: root $root + parents $par + leaves $leaf = $((root + par + leaf))"
  grep -o -E '^[[:space:]]+--- FAIL: TestLookupNoSuchHost/[^/[:space:]]+/[^[:space:]]+' "$log" 2>/dev/null | sed -E 's/.*NoSuchHost\///; s/_resolver$//' |
    awk -F/ '{a[$1]=a[$1] " " $2} END {for (k in a) printf "  %-22s failing modes:%s\n", k, a[k]}' | sort
  for k in "${MARKERS[@]}"; do C[$k]=$(cnt -F -- "$k" "$log"); printf '  %-28s x%s\n' "$k" "${C[$k]}"; done
  [ "$(cnt -E '^=== RUN[[:space:]]+TestLookupNoSuchHost$' "$log")" != 0 ] && ran=1 || ran=0
  [ "$(cnt -E '^--- PASS: TestLookupNoSuchHost ' "$log")" != 0 ] && pass=1 || pass=0
  rootl=$(cnt -E '^--- (PASS|FAIL|SKIP): TestLookupNoSuchHost \(' "$log"); skip=$(cnt -E '^[[:space:]]*--- SKIP: TestLookupNoSuchHost' "$log")
  to=$(( ${C['i/o timeout']} + ${C['timeout period expired']} )); sf=$(( ${C['server misbehaving']} + ${C['DNS server failure']} ))
  if [ "${C['unexpected success']}" -gt 0 ]; then cls='HIJACK class (a nonexistent name answered)'
  elif [ $to -gt 0 ] && [ $sf -gt 0 ]; then cls='MIXED class (SERVFAIL + TIMEOUT)'; elif [ $to -gt 0 ]; then cls='TIMEOUT class'
  elif [ $sf -gt 0 ]; then cls='SERVFAIL class'; else cls='class unread (see the counts)'; fi
  if [ $ran = 0 ]; then v="NO RUN (build/launch failed: no \"=== RUN   TestLookupNoSuchHost\" in its call's log)"
  elif [ "${C['panic: test timed out']}" -gt 0 ]; then v='BROKEN, TIMEOUT class (killed by -timeout 40m; the leaf counts are partial) -- never a pass'
  elif [ "$skip" -gt 0 ]; then v='NOT JUDGEABLE (TestLookupNoSuchHost or a leaf SKIPPED)'
  elif [ "$rootl" = 0 ]; then v='NO VERDICT (test binary died; read the raw log locally)'
  elif [ $pass = 1 ] && [ "$rc" = 0 ]; then v='DNS-CONFORMING (TestLookupNoSuchHost passed, rc=0; the CNAME drift is tolerated)'
  elif [ $pass = 1 ] || [ "$rc" = 0 ]; then v="INCONSISTENT (log PASS=$pass but rc=$rc): read the raw log locally"
  else v="BROKEN, $cls"; fi
  echo "VERDICT: $v"; }
[ "${BASH_SOURCE[0]}" != "$0" ] && return 0

stamp=$(date +%Y%m%d%H%M%S)
echo "dns-probe.sh (WSL/Linux leg) stamp $stamp -- read-only; resolvers print as KINDS only"; echo "$NOTE"
case "$GODEBUG" in *netdns*) echo 'ABORT: GODEBUG names netdns: it changes which resolver the default leaves use'; exit 2;; esac
was=''; for k in GODEBUG GOFLAGS GO_BUILDER_FLAKY_NET GOCACHEPROG GOOS GOARCH GOEXPERIMENT; do was+=" $k=$([ -n "${!k}" ] && echo SET || echo unset)"; done
unset GODEBUG GOFLAGS GO_BUILDER_FLAKY_NET GOCACHEPROG GOOS GOARCH GOEXPERIMENT  # FLAKY_NET turns every DNS failure into a SKIP
export GOTOOLCHAIN=local CGO_ENABLED=0 GOENV=off GOWORK=off                    # GOENV=off: a 'go env -w' GOFLAGS cannot apply
gobin=$(command -v "${GO124:-go}") || { echo 'ABORT: no go binary (set GO124)'; exit 2; }; GO=$(readlink -f "$gobin")
GOROOT=$(cd "$(dirname "$GO")/.." && pwd -P); export GOROOT   # the binary's own root, never an inherited one
cd /tmp || exit 2   # before the FIRST go call: a go.mod/go.work in the caller's cwd can refuse GOTOOLCHAIN=local
ver=$("$GO" version 2>/dev/null); rc=$?   # stderr dropped: a cmd/go warning can quote a path; the guards judge the values
cgo=$("$GO" env CGO_ENABLED 2>/dev/null); gfl=$("$GO" env GOFLAGS 2>/dev/null); genv=$("$GO" env GOENV 2>/dev/null)
[ "$("$GO" env GOROOT 2>/dev/null)" = "$GOROOT" ] && rootok=yes || rootok=MISMATCH
echo "go: $ver (rc=$rc) CGO_ENABLED=$cgo GOTOOLCHAIN=local GOENV=off GOWORK=off (env file: $([ -n "$genv" ] && echo IN-USE || echo none))" \
  "GOFLAGS=$([ -n "$gfl" ] && echo SET || echo empty) GOROOT=binary's root, match $rootok"
echo "env the probe cleared (value before: SET/unset; values are never printed):$was"
if [ "$rc" != 0 ] || [ "$cgo" != 0 ] || [ "$rootok" != yes ] || [ -n "$gfl" ] || [ -n "$genv" ] || [[ "$ver" != *"go1.24.13 linux/"* ]]; then
  echo "ABORT: need the linux go1.24.13, CGO_ENABLED=0, empty GOFLAGS, no go env file, go env GOROOT = the binary's root"; exit 2; fi

echo "--- resolver readout, KINDS only (with CGO_ENABLED=0 all three Go modes use Go's own resolver on resolv.conf's FIRST 3 nameservers)"
g=$(grep -i -E '^[[:space:]]*generateResolvConf' /etc/wsl.conf 2>/dev/null | tr -d '\r')
echo "wsl.conf: ${g:-no generateResolvConf line (default: WSL generates resolv.conf)}"
t=$(readlink -f /etc/resolv.conf); case "$t" in /etc/*|/run/*|/mnt/wsl/*|/var/*) ;; *) t="other (leaf ${t##*/})";; esac
if [ -L /etc/resolv.conf ]; then case "$t" in
    */stub-resolv.conf) lab='SYMLINK -> systemd-resolved STUB (loopback stub; the real upstreams live in resolved, not in this file)';;
    */systemd/resolve/*) lab='SYMLINK -> systemd-resolved upstream list';; /mnt/wsl/*) lab='SYMLINK -> WSL-generated (follows the Windows host)';;
    *) lab='SYMLINK -> other';; esac
elif grep -q -i 'generated by WSL' /etc/resolv.conf 2>/dev/null; then lab='regular file with the WSL-generated header (follows the Windows host)'
else lab='regular file, no WSL header (pinned)'; fi
echo "resolv.conf: $lab; readlink -f = $t"
nss=$(grep -E '^[[:space:]]*hosts:' /etc/nsswitch.conf 2>/dev/null | tr -d '\r' | tr -s ' \t' ' ')
echo "nsswitch ${nss:-hosts: (no line)}  <- python getaddrinfo follows this; Go (cgo off) reads resolv.conf directly"
# Go's list (dnsconfig_unix.go): 'nameserver' lines whose value parses as an IP, the first 3 only; later ones are never queried by Go
ns=''; nsx=''; n3=0
for a in $(awk '$1 == "nameserver" && NF > 1 {print $2}' /etc/resolv.conf 2>/dev/null | tr -d '\r'); do
  [[ $a =~ ^[0-9]+(\.[0-9]+){3}$ || $a == *:* ]] || continue
  if [ $n3 -lt 3 ]; then ns+="$a "; n3=$((n3+1)); else nsx+="$a "; fi; done
kl=''; for a in $ns; do kl+="$(kind "$a") "; done; kx=''; for a in $nsx; do kx+="$(kind "$a") "; done
[ -n "$kx" ] && kx="| beyond Go's 3, not queried by Go (no arm): $kx"
echo "nameservers: ${kl:-(none) }$kx"
[ -n "$SHOW_ADDRESSES" ] && echo "  OWNER CONSOLE ONLY - REDACT: $ns| $nsx" >&2
if [ "$1" != lookups-only ]; then  # go test FIRST: no lookup arm may seed a caching resolver (systemd-resolved) before the leaves run
  log1=dns-probe-$stamp-1.log; log2=dns-probe-$stamp-2.log; log=$log1   # one log per call: only call 2's judges NoSuchHost
  for pat in '^(TestLookupCNAME|TestLookupLocalPTR)$' '^TestLookupNoSuchHost$'; do
    t0=$(date +%s); "$GO" test -count=1 -timeout 40m -run "$pat" -v net >"$log" 2>&1; rc=$?; rcnsh=$rc; log=$log2
    echo "go test -run $pat : rc=$rc wall=$(( $(date +%s) - t0 ))s"; done
fi
echo "--- timed lookups: class/time (system = getaddrinfo via nsswitch; server arms = Go's list (first 3) + the public controls, via dig)"
echo "system flap x6 (fresh nx .com): $(for i in 1 2 3 4 5 6; do printf '%s ' "$(look "nx-$stamp-$i.com")"; done)"
echo "system (getaddrinfo) ex.com=$(look example.com) nx.com=$(look "nx-$stamp-a0.com") A_AAAA=$(look invalid.invalid.)  <- acceptance: A_AAAA=NXDOMAIN (errno -2); TEMPFAIL (-3) = broken"
if command -v dig >/dev/null; then i=0; seen=' '
  for a in $ns $pubs; do c=$(canon "$a"); case "$seen" in *" $c "*) continue;; esac; seen+="$c "   # deduped AS addresses
    i=$((i+1)); k=$(kind "$a"); [ "$k" = ctl-match ] && case "$c" in *:*) k='public-ctl v6';; *) k='public-ctl v4';; esac
    arm "server#$i $k" "$a" "nx-$stamp-a$i.com"; done
else echo '(dig absent: the per-server, per-type arms are skipped -- report it; install nothing)'; fi
[ "$1" = lookups-only ] && exit 0
echo "--- go test summary (rc of the NoSuchHost call is a valid discriminator: 0 = passed, no kill; the CNAME call's rc is not; LocalPTR is Windows-only, NOT REACHED here)"
summarize "$log1" "$log2" "$rcnsh"
echo "raw logs, LOCAL ONLY (may hold addresses and the host name): $log1 (CNAME) and $log2 (NoSuchHost), in /tmp"; echo "$NOTE"
exit 0
