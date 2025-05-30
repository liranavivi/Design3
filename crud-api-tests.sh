#!/bin/bash

# EntitiesManager CRUD REST API Tests
# Comprehensive test suite for Source and Destination entities

set -e

API_BASE="http://localhost:5130/api"
TEST_RESULTS=()
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test helper functions
log_test() {
    echo -e "${BLUE}[TEST]${NC} $1"
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
}

log_success() {
    echo -e "${GREEN}[PASS]${NC} $1"
    PASSED_TESTS=$((PASSED_TESTS + 1))
    TEST_RESULTS+=("✅ $1")
}

log_failure() {
    echo -e "${RED}[FAIL]${NC} $1"
    FAILED_TESTS=$((FAILED_TESTS + 1))
    TEST_RESULTS+=("❌ $1")
}

log_info() {
    echo -e "${YELLOW}[INFO]${NC} $1"
}

# Test API health
test_health() {
    log_test "API Health Check"
    response=$(curl -s -w "%{http_code}" -o /dev/null "$API_BASE/../health")
    if [ "$response" = "200" ]; then
        log_success "API is healthy"
    else
        log_failure "API health check failed (HTTP $response)"
        exit 1
    fi
}

# Test Source Entity CRUD Operations
test_source_crud() {
    log_info "=== SOURCE ENTITY CRUD TESTS ==="

    # Test 1: Create Source
    log_test "Create Source Entity"
    create_response=$(curl -s -X POST "$API_BASE/sources" \
        -H "Content-Type: application/json" \
        -d '{
            "address": "test-crud-source",
            "version": "1.0.0",
            "name": "CRUD Test Source",
            "configuration": {
                "endpoint": "http://api.test.com",
                "timeout": 30,
                "retries": 3,
                "auth": {
                    "type": "bearer",
                    "token": "test-token"
                }
            }
        }')

    source_id=$(echo "$create_response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    if [ -n "$source_id" ] && [ "$source_id" != "null" ]; then
        log_success "Source created with ID: $source_id"
    else
        log_failure "Failed to create source: $create_response"
        return 1
    fi

    # Test 2: Get Source by ID
    log_test "Get Source by ID"
    get_response=$(curl -s "$API_BASE/sources/$source_id")
    get_id=$(echo "$get_response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    if [ "$get_id" = "$source_id" ]; then
        log_success "Source retrieved successfully"
    else
        log_failure "Failed to retrieve source by ID"
    fi

    # Test 3: Get All Sources
    log_test "Get All Sources"
    all_response=$(curl -s "$API_BASE/sources")
    if echo "$all_response" | grep -q "$source_id"; then
        log_success "Source found in all sources list"
    else
        log_failure "Source not found in all sources list"
    fi

    # Test 4: Get Source by Composite Key
    log_test "Get Source by Composite Key"
    key_response=$(curl -s "$API_BASE/sources/by-key/test-crud-source/1.0.0")
    key_id=$(echo "$key_response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    if [ "$key_id" = "$source_id" ]; then
        log_success "Source retrieved by composite key"
    else
        log_failure "Failed to retrieve source by composite key"
    fi

    # Test 5: Update Source
    log_test "Update Source Entity"
    update_response=$(curl -s -X PUT "$API_BASE/sources/$source_id" \
        -H "Content-Type: application/json" \
        -d "{
            \"id\": \"$source_id\",
            \"address\": \"test-crud-source\",
            \"version\": \"1.0.0\",
            \"name\": \"Updated CRUD Test Source\",
            \"configuration\": {
                \"endpoint\": \"http://updated-api.test.com\",
                \"timeout\": 60,
                \"retries\": 5,
                \"auth\": {
                    \"type\": \"oauth2\",
                    \"clientId\": \"test-client\"
                },
                \"features\": {
                    \"caching\": true,
                    \"compression\": \"gzip\"
                }
            }
        }")

    updated_name=$(echo "$update_response" | grep -o '"name":"[^"]*"' | cut -d'"' -f4)
    if [ "$updated_name" = "Updated CRUD Test Source" ]; then
        log_success "Source updated successfully"
    else
        log_failure "Failed to update source"
    fi

    # Test 6: Verify Update
    log_test "Verify Source Update"
    verify_response=$(curl -s "$API_BASE/sources/$source_id")
    verify_name=$(echo "$verify_response" | grep -o '"name":"[^"]*"' | cut -d'"' -f4)
    if [ "$verify_name" = "Updated CRUD Test Source" ]; then
        log_success "Source update verified"
    else
        log_failure "Source update verification failed"
    fi

    # Test 7: Delete Source
    log_test "Delete Source Entity"
    delete_response=$(curl -s -w "%{http_code}" -o /dev/null -X DELETE "$API_BASE/sources/$source_id")
    if [ "$delete_response" = "204" ]; then
        log_success "Source deleted successfully"
    else
        log_failure "Failed to delete source (HTTP $delete_response)"
    fi

    # Test 8: Verify Deletion
    log_test "Verify Source Deletion"
    verify_delete_response=$(curl -s -w "%{http_code}" -o /dev/null "$API_BASE/sources/$source_id")
    if [ "$verify_delete_response" = "404" ]; then
        log_success "Source deletion verified"
    else
        log_failure "Source deletion verification failed (HTTP $verify_delete_response)"
    fi
}

