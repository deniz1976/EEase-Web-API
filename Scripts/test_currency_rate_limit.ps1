# Test script for currency rate limiting
$baseUrl = "https://localhost:7207/api/Currency"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiZGVuaXoxOTc2IiwibmJmIjoxNzM2ODk3NTI4LCJleHAiOjE3MzY4OTgxMjgsImlzcyI6Ind3dy5teWFwaS5jb20iLCJhdWQiOiJ3d3cuYmlsbWVtbmUuY29tIn0.Zl1fMIfI6u_si_XXaNVlyB5GDHBSU54vuL46y_sREGY"

$headers = @{
    "Authorization" = "Bearer $token"
}

$totalRequests = 120
$successCount = 0
$failureCount = 0
$startTime = Get-Date

Write-Host "Starting currency rate limit test at $startTime"
Write-Host "Using URL: $baseUrl"
Write-Host "Sending $totalRequests requests..."

for ($i = 1; $i -le $totalRequests; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $baseUrl -Headers $headers -Method Get
        $successCount++
        Write-Host "Request $i : Success (Status: $($response.StatusCode))" -ForegroundColor Green
    }
    catch {
        $failureCount++
        $errorMessage = $_.Exception.Message
        $statusCode = $_.Exception.Response.StatusCode.value__

        if ($statusCode -eq 429) {
            Write-Host "Request $i : Rate Limit Exceeded (Status: 429)" -ForegroundColor Yellow
        }
        else {
            Write-Host "Request $i : Failed (Status: $statusCode) - Error: $errorMessage" -ForegroundColor Red
        }
    }
    Start-Sleep -Milliseconds 100
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host "`nTest completed at $endTime"
Write-Host "Duration: $duration seconds"
Write-Host "Successful requests: $successCount"
Write-Host "Failed requests: $failureCount"
Write-Host "Success rate: $([math]::Round(($successCount/$totalRequests)*100, 2))%"