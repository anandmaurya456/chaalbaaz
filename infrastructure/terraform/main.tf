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

# ---- Redis (provisioned first — engine depends on it) ----
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

# ---- Container Instance — Python Stockfish Engine ----
module "engine_container" {
  source              = "./modules/container"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  cpu                 = var.container_cpu
  memory              = var.container_memory
  stockfish_depth     = 20
  stockfish_threads   = 2
  redis_url           = module.redis.connection_string
  tags                = var.tags
}

# ---- SignalR ----
module "signalr" {
  source              = "./modules/signalr"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  sku                 = var.environment == "prod" ? "Standard_S1" : "Free_F1"
  tags                = var.tags
}

# ---- App Service — .NET 8 API ----
module "app_service" {
  source                         = "./modules/app-service"
  resource_group_name            = azurerm_resource_group.main.name
  location                       = var.location
  project_name                   = var.project_name
  environment                    = var.environment
  sku                            = var.backend_sku
  engine_url                     = module.engine_container.engine_url
  redis_connection_string        = module.redis.connection_string
  app_insights_connection_string = ""
  tags                           = var.tags
}

# ---- Outputs ----
output "api_url" {
  value       = module.app_service.app_url
  description = "Chaalbaaz .NET API URL — set this in the Chrome extension popup"
}

output "signalr_hostname" {
  value = module.signalr.hostname
}

output "redis_hostname" {
  value = module.redis.hostname
}
