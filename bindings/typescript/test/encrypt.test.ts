import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { PlaintextBallotConverter } from "../src/ballot";
import { ElectionContextConverter } from "../src/election";
import {
  EncryptionDeviceConverter,
  EncryptionMediatorConverter,
} from "../src/encrypt";
import { InternalManifestConverter } from "../src/manifest";

const ciphertextElectionContext = (test_data as unknown as any).election
  .context;
const internalManifest = (test_data as unknown as any).election
  .internal_manifest;

const plaintextBallots = (test_data as unknown as any).plaintext_ballots;

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

describe("EncryptionMediatorConverter Tests", () => {
  it("should encrypt a ballot", () => {
    const context = ElectionContextConverter.fromJson(
      JSON.stringify(ciphertextElectionContext)
    );
    const manifest = InternalManifestConverter.fromJson(
      JSON.stringify(internalManifest)
    );
    const device = EncryptionDeviceConverter.fromJson(
      JSON.stringify(testDevice)
    );
    const data = PlaintextBallotConverter.fromJson(
      JSON.stringify(plaintextBallots[0])
    );
    const subject = EncryptionMediatorConverter.make(manifest, context, device);
    const result = subject.encrypt(data, false);

    assert.isTrue(result.getObjectId().includes(data.getObjectId()));
    // TODO: reasonable timoeut for this test when optimizatiosn enabled
  }).timeout(100000); // 100 seconds :-(
});
