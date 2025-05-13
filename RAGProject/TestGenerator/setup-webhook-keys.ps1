param(
    [string]$VaultName,
    [string]$TenantId = "test-tenant-123"
)

$keyName = "webhook-decryption-key-$TenantId"

# Create RSA key
az keyvault key create `
  --vault-name $VaultName `
  --name $keyName `
  --protection software `
  --kty RSA `
  --size 2048 `
  --query key.kid

# Export the public key
$key = az keyvault key download `
  --vault-name $VaultName `
  --name $keyName `
  --encoding PEM `
  --file "./public-${TenantId}.pem" `
  --query key.kid

# Store the public key as a secret (for signature verification simulation)
$publicPem = Get-Content "./public-${TenantId}.pem" -Raw
az keyvault secret set `
  --vault-name $VaultName `
  --name "$keyName-public" `
  --value $publicPem
