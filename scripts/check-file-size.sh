#!/usr/bin/env bash
# Fails when a hand-written source file grows past the line limit, to keep the codebase easy to read.
set -euo pipefail

limit="${1:-250}"
status=0

while IFS= read -r file; do
  lines=$(wc -l < "$file")
  if [ "$lines" -gt "$limit" ]; then
    echo "TOO LONG ($lines > $limit lines): $file"
    status=1
  fi
done < <(find src tests tools web/src -type f \( -name '*.cs' -o -name '*.ts' -o -name '*.tsx' \) \
  -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/node_modules/*' \
  -not -path '*/generated/*' -not -path '*/Migrations/*' 2>/dev/null)

if [ "$status" -eq 0 ]; then
  echo "All source files are within $limit lines."
fi
exit "$status"
