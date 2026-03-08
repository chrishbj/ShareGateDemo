output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "container_app_url" {
  value = "https://${azurerm_container_app.app.latest_revision_fqdn}"
}

output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "acr_name" {
  value = azurerm_container_registry.acr.name
}