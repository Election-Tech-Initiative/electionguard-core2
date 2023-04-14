require("./setup");
import { assert } from "chai";
import { ElementModP, ElementModQ, GroupFunctions } from "../src/group";

describe("ElementModP Tests", () => {
  it("should convert from a hex string", async () => {
    const result = await ElementModP.fromHex("0x05");
    assert.equal(result.toHex(), "05");
  });
});

describe("ElementModQ Tests", () => {
  it("should convert from a hex string", async () => {
    const result = await ElementModQ.fromHex("0x05");
    assert.equal(result.toHex(), "05");
  });
});

describe("GroupFunctions addModQ Math Tests", () => {
  it("should return 5 when 2 is added to 3", async () => {
    const result = await GroupFunctions.addModQ(2, 3);
    assert.equal(result, 5n);
  });
});
