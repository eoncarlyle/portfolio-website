---
title: Mobile Network Operators and Capex Tradeoffs
date: 2024.11.10
---
# Mobile Network Operators and Capex Tradeoffs

A discussion of why being a wireless carrier is such a tough business, and what lessons about the industry from both sides of the Atlantic can
teach us about electrical utilities.

## Wireless Carrier Challenges

It can't be fun to be a wireless carrier. Given that it's corporate earning season in the US, we can see Alphabet (Google's parent company)
and Meta Platforms had 2024 Q3 net incomes of $26 and $15 billion while Verizon and T-Mobile  brought in a measly $3.4 and $3.1 billion this quarter.
This would take some work to explain to someone in 1970: the companies
that built continent-scale networks for wireless customers to access an incredible wealth of services and publicly
available information are less profitable than the services _enabled_ by said networks. Ben Thompson's aggregation
theory explains the Google and Meta side of the equation: [^aggregation-theory]

> The value chain for any given consumer market is divided into three parts: suppliers, distributors, and
> consumers/users. The best way to make outsize profits in any of these markets is to either gain a horizontal monopoly in
> one of the three parts or to integrate two of the parts such that you have a competitive advantage in delivering a
> vertical solution. In the pre-Internet era the latter depended on controlling distribution. The fundamental disruption
> of the Internet has been to turn this dynamic on its head...the Internet has made distribution (of digital goods)
> free, neutralizing the advantage that pre-Internet distributors leveraged to integrate with suppliers. Secondly, the
> Internet has made transaction costs zero, making it viable for a distributor to integrate forward with end
> users/consumers at scale.

Meta Platforms jealously guards the exact number of servers at their disposal to enable a significant fraction of
humanity to have an account on Facebook, Instagram, or WhatsApp, but the answer is "at least one
million".[^hunting-heisenbugs] This enables incredible economies of scale when it comes to purchasing hardware,
operating data centres, and writing purpose-built software to administer their services. For both Google and Meta, this
is a large part of what enables the zero transaction costs at the heart of their aggregator business model.

Carriers/mobile network operators (MNOs) do not have this luxury. For example, the LTE wireless standard is optimised
for performance at a 3.1 mile cell radius, which requires cell towers roughly 5.3 miles apart from each other in a
hexagonal grid [^cox]. While Google and Meta can spread workloads across planet-scale fleets of servers and network
infrastructure, carriers need to have cell sites wherever they intend to serve customers: the modern consumer expects
and modern life demands cell coverage in all but the most remote corners of America. As of December 31st of 2023,
this meant that T-Mobile operated 128,000 cell sites in order to reach 98% of Americans.[^t-mobile-2023-10K] Building
cellular networks that reach more than 90% of American households means providing service in sparsely populated places where the unit economics
of building towers is far worse, but it's my understanding that American MNOs have a limited ability to increase prices in
these harder to serve areas.

When the aggregators want to increase their network throughput, they can do so without really impacting one another. But
not every piece of the radio spectrum can facilitate efficient wireless communication, and wireless communication
standards require exclusive access to a defined band of radio spectrum to work correctly. These spectrum bands are
generally auctioned by the relevant telecommunications regulator in a jurisdiction, which is the FCC in the United
States. The FCC has a rather archaic website for showing this, but a search for T-Mobile's FCC Registration Number shows
that they own cellular licence KNKN557 for the A channel block near Myrtle Beach, South Carolina.[^KNKN557] Once a carrier has won
spectrum at an FCC auction, there are hard engineering limits for the network traffic they can squeeze out of it.
Ultimately all radio communication works by encoding digital signal into a radio wave. Even in ideal conditions there
are physical limits to the digital information density that be crammed into a radio wave at a given frequency, but wireless
communication protocols require message redundancy and retry mechanisms given the unreliable nature of wireless
communications. As a result, there are tradeoffs between throughput and fidelity:

> Despite adaptive modulation and coding schemes, it is always possible that some of the transmitted data packets are
> not received correctly. In fact, it is even desirable that not all packets are received correctly, as this would
> indicate that the modulation and coding scheme is too conservative and hence capacity on the air interface is wasted. In
> practice \[in LTE\], the **air interface is best utilized if about 10% of the packets have to be retransmitted** because
> they have not been received correctly.

from Martin Sauter's _From GSM to LTE-Advanced Pro and 5G_.[^sauter]

All together MNOs are constrained by cell tower geography, spectrum availability, and the laws of physics. These three
constraints mean carriers face very nonzero transaction costs, giving them daunting unit economics as compared to
consumer aggregators like Meta and Google. But it gets worse. The American MNO market is an oligopoly with limited
differentiation between the big three carriers. As of this quarter, Verizon and AT&T have 116 and 114 million
connections to their consumer offerings while T-Mobile has 127 million connections but doesn't break them down between consumer 
and enterprise accounts. The three networks are more or less fighting to a draw.

At least in the US, carriers were once more differentiated as before the iPhone they had exclusive agreements with
handset vendors. The carriers had more leverage over the likes of Nokia or Motorola by owning the customer touchpoint.
But the advent of smartphones meant that the primary touchpoint moved from the carrier to the handset maker. Around the
world Apple demonstrated that customers would switch carriers to get the iPhone, meaning that they would have to carry
the handset to remain competitive, and they would do so on Apple's terms.[^moffat-stratechery-09-2024]

Despite all of this, cellular networks enable an incredible amount of modern life. In all but the most remote corners of
the country, you can depend on having signal. Not only does this require the unglamorous work of blanketing a continent
with cell towers, communication from a connected device to a cell tower requires rapid digital signal modulation,
radio transmission by the device, reception by the tower, demodulation, and error correction including possible retries. In the
last few months I've made an effort to learn more about LTE mobile broadband; wireless communication between a single
handset and a tower is a hard enough technical problem, but enabling hundreds or thousands of connected devices to
communicate with a cell tower requires sophisticated techniques like orthogonal frequency division multiple access (OFDM).
Without going into too much detail, it's impossible to learn about OFDM and not come away from it without respect
for the engineering that makes it all possible and genuine surprise that this complicated process works literally
hundreds of times per second on commodity consumer hardware.

## Lessons from Europe and the Electric Utility Parallels

A lot of what is spelled out in the last section was stated more succinctly in the annual SEC filings of the big three
carriers. Verizon's 10K explains "The telecommunications industry is highly competitive" while T-Mobile wrote nearly the
same with "The wireless communications services industry is highly competitive". AT&T had the decency to mix it up
with "We have multiple wireless competitors in each of our service areas and compete for customers". I promise the
reader that the SEC filings of major telecommunications companies are more interesting than they sound, but it's not
surprising to see such anodyne language in the annual reports given how unassuming the industry is.

For more drama, we turn to the European Telecommunications Network Operators' Association (ETNO) 2024 "State of Digital
Communications" report: [^etno-2024-state-of-digital-communications]

> There is no end in sight for the slide in the financial performance of European telecoms operators. European telecoms
> operators are among the largest European-owned entities in the digital value chain, and their continued financial
> weakness makes them less able to develop skills and services in Europe, and makes them prey to takeover and break-up by
> entities whose values may not be aligned with a European vision for strategic autonomy

By 'financial weakness' they mean that European MNO revenue growth lags behind both European economic growth and the
revenue growth for American carriers. The Stoxx Europe 600 Telecommunications index lags behind European equities as
well as global telecommunications indices. While there are many factors at play, average MNO monthly revenue per user in
Europe was €15 as compared with €42.50 in America. These lower prices have come at a cost: earlier in the report it's
explained that only 17.1% of all mobile connections in Europe are over 5G as opposed to 48.7% in the US. Europe's mobile
downlink speeds also trail America's, 97 to 64 Mbps. While American consumers pay more, their carriers invest twice as
much in capital as compared to their European counterparts.

Rather than seeing low mobile broadband pricing as a sign of the European common market working well for consumers,
European policymakers are concerned that prices are too low to support modern, performant cellular networks. Earlier in
the year, former European Central Bank president and Italian Prime Minister Mario Draghi commissioned a report for the
European Union on improving the competitiveness of the bloc's economy. The chapter on the European broadband market
struck a similar chord to the ETNO report: [^draghi-report]

> Lower prices in Europe have undoubtedly benefitted citizens and businesses but, over time, they have also reduced the
> industry profitability and, as a consequence, investment levels in Europe, including EU companies’ innovation in new
> technologies beyond basic connectivity.

Some of the cited drivers of lower profitability included _ex-ante_ regulation of telecommunications pricing (as opposed to
_ex-post_ regulatory action in the US when responding to malfeasance) the market operating on a country-by-country basis rather 
than bloc wide, as well as:

> Spectrum auctions to assign mobile frequencies have not been harmonised across member states and have been purely
> designed to command high prices (for 3G, 4G and 5G) over the past 25 years, with limited consideration for investment
> commitments, service quality or innovation.

This wouldn't be as much of a concern if it were not for the cost required to build out 5G; while they have considerably
higher throughout, 5G networks require new network hardware in cell sites. This is a substantial capital investment, and
the money to make it happen has to come from somewhere. The Draghi report has come to the conclusion that increasing
European consumer access to 5G networks today and 6G networks tomorrow will require charging European consumers more, but
it remains to be seen if this will be politically feasible.

When reading the ETNO and Draghi reports, I saw a lot of parallels between the state of the European telecommunications
market and the role of regulated electrical utilities in America. In many parts of America, state governments
grant one company to be the electrical utility monopoly. The idea is that duplicate electrical transmission
infrastructure would be inefficient, and in lieu of competition between firms keeping prices in check, state governments
set electricity prices that maintain relatively small profits for the utility company.

This is made more interesting by policy initiatives to encourage residential and industrial electrification in an effort
to reduce carbon emissions. There is political pressure to keep electricity prices low, as electricity rates set by the
government have many tax-like qualities. However, electrifying residential and industrial uses of power that currently rely
on fossil fuels requires investments in transmission infrastructure to support higher loads. It's worth noting that electrical 
utilities have a business incentive to do this, as more electrification means moving dollars from natural gas companies to power 
companies. But policymakers with ambitious carbon reduction targets may have more aggressive timelines than what would otherwise 
make economic sense for the utility, which requires bargaining.

Rate setters have a healthy skepticism of the utilities they regulate - no power company would tell a rate commission that
their company would survive just fine with a slightly lower the price of electricity. But just as for Europe's 5G buildout, 
the capital investment required for both regular operations and any further electrification has to come from somewhere, and 
utilities need to balance their books one way or another. For both European MNOs and American power companies there is a 
tradeoff between prices paid by consumers and investments in infrastructure. American policymakers committed to abundant, 
low carbon electricity would do well to heed Europe's warning on the consequences of ignoring this tradeoff.

[^aggregation-theory]: [Stratechery "Aggregation Theory" Post](https://stratechery.com/2015/aggregation-theory/)

[^hunting-heisenbugs]: [Paul McKenney's "Hunting Heisenbugs"](https://www.youtube.com/watch?v=gshynRXwrm8)

[^cox]: [Christopher Cox "An Introduction to LTE: LTE, LTE-Advanced, SAE, VoLTE and 4G Mobile Communications, 2nd Edition"](https://learning.oreilly.com/library/view/an-introduction-to/9781118818015/)

[^KNKN557]: [FCC Licence KNKN557](https://wireless2.fcc.gov/UlsApp/UlsSearch/license.jsp?licKey=12709)

[^sauter]: [Martin Sauter "From GSM to LTE-Advanced Pro and 5G, 4th Edition"](https://learning.oreilly.com/library/view/from-gsm-to/9781119714675/)

[^moffat-stratechery-09-2024]: [September 2024 Stratechery Daily Update Interview Craig Moffett](https://stratechery.com/2024/an-interview-with-craig-moffett-about-apple-and-telecoms)

[^etno-2024-state-of-digital-communications]: [ETNO 2024 State of Digital Communications](https://connecteurope.org/sites/default/files/2024-09/downloads/reports/etno%2520state%2520of%2520digital%2520communications%2520-%25202024.pdf)

[^draghi-report]: [The future of European competitiveness: Part B](https://commission.europa.eu/document/download/ec1409c1-d4b4-4882-8bdd-3519f86bbb92_en?filename=The%20future%20of%20European%20competitiveness_%20In-depth%20analysis%20and%20recommendations_0.pdf)

[^verizon-2023-10K]: [Verizon 2023 Form 10K](https://quotes.quotemedia.com/data/downloadFiling?webmasterId=104600&ref=318048243&type=PDF&formType=10-K&formDescription=Annual+report+pursuant+to+Section+13+or+15%28d%29&dateFiled=2024-02-09&cik=0000732712)

[^t-mobile-2023-10K]: [T-Mobile-2023-10K](https://investor.t-mobile.com/financials/sec-filings/default.aspx)