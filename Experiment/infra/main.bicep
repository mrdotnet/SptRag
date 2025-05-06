param location string = resourceGroup().location
param sqlAdmin string
@secure()
param sqlPassword string
param openAiName string = 'sptrag-openai'
param postgresName string = 'sptrag-db'
param storageName string = 'sptragstore'

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2022-01-20-preview' = {
  name: postgresName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: sqlAdmin
    administratorLoginPassword: sqlPassword
    version: '15'
    storage: {
      storageSizeGB: 32
    }
    network: {
    }
  }
}

resource openai 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAiName
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: openAiName
    apiProperties: {
      defaultSku: 'S0'
    }
  }
}

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: 'lightrag-api'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: resourceId('Microsoft.Web/serverfarms', 'lightrag-plan')
    siteConfig: {
      appSettings: [
        { name: 'OPENAI_ENDPOINT', value: openai.properties.endpoint }
        { name: 'AZURE_STORAGE_ACCOUNT', value: storage.name }
        { name: 'POSTGRES_HOST', value: '${postgres.name}.postgres.database.azure.com' }
        // Additional secrets should be stored in Key Vault or AZD .env
      ]
    }
  }
}
