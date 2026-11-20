namespace go.github.com.dustin.go_humanize;

using strconv = strconv_package;

partial class humanize_package {

public static @string Ordinal(nint x) {
    @string suffix = "th";
    switch (x % 10) {
    case 1: {
        if (x % 100 != 11) {
            suffix = "st";
        }
        break;
    }
    case 2: {
        if (x % 100 != 12) {
            suffix = "nd";
        }
        break;
    }
    case 3: {
        if (x % 100 != 13) {
            suffix = "rd";
        }
        break;
    }}

    return strconv.Itoa(x) + suffix;
}

} // end humanize_package
