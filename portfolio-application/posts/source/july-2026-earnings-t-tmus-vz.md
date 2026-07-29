---
title: July 2026 Earnings for AT&T, T-Mobile, and Verizon
date: 2026.07.28
---

# July 2026 Earnings for AT&T, T-Mobile, and Verizon

In the week of July 20th, the big three American mobile network operators (MNOs) announced earnings for the second quarter of 2026. It was a pretty decent quarter for each of the carriers, and each beat Wall Street's estimates as shown below. T-Mobile was the only carrier to open lower after the news. In part this was because of 13% fewer new accounts created last quarter as compared to 2Q 2025, but that drop seems high to me given that they beat estimates by a healthy margin. And the S&P 500 was only down about one point on the week, so it isn't that T-Mobile got caught up in a larger drawdown.

| Company  | EPS Relative to Estimate | YoY Service Revenue Growth | Share Price Change After Earnings |
|----------|--------------------------|----------------------------|-----------------------------------|
| AT&T     | +10.1%                   | 2.7%                       | +4.3%                             |
| T-Mobile | +15.0%                   | 8.9%                       | -6.9%                             |
| Verizon  | +2.3%                    | 3.5%                       | +2.7%                             |

While all three are pretty direct competitors, this quarter's earnings calls showed interesting differences in company strategy and outlook on the future of the industry.

## Wireline vs. FWA Broadband

During his prepared remarks, the T-Mobile CEO commented that the company wanted to maintain a "capital envelope that is considerate of upcoming spectrum opportunities in both 2027 and 2028, including C-Band 2.0 and 2.7 gigahertz". As I've <a href="/post/t-mobile-fiber-ambitions">written before</a> T-Mobile is certainly active in wireline broadband, but when asked about capacity constraints in fixed-wireless access (FWA) broadband, T-Mobile's CEO commented that:

> ...given the rapid evolution in wireless technology, **especially in comparison with other forms of tech that are being used for broadband**, we believe this is a time to double down on wireless tech and really build out our network superiority, not just in 5G, but also 6G.

This is very different from AT&T's thesis on wireline and spectrum. AT&T CEO John Stankey commented "As I've said in the past, where we have fiber, we win with fiber and wireless" during his prepared remarks, continuing later in the Q&A with "I don't consider it [FWA] to be the optimal technology to serve fixed traffic over the long haul, and I have been pretty clear that, that's why we invest in fiber because that is the optimal technology to use."

I had assumed that bundling fiber and wireless service was more about preventing customer churn and that wireless backhaul operated pretty independently of their wireline service, but Stankey commented that AT&T was introducing small cell infrastructure that would take advantage of the company's fiber assets to connect to backhaul. Integrating an ISP's network into an MNO's seems time consuming and daunting, but perhaps AT&T is pulling this off at the margins. 

## Activity in the Spectrum Market 

