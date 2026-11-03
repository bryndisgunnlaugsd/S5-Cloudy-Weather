# Cloudy Weather

The solution contains two projects with the same functionality:

| Project | Hosting | Endpoints |
|---|---|---|
| `CloudyWeather.API` | Azure App Service | `GET /ServiceCheck`, `GET /api/Weather` |
| `CloudyWeather.Functions` | Azure Function (HTTP trigger, public) | `GET /ServiceCheck`, `GET /api/Weather` |

In both projects `SYS_ENVIRONMENT` is set to `PRODUCTION` in Azure, so the endpoints return:
`The current environment is: PRODUCTION` and `The weather is snowy`

## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)

All commands are run from the root of the solution cloudy-weather in a bash/zsh terminal. They assume nothing has been set up on the Azure account before.


## 0. Common setup (run once)

```bash
# Log in to Azure
az login

# Register the resource providers we need (required on a brand new subscription)
az provider register --namespace Microsoft.Web --wait
az provider register --namespace Microsoft.Storage --wait
az provider register --namespace Microsoft.Insights --wait

# Variables (app names must be globally unique, so a random number is added)
RG=cloudy-weather-rg
LOCATION=northeurope
SUFFIX=$RANDOM
PLAN=cloudy-weather-plan
WEB_APP=cloudy-weather-api-$SUFFIX
FUNC_APP=cloudy-weather-func-$SUFFIX
STORAGE=cloudyweather$SUFFIX

# Resource group for everything
az group create --name $RG --location $LOCATION
```


## 1. Deploy the template to Azure App Service

```bash
# App Service plan (Linux, free tier)
az appservice plan create \
  --name $PLAN \
  --resource-group $RG \
  --location $LOCATION \
  --sku F1 \
  --is-linux

# Web app running .NET 10
az webapp create \
  --name $WEB_APP \
  --resource-group $RG \
  --plan $PLAN \
  --runtime "DOTNETCORE:10.0"

# Set the environment variable to PRODUCTION
az webapp config appsettings set \
  --name $WEB_APP \
  --resource-group $RG \
  --settings SYS_ENVIRONMENT=PRODUCTION

# Build and package the API
dotnet publish CloudyWeather.API/CloudyWeather.API.csproj -c Release -o publish/api
(cd publish/api && zip -r ../api.zip .)

# Deploy
az webapp deploy \
  --name $WEB_APP \
  --resource-group $RG \
  --src-path publish/api.zip \
  --type zip

# Test
curl https://$WEB_APP.azurewebsites.net/ServiceCheck
curl https://$WEB_APP.azurewebsites.net/api/Weather
```


## 2. Deploy the Azure Function

```bash
# Storage account (required by Azure Functions)
az storage account create \
  --name $STORAGE \
  --resource-group $RG \
  --location $LOCATION \
  --sku Standard_LRS \
  --allow-blob-public-access false

# Function app on the Flex Consumption plan (serverless) running .NET 10 isolated
az functionapp create \
  --name $FUNC_APP \
  --resource-group $RG \
  --storage-account $STORAGE \
  --flexconsumption-location $LOCATION \
  --runtime dotnet-isolated \
  --runtime-version 10.0

# Set the environment variable to PRODUCTION
az functionapp config appsettings set \
  --name $FUNC_APP \
  --resource-group $RG \
  --settings SYS_ENVIRONMENT=PRODUCTION

# Build and package the function
dotnet publish CloudyWeather.Functions/CloudyWeather.Functions.csproj -c Release -o publish/func
(cd publish/func && zip -r ../func.zip .)

# Deploy
az functionapp deployment source config-zip \
  --name $FUNC_APP \
  --resource-group $RG \
  --src publish/func.zip

# Test (public endpoints, no function key needed)
curl https://$(az functionapp show --name $FUNC_APP --resource-group $RG --query defaultHostName -o tsv)/ServiceCheck
curl https://$(az functionapp show --name $FUNC_APP --resource-group $RG --query defaultHostName -o tsv)/api/Weather
```


## Running locally

```bash
# API (reads SYS_ENVIRONMENT=LOCAL from CloudyWeather.API/.env.local)
cd CloudyWeather.API && dotnet run

# Function (reads SYS_ENVIRONMENT=LOCAL from local.settings.json,
# requires Azure Functions Core Tools: https://learn.microsoft.com/azure/azure-functions/functions-run-local)
cd CloudyWeather.Functions && func start
```

## Clean up

```bash
az group delete --name $RG --yes --no-wait
```