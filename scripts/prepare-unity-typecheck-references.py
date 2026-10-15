"""Extract pinned public API DLLs from Unity's official archive; never run Unity.

The XZ index gives independent compression blocks. HTTP range requests fetch
only blocks containing the required DLLs, instead of installing the editor.
Every extracted DLL must match its checked-in SHA-256 reference.
"""
import argparse
import base64
import hashlib
import json
import lzma
from pathlib import Path
import struct
import urllib.request
import zlib


def vli(value):
    result = bytearray()
    while True:
        result.append((value & 127) | (128 if value >= 128 else 0))
        value >>= 7
        if not value:
            return bytes(result)


def single_block(header, block, compressed):
    index = b"\0\1" + vli(block["unpadded"]) + vli(block["uncompressed"])
    index += bytes((-len(index)) % 4)
    index += struct.pack("<I", zlib.crc32(index))
    footer = struct.pack("<I", len(index) // 4 - 1) + header[6:8]
    footer = struct.pack("<I", zlib.crc32(footer)) + footer + b"YZ"
    return header + compressed + index + footer


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    repository = Path(__file__).resolve().parent.parent
    manifest_path = repository / "tools/EchoSphere.UnityTypeChecks/reference-manifest.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    # Reject version drift instead of silently compiling against a different API.
    version = (repository / "UnityProject/ProjectSettings/ProjectVersion.txt").read_text()
    if f"m_EditorVersion: {manifest['editorVersion']}\n" not in version:
        raise RuntimeError("Unity version changed; regenerate the pinned API references.")

    def destination(file):
        path = (output / file["target"]).resolve()
        if not path.is_relative_to(output):
            raise RuntimeError("Reference path escapes output directory")
        return path

    def valid(file):
        path = destination(file)
        return path.is_file() and hashlib.sha256(path.read_bytes()).hexdigest() == file["sha256"]

    missing = [file for file in manifest["files"] if not valid(file)]
    blocks = [block for block in manifest["blocks"] if any(
        file["offset"] < block["raw"] + block["uncompressed"]
        and file["offset"] + file["size"] > block["raw"] for file in missing)]
    header = base64.b64decode(manifest["xzHeader"])
    raw_blocks = {}
    cache = output / ".blocks"
    cache.mkdir(exist_ok=True)
    for block in blocks:
        cached = cache / f"{block['id']}.xz"
        if not cached.exists():
            start = block["offset"]
            end = start + ((block["unpadded"] + 3) // 4) * 4 - 1
            request = urllib.request.Request(manifest["archiveUrl"], headers={"Range": f"bytes={start}-{end}"})
            with urllib.request.urlopen(request, timeout=60) as response:
                expected = f"bytes {start}-{end}/{manifest['archiveSize']}"
                if response.status != 206 or response.headers.get("Content-Range") != expected:
                    raise RuntimeError("The official download did not honor the requested byte range")
                compressed = response.read()
            if len(compressed) != end - start + 1:
                raise RuntimeError("Incomplete Unity archive block")
            cached.write_bytes(single_block(header, block, compressed))
        raw = lzma.decompress(cached.read_bytes())  # validates block/header/index checksums
        if len(raw) != block["uncompressed"]:
            raise RuntimeError("Unexpected decompressed Unity archive block size")
        raw_blocks[block["id"]] = raw
        print(f"Unity API reference block {block['id']}: verified", flush=True)

    for file in missing:
        data = bytearray()
        for block in manifest["blocks"]:
            start = max(file["offset"], block["raw"])
            end = min(file["offset"] + file["size"], block["raw"] + block["uncompressed"])
            if start < end:
                data.extend(raw_blocks[block["id"]][start-block["raw"]:end-block["raw"]])
        if len(data) != file["size"] or hashlib.sha256(data).hexdigest() != file["sha256"]:
            raise RuntimeError("Official Unity DLL hash mismatch: " + file["target"])
        path = destination(file)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
    (output / "reference-manifest.json").write_bytes(manifest_path.read_bytes())
    print(f"Unity {manifest['editorVersion']} API DLLs: {len(manifest['files'])} verified. No editor installation or execution.")


if __name__ == "__main__":
    main()
