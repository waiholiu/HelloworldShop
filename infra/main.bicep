targetScope = 'subscription'

@minLength(1)
@maxLength(20)
param environmentName string

param location string
param principalId string
param principalName string

@allowed([
  'User'
  'Group'
  'Application'
])
param principalType string = 'User'

param tags object = {}

var resourceGroupName = 'rg-${environmentName}'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: union(tags, {
    'azd-env-name': environmentName
  })
}

module resources 'resources.bicep' = {
  name: 'resources-${environmentName}'
  scope: resourceGroup
  params: {
    environmentName: environmentName
    location: location
    principalId: principalId
    principalName: principalName
    principalType: principalType
    tags: tags
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = resourceGroup.name
output SERVICE_WEB_NAME string = resources.outputs.webAppName
output SQL_SERVER string = resources.outputs.sqlServerName
output SQL_DATABASE string = resources.outputs.sqlDatabaseName
output AZURE_KEY_VAULT_NAME string = resources.outputs.keyVaultName
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.applicationInsightsConnectionString
output WEB_ENDPOINT string = resources.outputs.webEndpoint
