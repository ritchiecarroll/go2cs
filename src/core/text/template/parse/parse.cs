// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package parse builds parse trees for templates as defined by text/template
// and html/template. Clients should use those packages to construct templates
// rather than this one, which provides shared internal data structures not
// intended for general use.
namespace go.text.template;

using bytes = bytes_package;
using fmt = fmt_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using ꓸꓸꓸany = Span<any>;

partial class parse_package {

// Tree is the representation of a single parsed template.
[GoType] partial struct Tree {
    public @string Name;   // name of the template represented by the tree.
    public @string ParseName;   // name of the top-level template during parsing, for error messages.
    public ж<ListNode> Root; // top-level root of the tree.
    public Mode Mode;      // parsing mode.
    internal @string text;   // text parsed to create the template (or its parent)
    // Parsing only; cleared after parse.
    internal slice<map<@string, any>> funcs;
    internal ж<lexer> lex;
    internal array<item> token = new(3); // three-token lookahead for parser.
    internal nint peekCount;
    internal slice<@string> vars; // variables defined at the moment.
    internal map<@string, ж<Tree>> treeSet;
    internal nint actionLine; // line of left delim starting action
    internal nint rangeDepth;
}

[GoType("num:nuint")] partial struct Mode;

public static Mode ParseComments => /* 1 << iota */ 1;       // parse comments and add them to AST
public static Mode SkipFuncCheck => 2;       // do not check that functions are defined

// Copy returns a copy of the [Tree]. Any parsing state is discarded.
public static ж<Tree> Copy(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt == nil) {
        return default!;
    }
    return Ꮡ(new Tree(
        Name: t.Name,
        ParseName: t.ParseName,
        Root: t.Root.CopyList(),
        text: t.text
    ));
}

// Parse returns a map from template name to [Tree], created by parsing the
// templates described in the argument string. The top-level template will be
// given the specified name. If an error is encountered, parsing stops and an
// empty map is returned with the error.
public static (map<@string, ж<Tree>>, error) Parse(@string name, @string text, @string leftDelim, @string rightDelim, params Span<map<@string, any>> funcsʗp) {
    var funcs = funcsʗp.sslice();

    var treeSet = new map<@string, ж<Tree>>();
    var t = New(name);
    t.Value.text = text;
    var (_, err) = t.Parse(text, leftDelim, rightDelim, treeSet, funcs.ꓸꓸꓸ);
    return (treeSet, err);
}

// next returns the next token.
[GoRecv] internal static item next(this ref Tree t) {
    if (t.peekCount > 0){
        t.peekCount--;
    } else {
        t.token[0] = t.lex.nextItem();
    }
    return t.token[t.peekCount];
}

// backup backs the input stream up one token.
[GoRecv] internal static void backup(this ref Tree t) {
    t.peekCount++;
}

// backup2 backs the input stream up two tokens.
// The zeroth token is already there.
[GoRecv] internal static void backup2(this ref Tree t, item t1) {
    t.token[1] = t1;
    t.peekCount = 2;
}

// backup3 backs the input stream up three tokens
// The zeroth token is already there.
[GoRecv] internal static void backup3(this ref Tree t, item t2, item t1) {
    // Reverse order: we're pushing back.
    t.token[1] = t1;
    t.token[2] = t2;
    t.peekCount = 3;
}

// peek returns but does not consume the next token.
[GoRecv] internal static item peek(this ref Tree t) {
    if (t.peekCount > 0) {
        return t.token[t.peekCount - 1];
    }
    t.peekCount = 1;
    t.token[0] = t.lex.nextItem();
    return t.token[0];
}

// nextNonSpace returns the next non-space token.
[GoRecv] internal static item /*token*/ nextNonSpace(this ref Tree t) {
    item token = default!;

    while (ᐧ) {
        token = t.next();
        if (token.typ != itemSpace) {
            break;
        }
    }
    return token;
}

// peekNonSpace returns but does not consume the next non-space token.
[GoRecv] internal static item peekNonSpace(this ref Tree t) {
    var token = t.nextNonSpace();
    t.backup();
    return token;
}

// Parsing.

