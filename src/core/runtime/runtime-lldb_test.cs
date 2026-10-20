// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using fs = global::go.io.fs_package;
using global::go.os;
using path;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static @string lldbPath;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lldbˢ = "lldb"u8;
internal static readonly @string usrBinPython27ˢ = "/usr/bin/python2.7"u8;
internal static readonly @string importSysSysPathAppendˢ = "import sys;sys.path.append(sys.argv[1]);import lldb; print('go lldb python support')"u8;
internal static readonly @string usrSbinDevToolsSecurityˢ = "/usr/sbin/DevToolsSecurity"u8;
internal static readonly @string statusˢ = "-status"u8;
internal static readonly @string enabledˢ = "enabled"u8;
internal static readonly @string usrBinGroupsˢ = "/usr/bin/groups"u8;
internal static readonly @string developerˢ = "_developer"u8;
internal static readonly object notInDeveloperGroupˢ = (@string)"Not in _developer group"u8;

internal static void checkLldbPython(ж<testing.T> Ꮡt) {
    var cmd = exec.Command(lldbˢ, "-P"u8);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Skipf("skipping due to issue running lldb: %v\n%s"u8, err, @out);
    }
    lldbPath = strings.TrimSpace((@string)@out);

    cmd = exec.Command(usrBinPython27ˢ, "-c"u8, importSysSysPathAppendˢ, lldbPath);
    (@out, err) = cmd.CombinedOutput();

    if (err != default!) {
        Ꮡt.Skipf("skipping due to issue running python: %v\n%s"u8, err, @out);
    }
    if (((sstring)@out) != "go lldb python support\n"u8) {
        Ꮡt.Skipf("skipping due to lack of python lldb support: %s"u8, @out);
    }

    if (Δruntime.GOOS == "darwin"u8) {
        // Try to see if we have debugging permissions.
        cmd = exec.Command(usrSbinDevToolsSecurityˢ, statusˢ);
        (@out, err) = cmd.CombinedOutput();
        if (err != default!){
            Ꮡt.Skipf("DevToolsSecurity failed: %v"u8, err);
        } else 
        if (!strings.Contains((@string)@out, enabledˢ)) {
            Ꮡt.Skip((@string)@out);
        }
        cmd = exec.Command(usrBinGroupsˢ);
        (@out, err) = cmd.CombinedOutput();
        if (err != default!){
            Ꮡt.Skipf("groups failed: %v"u8, err);
        } else 
        if (!strings.Contains((@string)@out, developerˢ)) {
            Ꮡt.Skip(notInDeveloperGroupˢ);
        }
    }
}

internal static readonly @string lldbHelloSource = """

package main
import "fmt"
func main() {
	mapvar := make(map[string]string,5)
	mapvar["abc"] = "def"
	mapvar["ghi"] = "jkl"
	intvar := 42
	ptrvar := &intvar
	fmt.Println("hi") // line 10
	_ = ptrvar
}

"""u8.Raw();

internal static readonly @string lldbScriptSource = """

import sys
sys.path.append(sys.argv[1])
import lldb
import os

TIMEOUT_SECS = 5

debugger = lldb.SBDebugger.Create()
debugger.SetAsync(True)
target = debugger.CreateTargetWithFileAndArch("a.exe", None)
if target:
  print "Created target"
  main_bp = target.BreakpointCreateByLocation("main.go", 10)
  if main_bp:
    print "Created breakpoint"
  process = target.LaunchSimple(None, None, os.getcwd())
  if process:
    print "Process launched"
    listener = debugger.GetListener()
    process.broadcaster.AddListener(listener, lldb.SBProcess.eBroadcastBitStateChanged)
    while True:
      event = lldb.SBEvent()
      if listener.WaitForEvent(TIMEOUT_SECS, event):
        if lldb.SBProcess.GetRestartedFromEvent(event):
          continue
        state = process.GetState()
        if state in [lldb.eStateUnloaded, lldb.eStateLaunching, lldb.eStateRunning]:
          continue
      else:
        print "Timeout launching"
      break
    if state == lldb.eStateStopped:
      for t in process.threads:
        if t.GetStopReason() == lldb.eStopReasonBreakpoint:
          print "Hit breakpoint"
          frame = t.GetFrameAtIndex(0)
          if frame:
            if frame.line_entry:
              print "Stopped at %s:%d" % (frame.line_entry.file.basename, frame.line_entry.line)
            if frame.function:
              print "Stopped in %s" % (frame.function.name,)
            var = frame.FindVariable('intvar')
            if var:
              print "intvar = %s" % (var.GetValue(),)
            else:
              print "no intvar"
    else:
      print "Process state", state
    process.Destroy()
else:
  print "Failed to create target a.exe"

lldb.SBDebugger.Destroy(debugger)
sys.exit()

"""u8.Raw();

internal static readonly @string expectedLldbOutput = """
Created target
Created breakpoint
Process launched
Hit breakpoint
Stopped at main.go:10
Stopped in main.main
intvar = 42

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goModˢ = "go.mod"u8;
internal static readonly @string ldflagsCompressdwarfˢ = "-ldflags=-compressdwarf=false"u8;
internal static readonly @string scriptPyˢ = "script.py"u8;
internal static readonly @string timeoutLaunchingˢ = "Timeout launching"u8;

public static void TestLldbPython(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new runtime_test_package.testing_TжTB(Ꮡt));
    testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 31188);

    checkLldbPython(Ꮡt);

    @string dir = Ꮡt.TempDir();

    @string src = filepath.Join(dir, mainGoˢ);
    var err = Δos.WriteFile(src, slice<byte>(lldbHelloSource), 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create src file: %v"u8, err);
    }

    @string mod = filepath.Join(dir, goModˢ);
    err = Δos.WriteFile(mod, slice<byte>("module lldbtest"u8), 420);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create mod file: %v"u8, err);
    }

    // As of 2018-07-17, lldb doesn't support compressed DWARF, so
    // disable it for this test.
    var cmd = exec.Command(testenv.GoToolPath(new runtime_test_package.testing_TжTB(Ꮡt)), buildˢ, gcflagsAllNLˢ, ldflagsCompressdwarfˢ, "-o", aExeˢ);
    cmd.Value.Dir = dir;
    cmd.Value.Env = append(Δos.Environ(), "GOPATH="u8); // issue 31100
    (var @out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("building source %v\n%s"u8, err, @out);
    }

    src = filepath.Join(dir, scriptPyˢ);
    err = Δos.WriteFile(src, slice<byte>(lldbScriptSource), 493);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create script: %v"u8, err);
    }

    cmd = exec.Command(usrBinPython27ˢ, scriptPyˢ, lldbPath);
    cmd.Value.Dir = dir;
    var (got, _) = cmd.CombinedOutput();

    if (((sstring)got) != expectedLldbOutput) {
        if (strings.Contains((@string)got, timeoutLaunchingˢ)) {
            Ꮡt.Skip(timeoutLaunchingˢ);
        }
        Ꮡt.Fatalf("Unexpected lldb output:\n%s"u8, got);
    }
}

} // end runtime_test_package
