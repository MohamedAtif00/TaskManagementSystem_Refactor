import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const clientRoot = path.join(__dirname, "..");
const pagesDir = path.join(clientRoot, ".next", "server", "pages");
const staticDir = path.join(clientRoot, ".next", "static");
const publicDir = path.join(clientRoot, "public");
const outDir = path.join(clientRoot, "build");

function copyRecursive(src, dest) {
  if (!fs.existsSync(src)) {
    throw new Error(`Missing path: ${src}`);
  }
  const stat = fs.statSync(src);
  if (stat.isDirectory()) {
    fs.mkdirSync(dest, { recursive: true });
    for (const entry of fs.readdirSync(src)) {
      if (entry.endsWith(".js.nft.json")) continue;
      copyRecursive(path.join(src, entry), path.join(dest, entry));
    }
    return;
  }
  if (src.endsWith(".js.nft.json")) return;
  fs.mkdirSync(path.dirname(dest), { recursive: true });
  fs.copyFileSync(src, dest);
}

if (!fs.existsSync(pagesDir)) {
  console.error("Run `npm run build` before prepare-iis.");
  process.exit(1);
}

if (fs.existsSync(outDir)) {
  fs.rmSync(outDir, { recursive: true, force: true });
}
fs.mkdirSync(outDir, { recursive: true });

copyRecursive(pagesDir, outDir);
copyRecursive(staticDir, path.join(outDir, "_next", "static"));

if (fs.existsSync(publicDir)) {
  for (const entry of fs.readdirSync(publicDir)) {
    const src = path.join(publicDir, entry);
    const dest = path.join(outDir, entry);
    if (fs.statSync(src).isDirectory()) {
      copyRecursive(src, dest);
    } else {
      fs.copyFileSync(src, dest);
    }
  }
}

console.log(`IIS static site prepared at: ${outDir}`);
