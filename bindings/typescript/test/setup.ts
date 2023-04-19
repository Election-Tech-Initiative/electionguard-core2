import { initialize } from "../src";

before(async function () {
  console.log("global vsetup");
  this.timeout(10000);
  await initialize();
});
