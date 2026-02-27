output "app_url" {
  value       = "https://${azurerm_linux_web_app.main.default_hostname}"
  description = "URL of the deployed .NET API"
}

output "app_name" {
  value = azurerm_linux_web_app.main.name
}

output "app_insights_connection_string" {
  value     = azurerm_application_insights.main.connection_string
  sensitive = true
}

output "instrumentation_key" {
  value     = azurerm_application_insights.main.instrumentation_key
  sensitive = true
}
