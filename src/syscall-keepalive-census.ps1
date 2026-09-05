<#
.SYNOPSIS
    Static census guard for the syscall funnel keep-alive emission (RULING 2, 2026-08-30): every
    corpus .cs file's `var kN = ...;` temp-capture (k = U+1D0B, the converter's exclusive glyph for
    this emission) must be paired with exactly one matching `System.GC.KeepAlive(kN);` -- the
    call-site closure this arc's escalation implements.

.DESCRIPTION
    Regenerable and static: it takes any corpus root (a fresh seeded reconvert, or the committed
    src\core tree) and greps every .cs file for two patterns -- the temp declaration and its
    KeepAlive -- counting occurrences rather than hand-maintaining a site list. It does not build or
    execute anything; it is a pure text census over already-emitted output, meant to run BEFORE (and
    far more cheaply than) a full corpus build.

    What it protects against, and why a build alone cannot: an UNPROTECTED pointer-derived uintptr
    argument (`(uintptr)Ꮡx` with no capturing temp) is syntactically valid C# and compiles clean --
    it is exactly the original defect this arc exists to fix. A regression that silently narrows
    pointerDerivedArgSource's detection (or otherwise stops capturing a real site) would not fail the
    corpus build; it would only show up here, as a drop in the counted total or a name mismatch.

.PARAMETER CorpusRoot
    Directory to scan (recursively) for .cs files. Defaults to the committed src\core tree.

