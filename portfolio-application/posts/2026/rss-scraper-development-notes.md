---
title: RSS Scraper Development Notes
date: 2026.05.05
---

# RSS Scraper Development Notes

I wasn't old enough to mourn the loss of Google Reader, but a few months back I started self-hosting an instance of
FreshRSS on my VPC because iCloud RSS sync wasn't very reliable. The only issue I can think of with FreshRSS is that the
browser UI is a little clunky, and it wasn't until I found
[this GitHub issue](https://github.com/Ranchero-Software/NetNewsWire/issues/3731) that I could get it working with my
reader. But those are incredibly small issues, because the synchronisation has worked without any issue whatsoever. I
use NetNewsWire as an RSS reader application, and based off of the language line count in its
[repository](https://github.com/Ranchero-Software/NetNewsWire), it is clearly a native Swift application. Right now it
is using 238 MB of RAM on my machine and while it really should be possible to use less I have very little room to
complain because Visual Studio Code uses over 1500 MB when only a single file is open.[^1] RAM aside, the application
works well and I see no reason to use anything else on iOS or macOS.

[^1]: This is not the only reason I don't use VS Code, but even by itself this would be reason enough to not use it.
