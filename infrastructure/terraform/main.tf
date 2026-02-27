terraform {
  required_version = ">= 1.7.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.100"
    }
  }

  backend "azurerm" {
    resource_group_name  = "chaalbaaz-tfstate-rg"
    storage_account_name = "chaalbaaztfstate"
    container_name       = "tfstate"
    key                  = "chaalbaaz.terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

# ---- Resource Group ----
resource "azurerm_resource_group" "main" {
  name     = "${var.project_name}-${var.environment}-rg"
  location = var.location
  tags     = var.tags
}

# ---- App Service Plan (.NET API) ----
module "app_service" {
  source              = "./modules/app-service"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  sku                 = var.backend_sku
  tags                = var.tags
}

# ---- Azure SignalR Service ----
module "signalr" {
  source              = "./modules/signalr"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  tags                = var.tags
}

# ---- Azure Redis Cache ----
module "redis" {
  source              = "./modules/redis"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  sku                 = var.redis_sku
  family              = var.redis_family
  capacity            = var.redis_capacity
  tags                = var.tags
}

# ---- Azure Container Instance (Python Engine) ----
module "engine_container" {
  source              = "./modules/container"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  cpu                 = var.container_cpu
  memory              = var.container_memory
  tags                = var.tags
}
