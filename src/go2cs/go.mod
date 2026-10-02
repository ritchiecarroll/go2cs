module go2cs

go 1.24.13

require (
	github.com/ritchiecarroll/hashset v0.0.0-00010101000000-000000000000
	golang.org/x/mod v0.33.0
	golang.org/x/tools v0.42.0
)

require golang.org/x/sync v0.19.0 // indirect

// LOCAL ONLY while the hashset cut is gated; dropped for the v1.0.0 tag.
replace github.com/ritchiecarroll/hashset => /home/user/hashset
