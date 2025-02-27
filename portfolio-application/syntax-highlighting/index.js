const Prism = require("./prism");
const fs = require("node:fs");
const process = require('node:process');


function highlightFile(filename) {
    const currentFileContents = fs.readFileSync(filename, "utf8");
    let position = 0;
    const backticks = [];

    while (true) {
        let foundIndex = currentFileContents.indexOf("```", position);
        if (foundIndex !== -1) {
            backticks.push(foundIndex);
            position = foundIndex + 1;
        } else break;
    }

    if (backticks.length % 2 !== 0) throw Error("[Syntax Highlighting]: Uneven number of backticks found")

    const replacementPair = []

    for (let index = 0; index < backticks.length; index = index + 2) {
        const sliceIncludingLang = currentFileContents.slice(backticks[index], backticks[index + 1]); //Includes the ```myLang
        const fistNewlineIndex = sliceIncludingLang.indexOf("\n");

        if (fistNewlineIndex === -1) throw Error("[Syntax Highlighting]: Newline not found when expected")

        const language = sliceIncludingLang.match(/```(\w+)\n/)[1]
        const sliceWithoutLang = sliceIncludingLang.slice(fistNewlineIndex + 1, backticks[index + 1])

        const openingTags = `<pre class="language-${language} tabindex="0">\n<code class="language-${language}>\n`
        const closingTags = "</code>\n</pre>"
        const syntaxHighlighting = openingTags + Prism.highlight(sliceWithoutLang, Prism.languages[language], language) + closingTags;
        const sliceIncludingLangAndClosingBackticks = currentFileContents.slice(backticks[index], backticks[index + 1] + 3);

        replacementPair.push([sliceIncludingLangAndClosingBackticks, syntaxHighlighting]);
    }

    let highlightedFileContents = currentFileContents;

    replacementPair.forEach(pair => {
        const [pattern, replacement] = pair
        highlightedFileContents = highlightedFileContents.replace(pattern, replacement);
    })
    fs.writeFileSync(filename, highlightedFileContents)
}

try {
    if (process.argv.length !== 3) throw Error(`[Syntax Highlighting]: Illegal number (${process.argv.length}) of arrguments provided`)
    
    const markdownDirectory = process.argv[2]
    console.log(`[Syntax Highlighting]: ${markdownDirectory}`);
    fs.readdirSync(markdownDirectory).forEach(filename => {
        highlightFile(`${markdownDirectory}/${filename}`)
    })
    
} catch (err) {
    console.error(err);
    process.exit(1);
}