.NOTES
    TWO ARMS, one contract. Arm 1 counts the CONVERTER's emission (the kN temp and its KeepAlive).
    Arm 2 checks the same pin-lifetime contract in the listed HAND-OWNS, which the converter never
    emits into at all -- see its own banner below for the predicate, the explicit file list, and why
    the list is not a glob.

    Counts, never asserts a specific number -- the corpus grows and the count is expected to move.
    What must ALWAYS hold: arm 1's two counts match (every temp has exactly one KeepAlive, per file,
    by name-multiset) and its total is greater than zero; arm 2 reports zero unheld sites and a
    non-zero site total. Both vacuity checks are there for the same reason (a census that can report
    zero and call it clean has never been positive-controlled -- CLAUDE.md's own "a gate that has
    never been made to fail proves nothing"), and both arms have been made to fail: arm 1 by
    injecting one unpaired temp into zsyscall_linux_amd64.cs, arm 2 by deleting one KeepAlive and,
    separately, by stripping the two retention arguments from ConnectEx's rearmOverlapped call --
    each naming exactly the site touched, each restored byte-identical.
#>

param(
    [string] $CorpusRoot
)

. (Join-Path $PSScriptRoot '_paths.ps1')

if (-not $CorpusRoot) {
    $CorpusRoot = Join-Path $RepoRoot 'src\core'
}

if (-not (Test-Path -LiteralPath $CorpusRoot)) {
    Write-Error "Corpus root not found: $CorpusRoot"
    exit 1
}

$tempPattern = [regex]'var\s+(ᴋ\d+)\s*='
$keepAlivePattern = [regex]'System\.GC\.KeepAlive\((ᴋ\d+)\);'

# The funnel set the converter intercepts (src\go2cs\syscallKeepAliveAnalysis.go:
# syscallFunnelFuncNames x syscallFunnelPackagePaths), spelled for a TEXT scan:
#
#   * RawSyscall/RawSyscall6 joined the set on 2026-09-02 -- Go marks them with the same
#     //go:uintptrkeepalive directive as Syscall/Syscall6 (GOROOT syscall/syscall_linux.go:50,58),
#     and the converter had omitted them.
#   * The package qualifier is OPTIONAL. Every file that carries the LARGEST protected populations
#     is IN the syscall package itself and calls its funnels UNQUALIFIED -- so the previous
#     `syscall\.`-anchored form silently skipped syscall\linux\zsyscall_linux_amd64.cs (58 temps),
#     syscall\windows\zsyscall_windows.cs (187) and syscall\windows\syscall_windows.cs (21) --
#     scanning 83 of the corpus's 351 protected sites. Measured at the widening: over the syscall
#     package ALONE the old pattern reported ZERO temps and tripped this script's own vacuity check,
#     so a real unpaired temp injected into zsyscall_linux_amd64.cs was invisible to it. A census
#     that cannot see three quarters of its own subject is the vacuous-instrument shape this guard's
#     header warns about; the qualifier is optional now, and the scanned population is the one that
#     moves when the emission moves.
#   * The optional `\w+\.` also carries the ALIASED qualifier the converter mints for a shadowed
#     import (`Δsyscall.Syscall(`, internal\poll\linux\fd_writev_unix.cs), which a literal
#     `syscall\.` matched only by accident of substring.
#
# The leading `(?:^|[^\w])` is what keeps AllThreadsSyscall/runtime_doAllThreadsSyscall out: those
# are DIFFERENT contracts (//go:uintptrescapes, and in this corpus a hand-own that reaches no
# kernel at all), and a name-substring match would pull them in. Over-matching costs nothing here
# -- a scanned file with no temps and no KeepAlives passes trivially -- while under-matching is
# exactly the defect above, so the pattern errs wide on the qualifier and narrow on the name.
# darwin's funnels joined on 2026-09-05 (Q49): the lowercase libc trampoline funnels `syscall`,
# `syscall6`, `syscall6X`, `syscallX`, `syscallPtr`, `rawSyscall`, `rawSyscall6` (syscall_darwin.go),
# which the uppercase-only set above never named -- so syscall\darwin read GREEN at ZERO protected
# sites on every train (a file with no temps and no KeepAlives passes arm 1 trivially) while its
# 75 funnel call lines carried 101 pointer-derived arguments unheld. The `[^\w]` anchor keeps
# `syscall_syscall(` (internal/syscall/unix's linkname family, integer/handle arguments) apart from
# `syscall(`, exactly as it keeps AllThreadsSyscall apart from Syscall.
$funnelCallPattern = [regex]'(?:^|[^\w])(?:\w+\.)?(RawSyscall6?|SyscallN|Syscall(?:6|9|12|15|18)?|rawSyscall6?|syscall(?:6X?|X|Ptr)?)\('

# ARM 3 (2026-09-05, Q49): a pointer-derived argument INSIDE a funnel call's argument list that is
# not a captured `ᴛN` temp -- `(uintptr)Ꮡbox`, `(uintptr)_pN`, `(uintptr)@unsafe.Pointer.FromPinnedBox(`
# -- is the pre-fix emission shape: the kernel is handed a managed address nothing holds still.
# A `_pN` counts only when its nearest declaration is `@unsafe.Pointer _pN` or a `ж<…> _pN` box;
# mksyscall spells a bool through the same name (`uint32 _p0 = default!; … _p0 = 1;`) and that
# integer is not an address -- seven Windows sites read as raw by this arm's first form.
# Arm 1 cannot see it (it pairs temps with KeepAlives and a file with neither passes), which is the
# route-#8 shape one target over: darwin sat at zero temps, zero KeepAlives and zero findings while
# every one of its pointer-derived funnel arguments was raw. Positive-controlled at its landing: RED
# on the pre-cut tree's syscall\darwin (75 lines), green on windows and linux pre-cut, green on
# darwin after the cut. The glyphs are written as escapes, never literally (the PS 5.1 codepage trap).
$rawFunnelArgPattern = [regex]'\(uintptr\)(\u13D1[A-Za-z_][A-Za-z0-9_]*|_p\d+|@unsafe\.Pointer\.FromPinnedBox\()'


# A COMMENT is not an emission, and this is LOAD-BEARING for the widening rather than defensive.
# syscall\windows\dll_windows.cs documents the contract in its header by quoting the emission
# verbatim -- `var k0 = ...; ... GC.KeepAlive(k0);` -- so the widened pattern brings that file into
# the scanned set and its PROSE contributes one temp name that exists nowhere in the compiled code.
# Measured 2026-09-02: the widened pattern WITHOUT this stripping exits 1 at master naming exactly
# that file (352 temps vs 351 KeepAlives), a red on a file with no defect in it -- FALSE-GREEN
# route #8's family, inverted. Whole-line `//` comments are dropped before any pattern runs, so all
# three patterns see the same text.
#
# This is a TEXT census by design (its whole value is that it runs over ANY corpus root -- a fresh
# seeded reconvert or the committed tree -- with no converter and no build), so asserting the
# converter's own DECISION instead is not available here; the converter-side guard that does assert
# the decision is src\go2cs\syscallFunnelSet_test.go.
function Get-ScannableLines {
    param([string] $Text)

    # BLANKS comment lines rather than dropping them: the hand-own arm reports by LINE NUMBER, so
    # the index of every surviving line has to be the index it has in the file.
    return ($Text -split "`n") | ForEach-Object { if ($_.TrimStart() -like '//*') { '' } else { $_ } }
}

function Remove-LineComments {
    param([string] $Text)

    return (Get-ScannableLines -Text $Text) -join "`n"
}

# --- ARM 2: the HAND-OWN pin-holder arm -------------------------------------------------------
#
# Arm 1 above counts what the CONVERTER emits. A `[module: go.GoManualConversion]` file is dropped
# from the convert set, so it receives none of that emission and arm 1 can say nothing about it --
# yet the pin contract is identical: `(uintptr)<box>` calls golib's operator, which pins the managed
# storage for the LIFETIME OF THE BOX (ж.cs EnsureStableAddress -> a PinnedBuffer in the box's own
# m_pin field), and a hand-written body that lets the box die at the argument hands the kernel a
# relocatable buffer. dll_windows.cs states the division outright: a pointer-derived argument "is
# NOT resolved or pinned inside this file at all -- the caller's own converted statement does the
# work".
#
# So each `(uintptr)<box>` in a listed hand-own must be closed by one of the two shapes the corpus
# already uses:
#
#   * `System.GC.KeepAlive(<the same box>)` on a LATER line -- the synchronous shape, the hand
#     equivalent of convSyscallFunnelCall's own emission; or
#   * the box handed to the OverlappedOp retention plumbing (rearmOverlapped / stageBuffers /
#     retainForFlight, zsyscall_windows_wsa_impl.cs) -- the ASYNCHRONOUS shape, for a submit whose
#     flight begins where a KeepAlive would end. ConnectEx is the measured member: its lpOverlapped
#     may not be NULL, so the kernel's use of lpSendBuffer starts after the call returns.
#
# The file list is EXPLICIT, not a glob over every hand-own, and stays that way. A glob would fold
# in files whose pointer arguments are native stack images or unmanaged copies (the SAFE-by-copy
# population -- `exec_unix.cs`, `zsyscall_windows_wsa_impl.cs`'s own stackalloc staging), where the
# rule does not apply and every site would read as a finding. Members are added when a lane has
# actually classified the file.
#
# NEXT MEMBERS, named rather than added: syscall\linux\sockaddr_linux_impl.cs and
# internal\syscall\unix\linux\net_linux_impl.cs carry the same class on the Linux side (Recvfrom,
# GoRecvmsgNative, GoSendmsgNative, RecvfromInet4/6, sendtoNative). They are lane C2's cut and join
# this list when it lands -- listing them before the bodies are fixed would only manufacture a red.
#
# zsyscall_windows_privilege_impl.cs is listed and contributes ZERO sites, deliberately: its
# census row was RETRACTED on measurement (`_p0` there is a local uint32 holding 0/1 for
# disableAllPrivileges, not a pointer box; the other pointer arguments are native stack images).
# Keeping it listed makes that verdict re-checkable -- if a box argument ever appears in it, the
# arm reports it -- rather than resting on a note.
$handOwnPinFiles = @(
    'syscall\windows\zsyscall_windows_ptrout_impl.cs',
    'syscall\windows\syscall_windows_impl.cs',
    'internal\syscall\windows\windows\zsyscall_windows_ptrout_impl.cs',
    'internal\syscall\windows\windows\zsyscall_windows_privilege_impl.cs'
)

# Ꮡ is the converter's address-of glyph. Spelled as an escape, not as a literal: a .ps1 that
# lost its BOM is re-read by Windows PowerShell 5.1 through the system codepage, and an embedded
# non-ASCII literal then decodes to mojibake at PARSE time -- a regex that silently never matches
# and a guard that reports a clean zero (CLAUDE.md's own BOM trap, met from the detector side).
$handOwnBoxArgPattern = [regex]'\(uintptr\)(\u13D1[A-Za-z_][A-Za-z0-9_]*)'
$handOwnRetentionCalls = [regex]'(rearmOverlapped|stageBuffers|retainForFlight)\('

$totalTemps = 0
$totalKeepAlives = 0
$totalFunnelCalls = 0
$mismatchFiles = @()
$rawFunnelArgs = @()

$csFiles = Get-ChildItem -LiteralPath $CorpusRoot -Recurse -Filter '*.cs' -File |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj|Generated)[\\/]' }

