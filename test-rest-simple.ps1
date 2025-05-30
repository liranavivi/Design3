# Simple REST API Test for All Entities
$baseUrl = "http://localhost:5130/api"
$entities = @("sources", "steps", "destinations", "protocols", "importers", "exporters", "processors", "processingchains", "flows", "taskscheduleds", "scheduledflows")

Write-Host "Starting REST API Testing for All 11 Entities"
Write-Host "=============================================="

$totalTests = 0
$passedTests = 0

foreach ($entity in $entities) {
    Write-Host ""
    Write-Host "Testing Entity: $($entity.ToUpper())"
    Write-Host "--------------------------------"
    
    try {
        # 1. CREATE
        Write-Host "1. Creating $entity..."
        $createData = @{
            address = "tcp://test-$entity.example.com:8080"
            version = "1.0.0"
            name = "Test$entity"
            configuration = @{
                timeout = 30
                retries = 3
                testMode = $true
            }
        } | ConvertTo-Json -Depth 3
        
        $createResponse = Invoke-RestMethod -Uri "$baseUrl/$entity" -Method POST -Body $createData -ContentType "application/json"
        Write-Host "   SUCCESS - Created with ID: $($createResponse.id)"
        $totalTests++; $passedTests++
        
        # 2. GET ALL
        Write-Host "2. Getting all $entity..."
        $getAllResponse = Invoke-RestMethod -Uri "$baseUrl/$entity" -Method GET
        Write-Host "   SUCCESS - Retrieved $($getAllResponse.Count) items"
        $totalTests++; $passedTests++
        
        # 3. GET BY ID
        Write-Host "3. Getting $entity by ID..."
        $getByIdResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method GET
        Write-Host "   SUCCESS - Retrieved by ID"
        $totalTests++; $passedTests++
        
        # 4. UPDATE
        Write-Host "4. Updating $entity..."
        $updateData = $createResponse
        $updateData.name = "Updated$entity"
        $updateJson = $updateData | ConvertTo-Json -Depth 3
        
        $updateResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method PUT -Body $updateJson -ContentType "application/json"
        Write-Host "   SUCCESS - Updated"
        $totalTests++; $passedTests++
        
        # 5. DELETE
        Write-Host "5. Deleting $entity..."
        Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method DELETE
        Write-Host "   SUCCESS - Deleted"
        $totalTests++; $passedTests++
        
    } catch {
        Write-Host "   ERROR: $($_.Exception.Message)"
        $totalTests += 5
    }
}

Write-Host ""
Write-Host "SUMMARY RESULTS"
Write-Host "==============="
Write-Host "Total Tests: $totalTests"
Write-Host "Passed: $passedTests"
Write-Host "Failed: $($totalTests - $passedTests)"
Write-Host "Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 2))%"

if ($passedTests -eq $totalTests) {
    Write-Host "ALL TESTS PASSED! REST API is fully functional!"
} else {
    Write-Host "Some tests failed. Check the results above."
}