// New allocates a new parse tree with the given name.
public static ж<Tree> New(@string name, params Span<map<@string, any>> funcsʗp) {
    var funcs = funcsʗp.slice();

    return Ꮡ(new Tree(
        Name: name,
        funcs: funcs
    ));
}

// ErrorContext returns a textual representation of the location of the node in the input text.
// The receiver is only used when the node does not have a pointer to the tree inside,
// which can occur in old code.
public static (@string location, @string context) ErrorContext(this ж<Tree> Ꮡt, Node n) {
    @string context = default!;

    nint pos = (nint)n.Position();
    var tree = n.tree();
    if (tree == nil) {
        tree = Ꮡt;
    }
    @string text = (~tree).text.slice(0, pos);
    nint byteNum = strings.LastIndex(text, "\n"u8);
    if (byteNum == -1){
        byteNum = pos; // On first line.
    } else {
        byteNum++; // After the newline.
        byteNum = pos - byteNum;
    }
    nint lineNum = 1 + strings.Count(text, "\n"u8);
    context = n.String();
    return (fmt.Sprintf("%s:%d:%d"u8, (~tree).ParseName, lineNum, byteNum), context);
}

// errorf formats the error and terminates processing.
[GoRecv] internal static void errorf(this ref Tree t, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Root = default!;
    format = fmt.Sprintf("template: %s:%d: %s"u8, t.ParseName, t.token[0].line, format);
    throw panic(fmt.Errorf(format, args.ꓸꓸꓸ));
}

// error terminates processing.
[GoRecv] internal static void error(this ref Tree t, error err) {
    t.errorf("%s"u8, err);
}

// expect consumes the next token and guarantees it has the required type.
[GoRecv] internal static item expect(this ref Tree t, itemType expected, @string context) {
    var token = t.nextNonSpace();
    if (token.typ != expected) {
        t.unexpected(token, context);
    }
    return token;
}

