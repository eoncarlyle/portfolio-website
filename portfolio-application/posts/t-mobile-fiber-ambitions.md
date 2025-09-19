---
title: T-Mobile's Fiber Ambitions
date: 2025.09.18
---

# T-Mobile's Fiber Ambitions

US Internet (USI) is a regional ISP based out of Minnetonka, a western suburb of Minneapolis, Minnesota. Its primary offering is fiber optic broadband, and its service area is about 2/3rds of Minneapolis and a smattering of her western suburbs. At most about half a million people are in the USI service area, so the company isn't a household name anywhere outside of Minnesota. While few ISPs are well loved, anecdotally USI has a good reputation; the CEO is known to answer customer support questions in Reddit DMs. Regional fiber ISPs in the Midwest don't often attract the attention of the EU's competition authority, but on June 13th the Competition Director-General Oliver Guersent approved the sale of US Internet to T-Mobile US (TMUS) and the investment bank KKR holdings. This is one of the fun things about the EU: because T-Mobile's parent company is based in Germany, they have to let competition authorities know about the purchase of a small American ISP months before any of said ISP's customers. The general public in Minnesota got news of the sale in early August 2025,[^customer-note] but USI isn't the only regional ISP newly under TMUS ownership - Metronet and Lumos were purchased in 2024 and 2025 respectively. T-Mobile isn't new to the broadband game given that they already have a fixed-wireless access (FWA) home internet service, but the 'T-Mobile Fiber' brand that these ISPs are being rolled into is an new terrestrial broadband offering.

T-Mobile is, first and foremost, a mobile network operator (MNO). So why are they getting into the terrestrial broadband game? This is ultimately pretty predictable given the state of the American mobile network market in 2025. At this point, almost everyone who wants cellular broadband can get it. Thanks to the Cambrian explosion of mobile virtual network operators (MVNOs), this is even true for downmarket segments. It isn't 2010 anymore when vanishingly few American consumers had a smartphone, and many didn't even have a cellphone at all. Given slow population growth and high smartphone adoption, AT&T, Verizon, and T-Mobile cannot grow the absolute size of the market very much. Getting higher revenue requires increasing average revenue per account (ARPA) and playing 'defence' to maintain your existing customers. T-Mobile's ISP strategy plays to both of these.[^streaming-economics]

The ARPA impact of buying an ISP is pretty clear: when T-Mobile buys a regional ISP, existing T-Mobile wireless customers who were on the incoming ISP will now shift their fiber spend to T-Mobile, driving ARPA higher. [^customer-acquisition] The churn-fighting part of this is more interesting. Because cellphones fit into pockets, customers can switch carriers without new equipment being installed to their home or business. With the advent of eSIMs, it can mean switching without ever leaving the home. But terrestrial broadband can be higher friction, requiring burying a new cable to the customer and installing new equipment in the most involved circumstances. For the vast majority of consumers who think as little as they possibly can about their internet service, remembering that switching off of T-Mobile will require changing their home internet may convince them to stay at T-Mobile rather than leave for Verizon or AT&T. So if you're a mobile network, it's in your interest to also be an ISP. This sounds a little conspiratorial, but it is worth pointing out that there is very little overlap in the metropolitan areas where Verizon, AT&T, and T-Mobile offer fiber broadband even though all three carriers serve wireless customers in most of America. This is to the benefit of all companies, because there is more friction in switching services if your new mobile carrier and ISP are different companies.

What I am pretty confident in is that this is not some insidious effort to jack up prices after consolidating players in non-cellular broadband. It would be wrong to say that T-Mobile's entry won't increase market concentration given their existing FWA offering. But FWA doesn't look to be that large of a player as compared to terrestrial broadband: in 2Q[^2q] of 2025 Verizon had over twice as many _wireline_ broadband customers as they did for FWA. And Craig Moffett explained in a 2023 Stratechery interview that capacity constraints for MNOs may render fixed wireless broadband as something of an industry afterthought. And with the comparatively small ISPs that T-Mobile has bought up, they wouldn't have enough market share to dictate pricing terms. This all only makes sense in the context of protecting their core business.

I will admit that the Twin Cities broadband market is in worse shape than I had thought prior to writing this post. I was able to find a residential block in Golden Valley that was 15 minutes from downtown Minneapolis, 10 minutes from General Mill headquarters yet, according to the FCC National Broadband Map, the only non-satellite broadband options were T-Mobile FWA, copper from CenturyLink, and DOCSIS over cable from Xfinity. Having DOCSIS as the fastest option strikes me as bizarre for such a centrally located suburb.

## References

[Ben Thompson. An Interview with Craig Moffett About Charter vs. Disney and the Path Dependency of the Communications Industry. 14 September 2023.](https://stratechery.com/2023/an-interview-with-craig-moffett-about-charter-vs-disney-and-the-path-dependency-of-the-communications-industry/)

[FCC National Broadband Map. United States Federal Communications Commission.](https://broadbandmap.fcc.gov/home)

[J.D. Duggan.  Minneapolis/St. Paul Buisness Journal. T-Mobile to acquire Twin Cities fiber provider U.S. Internet, expanding home internet footprint. 7 August 2025.](https://www.bizjournals.com/twincities/news/2025/08/07/t-mobile-to-acquire-minneapolis-based-us-internet.html)

[Official Journal of the European Union. 7 May 2025. Non-opposition to a notified concentration, Case M.11985](https://eur-lex.europa.eu/legal-content/EN/TXT/PDF/?uri=OJ:C_202503450&qid=1758247916688)

[T-Mobile. T‑Mobile and KKR Announce Joint Venture to Acquire Metronet and Offer Leading Fiber Solution to More U.S. Consumers. 24 July 2024.](https://www.t-mobile.com/news/network/t-mobile-kkr-joint-venture-to-acquire-metronet)

[T-Mobile. T‑Mobile and EQT Close Joint Venture to Acquire Lumos and Expand Fiber Internet Access. 1 April 2025.](https://www.t-mobile.com/news/business/t-mobile-eqt-close-lumos-fiber-jv)

[Verizon. Form 10-Q, 2Q 2025. 25 July 2025.](https://quotes.quotemedia.com/data/downloadFiling?webmasterId=104600&ref=319320781&type=PDF&formType=10-Q&formDescription=General+form+for+quarterly+reports+under+Section+13+or+15%28d%29&dateFiled=2025-07-25&cik=0000732712)

[^customer-note]: I must have been on the wronge email list, because I got the 'Exciting News! US Internet is now a part of the T-Mobile Fiber family' early this September.


[^streaming-economics]: As Ben Thompson often notes, it is also usually cheaper to retain existing customers than pay marketing and sales costs to acquire new ones. I can only imagine that the same is true for large mobile network operators. While MVNOs may have a different customer acquisition profile because of their smaller size, the sheer scale of traditional MNO marketing spend makes this plausible.

[^customer-acquisition]: This almost goes without saying, but newly acquired T-Mobile wireless customers who aren't T-Mobile customers are probably much easier for the company to convert given that they necessarily have the contact information and a billing relationship with said customers.

[^2q]: I don't understand why Verizon calls it '2Q' and not 'Q2', but I will respect the Verizon style guide just this once
