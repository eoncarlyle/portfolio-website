# Iain's Portfolio Website

This is my personal portfolio website available on [iainschmitt.com](https://iainschmitt.com), where I have both ways to review my work and write the occasional post.
It is an F# application running on .NET Core 8 using the Giraffe framework, and it is deployed using GitHub Actions to my home server but hosted behind a public cloud reverse proxy.

## Syntax highlighting notes
I took a stab at a re-write of Prism, running JavaScript from .NET, and even thought of running the Python Pygments library before realising that there's probably a way to take care of this at build time instead.

```xml
<Project>
 <!-- ...  -->
  <Target Name="ApplySyntaxHighlighting" AfterTargets="CopyFilesToOutputDirectory">
    <Exec Command="powershell -ExecutionPolicy Bypass -File $(ProjectDir)scripts\highlight-markdown.ps1 -targetDir $(OutDir)WebRoot" />
  </Target>
</Project>
```