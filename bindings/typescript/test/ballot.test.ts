import { assert } from "chai";
import simpleBallot from "../../../data/ballot_in_simple.json";
import test_data from "../../../data/test/test-data.json";
import {
  CiphertextBallotConverter,
  PlaintextBallotConverter,
} from "../src/ballot";

const plaintextBallots = (test_data as unknown as any).plaintext_ballots;

const ciphertextBallots = (test_data as unknown as any).ciphertext_ballots;

describe("PlaintextBallotConverter Tests", () => {
  it("should convert simple ballot from json", () => {
    const expected = JSON.stringify(simpleBallot);
    const result = PlaintextBallotConverter.fromJson(expected);
    assert.isTrue(result.toJson().includes(simpleBallot.object_id));
  });
  it("should convert complex ballot from json", () => {
    const expected = JSON.stringify(plaintextBallots[0]);
    const result = PlaintextBallotConverter.fromJson(expected);
    assert.isTrue(result.toJson().includes(plaintextBallots[0].object_id));
  });
});

// describe("CiphertextBallotConverter Tests", () => {
//   it("should convert complex ballot from json", () => {
//     const expected = JSON.stringify(ciphertextBallots[0]);
//     console.log(expected);
//     const result = CiphertextBallotConverter.fromJson(expected);
//     assert.isTrue(result.toJson().includes(ciphertextBallots[0].object_id));
//   });
// });