foreach ($file in $csFiles) {
    $content = Remove-LineComments -Text ([System.IO.File]::ReadAllText($file.FullName))

    $funnelMatches = $funnelCallPattern.Matches($content)

    if ($funnelMatches.Count -eq 0) {
        continue
    }

    $totalFunnelCalls += $funnelMatches.Count

    # ARM 3: every funnel call's argument list, paren-matched from the name's `(`, scanned for a raw
    # pointer-derived argument. Hand-own files are arm 2's (their raw box arguments are paired with
    # their own KeepAlives there); this arm reads CONVERTED emission only.
    if ($file.Name -notlike '*_impl.cs' -and $content -notmatch '\[module:\s*(go\.)?GoManualConversion\]') {
        foreach ($funnelMatch in $funnelMatches) {
            $open = $funnelMatch.Index + $funnelMatch.Length - 1
            $depth = 0
            $close = -1

            for ($k = $open; $k -lt $content.Length; $k++) {
                $ch = $content[$k]

                if ($ch -eq '(') { $depth++ }
                elseif ($ch -eq ')') { $depth--; if ($depth -eq 0) { $close = $k; break } }
            }

            if ($close -lt 0) { continue }

            $argText = $content.Substring($open, $close - $open + 1)

            foreach ($raw in $rawFunnelArgPattern.Matches($argText)) {
                # A `_pN` temp is pointer-derived only when its nearest preceding declaration in the
                # file is `@unsafe.Pointer _pN` -- mksyscall spells a bool argument through the same
                # `_p0` name (`var _p0 uint32; if inheritHandle { _p0 = 1 }`), and `(uintptr)_p0`
                # over that integer is not an address (seven such sites on Windows, first form of
                # this arm, all false positives).
                if ($raw.Groups[1].Value -match '^_p\d+$') {
                    $before = $content.Substring(0, $funnelMatch.Index)
                    $decls = [regex]::Matches($before, '(?m)^\s*(\S+)\s+' + [regex]::Escape($raw.Groups[1].Value) + '\s*=\s*default!;')

                    # Pointer-derived when the temp is a Pointer OR a box (`ж<byte> _p0` from
                    # BytePtrFromString, whose `(uintptr)_p0` is the box's pinned address).
                    $declType = if ($decls.Count -gt 0) { $decls[$decls.Count - 1].Groups[1].Value } else { '' }

                    if ($declType -ne '@unsafe.Pointer' -and -not $declType.StartsWith("`u{0436}<")) {
                        continue
                    }
                }

                $lineNumber = ($content.Substring(0, $funnelMatch.Index) -split "`n").Count
                $rawFunnelArgs += "$(Get-RelativeDisplayPath -Path $file.FullName -Root $CorpusRoot):$lineNumber  $($funnelMatch.Groups[1].Value)(... $($raw.Value) ...)  -- raw pointer-derived argument, not a captured temp"
            }
        }
    }

    $tempMatches = $tempPattern.Matches($content)
    $keepAliveMatches = $keepAlivePattern.Matches($content)

    $totalTemps += $tempMatches.Count
    $totalKeepAlives += $keepAliveMatches.Count

    $tempNames = ($tempMatches | ForEach-Object { $_.Groups[1].Value } | Sort-Object) -join ','
    $keepAliveNames = ($keepAliveMatches | ForEach-Object { $_.Groups[1].Value } | Sort-Object) -join ','

    if ($tempNames -ne $keepAliveNames) {
        $mismatchFiles += Get-RelativeDisplayPath -Path $file.FullName -Root $CorpusRoot
    }
}

