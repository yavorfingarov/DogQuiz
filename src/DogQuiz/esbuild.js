import { build } from "esbuild";
import { promises as fs } from "fs";
import { manifest } from "./Pages/Assets/manifest.js";

const assets = "./Pages/Assets";
const entryPoints = [`${assets}/main.js`, `${assets}/main.css`];
const staticAssets = `${assets}/Static`;
const outdir = "./wwwroot";
const manifestPath = `${outdir}/manifest.json`;

await fs.rm(outdir, { force: true, recursive: true });
await fs.cp(staticAssets, outdir, { recursive: true });
await fs.writeFile(manifestPath, JSON.stringify(manifest));
await build({
    entryPoints,
    bundle: true,
    minify: true,
    legalComments: "none",
    outdir
});
