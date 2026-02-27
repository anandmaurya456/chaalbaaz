resource "azurerm_service_plan" "main" {
  name                = "${var.project_name}-${var.environment}-asp"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = var.sku

  tags = var.tags
}

resource "azurerm_linux_web_app" "main" {
  name                = "${var.project_name}-${var.environment}-api"
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    always_on = var.environment == "prod" ? true : false

    application_stack {
      dotnet_version = "8.0"
    }

    cors {
      allowed_origins = [
        "chrome-extension://*",
        "https://www.chess.com"
      ]
    }

    health_check_path = "/health"
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"          = var.environment == "prod" ? "Production" : "Development"
    "StockfishEngine__BaseUrl"        = var.engine_url
    "ConnectionStrings__Redis"        = var.redis_connection_string
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.app_insights_connection_string
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
  }

  logs {
    application_logs {
      file_system_level = "Information"
    }
    http_logs {
      retention_in_days {
        retention_in_days = 7
      }
    }
  }

  tags = var.tags
}

resource "azurerm_application_insights" "main" {
  name                = "${var.project_name}-${var.environment}-ai"
  resource_group_name = var.resource_group_name
  location            = var.location
  application_type    = "web"
  tags                = var.tags
}
