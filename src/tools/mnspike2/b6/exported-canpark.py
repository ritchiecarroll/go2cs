"""exported-canpark.py CENSUS_JSON

B6 of the M:N spike, phase 2: how many of the can-park functions are consumer-facing?

CENSUS_JSON is phase 1's can-park census with names, for example
src/tools/mnspike/results/P1-windows-upper-long.json at ref claude/g-mn-spike-p4.

A name counts as exported and public when its package path has no `internal` or `vendor` segment, the function
or method name starts with an upper-case letter, and, for a method, so does the receiver type's name. Under
visible async (R3) each of these takes a task-shaped signature.

The PROBES list is the census's own control: names that must be in the set and names that must not.
"""
import collections
import json
import re
import sys

MUST = ["fmt.Println", "io.Copy", "os.ReadFile", "(*os.File).Write", "(*sync.WaitGroup).Wait"]
MUST_NOT = ["strings.Map", "errors.Is", "(*bytes.Buffer).Write", "(*strings.Builder).WriteString"]


def main() -> int:
    census = json.load(open(sys.argv[1], encoding="utf-8"))
    names = census["canParkNames"]
    present = set(names)
    hidden = exported = funcs = methods = unparsed = 0
    packages = collections.Counter()

    for name in names:
        method = re.match(r"^\(\*?(.+)\.([^.()]+)\)\.([^.]+)$", name)

        if method:
            package, receiver, leaf = method.group(1), method.group(2), method.group(3)
        else:
            plain = re.match(r"^(.+)\.([^.]+)$", name)

            if not plain:
                unparsed += 1
                continue

            package, receiver, leaf = plain.group(1), None, plain.group(2)

        segments = package.split("/")

        if "internal" in segments or "vendor" in segments:
            hidden += 1
            continue

        if not leaf[:1].isupper() or (receiver is not None and not receiver[:1].isupper()):
            continue

        exported += 1
        packages[package] += 1

        if receiver is None:
            funcs += 1
        else:
            methods += 1

    print(f"READING can-park names {len(names)} of {census['functions']} ({census['goos']}); internal or vendor {hidden}; unparsed {unparsed}")
    print(f"READING exported and public {exported}: functions {funcs}, methods {methods}, in {len(packages)} packages")
    print("READING top packages " + ", ".join(f"{p} {c}" for p, c in packages.most_common(8)))

    failed = [n for n in MUST if n not in present] + [n for n in MUST_NOT if n in present]
    print("CONTROL " + ("ok" if not failed and unparsed == 0 else "FAILED " + ", ".join(failed)))
    return 0 if not failed and unparsed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
