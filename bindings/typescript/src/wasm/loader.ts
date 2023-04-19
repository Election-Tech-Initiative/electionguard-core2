import createModule, { ElectionguardModule } from "./electionguard";

let _module: ElectionguardModule;

let _pending: Promise<void> | undefined = undefined;
let _status: "loaded" | "loading" | "unknown" = "unknown";

const load = async () => {
  console.log("Loading Electionguard WASM module");
  _module = await createModule();
};

const getInstance = async (): Promise<ElectionguardModule> => {
  switch (_status) {
    case "loaded":
      return _module;
    case "loading":
      if (_pending) {
        await _pending;
      }
      return _module;
    case "unknown":
      _pending = load();
      _status = "loading";
      await _pending;
      return _module;
    default:
      throw new Error(`Unexpected load status >${_status}<`);
  }
};

export { getInstance };
