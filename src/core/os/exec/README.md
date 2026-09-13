# go.os.exec

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-116%2F116_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/os.exec.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/os/exec@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/os/exec) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/os/exec)

Package exec runs external commands. It wraps os.StartProcess to make it easier to remap stdin and stdout, connect I/O with pipes, and do other adjustments.

Unlike the "system" library call from C and other languages, the os/exec package intentionally does not invoke the system shell and does not expand any glob patterns or handle other expansions, pipelines, or redirections typically done by shells. The package behaves more like C's "exec" family of functions. To expand glob patterns, either call the shell directly, taking care to escape any dangerous input, or use the [path/filepath](https://pkg.go.dev/path/filepath@go1.23.12) package's Glob function. To expand environment variables, use package os's ExpandEnv.

Note that the examples in this package assume a Unix system. They may not run on Windows, and they do not run in the Go Playground used by golang.org and godoc.org.

### Executables in the current directory

The functions \[Command] and \[LookPath] look for a program in the directories listed in the current path, following the conventions of the host operating system. Operating systems have for decades included the current directory in this search, sometimes implicitly and sometimes configured explicitly that way by default. Modern practice is that including the current directory is usually unexpected and often leads to security problems.

To avoid those security problems, as of Go 1.19, this package will not resolve a program using an implicit or explicit path entry relative to the current directory. That is, if you run \[LookPath]("go"), it will not successfully return ./go on Unix nor .\\go.exe on Windows, no matter how the path is configured. Instead, if the usual path algorithms would result in that answer, these functions return an error err satisfying [errors.Is](https://pkg.go.dev/errors@go1.23.12#Is)(err, \[ErrDot]).

For example, consider these two program snippets:

	path, err := exec.LookPath("prog")
	if err != nil {
		log.Fatal(err)
	}
	use(path)

and

	cmd := exec.Command("prog")
	if err := cmd.Run(); err != nil {
		log.Fatal(err)
	}

These will not find and run ./prog or .\\prog.exe, no matter how the current path is configured.

Code that always wants to run a program from the current directory can be rewritten to say "./prog" instead of "prog".

Code that insists on including results from relative path entries can instead override the error using an errors.Is check:

	path, err := exec.LookPath("prog")
	if errors.Is(err, exec.ErrDot) {
		err = nil
	}
	if err != nil {
		log.Fatal(err)
	}
	use(path)

and

	cmd := exec.Command("prog")
	if errors.Is(cmd.Err, exec.ErrDot) {
		cmd.Err = nil
	}
	if err := cmd.Run(); err != nil {
		log.Fatal(err)
	}

Setting the environment variable GODEBUG=execerrdot=0 disables generation of ErrDot entirely, temporarily restoring the pre-Go 1.19 behavior for programs that are unable to apply more targeted fixes. A future version of Go may remove support for this variable.

Before adding such overrides, make sure you understand the security implications of doing so. See [https://go.dev/blog/path-security](https://go.dev/blog/path-security) for more information.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
