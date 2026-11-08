// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using filepath = path.filepath_package;
using testing = testing_package;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.@internal;
using go.os;
using path;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xGoˢ => ref xGoˢᵇ;
private static readonly @string xGoˢᵇ = "x.go"u8;
internal static ref readonly @string xExeˢ => ref xExeˢᵇ;
private static readonly @string xExeˢᵇ = "x.exe"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;

internal partial struct TestLinkerGC_tests /*dyn*/ {
    internal @string name;
    internal @string program;
    internal slice<@string> want;
    internal slice<@string> bad;
}

// Tests that the linker is able to remove references to the Client or Server if unused.
public static void TestLinkerGC(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    t.Parallel();

    var tests = slice<TestLinkerGC_tests>([
        new(
            name: "empty_import",
            program: """
package main
import _ "crypto/tls"
func main() {}

"""u8.Raw(),
            bad: [
                "tls.(*Conn)",
                "type:crypto/tls.clientHandshakeState",
                "type:crypto/tls.serverHandshakeState"
            ]
        ),
        new(
            name: "client_and_server",
            program: """
package main
import "crypto/tls"
func main() {
  tls.Dial("", "", nil)
  tls.Server(nil, nil)
}

"""u8.Raw(),
            want: [
                "crypto/tls.(*Conn).clientHandshake",
                "crypto/tls.(*Conn).serverHandshake"
            ]
        ),
        new(
            name: "only_client",
            program: """
package main
import "crypto/tls"
func main() { tls.Dial("", "", nil) }

"""u8.Raw(),
            want: [
                "crypto/tls.(*Conn).clientHandshake"
            ],
            bad: [
                "crypto/tls.(*Conn).serverHandshake"
            ]
        )
        // TODO: add only_server like func main() { tls.Server(nil, nil) }
        // That currently brings in the client via Conn.handleRenegotiation.
    ]);
    @string tmpDir = t.TempDir();
    @string goFile = filepath.Join(tmpDir, xGoˢ);
    @string exeFile = filepath.Join(tmpDir, xExeˢ);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestLinkerGC_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var errΔ1 = os.WriteFile(goFile, slice<byte>(ttʗ1.program), 420); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            os.Remove(exeFile);
            var cmd = testenv.Command(new tls_test_package.testing_TжTB(tΔ1), testenv.GoToolPath(new tls_test_package.testing_TжTB(tΔ1)), buildˢ, "-o", xExeˢ, xGoˢ);
            cmd.Dir = tmpDir;
            var (@out, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != nil) {
                tΔ1.Fatalf("compile: %v\n%s", errΔ2, @out);
            }

            cmd = testenv.Command(new tls_test_package.testing_TжTB(tΔ1), testenv.GoToolPath(new tls_test_package.testing_TжTB(tΔ1)), toolˢ, "nm", xExeˢ);
            cmd.Dir = tmpDir;
            var (nm, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
            if (err != nil) {
                tΔ1.Fatalf("nm: %v\n%s", err, nm);
            }
            foreach (var (_, sym) in range(ttʗ1.want)) {
                if (!bytes.Contains(nm, slice<byte>(sym))) {
                    tΔ1.Errorf("expected symbol %q not found", sym);
                }
            }
            foreach (var (_, sym) in range(ttʗ1.bad)) {
                if (bytes.Contains(nm, slice<byte>(sym))) {
                    tΔ1.Errorf("unexpected symbol %q found", sym);
                }
            }
        });
    }
}

} // end tls_internal_test_package
