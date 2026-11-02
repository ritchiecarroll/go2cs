// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflectlite = @internal.reflectlite_package;
using @internal;
using abi = @internal.abi_package;

partial class errors_package {

internal partial interface Unwrap_type /*dyn*/ {
    error Unwrap();
}

// Unwrap returns the result of calling the Unwrap method on err, if err's
// type contains an Unwrap method returning error.
// Otherwise, Unwrap returns nil.
//
// Unwrap only calls a method of the form "Unwrap() error".
// In particular Unwrap does not unwrap errors returned by [Join].
public static error Unwrap(error err) {
    var (u, ok) = err._<Unwrap_type>(ᐧ);
    if (!ok) {
        return default;
    }
    return u.Unwrap();
}

// Is reports whether any error in err's tree matches target.
// The target must be comparable.
//
// The tree consists of err itself, followed by the errors obtained by repeatedly
// calling its Unwrap() error or Unwrap() []error method. When err wraps multiple
// errors, Is examines err followed by a depth-first traversal of its children.
//
// An error is considered to match a target if it is equal to that target or if
// it implements a method Is(error) bool such that Is(target) returns true.
//
// An error type might provide an Is method so it can be treated as equivalent
// to an existing error. For example, if MyError defines
//
//	func (m MyError) Is(target error) bool { return target == fs.ErrExist }
//
// then Is(MyError{}, fs.ErrExist) returns true. See [syscall.Errno.Is] for
// an example in the standard library. An Is method should only shallowly
// compare err and the target and not call [Unwrap] on either.
public static bool Is(error err, error target) {
    if (err == nil || target == nil) {
        return AreEqual(err, target);
    }

    var isComparable = reflectlite.TypeOf(target).Comparable();
    return @is(err, target, isComparable);
}

internal partial interface is_type /*dyn*/ {
    bool Is(error _);
}

internal partial interface is_typeᴛ1 /*dyn*/ {
    slice<error> Unwrap();
}

internal static bool @is(error err, error target, bool targetComparable) {
    for (;;) {
        if (targetComparable && AreEqual(err, target)) {
            return true;
        }
        {
            var (x, ok) = err._<is_type>(ᐧ); if (ok && x.Is(target)) {
                return true;
            }
        }
        switch (err.type()) {
        case {} Δx when Δx._<Unwrap_type>(out var x): {
            err = x.Unwrap();
            if (err == nil) {
                return false;
            }
            break;
        }
        case {} Δx when Δx._<is_typeᴛ1>(out var x): {
            foreach (var (_, errΔ1) in x.Unwrap()) {
                if (@is(errΔ1, target, targetComparable)) {
                    return true;
                }
            }
            return false;
        }
        default: {
            var x = err;
            return false;
        }}
    }
}

// As finds the first error in err's tree that matches target, and if one is found, sets
// target to that error value and returns true. Otherwise, it returns false.
//
// For most uses, prefer [AsType]. As is equivalent to [AsType] but sets its target
// argument rather than returning the matching error and doesn't require its target
// argument to implement error.
//
// The tree consists of err itself, followed by the errors obtained by repeatedly
// calling its Unwrap() error or Unwrap() []error method. When err wraps multiple
// errors, As examines err followed by a depth-first traversal of its children.
//
// An error matches target if the error's concrete value is assignable to the value
// pointed to by target, or if the error has a method As(any) bool such that
// As(target) returns true. In the latter case, the As method is responsible for
// setting target.
//
// An error type might provide an As method so it can be treated as if it were a
// different error type.
//
// As panics if target is not a non-nil pointer to either a type that implements
// error, or to any interface type.
public static bool As(error err, any target) {
    if (err == nil) {
        return false;
    }
    if (target == nil) {
        throw panic("errors: target cannot be nil");
    }
    var val = reflectlite.ValueOf(target);
    var typ = val.Type();
    if (typ.Kind() != reflectlite.Ptr || val.IsNil()) {
        throw panic("errors: target must be a non-nil pointer");
    }
    var targetType = typ.Elem();
    if (targetType.Kind() != reflectlite.Interface && !targetType.Implements(errorType)) {
        throw panic("errors: *target must be interface or implement error");
    }
    return @as(err, target, val, targetType);
}

internal partial interface as_type /*dyn*/ {
    bool As(any _);
}

internal static bool @as(error err, any target, reflectlite.Value targetVal, reflectliteꓸType targetType) {
    for (;;) {
        if (reflectlite.TypeOf(err).AssignableTo(targetType)) {
            targetVal.Elem().Set(reflectlite.ValueOf(err));
            return true;
        }
        {
            var (x, ok) = err._<as_type>(ᐧ); if (ok && x.As(target)) {
                return true;
            }
        }
        switch (err.type()) {
        case {} Δx when Δx._<Unwrap_type>(out var x): {
            err = x.Unwrap();
            if (err == nil) {
                return false;
            }
            break;
        }
        case {} Δx when Δx._<is_typeᴛ1>(out var x): {
            foreach (var (_, errΔ1) in x.Unwrap()) {
                if (errΔ1 == nil) {
                    continue;
                }
                if (@as(errΔ1, target, targetVal, targetType)) {
                    return true;
                }
            }
            return false;
        }
        default: {
            var x = err;
            return false;
        }}
    }
}

internal static reflectliteꓸType errorType = reflectlite.TypeOf((ж<error>)nil).Elem();

// AsType finds the first error in err's tree that matches the type E, and
// if one is found, returns that error value and true. Otherwise, it
// returns the zero value of E and false.
//
// The tree consists of err itself, followed by the errors obtained by
// repeatedly calling its Unwrap() error or Unwrap() []error method. When
// err wraps multiple errors, AsType examines err followed by a
// depth-first traversal of its children.
//
// An error err matches the type E if the type assertion err.(E) holds,
// or if the error has a method As(any) bool such that err.As(target)
// returns true when target is a non-nil *E. In the latter case, the As
// method is responsible for setting target.
public static (E, bool) AsType<E>(error err) {
    if (err == nil) {
        E zero = GoZero<E>();
        return (zero, false);
    }
    ref var pe = ref heap<ж<E>>(out var Ꮡpe); // lazily initialized
    return asType(err, ref pe);
}

internal static (E, bool) asType<E>(error err, ref ж<E> ppe) {
    for (;;) {
        {
            var (e, ok) = err._<E>(ᐧ); if (ok) {
                return (e, true);
            }
        }
        {
            var (x, ok) = err._<as_type>(ᐧ); if (ok) {
                if (ppe == nil) {
                    ppe = @new<E>();
                }
                if (x.As(ppe.OrTypedNil())) {
                    return (ppe.ValueSlot, true);
                }
            }
        }
        switch (err.type()) {
        case {} Δx when Δx._<Unwrap_type>(out var x): {
            err = x.Unwrap();
            if (err == nil) {
                return (default, default);
            }
            break;
        }
        case {} Δx when Δx._<is_typeᴛ1>(out var x): {
            foreach (var (_, errΔ1) in x.Unwrap()) {
                if (errΔ1 == nil) {
                    continue;
                }
                {
                    var (xΔ1, ok) = asType(errΔ1, ref ppe); if (ok) {
                        return (xΔ1, true);
                    }
                }
            }
            return (default, default);
        }
        default: {
            var x = err;
            return (default, default);
        }}
    }
}

} // end errors_package
