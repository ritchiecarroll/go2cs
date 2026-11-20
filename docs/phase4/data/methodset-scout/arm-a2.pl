#!/usr/bin/perl
# arm-a2.pl METHODSET_CS : SCOUT ONLY -- hoist the bound arms' captured receivers (ᴛr, ᴛp) to ᴛMethod's top scope, so
# C# mints ONE display class per type instead of one per case. Each case assigns instead of declaring.
use strict; use warnings;
my $f = shift;
open(my $in, '<:raw', $f) or die $!; my $s = do { local $/; <$in> }; close $in;
my $k = 0;
# 1. the binds assign, not declare
$k += ($s =~ s/Bound\(\$"\{box\} \{TempVarMarker\}p = \(\{box\}\)receiver!;"/Bound(\$"{TempVarMarker}p = ({box})receiver!;"/g);
$k += ($s =~ s/Bound\(\$"\{self\} \{TempVarMarker\}r = \(\{self\}\)receiver!;"/Bound(\$"{TempVarMarker}r = ({self})receiver!;"/g);
$k == 3 or die "bind rewrites: $k\n";
# 2. declare the captured receivers once, at the method's top, only when some arm binds them
my $anchor = '        body.Append("        switch ((method, pointer))\r\n");';
my $decl = '        if (arms.Any(arm => arm.bound?.StartsWith($"{TempVarMarker}r = ") == true)) body.Append($"        {self} {TempVarMarker}r;\r\n"); // SCOUT A2' . "\r\n"
         . '        if (arms.Any(arm => arm.bound?.StartsWith($"{TempVarMarker}p = ") == true)) body.Append($"        global::go.ж<{self}> {TempVarMarker}p;\r\n"); // SCOUT A2' . "\r\n";
my $m = ($s =~ s/(\Q$anchor\E)/$decl$1/);
$m == 1 or die "header anchor $m\n";
open(my $o, '>:raw', $f) or die $!; print $o $s; close $o; print "A2 applied\n";
