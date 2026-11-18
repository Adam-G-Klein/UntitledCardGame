#!/usr/bin/env bash
# Downscale companion sprites from full-resolution masters for crisp in-game display.
# Uses only built-in macOS tools (Swift + Core Image, sips); nothing to install.
#
# Masters live in SourceArt/Companions/ (outside Assets/, so Unity ignores them).
# Output overwrites Assets/Sprites/Companions/<name>.png in place; .meta files are
# never touched, so GUIDs and all references stay intact.
#
# Usage:  Tools/resize_companions.sh [SIZE]                 (default 512, multiple of 4)
#         UNSHARP_RADIUS=0.8 UNSHARP_INTENSITY=0.8 Tools/resize_companions.sh   (stronger sharpening)
#         UNSHARP_INTENSITY=0 Tools/resize_companions.sh    (no sharpening)
#
# Safe to re-run with a different SIZE: it always resamples from the masters.

set -euo pipefail

SIZE="${1:-512}"
UNSHARP_RADIUS="${UNSHARP_RADIUS:-0.6}"
UNSHARP_INTENSITY="${UNSHARP_INTENSITY:-0.6}"

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_DIR="$REPO_ROOT/Assets/Sprites/Companions"
MASTER_DIR="$REPO_ROOT/SourceArt/Companions"
SWIFT_SRC="$REPO_ROOT/Tools/resize_image.swift"

if ! [[ "$SIZE" =~ ^[0-9]+$ ]] || (( SIZE <= 0 || SIZE % 4 != 0 )); then
    echo "error: SIZE must be a positive multiple of 4 (got '$SIZE')" >&2
    exit 1
fi
if ! command -v swiftc >/dev/null 2>&1; then
    echo "error: swiftc not found. Install the Xcode Command Line Tools: xcode-select --install" >&2
    exit 1
fi

WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/resize_companions.XXXXXX")"
trap 'rm -rf "$WORK_DIR"' EXIT

# Compile the resizer once (much faster than interpreting it per file).
RESIZER="$WORK_DIR/resize_image"
swiftc -O "$SWIFT_SRC" -o "$RESIZER"

width()  { sips -g pixelWidth  "$1" | awk '/pixelWidth/  {print $2}'; }
height() { sips -g pixelHeight "$1" | awk '/pixelHeight/ {print $2}'; }
dims()   { echo "$(width "$1")x$(height "$1")"; }
max_side() { local w h; w=$(width "$1"); h=$(height "$1"); echo $(( w > h ? w : h )); }

mkdir -p "$MASTER_DIR"

shopt -s nullglob
count=0
for unity_png in "$UNITY_DIR"/*.png; do
    name="$(basename "$unity_png")"
    master="$MASTER_DIR/$name"

    # Seed the master from the Unity copy, but only if it is still full-res,
    # so an already-downscaled file can never become the master.
    if [[ ! -f "$master" ]]; then
        if (( $(max_side "$unity_png") > SIZE )); then
            cp -p "$unity_png" "$master"
            echo "seeded master: $name ($(dims "$master"))"
        else
            echo "skip: $name has no master and is not larger than ${SIZE}px ($(dims "$unity_png"))"
            continue
        fi
    fi

    before="$(dims "$unity_png")"
    tmp="$WORK_DIR/$name"
    "$RESIZER" "$master" "$tmp" "$SIZE" "$UNSHARP_RADIUS" "$UNSHARP_INTENSITY"
    mv "$tmp" "$unity_png"
    echo "resized: $name  $before -> $(dims "$unity_png")  (master $(dims "$master"))"
    count=$((count + 1))
done

echo "done: $count companion sprite(s) at ${SIZE}x${SIZE}. Reimport Assets/Sprites/Companions in Unity."
