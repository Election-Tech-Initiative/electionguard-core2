import { assert } from "chai";
import simpleBallot from "../../../data/ballot_in_simple.json";
import test_data from "../../../data/test/test-data.json";
import { ElectionContextConverter } from "../src/election";

const ciphertextElectionContext = (test_data as unknown as any).election
  .context;

describe("ElectionContextConverter Tests", () => {
  it("should convert from json", () => {
    const expected = JSON.stringify(ciphertextElectionContext);
    const result = ElectionContextConverter.fromJson(expected);
    assert.isTrue(
      result.toJson().includes(ciphertextElectionContext.elgamal_public_key)
    );
  });
});
