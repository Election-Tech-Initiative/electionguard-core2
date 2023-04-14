require("./setup");
import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { ElectionContextConverter } from "../src/election";

const ciphertextElectionContext = (test_data as unknown as any).election
  .context;

describe("ElectionContextConverter Tests", () => {
  it("should convert from json", async () => {
    const expected = JSON.stringify(ciphertextElectionContext);
    const result = await ElectionContextConverter.fromJson(expected);
    assert.isTrue(
      result.toJson().includes(ciphertextElectionContext.elgamal_public_key)
    );
  });
});
