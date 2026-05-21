import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { runDoctor } from "../lib/doctor.mjs";
import { runExport } from "../lib/export.mjs";
import { runImport } from "../lib/import.mjs";
import { runPromptContext } from "../lib/prompt-context.mjs";

const __dirname = dirname(fileURLToPath(import.meta.url));

function printHelp() {
  console.log(`Usage: pencil-ugui <command> [options]

Commands:
  doctor            Validate harness setup
  export            Export OpenPencil .fig to UI IR JSON
  import            Import UI IR JSON through Unity local server
  prompt-context    Print provider and import prompt context

Options:
  --project <path>  Unity project root (default: current directory)
  --provider <id>   Canvas provider id (default: config value)
  --input <path>    Source design file for export
  --output <path>   UI IR output path for export
  --ui-ir <path>    UI IR input path for import
  --target <mode>   Import target mode (default: selection)
`);
}

function parseArgs(argv) {
  const options = {
    command: "",
    projectRoot: process.cwd(),
    provider: "",
    input: "",
    output: "",
    uiIrPath: "",
    target: "selection",
  };

  const args = [...argv];
  if (args.length === 0 || args[0] === "--help" || args[0] === "-h") {
    printHelp();
    process.exit(0);
  }

  options.command = args.shift();
  while (args.length > 0) {
    const arg = args[0];
    if (arg === "--project") {
      options.projectRoot = resolve(args[1]);
      args.splice(0, 2);
    } else if (arg === "--provider") {
      options.provider = args[1];
      args.splice(0, 2);
    } else if (arg === "--input") {
      options.input = args[1];
      args.splice(0, 2);
    } else if (arg === "--output") {
      options.output = args[1];
      args.splice(0, 2);
    } else if (arg === "--ui-ir") {
      options.uiIrPath = args[1];
      args.splice(0, 2);
    } else if (arg === "--target") {
      options.target = args[1];
      args.splice(0, 2);
    } else if (arg === "--help" || arg === "-h") {
      printHelp();
      process.exit(0);
    } else {
      throw new Error(`Unknown argument: ${arg}`);
    }
  }

  return options;
}

const options = parseArgs(process.argv.slice(2));

try {
  switch (options.command) {
    case "doctor":
      await runDoctor(options);
      break;
    case "export":
      await runExport(options);
      break;
    case "import":
      await runImport(options);
      break;
    case "prompt-context":
      await runPromptContext(options);
      break;
    default:
      throw new Error(`Unknown command: ${options.command}`);
  }
} catch (error) {
  console.error(error.message ?? error);
  process.exit(1);
}
