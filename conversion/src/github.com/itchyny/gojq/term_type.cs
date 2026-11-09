namespace go.github.com.itchyny;

partial class gojq_package {

partial struct TermType /*num:nint*/;

public static TermType TermTypeIdentity => /* iota + 1 */ 1;
public static TermType TermTypeRecurse => 2;
public static TermType TermTypeNull => 3;
public static TermType TermTypeTrue => 4;
public static TermType TermTypeFalse => 5;
public static TermType TermTypeIndex => 6;
public static TermType TermTypeFunc => 7;
public static TermType TermTypeObject => 8;
public static TermType TermTypeArray => 9;
public static TermType TermTypeNumber => 10;
public static TermType TermTypeUnary => 11;
public static TermType TermTypeFormat => 12;
public static TermType TermTypeString => 13;
public static TermType TermTypeIf => 14;
public static TermType TermTypeTry => 15;
public static TermType TermTypeReduce => 16;
public static TermType TermTypeForeach => 17;
public static TermType TermTypeLabel => 18;
public static TermType TermTypeBreak => 19;
public static TermType TermTypeQuery => 20;

public static @string /*str*/ GoString(this TermType termType) {
    @string str = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            str = "gojq."u8 + str;
        }, ref ᒐ);
        switch (termType) {
        case var ᴛ when ᴛ == TermTypeIdentity: {
            str = "TermTypeIdentity"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeRecurse: {
            str = "TermTypeRecurse"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeNull: {
            str = "TermTypeNull"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeTrue: {
            str = "TermTypeTrue"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeFalse: {
            str = "TermTypeFalse"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeIndex: {
            str = "TermTypeIndex"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeFunc: {
            str = "TermTypeFunc"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeObject: {
            str = "TermTypeObject"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeArray: {
            str = "TermTypeArray"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeNumber: {
            str = "TermTypeNumber"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeUnary: {
            str = "TermTypeUnary"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeFormat: {
            str = "TermTypeFormat"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeString: {
            str = "TermTypeString"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeIf: {
            str = "TermTypeIf"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeTry: {
            str = "TermTypeTry"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeReduce: {
            str = "TermTypeReduce"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeForeach: {
            str = "TermTypeForeach"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeLabel: {
            str = "TermTypeLabel"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeBreak: {
            str = "TermTypeBreak"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == TermTypeQuery: {
            str = "TermTypeQuery"; goto ᒐdone;
        }
        default: {
            throw panic(termType);
            break;
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return str;
}

} // end gojq_package
