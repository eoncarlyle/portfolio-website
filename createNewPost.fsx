open System
open System.IO

let colorPrint (color: ConsoleColor) text =
    Console.ForegroundColor <- color
    printf "%s" text
    Console.ResetColor()

let inputPrompt label =
    colorPrint ConsoleColor.Blue $"{label}: "
    Console.ReadLine()

let inputPromptWithDefault label defaultVal =
    colorPrint ConsoleColor.Blue $"{label} ("
    colorPrint ConsoleColor.Yellow defaultVal
    colorPrint ConsoleColor.Blue "): "
    Console.ReadLine()

colorPrint ConsoleColor.Cyan "Create New Post\n"

let postsPath = "portfolio-application/posts"
let sourcePath = $"{postsPath}/source"

let title = inputPrompt "Title"

let currentDate = DateTime.Now.ToString("yyyy.MM.dd")
let date =
    match inputPromptWithDefault "Date" currentDate with
    | s when String.IsNullOrWhiteSpace(s) -> currentDate
    | s -> s

let year = date[..3]

let defaultSlug = title.ToLower().Replace(" ", "-")
let existingSlugs =
    Directory.GetFiles(sourcePath, "*.md")
    |> Array.map Path.GetFileNameWithoutExtension

let slug =
    match inputPromptWithDefault "Slug" defaultSlug with
    | s when String.IsNullOrWhiteSpace(s) -> defaultSlug
    | s -> s

if existingSlugs |> Array.contains slug then
    colorPrint ConsoleColor.Red $"Error: '{slug}' already exists. Aborting.\n"
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

colorPrint ConsoleColor.Green $"Created:  {sourceFile}\n"
colorPrint ConsoleColor.Green $"Symlink:  {symlinkPath} -> ../source/{slug}.md\n"
