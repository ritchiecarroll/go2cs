#!/usr/bin/perl
# Frozen-object name collision census (ILC names a preinitialized object <mangled owning type><allocation site id>,
# with no separator). For every package and GOOS flavour, find struct pairs A and A<digits> and predict, from the
# per-field accessor tokens (`static readonly FieldRefFunc<S,F> ᏑF‿ = ᏑF;`, one newobj per field at interpreted
# instruction 3 + 4k), whether some site id of A equals <digits> followed by a site id of A<digits>.
#   usage: frozen-census.pl <src/core root>
# Flavour model: a file belongs to GOOS g when it sits in <pkg>/<g>/ (layout L3) or, at the package root, when its name
# carries no _<other GOOS> marker. Field model: non-blank field declarations in the struct body (one per line).
use strict; use warnings; use File::Find;
my $root = shift or die "usage: frozen-census.pl <src/core>\n";
my @goos = qw(windows linux darwin);
my %structs;   # {pkg}{goos}{name} = field count

my @files;
find({ no_chdir => 1, wanted => sub {
  return if $File::Find::dir =~ m{[\\/](bin|obj|Generated)([\\/]|$)};
  push @files, $File::Find::name if /\.cs$/ && !/\.cs\.auto$/;
}}, $root);

for my $f (@files) {
  (my $rel = $f) =~ s{^\Q$root\E[\\/]?}{}; $rel =~ s{\\}{/}g;
  my @parts = split m{/}, $rel; my $file = pop @parts;
  next unless @parts;
  my @fl;
  if (@parts >= 2 && grep { $_ eq $parts[-1] } @goos) { @fl = ($parts[-1]); pop @parts; }
  else {
    @fl = grep { my $g = $_; !grep { $file =~ /_\Q$_\E(_|\.|$)/ } grep { $_ ne $g } @goos } @goos;
  }
  my $pkg = join '/', @parts;
  open my $h, '<:raw', $f or next; my @lines = <$h>; close $h;
  for (my $i = 0; $i < @lines; $i++) {
    next unless $lines[$i] =~ /^\s*(?:public |internal |private )?(?:readonly )?partial struct (\w+)\s*(?:<[^>]*>)?\s*\{\s*\r?$/;
    my $name = $1; my $n = 0;
    for (my $j = $i + 1; $j < @lines; $j++) {
      last if $lines[$j] =~ /^\}/;
      next unless $lines[$j] =~ /^\s+(?:public |internal |private )?(?:readonly )?[\w.<>\[\],ж ]+?\s+(\w+)\s*(?:=[^;]*)?;\s*\r?$/;
      my $field = $1; next if $field =~ /^_+$/;
      $n++;
    }
    $structs{$pkg}{$_}{$name} = $n for @fl;
  }
}

my @ids = sub { my $n = shift; map { 3 + 4 * $_ } 0 .. $n - 1 };
my ($pairs, $hits) = (0, 0);
for my $pkg (sort keys %structs) {
  for my $g (@goos) {
    my $s = $structs{$pkg}{$g} or next;
    for my $b (sort keys %$s) {
      next unless $b =~ /^(.*?)(\d+)$/; my ($a, $digits) = ($1, $2);
      next unless exists $s->{$a};
      $pairs++;
      my @ia = map { 3 + 4 * $_ } 0 .. $s->{$a} - 1;
      my @ib = map { 3 + 4 * $_ } 0 .. $s->{$b} - 1;
      my %ida = map { $_ => 1 } @ia;
      my @collide = grep { $ida{"$digits$_"} } @ib;
      my $tag = @collide ? 'COLLIDES' : 'pair';
      $hits++ if @collide;
      printf "%-8s %-7s %-34s %-40s fields %2d / %2d %s\n", $tag, $g, $pkg, "$a / $b", $s->{$a}, $s->{$b},
        @collide ? "ids: " . join(',', map { "$a$digits$_" } @collide) : '';
    }
  }
}
print "pairs=$pairs predicted_collisions=$hits (package+GOOS flavour rows)\n";
