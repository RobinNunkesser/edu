# Exercise Rendering POC

## Zielbild

Der POC trennt drei Ebenen:

1. Aufgabensemantik
   Lokale Vorstufe fuer ein kuenftiges Paket wie `Italbytz.Exercises.Abstractions`.
   Typen im aktuellen Slice: `ExerciseDocumentViewModel`, `ExerciseSectionViewModel`, `ExerciseBlockViewModel`.

2. Aufgabenerzeugung
   Lokale Vorstufe fuer ein kuenftiges Paket wie `Italbytz.Exercises.Generation`.
   Im aktuellen Slice erzeugt `ExerciseDocumentFactory` ein neutrales Dokument fuer Binary Addition.

3. Rendering
   Lokale HTML-Ansicht in edu als Vorstufe fuer `Italbytz.Exercises.Rendering.Html`.
   Ein kuenftiger LaTeX-Adapter kann dieselbe Dokumentstruktur fuer den Exam Generator rendern.

## Paketgrenzen

- `Italbytz.Exercises.Abstractions`
  Neutrale Dokument- und Blocktypen fuer Einzelaufgaben, Arbeitsblaetter und Musterloesungen.

- `Italbytz.Exercises.Generation`
  Fabriken bzw. Projektoren, die aus bestehender Fachlogik und Parametern konkrete `ExerciseDocument`-Instanzen erzeugen.

- `Italbytz.Exercises.Rendering.Html`
  HTML- und Print-CSS-Renderer fuer edu und spaetere Companion-Webvorschauen.

- `Italbytz.Exercises.Rendering.LaTeX`
  Adapter fuer die bestehende Exam-Generator-Pipeline.

## Warum kein LaTeX als gemeinsames Austauschformat

Geteilt werden sollten semantische Bausteine wie Titel, Hinweise, Varianten, Antwortfelder und Loesungen.
Nicht geteilt werden sollten rohe LaTeX-Strings, Makros und Formularfelder, weil dadurch neue Clients wieder an das Zielformat gekoppelt wuerden.

## Aktueller Slice

- edu liest importierte Exercise-JSON-Dateien unter `src/WebSite/wwwroot/content/study` als Primaerquelle.
- Ein Fallback ueber Laufzeit-Erzeugung bleibt aktiv, falls eine Importdatei fehlt.
- Aktuell abgebildete Task-Typen: Binary Addition, Binary to Decimal, Decimal to Binary, Twos Complement.
- Der Importer erzeugt pro Aufgabe und Sprache sowohl JSON als auch zwei LaTeX-Dateien: Arbeitsblatt und Loesung.
- Ein Regenerierungsskript und eine VS-Code-Task halten die Importartefakte reproduzierbar.

## Naechste Schritte

1. Weitere Exam-Generator-Tasks auf dieselbe semantische Dokumentstruktur abbilden.
2. Den LaTeX-Renderer noch enger an bestehende HSHL-Makros und Layoutbausteine anbinden.
3. Companion-Seiten schrittweise auf dieselbe semantische Aufgabenerzeugung umstellen.