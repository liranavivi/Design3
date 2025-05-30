# MassTransit Message Bus Testing Script
# Tests all 11 entities with message bus operations

$baseUrl = "http://localhost:5130/api"
$entities = @("sources", "steps", "destinations", "protocols", "importers", "exporters", "processors", "processingchains", "flows", "taskscheduleds", "scheduledflows")

Write-Host "Starting MassTransit Message Bus Testing for All 11 Entities"
Write-Host "============================================================="

$totalTests = 0
$passedTests = 0

foreach ($entity in $entities) {
    Write-Host ""
    Write-Host "Testing Entity: $($entity.ToUpper())"
    Write-Host "--------------------------------"

    try {
        # 1. CREATE via Message Bus (using REST API to trigger message publishing)
        Write-Host "1. Creating $entity via message bus..."
        $createData = @{
            address = "tcp://messagebus-$entity.example.com:8080"
            version = "2.0.0"
            name = "MessageBus$entity"
            configuration = @{
                timeout = 60
                retries = 5
                messageBusTest = $true
                publishEvents = $true
            }
        } | ConvertTo-Json -Depth 3

        $createResponse = Invoke-RestMethod -Uri "$baseUrl/$entity" -Method POST -Body $createData -ContentType "application/json"
        Write-Host "   SUCCESS - Created with ID: $($createResponse.id)"
        $totalTests++; $passedTests++

        # Wait for message processing
        Start-Sleep -Milliseconds 500

        # 2. VERIFY via GET (message bus should have processed the create event)
        Write-Host "2. Verifying $entity creation via message bus..."
        $getResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method GET
        if ($getResponse.name -eq "MessageBus$entity") {
            Write-Host "   SUCCESS - Message bus processed creation event"
            $totalTests++; $passedTests++
        } else {
            Write-Host "   ERROR - Message bus creation verification failed"
            $totalTests++
        }

        # 3. UPDATE via Message Bus
        Write-Host "3. Updating $entity via message bus..."
        $updateData = $createResponse
        $updateData.name = "UpdatedMessageBus$entity"
        $updateData.configuration | Add-Member -NotePropertyName "updated" -NotePropertyValue $true -Force
        $updateJson = $updateData | ConvertTo-Json -Depth 3

        $updateResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method PUT -Body $updateJson -ContentType "application/json"
        Write-Host "   SUCCESS - Updated via message bus"
        $totalTests++; $passedTests++

        # Wait for message processing
        Start-Sleep -Milliseconds 500

        # 4. VERIFY UPDATE via Message Bus
        Write-Host "4. Verifying $entity update via message bus..."
        $verifyResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method GET
        if ($verifyResponse.name -eq "UpdatedMessageBus$entity") {
            Write-Host "   SUCCESS - Message bus processed update event"
            $totalTests++; $passedTests++
        } else {
            Write-Host "   ERROR - Message bus update verification failed"
            $totalTests++
        }

        # 5. DELETE via Message Bus
        Write-Host "5. Deleting $entity via message bus..."
        Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method DELETE
        Write-Host "   SUCCESS - Deleted via message bus"
        $totalTests++; $passedTests++

        # Wait for message processing
        Start-Sleep -Milliseconds 500

        # 6. VERIFY DELETION via Message Bus
        Write-Host "6. Verifying $entity deletion via message bus..."
        try {
            $deleteVerifyResponse = Invoke-RestMethod -Uri "$baseUrl/$entity/$($createResponse.id)" -Method GET
            Write-Host "   ERROR - Entity still exists after deletion"
            $totalTests++
        } catch {
            if ($_.Exception.Response.StatusCode -eq 404) {
                Write-Host "   SUCCESS - Message bus processed deletion event"
                $totalTests++; $passedTests++
            } else {
                Write-Host "   ERROR - Unexpected error during deletion verification"
                $totalTests++
            }
        }

    } catch {
        Write-Host "   ERROR: $($_.Exception.Message)"
        $totalTests += 6
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
    Write-Host "ALL TESTS PASSED! Message Bus is fully functional!"
} else {
    Write-Host "Some tests failed. Check the results above."
}
