require("./setup");
import { assert } from "chai";
import simpleBallot from "../../../data/ballot_in_simple.json";
import test_data from "../../../data/test/test-data.json";
import { CiphertextBallot, PlaintextBallot } from "../src/ballot";

const plaintextBallots = (test_data as unknown as any).plaintext_ballots;

const ciphertextBallots = (test_data as unknown as any).ciphertext_ballots;

describe("PlaintextBallot Tests", () => {
  it("should convert simple ballot from json", async () => {
    const expected = JSON.stringify(simpleBallot);
    const result = await PlaintextBallot.fromJson(expected);
    assert.isTrue(result.toJson().includes(simpleBallot.object_id));
  });
  it("should convert from json", async () => {
    const expected = JSON.stringify(plaintextBallots[0]);
    const result = await PlaintextBallot.fromJson(expected);
    assert.isTrue(result.toJson().includes(plaintextBallots[0].object_id));
  });
});

describe("CiphertextBallot Tests", () => {
  const expected = JSON.stringify(ciphertextBallots[0]);
  it("should convert from json", async () => {
    const result = await CiphertextBallot.fromJson(expected);
    assert.isTrue(
      result.toJson(false).includes(ciphertextBallots[0].object_id)
    );
  });

  it("should have same object id", async () => {
    const result = await CiphertextBallot.fromJson(expected);
    assert.isTrue(result.objectId.includes(ciphertextBallots[0].object_id));
  });
});
