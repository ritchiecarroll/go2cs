// oneline.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
namespace go.example.com;

using context = context_package;

partial class oneline_package {

// timeoutError reports a timeout.
partial struct timeoutError {
    internal @string err;
}

internal static @string Error(this ref timeoutError e) => e.err;
internal static bool Timeout(this ref timeoutError e) => true;

internal static bool Is(this ref timeoutError e, error err) => AreEqual(err, context_package.DeadlineExceeded);
internal static void marker(this ref timeoutError _) { }

// wrap returns a timeout error.
internal static error wrap(@string s) {
    return new timeoutErrorжerror(Ꮡ(new timeoutError(s)));
}

} // end oneline_package
