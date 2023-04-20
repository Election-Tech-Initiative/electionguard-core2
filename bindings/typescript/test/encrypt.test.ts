require("./setup");
import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { PlaintextBallot } from "../src/ballot";
import { ElectionContext } from "../src/election";
import { EncryptionDevice, EncryptionMediator } from "../src/encrypt";
import { InternalManifest } from "../src/manifest";
import { PrecomputeBufferContext } from "../src";

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

describe("EncryptionDevice Tests", () => {
  it("should convert from json", async () => {
    const expected = JSON.stringify(testDevice);
    const result = await EncryptionDevice.fromJson(expected);
    assert.isTrue(result.toJson().includes(testDevice.device_id.toString()));
  });
});

describe("EncryptionMediator Tests", () => {
  it("should encrypt a ballot", async () => {
    const context = await ElectionContext.fromJson(
      JSON.stringify(ciphertextElectionContext)
    );
    const manifest = await InternalManifest.fromJson(
      JSON.stringify(internalManifest)
    );
    const device = await EncryptionDevice.fromJson(JSON.stringify(testDevice));
    const data = await PlaintextBallot.fromJson(
      JSON.stringify(plaintextBallots[0])
    );
    const subject = await EncryptionMediator.make(manifest, context, device);
    const result = await subject.encrypt(data, false);

    assert.isTrue(result.objectId.includes(data.objectId));
  });
});

describe("EncryptionMediator Precompute Tests", () => {
  before(async () => {
    const context = await ElectionContext.fromJson(
      JSON.stringify(ciphertextElectionContext)
    );
    await PrecomputeBufferContext.initialize(context.publicKeyRef, 150);
    await PrecomputeBufferContext.start();
  });
  it("should encrypt a ballot", async () => {
    const context = await ElectionContext.fromJson(
      JSON.stringify(ciphertextElectionContext)
    );
    const manifest = await InternalManifest.fromJson(
      JSON.stringify(internalManifest)
    );
    const device = await EncryptionDevice.fromJson(JSON.stringify(testDevice));
    const data = await PlaintextBallot.fromJson(
      JSON.stringify(plaintextBallots[0])
    );
    const subject = await EncryptionMediator.make(manifest, context, device);
    const result = await subject.encrypt(data, false);

    assert.isTrue(result.objectId.includes(data.objectId));
  });
});
