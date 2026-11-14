#!/usr/bin/env python3
"""Writes a Go program shaped like the case P4 measured: one package with N node structs that each embed a
base holding M pointer-receiver methods, and one small named type with a String method that fmt prints once."""
import sys
n, m, out = int(sys.argv[1]), int(sys.argv[2]), sys.argv[3]
w = []
w.append("package main\n\nimport \"fmt\"\n")
w.append("type Kind int16\n\nfunc (k Kind) String() string {\n\tif k == 7 {\n\t\treturn \"KindSeven\"\n\t}\n\treturn \"KindOther\"\n}\n")
w.append("type Node struct {\n\tKind Kind\n\tPos  int\n}\n")
for i in range(m):
    w.append(f"func (n *Node) M{i:03d}() int {{ return n.Pos + {i} }}\n")
w.append("\ntype NodeBase struct {\n\tNode\n}\n")
for i in range(n):
    w.append(f"type T{i:03d} struct {{\n\tNodeBase\n\tF{i:03d} int\n}}\n")
w.append("\nfunc main() {\n\tvar total int\n")
for i in range(n):
    w.append(f"\t{{\n\t\tv := &T{i:03d}{{}}\n\t\tv.Pos = {i}\n\t\ttotal += v.M000()\n\t}}\n")
w.append("\tfmt.Println(total)\n\tfmt.Println(Kind(7))\n}\n")
open(out, "w").write("".join(w))
