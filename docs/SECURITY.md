## Security

go2cs is a transpiler: it reads Go source and emits C#. The security surface that matters most is the
*generated* code — a conversion defect that silently produces C# with different memory, bounds, or
concurrency behavior than the Go it came from is treated as a security issue, not merely a correctness bug.
Reports against the converter, the `golib` runtime, the `go2cs-gen` source generators, and the converted
standard library under `src/core` are all in scope.

## Supported Versions

Security fixes go into the **latest published release** only: the newest `go.*` packages on NuGet and the
converter at the matching tag. Earlier releases are not patched; the fix is to move to the latest one.

The converted standard library is Go's own code at one Go release, the one the README's
[Requirements](README.md#requirements) name. It carries the Go security fixes that release carries and none
issued after it, so when that Go release is out of upstream support the converted library is behind Go's
current fixes. The [Roadmap](Roadmap.md#go-releases) says which Go releases come next.

## Reporting Security Issues

> **_Please report security vulnerabilities privately as described below, not through public GitHub issues._**

Use GitHub's **[private vulnerability reporting](https://github.com/ritchiecarroll/go2cs/security/advisories/new)**
for this repository (Security → Report a vulnerability). This keeps the report confidential between you and
the maintainer until a fix is available, and it does not expose an email address to be scraped.

When submitting the report, please include the information listed below to help clarify the nature and scope
of the possible issue:

  * Type of issue (e.g. buffer overflow, unbounded allocation, unsafe pointer handling, etc.)
  * Full paths of source file(s) related to the manifestation of the issue
  * The location of the affected source code (tag/branch/commit or direct URL)
  * Whether the issue is in the converter itself or in the C# it generates
  * The Go input that reproduces it, and the C# go2cs emitted for that input
  * Any special configuration required to reproduce the issue
  * Step-by-step instructions to reproduce the issue
  * Proof-of-concept or exploit code (if possible)
  * Impact of the issue, including how an attacker might exploit it

## Preferred Languages

We prefer all communications to be in English.

## Policy

go2cs follows the principle of
[Coordinated Vulnerability Disclosure](https://en.wikipedia.org/wiki/Coordinated_vulnerability_disclosure).
This is a single-maintainer project, so please allow a reasonable window for a response and a fix before any
public disclosure.