# Test Destination Entity CRUD Operations
test_destination_crud() {
    log_info "=== DESTINATION ENTITY CRUD TESTS ==="

    # Test 1: Create Destination
    log_test "Create Destination Entity"
    create_response=$(curl -s -X POST "$API_BASE/destinations" \
        -H "Content-Type: application/json" \
        -d '{
            "name": "CRUD Test Destination",
            "version": "1.0.0",
            "inputSchema": "{\"type\": \"object\", \"properties\": {\"id\": {\"type\": \"string\"}, \"data\": {\"type\": \"object\"}, \"timestamp\": {\"type\": \"string\", \"format\": \"date-time\"}}}"
        }')

    dest_id=$(echo "$create_response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    if [ -n "$dest_id" ] && [ "$dest_id" != "null" ]; then
        log_success "Destination created with ID: $dest_id"
    else
        log_failure "Failed to create destination: $create_response"
        return 1
    fi

    # Test 2: Get Destination by ID
    log_test "Get Destination by ID"
    get_response=$(curl -s "$API_BASE/destinations/$dest_id")
    get_id=$(echo "$get_response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    if [ "$get_id" = "$dest_id" ]; then
        log_success "Destination retrieved successfully"
    else
        log_failure "Failed to retrieve destination by ID"
    fi

    # Test 3: Get All Destinations
    log_test "Get All Destinations"
    all_response=$(curl -s "$API_BASE/destinations")
    if echo "$all_response" | grep -q "$dest_id"; then
        log_success "Destination found in all destinations list"
    else
        log_failure "Destination not found in all destinations list"
    fi

    # Test 4: Get Destination by Composite Key
    log_test "Get Destination by Composite Key"
    key_response=$(curl -s "$API_BASE/destinations/by-key/CRUD%20Test%20Destination/1.0.0")
    key_id=$(echo "$key_response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    if [ "$key_id" = "$dest_id" ]; then
        log_success "Destination retrieved by composite key"
    else
        log_failure "Failed to retrieve destination by composite key"
    fi

    # Test 5: Update Destination
    log_test "Update Destination Entity"
    update_response=$(curl -s -X PUT "$API_BASE/destinations/$dest_id" \
        -H "Content-Type: application/json" \
        -d "{
            \"id\": \"$dest_id\",
            \"name\": \"CRUD Test Destination\",
            \"version\": \"1.0.0\",
            \"inputSchema\": \"{\\\"type\\\": \\\"object\\\", \\\"properties\\\": {\\\"id\\\": {\\\"type\\\": \\\"string\\\"}, \\\"data\\\": {\\\"type\\\": \\\"object\\\"}, \\\"timestamp\\\": {\\\"type\\\": \\\"string\\\", \\\"format\\\": \\\"date-time\\\"}, \\\"metadata\\\": {\\\"type\\\": \\\"object\\\", \\\"properties\\\": {\\\"source\\\": {\\\"type\\\": \\\"string\\\"}, \\\"priority\\\": {\\\"type\\\": \\\"integer\\\"}}}}}\"
        }")

    if echo "$update_response" | grep -q "metadata"; then
        log_success "Destination updated successfully"
    else
        log_failure "Failed to update destination"
    fi

    # Test 6: Delete Destination
    log_test "Delete Destination Entity"
    delete_response=$(curl -s -w "%{http_code}" -o /dev/null -X DELETE "$API_BASE/destinations/$dest_id")
    if [ "$delete_response" = "204" ]; then
        log_success "Destination deleted successfully"
    else
        log_failure "Failed to delete destination (HTTP $delete_response)"
    fi

    # Test 7: Verify Deletion
    log_test "Verify Destination Deletion"
    verify_delete_response=$(curl -s -w "%{http_code}" -o /dev/null "$API_BASE/destinations/$dest_id")
    if [ "$verify_delete_response" = "404" ]; then
        log_success "Destination deletion verified"
    else
        log_failure "Destination deletion verification failed (HTTP $verify_delete_response)"
    fi
}

# Test Error Handling
test_error_handling() {
    log_info "=== ERROR HANDLING TESTS ==="

    # Test 1: Get Non-existent Entity
    log_test "Get Non-existent Source"
    fake_id="00000000-0000-0000-0000-000000000000"
    response=$(curl -s -w "%{http_code}" -o /dev/null "$API_BASE/sources/$fake_id")
    if [ "$response" = "404" ]; then
        log_success "Non-existent source returns 404"
    else
        log_failure "Non-existent source should return 404, got $response"
    fi

    # Test 2: Invalid JSON
    log_test "Create Source with Invalid JSON"
    response=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$API_BASE/sources" \
        -H "Content-Type: application/json" \
        -d '{"invalid": json}')
    if [ "$response" = "400" ]; then
        log_success "Invalid JSON returns 400"
    else
        log_failure "Invalid JSON should return 400, got $response"
    fi

    # Test 3: Missing Required Fields
    log_test "Create Source with Missing Required Fields"
    response=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$API_BASE/sources" \
        -H "Content-Type: application/json" \
        -d '{"name": "Test"}')
    if [ "$response" = "400" ]; then
        log_success "Missing required fields returns 400"
    else
        log_failure "Missing required fields should return 400, got $response"
    fi
}

# Main test execution
main() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}  EntitiesManager CRUD REST API Tests  ${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""

    # Run all test suites
    test_health
    test_source_crud
    test_destination_crud
    test_error_handling

    # Print summary
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}           TEST SUMMARY                 ${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo -e "Total Tests: ${TOTAL_TESTS}"
    echo -e "${GREEN}Passed: ${PASSED_TESTS}${NC}"
    echo -e "${RED}Failed: ${FAILED_TESTS}${NC}"
    echo ""

    # Print detailed results
    for result in "${TEST_RESULTS[@]}"; do
        echo -e "$result"
    done

    echo ""
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "${GREEN}🎉 ALL TESTS PASSED! 🎉${NC}"
        exit 0
    else
        echo -e "${RED}❌ SOME TESTS FAILED ❌${NC}"
        exit 1
    fi
}

# Run the tests
main "$@"
