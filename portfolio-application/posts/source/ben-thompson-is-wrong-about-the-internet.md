---
title: Ben Thompson is wrong about the internet
date: 2026.10.26
---

# Ben Thompson is wrong about the internet

From [this morning's
Stratechery](https://stratechery.com/2026/the-end-of-the-internet/), emphasis
mine:

> The saving factor previously is that attackers were limited by human time
> and ingenuity as well; AI, however, has limitless time and the ability to
> substitute iteration for cleverness — and it’s getting pretty clever as well!
> It’s not a surprise that this wave of vulnerabilities is coming to light, and
> it’s going to get worse before it gets better — which I do, for the record,
> expect to happen; after all, the good guys have AI as well, and in the long run
> software will be better when it is (re)written by AI, and defended by agents
> continuously.
> <br><br>
> At the same time, what this entails is that the public Internet, to an even
> greater extent than now, will be a battle zone at best (i.e. social media), and
> a wasteland at worst (everything else). Some of this is economic, as AI consumes
> everything — both content and attention — without giving any of the traffic back
> that Google did, but it is also technical: **it was already a dodgy proposition to
> stand up your own site or app, which was the very promise of the Internet, given
> the hacking risks; in the age of AI it’s madness.**

While I was still working in a customer-facing job and trying to break into
engineering, I remember a frustrating late night in 2021 trying to get
[Kanboard](https://kanboard.org/) working on a Debian virtual machine I recently
set up on Linode. Pre-LLM this meant command-tabbing between Stack Overflow and
reading error messages to understand what was going wrong. I'm positive that
there was a way to get Kanboard working on my existing virtual machine, but
because the default Debian package registry didn't include a dependency required
by Kanboard I gave up and started with a fresh Ubuntu VM; the new machine worked
without too much ceremony. It wasn't the last time that a Linux infrastructure
issue outside of work tested my patience, but it taught me a lot about computers
and is partially responsible for why I was able to break into computing. To this
day I have a personal website, a FreshRSS server, and a few other services open
to the wider internet. I have many more internally-facing self-hosted things
that are gated behind [Authelia](https://www.authelia.com/).

Ben isn't wrong about everything he wrote today, and self-hosting isn't a good
idea for everyone, but if you are an engineer making your living with computers,
it is not "madness" to self-host to the wider internet. If you take the time to
[read a
book](https://www.pearsonitcertification.com/store/red-hat-rhcsa-9-cert-guide-ex200-9780138096274)
on system administration, patch your OS and services routinely, and run
relatively normal technologies, you are not putting yourself into harm's way. It
is a bad idea to run a server out of your home network[^vlan], but
[plenty](https://www.hetzner.com/) [of](https://www.digitalocean.com/)
[companies](https://www.akamai.com/) are more than happy to charge $7 a month or
less for a virtual machine to host on. If a state actor is hell-bent on
compromising your personal blog and media server you're probably done for
regardless, but the overwhelming majority of attackers run scaled operations to
pick the low-hanging fruit of services that haven't been patched in years. And
unless you're doing something insane like filing your taxes on the same server
you expose to the wider internet, it is not the end of the world if your
DigitalOcean virtual machine is compromised.

An internet where no one bothers to set up their own VM and try something
impossible on a managed service is a worse internet. Making
[Pressable](https://pressable.com/) handle your WordPress site is what you
should do if you are Ben Thompson. But people on the .NET team at Microsoft,
maintainers of Apache, and developers working on Django all take security
seriously: all of these actors have incentives to keep attackers at bay and
quickly respond to incidents. So when managing your own infrastructure you get
to rely on a _lot_ of people who have already solved the hard problems, and
they're also using machine intelligence to raise the bar on security.

Ben Thompson's "End of the Internet" may become a self-fulfilling prophecy if we
all retreat behind walled gardens instead of learning how the infrastructure
we rely on actually works. Decline is a choice, and the "End of the Internet"
is not inevitable.

[^vlan]: Doing so on a dedicated VLAN is of course fine, but that is not what I'm
    talking about here.