# --- ARM 2 execution: the hand-own pin-holder sites --------------------------------------------
$handOwnSites = 0
$handOwnHeld = 0
$handOwnUnheld = @()
$handOwnMissingFiles = @()

foreach ($relative in $handOwnPinFiles) {
    $path = Join-Path $CorpusRoot $relative

    if (-not (Test-Path -LiteralPath $path)) {
        # A listed file that is absent is a FINDING, never a silent skip: a renamed or deleted
        # hand-own must not quietly take its sites out of the census.
        $handOwnMissingFiles += $relative
        continue
    }

    $lines = @(Get-ScannableLines -Text ([System.IO.File]::ReadAllText($path)))

    for ($i = 0; $i -lt $lines.Count; $i++) {
        foreach ($match in $handOwnBoxArgPattern.Matches($lines[$i])) {
            $box = $match.Groups[1].Value
            $handOwnSites++

            # The KeepAlive must come AFTER the call: one placed before it holds nothing across
            # the boundary, which is the whole property being asserted.
            $keepAlive = 'System.GC.KeepAlive(' + $box + ')'
            $held = $false

            for ($j = $i + 1; $j -lt $lines.Count; $j++) {
                if ($lines[$j].Contains($keepAlive)) {
                    $held = $true
                    break
                }
            }

            if (-not $held) {
                # The asynchronous alternative: the box handed to the OverlappedOp retention
                # plumbing, which holds it for the flight instead.
                foreach ($line in $lines) {
                    if ($handOwnRetentionCalls.IsMatch($line) -and $line.Contains($box)) {
                        $held = $true
                        break
                    }
                }
            }

            if ($held) {
                $handOwnHeld++
            }
            else {
                $handOwnUnheld += "$relative`:$($i + 1)  (uintptr)$box  -- no later System.GC.KeepAlive($box) and not retained for flight"
            }
        }
    }
}

