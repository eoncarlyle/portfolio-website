const { posix } = require("path/posix");
const Prism = require("./prism");
const fs = require("node:fs");

try {
  const file = fs.readFileSync("demo.md", "utf8");
  let position = 0;
  const codeMatches = [];

  while (true) {
    let foundIndex = file.indexOf("```", position);
    if (foundIndex !== -1) {
      codeMatches.push(foundIndex);
      position = foundIndex + 1;
    } else break;
  }

  /*
  1 - Get the prism representations using `Prism.langauges["myLang"]`
  2 - Do a string replace of the matched range
  3 - Write the files
  4 - Fail if anything bad happens
  */

  console.log(file.slice(1796, 2178 + 3));

  // ```javascript
  // // identity
  // map(id) === id;

  // // composition
  // compose(map(f), map(g)) === map(compose(f, g));

  // const compLaw1 = compose(map(append(" romanus ")), map(append(" sum")));
  // const compLaw2 = map(compose(append(" romanus "), append(" sum")));
  // compLaw1(Container.of("civis")); // Container("civis romanus sum")
  // compLaw2(Container.of("civis")); // Container("civis romanus sum")
  // ```;

  console.log(codeMatches);

  // [
  //   1796, 2178, 2625, 3028,
  //   3528, 3903, 4041, 4128,
  //   4338, 4812, 5738, 6653,
  //   7096, 7189, 7297, 7887
  // ]
} catch (err) {
  console.error(err);
}
