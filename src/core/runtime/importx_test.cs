// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// See import_test.go. This is the half that lives in the runtime package.
// TODO: Generate this
namespace go;

using static global::go.runtime_package;
using ꓸꓸꓸany = Span<any>;

partial class runtime_internal_test_package {

public partial interface TestingT {
    void Cleanup(Action _);
    void Error(params ꓸꓸꓸany argsʗp);
    void Errorf(@string format, params ꓸꓸꓸany argsʗp);
    void Fail();
    void FailNow();
    bool Failed();
    void Fatal(params ꓸꓸꓸany argsʗp);
    void Fatalf(@string format, params ꓸꓸꓸany argsʗp);
    void Helper();
    void Log(params ꓸꓸꓸany argsʗp);
    void Logf(@string format, params ꓸꓸꓸany argsʗp);
    @string Name();
    void Setenv(@string key, @string value);
    void Skip(params ꓸꓸꓸany argsʗp);
    void SkipNow();
    void Skipf(@string format, params ꓸꓸꓸany argsʗp);
    bool Skipped();
    @string TempDir();
}

public static readonly ж<Funcꓸꓸꓸ<@string, any, @string>> ᏑFmtSprintf = new StaticBox<Funcꓸꓸꓸ<@string, any, @string>>(default(Funcꓸꓸꓸ<@string, any, @string>));
public static ref Funcꓸꓸꓸ<@string, any, @string> FmtSprintf => ref ((StaticBox<Funcꓸꓸꓸ<@string, any, @string>>)ᏑFmtSprintf).ValueSlot;

public static readonly ж<Func<bool>> ᏑTestenvOptimizationOff = new StaticBox<Func<bool>>(default(Func<bool>));
public static ref Func<bool> TestenvOptimizationOff => ref ((StaticBox<Func<bool>>)ᏑTestenvOptimizationOff).ValueSlot;

} // end runtime_internal_test_package
