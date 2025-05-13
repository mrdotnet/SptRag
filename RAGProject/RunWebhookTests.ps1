param (
    [string]$Action = "test"
)

$buildDir = "bin/Debug/net8.0"
$keyVaultUri = "https://yoursystemvault.vault.azure.net"
$keyName = "webhook-signing-key"

$env:KEYVAULT_URI = $keyVaultUri
$env:KEY_NAME = $keyName

switch ($Action) {
    "test" {
        Write-Host "Running webhook test simulation..."
        dotnet run --project .\WebhookTestHarness\WebhookTestHarness.csproj
    }
    "encrypt" {
        Write-Host "Encrypting webhook payload..."
        dotnet run --project .\WebhookCryptoTools\EncryptWebhookPayload.csproj
    }
    "decrypt" {
        Write-Host "Decrypting webhook payload..."
        dotnet run --project .\WebhookCryptoTools\DecryptWebhookPayload.csproj
    }
    default {
        Write-Host "Invalid action. Use 'test', 'encrypt', or 'decrypt'."
    }
}
