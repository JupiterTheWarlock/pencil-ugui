$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$OpenPencilDir = Join-Path $RepoRoot "references/open-pencil-complete"
$FigPath = Join-Path $RepoRoot "samples/harness/open-pencil-settings-panel.fig"
$SeedPath = Join-Path $RepoRoot "samples/harness/open-pencil-seed.pen"
$CreateScript = Join-Path $RepoRoot "samples/harness/open-pencil-create-settings.js"
$UiIrPath = Join-Path $RepoRoot "samples/ui-ir/settings-panel.json"
$ExporterDir = Join-Path $RepoRoot "tools/open-pencil-exporter"

Write-Host "Step 1/2: Regenerate OpenPencil .fig"
Push-Location $OpenPencilDir
try {
  cmd /c "bun run open-pencil -- eval `"$SeedPath`" --stdin --output `"$FigPath`" --json < `"$CreateScript`""
  if ($LASTEXITCODE -ne 0) {
    throw "OpenPencil eval failed with exit code $LASTEXITCODE"
  }
}
finally {
  Pop-Location
}

Write-Host "Step 2/2: Export UI IR"
$CliPath = Join-Path $RepoRoot "tools/openpencil-ugui-cli/bin/openpencil-ugui.mjs"
if (Test-Path $CliPath) {
  node $CliPath export --input "samples/harness/open-pencil-settings-panel.fig" --output "samples/ui-ir/settings-panel.json"
  if ($LASTEXITCODE -ne 0) {
    throw "UI IR export failed with exit code $LASTEXITCODE"
  }
} else {
  Push-Location $ExporterDir
  try {
    npm run export:settings-panel
    if ($LASTEXITCODE -ne 0) {
      throw "UI IR export failed with exit code $LASTEXITCODE"
    }
  }
  finally {
    Pop-Location
  }
}

Write-Host ""
Write-Host "Done."
Write-Host "  Fig:    $FigPath"
Write-Host "  UI IR:  $UiIrPath"
Write-Host ""
Write-Host "Next: In Unity, select a Canvas and use Tools > Pencil UGUI > Import UI IR..."
