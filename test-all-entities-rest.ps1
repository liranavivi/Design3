# EntitiesManager - Complete REST API Testing Script
# Tests all 11 entities with full CRUD operations

$baseUrl = "http://localhost:5130/api"
$entities = @(
    "sources", "steps", "destinations", "protocols", "importers",
    "exporters", "processors", "processingchains", "flows",
    "taskscheduleds", "scheduledflows"
)

Write-Host "🚀 Starting Complete REST API Testing for All 11 Entities" -ForegroundColor Green
Write-Host "=" * 70

$results = @{}
$createdIds = @{}

foreach ($entity in $entities) {
    Write-Host "`n📋 Testing Entity: $($entity.ToUpper())" -ForegroundColor Cyan
    Write-Host "-" * 50

    $results[$entity] = @{
        "Create" = $false
        "GetAll" = $false
        "GetById" = $false
        "GetByKey" = $false
        "Update" = $false
        "Delete" = $false
    }

    try {
        # 1. CREATE
        Write-Host "1️⃣ Creating $entity..." -ForegroundColor Yellow
        $createData = @{
            address = "tcp://test-$entity.example.com:8080"
            version = "1.0.0"
            name = "Test$($entity.Substring(0,1).ToUpper() + $entity.Substring(1))"
            configuration = @{
                timeout = 30
                retries = 3
                testMode = $true
                entityType = $entity
            }
        } | ConvertTo-Json -Depth 3

        $createResponse = Invoke-RestMethod -Uri "$baseUrl/$entity" -Method POST -Body $createData -ContentType "application/json"
        $createdIds[$entity] = $createResponse.id
        $results[$entity]["Create"] = $true
        Write-Host "   ✅ Created with ID: $($createResponse.id)" -ForegroundColor Green

        # 2. GET ALL
        Write-Host "2️⃣ Getting all $entity..." -ForegroundColor Yellow
        $getAllResponse = Invoke-RestMethod -Uri "$baseUrl/$entity" -Method GET
        $results[$entity]["GetAll"] = $getAllResponse.Count -gt 0
        Write-Host "   ✅ Retrieved $($getAllResponse.Count) items" -ForegroundColor Green

        # 3. GET BY ID
        Write-Host "3️⃣ Getting $entity by ID..." -ForegroundColor Yellow
        $getByIdResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method GET
        $results[$entity]["GetById"] = $getByIdResponse.id -eq $createResponse.id
        Write-Host "   ✅ Retrieved by ID successfully" -ForegroundColor Green

        # 4. GET BY COMPOSITE KEY
        Write-Host "4️⃣ Getting $entity by composite key..." -ForegroundColor Yellow
        $getByKeyResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/by-key/tcp://test-$entity.example.com:8080/1.0.0" -Method GET
        $results[$entity]["GetByKey"] = $getByKeyResponse.id -eq $createResponse.id
        Write-Host "   ✅ Retrieved by composite key successfully" -ForegroundColor Green

        # 5. UPDATE
        Write-Host "5️⃣ Updating $entity..." -ForegroundColor Yellow
        $updateData = $createResponse
        $updateData.name = "Updated$($entity.Substring(0,1).ToUpper() + $entity.Substring(1))"
        $updateData.configuration.updated = $true
        $updateJson = $updateData | ConvertTo-Json -Depth 3

        $updateResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method PUT -Body $updateJson -ContentType "application/json"
        $results[$entity]["Update"] = $updateResponse.name.StartsWith("Updated")
        Write-Host "   ✅ Updated successfully" -ForegroundColor Green

        # 6. DELETE
        Write-Host "6️⃣ Deleting $entity..." -ForegroundColor Yellow
        Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method DELETE
        $results[$entity]["Delete"] = $true
        Write-Host "   ✅ Deleted successfully" -ForegroundColor Green

    } catch {
        Write-Host "   ❌ Error testing $entity`: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Summary Report
Write-Host "`n🏆 COMPLETE REST API TEST RESULTS" -ForegroundColor Green
Write-Host "=" * 70

$totalTests = 0
$passedTests = 0

foreach ($entity in $entities) {
    Write-Host "`n📊 $($entity.ToUpper()):" -ForegroundColor Cyan
    foreach ($operation in @("Create", "GetAll", "GetById", "GetByKey", "Update", "Delete")) {
        $status = if ($results[$entity][$operation]) { "✅ PASS" } else { "❌ FAIL" }
        Write-Host "   $operation`: $status"
        $totalTests++
        if ($results[$entity][$operation]) { $passedTests++ }
    }
}

Write-Host "`n📈 SUMMARY:" -ForegroundColor Green
Write-Host "   Total Tests: $totalTests"
Write-Host "   Passed: $passedTests"
Write-Host "   Failed: $($totalTests - $passedTests)"
Write-Host "   Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 2))%"

if ($passedTests -eq $totalTests) {
    Write-Host "`n🎉 ALL TESTS PASSED! REST API is fully functional!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️ Some tests failed. Check the results above." -ForegroundColor Yellow
}
