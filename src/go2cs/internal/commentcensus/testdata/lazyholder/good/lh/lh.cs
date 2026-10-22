// lh.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
namespace go;

using template = text.template_package;

partial class lh_package {

// funcs maps names to the functions that render them.
[GoLazy] partial class lazyᴛfuncs { internal static template.FuncMap funcs = new text.template_package.FuncMap(new map<@string, any>{
    ["first"u8] = first,
    ["second"u8] = first
}); }

// limit is eager.
internal static nint limit = 3;

internal static nint first() {
    return limit;
}

} // end lh_package
