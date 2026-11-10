// Program.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The break-floor fixture: a loop over a slice inside hashset, the member B3 changed in go.* 1.26.10.3. A hashset
// revision built below that release restores beside it and throws MissingMethodException at NewHashSet.
using fmt = go.fmt_package;
using hashset = go.github.com.ritchiecarroll.hashset_package;

var s = hashset.NewHashSet(new nint[] { 1, 2, 3 }.slice());
fmt.Println("BREAK-FLOOR:", s.Contains(2), len(s.Keys()));
