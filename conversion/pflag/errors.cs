namespace go.github.com.spf13;

using fmt = fmt_package;

partial class pflag_package {

partial struct notExistErrorMessageType /*num:nint*/;

internal static notExistErrorMessageType flagNotExistMessage => /* iota */ 0;
internal static notExistErrorMessageType flagNotDefinedMessage => 1;
internal static notExistErrorMessageType flagNoSuchFlagMessage => 2;
internal static notExistErrorMessageType flagUnknownFlagMessage => 3;
internal static notExistErrorMessageType flagUnknownShorthandFlagMessage => 4;

partial struct NotExistError {
    internal @string name;
    internal @string specifiedShorthands;
    internal notExistErrorMessageType messageType;
}

public static @string Error(this ref NotExistError e) {
    var exprᴛ1 = e.messageType;
    if (exprᴛ1 == flagNotExistMessage) {
        return fmt.Sprintf("flag %q does not exist"u8, e.name);
    }
    if (exprᴛ1 == flagNotDefinedMessage) {
        return fmt.Sprintf("flag accessed but not defined: %s"u8, e.name);
    }
    if (exprᴛ1 == flagNoSuchFlagMessage) {
        return fmt.Sprintf("no such flag -%v"u8, e.name);
    }
    if (exprᴛ1 == flagUnknownFlagMessage) {
        return fmt.Sprintf("unknown flag: --%s"u8, e.name);
    }
    if (exprᴛ1 == flagUnknownShorthandFlagMessage) {
        var c = (rune)e.name[0];
        return fmt.Sprintf("unknown shorthand flag: %q in -%s"u8, c, e.specifiedShorthands);
    }

    throw panic(fmt.Errorf("unknown flagNotExistErrorMessageType: %v"u8, e.messageType));
}

public static @string GetSpecifiedName(this ref NotExistError e) {
    return e.name;
}

public static @string GetSpecifiedShortnames(this ref NotExistError e) {
    return e.specifiedShorthands;
}

partial struct ValueRequiredError {
    internal ж<Flag> flag;
    internal @string specifiedName;
    internal @string specifiedShorthands;
}

public static @string Error(this ref ValueRequiredError e) {
    if (len(e.specifiedShorthands) > 0) {
        var c = (rune)e.specifiedName[0];
        return fmt.Sprintf("flag needs an argument: %q in -%s"u8, c, e.specifiedShorthands);
    }

    return fmt.Sprintf("flag needs an argument: --%s"u8, e.specifiedName);
}

public static ж<Flag> GetFlag(this ref ValueRequiredError e) {
    return e.flag;
}

public static @string GetSpecifiedName(this ref ValueRequiredError e) {
    return e.specifiedName;
}

public static @string GetSpecifiedShortnames(this ref ValueRequiredError e) {
    return e.specifiedShorthands;
}

partial struct InvalidValueError {
    internal ж<Flag> flag;
    internal @string value;
    internal error cause;
}

public static @string Error(this ref InvalidValueError e) {
    var flag = e.flag;
    @string flagName = default!;
    if ((~flag).Shorthand != ""u8 && (~flag).ShorthandDeprecated == ""u8){
        flagName = fmt.Sprintf("-%s, --%s"u8, (~flag).Shorthand, (~flag).Name);
    } else {
        flagName = fmt.Sprintf("--%s"u8, (~flag).Name);
    }
    return fmt.Sprintf("invalid argument %q for %q flag: %v"u8, e.value, flagName, e.cause);
}

public static error Unwrap(this ref InvalidValueError e) {
    return e.cause;
}

public static ж<Flag> GetFlag(this ref InvalidValueError e) {
    return e.flag;
}

public static @string GetValue(this ref InvalidValueError e) {
    return e.value;
}

partial struct InvalidSyntaxError {
    internal @string specifiedFlag;
}

public static @string Error(this ref InvalidSyntaxError e) {
    return fmt.Sprintf("bad flag syntax: %s"u8, e.specifiedFlag);
}

public static @string GetSpecifiedFlag(this ref InvalidSyntaxError e) {
    return e.specifiedFlag;
}

} // end pflag_package
