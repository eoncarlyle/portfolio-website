---
title: July 2026 Earnings for AT&T, T-Mobile, and Verizon
date: 2026.07.29
---

# July 2026 Earnings for AT&T, T-Mobile, and Verizon

In the week of July 20th, the big three American mobile network operators (MNOs) announced earnings for the second quarter of 2026. It was a pretty decent quarter for each of the carriers, and each beat Wall Street's estimates as shown below. T-Mobile was the only carrier to open lower after earnings; in part this was due to the company adding new accounts at a slower rate. But that drop seems high to me given that they beat estimates by a healthy margin. The S&P 500 was only down about one point on the week, so it isn't that T-Mobile got caught up in a larger drawdown.

| Company  | EPS Relative to Estimate | YoY Service Revenue Growth | Share Price Change After Earnings |
|----------|--------------------------|----------------------------|-----------------------------------|
| AT&T     | +10.1%                   | 2.7%                       | +4.3%                             |
| T-Mobile | +15.0%                   | 8.9%                       | -6.9%                             |
| Verizon  | +2.3%                    | 3.5%                       | +2.7%                             |


This quarter's earnings calls showed interesting differences in company strategy and industry outlook that are worth breaking down.

## Wireline vs. FWA Broadband

During his prepared remarks, the T-Mobile CEO commented that the company wanted to maintain a "capital envelope that is considerate of upcoming spectrum opportunities in both 2027 and 2028". As I've <a href="/post/t-mobile-fiber-ambitions">written before</a>, T-Mobile is certainly active in fiber broadband, but when asked about capacity constraints in fixed-wireless access (FWA) broadband, T-Mobile's CEO commented that:

> ...given the rapid evolution in wireless technology, **especially in comparison with other forms of tech that are being used for broadband**, we believe this is a time to double down on wireless tech and really build out our network superiority, not just in 5G, but also 6G.

This is very different from AT&T's home broadband thesis. Their CEO, John Stankey, commented in his prepared remarks that "As I've said in the past, where we have fiber, we win with fiber and wireless", continuing later in the Q&A with "I don't consider it [FWA] to be the optimal technology to serve fixed traffic over the long haul, and I have been pretty clear that, that's why we invest in fiber because that is the optimal technology to use." He also commented that AT&T was introducing small cell infrastructure that would take advantage of the company's fiber assets to connect to backhaul.

This surprised me, because I had assumed that bundling fiber and wireless service was entirely about preventing customer churn. Operating an integrated network for wireless backhaul and fiber-to-the-home would require a lot of work especially for bringing in networks of acquired fiber ISPs, but perhaps AT&T has made real progress on this front. This makes sense in light of the fiber first company perogative.

## Activity in the Spectrum Market 

