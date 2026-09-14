import io, json

P = r"C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c\docs\phase4\TRACKER-100-percent.md"
with io.open(P, encoding='utf-8', newline='') as f:
    t = f.read()

print("CRLF count:", t.count('\r\n'), " bare LF:", t.count('\n') - t.count('\r\n'))

lines = t.split('\n')
for n in (17, 21, 30, 31, 32, 37, 51):
    s = lines[n-1]
    print("\n=== line %d (len %d) ===" % (n, len(s)))

# candidate anchors -> print with repr for exactness
import re
def show(sub):
    c = t.count(sub)
    print("count=%d :: %s" % (c, json.dumps(sub, ensure_ascii=False)))

show("raw 201/216 = 93.1%")
show("reflect 115 \u2192 48 real mismatches")
show("**Rows remaining (implementable)** | **7** (os/user BANKED")
show("\u2014 reflect (48: the re-mapped tail above), os (682/685")
show("runtime/pprof, runtime/trace, testing (Option 1 ruled, sequenced) |")
show("runtime (-tests at **1** \u2014 the lone CS8175")
show("at that landing the 45 re-measures at master.")
show("The remaining distance is 17 named rows, none a mystery.")
show("both green on the i7 at the merge result).")
print()
# print the tail of line 37 and line 21 fragments to build safe anchors
l21 = lines[20]
print("L21 tail:", json.dumps(l21[-260:], ensure_ascii=False))
l17 = lines[16]
i = l17.find("raw 201/216")
print("L17 ctx:", json.dumps(l17[i-90:i+120], ensure_ascii=False))
j = l17.find("reflect 115")
print("L17 ctx2:", json.dumps(l17[j-60:j+90], ensure_ascii=False))
l37 = lines[36]
print("L37 tail:", json.dumps(l37[-300:], ensure_ascii=False))
l30 = lines[29]
k = l30.find("at that landing")
print("L30 ctx:", json.dumps(l30[k-140:k+80], ensure_ascii=False))
l21b = l21.find("runtime (-tests at")
print("L21 ctx:", json.dumps(l21[l21b:l21b+190], ensure_ascii=False))
