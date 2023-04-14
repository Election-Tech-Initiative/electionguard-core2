require("./setup");
import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { ElectionContext } from "../src/election";
import { ElementModP } from "../src";

const ciphertextElectionContext = (test_data as unknown as any).election
  .context;

describe("ElectionContext Tests", () => {
  it("should convert from json", async () => {
    const context = JSON.stringify(ciphertextElectionContext);
    //const expected = ElementModP.fromHex(ciphertextElectionContext.elgamal_public_key)
    const result = await ElectionContext.fromJson(context);
    assert.isTrue(
      result.publicKey
        .toHex()
        .includes(ciphertextElectionContext.elgamal_public_key)
    );
  });
});