Last quarter was relatively quiet on the spectrum front for AT&T. Last year the company [announced](https://about.att.com/story/2025/echostar.html) a deal to buy 600 MHz low-band and 3.45 GHz mid-band spectrum across nearly all of America in a $23 billion all-cash transaction, and the transfer was [approved by the FCC](https://www.fcc.gov/document/order-granting-att-echostar-applications) this quarter.[^boost-mobile] But Stankey was clear that AT&T wasn't planning another big spectrum purchase:

> I don't think that the notion of having to get these really dense national swatches of spectrum and painting it with a paint brush across the US is the game anymore. I think the game is using your infrastructure to penetrate where you need more density and then being very selective at where you go and get that broad paint brush of additional spectrum to add in.

Verizon was more involved, winning 82 licences of AWS-3 in [FCC auction 113](https://docs.fcc.gov/public/attachments/DA-26-633A2.pdf) at a cost of $3.2 billion.[^tmus-auction-113] They also announced a nationwide 'Verizon One' offering for bundled mobile and home broadband for $70. In the Q&A they explained that FWA will be used where Verizon doesn't already offer fiber, and this isn't always easy for the carrier to plan for: FWA takes advantage of underutilised spectrum in a location to serve broadband at a given stationary address, but you need to have the cell-site capacity to serve the customer. Everything I've seen implies that not every 4G or 5G cell site has the capacity to serve FWA broadband. 

The reason that I bring this up is that you'd expect Verizon One to increase demand for FWA, but unlike T-Mobile, Verizon wasn't asked in the Q&A about relevant capacity limits. Perhaps the analysts on the call took Auction 113 as a signal that Verizon was serious about acquiring the spectrum they need for higher FWA demand. Future 5G spectrum auctions will tell us if this is the case.

## AI & Networks

T-Mobile and AT&T have substantially different outlooks on how AI workloads will impact network traffic. From AT&T's prepared remarks:

> Today, industry research shows AI agents generate up to 450% more total traffic per task than a human performing the same work. Agentic adoption is projected to drive approximately 9x growth in enterprise traffic and approximately 7x growth in consumer traffic by 2035...We believe AT&T is the only provider building and investing in this infrastructure at the scale necessary today to support the demands a decade from now.

The industry research cited likely comes from a Cisco report titled ["AI Impact on Wide Area Networks"](https://www.cisco.com/c/dam/en/us/solutions/collateral/artificial-intelligence/mass-scale-infrastructure/ai-network-traffic-report.pdf). This would be more believable if Cisco broke down where the increase is coming from: is this WAN traffic between the model user and the model vendor's network, traffic between cloud regions of a vendor's network, or traffic within said regions?[^cisco-citation] If much of the increased network traffic is _within_ a model vendor then it doesn't change the capacity outlook for carriers all that much. A lot of user network traffic that ultimately writes something to a database somewhere will be replaced with traffic that ultimately calls some LLM somewhere. Traffic will probably increase, but I'm very sceptical that AI will increase MNO traffic growth six-fold. 

Luckily when T-Mobile was asked if they expected a similar increase in traffic due to AI workloads, CTO John Saw was also sceptical:

> With regards to AI traffic, I think it's important to understand where these AI workloads actually live. The current growth in AI is focused on model training, large-scale agentic automations and heavy back-end automations. A lot of them actually are confined to wireline transport networks and massive data centers. So this type of compute, **we have not seen putting a
material strain on mobile networks**. So we have not seen any surge in mobile traffic due to AI, even though every year, we see mobile traffic going up, regardless of AI or not...

Both AT&T and T-Mobile are talking their books:[^talking-book] AT&T is arguing that higher-capacity fiber will pay off, and T-Mobile is arguing that serving AI traffic won't require unusual spectrum investments to accommodate.

The most interesting AI announcements came from Verizon. The first was that they signed a $1 billion agreement for Google to use their dark fiber, and they hinted that similar fiber deals worth "multiple billions of dollars in revenue" were in the works. The second is more interesting: at a time when permitting and access to electricity are throwing sand into the gears of the AI buildout, Verizon has many facilities ripe for retrofitting:

> We have thousands of central offices, many of which we're taking copper out of, and we are retrofitting them to be remote data centers that are power-ready, permitted, fully redundant infrastructure. We did a small trial on that and sold out capability in 24 hours... The demand for these fiber routes that we currently have and that we are building is ultimately limited, and that capacity and pricing, we're going to have to think about how we handle the demand for that. We're clearly an attractive player with differentiated assets that are in high demand. We've been building carrier-grade fiber routes for decades. **We know how to get it done, we understand permitting, we do our own construction, and we have a solid balance sheet and a solid service reputation that the AI ecosystem needs and counts on. The revenues that we're talking about are meaningful, they've got margins that are equal to or greater than our existing margin structures, and they will begin to impact our revenues and margins beginning next year and grow substantially over the next five to ten years**

Unlike new data centre projects, this physical plant is likely completely depreciated and these facilities are only a few weeks away from serving workloads.

## Where the Carriers Agree

There is plenty that the carriers see eye-to-eye on. All three spoke to how important it is to grow the share of mobile customers who bundle their home internet, be it fiber or otherwise. And device prices have driven down churn: while carriers subsidise device prices, a large portion of higher RAM costs are still passed onto consumers. These higher prices have persuaded many to stick with the phones they already have, taking away a reason to switch carriers.

All three carriers were emphatic that their terrestrial infrastructure could compete with satellite networks. AT&T only sees value in satellite communications for the ~2% of traffic remote enough that it can't be well served by other means, with both T-Mobile and Verizon arguing that the large beam sizes of modern satellites make it impossible for satellite network providers to compete with FWA in suburban or urban markets. While satellite networking is very hard, MNO leadership seems to take it as a given that the satellite state-of-the-art can't be improved upon much more; I'm sure people made similar arguments that early cellular technology wasn't going to get much better than slightly worse than a landline.


[^boost-mobile]: It wasn't until I read more about this spectrum transaction and the commentary around it that I realised that Boost Mobile was A) owned by EchoStar and B) [was not an MVNO and operated its own network](https://www.lightreading.com/open-ran/boost-mobile-isn-t-a-going-concern-but-it-s-not-going-anywhere) until, as you could imagine, EchoStar sold the spectrum it was operating on. Steal of a deal to get to buy out an entire network's worth of spectrum in one fell swoop.
[^tmus-auction-113]: T-Mobile was also active in auction 113, but spent less than $300 million
[^cisco-citation]: It also isn't confidence inspiring that the citation is 'Cisco model'. If I was a buy side analyst I would not take this at face value.
[^talking-book]: As is their wont, it is an earnings call after all.