#!/bin/bash

# EntitiesManager Advanced REST API Tests
# Tests for pagination, performance, and concurrent operations

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

# Test pagination functionality
test_pagination() {
    log_info "=== PAGINATION TESTS ==="
    
    # Create multiple sources for pagination testing
    log_test "Create Multiple Sources for Pagination"
    created_ids=()
    for i in {1..15}; do
        response=$(curl -s -X POST "$API_BASE/sources" \
            -H "Content-Type: application/json" \
            -d "{
                \"address\": \"pagination-test-$i\",
                \"version\": \"1.0.0\",
                \"name\": \"Pagination Test Source $i\",
                \"configuration\": {\"index\": $i}
            }")
        
        id=$(echo "$response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
        if [ -n "$id" ] && [ "$id" != "null" ]; then
            created_ids+=("$id")
        fi
    done
    
    if [ ${#created_ids[@]} -eq 15 ]; then
        log_success "Created 15 sources for pagination testing"
    else
        log_failure "Failed to create all sources for pagination testing (created ${#created_ids[@]}/15)"
    fi
    
    # Test first page
    log_test "Get First Page (page=1, pageSize=5)"
    page1_response=$(curl -s "$API_BASE/sources/paged?page=1&pageSize=5")
    page1_count=$(echo "$page1_response" | grep -o '"data":\[' | wc -l)
    page1_total=$(echo "$page1_response" | grep -o '"totalCount":[0-9]*' | cut -d':' -f2)
    
    if [ "$page1_count" -eq 1 ] && [ "$page1_total" -ge 15 ]; then
        log_success "First page returned correctly (totalCount: $page1_total)"
    else
        log_failure "First page pagination failed"
    fi
    
    # Test second page
    log_test "Get Second Page (page=2, pageSize=5)"
    page2_response=$(curl -s "$API_BASE/sources/paged?page=2&pageSize=5")
    page2_count=$(echo "$page2_response" | grep -o '"data":\[' | wc -l)
    
    if [ "$page2_count" -eq 1 ]; then
        log_success "Second page returned correctly"
    else
        log_failure "Second page pagination failed"
    fi
    
    # Test page size validation
    log_test "Test Page Size Validation (pageSize=200)"
    large_page_response=$(curl -s "$API_BASE/sources/paged?page=1&pageSize=200")
    actual_page_size=$(echo "$large_page_response" | grep -o '"pageSize":[0-9]*' | cut -d':' -f2)
    
    if [ "$actual_page_size" -eq 10 ]; then
        log_success "Page size validation works (limited to 10)"
    else
        log_failure "Page size validation failed (got $actual_page_size)"
    fi
    
    # Cleanup pagination test data
    log_test "Cleanup Pagination Test Data"
    cleanup_count=0
    for id in "${created_ids[@]}"; do
        delete_response=$(curl -s -w "%{http_code}" -o /dev/null -X DELETE "$API_BASE/sources/$id")
        if [ "$delete_response" = "204" ]; then
            cleanup_count=$((cleanup_count + 1))
        fi
    done
    
    if [ "$cleanup_count" -eq ${#created_ids[@]} ]; then
        log_success "Cleaned up all pagination test data"
    else
        log_failure "Failed to cleanup all pagination test data ($cleanup_count/${#created_ids[@]})"
    fi
}

# Test concurrent operations
test_concurrent_operations() {
    log_info "=== CONCURRENT OPERATIONS TESTS ==="
    
    # Test concurrent creates
    log_test "Concurrent Create Operations"
    pids=()
    temp_files=()
    
    for i in {1..5}; do
        temp_file="/tmp/concurrent_test_$i.json"
        temp_files+=("$temp_file")
        
        (
            curl -s -X POST "$API_BASE/sources" \
                -H "Content-Type: application/json" \
                -d "{
                    \"address\": \"concurrent-test-$i\",
                    \"version\": \"1.0.0\",
                    \"name\": \"Concurrent Test Source $i\",
                    \"configuration\": {\"thread\": $i}
                }" > "$temp_file"
        ) &
        pids+=($!)
    done
    
    # Wait for all concurrent operations to complete
    for pid in "${pids[@]}"; do
        wait $pid
    done
    
    # Check results
    success_count=0
    created_ids=()
    for temp_file in "${temp_files[@]}"; do
        if [ -f "$temp_file" ]; then
            id=$(grep -o '"id":"[^"]*"' "$temp_file" | cut -d'"' -f4)
            if [ -n "$id" ] && [ "$id" != "null" ]; then
                success_count=$((success_count + 1))
                created_ids+=("$id")
            fi
            rm -f "$temp_file"
        fi
    done
    
    if [ "$success_count" -eq 5 ]; then
        log_success "All 5 concurrent creates succeeded"
    else
        log_failure "Concurrent creates failed ($success_count/5 succeeded)"
    fi
    
    # Cleanup concurrent test data
    for id in "${created_ids[@]}"; do
        curl -s -X DELETE "$API_BASE/sources/$id" > /dev/null
    done
}

# Test performance with bulk operations
test_performance() {
    log_info "=== PERFORMANCE TESTS ==="
    
    # Test bulk create performance
    log_test "Bulk Create Performance (10 entities)"
    start_time=$(date +%s%N)
    
    created_ids=()
    for i in {1..10}; do
        response=$(curl -s -X POST "$API_BASE/destinations" \
            -H "Content-Type: application/json" \
            -d "{
                \"name\": \"Performance Test Destination $i\",
                \"version\": \"1.0.0\",
                \"inputSchema\": \"{\\\"type\\\": \\\"object\\\", \\\"properties\\\": {\\\"id\\\": {\\\"type\\\": \\\"string\\\"}}}\"
            }")
        
        id=$(echo "$response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
        if [ -n "$id" ] && [ "$id" != "null" ]; then
            created_ids+=("$id")
        fi
    done
    
    end_time=$(date +%s%N)
    duration_ms=$(( (end_time - start_time) / 1000000 ))
    
    if [ ${#created_ids[@]} -eq 10 ] && [ $duration_ms -lt 5000 ]; then
        log_success "Bulk create completed in ${duration_ms}ms (10 entities)"
    else
        log_failure "Bulk create performance test failed (${#created_ids[@]}/10 entities, ${duration_ms}ms)"
    fi
    
    # Test bulk read performance
    log_test "Bulk Read Performance (Get All)"
    start_time=$(date +%s%N)
    
    response=$(curl -s "$API_BASE/destinations")
    
    end_time=$(date +%s%N)
    duration_ms=$(( (end_time - start_time) / 1000000 ))
    
    entity_count=$(echo "$response" | grep -o '"id":"[^"]*"' | wc -l)
    
    if [ $entity_count -ge 10 ] && [ $duration_ms -lt 2000 ]; then
        log_success "Bulk read completed in ${duration_ms}ms ($entity_count entities)"
    else
        log_failure "Bulk read performance test failed ($entity_count entities, ${duration_ms}ms)"
    fi
    
    # Cleanup performance test data
    for id in "${created_ids[@]}"; do
        curl -s -X DELETE "$API_BASE/destinations/$id" > /dev/null
    done
}

# Test data validation and edge cases
test_edge_cases() {
    log_info "=== EDGE CASE TESTS ==="
    
    # Test very long names
    log_test "Create Source with Maximum Length Name"
    long_name=$(printf 'A%.0s' {1..200})
    response=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$API_BASE/sources" \
        -H "Content-Type: application/json" \
        -d "{
            \"address\": \"edge-case-test\",
            \"version\": \"1.0.0\",
            \"name\": \"$long_name\",
            \"configuration\": {}
        }")
    
    if [ "$response" = "201" ]; then
        log_success "Maximum length name accepted"
        # Cleanup
        curl -s -X DELETE "$API_BASE/sources/by-key/edge-case-test/1.0.0" > /dev/null 2>&1
    else
        log_failure "Maximum length name rejected (HTTP $response)"
    fi
    
    # Test special characters in composite key
    log_test "Create Source with Special Characters"
    response=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$API_BASE/sources" \
        -H "Content-Type: application/json" \
        -d '{
            "address": "test-special-chars",
            "version": "1.0.0-beta+build.123",
            "name": "Special Characters Test",
            "configuration": {}
        }')
    
    if [ "$response" = "201" ]; then
        log_success "Special characters in version accepted"
        # Cleanup
        curl -s -X DELETE "$API_BASE/sources/by-key/test-special-chars/1.0.0-beta+build.123" > /dev/null 2>&1
    else
        log_failure "Special characters in version rejected (HTTP $response)"
    fi
    
    # Test empty configuration
    log_test "Create Source with Empty Configuration"
    response=$(curl -s -X POST "$API_BASE/sources" \
        -H "Content-Type: application/json" \
        -d '{
            "address": "empty-config-test",
            "version": "1.0.0",
            "name": "Empty Config Test",
            "configuration": {}
        }')
    
    id=$(echo "$response" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    if [ -n "$id" ] && [ "$id" != "null" ]; then
        log_success "Empty configuration accepted"
        # Cleanup
        curl -s -X DELETE "$API_BASE/sources/$id" > /dev/null
    else
        log_failure "Empty configuration rejected"
    fi
}

# Main test execution
main() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}  EntitiesManager Advanced API Tests   ${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""
    
    # Run all test suites
    test_pagination
    test_concurrent_operations
    test_performance
    test_edge_cases
    
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
        echo -e "${GREEN}🎉 ALL ADVANCED TESTS PASSED! 🎉${NC}"
        exit 0
    else
        echo -e "${RED}❌ SOME ADVANCED TESTS FAILED ❌${NC}"
        exit 1
    fi
}

# Run the tests
main "$@"
