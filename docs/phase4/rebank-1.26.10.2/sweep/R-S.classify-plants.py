"""Red-test classify-drift.py: a clean control of each ruled shape reads rc 0; each plant reads rc 1 and is named."""
import subprocess, sys, os

HERE = os.path.dirname(os.path.abspath(__file__))
HDR = "diff --git a/{f} b/{f}\n--- a/{f}\n+++ b/{f}\n"

clean = (HDR.format(f="src/core/net/http/serve_test.cs") +
         "@@ -10 +10 @@\n-        });\n+        }, GoStack.Bounded);\n"
         "@@ -20 +20 @@\n-    if (Ꮡ(a, 0) != Ꮡ(b, 1)) {\n+    if (Ꮡelem(a, 0) != Ꮡelem(b, 1)) {\n"
         "@@ -30 +30 @@\n-    var e = p.Ꮡ(i);\n+    var e = p.Ꮡelem<byte>(i);\n"
         "@@ -40 +40 @@\n-[assembly: GoPositionMap(\"aaaa\")]\n+[assembly: GoPositionMap(\"bbbb\")]\n"
         "@@ -50 +50 @@\n-        defer(() => (~resʗ1).Body.Close(), ref ᒐ);\n+        defer(ᴛ0 => ᴛ0.Close(), (~resʗ1).Body, ref ᒐ);\n"
         "@@ -60 +60 @@\n-        goǃ(() => Client(cʗ2, cfgʗ1).Handshake());\n+        goǃ(ᴛ0 => ᴛ0.Handshake(), Client(cʗ2, cfgʗ1));\n")

C_HDR = HDR.format(f="src/core/net/http/serve_test.cs")

plants = {
    "literal": HDR.format(f="src/core/net/http/serve_test.cs") + "@@ -5 +5 @@\n-    x = 1;\n+    x = 2;\n",
    "production": HDR.format(f="src/core/net/http/server.cs") + "@@ -5 +5 @@\n-        });\n+        }, GoStack.Bounded);\n",
    "unequal": HDR.format(f="src/core/net/http/serve_test.cs") + "@@ -5,2 +5 @@\n-        });\n-    x = 1;\n+        }, GoStack.Bounded);\n",
    "elem-plus-other": HDR.format(f="src/core/net/http/serve_test.cs") + "@@ -5 +5 @@\n-    if (Ꮡ(a, 0) != Ꮡ(b, 1)) {\n+    if (Ꮡelem(a, 0) == Ꮡelem(b, 1)) {\n",
    "bounded-elsewhere": HDR.format(f="src/core/net/http/serve_test.cs") + "@@ -5 +5 @@\n-    f(a);\n+    f(a, GoStack.Bounded, b);\n",
    "c-receiver-changed": C_HDR + "@@ -5 +5 @@\n-        defer(() => (~resʗ1).Body.Close(), ref ᒐ);\n+        defer(ᴛ0 => ᴛ0.Close(), (~resʗ2).Body, ref ᒐ);\n",
    "c-method-changed": C_HDR + "@@ -5 +5 @@\n-        defer(() => (~resʗ1).Body.Close(), ref ᒐ);\n+        defer(ᴛ0 => ᴛ0.Flush(), (~resʗ1).Body, ref ᒐ);\n",
    "c-ref-dropped": C_HDR + "@@ -5 +5 @@\n-        defer(() => (~resʗ1).Body.Close(), ref ᒐ);\n+        defer(ᴛ0 => ᴛ0.Close(), (~resʗ1).Body);\n",
    "c-callee-swapped": C_HDR + "@@ -5 +5 @@\n-        defer(() => (~resʗ1).Body.Close(), ref ᒐ);\n+        goǃ(ᴛ0 => ᴛ0.Close(), (~resʗ1).Body, ref ᒐ);\n",
    "c-in-production": HDR.format(f="src/core/net/http/server.cs") + "@@ -5 +5 @@\n-        defer(() => (~resʗ1).Body.Close(), ref ᒐ);\n+        defer(ᴛ0 => ᴛ0.Close(), (~resʗ1).Body, ref ᒐ);\n",
}

def run(name, text):
    p = os.path.join(HERE, f"plant-{name}.diff")
    open(p, "w", encoding="utf-8", newline="\n").write(text)
    r = subprocess.run([sys.executable, "-I", "-X", "utf8", os.path.join(HERE, "R-S.classify-drift.py"), p],
                       capture_output=True, text=True, encoding="utf-8")
    return r.returncode, r.stdout

bad = 0
rc, out = run("clean", clean)
ok = rc == 0 and "a=1 a-other=0 b=2 c=2 m=1  OUTSIDE 0" in out
print(f"control clean: rc={rc} {'OK' if ok else 'WRONG'}")
bad += not ok
for name, text in plants.items():
    rc, out = run(name, text)
    ok = rc == 1 and "OUTSIDE src/core" in out
    print(f"plant {name}: rc={rc} {'OK (refused, named)' if ok else 'WRONG'}")
    bad += not ok
print("PLANTS", "ALL OK" if bad == 0 else f"{bad} WRONG")
sys.exit(1 if bad else 0)
