require("./setup");
import { assert } from "chai";
import test_data from "../../../data/test/test-data.json";
import { InternalManifest, Manifest } from "../src/manifest";

const manifest = (test_data as unknown as any).election.manifest;

const internalManifest = (test_data as unknown as any).election
  .internal_manifest;

describe("Manifest Tests", () => {
  it("should convert from json", async () => {
    const data = JSON.stringify(manifest);
    const result = await Manifest.fromJson(data);
    assert.isTrue(result.toJson().includes(manifest.election_scope_id));
  });
});

describe("InternalManifest Tests", () => {
  it("should convert from json", async () => {
    const data = JSON.stringify(internalManifest);
    const result = await InternalManifest.fromJson(data);
    assert.isTrue(result.toJson().includes(internalManifest.manifest_hash));
  });
  it("should convert from manifest json", async () => {
    const data = JSON.stringify(manifest);
    const result = await InternalManifest.fromManifestJson(data);
    assert.isTrue(result.toJson().includes(internalManifest.manifest_hash));
  });
  it("should convert from manifest", async () => {
    const data = JSON.stringify(manifest);
    const manifest_ = await Manifest.fromJson(data);
    const result = await InternalManifest.fromManifest(manifest_);
    assert.isTrue(result.toJson().includes(internalManifest.manifest_hash));
  });
});
