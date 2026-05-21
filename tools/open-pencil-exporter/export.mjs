import { spawnSync } from "node:child_process";
import { basename, dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { mkdirSync, writeFileSync } from "node:fs";

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(__dirname, "../..");
const defaultOpenPencilDir = resolve(repoRoot, "references/open-pencil-complete");

function parseArgs(argv) {
  const options = {
    input: "",
    output: "",
    openPencilDir: defaultOpenPencilDir,
    depth: 10,
    skipSeed: true,
  };

  for (let i = 0; i < argv.length; i++) {
    const arg = argv[i];
    if (arg === "--input") {
      options.input = resolve(argv[++i]);
    } else if (arg === "--output") {
      options.output = resolve(argv[++i]);
    } else if (arg === "--open-pencil-dir") {
      options.openPencilDir = resolve(argv[++i]);
    } else if (arg === "--depth") {
      options.depth = Number(argv[++i]);
    } else if (arg === "--include-seed") {
      options.skipSeed = false;
    } else if (arg === "--help" || arg === "-h") {
      printHelp();
      process.exit(0);
    } else {
      throw new Error(`Unknown argument: ${arg}`);
    }
  }

  if (!options.input || !options.output) {
    printHelp();
    throw new Error("--input and --output are required.");
  }

  return options;
}

function printHelp() {
  console.log(`Usage: node export.mjs --input <file.fig> --output <ui-ir.json> [options]

Options:
  --open-pencil-dir <path>  OpenPencil repo (default: references/open-pencil-complete)
  --depth <n>               Tree depth passed to open-pencil tree (default: 10)
  --include-seed            Keep root "Seed" frame in output
`);
}

function parseJsonOutput(stdout) {
  const startArray = stdout.indexOf("[");
  const startObject = stdout.indexOf("{");
  let start = -1;

  if (startArray !== -1 && startObject !== -1) {
    start = Math.min(startArray, startObject);
  } else {
    start = Math.max(startArray, startObject);
  }

  if (start === -1) {
    throw new Error(`OpenPencil CLI did not return JSON:\n${stdout}`);
  }

  return JSON.parse(stdout.slice(start));
}

function runOpenPencil(openPencilDir, args) {
  const result = spawnSync("bun", ["run", "open-pencil", "--", ...args], {
    cwd: openPencilDir,
    encoding: "utf8",
    shell: process.platform === "win32",
  });

  if (result.status !== 0) {
    throw new Error(result.stderr || result.stdout || "OpenPencil CLI failed.");
  }

  return parseJsonOutput(result.stdout);
}

function collectTreeNodes(nodes, output = []) {
  for (const node of nodes) {
    output.push(node);
    if (node.children?.length) {
      collectTreeNodes(node.children, output);
    }
  }

  return output;
}

function mapType(type) {
  switch (type?.toUpperCase()) {
    case "FRAME":
    case "GROUP":
      return "frame";
    case "RECTANGLE":
      return "rectangle";
    case "TEXT":
      return "text";
    case "IMAGE":
      return "image";
    default:
      return type?.toLowerCase() ?? "frame";
  }
}

function mapFills(fills) {
  if (!fills?.length) {
    return undefined;
  }

  const mapped = fills
    .filter((fill) => fill?.type === "SOLID" && fill.color)
    .map((fill) => ({
      type: "SOLID",
      color: {
        r: fill.color.r,
        g: fill.color.g,
        b: fill.color.b,
        a: fill.color.a ?? 1,
      },
    }));

  return mapped.length ? mapped : undefined;
}

function estimateTextBounds(text, fontSize, bounds) {
  const characters = text ?? "";
  const width = Math.max(Math.round(fontSize * 0.55 * characters.length), Math.round(fontSize));
  const height = Math.max(Math.round(fontSize * 1.25), Math.round(fontSize));
  const looksLikePlaceholder = bounds.width >= 100 && bounds.height >= 100;

  if (!looksLikePlaceholder) {
    return bounds;
  }

  return {
    ...bounds,
    width,
    height,
  };
}

function transformNode(treeNode, detailsById) {
  const detail = detailsById.get(treeNode.id) ?? treeNode;
  const type = mapType(detail.type ?? treeNode.type);
  const bounds = {
    x: treeNode.x ?? detail.x ?? 0,
    y: treeNode.y ?? detail.y ?? 0,
    width: treeNode.width ?? detail.width ?? 0,
    height: treeNode.height ?? detail.height ?? 0,
  };
  const node = {
    id: treeNode.id,
    name: treeNode.name,
    type,
    bounds: type === "text"
      ? estimateTextBounds(detail.text, detail.fontSize ?? 14, bounds)
      : bounds,
    children: [],
  };

  const fills = mapFills(detail.fills);
  if (fills) {
    node.fills = fills;
  }

  if (detail.cornerRadius > 0) {
    node.cornerRadius = detail.cornerRadius;
  }

  if (type === "text" && detail.text) {
    node.text = {
      characters: detail.text,
      fontSize: detail.fontSize ?? 14,
    };
  }

  if (treeNode.children?.length) {
    node.children = treeNode.children.map((child) => transformNode(child, detailsById));
  }

  return node;
}

function filterRootNodes(nodes, skipSeed) {
  if (!skipSeed) {
    return nodes;
  }

  return nodes.filter((node) => node.name !== "Seed");
}

function exportUiIr(options) {
  const treeRoots = runOpenPencil(options.openPencilDir, [
    "tree",
    options.input,
    "--depth",
    String(options.depth),
    "--json",
  ]);

  const roots = filterRootNodes(treeRoots, options.skipSeed);
  const treeNodes = collectTreeNodes(roots);
  const detailsById = new Map();

  for (const treeNode of treeNodes) {
    const detail = runOpenPencil(options.openPencilDir, [
      "node",
      options.input,
      "--id",
      treeNode.id,
      "--json",
    ]);
    detailsById.set(treeNode.id, detail);
  }

  const document = {
    version: 1,
    source: "open-pencil",
    documentId: basename(options.input),
    nodes: roots.map((root) => transformNode(root, detailsById)),
  };

  mkdirSync(dirname(options.output), { recursive: true });
  writeFileSync(options.output, `${JSON.stringify(document, null, 2)}\n`, "utf8");
  console.log(`Exported ${treeNodes.length} nodes to ${options.output}`);
}

const options = parseArgs(process.argv.slice(2));
try {
  exportUiIr(options);
} catch (error) {
  console.error(error.message ?? error);
  process.exit(1);
}
