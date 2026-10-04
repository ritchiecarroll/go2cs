#!/usr/bin/env python3
"""E2: drive netcoredbg over DAP against a #line-mapped converted sample.

usage: dap.py <netcoredbg> <dotnet> <app.dll> <main.go> <script.json>
script: {"breakpoints": [lines], "steps": [...]} where each step is one of
  "continue", "next", "stepIn", "locals", "stack", {"evaluate": expr}
Prints one JSON line per event of interest.
"""
import json, os, queue, subprocess, sys, threading, time

ndbg, dotnet, dll, gofile, script_path = sys.argv[1:6]
script = json.load(open(script_path))
proc = subprocess.Popen([ndbg, "--interpreter=vscode"], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
inbox = queue.Queue()
seq = 0


def reader():
    f = proc.stdout
    while True:
        header = b""
        while not header.endswith(b"\r\n\r\n"):
            c = f.read(1)
            if not c:
                return
            header += c
        length = int([h for h in header.decode().split("\r\n") if h.lower().startswith("content-length")][0].split(":")[1])
        inbox.put(json.loads(f.read(length)))


threading.Thread(target=reader, daemon=True).start()


def send(command, arguments=None):
    global seq
    seq += 1
    body = json.dumps({"seq": seq, "type": "request", "command": command, "arguments": arguments or {}}).encode()
    proc.stdin.write(b"Content-Length: %d\r\n\r\n" % len(body) + body)
    proc.stdin.flush()
    return seq


def wait(pred, timeout=60):
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            m = inbox.get(timeout=1)
        except queue.Empty:
            continue
        if m.get("type") == "event" and m.get("event") == "output":
            continue
        if m.get("type") == "event" and m.get("event") == "breakpoint":
            b = m["body"]["breakpoint"]
            print(json.dumps({"kind": "breakpointEvent", "reason": m["body"].get("reason"), "id": b.get("id"),
                              "verified": b.get("verified"), "line": b.get("line"), "message": b.get("message")}), flush=True)
            continue
        if pred(m):
            return m
    raise TimeoutError("no matching message")


def request(command, arguments=None, timeout=60):
    s = send(command, arguments)
    return wait(lambda m: m.get("type") == "response" and m.get("request_seq") == s, timeout)


def out(kind, **kw):
    print(json.dumps({"kind": kind, **kw}), flush=True)


request("initialize", {"clientID": "e2", "adapterID": "coreclr", "linesStartAt1": True, "columnsStartAt1": True, "pathFormat": "path"})
send("launch", {"name": "e2", "type": "coreclr", "request": "launch", "program": dotnet, "args": [dll],
                "cwd": os.path.dirname(dll), "stopAtEntry": False, "justMyCode": True, "requireExactSource": True})
wait(lambda m: m.get("event") == "initialized")
r = request("setBreakpoints", {"source": {"path": gofile}, "breakpoints": [{"line": l} for l in script["breakpoints"]]})
out("setBreakpoints", success=r.get("success"),
    breakpoints=[{"id": b.get("id"), "verified": b.get("verified"), "line": b.get("line")} for b in r["body"]["breakpoints"]])
request("configurationDone")

thread = None


def stopped(timeout=60):
    global thread
    m = wait(lambda m: m.get("event") in ("stopped", "terminated", "exited"), timeout)
    if m.get("event") != "stopped":
        out("ended", event=m.get("event"))
        return False
    thread = m["body"].get("threadId")
    st = request("stackTrace", {"threadId": thread, "levels": 6})["body"]["stackFrames"]
    out("stopped", reason=m["body"].get("reason"),
        frames=[{"name": f["name"], "file": os.path.basename((f.get("source") or {}).get("path") or "") or None, "line": f["line"]} for f in st], path0=(st[0].get("source") or {}).get("path"))
    return True


if not stopped():
    sys.exit(0)

for step in script["steps"]:
    if step in ("continue", "next", "stepIn"):
        request(step, {"threadId": thread})
        if not stopped():
            break
    elif step == "locals":
        st = request("stackTrace", {"threadId": thread, "levels": 1})["body"]["stackFrames"]
        scopes = request("scopes", {"frameId": st[0]["id"]})["body"]["scopes"]
        vals = []
        for sc in scopes:
            for v in request("variables", {"variablesReference": sc["variablesReference"]})["body"]["variables"]:
                vals.append({"name": v["name"], "value": v["value"][:80], "type": v.get("type", "")[:60]})
        out("locals", values=vals)
    elif isinstance(step, dict) and "evaluate" in step:
        st = request("stackTrace", {"threadId": thread, "levels": 1})["body"]["stackFrames"]
        r = request("evaluate", {"expression": step["evaluate"], "frameId": st[0]["id"], "context": "watch"})
        out("evaluate", expression=step["evaluate"], success=r.get("success"),
            result=(r.get("body") or {}).get("result", r.get("message", ""))[:120])

try:
    request("disconnect", {"terminateDebuggee": True}, timeout=10)
except Exception:
    pass
proc.kill()
