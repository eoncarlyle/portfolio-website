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
relevant work for a small handful of firms who likely wouldn't mind paying $50/month for the service.

However, as hard as this is to imagine, content about Insurance Linked Securities and the Reinsurance 
market can be a little bit dry especially for someone who works in a completely unrelated field. Back when I 
subscribed to their RSS feed, I didn't read it often enough to stay subscribed. 

```fsharp
type FetchSource = logger: ILogger -> url: string -> Task<RssItem array>
```

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