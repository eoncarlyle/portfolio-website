---
title: January Playground Project First Thoughts
date: 2025.02.24
---

# Minimal API and Kotlin

Given that most of my job is writing Java, I have some appreciation for the syntactic sugar and batteries-included
nature of C#. The ASP.NET minimal API syntax top-level statements make it seem like there is less 'weighing you 
down' as you start a project. While the code example below is trivial, the equivalent in Spring is considerably 
more annoying. This really isn't all that important in enterprise settings, but the last time I wrote a [Spring 
project](https://github.com/eoncarlyle/HumbleMarket) outside of work it was frankly kind of annoying. Having so much
out-of-the-box in Spring is great, but it's not exactly what you want if you're just looking to get a few endpoints
up-and-running on a side project.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.Run();
```

At the start of the year there were a few technologies that I wanted to get more exposure to, namely using Apache
Kafka, WebSockets, and the Shadcn component library. [^shadcn] Given the experience that I had with F#, the minimal
API was certainly appealing to me, and I was surprised that it had WebSocket support. But given that I was trying
to build some up some work-relevant Kafka experience it was better to stay in the JVM world lest I learn .NET client
related information that would only be relevant to me off-hours. However, it turns out that Kotlin and the Javalin
framework gave me exactly what I was looking for. Not only does Kotlin support standalone functions,

```kotlin
import io.javalin.Javalin

fun main() {
    val app = Javalin.create(/*config*/)
        .get("/") { ctx -> ctx.result("Hello World") }
        .ws(("/websocket/{path}") { ws -> 
            ws.onConnect { ctx -> println("Connected") }
        })
        .start(7070)
}
```

# Session-based Authentication and WebSockets

[^shadcn]: [Shadcn Component Library](https://ui.shadcn.com/)