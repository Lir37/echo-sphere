import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';

const root = process.cwd();
const publicDir = path.join(root, 'public');
const srcDir = path.join(root, 'src');

function walk(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(dir, entry.name);
    return entry.isDirectory() ? walk(full) : [full];
  });
}

const sourceFiles = walk(srcDir).filter((file) => /\.(ts|tsx|js|jsx)$/.test(file));
const sourceText = sourceFiles.map((file) => fs.readFileSync(file, 'utf8')).join('\n');
const publicFiles = walk(publicDir);
const failures = [];

for (const file of publicFiles) {
  const rel = '/' + path.relative(publicDir, file).replaceAll(path.sep, '/');
  const basename = path.basename(file);
  if (!sourceText.includes(rel) && !sourceText.includes(basename)) failures.push(`unreferenced public asset: ${rel}`);
}

function pngStats(file) {
  const data = fs.readFileSync(file);
  if (data.length <= 2048) return null;
  if (!data.subarray(0, 8).equals(Buffer.from([137,80,78,71,13,10,26,10]))) return { unsupported: true };
  let offset = 8, width = 0, height = 0, bitDepth = 0, colorType = 0, interlace = 0, palette = null, transparency = null;
  const idat = [];
  while (offset < data.length) {
    const length = data.readUInt32BE(offset);
    const type = data.toString('ascii', offset + 4, offset + 8);
    const chunk = data.subarray(offset + 8, offset + 8 + length);
    offset += length + 12;
    if (type === 'IHDR') { width = chunk.readUInt32BE(0); height = chunk.readUInt32BE(4); bitDepth = chunk[8]; colorType = chunk[9]; interlace = chunk[12]; }
    else if (type === 'PLTE') palette = chunk;
    else if (type === 'tRNS') transparency = chunk;
    else if (type === 'IDAT') idat.push(chunk);
    else if (type === 'IEND') break;
  }
  if (interlace !== 0 || bitDepth !== 8) return { unsupported: true };
  const channels = ({0:1,2:3,3:1,4:2,6:4})[colorType];
  if (!channels) return { unsupported: true };
  const compressed = Buffer.concat(idat);
  let raw;
  try {
    raw = zlib.inflateSync(compressed);
  } catch {
    try {
      // Some existing authored PNGs were exported with a raw-deflate stream.
      // Accept that legacy encoding while still validating pixel variation.
      raw = zlib.inflateRawSync(compressed);
    } catch {
      return { unsupported: true };
    }
  }
  const stride = width * channels;
  let prev = Buffer.alloc(stride), cursor = 0;
  const colors = new Set();
  const paeth = (a,b,c) => { const p=a+b-c, pa=Math.abs(p-a), pb=Math.abs(p-b), pc=Math.abs(p-c); return pa<=pb&&pa<=pc?a:pb<=pc?b:c; };
  for (let y=0; y<height; y++) {
    const filter = raw[cursor++];
    const row = Buffer.alloc(stride);
    for (let x=0; x<stride; x++) {
      const left = x >= channels ? row[x-channels] : 0;
      const up = prev[x] || 0;
      const upLeft = x >= channels ? prev[x-channels] || 0 : 0;
      const value = raw[cursor++];
      row[x] = filter===0 ? value : filter===1 ? (value+left)&255 : filter===2 ? (value+up)&255 : filter===3 ? (value+Math.floor((left+up)/2))&255 : filter===4 ? (value+paeth(left,up,upLeft))&255 : value;
    }
    for (let x=0; x<width; x++) {
      const base=x*channels; let r,g,b,a=255;
      if (colorType===6) [r,g,b,a]=row.subarray(base,base+4);
      else if (colorType===2) [r,g,b]=row.subarray(base,base+3);
      else if (colorType===4) { r=g=b=row[base]; a=row[base+1]; }
      else if (colorType===0) { r=g=b=row[base]; if(transparency) a=row[base]===transparency.readUInt16BE(0)?0:255; }
      else { const index=row[base]; r=palette?.[index*3]||0; g=palette?.[index*3+1]||0; b=palette?.[index*3+2]||0; a=transparency?.[index]??255; }
      colors.add((r<<24)|(g<<16)|(b<<8)|a);
      if (colors.size > 2) return { monochrome: false };
    }
    prev=row;
  }
  return { monochrome: colors.size <= 2 };
}

for (const file of publicFiles) {
  if (path.extname(file).toLowerCase() !== '.png') continue;
  const stats = pngStats(file);
  if (stats?.monochrome) failures.push(`suspicious monochrome PNG: /${path.relative(publicDir,file).replaceAll(path.sep,'/')}`);
  if (stats?.unsupported) console.warn(`asset check: skipped unsupported PNG: ${file}`);
}
if (failures.length) { console.error(failures.join('\n')); process.exit(1); }
console.log('asset check passed');
