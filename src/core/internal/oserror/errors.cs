// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package oserror defines errors values used in the os package.
//
// These types are defined here to permit the syscall package to reference them.
namespace go.@internal;

using errors = errors_package;

partial class oserror_package {

public static error ErrInvalid = errors.New("invalid argument");
public static error ErrPermission = errors.New("permission denied");
public static error ErrExist = errors.New("file already exists");
public static error ErrNotExist = errors.New("file does not exist");
public static error ErrClosed = errors.New("file already closed");

} // end oserror_package
