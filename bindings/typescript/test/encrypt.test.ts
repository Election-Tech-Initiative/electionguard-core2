import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { EncryptionDeviceConverter } from "../src/encrypt";

const ciphertextElectionContext = (test_data as unknown as any).election
  .context;

const testDevice = {
  device_id: 12345,
  session_id: 23456,
  launch_code: 34567,
  location: "Location",
};

describe("EncryptionDeviceConverter Tests", () => {
  it("should convert from json", () => {
    const expected = JSON.stringify(testDevice);
    const result = EncryptionDeviceConverter.fromJson(expected);
    assert.isTrue(result.toJson().includes(testDevice.device_id.toString()));
  });
});
