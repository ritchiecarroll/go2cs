// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using fmt = fmt_package;
using html = html_package;
using godebug = @internal.godebug_package;
using io = io_package;
using maps = maps_package;
using regexp = regexp_package;
using template = text.template_package;
using parse = text.template.parse_package;
using @internal;
using text;
using text.template;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// escapeTemplate rewrites the named template, which must be
// associated with t, to guarantee that the output of any of the named
// templates is properly escaped. If no error is returned, then the named templates have
// been modified. Otherwise the named templates have been rendered
// unusable.
internal static error escapeTemplate(ж<Template> Ꮡtmpl, parse.Node node, @string name) {
    ref var tmpl = ref Ꮡtmpl.DerefOrNull();

    var (c, _) = tmpl.esc.escapeTree(new context(nil), node, name, 0);
    error err = default!;
    if (c.err != nil){
        (err, c.err.Value.Name) = (new ΔErrorжerror(c.err), name);
    } else 
    if (c.state != stateText) {
        err = new ΔErrorжerror(Ꮡ(new ΔError(ErrEndContext, default!, name, 0, fmt.Sprintf("ends in a non-text context: %v"u8, c))));
    }
    if (err != default!) {
        // Prevent execution of unsafe templates.
        {
            var t = tmpl.set[name]; if (t != nil) {
                t.Value.escapeErr = err;
                t.Value.text.Value.Tree = default!;
                t.Value.Tree = default!;
            }
        }
        return err;
    }
    tmpl.esc.commit();
    {
        var t = tmpl.set[name]; if (t != nil) {
            t.Value.escapeErr = escapeOK;
            t.Value.Tree = t.Value.text.Value.Tree;
        }
    }
    return default!;
}

// evalArgs formats the list of arguments into a string. It is equivalent to
// fmt.Sprint(args...), except that it dereferences all pointers.
internal static @string evalArgs(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    // Optimization for simple common case of a single string argument.
    if (len(args) == 1) {
        {
            var (s, ok) = args[0]._<@string>(ᐧ); if (ok) {
                return s;
            }
        }
    }
    foreach (var (i, arg) in args) {
        args[i] = indirectToStringerOrError(arg);
    }
    return fmt.Sprint(args.ꓸꓸꓸ);
}