Write-Host "syscall funnel keep-alive census: $($csFiles.Count) .cs file(s) scanned under $CorpusRoot"
Write-Host "  ARM 1 -- converter emission"
Write-Host "    funnel call occurrences (informational -- includes non-pointer-arg calls): $totalFunnelCalls"
Write-Host "    captured temps (var kN = ...;):   $totalTemps"
Write-Host "    matching KeepAlive calls:         $totalKeepAlives"
Write-Host "  ARM 3 -- raw pointer-derived funnel arguments in converted emission"
Write-Host "    UNHELD:                           $($rawFunnelArgs.Count)"
Write-Host "  ARM 2 -- hand-own pin holders ($($handOwnPinFiles.Count) listed file(s))"
Write-Host "    (uintptr)<box> argument sites:    $handOwnSites"
Write-Host "    held across the call:             $handOwnHeld"
Write-Host "    UNHELD:                           $($handOwnUnheld.Count)"

foreach ($finding in $handOwnUnheld) {
    Write-Host "      $finding"
}

if ($handOwnMissingFiles.Count -gt 0) {
    Write-Error "HAND-OWN ARM: $($handOwnMissingFiles.Count) listed file(s) not found under $CorpusRoot -- the list is stale, and a missing file silently removes its sites from the census:`n$($handOwnMissingFiles -join "`n")"
    exit 1
}

if ($rawFunnelArgs.Count -gt 0) {
    Write-Host ""
    Write-Host "CENSUS RED (arm 3): $($rawFunnelArgs.Count) raw pointer-derived funnel argument(s) -- the kernel is handed a managed address nothing holds still:"
    $rawFunnelArgs | ForEach-Object { Write-Host "    $_" }
    exit 1
}

if ($handOwnUnheld.Count -gt 0) {
    Write-Error "HAND-OWN ARM: $($handOwnUnheld.Count) pointer-derived argument(s) hand the kernel a managed address with nothing holding the box that pins it:`n$($handOwnUnheld -join "`n")"
    exit 1
}

if ($mismatchFiles.Count -gt 0) {
    Write-Error "MISMATCH: $($mismatchFiles.Count) file(s) have a temp/KeepAlive name-multiset mismatch:`n$($mismatchFiles -join "`n")"
    exit 1
}

if ($totalTemps -ne $totalKeepAlives) {
    Write-Error "MISMATCH: total temps ($totalTemps) != total KeepAlives ($totalKeepAlives) -- counted equal per file but not corpus-wide, which should be impossible; re-check the patterns"
    exit 1
}

if ($totalTemps -eq 0) {
    Write-Error "CENSUS IS VACUOUS: zero captured temps found anywhere under $CorpusRoot -- positive control failed (either this corpus predates the fix, or CorpusRoot is wrong)"
    exit 1
}

if ($handOwnSites -eq 0) {
    Write-Error "HAND-OWN ARM IS VACUOUS: zero (uintptr)<box> sites found across $($handOwnPinFiles.Count) listed file(s) -- either the corpus changed shape or the glyph escape stopped matching; an arm that can only report zero has never been positive-controlled"
    exit 1
}

Write-Host "CENSUS CLEAN: arm 3 -- no raw pointer-derived funnel argument in converted emission; arm 1 -- every captured temp has exactly one matching KeepAlive, $totalTemps site(s) protected; arm 2 -- all $handOwnSites hand-own pointer argument(s) held across their call."
exit 0