// expectOneOf consumes the next token and guarantees it has one of the required types.
[GoRecv] internal static item expectOneOf(this ref Tree t, itemType expected1, itemType expected2, @string context) {
    var token = t.nextNonSpace();
    if (token.typ != expected1 && token.typ != expected2) {
        t.unexpected(token, context);
    }
    return token;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string actionˢ = " action"u8;

// unexpected complains about the token and terminates processing.
[GoRecv] internal static void unexpected(this ref Tree t, item token, @string context) {
    if (token.typ == itemError) {
        @string extra = ""u8;
        if (t.actionLine != 0 && t.actionLine != token.line) {
            extra = fmt.Sprintf(" in action started at %s:%d"u8, t.ParseName, t.actionLine);
            if (strings.HasSuffix(token.val, actionˢ)) {
                extra = extra[(int)(len(" in action"))..]; // avoid "action in action"
            }
        }
        t.errorf("%s%s"u8, token, extra);
    }
    t.errorf("unexpected %s in %s"u8, token, context);
}

// recover is the handler that turns panics into returns from the top level of Parse.
internal static void recover(this ж<Tree> Ꮡt, ж<error> Ꮡerrp) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();
        ref var errp = ref Ꮡerrp.DerefOrNull();

        var e = builtin.recover();
        if (e != default!) {
            {
                var (_, ok) = e._<runtimeꓸError>(ᐧ); if (ok) {
                    throw panic(e);
                }
            }
            if (Ꮡt != nil) {
                t.stopParse();
            }
            errp = e._<error>();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string breakˢ2 = "break"u8;
internal static readonly @string continueˢ2 = "continue"u8;

// startParse initializes the parser, using the lexer.
[GoRecv] internal static void startParse(this ref Tree t, slice<map<@string, any>> funcs, ж<lexer> Ꮡlex, map<@string, ж<Tree>> treeSet) {
    ref var lex = ref Ꮡlex.DerefOrNull();

    t.Root = default!;
    t.lex = Ꮡlex;
    t.vars = new @string[]{"$"u8}.slice();
    t.funcs = funcs;
    t.treeSet = treeSet;
    lex.options = new lexOptions(
        emitComment: (Mode)(t.Mode & ParseComments) != 0,
        breakOK: !t.hasFunction(breakˢ2),
        continueOK: !t.hasFunction(continueˢ2)
    );
}

// stopParse terminates parsing.
[GoRecv] internal static void stopParse(this ref Tree t) {
    t.lex = default!;
    t.vars = default!;
    t.funcs = default!;
    t.treeSet = default!;
}

// Parse parses the template definition string to construct a representation of
// the template for execution. If either action delimiter string is empty, the
// default ("{{" or "}}") is used. Embedded template definitions are added to
// the treeSet map.
public static (ж<Tree> tree, error err) Parse(this ж<Tree> Ꮡt, @string text, @string leftDelim, @string rightDelim, map<@string, ж<Tree>> treeSet, params Span<map<@string, any>> funcsʗp) {
    ж<Tree> tree = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        var funcs = funcsʗp.slice();

        ref var t = ref Ꮡt.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;
        defer(Ꮡt.recover, Ꮡerr, ref ᒐ);
        t.ParseName = t.Name;
        var lexer = lex(t.Name, text, leftDelim, rightDelim);
        t.startParse(funcs, lexer, treeSet);
        t.text = text;
        Ꮡt.parse();
        Ꮡt.add();
        t.stopParse();
        (tree, err) = (Ꮡt, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (tree, Ꮡerr.ValueSlot);
}

// add adds tree to t.treeSet.
internal static void add(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tree = t.treeSet[t.Name];
    if (tree == nil || IsEmptyTree(new ListNodeжNode((~tree).Root))) {
        t.treeSet[t.Name] = Ꮡt;
        return;
    }
    if (!IsEmptyTree(new ListNodeжNode(t.Root))) {
        t.errorf("template: multiple definition of template %q"u8, t.Name);
    }
}

// IsEmptyTree reports whether this tree (node) is empty of everything but space or comments.
public static bool IsEmptyTree(Node n) {
    switch (n.type()) {
    case null: {
        return true;
    }
    case ж<ActionNode> nΔ1: {
        break;
    }
    case ж<CommentNode> nΔ1: {
        return true;
    }
    case ж<IfNode> nΔ1: {
        break;
    }
    case ж<ListNode> nΔ1: {
        foreach (var (_, node) in (~nΔ1).Nodes) {
            if (!IsEmptyTree(node)) {
                return false;
            }
        }
        return true;
    }
    case ж<RangeNode> nΔ1: {
        break;
    }
    case ж<TemplateNode> nΔ1: {
        break;
    }
    case ж<TextNode> nΔ1: {
        return len(bytes.TrimSpace((~nΔ1).Text)) == 0;
    }
    case ж<WithNode> nΔ1: {
        break;
    }
    default: {
        var nΔ1 = n;
        throw panic("unknown node: " + nΔ1.String());
        break;
    }}
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string definitionˢ = "definition"u8;

// parse is the top-level parser for a template, essentially the same
// as itemList except it also parses {{define}} actions.
// It runs to EOF.
internal static void parse(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.Root = Ꮡt.newList(t.peek().pos);
    while (t.peek().typ != itemEOF) {
        if (t.peek().typ == itemLeftDelim) {
            var delim = t.next();
            if (t.nextNonSpace().typ == itemDefine) {
                var newT = New(definitionˢ); // name will be updated once we know it.
                newT.Value.text = t.text;
                newT.Value.Mode = t.Mode;
                newT.Value.ParseName = t.ParseName;
                newT.startParse(t.funcs, t.lex, t.treeSet);
                newT.parseDefinition();
                continue;
            }
            t.backup2(delim);
        }
        {
            var n = Ꮡt.textOrAction();
            var exprᴛ1 = n.Type();
            if (exprᴛ1 == nodeEnd || exprᴛ1 == nodeElse) {
                t.errorf("unexpected %s"u8, n);
            }
            else { /* default: */
                t.Root.append(n);
            }
        }

    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contextᶜ = "define clause"u8;

// parseDefinition parses a {{define}} ...  {{end}} template definition and
// installs the definition in t.treeSet. The "define" keyword has already
// been scanned.
internal static void parseDefinition(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string context = contextᶜ;
    var name = t.expectOneOf(itemString, itemRawString, context);
    error err = default!;
    (t.Name, err) = strconv.Unquote(name.val);
    if (err != default!) {
        t.error(err);
    }
    t.expect(itemRightDelim, context);
    Node end = default!;
    (t.Root, end) = Ꮡt.itemList();
    if (end.Type() != nodeEnd) {
        t.errorf("unexpected %s in %s"u8, end, context);
    }
    Ꮡt.add();
    t.stopParse();
}

// itemList:
//
//	textOrAction*
//
// Terminates at {{end}} or {{else}}, returned separately.
internal static (ж<ListNode> list, Node next) itemList(this ж<Tree> Ꮡt) {
    ж<ListNode> list = default!;
    Node next = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    list = Ꮡt.newList(t.peekNonSpace().pos);
    while (t.peekNonSpace().typ != itemEOF) {
        var n = Ꮡt.textOrAction();
        var exprᴛ1 = n.Type();
        if (exprᴛ1 == nodeEnd || exprᴛ1 == nodeElse) {
            return (list, n);
        }

        list.append(n);
    }
    t.errorf("unexpected EOF"u8);
    return (list, next);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inputˢ = "input"u8;

// textOrAction:
//
//	text | comment | action
internal static Node textOrAction(this ж<Tree> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        {
            var token = t.nextNonSpace();
            var exprᴛ1 = token.typ;
            if (exprᴛ1 == itemText) {
                return new TextNodeжNode(Ꮡt.newText(token.pos, token.val));
            }
            if (exprᴛ1 == itemLeftDelim) {
                t.actionLine = token.line;
                ᒐd1 = true;
                return Ꮡt.action();
            }
            if (exprᴛ1 == itemComment) {
                return new CommentNodeжNode(Ꮡt.newComment(token.pos, token.val));
            }
            { /* default: */
                t.unexpected(token, inputˢ);
            }
        }

        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().clearActionLine(); ᒐ.Run(); }
}

[GoRecv] internal static void clearActionLine(this ref Tree t) {
    t.actionLine = 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string commandˢ = "command"u8;

// Action:
//
//	control
//	command ("|" command)*
//
// Left delim is past. Now get actions.
// First word could be a keyword such as range.
internal static Node /*n*/ action(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var tokenΔ1 = t.nextNonSpace();
        var exprᴛ1 = tokenΔ1.typ;
        if (exprᴛ1 == itemBlock) {
            return Ꮡt.blockControl();
        }
        if (exprᴛ1 == itemBreak) {
            return Ꮡt.breakControl(tokenΔ1.pos, tokenΔ1.line);
        }
        if (exprᴛ1 == itemContinue) {
            return Ꮡt.continueControl(tokenΔ1.pos, tokenΔ1.line);
        }
        if (exprᴛ1 == itemElse) {
            return Ꮡt.elseControl();
        }
        if (exprᴛ1 == itemEnd) {
            return Ꮡt.endControl();
        }
        if (exprᴛ1 == itemIf) {
            return Ꮡt.ifControl();
        }
        if (exprᴛ1 == itemRange) {
            return Ꮡt.rangeControl();
        }
        if (exprᴛ1 == itemTemplate) {
            return Ꮡt.templateControl();
        }
        if (exprᴛ1 == itemWith) {
            return Ꮡt.withControl();
        }
    }

    t.backup();
    var token = t.peek();
    // Do not pop variables; they persist until "end".
    return new ActionNodeжNode(Ꮡt.newAction(token.pos, token.line, Ꮡt.pipeline(commandˢ, itemRightDelim)));
}

// Break:
//
//	{{break}}
//
// Break keyword is past.
internal static Node breakControl(this ж<Tree> Ꮡt, Pos pos, nint line) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var token = t.nextNonSpace(); if (token.typ != itemRightDelim) {
            t.unexpected(token, breakˢ);
        }
    }
    if (t.rangeDepth == 0) {
        t.errorf("{{break}} outside {{range}}"u8);
    }
    return new BreakNodeжNode(Ꮡt.newBreak(pos, line));
}

// Continue:
//
//	{{continue}}
//
// Continue keyword is past.
internal static Node continueControl(this ж<Tree> Ꮡt, Pos pos, nint line) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var token = t.nextNonSpace(); if (token.typ != itemRightDelim) {
            t.unexpected(token, continueˢ);
        }
    }
    if (t.rangeDepth == 0) {
        t.errorf("{{continue}} outside {{range}}"u8);
    }
    return new ContinueNodeжNode(Ꮡt.newContinue(pos, line));
}

// Pipeline:
//
//	declarations? command ('|' command)*
internal static ж<PipeNode> /*pipe*/ pipeline(this ж<Tree> Ꮡt, @string context, itemType end) {
    ж<PipeNode> pipe = default!;

    ref var t = ref Ꮡt.DerefOrNull();
    var token = t.peekNonSpace();
    pipe = Ꮡt.newPipeline(token.pos, token.line, default!);
    // Are there declarations or assignments?
decls:
    {
        var v = t.peekNonSpace(); if (v.typ == itemVariable) {
            t.next();
            // Since space is a token, we need 3-token look-ahead here in the worst case:
            // in "$x foo" we need to read "foo" (as opposed to ":=") to know that $x is an
            // argument variable rather than a declaration. So remember the token
            // adjacent to the variable so we can push it back if necessary.
            var tokenAfterVariable = t.peek();
            var next = t.peekNonSpace();
            switch (ᐧ) {
            case {} when (next.typ == itemAssign) || (next.typ == itemDeclare): {
                pipe.Value.IsAssign = next.typ == itemAssign;
                t.nextNonSpace();
                pipe.Value.Decl = builtin.append((~pipe).Decl, Ꮡt.newVariable(v.pos, v.val));
                t.vars = builtin.append(t.vars, v.val);
                break;
            }
            case {} when next.typ == itemChar && next.val == ","u8: {
                t.nextNonSpace();
                pipe.Value.Decl = builtin.append((~pipe).Decl, Ꮡt.newVariable(v.pos, v.val));
                t.vars = builtin.append(t.vars, v.val);
                if (context == "range"u8 && len((~pipe).Decl) < 2) {
                    var exprᴛ1 = t.peekNonSpace().typ;
                    if (exprᴛ1 == itemVariable || exprᴛ1 == itemRightDelim || exprᴛ1 == itemRightParen) {
                        goto decls;
                    }
                    else { /* default: */
                        t.errorf("range can only initialize variables"u8);
                    }

                }
                t.errorf("too many declarations in %s"u8, // second initialized variable in a range pipeline
 context);
                break;
            }
            case {} when tokenAfterVariable.typ == itemSpace: {
                t.backup3(v, tokenAfterVariable);
                break;
            }
            default: {
                t.backup2(v);
                break;
            }}

        }
    }
    while (ᐧ) {
        {
            var tokenΔ1 = t.nextNonSpace();
            var exprᴛ2 = tokenΔ1.typ;
            if (exprᴛ2 == end) {
                t.checkPipeline(pipe, // At this point, the pipeline is complete
 context);
                return pipe;
            }
            if (exprᴛ2 == itemBool || exprᴛ2 == itemCharConstant || exprᴛ2 == itemComplex || exprᴛ2 == itemDot || exprᴛ2 == itemField || exprᴛ2 == itemIdentifier || exprᴛ2 == itemNumber || exprᴛ2 == itemNil || exprᴛ2 == itemRawString || exprᴛ2 == itemString || exprᴛ2 == itemVariable || exprᴛ2 == itemLeftParen) {
                t.backup();
                pipe.append(Ꮡt.command());
            }
            else { /* default: */
                t.unexpected(tokenΔ1, context);
            }
        }

    }
}

[GoRecv] internal static void checkPipeline(this ref Tree t, ж<PipeNode> Ꮡpipe, @string context) {
    ref var pipe = ref Ꮡpipe.DerefOrNull();

    // Reject empty pipelines
    if (len(pipe.Cmds) == 0) {
        t.errorf("missing value for %s"u8, context);
    }
    // Only the first command of a pipeline can start with a non executable operand
    foreach (var (i, c) in pipe.Cmds[1..]) {
        var exprᴛ1 = (~c).Args[0].Type();
        if (exprᴛ1 == NodeBool || exprᴛ1 == NodeDot || exprᴛ1 == NodeNil || exprᴛ1 == NodeNumber || exprᴛ1 == NodeString) {
            t.errorf("non executable command in pipeline stage %d"u8, // With A|B|C, pipeline stage 2 is B
 i + 2);
        }

    }
}

internal static (Pos pos, nint line, ж<PipeNode> pipe, ж<ListNode> list, ж<ListNode> elseList) parseControl(this ж<Tree> Ꮡt, @string context) {
    Pos pos = default!;
    nint line = default!;
    ж<PipeNode> pipe = default!;
    ж<ListNode> list = default!;
    ж<ListNode> elseList = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(Ꮡt.popVars, len(Ꮡt.Value.vars), ref ᒐ);
        pipe = Ꮡt.pipeline(context, itemRightDelim);
        if (context == "range"u8) {
            t.rangeDepth++;
        }
        Node next = default!;
        (list, next) = Ꮡt.itemList();
        if (context == "range"u8) {
            t.rangeDepth--;
        }
        var exprᴛ1 = next.Type();
        if (exprᴛ1 == nodeEnd) {
        }
        else if (exprᴛ1 == nodeElse) {
            if (context == "if"u8 && t.peek().typ == itemIf){
                //done
                // Special case for "else if" and "else with".
                // If the "else" is followed immediately by an "if" or "with",
                // the elseControl will have left the "if" or "with" token pending. Treat
                //	{{if a}}_{{else if b}}_{{end}}
                //  {{with a}}_{{else with b}}_{{end}}
                // as
                //	{{if a}}_{{else}}{{if b}}_{{end}}{{end}}
                //  {{with a}}_{{else}}{{with b}}_{{end}}{{end}}.
                // To do this, parse the "if" or "with" as usual and stop at it {{end}};
                // the subsequent{{end}} is assumed. This technique works even for long if-else-if chains.
                t.next(); // Consume the "if" token.
                elseList = Ꮡt.newList(next.Position());
                elseList.append(Ꮡt.ifControl());
            } else 
            if (context == "with"u8 && t.peek().typ == itemWith){
                t.next();
                elseList = Ꮡt.newList(next.Position());
                elseList.append(Ꮡt.withControl());
            } else {
                (elseList, next) = Ꮡt.itemList();
                if (next.Type() != nodeEnd) {
                    t.errorf("expected end; found %s"u8, next);
                }
            }
        }

        (pos, line, pipe, list, elseList) = (pipe.Position(), (~pipe).Line, pipe, list, elseList);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (pos, line, pipe, list, elseList);
}

// If:
//
//	{{if pipeline}} itemList {{end}}
//	{{if pipeline}} itemList {{else}} itemList {{end}}
//
// If keyword is past.
internal static Node ifControl(this ж<Tree> Ꮡt) {
    var (ᴛ1, ᴛ2, ᴛ3, ᴛ4, ᴛ5) = Ꮡt.parseControl("if"u8);
    return new IfNodeжNode(Ꮡt.newIf(ᴛ1, ᴛ2, ᴛ3, ᴛ4, ᴛ5));
}

// Range:
//
//	{{range pipeline}} itemList {{end}}
//	{{range pipeline}} itemList {{else}} itemList {{end}}
//
// Range keyword is past.
internal static Node rangeControl(this ж<Tree> Ꮡt) {
    var (ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10) = Ꮡt.parseControl(rangeˢ);
    var r = Ꮡt.newRange(ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10);
    return new RangeNodeжNode(r);
}

// With:
//
//	{{with pipeline}} itemList {{end}}
//	{{with pipeline}} itemList {{else}} itemList {{end}}
//
// If keyword is past.
internal static Node withControl(this ж<Tree> Ꮡt) {
    var (ᴛ11, ᴛ12, ᴛ13, ᴛ14, ᴛ15) = Ꮡt.parseControl(withˢ);
    return new WithNodeжNode(Ꮡt.newWith(ᴛ11, ᴛ12, ᴛ13, ᴛ14, ᴛ15));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string endˢ2 = "end"u8;

// End:
//
//	{{end}}
//
// End keyword is past.
internal static Node endControl(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    return new endNodeжNode(Ꮡt.newEnd(t.expect(itemRightDelim, endˢ2).pos));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string elseˢ2 = "else"u8;

// Else:
//
//	{{else}}
//
// Else keyword is past.
internal static Node elseControl(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var peek = t.peekNonSpace();
    // The "{{else if ... " and "{{else with ..." will be
    // treated as "{{else}}{{if ..." and "{{else}}{{with ...".
    // So return the else node here.
    if (peek.typ == itemIf || peek.typ == itemWith) {
        return new elseNodeжNode(Ꮡt.newElse(peek.pos, peek.line));
    }
    var token = t.expect(itemRightDelim, elseˢ2);
    return new elseNodeжNode(Ꮡt.newElse(token.pos, token.line));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contextᶜ1 = "block clause"u8;

// Block:
//
//	{{block stringValue pipeline}}
//
// Block keyword is past.
// The name must be something that can evaluate to a string.
// The pipeline is mandatory.
internal static Node blockControl(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string context = contextᶜ1;
    var token = t.nextNonSpace();
    @string name = t.parseTemplateName(token, context);
    var pipe = Ꮡt.pipeline(context, itemRightDelim);
    var block = New(name); // name will be updated once we know it.
    block.Value.text = t.text;
    block.Value.Mode = t.Mode;
    block.Value.ParseName = t.ParseName;
    block.startParse(t.funcs, t.lex, t.treeSet);
    Node end = default!;
    (block.Value.Root, end) = block.itemList();
    if (end.Type() != nodeEnd) {
        t.errorf("unexpected %s in %s"u8, end, context);
    }
    block.add();
    block.stopParse();
    return new TemplateNodeжNode(Ꮡt.newTemplate(token.pos, token.line, name, pipe));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contextᶜ2 = "template clause"u8;

// Template:
//
//	{{template stringValue pipeline}}
//
// Template keyword is past. The name must be something that can evaluate
// to a string.
internal static Node templateControl(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string context = contextᶜ2;
    var token = t.nextNonSpace();
    @string name = t.parseTemplateName(token, context);
    ж<PipeNode> pipe = default!;
    if (t.nextNonSpace().typ != itemRightDelim) {
        t.backup();
        // Do not pop variables; they persist until "end".
        pipe = Ꮡt.pipeline(context, itemRightDelim);
    }
    return new TemplateNodeжNode(Ꮡt.newTemplate(token.pos, token.line, name, pipe));
}

[GoRecv] internal static @string /*name*/ parseTemplateName(this ref Tree t, item token, @string context) {
    @string name = default!;

    var exprᴛ1 = token.typ;
    if (exprᴛ1 == itemString || exprᴛ1 == itemRawString) {
        var (s, err) = strconv.Unquote(token.val);
        if (err != default!) {
            t.error(err);
        }
        name = s;
    }
    else { /* default: */
        t.unexpected(token, context);
    }

    return name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string operandˢ = "operand"u8;

// command:
//
//	operand (space operand)*
//
// space-separated arguments up to a pipeline character or right delimiter.
// we consume the pipe character but leave the right delim to terminate the action.
internal static ж<CommandNode> command(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var cmd = Ꮡt.newCommand(t.peekNonSpace().pos);
    while (ᐧ) {
        t.peekNonSpace(); // skip leading spaces.
        var operand = Ꮡt.operand();
        if (operand != default!) {
            cmd.append(operand);
        }
        {
            var token = t.next();
            var exprᴛ1 = token.typ;
            if (exprᴛ1 == itemSpace) {
                continue;
            }
            else if (exprᴛ1 == itemRightDelim || exprᴛ1 == itemRightParen) {
                t.backup();
            }
            else if (exprᴛ1 == itemPipe) {
            }
            else { /* default: */
                t.unexpected(token, // nothing here; break loop below
 operandˢ);
            }
        }

        break;
    }
    if (len((~cmd).Args) == 0) {
        t.errorf("empty command"u8);
    }
    return cmd;
}

// operand:
//
//	term .Field*
//
// An operand is a space-separated component of a command,
// a term possibly followed by field accesses.
// A nil return means the next item is not an operand.
internal static Node operand(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var node = Ꮡt.term();
    if (node == default!) {
        return default!;
    }
    if (t.peek().typ == itemField) {
        var chain = Ꮡt.newChain(t.peek().pos, node);
        while (t.peek().typ == itemField) {
            chain.Add(t.next().val);
        }
        // Compatibility with original API: If the term is of type NodeField
        // or NodeVariable, just put more fields on the original.
        // Otherwise, keep the Chain node.
        // Obvious parsing errors involving literal values are detected here.
        // More complex error cases will have to be handled at execution time.
        var exprᴛ1 = node.Type();
        if (exprᴛ1 == NodeField) {
            node = new FieldNodeжNode(Ꮡt.newField(chain.Position(), chain.String()));
        }
        else if (exprᴛ1 == NodeVariable) {
            node = new VariableNodeжNode(Ꮡt.newVariable(chain.Position(), chain.String()));
        }
        else if (exprᴛ1 == NodeBool || exprᴛ1 == NodeString || exprᴛ1 == NodeNumber || exprᴛ1 == NodeNil || exprᴛ1 == NodeDot) {
            t.errorf("unexpected . after term %q"u8, node.String());
        }
        else { /* default: */
            node = new ChainNodeжNode(chain);
        }

    }
    return node;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parenthesizedPipelineˢ = "parenthesized pipeline"u8;

// term:
//
//	literal (number, string, nil, boolean)
//	function (identifier)
//	.
//	.Field
//	$
//	'(' pipeline ')'
//
// A term is a simple "expression".
// A nil return means the next item is not a term.
internal static Node term(this ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var token = t.nextNonSpace();
        var exprᴛ1 = token.typ;
        if (exprᴛ1 == itemIdentifier) {
            var checkFunc = (Mode)(t.Mode & SkipFuncCheck) == 0;
            if (checkFunc && !t.hasFunction(token.val)) {
                t.errorf("function %q not defined"u8, token.val);
            }
            return new IdentifierNodeжNode(NewIdentifier(token.val).SetTree(Ꮡt).SetPos(token.pos));
        }
        if (exprᴛ1 == itemDot) {
            return new DotNodeжNode(Ꮡt.newDot(token.pos));
        }
        if (exprᴛ1 == itemNil) {
            return new NilNodeжNode(Ꮡt.newNil(token.pos));
        }
        if (exprᴛ1 == itemVariable) {
            return Ꮡt.useVar(token.pos, token.val);
        }
        if (exprᴛ1 == itemField) {
            return new FieldNodeжNode(Ꮡt.newField(token.pos, token.val));
        }
        if (exprᴛ1 == itemBool) {
            return new BoolNodeжNode(Ꮡt.newBool(token.pos, token.val == "true"u8));
        }
        if (exprᴛ1 == itemCharConstant || exprᴛ1 == itemComplex || exprᴛ1 == itemNumber) {
            var (number, err) = Ꮡt.newNumber(token.pos, token.val, token.typ);
            if (err != default!) {
                t.error(err);
            }
            return new NumberNodeжNode(number);
        }
        if (exprᴛ1 == itemLeftParen) {
            return new PipeNodeжNode(Ꮡt.pipeline(parenthesizedPipelineˢ, itemRightParen));
        }
        if (exprᴛ1 == itemString || exprᴛ1 == itemRawString) {
            var (s, err) = strconv.Unquote(token.val);
            if (err != default!) {
                t.error(err);
            }
            return new StringNodeжNode(Ꮡt.newString(token.pos, token.val, s));
        }
    }

    t.backup();
    return default!;
}

// hasFunction reports if a function name exists in the Tree's maps.
[GoRecv] internal static bool hasFunction(this ref Tree t, @string name) {
    foreach (var (_, funcMap) in t.funcs) {
        if (funcMap == default!) {
            continue;
        }
        if (funcMap[name] != default!) {
            return true;
        }
    }
    return false;
}

// popVars trims the variable list to the specified length
[GoRecv] internal static void popVars(this ref Tree t, nint n) {
    t.vars = t.vars.slice(0, n);
}

// useVar returns a node for a variable reference. It errors if the
// variable is not defined.
internal static Node useVar(this ж<Tree> Ꮡt, Pos pos, @string name) {
    ref var t = ref Ꮡt.DerefOrNull();

    var v = Ꮡt.newVariable(pos, name);
    foreach (var (_, varName) in t.vars) {
        if (varName == (~v).Ident[0]) {
            return new VariableNodeжNode(v);
        }
    }
    t.errorf("undefined variable %q"u8, (~v).Ident[0]);
    return default!;
}

} // end parse_package
