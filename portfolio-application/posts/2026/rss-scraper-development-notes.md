---
title: RSS Scraper Development Notes
date: 2026.05.05
---

# RSS Scraper Development Notes

Artemis.bm is a strange website and I really wish that more industries had a counterpart: the website tracks the
insurance-linked security (ILS) and catastrophe bond industries. ILSs are an alternative to traditional reinsurance,
which is the insurance that insurance companies themselves purchase to protect against tail risks. One bad hurricane
could result in a lot of claims to State Farm or Allstate, so reinsurance is what protects retailer insurers from this
type of risk. There are only so many reinsurers to go around, and not all catastrophic risk is something that reinsurers
can confidently assess, so insurance-linked securities in general or catastrophy bonds in particular help to fill in the
gap. Catastrophe bonds pay above the risk-free rate, but if the insured event occurs, it comes out of the principal. For
example, the Louisiana Citizens Property Insurance Corporation is issuing
[$150m in named storm catastrophe bonds](https://www.artemis.bm/news/louisiana-citizens-seeks-150m-named-storm-reinsurance-with-bayou-re-2026-1-cat-bond/),
where coverage to the insurer and losses to the investor only take place if there is more than $540M in losses to
Louisiana Citizens due to a tropical storm or hurricane that is given a name by NOAA.

Artemis has an RSS feed with full articles in feed updates, the content is well written, and the website is updated
frequently. I don't really understand why this isn't all paywalled, because most people for whom this content is
relevant work for a handful of firms who wouldn't mind paying $50/month to subscribe.

However, as hard as this is to imagine, content about Insurance Linked Securities and the Reinsurance market can be a
little bit dry especially for someone who works in a completely unrelated field. Back when I subscribed to their RSS
feed, I didn't read it often enough to stay subscribed, so I decided this would make for a good first project where I'd
make LLM calls from within an application.

The repository for the RSS scraper and summary service is [here](https://github.com/eoncarlyle/portfolio-website). I
thought it would be passé to use a relational database to store summaries, so I defined a derived feed type as shown
below and stored it in Tigris object storage. While the feed schema was in JSON, this is something roughly equivalent to

Every 10 minutes each source RSS feed is checked against its derived feed, and per-article requests are made to the
Claude or Gemini APIs depending on the feed configuration. Asynchronous batch requests are supported by the program
which makes recording derived items a little more complicated, but at the same stage where new source feed items are
checked, the program uses the `Id` and `ProcessingStatus` fields in `DerivedBatch` to check up on said batches which can
take up to 48 hours to complete. Asynchronous batch completion time seems to be dependent on current demand to the model
provider, but regardless if a synchronous or asynchronous call is made the `Result` field in `DerivedItem` stores the
LLM summary result when it is complete.

I really wanted to avoid a situation where I run out of credits from duplicated requests sent off in rapid succession,
so not only did I check incoming requests against the derived feed items, I also had a single-permit semaphore gating
each model call with a 100 ms cooldown time. With JSON files and object storage used as the only persistence that I
used, `If-match`

```fsharp
type DerivedItem =
    { Guid: String
      Included: Boolean
      Item: RssItem
      Result: String option }

type DerivedBatch =
    { Id: String
      ProcessingStatus: ProcessingStatus
      BatchItems: DerivedItem array }

type DerivedFeed =
    { SourceUrl: String
      Batches: DerivedBatch array }
```

The feeds that get externally published are

```fsharp
type SinkFeed =
    { Title: string
      Link: string
      PubDate: String
      Description: string
      Items: SinkItem array }

and SinkItem =
    { Item: RssItem
      DerivedItemReferences: DerivedItemReference array }

and DerivedItemReference =
    { Title: String
      Guid: String option
      Link: String option }
```

Challenges:

- XML type providers not helpful for writing (have to duplicate structure anyway)
- Gemini batches haven't been working but that is probably a me issue
