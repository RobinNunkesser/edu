# Content Pipeline

Der geplante Standardfluss ist:

```text
Quellprojekt
  -> Importer unter tools/*
  -> generierte Artefakte / Content-Schema
  -> Shared.Application
  -> Web.UI / WebSite
  -> GitHub Pages
```

## Erste Ausbaustufen

1. ISD-Companion-Metadaten lesen und in stabile DTOs ueberfuehren.
2. Katalogseiten aus generierten Dokumenten rendern.
3. Detailseiten schrittweise aus importierten Inhalten ableiten.