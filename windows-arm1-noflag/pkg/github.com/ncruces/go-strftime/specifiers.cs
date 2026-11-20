namespace go.github.com.ncruces.go_strftime;

using strings = strings_package;

partial class strftime_package {

internal static @string goLayout(byte spec, byte flag, bool parsing) {
    switch (spec) {
    default: {
        return "";
    }
    case (byte)'B': {
        return "January";
    }
    case (byte)'b' or (byte)'h': {
        return "Jan";
    }
    case (byte)'m': {
        if (flag == '-' || parsing) {
            return "1";
        }
        return "01";
    }
    case (byte)'A': {
        return "Monday";
    }
    case (byte)'a': {
        return "Mon";
    }
    case (byte)'e': {
        return "_2";
    }
    case (byte)'d': {
        if (flag == '-' || parsing) {
            return "2";
        }
        return "02";
    }
    case (byte)'j': {
        if (flag == '-') {
            if (parsing) {
                return "__2";
            }
            return "";
        }
        return "002";
    }
    case (byte)'I': {
        if (flag == '-' || parsing) {
            return "3";
        }
        return "03";
    }
    case (byte)'H': {
        if (flag == '-' && !parsing) {
            return "";
        }
        return "15";
    }
    case (byte)'M': {
        if (flag == '-' || parsing) {
            return "4";
        }
        return "04";
    }
    case (byte)'S': {
        if (flag == '-' || parsing) {
            return "5";
        }
        return "05";
    }
    case (byte)'y': {
        return "06";
    }
    case (byte)'Y': {
        return "2006";
    }
    case (byte)'p': {
        return "PM";
    }
    case (byte)'P': {
        return "pm";
    }
    case (byte)'Z': {
        return "MST";
    }
    case (byte)'z': {
        if (flag == ':') {
            if (parsing) {
                return "Z07:00";
            }
            return "-07:00";
        }
        if (parsing) {
            return "Z0700";
        }
        return "-0700";
    }
    case (byte)'+': {
        if (parsing) {
            return "Mon Jan _2 15:4:5 MST 2006";
        }
        return "Mon Jan _2 15:04:05 MST 2006";
    }
    case (byte)'c': {
        if (parsing) {
            return "Mon Jan _2 15:4:5 2006";
        }
        return "Mon Jan _2 15:04:05 2006";
    }
    case (byte)'v': {
        return "_2-Jan-2006";
    }
    case (byte)'F': {
        if (parsing) {
            return "2006-1-2";
        }
        return "2006-01-02";
    }
    case (byte)'D' or (byte)'x': {
        if (parsing) {
            return "1/2/06";
        }
        return "01/02/06";
    }
    case (byte)'r': {
        if (parsing) {
            return "3:4:5 PM";
        }
        return "03:04:05 PM";
    }
    case (byte)'T' or (byte)'X': {
        if (parsing) {
            return "15:4:5";
        }
        return "15:04:05";
    }
    case (byte)'R': {
        if (parsing) {
            return "15:4";
        }
        return "15:04";
    }
    case (byte)'%': {
        return "%";
    }
    case (byte)'t': {
        return "\t";
    }
    case (byte)'n': {
        return "\n";
    }}

}

internal static @string uts35Pattern(byte spec, byte flag) {
    switch (spec) {
    default: {
        return "";
    }
    case (byte)'B': {
        return "MMMM";
    }
    case (byte)'b' or (byte)'h': {
        return "MMM";
    }
    case (byte)'m': {
        if (flag == '-') {
            return "M";
        }
        return "MM";
    }
    case (byte)'A': {
        return "EEEE";
    }
    case (byte)'a': {
        return "E";
    }
    case (byte)'d': {
        if (flag == '-') {
            return "d";
        }
        return "dd";
    }
    case (byte)'j': {
        if (flag == '-') {
            return "D";
        }
        return "DDD";
    }
    case (byte)'I': {
        if (flag == '-') {
            return "h";
        }
        return "hh";
    }
    case (byte)'H': {
        if (flag == '-') {
            return "H";
        }
        return "HH";
    }
    case (byte)'M': {
        if (flag == '-') {
            return "m";
        }
        return "mm";
    }
    case (byte)'S': {
        if (flag == '-') {
            return "s";
        }
        return "ss";
    }
    case (byte)'y': {
        return "yy";
    }
    case (byte)'Y': {
        return "yyyy";
    }
    case (byte)'g': {
        return "YY";
    }
    case (byte)'G': {
        return "YYYY";
    }
    case (byte)'V': {
        if (flag == '-') {
            return "w";
        }
        return "ww";
    }
    case (byte)'p': {
        return "a";
    }
    case (byte)'Z': {
        return "zzz";
    }
    case (byte)'z': {
        if (flag == ':') {
            return "xxx";
        }
        return "xx";
    }
    case (byte)'L': {
        return "SSS";
    }
    case (byte)'f': {
        return "SSSSSS";
    }
    case (byte)'N': {
        return "SSSSSSSSS";
    }
    case (byte)'+': {
        return "E MMM d HH:mm:ss zzz yyyy";
    }
    case (byte)'c': {
        return "E MMM d HH:mm:ss yyyy";
    }
    case (byte)'v': {
        return "d-MMM-yyyy";
    }
    case (byte)'F': {
        return "yyyy-MM-dd";
    }
    case (byte)'D' or (byte)'x': {
        return "MM/dd/yy";
    }
    case (byte)'r': {
        return "hh:mm:ss a";
    }
    case (byte)'T' or (byte)'X': {
        return "HH:mm:ss";
    }
    case (byte)'R': {
        return "HH:mm";
    }
    case (byte)'%': {
        return "%";
    }
    case (byte)'t': {
        return "\t";
    }
    case (byte)'n': {
        return "\n";
    }}

}

internal static bool okModifier(byte mod, byte spec) {
    if (mod == 'E') {
        return strings.Contains("cCxXyY", (@string)spec);
    }
    if (mod == 'O') {
        return strings.Contains("deHImMSuUVwWy", (@string)spec);
    }
    return false;
}

} // end strftime_package
