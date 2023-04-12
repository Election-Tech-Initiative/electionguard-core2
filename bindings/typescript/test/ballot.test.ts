import { assert } from "chai";
import ballot from "../../../data/ballot_in_simple.json";
import { fromJson } from "../src/ballot";

describe("PlaintextBallot Tests", () => {
  it("should convert from json", () => {
    const expected = JSON.stringify(ballot);
    const result = fromJson(expected);
    assert.isTrue(result.toJson().includes(ballot.object_id));
  });
});
