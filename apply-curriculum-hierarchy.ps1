# Applies curriculum hierarchy migration (drops Folders, adds explicit tables):
# Year -> Project -> Term -> Subject Group -> Subject records

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Curriculum Hierarchy Migration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path "AutomatedTaskSystem")) {
    Write-Host "ERROR: Run this script from the repository root." -ForegroundColor Red
    exit 1
}

Set-Location "AutomatedTaskSystem"

Write-Host "Applying EF migrations (ReplaceFoldersWithCurriculumHierarchy)..." -ForegroundColor Green
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration failed." -ForegroundColor Red
    Set-Location ..
    exit 1
}

Set-Location ..
Write-Host ""
Write-Host "Done. Expected hierarchy in DB:" -ForegroundColor Green
Write-Host "  AcademicYears (Year)" -ForegroundColor White
Write-Host "    -> CurriculumProjects (Project)" -ForegroundColor White
Write-Host "         -> CurriculumTerms (Term)" -ForegroundColor White
Write-Host "              -> SubjectGroups (Subject Group)" -ForegroundColor White
Write-Host "                   -> Subjects table rows" -ForegroundColor White
Write-Host ""
Write-Host "API route: /curriculum/*" -ForegroundColor Gray
Write-Host "Restart the API after migration." -ForegroundColor Yellow
