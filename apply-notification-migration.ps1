# PowerShell script to apply the notification migration and verify the fix

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Notification Migration Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if we're in the correct directory
if (-not (Test-Path "AutomatedTaskSystem")) {
    Write-Host "ERROR: AutomatedTaskSystem directory not found!" -ForegroundColor Red
    Write-Host "Please run this script from the repository root directory." -ForegroundColor Yellow
    exit 1
}

Write-Host "Step 1: Checking current directory..." -ForegroundColor Green
Write-Host "Current directory: $(Get-Location)" -ForegroundColor Gray
Write-Host ""

# Step 2: Navigate to the backend project
Set-Location "AutomatedTaskSystem"
Write-Host "Step 2: Navigated to AutomatedTaskSystem directory" -ForegroundColor Green
Write-Host ""

# Step 3: Check for pending migrations
Write-Host "Step 3: Checking for pending migrations..." -ForegroundColor Green
Write-Host "Running: dotnet ef migrations list" -ForegroundColor Gray
Write-Host ""

try {
    $migrations = dotnet ef migrations list 2>&1
    Write-Host $migrations -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "WARNING: Could not list migrations. Continuing anyway..." -ForegroundColor Yellow
    Write-Host ""
}

# Step 4: Apply the migration
Write-Host "Step 4: Applying database migrations..." -ForegroundColor Green
Write-Host "Running: dotnet ef database update" -ForegroundColor Gray
Write-Host ""

try {
    $updateResult = dotnet ef database update 2>&1
    Write-Host $updateResult -ForegroundColor Gray
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "SUCCESS: Database migration applied successfully!" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "ERROR: Migration failed. Please check the error messages above." -ForegroundColor Red
        Write-Host ""
        Set-Location ..
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to apply migration: $_" -ForegroundColor Red
    Write-Host ""
    Set-Location ..
    exit 1
}

# Step 5: Verify the AdditionalData column exists
Write-Host "Step 5: Verifying the Notifications table structure..." -ForegroundColor Green
Write-Host "Please manually verify that the 'AdditionalData' column exists in the Notifications table." -ForegroundColor Yellow
Write-Host ""

# Step 6: Return to original directory
Set-Location ..
Write-Host "Step 6: Returned to original directory" -ForegroundColor Green
Write-Host ""

# Step 7: Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Migration Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Restart the backend application if it's running" -ForegroundColor White
Write-Host "2. Test task assignment notifications:" -ForegroundColor White
Write-Host "   - Assign a task to a user" -ForegroundColor White
Write-Host "   - Check for toast notification (real-time)" -ForegroundColor White
Write-Host "   - Check /notifications page (persistent)" -ForegroundColor White
Write-Host ""
Write-Host "If notifications still don't work, check:" -ForegroundColor Yellow
Write-Host "- SignalR connection in browser console (F12)" -ForegroundColor White
Write-Host "- Backend logs for error messages" -ForegroundColor White
Write-Host "- User is logged in with valid authentication" -ForegroundColor White
Write-Host ""
Write-Host "For detailed troubleshooting, see: TASK_NOTIFICATION_FIX_GUIDE.md" -ForegroundColor Cyan
Write-Host ""

