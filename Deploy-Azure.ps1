# Deploy-Azure.ps1
# This script will provision your Azure resources and deploy the TaskPulse code.

Write-Host "Starting Azure Deployment for TaskPulse..." -ForegroundColor Cyan

# 1. Authenticate (This will pop up a browser for you)
Write-Host "Logging into Azure..." -ForegroundColor Yellow
Connect-AzAccount

# 2. Setup Variables
$ResourceGroupName = "TaskPulse-RG"
$Location = "eastus"

# 3. Create Resource Group
Write-Host "Creating Resource Group: $ResourceGroupName" -ForegroundColor Yellow
New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Force

Write-Host "Resource Group created successfully! You are logged in." -ForegroundColor Green
Write-Host "Please tell Antigravity that the script finished." -ForegroundColor Cyan
