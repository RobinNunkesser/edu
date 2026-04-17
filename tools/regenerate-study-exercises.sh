#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
IMPORT_PROJECT="$PROJECT_ROOT/tools/ExamGenerator.Import/ExamGenerator.Import.csproj"
CONTENT_DIR="$PROJECT_ROOT/src/WebSite/wwwroot/content/study"
LATEX_DIR="${1:-/tmp/edu-study-exercises}"

mkdir -p "$CONTENT_DIR" "$LATEX_DIR"

run_export() {
  local task_name="$1"
  local language="$2"

  dotnet run --project "$IMPORT_PROJECT" -- \
    --task "$task_name" \
    --language "$language" \
    --output-json "$CONTENT_DIR/${task_name}.${language}.json" \
    --output-tex "$LATEX_DIR/${task_name}.${language}.tex"
}

for task_name in binary-addition binary-to-decimal decimal-to-binary twos-complement; do
  for language in de en; do
    run_export "$task_name" "$language"
  done
done

echo "Regenerated study exercise imports in $CONTENT_DIR"
echo "Wrote LaTeX previews to $LATEX_DIR"