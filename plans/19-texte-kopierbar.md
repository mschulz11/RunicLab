# Plan: Texte auf den Seiten kopierbar (#19)

- **Issue:** #19
- **Ziel:** Alle sichtbaren Texte der App lassen sich auf Android markieren und
  kopieren – Erklärtexte, Statuszeilen, Gitterbeschriftungen, Ergebnisse.
- **Nicht-Ziel:** Layout-/Designänderungen, Teilen-Funktion, Änderungen an den
  Chiffre-Algorithmen, eigene Auswahl-Logik pro Seite.

## Ausgangslage (auf dem Emulator geprüft)

Android 15-Emulator (`pixel_7_-_api_35`, x86_64) eingerichtet (Issue #20). Der
Stand von `master` lief dort durch:

- Eingabe klappt, Cäser verschlüsselt (`Buchstaben` → `Exfkvwdehq`).
- Der `KopierenButton` aus #10 ist sichtbar und legt den Text wirklich in die
  Zwischenablage (per Einfügen im Eingabefeld geprüft: „RunicLab pasted from
  your clipboard“).
- **Aber:** Erklärtexte („Das Cäsar-Verfahren …“, „Funktionsweise …“) sind
  MAUI-`Label` ohne Plattform-Textauswahl. Auf Android gibt es damit keinen Weg,
  sie zu markieren – genau das, was #19 beschreibt.
- Damit ist der offene Punkt aus `plans/10-kopieren.md` („Android nicht auf einem
  Gerät getestet“) erledigt: der Button-Pfad funktioniert dort.

## Umsetzung

1. `Views/TextAuswahl.cs`: Mapping auf `LabelHandler`, das die
   Plattform-Textauswahl anhängt.

   | Plattform | Aufruf |
   | --- | --- |
   | Android | `TextView.SetTextIsSelectable(true)` |
   | Windows | `TextBlock.IsTextSelectionEnabled = true` |
   | iOS/Mac Catalyst/Tizen | nichts (bleiben nur kompilierbar) |

   Einmalig in `MauiProgram.CreateMauiApp()` aktiviert. Warum zentral und nicht
   pro Seite: Die Markierbarkeit ist eine Eigenschaft der Plattform-Ansicht, kein
   Inhalt. Ein Mapping erfasst alle `Label`-Vorkommen und jede künftige Seite,
   ohne dass Regeln in `CipherPageBase` oder XAML dupliziert werden.

2. `KopierenButton` ersatzlos entfernt (Entscheidung: native Auswahl genügt).
   Damit fallen `Views/KopierenButton.cs`, der Button in `CipherEingabe`, der
   Button in `CodeAlKindi` und die zugehörige Sichtbarkeitslogik weg; `Label` +
   Textauswahl ist der einzige Kopierweg.

## Verifikation (Android-Emulator, echte App)

- [x] Langdruck auf Erklärtext → Griffe + Kontextmenü (Translate/Copy/Share/Select all)
- [x] „Copy“ im Kontextmenü → markiertes Wort per Einfügen im Eingabefeld
      nachgewiesen („RunicLab pasted from your clipboard“, Feld zeigt `Buchstaben`)
- [x] Ergebnis-Label (`Exfkvwdehq`) markierbar, inklusive „Select all“
- [ ] Nach dem Entfernen des Buttons: kein „Kopieren“-Button mehr sichtbar,
      Auswahl unverändert

## Grenzen

Ohne Plattform-Unterstützung (iOS/Mac Catalyst) bleibt Text nicht markierbar;
das ist laut `.architektur.md`, Abschnitt 4 akzeptiert, weil diese Plattformen
nicht ausgeliefert werden.
