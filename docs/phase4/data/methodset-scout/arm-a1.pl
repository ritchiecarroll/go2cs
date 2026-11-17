#!/usr/bin/perl
# arm-a1.pl METHODSET_CS : SCOUT ONLY -- MethodSet.Source answers null, so no type gets IGoMethodSet arms.
use strict; use warnings;
my $f = shift;
open(my $in, '<:raw', $f) or die $!; my $s = do { local $/; <$in> }; close $in;
my $n = ($s =~ s/(    public static string\? Source\(string ns, INamedTypeSymbol\? type, Compilation compilation, string generatedPart\)\r?\n    \{\r?\n)/$1        if (ns.Length >= 0) return null; \/\/ SCOUT A1: arms off\r\n/);
$n == 1 or die "anchor $n\n";
open(my $o, '>:raw', $f) or die $!; print $o $s; close $o; print "A1 applied\n";
