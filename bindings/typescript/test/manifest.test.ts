import { assert } from "chai";
import simpleBallot from "../../../data/ballot_in_simple.json";
import test_data from "../../../data/test/test-data.json";
import { InternalManifestConverter, ManifestConverter } from "../src/manifest";

const manifest = (test_data as unknown as any).election.manifest;

const internalManifest = (test_data as unknown as any).election
  .internal_manifest;

describe("ManifestConverter Tests", () => {
  it("should convert from json", () => {
    const expected = JSON.stringify(manifest);
    const result = ManifestConverter.fromJson(expected);
    assert.isTrue(result.toJson().includes(manifest.election_scope_id));
  });
});

describe("InternalManifestConverter Tests", () => {
  it("should convert from json", () => {
    const expected = JSON.stringify(internalManifest);
    const result = InternalManifestConverter.fromJson(expected);
    assert.isTrue(result.toJson().includes(internalManifest.manifest_hash));
  });
});
