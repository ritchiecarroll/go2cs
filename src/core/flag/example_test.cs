// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// These examples demonstrate more intricate uses of the flag package.
namespace go;

using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using strings = strings_package;
using time = time_package;
using static go.flag_internal_test_package;

partial class flag_test_package {

// Example 1: A single string flag called "species" with default value "gopher".
internal static ж<@string> species = flag.String("species"u8, "gopher"u8, "the species we are studying"u8);

// Example 2: Two flags sharing a variable, so we can have a shorthand.
// The order of initialization is undefined, so make sure both use the
// same default value. They must be set up with an init function.
internal static ж<@string> ᏑgopherType = new StandardBox<@string>(default(@string));
internal static ref @string gopherType => ref ᏑgopherType.Value;

[GoInit] internal static void init() {
    @string defaultGopher = "pocket"u8;
    @string usage = "the variety of gopher"u8;
    flag.StringVar(ᏑgopherType, "gopher_type"u8, defaultGopher, usage);
    flag.StringVar(ᏑgopherType, "g"u8, defaultGopher, usage + " (shorthand)");
}

partial struct interval /*[]global::go.time_package.Duration*/;

// String is the method to format the flag's value, part of the flag.Value interface.
// The String method's output will be used in diagnostics.
internal static @string ΔString(this ref interval i) {
    return fmt.Sprint(i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string intervalFlagAlreadySetˢ = "interval flag already set"u8;

// Set is the method to set the flag value, part of the flag.Value interface.
// Set's argument is a string to be parsed to set the flag.
// It's a comma-separated list, so we split it.
internal static error ΔSet(this ref interval i, @string value) {
    // If we wanted to allow the flag to be set multiple times,
    // accumulating values, we would delete this if statement.
    // That would permit usages such as
    //	-deltaT 10s -deltaT 15s
    // and other combinations.
    if (len(i) > 0) {
        return errors.New(intervalFlagAlreadySetˢ);
    }
    foreach (var (_, dt) in strings.Split(value, ","u8)) {
        var (duration, err) = time.ParseDuration(dt);
        if (err != default!) {
            return err;
        }
        i = append(i, duration);
    }
    return default!;
}

// Define a flag to accumulate durations. Because it has a special type,
// we need to use the Var function and therefore create the flag during
// init.
internal static ж<interval> ᏑintervalFlag = new StandardBox<interval>(default(interval));
internal static ref interval intervalFlag => ref ᏑintervalFlag.ValueSlot;

[GoInit] internal static void initΔ1() {
    // Tie the command-line flag to the intervalFlag variable and
    // set a usage message.
    flag.Var(new flag_test_package.intervalжValue(ᏑintervalFlag), "deltaT"u8, "comma-separated list of intervals to use between events"u8);
}

public static void Example() {
}

// All the interesting pieces are with the variables declared above, but
// to enable the flag package to see the flags defined there, one must
// execute, typically at the start of main (not init!):
//	flag.Parse()
// We don't call it here because this code is a function called "Example"
// that is part of the testing suite for the package, which has already
// parsed the flags. When viewed at pkg.go.dev, however, the function is
// renamed to "main" and it could be run as a standalone example.

} // end flag_test_package
