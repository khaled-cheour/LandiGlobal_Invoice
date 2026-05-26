#!/bin/pwsh
# Test Script for Product Validation APIs

$baseUrl = "http://localhost:5206"

Write-Host "=== Testing Product Validation APIs ===" -ForegroundColor Green
Write-Host ""

# Test 1: Valid Product
Write-Host "Test 1: Valid Product (53004-00--)" -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/product/validate/53004-00--" -Method Get
Write-Host "Response:" -ForegroundColor Cyan
$response | ConvertTo-Json | Write-Host
Write-Host ""

# Test 2: Valid Product (Accessory)
Write-Host "Test 2: Valid Product - Accessory (BM14000026)" -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/product/validate/BM14000026" -Method Get
Write-Host "Response:" -ForegroundColor Cyan
$response | ConvertTo-Json | Write-Host
Write-Host ""

# Test 3: Valid Product (C20Lite)
Write-Host "Test 3: Valid Product - C20Lite (8530M-00--)" -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$baseUrl/api/product/validate/8530M-00--" -Method Get
Write-Host "Response:" -ForegroundColor Cyan
$response | ConvertTo-Json | Write-Host
Write-Host ""

# Test 4: Invalid Product
Write-Host "Test 4: Invalid Product (INVALID123)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/product/validate/INVALID123" -Method Get
    Write-Host "Response:" -ForegroundColor Cyan
    $response | ConvertTo-Json | Write-Host
} catch {
    Write-Host "Error (Expected): Invalid product not found" -ForegroundColor Red
}
Write-Host ""

# Test 5: Enrich Product
Write-Host "Test 5: Enrich Product via API" -ForegroundColor Yellow
$productData = @{
    ProductNumber = "53004-00--"
    Quantity = 110
    UnitPrice = 162.50
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "$baseUrl/api/product/enrich" -Method Post -Body $productData -ContentType "application/json"
Write-Host "Response:" -ForegroundColor Cyan
$response | ConvertTo-Json | Write-Host
Write-Host ""

Write-Host "=== All Tests Completed ===" -ForegroundColor Green
