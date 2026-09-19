#!/usr/bin/env python3
"""Split combined Command/Query files into separate DTO, Handler, and Validator files."""

from __future__ import annotations

import re
import sys
from pathlib import Path

MODULES_ROOT = Path(__file__).resolve().parents[1] / "src" / "Modules"
SKIP_MODULES = {"Identity"}

MINIMAL_USINGS = [
    "TaskManagementSystem.BuildingBlocks.Application",
    "TaskManagementSystem.BuildingBlocks.Domain",
]


def find_matching_brace(text: str, open_index: int) -> int:
    depth = 0
    in_string = False
    escape = False
    i = open_index
    while i < len(text):
        ch = text[i]
        if in_string:
            if escape:
                escape = False
            elif ch == "\\":
                escape = True
            elif ch == '"':
                in_string = False
            i += 1
            continue

        if ch == '"':
            in_string = True
            i += 1
            continue

        if ch == "{":
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth == 0:
                return i
        i += 1

    raise ValueError("Unbalanced braces")


def extract_namespace(content: str) -> str:
    match = re.search(r"namespace\s+([\w.]+)\s*;", content)
    if not match:
        raise ValueError("Namespace not found")
    return match.group(1)


def extract_usings(content: str) -> list[str]:
    return re.findall(r"^using\s+([^;]+);", content, re.MULTILINE)


def find_type_blocks(content: str, kind: str) -> list[tuple[int, int, str]]:
    """Find public sealed record/class blocks. Returns (start, end_exclusive, text)."""
    pattern = re.compile(r"^public\s+sealed\s+(record|class)\s+", re.MULTILINE)
    blocks: list[tuple[int, int, str]] = []
    for match in pattern.finditer(content):
        start = match.start()
        subtype = match.group(1)
        if kind == "record" and subtype != "record":
            continue
        if kind == "class" and subtype != "class":
            continue

        brace_index = content.find("{", match.end())
        if brace_index == -1:
            semi = content.find(";", match.end())
            if semi == -1:
                raise ValueError(f"Could not parse {kind} at {start}")
            end = semi + 1
        else:
            end = find_matching_brace(content, brace_index) + 1

        blocks.append((start, end, content[start:end].strip()))

    return blocks


def classify_class(class_text: str) -> str:
    if ": AbstractValidator<" in class_text:
        return "validator"
    if ": IRequestHandler<" in class_text:
        return "handler"
    return "other"


def minimal_command_usings(original_usings: list[str], dto_text: str, namespace: str) -> list[str]:
    usings = list(MINIMAL_USINGS)

    for using in original_usings:
        if using.startswith("TaskManagementSystem.Modules."):
            module_part = using.split(".")[2] if len(using.split(".")) > 2 else ""
            if module_part and module_part in namespace:
                if using not in usings:
                    usings.append(using)

    for using in original_usings:
        simple = using.split(".")[-1]
        if simple and re.search(rf"\b{re.escape(simple)}\b", dto_text):
            if using not in usings:
                usings.append(using)

    return usings


def handler_usings(original_usings: list[str]) -> list[str]:
    usings = [u for u in original_usings if u != "FluentValidation"]
    if "MediatR" not in usings:
        usings.insert(0, "MediatR")
    return usings


def validator_usings() -> list[str]:
    return ["FluentValidation"]


def write_file(path: Path, namespace: str, usings: list[str], body: str) -> None:
    lines = [f"using {u};" for u in usings]
    lines.append("")
    lines.append(f"namespace {namespace};")
    lines.append("")
    lines.append(body)
    lines.append("")
    path.write_text("\n".join(lines), encoding="utf-8")


def split_file(path: Path, dry_run: bool = False) -> bool:
    content = path.read_text(encoding="utf-8-sig")
    if "IRequestHandler<" not in content:
        return False

    namespace = extract_namespace(content)
    original_usings = extract_usings(content)

    records = find_type_blocks(content, "record")
    classes = find_type_blocks(content, "class")

    if not records:
        print(f"SKIP (no record): {path}")
        return False

    handlers = [c for c in classes if classify_class(c[2]) == "handler"]
    validators = [c for c in classes if classify_class(c[2]) == "validator"]

    if not handlers:
        print(f"SKIP (no handler): {path}")
        return False

    if len(handlers) > 1:
        print(f"WARN multiple handlers: {path}")

    dto_text = "\n\n".join(block[2] for block in records)
    handler_text = handlers[0][2]
    validator_text = validators[0][2] if validators else None

    stem = path.stem
    suffix = "Command" if stem.endswith("Command") else "Query"
    base_name = stem
    handler_path = path.with_name(f"{base_name}Handler.cs")
    validator_path = path.with_name(f"{base_name}Validator.cs")

    if validator_text and validator_path.exists():
        validator_text = None

    dto_usings = minimal_command_usings(original_usings, dto_text, namespace)

    if dry_run:
        print(f"SPLIT: {path}")
        return True

    write_file(path, namespace, dto_usings, dto_text)
    write_file(handler_path, namespace, handler_usings(original_usings), handler_text)

    if validator_text:
        write_file(validator_path, namespace, validator_usings(), validator_text)

    print(f"OK: {path.name} -> handler" + (" + validator" if validator_text else ""))
    return True


def main() -> int:
    dry_run = "--dry-run" in sys.argv
    module_filter = [a for a in sys.argv[1:] if not a.startswith("--")]

    count = 0
    for module_dir in sorted(MODULES_ROOT.iterdir()):
        if not module_dir.is_dir():
            continue
        module_name = module_dir.name.replace("TaskManagementSystem.Modules.", "")
        if module_name in SKIP_MODULES:
            continue
        if module_filter and module_name not in module_filter:
            continue

        features = module_dir / f"TaskManagementSystem.Modules.{module_name}" / "Features"
        if not features.exists():
            continue

        for pattern in ("*Command.cs", "*Query.cs"):
            for path in sorted(features.rglob(pattern)):
                if path.name.endswith("Handler.cs") or path.name.endswith("Validator.cs"):
                    continue
                if split_file(path, dry_run=dry_run):
                    count += 1

    print(f"\nProcessed {count} files")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