// funcMap maps command names to functions that render their inputs safe.
internal static template.FuncMap funcMap;
internal static void initᴛfuncMap() { funcMap = new text.template_package.FuncMap(new map<@string, any>{
    ["_html_template_attrescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(attrEscaper)),
    ["_html_template_commentescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(commentEscaper)),
    ["_html_template_cssescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(cssEscaper)),
    ["_html_template_cssvaluefilter"u8] = ((Funcꓸꓸꓸ<any, @string>)(cssValueFilter)),
    ["_html_template_htmlnamefilter"u8] = ((Funcꓸꓸꓸ<any, @string>)(htmlNameFilter)),
    ["_html_template_htmlescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(htmlEscaper)),
    ["_html_template_jsregexpescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(jsRegexpEscaper)),
    ["_html_template_jsstrescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(jsStrEscaper)),
    ["_html_template_jstmpllitescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(jsTmplLitEscaper)),
    ["_html_template_jsvalescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(jsValEscaper)),
    ["_html_template_nospaceescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(htmlNospaceEscaper)),
    ["_html_template_rcdataescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(rcdataEscaper)),
    ["_html_template_srcsetescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(srcsetFilterAndEscaper)),
    ["_html_template_urlescaper"u8] = ((Funcꓸꓸꓸ<any, @string>)(urlEscaper)),
    ["_html_template_urlfilter"u8] = ((Funcꓸꓸꓸ<any, @string>)(urlFilter)),
    ["_html_template_urlnormalizer"u8] = ((Funcꓸꓸꓸ<any, @string>)(urlNormalizer)),
    ["_eval_args_"u8] = ((Funcꓸꓸꓸ<any, @string>)(evalArgs))
}); }

// escaper collects type inferences about templates and changes needed to make
// templates injection safe.
partial struct escaper {
    // ns is the nameSpace that this escaper is associated with.
    internal ж<nameSpace> ns;
    // output[templateName] is the output context for a templateName that
    // has been mangled to include its input context.
    internal map<@string, context> output;
    // derived[c.mangle(name)] maps to a template derived from the template
    // named name templateName for the start context c.
    internal map<@string, ж<template.Template>> derived;
    // called[templateName] is a set of called mangled template names.
    internal map<@string, bool> called;
    // xxxNodeEdits are the accumulated edits to apply during commit.
    // Such edits are not applied immediately in case a template set
    // executes a given template in different escaping contexts.
    internal map<ж<parse.ActionNode>, slice<@string>> actionNodeEdits;
    internal map<ж<parse.TemplateNode>, @string> templateNodeEdits;
    internal map<ж<parse.TextNode>, slice<byte>> textNodeEdits;
    // rangeContext holds context about the current range loop.
    internal ж<rangeContext> rangeContext;
}

// rangeContext holds information about the current range loop.
partial struct rangeContext {
    internal ж<rangeContext> outer; // outer loop
    internal slice<context> breaks; // context at each break action
    internal slice<context> continues; // context at each continue action
}

// makeEscaper creates a blank escaper for the given set.
internal static escaper makeEscaper(ж<nameSpace> Ꮡn) {
    return new escaper(
        Ꮡn,
        new map<@string, context>{},
        new map<@string, ж<template.Template>>{},
        new map<@string, bool>{},
        new map<ж<parse.ActionNode>, slice<@string>>{},
        new map<ж<parse.TemplateNode>, @string>{},
        new map<ж<parse.TextNode>, slice<byte>>{},
        nil
    );
}

// filterFailsafe is an innocuous word that is emitted in place of unsafe values
// by sanitizer functions. It is not a keyword in any programming language,
// contains no special characters, is not empty, and when it appears in output
// it is distinct enough that a developer can find the source of the problem
// via a search engine.
internal static readonly @string filterFailsafe = "ZgotmplZ"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangeˢ = "range"u8;
internal static readonly @string withˢ = "with"u8;

// escape escapes a template node.
internal static context escape(this ref escaper e, context c, parse.Node n) {
    switch (n.type()) {
    case ж<parse.ActionNode> nΔ1: {
        return e.escapeAction(c, nΔ1);
    }
    case ж<parse.BreakNode> nΔ1: {
        c.n = new parse.BreakNodeжNode(nΔ1);
        e.rangeContext.Value.breaks = append((~e.rangeContext).breaks, c);
        return new context(state: stateDead);
    }
    case ж<parse.CommentNode> nΔ1: {
        return c;
    }
    case ж<parse.ContinueNode> nΔ1: {
        c.n = new parse.ContinueNodeжNode(nΔ1);
        e.rangeContext.Value.continues = append((~e.rangeContext).continues, c);
        return new context(state: stateDead);
    }
    case ж<parse.IfNode> nΔ1: {
        return e.escapeBranch(c, nΔ1.of(parse.IfNode.ᏑBranchNode), "if"u8);
    }
    case ж<parse.ListNode> nΔ1: {
        return e.escapeList(c, nΔ1);
    }
    case ж<parse.RangeNode> nΔ1: {
        return e.escapeBranch(c, nΔ1.of(parse.RangeNode.ᏑBranchNode), rangeˢ);
    }
    case ж<parse.TemplateNode> nΔ1: {
        return e.escapeTemplate(c, nΔ1);
    }
    case ж<parse.TextNode> nΔ1: {
        return e.escapeText(c, nΔ1);
    }
    case ж<parse.WithNode> nΔ1: {
        return e.escapeBranch(c, nΔ1.of(parse.WithNode.ᏑBranchNode), withˢ);
    }}
    throw panic("escaping " + n.String() + " is unimplemented");
}

internal static ж<godebug.Setting> debugAllowActionJSTmpl = godebug.New("jstmpllitinterp"u8);

// escapeAction escapes an action template node.
internal static context escapeAction(this ref escaper e, context c, ж<parse.ActionNode> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (len((~n.Pipe).Decl) != 0) {
        // A local variable assignment, not an interpolation.
        return c;
    }
    c = nudge(c);
    // Check for disallowed use of predefined escapers in the pipeline.
    foreach (var (pos, idNode) in (~n.Pipe).Cmds) {
        var (node, ok) = (~idNode).Args[0]._<ж<parse.IdentifierNode>>(ᐧ);
        if (!ok) {
            // A predefined escaper "esc" will never be found as an identifier in a
            // Chain or Field node, since:
            // - "esc.x ..." is invalid, since predefined escapers return strings, and
            //   strings do not have methods, keys or fields.
            // - "... .esc" is invalid, since predefined escapers are global functions,
            //   not methods or fields of any types.
            // Therefore, it is safe to ignore these two node types.
            continue;
        }
        @string ident = node.Value.Ident;
        {
            var (_, okΔ1) = predefinedEscapers[ident, ꟷ]; if (okΔ1) {
                if (pos < len((~n.Pipe).Cmds) - 1 || c.state == stateAttr && c.delim == delimSpaceOrTagEnd && ident == "html"u8) {
                    return new context(
                        state: stateError,
                        err: errorf(ErrPredefinedEscaper, new parse.ActionNodeжNode(Ꮡn), n.Line, "predefined escaper %q disallowed in template"u8, ident)
                    );
                }
            }
        }
    }
    var s = new slice<@string>(0, 3);
    var exprᴛ1 = c.state;
    if (exprᴛ1 == stateError) {
        return c;
    }
    if (exprᴛ1 == stateURL || exprᴛ1 == stateCSSDqStr || exprᴛ1 == stateCSSSqStr || exprᴛ1 == stateCSSDqURL || exprᴛ1 == stateCSSSqURL || exprᴛ1 == stateCSSURL) {
        var exprᴛ2 = c.urlPart;
        var matchᴛ1 = false;
        if (exprᴛ2 == urlPartNone) { matchᴛ1 = true;
            s = append(s, "_html_template_urlfilter"u8);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ2 == urlPartPreQuery) { matchᴛ1 = true;
            var exprᴛ3 = c.state;
            if (exprᴛ3 == stateCSSDqStr || exprᴛ3 == stateCSSSqStr) {
                s = append(s, "_html_template_cssescaper"u8);
            }
            else { /* default: */
                s = append(s, "_html_template_urlnormalizer"u8);
            }

        }
        else if (exprᴛ2 == urlPartQueryOrFrag) { matchᴛ1 = true;
            s = append(s, "_html_template_urlescaper"u8);
        }
        else if (exprᴛ2 == urlPartUnknown) {
            return new context(
                state: stateError,
                err: errorf(ErrAmbigContext, new parse.ActionNodeжNode(Ꮡn), n.Line, "%s appears in an ambiguous context within a URL"u8, Ꮡn.OrTypedNil())
            );
        }
        else if (!matchᴛ1) { /* default: */
            throw panic(c.urlPart.String());
        }

    }
    else if (exprᴛ1 == stateJS) {
        s = append(s, "_html_template_jsvalescaper"u8);
        c.jsCtx = jsCtxDivOp;
    }
    else if (exprᴛ1 == stateJSDqStr || exprᴛ1 == stateJSSqStr) {
        s = append(s, // A slash after a value starts a div operator.
 "_html_template_jsstrescaper"u8);
    }
    else if (exprᴛ1 == stateJSTmplLit) {
        s = append(s, "_html_template_jstmpllitescaper"u8);
    }
    else if (exprᴛ1 == stateJSRegexp) {
        s = append(s, "_html_template_jsregexpescaper"u8);
    }
    else if (exprᴛ1 == stateCSS) {
        s = append(s, "_html_template_cssvaluefilter"u8);
    }
    else if (exprᴛ1 == stateText) {
        s = append(s, "_html_template_htmlescaper"u8);
    }
    else if (exprᴛ1 == stateRCDATA) {
        s = append(s, "_html_template_rcdataescaper"u8);
    }
    else if (exprᴛ1 == stateAttr) {
    }
    else if (exprᴛ1 == stateAttrName || exprᴛ1 == stateTag) {
        c.state = stateAttrName;
        s = append(s, // Handled below in delim check.
 "_html_template_htmlnamefilter"u8);
    }
    else if (exprᴛ1 == stateSrcset) {
        s = append(s, "_html_template_srcsetescaper"u8);
    }
    else { /* default: */
        if (isComment(c.state)){
            s = append(s, "_html_template_commentescaper"u8);
        } else {
            throw panic("unexpected state " + c.state.String());
        }
    }

    var exprᴛ4 = c.delim;
    if (exprᴛ4 == delimNone) {
    }
    else if (exprᴛ4 == delimSpaceOrTagEnd) {
        s = append(s, // No extra-escaping needed for raw text content.
 "_html_template_nospaceescaper"u8);
    }
    else { /* default: */
        s = append(s, "_html_template_attrescaper"u8);
    }

    e.editActionNode(Ꮡn, s);
    return c;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string evalArgsˢ = "_eval_args_"u8;

// ensurePipelineContains ensures that the pipeline ends with the commands with
// the identifiers in s in order. If the pipeline ends with a predefined escaper
// (i.e. "html" or "urlquery"), merge it with the identifiers in s.
internal static void ensurePipelineContains(ж<parse.PipeNode> Ꮡp, slice<@string> s) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (len(s) == 0) {
        // Do not rewrite pipeline if we have no escapers to insert.
        return;
    }
    // Precondition: p.Cmds contains at most one predefined escaper and the
    // escaper will be present at p.Cmds[len(p.Cmds)-1]. This precondition is
    // always true because of the checks in escapeAction.
    nint pipelineLen = len(p.Cmds);
    if (pipelineLen > 0) {
        var lastCmd = p.Cmds[pipelineLen - 1];
        {
            var (idNode, ok) = (~lastCmd).Args[0]._<ж<parse.IdentifierNode>>(ᐧ); if (ok) {
                {
                    @string esc = idNode.Value.Ident; if (predefinedEscapers[esc]) {
                        // Pipeline ends with a predefined escaper.
                        if (len(p.Cmds) == 1 && len((~lastCmd).Args) > 1) {
                            // Special case: pipeline is of the form {{ esc arg1 arg2 ... argN }},
                            // where esc is the predefined escaper, and arg1...argN are its arguments.
                            // Convert this into the equivalent form
                            // {{ _eval_args_ arg1 arg2 ... argN | esc }}, so that esc can be easily
                            // merged with the escapers in s.
                            lastCmd.Value.Args[0] = new parse.IdentifierNodeжNode(parse.NewIdentifier(evalArgsˢ).SetTree(nil).SetPos((~lastCmd).Args[0].Position()));
                            p.Cmds = appendCmd(p.Cmds, newIdentCmd(esc, p.Position()));
                            pipelineLen++;
                        }
                        // If any of the commands in s that we are about to insert is equivalent
                        // to the predefined escaper, use the predefined escaper instead.
                        var dup = false;
                        foreach (var (i, escaper) in s) {
                            if (escFnsEq(esc, escaper)) {
                                s[i] = idNode.Value.Ident;
                                dup = true;
                            }
                        }
                        if (dup) {
                            // The predefined escaper will already be inserted along with the
                            // escapers in s, so do not copy it to the rewritten pipeline.
                            pipelineLen--;
                        }
                    }
                }
            }
        }
    }
    // Rewrite the pipeline, creating the escapers in s at the end of the pipeline.
    var newCmds = new slice<ж<parse.CommandNode>>(pipelineLen, pipelineLen + len(s));
    var insertedIdents = new map<@string, bool>();
    for (nint i = 0; i < pipelineLen; i++) {
        var cmd = p.Cmds[i];
        newCmds[i] = cmd;
        {
            var (idNode, ok) = (~cmd).Args[0]._<ж<parse.IdentifierNode>>(ᐧ); if (ok) {
                insertedIdents[normalizeEscFn((~idNode).Ident)] = true;
            }
        }
    }
    foreach (var (_, name) in s) {
        if (!insertedIdents[normalizeEscFn(name)]) {
            // When two templates share an underlying parse tree via the use of
            // AddParseTree and one template is executed after the other, this check
            // ensures that escapers that were already inserted into the pipeline on
            // the first escaping pass do not get inserted again.
            newCmds = appendCmd(newCmds, newIdentCmd(name, p.Position()));
        }
    }
    p.Cmds = newCmds;
}

// predefinedEscapers contains template predefined escapers that are equivalent
// to some contextual escapers. Keep in sync with equivEscapers.
internal static map<@string, bool> predefinedEscapers = new map<@string, bool>{
    ["html"u8] = true,
    ["urlquery"u8] = true
};

// The following pairs of HTML escapers provide equivalent security
// guarantees, since they all escape '\000', '\'', '"', '&', '<', and '>'.
// These two URL escapers produce URLs safe for embedding in a URL query by
// percent-encoding all the reserved characters specified in RFC 3986 Section
// 2.2
// These two functions are not actually equivalent; urlquery is stricter as it
// escapes reserved characters (e.g. '#'), while _html_template_urlnormalizer
// does not. It is therefore only safe to replace _html_template_urlnormalizer
// with urlquery (this happens in ensurePipelineContains), but not the otherI've
// way around. We keep this entry around to preserve the behavior of templates
// written before Go 1.9, which might depend on this substitution taking place.
// equivEscapers matches contextual escapers to equivalent predefined
// template escapers.
internal static map<@string, @string> equivEscapers = new map<@string, @string>{
    ["_html_template_attrescaper"u8] = "html"u8,
    ["_html_template_htmlescaper"u8] = "html"u8,
    ["_html_template_rcdataescaper"u8] = "html"u8,
    ["_html_template_urlescaper"u8] = "urlquery"u8,
    ["_html_template_urlnormalizer"u8] = "urlquery"u8
};

// escFnsEq reports whether the two escaping functions are equivalent.
internal static bool escFnsEq(@string a, @string b) {
    return normalizeEscFn(a) == normalizeEscFn(b);
}

// normalizeEscFn(a) is equal to normalizeEscFn(b) for any pair of names of
// escaper functions a and b that are equivalent.
internal static @string normalizeEscFn(@string e) {
    {
        @string norm = equivEscapers[e]; if (norm != ""u8) {
            return norm;
        }
    }
    return e;
}

// redundantFuncs[a][b] implies that funcMap[b](funcMap[a](x)) == funcMap[a](x)
// for all x.
internal static map<@string, map<@string, bool>> redundantFuncs = new map<@string, map<@string, bool>>{
    ["_html_template_commentescaper"u8] = new map<@string, bool>{
        ["_html_template_attrescaper"u8] = true,
        ["_html_template_htmlescaper"u8] = true},
    ["_html_template_cssescaper"u8] = new map<@string, bool>{
        ["_html_template_attrescaper"u8] = true},
    ["_html_template_jsregexpescaper"u8] = new map<@string, bool>{
        ["_html_template_attrescaper"u8] = true},
    ["_html_template_jsstrescaper"u8] = new map<@string, bool>{
        ["_html_template_attrescaper"u8] = true},
    ["_html_template_jstmpllitescaper"u8] = new map<@string, bool>{
        ["_html_template_attrescaper"u8] = true},
    ["_html_template_urlescaper"u8] = new map<@string, bool>{
        ["_html_template_urlnormalizer"u8] = true}
};

// appendCmd appends the given command to the end of the command pipeline
// unless it is redundant with the last command.
internal static slice<ж<parse.CommandNode>> appendCmd(slice<ж<parse.CommandNode>> cmds, ж<parse.CommandNode> Ꮡcmd) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    {
        nint n = len(cmds); if (n != 0) {
            var (last, okLast) = (~cmds[n - 1]).Args[0]._<ж<parse.IdentifierNode>>(ᐧ);
            var (next, okNext) = cmd.Args[0]._<ж<parse.IdentifierNode>>(ᐧ);
            if (okLast && okNext && redundantFuncs[(~last).Ident][(~next).Ident]) {
                return cmds;
            }
        }
    }
    return append(cmds, Ꮡcmd);
}

// newIdentCmd produces a command containing a single identifier node.
internal static ж<parse.CommandNode> newIdentCmd(@string identifier, parse.Pos pos) {
    return Ꮡ(new parse.CommandNode(
        NodeType: parse.NodeCommand,
        Args: new parse.Node[]{new parse.IdentifierNodeжNode(parse.NewIdentifier(identifier).SetTree(nil).SetPos(pos))}.slice()
    ));
}

// TODO: SetTree.

// nudge returns the context that would result from following empty string
// transitions from the input context.
// For example, parsing:
//
//	`<a href=`
//
// will end in context{stateBeforeValue, attrURL}, but parsing one extra rune:
//
//	`<a href=x`
//
// will end in context{stateURL, delimSpaceOrTagEnd, ...}.
// There are two transitions that happen when the 'x' is seen:
// (1) Transition from a before-value state to a start-of-value state without
//
//	consuming any character.
//
// (2) Consume 'x' and transition past the first value character.
// In this case, nudging produces the context after (1) happens.
internal static context nudge(context c) {
    var exprᴛ1 = c.state;
    if (exprᴛ1 == stateTag) {
        c.state = stateAttrName;
    }
    else if (exprᴛ1 == stateBeforeValue) {
        (c.state, c.delim, c.attr) = (attrStartStates[c.attr], delimSpaceOrTagEnd, attrNone);
    }
    else if (exprᴛ1 == stateAfterName) {
        (c.state, c.attr) = (stateAttrName, attrNone);
    }

    // In `<foo {{.}}`, the action should emit an attribute.
    // In `<foo bar={{.}}`, the action is an undelimited value.
    // In `<foo bar {{.}}`, the action is an attribute name.
    return c;
}

// join joins the two contexts of a branch template node. The result is an
// error context if either of the input contexts are error contexts, or if the
// input contexts differ.
internal static context join(context a, context b, parse.Node node, @string nodeName) {
    if (a.state == stateError) {
        return a;
    }
    if (b.state == stateError) {
        return b;
    }
    if (a.state == stateDead) {
        return b;
    }
    if (b.state == stateDead) {
        return a;
    }
    if (a.eq(b)) {
        return a;
    }
    var c = a;
    c.urlPart = b.urlPart;
    if (c.eq(b)) {
        // The contexts differ only by urlPart.
        c.urlPart = urlPartUnknown;
        return c;
    }
    c = a;
    c.jsCtx = b.jsCtx;
    if (c.eq(b)) {
        // The contexts differ only by jsCtx.
        c.jsCtx = jsCtxUnknown;
        return c;
    }
    // Allow a nudged context to join with an unnudged one.
    // This means that
    //   <p title={{if .C}}{{.}}{{end}}
    // ends in an unquoted value state even though the else branch
    // ends in stateBeforeValue.
    {
        var (cΔ1, d) = (nudge(a), nudge(b)); if (!(cΔ1.eq(a) && d.eq(b))) {
            {
                var e = join(cΔ1, d, node, nodeName); if (e.state != stateError) {
                    return e;
                }
            }
        }
    }
    return new context(
        state: stateError,
        err: errorf(ErrBranchEnd, node, 0, "{{%s}} branches end in different contexts: %v, %v"u8, nodeName, a, b)
    );
}

// escapeBranch escapes a branch template node: "if", "range" and "with".
internal static context escapeBranch(this ref escaper e, context c, ж<parse.BranchNode> Ꮡn, @string nodeName) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (nodeName == "range"u8) {
        e.rangeContext = Ꮡ(new rangeContext(outer: e.rangeContext));
    }
    var c0 = e.escapeList(c, n.List);
    if (nodeName == "range"u8) {
        if (c0.state != stateError) {
            c0 = joinRange(c0, ref (e.rangeContext).DerefOrNull());
        }
        e.rangeContext = e.rangeContext.Value.outer;
        if (c0.state == stateError) {
            return c0;
        }
        // The "true" branch of a "range" node can execute multiple times.
        // We check that executing n.List once results in the same context
        // as executing n.List twice.
        e.rangeContext = Ꮡ(new rangeContext(outer: e.rangeContext));
        var (c1Δ1, _) = e.escapeListConditionally(c0, n.List, default!);
        c0 = join(c0, c1Δ1, new parse.BranchNodeжNode(Ꮡn), nodeName);
        if (c0.state == stateError) {
            e.rangeContext = e.rangeContext.Value.outer;
            // Make clear that this is a problem on loop re-entry
            // since developers tend to overlook that branch when
            // debugging templates.
            c0.err.Value.Line = n.Line;
            c0.err.Value.Description = "on range loop re-entry: "u8 + (~c0.err).Description;
            return c0;
        }
        c0 = joinRange(c0, ref (e.rangeContext).DerefOrNull());
        e.rangeContext = e.rangeContext.Value.outer;
        if (c0.state == stateError) {
            return c0;
        }
    }
    var c1 = e.escapeList(c, n.ElseList);
    return join(c0, c1, new parse.BranchNodeжNode(Ꮡn), nodeName);
}

internal static context joinRange(context c0, ref rangeContext rc) {
    // Merge contexts at break and continue statements into overall body context.
    // In theory we could treat breaks differently from continues, but for now it is
    // enough to treat them both as going back to the start of the loop (which may then stop).
    foreach (var (_, c) in rc.breaks) {
        c0 = join(c0, c, c.n, rangeˢ);
        if (c0.state == stateError) {
            c0.err.Value.Line = c.n._<ж<parse.BreakNode>>().Value.Line;
            c0.err.Value.Description = "at range loop break: "u8 + (~c0.err).Description;
            return c0;
        }
    }
    foreach (var (_, c) in rc.continues) {
        c0 = join(c0, c, c.n, rangeˢ);
        if (c0.state == stateError) {
            c0.err.Value.Line = c.n._<ж<parse.ContinueNode>>().Value.Line;
            c0.err.Value.Description = "at range loop continue: "u8 + (~c0.err).Description;
            return c0;
        }
    }
    return c0;
}

// escapeList escapes a list template node.
internal static context escapeList(this ref escaper e, context c, ж<parse.ListNode> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return c;
    }
    foreach (var (_, m) in n.Nodes) {
        c = e.escape(c, m);
        if (c.state == stateDead) {
            break;
        }
    }
    return c;
}

// escapeListConditionally escapes a list node but only preserves edits and
// inferences in e if the inferences and output context satisfy filter.
// It returns the best guess at an output context, and the result of the filter
// which is the same as whether e was updated.
internal static (context, bool) escapeListConditionally(this ref escaper e, context c, ж<parse.ListNode> Ꮡn, Func<ж<escaper>, context, bool> filter) {
    ref var e1 = ref heap<escaper>(out var Ꮡe1);
    e1 = makeEscaper(e.ns);
    e1.rangeContext = e.rangeContext;
    // Make type inferences available to f.
    maps.Copy<map<@string, context>, map<@string, context>, @string, context>(e1.output, e.output);
    c = e1.escapeList(c, Ꮡn);
    var ok = filter != default! && filter(Ꮡe1, c);
    if (ok) {
        // Copy inferences and edits from e1 back into e.
        maps.Copy<map<@string, context>, map<@string, context>, @string, context>(e.output, e1.output);
        maps.Copy<map<@string, ж<template.Template>>, map<@string, ж<template.Template>>, @string, ж<template.Template>>(e.derived, e1.derived);
        maps.Copy<map<@string, bool>, map<@string, bool>, @string, bool>(e.called, e1.called);
        foreach (var (k, v) in e1.actionNodeEdits) {
            e.editActionNode(k, v);
        }
        foreach (var (k, v) in e1.templateNodeEdits) {
            e.editTemplateNode(k, v);
        }
        foreach (var (k, v) in e1.textNodeEdits) {
            e.editTextNode(k, v);
        }
    }
    return (c, ok);
}

// escapeTemplate escapes a {{template}} call node.
internal static context escapeTemplate(this ref escaper e, context c, ж<parse.TemplateNode> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    (c, var name) = e.escapeTree(c, new parse.TemplateNodeжNode(Ꮡn), n.Name, n.Line);
    if (name != n.Name) {
        e.editTemplateNode(Ꮡn, name);
    }
    return c;
}

// escapeTree escapes the named template starting in the given context as
// necessary and returns its output context.
internal static (context, @string) escapeTree(this ref escaper e, context c, parse.Node node, @string name, nint line) {
    // Mangle the template name with the input context to produce a reliable
    // identifier.
    ref var dname = ref heap<@string>(out var Ꮡdname);
    dname = c.mangle(name);
    e.called[dname] = true;
    {
        var (@out, ok) = e.output[dname, ꟷ]; if (ok) {
            // Already escaped.
            return (@out, dname);
        }
    }
    var t = e.template(name);
    if (t == nil) {
        // Two cases: The template exists but is empty, or has never been mentioned at
        // all. Distinguish the cases in the error messages.
        if ((~e.ns).set[name] != nil) {
            return (new context(
                state: stateError,
                err: errorf(ErrNoSuchTemplate, node, line, "%q is an incomplete or empty template"u8, name)
            ), dname);
        }
        return (new context(
            state: stateError,
            err: errorf(ErrNoSuchTemplate, node, line, "no such template %q"u8, name)
        ), dname);
    }
    if (dname != name) {
        // Use any template derived during an earlier call to escapeTemplate
        // with different top level templates, or clone if necessary.
        var dt = e.template(dname);
        if (dt == nil) {
            dt = text.template_package.New(dname);
            dt.Value.Tree = Ꮡ(new parse.Tree(Name: dname, Root: (~t).Root.CopyList()));
            e.derived[dname] = dt;
        }
        t = dt;
    }
    return (e.computeOutCtx(c, t), dname);
}

// computeOutCtx takes a template and its start context and computes the output
// context while storing any inferences in e.
internal static context computeOutCtx(this ref escaper e, context c, ж<template.Template> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Propagate context over the body.
    var (c1, ok) = e.escapeTemplateBody(c, Ꮡt);
    if (!ok) {
        // Look for a fixed point by assuming c1 as the output context.
        {
            var (c2, ok2) = e.escapeTemplateBody(c1, Ꮡt); if (ok2) {
                (c1, ok) = (c2, true);
            }
        }
    }
    // Use c1 as the error context if neither assumption worked.
    if (!ok && c1.state != stateError) {
        return new context(
            state: stateError,
            err: errorf(ErrOutputContext, new parse.ListNodeжNode((~t.Tree).Root), 0, "cannot compute output context for template %s"u8, t.Name())
        );
    }
    return c1;
}

// escapeTemplateBody escapes the given template assuming the given output
// context, and returns the best guess at the output context and whether the
// assumption was correct.
internal static (context, bool) escapeTemplateBody(this ref escaper e, context c, ж<template.Template> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var cʗ1 = c;
    var filter = (ж<escaper> e1, context c1) => {
        if (c1.state == stateError) {
            // Do not update the input escaper, e.
            return false;
        }
        if (!(~e1).called[Ꮡt.Value.Name()]) {
            // If t is not recursively called, then c1 is an
            // accurate output context.
            return true;
        }
        // c1 is accurate if it matches our assumed output context.
        return cʗ1.eq(c1);
    };
    // We need to assume an output context so that recursive template calls
    // take the fast path out of escapeTree instead of infinitely recurring.
    // Naively assuming that the input context is the same as the output
    // works >90% of the time.
    e.output[t.Name()] = c;
    return e.escapeListConditionally(c, (~t.Tree).Root, filter);
}

// Determined empirically by running the below in various browsers.
// var div = document.createElement("DIV");
// for (var i = 0; i < 0x10000; ++i) {
//   div.innerHTML = "<span title=x" + String.fromCharCode(i) + "-bar>";
//   if (div.getElementsByTagName("SPAN")[0].title.indexOf("bar") < 0)
//     document.write("<p>U+" + i.toString(16));
// }
// delimEnds maps each delim to a string of characters that terminate it.
internal static array<@string> delimEnds = new golib.SparseArray<@string>{
    [delimDoubleQuote] = @""""u8,
    [delimSingleQuote] = "'"u8,
    [delimSpaceOrTagEnd] = " \t\n\f\r>"u8
}.array(4);

internal static ж<regexp.Regexp> specialScriptTagRE = regexp.MustCompile("(?i)<(script|/script|!--)"u8);
internal static slice<byte> specialScriptTagReplacement = slice<byte>("\\x3C$1"u8);

internal static bool containsSpecialScriptTag(slice<byte> s) {
    return specialScriptTagRE.Match(s);
}

internal static slice<byte> escapeSpecialScriptTags(slice<byte> s) {
    return specialScriptTagRE.ReplaceAll(s, specialScriptTagReplacement);
}

internal static slice<byte> doctypeBytes = slice<byte>("<!DOCTYPE"u8);

// escapeText escapes a text template node.
internal static context escapeText(this ref escaper e, context c, ж<parse.TextNode> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    var s = n.Text;
    nint written = 0;
    nint i = 0;
    var b = @new<bytes.Buffer>();
    while (i != len(s)) {
        var (c1, nread) = contextAfterText(c, s.slice(i));
        nint i1 = i + nread;
        if (c.state == stateText || c.state == stateRCDATA){
            nint end = i1;
            if (c1.state != c.state) {
                for (nint j = end - 1; j >= i; j--) {
                    if (s[j] == (rune)'<') {
                        end = j;
                        break;
                    }
                }
            }
            for (nint j = i; j < end; j++) {
                if (s[j] == (rune)'<' && !bytes.HasPrefix(bytes.ToUpper(s.slice(j)), doctypeBytes)) {
                    b.Write(s.slice(written, j));
                    b.WriteString("&lt;"u8);
                    written = j + 1;
                }
            }
        } else 
        if (isComment(c.state) && c.delim == delimNone) {
            var exprᴛ1 = c.state;
            if (exprᴛ1 == stateJSBlockCmt) {
                if (bytes.ContainsAny(s.slice(written, i1), // https://es5.github.io/#x7.4:
 // "Comments behave like white space and are
 // discarded except that, if a MultiLineComment
 // contains a line terminator character, then
 // the entire comment is considered to be a
 // LineTerminator for purposes of parsing by
 // the syntactic grammar."
 "\n\r\u2028\u2029"u8)){
                    b.WriteByte((rune)'\n');
                } else {
                    b.WriteByte((rune)' ');
                }
            }
            else if (exprᴛ1 == stateCSSBlockCmt) {
                b.WriteByte((rune)' ');
            }

            written = i1;
        }
        if (c.state != c1.state && isComment(c1.state) && c1.delim == delimNone) {
            // Preserve the portion between written and the comment start.
            nint cs = i1 - 2;
            if (c1.state == stateHTMLCmt || c1.state == stateJSHTMLOpenCmt){
                // "<!--" instead of "/*" or "//"
                cs -= 2;
            } else 
            if (c1.state == stateJSHTMLCloseCmt) {
                // "-->" instead of "/*" or "//"
                cs -= 1;
            }
            b.Write(s.slice(written, cs));
            written = i1;
        }
        if (isInScriptLiteral(c.state) && containsSpecialScriptTag(s.slice(i, i1))) {
            b.Write(s.slice(written, i));
            b.Write(escapeSpecialScriptTags(s.slice(i, i1)));
            written = i1;
        }
        if (i == i1 && c.state == c1.state) {
            throw panic(fmt.Sprintf("infinite loop from %v to %v on %q..%q"u8, c, c1, s.slice(0, i), s.slice(i)));
        }
        (c, i) = (c1, i1);
    }
    if (written != 0 && c.state != stateError) {
        if (!isComment(c.state) || c.delim != delimNone) {
            b.Write(n.Text.slice(written));
        }
        e.editTextNode(Ꮡn, b.Bytes());
    }
    return c;
}

// contextAfterText starts in context c, consumes some tokens from the front of
// s, then returns the context after those tokens and the unprocessed suffix.
internal static (context, nint) contextAfterText(context c, slice<byte> s) {
    if (c.delim == delimNone) {
        var (c1, iΔ1) = tSpecialTagEnd(c, s);
        if (iΔ1 == 0) {
            // A special end tag (`</script>`) has been seen and
            // all content preceding it has been consumed.
            return (c1, 0);
        }
        // Consider all content up to any end tag.
        return transitionFunc[c.state](c, s.slice(0, iΔ1));
    }
    // We are at the beginning of an attribute value.
    nint i = bytes.IndexAny(s, delimEnds[c.delim]);
    if (i == -1) {
        i = len(s);
    }
    if (c.delim == delimSpaceOrTagEnd) {
        // https://www.w3.org/TR/html5/syntax.html#attribute-value-(unquoted)-state
        // lists the runes below as error characters.
        // Error out because HTML parsers may differ on whether
        // "<a id= onclick=f("     ends inside id's or onclick's value,
        // "<a class=`foo "        ends inside a value,
        // "<a style=font:'Arial'" needs open-quote fixup.
        // IE treats '`' as a quotation character.
        {
            nint j = bytes.IndexAny(s.slice(0, i), "\"'<=`"u8); if (j >= 0) {
                return (new context(
                    state: stateError,
                    err: errorf(ErrBadHTML, default!, 0, "%q in unquoted attr: %q"u8, s.slice(j, j + 1), s.slice(0, i))
                ), len(s));
            }
        }
    }
    if (i == len(s)) {
        // Remain inside the attribute.
        // Decode the value so non-HTML rules can easily handle
        //     <button onclick="alert(&quot;Hi!&quot;)">
        // without having to entity decode token boundaries.
        for (var u = slice<byte>(html.UnescapeString(((@string)s))); len(u) != 0; ) {
            var (c1, i1) = transitionFunc[c.state](c, u);
            (c, u) = (c1, u.slice(i1));
        }
        return (c, len(s));
    }
    var element = c.element;
    // If this is a non-JS "type" attribute inside "script" tag, do not treat the contents as JS.
    if (c.state == stateAttr && c.element == elementScript && c.attr == attrScriptType && !isJSType(((@string)(s.slice(0, i))))) {
        element = elementNone;
    }
    if (c.delim != delimSpaceOrTagEnd) {
        // Consume any quote.
        i++;
    }
    // On exiting an attribute, we discard all state information
    // except the state and element.
    return (new context(state: stateTag, element: element), i);
}

// editActionNode records a change to an action pipeline for later commit.
internal static void editActionNode(this ref escaper e, ж<parse.ActionNode> Ꮡn, slice<@string> cmds) {
    {
        var (_, ok) = e.actionNodeEdits[Ꮡn, ꟷ]; if (ok) {
            throw panic(fmt.Sprintf("node %s shared between templates"u8, Ꮡn.OrTypedNil()));
        }
    }
    e.actionNodeEdits[Ꮡn] = cmds;
}

// editTemplateNode records a change to a {{template}} callee for later commit.
internal static void editTemplateNode(this ref escaper e, ж<parse.TemplateNode> Ꮡn, @string callee) {
    {
        var (_, ok) = e.templateNodeEdits[Ꮡn, ꟷ]; if (ok) {
            throw panic(fmt.Sprintf("node %s shared between templates"u8, Ꮡn.OrTypedNil()));
        }
    }
    e.templateNodeEdits[Ꮡn] = callee;
}

// editTextNode records a change to a text node for later commit.
internal static void editTextNode(this ref escaper e, ж<parse.TextNode> Ꮡn, slice<byte> text) {
    {
        var (_, ok) = e.textNodeEdits[Ꮡn, ꟷ]; if (ok) {
            throw panic(fmt.Sprintf("node %s shared between templates"u8, Ꮡn.OrTypedNil()));
        }
    }
    e.textNodeEdits[Ꮡn] = text;
}

// commit applies changes to actions and template calls needed to contextually
// autoescape content and adds any derived templates to the set.
internal static void commit(this ref escaper e) {
    foreach (var (name, _) in e.output) {
        e.template(name).Funcs(funcMap);
    }
    // Any template from the name space associated with this escaper can be used
    // to add derived templates to the underlying text/template name space.
    var tmpl = e.arbitraryTemplate();
    foreach (var (_, t) in e.derived) {
        {
            var (_, err) = (~tmpl).text.AddParseTree(t.Name(), (~t).Tree); if (err != default!) {
                throw panic("error adding derived template");
            }
        }
    }
    foreach (var (n, s) in e.actionNodeEdits) {
        ensurePipelineContains((~n).Pipe, s);
    }
    foreach (var (n, name) in e.templateNodeEdits) {
        n.Value.Name = name;
    }
    foreach (var (n, s) in e.textNodeEdits) {
        n.Value.Text = s;
    }
    // Reset state that is specific to this commit so that the same changes are
    // not re-applied to the template on subsequent calls to commit.
    e.called = new map<@string, bool>();
    e.actionNodeEdits = new map<ж<parse.ActionNode>, slice<@string>>();
    e.templateNodeEdits = new map<ж<parse.TemplateNode>, @string>();
    e.textNodeEdits = new map<ж<parse.TextNode>, slice<byte>>();
}

// template returns the named template given a mangled template name.
internal static ж<template.Template> template(this ref escaper e, @string name) {
    // Any template from the name space associated with this escaper can be used
    // to look up templates in the underlying text/template name space.
    var t = (~e.arbitraryTemplate()).text.Lookup(name);
    if (t == nil) {
        t = e.derived[name];
    }
    return t;
}

// arbitraryTemplate returns an arbitrary template from the name space
// associated with e and panics if no templates are found.
internal static ж<Template> arbitraryTemplate(this ref escaper e) {
    foreach (var (_, t) in (~e.ns).set) {
        return t;
    }
    throw panic("no templates in name space");
}

// Forwarding functions so that clients need only import this package
// to reach the general escaping functions of text/template.

// HTMLEscape writes to w the escaped HTML equivalent of the plain text data b.
public static void HTMLEscape(io.Writer w, slice<byte> b) {
    text.template_package.HTMLEscape(w, b);
}

// HTMLEscapeString returns the escaped HTML equivalent of the plain text data s.
public static @string HTMLEscapeString(@string s) {
    return text.template_package.HTMLEscapeString(s);
}

// HTMLEscaper returns the escaped HTML equivalent of the textual
// representation of its arguments.
public static @string HTMLEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return text.template_package.HTMLEscaper(args.ꓸꓸꓸ);
}

// JSEscape writes to w the escaped JavaScript equivalent of the plain text data b.
public static void JSEscape(io.Writer w, slice<byte> b) {
    text.template_package.JSEscape(w, b);
}

// JSEscapeString returns the escaped JavaScript equivalent of the plain text data s.
public static @string JSEscapeString(@string s) {
    return text.template_package.JSEscapeString(s);
}

// JSEscaper returns the escaped JavaScript equivalent of the textual
// representation of its arguments.
public static @string JSEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return text.template_package.JSEscaper(args.ꓸꓸꓸ);
}

// URLQueryEscaper returns the escaped value of the textual representation of
// its arguments in a form suitable for embedding in a URL query.
public static @string URLQueryEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return text.template_package.URLQueryEscaper(args.ꓸꓸꓸ);
}

} // end template_package
