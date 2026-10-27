// Copyright 2013-2023 The Cobra Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace go.github.com.spf13;

using fmt = fmt_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using template = text.template_package;
using time = time_package;
using unicode = unicode_package;
using text;
using ꓸꓸꓸAction = Span<Action>;

partial class cobra_package {

[GoLazy] partial class lazyᴛtemplateFuncs { internal static template.FuncMap templateFuncs = new template.FuncMap(new map<@string, any>{
    ["trim"] = strings.TrimSpace,
    ["trimRightSpace"] = trimRightSpace,
    ["trimTrailingWhitespaces"] = trimRightSpace,
    ["appendIfNotPresent"] = appendIfNotPresent,
    ["rpad"] = rpad,
    ["gt"] = Gt,
    ["eq"] = Eq
}); }

internal static slice<Action> initializers;

internal static slice<Action> finalizers;

internal const bool defaultPrefixMatching = false;
internal const bool defaultCommandSorting = true;
internal const bool defaultCaseInsensitive = false;
internal const bool defaultTraverseRunHooks = false;

public static bool EnablePrefixMatching = defaultPrefixMatching;

public static bool EnableCommandSorting = defaultCommandSorting;

public static bool EnableCaseInsensitive = defaultCaseInsensitive;

public static bool EnableTraverseRunHooks = defaultTraverseRunHooks;

public static @string MousetrapHelpText = """
This is a command line tool.

You need to open cmd.exe and run it from there.

"""u8.Raw();

public static time.Duration MousetrapDisplayDuration = (time.Duration)(5000000000L);

public static void AddTemplateFunc(@string name, any tmplFunc) {
    templateFuncs[name] = tmplFunc;
}

public static void AddTemplateFuncs(template.FuncMap tmplFuncs) {
    foreach (var (k, v) in tmplFuncs) {
        templateFuncs[k] = v;
    }
}

public static void OnInitialize(params ꓸꓸꓸAction yʗp) {
    var y = yʗp.sslice();

    initializers = appendꓸꓸꓸ(initializers, y);
}

public static void OnFinalize(params ꓸꓸꓸAction yʗp) {
    var y = yʗp.sslice();

    finalizers = appendꓸꓸꓸ(finalizers, y);
}

public static bool Gt(any a, any b) {
    int64 left = default!;
    int64 right = default!;
    var av = reflect.ValueOf(a);

    var exprᴛ1 = av.Kind();
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔSlice) {
        left = (int64)av.Len();
    }
    else if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        left = av.Int();
    }
    else if (exprᴛ1 == reflect.ΔString) {
        (left, _) = strconv.ParseInt(av.String(), 10, 64);
    }

    var bv = reflect.ValueOf(b);

    var exprᴛ2 = bv.Kind();
    if (exprᴛ2 == reflect.Array || exprᴛ2 == reflect.Chan || exprᴛ2 == reflect.Map || exprᴛ2 == reflect.ΔSlice) {
        right = (int64)bv.Len();
    }
    else if (exprᴛ2 == reflect.ΔInt || exprᴛ2 == reflect.Int8 || exprᴛ2 == reflect.Int16 || exprᴛ2 == reflect.Int32 || exprᴛ2 == reflect.Int64) {
        right = bv.Int();
    }
    else if (exprᴛ2 == reflect.ΔString) {
        (right, _) = strconv.ParseInt(bv.String(), 10, 64);
    }

    return left > right;
}

public static bool Eq(any a, any b) {
    var av = reflect.ValueOf(a);
    var bv = reflect.ValueOf(b);

    var exprᴛ1 = av.Kind();
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔSlice) {
        throw panic("Eq called on unsupported type");
    }
    else if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return av.Int() == bv.Int();
    }
    if (exprᴛ1 == reflect.ΔString) {
        return av.String() == bv.String();
    }

    return false;
}

internal static @string trimRightSpace(@string s) {
    return strings.TrimRightFunc(s, unicode.IsSpace);
}

internal static @string appendIfNotPresent(@string s, @string stringToAppend) {
    if (strings.Contains(s, stringToAppend)) {
        return s;
    }
    return s + " "u8 + stringToAppend;
}

internal static @string rpad(@string s, nint padding) {
    @string formattedString = fmt.Sprintf("%%-%ds"u8, padding);
    return fmt.Sprintf(formattedString, s);
}

internal static ж<tmplFunc> tmpl(@string text) {
    return Ꮡ(new tmplFunc(
        tmpl: text,
        fn: (io.Writer w, any data) => {
            var t = template.New("top");
            t.Funcs(templateFuncs);
            var (ᴛ1, ᴛ2) = t.Parse(text);
            template.Must(ᴛ1, ᴛ2);
            return t.Execute(w, data);
        }
    ));
}

internal static nint ld(@string s, @string t, bool ignoreCase) {
    if (ignoreCase) {
        s = strings.ToLower(s);
        t = strings.ToLower(t);
    }
    var d = new slice<slice<nint>>(len(s) + 1);
    foreach (var i in range(d)) {
        d[i] = new slice<nint>(len(t) + 1);
        d[i][0] = i;
    }
    foreach (var j in range(d[0])) {
        d[0][j] = j;
    }
    for (nint j = 1; j <= len(t); j++) {
        for (nint i = 1; i <= len(s); i++) {
            if (s[i - 1] == t[j - 1]){
                d[i][j] = d[i - 1][j - 1];
            } else {
                nint min = d[i - 1][j];
                if (d[i][j - 1] < min) {
                    min = d[i][j - 1];
                }
                if (d[i - 1][j - 1] < min) {
                    min = d[i - 1][j - 1];
                }
                d[i][j] = min + 1;
            }
        }
    }
    return d[len(s)][len(t)];
}

internal static bool stringInSlice(@string a, slice<@string> list) {
    foreach (var (_, b) in list) {
        if (b == a) {
            return true;
        }
    }
    return false;
}

public static void CheckErr(any msg) {
    if (msg != default!) {
        fmt.Fprintln(new os.FileжWriter(os.Stderr), "Error:", msg);
        os.Exit(1);
    }
}

public static void WriteStringAndCheck(io.StringWriter b, @string s) {
    var (_, err) = b.WriteString(s);
    CheckErr(err);
}

} // end cobra_package
