import { assert } from "chai";
import { addModQ } from "../src/group";

describe("ElementModQ Math Tests", () => {
  it("should return 5 when 2 is added to 3", () => {
    const result = addModQ(2, 3);
    assert.equal(result, 5n);
  });
});