Last quarter was relatively quiet on the spectrum front for AT&T. Last year the company [announced](https://about.att.com/story/2025/echostar.html) a deal to buy 600 MHz low-band and 3.45 GHz mid-band spectrum across nearly all of America in a $23 billion all-cash transaction, and the transfer was [approved by the FCC](https://www.fcc.gov/document/order-granting-att-echostar-applications) this quarter.[^boost-mobile] But Stankey was clear that AT&T wasn't planning another big spectrum purchase:

> I don't think that the notion of having to get these really dense national swatches of spectrum and painting it with a paint brush across the US is the game anymore. I think the game is using your infrastructure to penetrate where you need more density and then being very selective at where you go and get that broad paint brush of additional spectrum to add in.

Verizon was more involved, winning 82 licences of AWS-3 in [FCC auction 113](https://docs.fcc.gov/public/attachments/DA-26-633A2.pdf) at a cost of $3.2 billion.[^tmus-auction-113] They also announced a nationwide 'Verizon One' offering for bundled mobile and home broadband for $70. In the Q&A they explained that FWA will be used where Verizon doesn't already offer fiber, and this isn't always easy for the carrier to plan for: FWA takes advantage of underutilised spectrum in a location to serve broadband to a stationary location, but you need to have the cell-site capacity to serve the customer. Everything I've seen implies that not every 4G or 5G cell site has the capacity to serve FWA broadband. 

The reason that I bring this up is that you'd expect Verizon One to increase demand for FWA, but unlike T-Mobile, Verizon wasn't asked in the Q&A about relevant capacity limits. Perhaps the analysts on the call took Auction 113 as a signal that Verizon was serious about acquiring the spectrum they need for higher FWA demand, but this is something that would play out over future 5G spectrum auctions.

## AI Traffic

T-Mobile and AT&T have substantially different outlooks on how AI workloads will impact network traffic. From AT&T's prepared remarks:

> Today, industry research shows AI agents generate up to 450% more total traffic per task than a human performing the same work. Agentic adoption is projected to drive approximately 9x growth in enterprise traffic and approximately 7x growth in consumer traffic by 2035...We believe AT&T is the only provider building and investing in this infrastructure at the scale necessary today to support the demands a decade from now.

This might come from a Cisco report titled ["AI Impact on Wide Area Networks"](https://www.cisco.com/c/dam/en/us/solutions/collateral/artificial-intelligence/mass-scale-infrastructure/ai-network-traffic-report.pdf). This would be more believable if there was a breakdown of where this increase was coming from. Is this WAN traffic between the model user and the model vendor's network, traffic between cloud regions of a vendor's network, or traffic within said regions?[^cisco-citation] If much of the increased network traffic is within a model vendor's network then it doesn't change the capacity outlook for carriers very much. A lot of customer network requests to a server that ultimately writes something to a database will be replaced with network requests that ultimately call an LLM, I'm very sceptical that AI will increase MNO traffic growth six-fold. 

Luckily when T-Mobile was asked if they expected a similar increase in traffic due to AI workloads, CTO John Saw was similarly skeptical:

> With regards to AI traffic, I think it's important to understand where these AI workloads actually live. The current growth in AI is focused on model training, large-scale agentic automations and heavy back-end automations. A lot of them actually are confined to wireline transport networks and massive data centers. So this type of compute, **we have not seen putting a
material strain on mobile networks**. So we have not seen any surge in mobile traffic due to AI, even though every year, we see mobile traffic going up, regardless of AI or not...

Both AT&T and T-Mobile are talking their books:[^talking-book] AT&T is arguing that higher-capacity fiber will pay off, and T-Mobile is arguing that serving AI traffic won't require unusual spectrum investments to accomodate.

## Where the Carriers Agree

### TMUS

• Postpaid Average Revenue Per Account (“ARPA”) of $152.91 grew 2% year-over-year
• Postpaid net account additions of 277 thousand decreased 13% year-over-year
• Diluted earnings per share (“EPS”) of $2.99 grew 5.3% year-over-year

- 8.9% service revenue YoY growth

### T

- 20.4% YoY growth in EPS
- Consolidated revenue up 2.3%
- 8.9% service revenue YoY growth
- 67.9% increase in

### VZ

- 3.5% YoY service revenue growth
- 6.6% YoY grown of EPS up to $1.39
- Highest ever reported EBITDA margin
- "we are raising our adjusted EPS guidance to 6% to 7% growth for the year"
- "We also completed $1 billion of share repurchases in the quarter, bringing year-to-date buybacks to $3.5 billion, already ahead of our full-year commitment of at least $3 billion"

## AT&T

> As I've said in the past, where we have fiber, we win with fiber and wireless. And I expect that as we expand our funnel of new fiber
> locations, we'll drive strong growth in our converged customer base and financial results.

- In process of decomissioning copper voice and internet service
- Only mention of Spectrum was

John Stankey
> Today, industry research shows AI agents generate up to 450% more total traffic per task than a human performing the same work.
> Agentic adoption is projected to drive approximately 9x growth in enterprise traffic and approximately 7x growth in consumer traffic
> by 2035.

> We returned $4.1 billion to shareholders during the second quarter, including approximately $2.2 billion of share repurchases. We
> are on pace to repurchase nearly $1 billion of stock in July. And as John previously shared, we now expect to buy back approximately
> $10 billion of our shares in 2026. This compares to our prior target of $8 billion of share repurchases this year and represents a pull
> forward of our planned buybacks through 2028. Together, our planned share repurchases and expected dividend payments will
> total approximately $18 billion this year, which is essentially 100% of our outlook for free cash flow

Claim that the combination of fiber and MNO really matters

> First of all, the wireless network, you need to build better upstream.
> And so part of why we did the Spectrum acquisition we did and why we leaned into the 600 megahertz is we believe the best way
> to manage a robust upstream in an agentic environment is to have really strong low-band position.
> And we already have an advantaged low-band position in the market. The 600 is going to make that advantage even more substantial.
> And because you can engineer the Spectrum a little bit differently, given how those bands are set up, we intend to try to engineer
> a really robust upstream network that reaches deep into buildings and has a lot of consistency to it. And we think that's what the
> low band is going to allow us to do.
>
> The other thing that's really important, of course, is to get density in the network and owning fiber footprint allows us to get density.
> So we're now introducing into our network PON-fed, small cell infrastructure, taking advantage of all that PON infrastructure we
> put out there using the backhaul on the wireless infrastructure to get more radiating points deeper into the network.


> On your question on capital allocation. Look, this is a decision for the Board. It's not my decision to make exclusively. It's something
> this Board is actively engaged in, in a nonstop level. And certainly, the events of the last couple of months have flavored those
> discussions differently than they might have been a year ago.
> I would tell you sitting here today with what I view as being a very suppressed valuation on the stock, I probably have a bias that
> says, I'd like to buy more of it back because I think it's incredibly undervalued.

> If you think about how wireless networks have now evolved where there's a pretty healthy amount of spectrum that's out there on
> most cell towers. We're providing really, really good service. The pockets of where you need better density and more bandwidth
> are becoming more and more contained. We've built a lot on the interior. You go into a stadium you're not hitting the cell site
> outside. You're hitting infrastructure that's been deployed in the stadium. Same thing in the hospital. Same thing in the high-rise
> building.
> So now we're in a situation where you go outside of those locations where when you augment capacity, it's going to be much more
> targeted, and you're just fine in the broader macro. And if you look at how a typical cell site works, if there's three sectors on the
> cell site, oftentimes, when you hit exhaust, it's not because all three sectors have exhausted. It's because there's one face on that
> cell site that happens to point toward a densely populated area that has a park that is busy on a Saturday or it's a congregating
> area.
> And so now with technology where you can go in and do this, I don't think that the notion of having to get these really dense national
> swatches of spectrum and painting it with a paint brush across the US is the game anymore. I think the game is using your
> infrastructure to penetrate where you need more density and then being very selective at where you go and get that broad paint
> brush of additional spectrum to add in.

## TMUS

Craig Moffett MoffettNathanson asked:

> So Peter, you talked about interest in upper C-Band, and it makes me wonder, your FWA business, by our calculations, is now
> consuming most of the capacity or at least most of the usage on your network. And it is ostensibly a fallow capacity strategy. Can
> you just talk about how you see traffic growth over the next few years?
> AT&T talked yesterday about a huge, expected increase due to AI workloads. Do you expect the same thing? And if you think about
> the upper C-Band, is that to support FWA or is it to support AI workloads? And I'm just wondering how FWA fits into that future and
> particularly in the context of a sort of fallow capacity model.

Srini Gopalan and John Saw

> And where we sit, given the rapid evolution in wireless technology, especially
> in comparison with other forms of tech that are being used for broadband, we believe this is a time to double down on wireless
> tech and really build out our network superiority, not just in 5G, but also 6G.
> Knocking off a couple of other things that you talked about, yes, FWA does consume a fair amount of our capacity, but this is where
> the numerator-denominator issue comes in, right? It consumes a fair amount of our traffic today, which is not the same thing as a
> fair amount of our capacity, because our capacity is several times multiple of the traffic that we hold today. So we feel very comfortable
> with where we are with FWA today.

> With regards to AI traffic, I think it's important to understand where these AI workloads actually live. The current growth in AI is
> focused on model training, large-scale agentic automations and heavy back-end automations. A lot of them actually are confined
> to wireline transport networks and massive data centers. So this type of compute, **we have not seen putting a
material strain on mobile networks**. So we have not seen any surge in mobile
> traffic due to AI, even though every year, we see mobile traffic going up, regardless of AI or not, to support our mobility tonnage as
> well as fixed wireless.


> Turning to shareholder returns. In addition to our dividend, we're also excited to have repurchased an incremental $2.5 billion in
> Q2 and through July 17. If you step back, since beginning our share buyback program in late 2022, we have repurchased 253 million
> shares and reduced total shares outstanding to $1.07 billion.
> While we continue to execute share buybacks this year, including with our previously increased authorization, we are also thoughtfully
> maintaining a capital envelope that is considerate of upcoming spectrum opportunities in both 2027 and 2028, including C-Band
> 2.0 and 2.7 gigahertz, which represents an opportunity to further cement our network leadership position and provide increased
> value creation through, for example, additional 5G broadband capacity unlock.

## VZ

> In terms of satellite, we see no impact from satellite providers on our broadband penetration or broadband capabilities. And that's not surprising — it is very difficult for satellite providers to > provide a service that's even close to what we can provide. Today's satellites, their beams cover hundreds of square miles. Even at the very lowest broadband speeds — 100 Mbps down and 20 up — the > ceiling that they can provide service without degrading quality for everyone is very small.

> Even if you look five years out and assume these satellites are in place at very low Earth orbits with 1,000 Gbps down and 100 Gbps uplinks, it takes five to seven years for them to scale that out. And when they do, maybe they take their beam size down from hundreds of miles to 25 miles in suburbs and urban areas. But even in suburbs, the ceiling before they degrade service is 10 to 40 homes passed per square mile at 100 Mbps. If they try and compete against FWA at 300 Mbps, that ceiling drops to 5 to 20 homes per square mile, versus what we can do which is anywhere between 500 and 2,000. So we're 100 to 1,000 times more efficient. Our speeds and our capabilities are much higher. And it's basically not possible — because of physics, not because of execution, but because of physics — for satellite to effectively compete against a terrestrial network in either mobility or broadband in urban and suburban geographies, which is where 95 to 98% of our revenues are.

> The strength of our cash flow allows us to execute on all aspects of our capital allocation framework. Our first priority continues to be investing in the business. You saw us taking meaningful action to accomplish that in FCC Auction 113. We're very pleased to have obtained high-quality AWS-3 spectrum that will enhance our network experience in alignment with our commitment to our customers. In total, we acquired 82 licenses for approximately $3.2 billion, which is a slight discount to their original auction price from 2015. The licenses are complementary to our existing spectrum and can be deployed with no capital investment. This reflects our prudent approach to spectrum acquisition. We expect to have this additional spectrum deployed within weeks of the FCC issuing these licenses.

> Our third announcement is also quite consequential because it foreshadows where our revenue growth profile is going from here. We recently signed an agreement with Google valued at over $1 billion to use Verizon Dark Fiber to connect their data centers. We have other deals that we expect to announce by year-end that, taken together, are expected to be worth multiple billions of dollars in revenue over the next several years. These are long-duration, high-quality, contracted revenue streams from some of the most demanding infrastructure customers in the world.

> We believe that this is just the beginning. The build-out of AI infrastructure across the United States is one of the largest capital cycles of our lifetime, and Verizon is uniquely positioned to participate in it. We own one of the most extensive long-haul and metro fiber footprints in North America. We have spent decades building the kind of carrier-grade, low-latency, highly resilient transport network that hyperscalers need to connect compute to compute, model to model, and region to region. We built that infrastructure for a different era, but it has turned out to be exactly the right asset for this one.

> We are also in the early stages of retrofitting many of our central offices into data centers for inference edge computing, with multiple conversations underway with partners who are eager to utilize these power-ready and permitted locations. We are moving quickly to expand our TAM in the rapidly growing AI infrastructure market. The agreements we have signed are the leading edge of a strategy that will become a meaningful, incremental leg of growth for Verizon. We expect this initiative to noticeably contribute to our revenue growth starting next year and to grow substantially from there.

> I want to emphasize what that means in combination with everything else I've said today. In the back half of 2026, our mobility and broadband service revenue growth is expected to accelerate, with Q4 forecasted to grow by approximately 4% year over year. That acceleration is independent of the incremental AI infrastructure revenue that begins to layer into our results starting in 2027. Said differently, our core business is accelerating and a new revenue growth factor arrives on top of it next year. This is a very different revenue growth profile than Verizon has had in a very long time, and it is the foundation of why we believe that we are at the beginning of a multi-year growth story.

> But behind the scenes, it was clear there was a once-in-a-generation opportunity for Verizon to participate in the massive AI infrastructure build-out. As we talked to hyperscalers, alternative cloud providers, and enterprises, there's a need for ever-increasing compute power. Everybody knows that. And the way that compute power has evolved over the last year or so is — at first it was about optimizing racks, then combining racks within a data center to optimize compute power. And now it really is about how do you connect data center to data center to maximize the ever-exploding need for compute. While that data center connectivity demand is exploding, there's an equal amount of desire and demand to power inference models and applications that need ultra-low latency — like robotics or remote surgery, autonomous driving — and move that out into the edge as opposed to these big, massive data centers.

> Clearly our long-haul and metro fiber networks — whether dark or lit depends on the customer; some want it dark so they can do the electronics around it, some want it lit so we do all the servicing — are suddenly in tremendous demand for that data center connectivity. We have thousands of central offices, many of which we're taking copper out of, and we are retrofitting them to be remote data centers that are power-ready, permitted, fully redundant infrastructure. We did a small trial on that and sold out capacity in 24 hours. We're just announcing this partnership with Google for several of our dark fiber routes for well in excess of a billion dollars. The demand for these fiber routes that we currently have and are building is ultimately limited, and that capacity and pricing is something we're going to have to think carefully about.

## Outline

- Overview on financial performance
    - All of them did pretty well, provide color from the highlights
    - Annoyingly the MNOs make it hard to really compare against eachother
    - Not sure what explained TMUS slide
- Spectrum
    - AT&T only concerned with Echostar
        - "I don't think that the notion of having to get these really dense national swatches of spectrum and painting it with a paint brush across the US is the game anymore. I think the game is using your infrastructure to penetrate where you need more density and then being very selective at where you go and get that broad paint brush of additional spectrum to add in."
    - VZ, TMUS engaged in spectrum auctions this quarter
    - VZ: maybe more spectrum sensitive because of Verizon One
- AI traffic
    - AT&T: insane stat
    - Verizon deals: Google + inference
- What the carriers share
    - Importance of converged customers
    - Sattelite
    - Device costs being good for them mostly

[^boost-mobile]: It wasn't until I read more about this spectrum transaction and the commentary around it that I realised that Boost Mobile was A) owned by EchoStar and B) [was not an MVNO and operated its own network](https://www.lightreading.com/open-ran/boost-mobile-isn-t-a-going-concern-but-it-s-not-going-anywhere) until, as you could image, EchoStar sold the spectrum it was operating on. Steal of a deal to get to buy out an entire network's worth of spectrum in one fell swoop.
[^tmus-auction-113]: T-Mobile was also active in auction 113, but spend less than $300 million
[^cisco-citation]: It also isn't confidence inspiring that the citation is 'Cisco model'. If I was a buy side analyst I would not take this at face value.
[^talking-book]: As is thier wont, it is an earnings call after all.