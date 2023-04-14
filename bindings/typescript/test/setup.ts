import { getInstance } from "../src/wasm";

before(async function () {
  console.log("global vsetup");
  this.timeout(10000);
  await getInstance();
});
