output "connection_string" {
  value     = "${azurerm_redis_cache.main.hostname}:${azurerm_redis_cache.main.ssl_port},password=${azurerm_redis_cache.main.primary_access_key},ssl=True,abortConnect=False"
  sensitive = true
}

output "hostname" {
  value = azurerm_redis_cache.main.hostname
}

output "primary_access_key" {
  value     = azurerm_redis_cache.main.primary_access_key
  sensitive = true
}
