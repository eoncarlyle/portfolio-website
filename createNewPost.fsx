open System
open System.IO

let colored (color: ConsoleColor) text =
    Console.ForegroundColor <- color
    printf "%s" text
    Console.ResetColor()

let prompt label =
    colored ConsoleColor.Blue $"{label}: "
    Console.ReadLine()

let promptWithDefault label defaultVal =
    colored ConsoleColor.Blue $"{label} ("
    colored ConsoleColor.Yellow defaultVal
    colored ConsoleColor.Blue "): "
    Console.ReadLine()

colored ConsoleColor.Cyan "Create New Post\n"

let postsPath = "portfolio-application/posts"
let sourcePath = $"{postsPath}/source"

let title = prompt "Title"

let currentDate = DateTime.Now.ToString("yyyy.MM.dd")
let date =
    match promptWithDefault "Date" currentDate with
    | s when String.IsNullOrWhiteSpace(s) -> currentDate
    | s -> s

let year = date[..3]

let defaultSlug = title.ToLower().Replace(" ", "-")
let existingSlugs =
    Directory.GetFiles(sourcePath, "*.md")
    |> Array.map Path.GetFileNameWithoutExtension

let slug =
    match promptWithDefault "Slug" defaultSlug with
    | s when String.IsNullOrWhiteSpace(s) -> defaultSlug
    | s -> s

if existingSlugs |> Array.contains slug then
    colored ConsoleColor.Red $"Error: '{slug}' already exists. Aborting.\n"
    Environment.Exit(1)

let sourceFile = $"{sourcePath}/{slug}.md"
let yearDir = $"{postsPath}/{year}"
let symlinkPath = $"{yearDir}/{slug}.md"

let frontmatter = $"""---
title: {title}
date: {date}
---

# {title}
"""

Directory.CreateDirectory(yearDir) |> ignore
File.WriteAllText(sourceFile, frontmatter)
File.CreateSymbolicLink(symlinkPath, $"../source/{slug}.md") |> ignore

colored ConsoleColor.Green $"Created:  {sourceFile}\n"
colored ConsoleColor.Green $"Symlink:  {symlinkPath} -> ../source/{slug}.md\n"
