// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || dragonfly || freebsd || illumos || linux || netbsd || windows
namespace go;

using time = time_package;
using static go.net_package;

partial class net_internal_test_package {

internal static slice<global::go.net_package.KeepAliveConfig> testConfigs = [
    new(
        Enable: true,
        Idle: (time.Duration)(5000000000L),
        Interval: (time.Duration)(3000000000L),
        Count: 10
    ),
    new(
        Enable: true,
        Idle: 0,
        Interval: 0,
        Count: 0
    ),
    new(
        Enable: true,
        Idle: -1,
        Interval: -1,
        Count: -1
    ),
    new(
        Enable: true,
        Idle: -1,
        Interval: (time.Duration)(3000000000L),
        Count: 10
    ),
    new(
        Enable: true,
        Idle: (time.Duration)(5000000000L),
        Interval: -1,
        Count: 10
    ),
    new(
        Enable: true,
        Idle: (time.Duration)(5000000000L),
        Interval: (time.Duration)(3000000000L),
        Count: -1
    ),
    new(
        Enable: true,
        Idle: -1,
        Interval: -1,
        Count: 10
    ),
    new(
        Enable: true,
        Idle: -1,
        Interval: (time.Duration)(3000000000L),
        Count: -1
    ),
    new(
        Enable: true,
        Idle: (time.Duration)(5000000000L),
        Interval: -1,
        Count: -1
    ),
    new(
        Enable: true,
        Idle: 0,
        Interval: (time.Duration)(3000000000L),
        Count: 10
    ),
    new(
        Enable: true,
        Idle: (time.Duration)(5000000000L),
        Interval: 0,
        Count: 10
    ),
    new(
        Enable: true,
        Idle: (time.Duration)(5000000000L),
        Interval: (time.Duration)(3000000000L),
        Count: 0
    ),
    new(
        Enable: true,
        Idle: 0,
        Interval: 0,
        Count: 10
    ),
    new(
        Enable: true,
        Idle: 0,
        Interval: (time.Duration)(3000000000L),
        Count: 0
    ),
    new(
        Enable: true,
        Idle: (time.Duration)(5000000000L),
        Interval: 0,
        Count: 0
    )
];

} // end net_internal_test_package
