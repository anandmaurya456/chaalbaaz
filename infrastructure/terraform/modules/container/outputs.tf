output "engine_url" {
  value       = "http://${azurerm_container_group.engine.ip_address}:8001"
  description = "Internal URL of the Stockfish engine container"
}

output "registry_login_server" {
  value = azurerm_container_registry.main.login_server
}

output "registry_admin_username" {
  value     = azurerm_container_registry.main.admin_username
  sensitive = true
}

output "registry_admin_password" {
  value     = azurerm_container_registry.main.admin_password
  sensitive = true
}
