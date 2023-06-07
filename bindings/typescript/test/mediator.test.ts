require("./setup");
import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { ElectionContext } from "../src/election";
import {
  ElementModP,
  EncryptionDevice,
  EncryptionMediator,
  InternalManifest,
} from "../src";

const ciphertextElectionContext = (test_data as unknown as any).election
  .context;

const manifestData = (test_data as unknown as any).election.manifest;

// Define a device
const testDeviceData = {
  device_id: 12345, // could be a static id passed from the server
  session_id: 23456, // could be a static id generated client side
  launch_code: 34567, // should be a random number generated as part of the election setup
  location: "Location", // could be the browser's UserAgent string
};

describe("Mediator Tests", () => {
  it("should convert from json", async () => {
    const context_data = JSON.stringify(ciphertextElectionContext);
    const manifest_data = JSON.stringify(manifestData);
    const context = await ElectionContext.fromJson(context_data);
    const internalManifest = await InternalManifest.fromManifestJson(
      manifest_data
    );
    const device = await EncryptionDevice.fromJson(
      JSON.stringify(testDeviceData)
    );

    const mediator = await EncryptionMediator.make(
      internalManifest,
      context,
      device
    );

    assert.isTrue(true);
  });
});
