# crypto/tls's Linux fourth state, attributed: WSL's `/etc/hosts` and BoGo's IPv6 listener

A record, dated 2026-09-23 and POST-HOP. It is on `fa18863b94` (the H10 close), and it is unmerged
until the 1.24.13.1 release.

## The state

At the close, crypto/tls's Linux annotation reads 1341 + 1. Go's OWN `TestBogoSuite` fails on the
G-LAPTOP WSL arm, so the row reads fail / fail on its root (ledger: the "fourth, oracle-broken" state).
769 of 3,418 cases failed, 767 of them with Go's shim logging
`dial tcp 127.0.0.1:<port>: connect: connection refused` (bogo_shim_test.go:484).

## The mechanism

Two halves, each correct on its own.

- **The runner listens on IPv6 when it can.** BoringSSL's runner
  (`ssl/test/runner/shim_dispatcher.go:37-40`, the pinned module `v0.0.0-20241120195446-5cce3fbd23e1`)
  listens on `[::1]` and falls back to `127.0.0.1` only when the IPv6 listen FAILS. When it is on IPv6,
  it passes the shim `-ipv6` (runner.go, `listener.IsIPv6()`).
- **Go's shim ignores that flag and dials by name.** Go's shim declares the flag and discards it:
  `_ = flag.Bool("ipv6", false, "")` at `$GOROOT(go1.24.13)/src/crypto/tls/bogo_shim_test.go:62`.
  It dials `net.JoinHostPort("localhost", *port)` at `:286`. With cgo off, Go resolves `localhost`
  from `/etc/hosts`.

On this arm, IPv6 loopback is up (`disable_ipv6 = 0`, `::1` on `lo`), so the runner is on `[::1]`.
The WSL-generated `/etc/hosts` maps `localhost` to IPv4 only:

    127.0.0.1 localhost
    ::1 ip6-localhost ip6-loopback

Go therefore resolves `localhost -> [127.0.0.1]`. The shim dials IPv4 while the runner listens on
IPv6 alone, and the connection is refused. The 255 cases that pass are the ones whose expected
outcome needs no connection.

## The measurement

A discriminating A/B that changes no system setting. Each arm runs Go's own
`go test -count=1 -run '^TestBogoSuite$' -v crypto/tls` (go1.24.13, cgo off) inside a private
user+mount namespace (`unshare -rm`), with a copy of `/etc/hosts` bind-mounted over the real one.

| Arm | Hosts copy | `localhost` resolves to | TestBogoSuite | Cases |
|:--|:--|:--|:--|:--|
| control | identical | `[127.0.0.1]` | FAIL, rc 1 | 255 pass / 769 fail (767 refused) / 2,396 skip |
| v6 | `::1 localhost` appended | `[127.0.0.1 ::1]` | **PASS, rc 0** | **1,022 pass / 0 fail / 2,396 skip** |

- The control reproduces the fourth state exactly.
- The v6 arm is the FULL fan-out. Its 1,022 / 2,396 split is the split R banked on Windows
  (`3ec2c9ff39`).
- The system `/etc/hosts` was byte-identical to the control copy afterwards.

## What it means

- **The fourth state is a property of the host's `/etc/hosts`, not of go2cs.** It will not reproduce
  on a host whose hosts file names `::1 localhost`, or one without IPv6 loopback.
- **The same mechanism lives in Go's test itself.** Its shim ignores `-ipv6` and dials a NAME. That
  is an upstream test-harness fragility worth one line to the Go issue tracker. It is not ours to fix
  in the corpus.
- **Re-reading the row would be an owner hand, and it is not taken here.** The step: add
  `::1 localhost` to the WSL arm's `/etc/hosts`, with `generateHosts = false` in `/etc/wsl.conf` so a
  distro restart does not regenerate the file. Or run the read inside the namespace above. The Go side
  would then reach the full fan-out.
- **What the converted side would then show is UNMEASURED.** Its shim is the converted test host, so a
  converted spawn per case against the runner's 600 s wall is the third state's question on Linux.
