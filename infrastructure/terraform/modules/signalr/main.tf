resource "azurerm_signalr_service" "main" {
  name                = "${var.project_name}-${var.environment}-signalr"
  resource_group_name = var.resource_group_name
  location            = var.location

  sku {
    name     = var.sku
    capacity = var.capacity
  }

  cors {
    allowed_origins = [
      "chrome-extension://*",
      "https://www.chess.com",
      "http://localhost:3000",
      "http://localhost:5000"
    ]
  }

  connectivity_logs_enabled = true
  messaging_logs_enabled    = true
  live_trace_enabled        = var.environment == "prod" ? false : true

  service_mode = "Default"

  tags = var.tags
}
