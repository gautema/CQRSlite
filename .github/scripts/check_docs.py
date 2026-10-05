#!/usr/bin/env python3
"""Checks the docs against the code so they can't silently drift.

1. A code block preceded by <!-- snippet: path/to/File.cs --> must match that file.
   Lines are compared without surrounding whitespace and blank lines are ignored.
   A line that is just "// ..." stands for skipped code: the parts around it must
   each appear in the file, in order.
2. Relative links between Markdown files, including #anchors, must resolve.
"""
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parents[2]
DOCS = [ROOT / "README.md", *sorted((ROOT / "docs").rglob("*.md"))]
SNIPPET = re.compile(r"<!--\s*snippet:\s*(\S+)\s*-->")
LINK = re.compile(r"\[[^\]]*\]\(([^)\s]+)\)")
ELISION = "// ..."

errors = []


def normalized(lines):
    return [l.strip() for l in lines if l.strip()]


def find_in_order(chunks, source):
    start = 0
    for chunk in chunks:
        for i in range(start, len(source) - len(chunk) + 1):
            if source[i:i + len(chunk)] == chunk:
                start = i + len(chunk)
                break
        else:
            return chunk
    return None


def check_snippets(doc, lines):
    for n, line in enumerate(lines):
        m = SNIPPET.search(line)
        if not m:
            continue
        source_path = ROOT / m.group(1)
        if not source_path.is_file():
            errors.append(f"{doc}:{n + 1}: snippet source {m.group(1)} does not exist")
            continue
        if n + 1 >= len(lines) or not lines[n + 1].startswith("```"):
            errors.append(f"{doc}:{n + 1}: snippet marker must be directly followed by a code block")
            continue
        end = next((i for i in range(n + 2, len(lines)) if lines[i].startswith("```")), None)
        if end is None:
            errors.append(f"{doc}:{n + 1}: unterminated code block")
            continue
        chunks, current = [], []
        for block_line in lines[n + 2:end]:
            if block_line.strip() == ELISION:
                chunks.append(current)
                current = []
            else:
                current.append(block_line)
        chunks.append(current)
        chunks = [c for c in (normalized(c) for c in chunks) if c]
        source = normalized(source_path.read_text(encoding="utf-8-sig").splitlines())
        missing = find_in_order(chunks, source)
        if missing:
            errors.append(f"{doc}:{n + 1}: snippet no longer matches {m.group(1)}, first differing part starts with: {missing[0]}")


def anchors(path):
    result, in_code = set(), False
    for line in path.read_text(encoding="utf-8").splitlines():
        if line.startswith("```"):
            in_code = not in_code
        if in_code or not line.startswith("#"):
            continue
        heading = line.lstrip("#").strip().lower()
        heading = re.sub(r"[^\w\- ]", "", heading)
        base = heading.replace(" ", "-")
        anchor, i = base, 1
        while anchor in result:
            anchor, i = f"{base}-{i}", i + 1
        result.add(anchor)
    return result


def check_links(doc, text):
    text = re.sub(r"```.*?```", "", text, flags=re.S)
    for target in LINK.findall(text):
        if re.match(r"[a-z]+:", target):
            continue
        path_part, _, anchor = target.partition("#")
        path = (doc.parent / path_part).resolve() if path_part else doc
        if not path.exists():
            errors.append(f"{doc}: broken link {target}")
        elif anchor and path.suffix == ".md" and anchor not in anchors(path):
            errors.append(f"{doc}: broken anchor {target}")


for doc in DOCS:
    text = doc.read_text(encoding="utf-8")
    check_snippets(doc.relative_to(ROOT), text.splitlines())
    check_links(doc, text)

if errors:
    print("\n".join(errors))
    sys.exit(1)
print(f"Docs OK: {len(DOCS)} files checked")
