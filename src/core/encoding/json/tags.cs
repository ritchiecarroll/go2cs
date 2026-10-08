// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using strings = strings_package;

partial class json_package {

partial struct tagOptions /*@string*/;

// parseTag splits a struct field's json tag into its name and
// comma-separated options.
internal static (@string, tagOptions) parseTag(@string tag) {
    (tag, var opt, _) = strings.Cut(tag, ","u8);
    return (tag, ((tagOptions)opt));
}

// Contains reports whether a comma-separated list of options
// contains a particular substr flag. substr must be surrounded by a
// string boundary or commas.
internal static bool Contains(this tagOptions o, @string optionName) {
    if (len(o) == 0) {
        return false;
    }
    @string s = ((@string)o);
    while (s != ""u8) {
        @string name = default!;
        (name, s, _) = strings.Cut(s, ","u8);
        if (name == optionName) {
            return true;
        }
    }
    return false;
}

} // end json_package
