// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.@internal;
using go.os;
using path;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xGoˢ = "x.go"u8;
internal static readonly @string xExeˢ = "x.exe"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string toolˢ = "tool"u8;

internal partial struct TestLinkerGC_tests /*dyn*/ {
    internal @string name;
    internal @string program;
    internal slice<@string> want;
    internal slice<@string> bad;
}

// Tests that the linker is able to remove references to the Client or Server if unused.
public static void TestLinkerGC(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    @string goBin = testenv.GoToolPath(new tls_test_package.testing_TжTB(Ꮡt));
    testenv.MustHaveGoBuild(new tls_test_package.testing_TжTB(Ꮡt));
    var tests = new TestLinkerGC_tests[]{
        new(
            name: "empty_import"u8,
            program: """
package main
import _ "crypto/tls"
func main() {}

"""u8,
            bad: new @string[]{
                "tls.(*Conn)"u8,
                "type:crypto/tls.clientHandshakeState"u8,
                "type:crypto/tls.serverHandshakeState"u8
            }.slice()
        ),
        new(
            name: "client_and_server"u8,
            program: """
package main
import "crypto/tls"
func main() {
  tls.Dial("", "", nil)
  tls.Server(nil, nil)
}

"""u8,
            want: new @string[]{
                "crypto/tls.(*Conn).clientHandshake"u8,
                "crypto/tls.(*Conn).serverHandshake"u8
            }.slice()
        ),
        new(
            name: "only_client"u8,
            program: """
package main
import "crypto/tls"
func main() { tls.Dial("", "", nil) }

"""u8,
            want: new @string[]{
                "crypto/tls.(*Conn).clientHandshake"u8
            }.slice(),
            bad: new @string[]{
                "crypto/tls.(*Conn).serverHandshake"u8
            }.slice()
        )
    }.slice();
    // TODO: add only_server like func main() { tls.Server(nil, nil) }
    // That currently brings in the client via Conn.handleRenegotiation.
    @string tmpDir = Ꮡt.TempDir();
    @string goFile = filepath.Join(tmpDir, xGoˢ);
    @string exeFile = filepath.Join(tmpDir, xExeˢ);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestLinkerGC_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            {
                var errΔ1 = os.WriteFile(goFile, slice<byte>(ttʗ1.program), 420); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            os.Remove(exeFile);
            var cmd = exec.Command(goBin, buildˢ, "-o", xExeˢ, xGoˢ);
            cmd.Value.Dir = tmpDir;
            {
                var (@out, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != default!) {
                    tΔ1.Fatalf("compile: %v, %s"u8, errΔ2, @out);
                }
            }
            cmd = exec.Command(goBin, toolˢ, "nm", xExeˢ);
            cmd.Value.Dir = tmpDir;
            var (nm, err) = cmd.CombinedOutput();
            if (err != default!) {
                tΔ1.Fatalf("nm: %v, %s"u8, err, nm);
            }
            foreach (var (_, sym) in ttʗ1.want) {
                if (!bytes.Contains(nm, slice<byte>(sym))) {
                    tΔ1.Errorf("expected symbol %q not found"u8, sym);
                }
            }
            foreach (var (_, sym) in ttʗ1.bad) {
                if (bytes.Contains(nm, slice<byte>(sym))) {
                    tΔ1.Errorf("unexpected symbol %q found"u8, sym);
                }
            }
        });
    }
}

} // end tls_internal_test_package
