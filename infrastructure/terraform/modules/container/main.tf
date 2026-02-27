resource "azurerm_container_registry" "main" {
  name                = "${var.project_name}${var.environment}acr"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "Basic"
  admin_enabled       = true
  tags                = var.tags
}

resource "azurerm_container_group" "engine" {
  name                = "${var.project_name}-${var.environment}-engine"
  resource_group_name = var.resource_group_name
  location            = var.location
  ip_address_type     = "Private"
  os_type             = "Linux"

  image_registry_credential {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password = azurerm_container_registry.main.admin_password
  }

  container {
    name   = "chaalbaaz-engine"
    image  = "${azurerm_container_registry.main.login_server}/chaalbaaz-engine:latest"
    cpu    = var.cpu
    memory = var.memory

    ports {
      port     = 8001
      protocol = "TCP"
    }

    environment_variables = {
      "STOCKFISH_DEPTH"   = tostring(var.stockfish_depth)
      "STOCKFISH_THREADS" = tostring(var.stockfish_threads)
      "ENVIRONMENT"       = var.environment
    }

    secure_environment_variables = {
      "REDIS_URL" = var.redis_url
    }

    liveness_probe {
      http_get {
        path   = "/health"
        port   = 8001
        scheme = "Http"
      }
      initial_delay_seconds = 15
      period_seconds        = 20
    }
  }

  tags = var.tags
}
