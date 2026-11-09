namespace go.github.com.itchyny;

partial class gojq_package {

partial struct code {
    internal any v;
    internal opcode op;
}

partial struct opcode /*num:nint*/;

internal static opcode opnop => /* iota */ 0;
internal static opcode oppush => 1;
internal static opcode oppop => 2;
internal static opcode opdup => 3;
internal static opcode opconst => 4;
internal static opcode opload => 5;
internal static opcode opstore => 6;
internal static opcode opobject => 7;
internal static opcode opappend => 8;
internal static opcode opfork => 9;
internal static opcode opforktrybegin => 10;
internal static opcode opforktryend => 11;
internal static opcode opforkalt => 12;
internal static opcode opforklabel => 13;
internal static opcode opbacktrack => 14;
internal static opcode opjump => 15;
internal static opcode opjumpifnot => 16;
internal static opcode opindex => 17;
internal static opcode opindexarray => 18;
internal static opcode opcall => 19;
internal static opcode opcallrec => 20;
internal static opcode oppushpc => 21;
internal static opcode opcallpc => 22;
internal static opcode opscope => 23;
internal static opcode opret => 24;
internal static opcode opiter => 25;
internal static opcode opexpbegin => 26;
internal static opcode opexpend => 27;
internal static opcode oppathbegin => 28;
internal static opcode oppathend => 29;

internal static @string String(this opcode op) {
    switch (op) {
    case var ᴛ when ᴛ == opnop: {
        return "nop";
    }
    case var ᴛ when ᴛ == oppush: {
        return "push";
    }
    case var ᴛ when ᴛ == oppop: {
        return "pop";
    }
    case var ᴛ when ᴛ == opdup: {
        return "dup";
    }
    case var ᴛ when ᴛ == opconst: {
        return "const";
    }
    case var ᴛ when ᴛ == opload: {
        return "load";
    }
    case var ᴛ when ᴛ == opstore: {
        return "store";
    }
    case var ᴛ when ᴛ == opobject: {
        return "object";
    }
    case var ᴛ when ᴛ == opappend: {
        return "append";
    }
    case var ᴛ when ᴛ == opfork: {
        return "fork";
    }
    case var ᴛ when ᴛ == opforktrybegin: {
        return "forktrybegin";
    }
    case var ᴛ when ᴛ == opforktryend: {
        return "forktryend";
    }
    case var ᴛ when ᴛ == opforkalt: {
        return "forkalt";
    }
    case var ᴛ when ᴛ == opforklabel: {
        return "forklabel";
    }
    case var ᴛ when ᴛ == opbacktrack: {
        return "backtrack";
    }
    case var ᴛ when ᴛ == opjump: {
        return "jump";
    }
    case var ᴛ when ᴛ == opjumpifnot: {
        return "jumpifnot";
    }
    case var ᴛ when ᴛ == opindex: {
        return "index";
    }
    case var ᴛ when ᴛ == opindexarray: {
        return "indexarray";
    }
    case var ᴛ when ᴛ == opcall: {
        return "call";
    }
    case var ᴛ when ᴛ == opcallrec: {
        return "callrec";
    }
    case var ᴛ when ᴛ == oppushpc: {
        return "pushpc";
    }
    case var ᴛ when ᴛ == opcallpc: {
        return "callpc";
    }
    case var ᴛ when ᴛ == opscope: {
        return "scope";
    }
    case var ᴛ when ᴛ == opret: {
        return "ret";
    }
    case var ᴛ when ᴛ == opiter: {
        return "iter";
    }
    case var ᴛ when ᴛ == opexpbegin: {
        return "expbegin";
    }
    case var ᴛ when ᴛ == opexpend: {
        return "expend";
    }
    case var ᴛ when ᴛ == oppathbegin: {
        return "pathbegin";
    }
    case var ᴛ when ᴛ == oppathend: {
        return "pathend";
    }
    default: {
        throw panic(op);
        break;
    }}

}

} // end gojq_package
