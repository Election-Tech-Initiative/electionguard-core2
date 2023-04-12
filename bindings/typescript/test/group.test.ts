import { assert } from "chai";
import {
  ElementModPConverter,
  ElementModQConverter,
  GroupFunctions,
} from "../src/group";

describe("ElementModPConverter Tests", () => {
  it("should convert from a hex string", () => {
    const result = ElementModPConverter.fromHex("0x05");
    assert.equal(result.toHex(), "05");
  });
});

describe("ElementModQConverter Tests", () => {
  it("should convert from a hex string", () => {
    const result = ElementModQConverter.fromHex("0x05");
    assert.equal(result.toHex(), "05");
  });
});

describe("GroupFunctions addModQ Math Tests", () => {
  it("should return 5 when 2 is added to 3", () => {
    const result = GroupFunctions.addModQ(2, 3);
    assert.equal(result, 5n);
  });
});
