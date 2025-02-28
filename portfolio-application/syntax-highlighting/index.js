import { codeToHtml } from "shiki";
import process from "node:process";
import { readFileSync, writeFileSync, readdirSync } from "node:fs";

async function highlightFile(filename) {
  const currentFileContents = readFileSync(filename, "utf8");
  let position = 0;
  const backticks = [];

  while (true) {
    let foundIndex = currentFileContents.indexOf("```", position);
    if (foundIndex !== -1) {
      backticks.push(foundIndex);
      position = foundIndex + 1;
    } else break;
  }

  if (backticks.length % 2 !== 0) throw Error("[Syntax Highlighting]: Uneven number of backticks found");

  const replacementPair = [];

  for (let index = 0; index < backticks.length; index = index + 2) {
    const sliceIncludingLang = currentFileContents.slice(backticks[index], backticks[index + 1]); //Includes the ```myLang
    const fistNewlineIndex = sliceIncludingLang.indexOf("\n");

    if (fistNewlineIndex === -1) throw Error("[Syntax Highlighting]: Newline not found when expected");

    const language = sliceIncludingLang.match(/```(\w+)\n/)[1];
    const sliceWithoutLang = sliceIncludingLang.slice(fistNewlineIndex + 1, backticks[index + 1]);

    //const openingTags = `<pre class="language-${language} tabindex="0">\n<code class="language-${language}>\n`;
    //const closingTags = "</code>\n</pre>";
    //const syntaxHighlighting =
    //  openingTags +
    //  (await codeToHtml(sliceWithoutLang, {
    //    lang: language,
    //    theme: "catppuccin-mocha",
    //  })) +
    //  closingTags;
    const defaultSyntaxHighlighting = await codeToHtml(sliceWithoutLang, { lang: language, theme: "catppuccin-mocha" });
    const adjustedSyntaxHighlighting = defaultSyntaxHighlighting.replace(
      "background-color:#1e1e2e;color:#cdd6f4",
      "background-color:#181825;color:#cdd6f4;padding:1em;border-radius:0.3em",
    );
    const sliceIncludingLangAndClosingBackticks = currentFileContents.slice(backticks[index], backticks[index + 1] + 3);
    replacementPair.push([sliceIncludingLangAndClosingBackticks, adjustedSyntaxHighlighting]);
  }

  let highlightedFileContents = currentFileContents;

  replacementPair.forEach((pair) => {
    const [pattern, replacement] = pair;
    highlightedFileContents = highlightedFileContents.replace(pattern, replacement);
  });
  writeFileSync(filename, highlightedFileContents);
}

try {
  if (process.argv.length !== 3) throw Error(`[Syntax Highlighting]: Illegal number (${process.argv.length}) of arrguments provided`);

  const markdownDirectory = process.argv[2];
  console.log(`[Syntax Highlighting]: ${markdownDirectory}`);
  readdirSync(markdownDirectory).forEach(async (filename) => {
    await highlightFile(`${markdownDirectory}/${filename}`);
  });
} catch (err) {
  console.error(err);
  process.exit(1);
}
