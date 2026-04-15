# edu

Edu ist die geplante GitHub-Pages-faehige Study- und Companion-Webanwendung fuer wiederverwendbare Lehrinhalte, Kataloge und spaeter importierte Inhalte aus bestehenden .NET-Projekten wie ISD Companion und Exam Generator.

## Zielbild

- Blazor WebAssembly als statischer Default fuer GitHub Pages
- gemeinsame Fachlogik in getrennten .NET-Bibliotheken
- build-time Importer fuer externe Inhaltsquellen
- spaeter optionaler separater Backend-Dienst fuer nicht statische Features

## Projektstruktur

```text
src/
	WebSite/              GitHub-Pages-faehiger Blazor-WASM-Host
	Web.UI/               wiederverwendbare Web-Komponenten
	Shared.Application/   anwendungsnahe, testbare Logik
	Shared.Domain/        fachliche Kernlogik und Routenregeln
	Content.Schema/       stabile DTOs fuer importierte Inhalte

tools/
	Companion.Import/     Importpfad fuer ISD Companion
	ExamGenerator.Import/ Importpfad fuer Exam Generator

docs/
	architecture/         Architekturentscheidungen und Zielstruktur
	content-pipeline/     Import- und Generierungsfluss
```

## Aktueller Stand

- Greenfield-Solution aufgesetzt
- erste Architekturgrenzen verdrahtet
- GitHub-Pages-tauglicher Host vorbereitet
- erste Seed-Daten und Katalogsicht als Platzhalter angelegt

## Build

```bash
dotnet build edu.sln
```

## Naechste Schritte

1. Companion-Importer auf echte Quellstruktur zuschneiden.
2. Study-Katalog aus generierten Artefakten statt Seed-Daten speisen.
3. GitHub-Pages-Deployment-Workflow und Base-Path finalisieren.