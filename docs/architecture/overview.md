# Architekturueberblick

Die Anwendung folgt einem GitHub-Pages-first-Ansatz.

## Leitlinien

- statisches Hosting ist der Normalfall
- Fachlogik bleibt von Web-UI und Importwerkzeugen getrennt
- Inhalte werden reproduzierbar importiert statt manuell kopiert
- serverseitige Erweiterungen bleiben optional und separat

## Schichten

- `src/WebSite`: Blazor-WebAssembly-Host und Deployment-spezifische Konfiguration
- `src/Web.UI`: Web-Komponenten, Listen, Layouts und Darstellungsmuster
- `src/Shared.Application`: Use-Case-nahe Services und View-Model-Mapping
- `src/Shared.Domain`: zentrale Regeln, Routen und fachliche Hilfslogik
- `src/Content.Schema`: stabile Content-DTOs fuer Import und Anzeige
- `tools/*`: Build-Time-Importer fuer Quellsysteme