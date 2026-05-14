---
title: RSS Scraper Development Notes
date: 2026.05.13
---

# RSS Scraper Development Notes

Artemis.bm is a strange website and I wish that more industries had a counterpart: the website tracks the
insurance-linked security (ILS) and catastrophe bond industries. ILSs are an alternative to traditional reinsurance,
which is the insurance that insurance companies themselves purchase to protect against tail risks. One bad hurricane
could result in a lot of claims to State Farm or Allstate, so reinsurance is what protects retail insurers from this
type of risk. There are only so many reinsurers to go around, and not all catastrophic risk is something that reinsurers
can confidently assess; insurance-linked securities in general or catastrophe bonds in particular help to fill in the
gap. Catastrophe bonds pay above the risk-free rate, but if the insured event occurs, it comes out of the bond
principal. For example, the Louisiana Citizens Property Insurance Corporation is issuing
[$150 million in named storm catastrophe bonds](https://www.artemis.bm/news/louisiana-citizens-seeks-150m-named-storm-reinsurance-with-bayou-re-2026-1-cat-bond/).
The terms of those bonds are that if a NOAA named hurricane or tropical storm causes more than $540 million in losses to
Louisiana Citizens insurance in the next 3 hurricane seasons, investors will start to take losses.

Artemis.bm is well written with frequent updates, and full articles are available in the website's RSS feed. I don't
really understand why this isn't all paywalled, because most people for whom this content is relevant work for a handful
of firms who wouldn't mind paying $50/month to subscribe.

However, as hard as this is to imagine, content about ILSs and the reinsurance market can be a little bit dry specially
for someone who works in a completely unrelated field. Back when I subscribed to their RSS feed, I didn't read it often
enough to stay subscribed. I hadn't yet done a project with LLM calls inside of a service, and RSS feed summaries was
something I would use.

The repository for the RSS scraper and summary service is [here](https://github.com/eoncarlyle/portfolio-website), and
this is what I use to keep on top of Artemis.bm and a several other RSS feeds that I don't have time to read in full. I
call each original, scraped feed a 'source' feed, each of which has a 'derived' feed storing in-progress and completed
LLM summaries for source items. Breaking out the derived feed makes it possible to handle asynchronous batch requests to
the Anthropic or Gemini APIs, because batch job status is checked in the process of checking source feeds for updates.
Also, for both synchronous and asynchronous requests I wanted to avoid a situation where I run out of credits from
duplicated requests sent off in rapid succession, so the service checks against the derived feed before making a model
call.

I thought it would be passé to use a relational database to store feeds, so the derived feeds as defined below are
stored in Tigris object storage. While the derived feeds are represented in JSON, these are otherwise very similar to an
RSS feed within the application.

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

With JSON files in object storage as the only persistence for the application, `If-Match` headers are used for
concurrency control. This doesn't do much to prevent duplicating requests, but it makes it safer to run the scrape
process ad-hoc without interfering with an in-progress scrape on my VPC.

Once the derived feeds associated with a 'sink' feed reach a configurable count of items, they are published to an RSS
client accessible sink feed. For example, both my Artemis.bm and Substack blog sink feeds publish in batches of 5, so
whenever there are 5 fresh derived feed items that aren't yet in the sink feed then the next sink feed item includes all
of these fresh summaries. This requires the `DerivedItemReference` in a `SinkItem` to keep track of which derived feed
items have already been published.

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

Initially I tried to use XML for the derived and source feeds; I figured if I was doing an RSS project I couldn't really
get away from XML. But ambiguities of representing lists as `<Items> <Item /> <Item/> </Items>` or just
`<Item /> <Item />` and much better serialisation and deserialization in JSON made me abandon XML. Even sink feeds
internally are represented as JSON, but the server that publishes the sink feeds uses the F# Giraffe view engine to
convert the JSON to XML. But because the schema of a given source RSS feed shouldn't change much, XML type providers
were very helpful in source feed deserialisation. For instance, `type ArtemisRss = XmlProvider<"Schema/artemis.rss">`
was used to define the Artemis source feed type from a file in the repository, so handling source feeds could be done in
a type-safe way that was friendly to autocomplete.

And despite the work that went into making batch LLM summary calls, I've had issues with both Anthropic and Gemini batch
requests. Admittedly, the current batch handling logic doesn't handle failures very gracefully; I don't have a timeout
or similar handling so that may well be my issue. This isn't a showstopper because this type of work isn't very token
intensive. I've used \$1.99 on Claude credits and \$0.21 on Gemini, and both of those numbers would be lower were it not
for some accidental duplicate LLM calls during development. The service uses Haiku 4.5 from Anthropic and Gemini 3.1
Flash Lite because text summaries don't require a crazy powerful model; right now Haiku 4.5 costs \$1/MTok input with
\$5/MTok on output. Gemini is far cheaper at $0.25/MTok and $0.75/MTok.
