# Plan: Base-Form für Verschlüsselungsseiten (#11)

- **Issue:** #11
- **Ziel:** Neue Verfahrensseiten entstehen durch Ableiten statt Copy/Paste;
  Eingabe, Button, Ausgabe und Einstellungen-Zugang sind einmal implementiert.
- **Nicht-Ziel:** ViewModels/MVVM einführen, Copy-Button in der Ausgabe
  (das ist #10), Änderungen an den Chiffre-Algorithmen.

## Ist-Zustand vorher

`CodeCaeserVerfahren`, `CodePolybius` und `CodeAlKindi` duplizierten je
ToolbarItem, Eingabe-`Border`, Modus-`Label`, Aktions-`Button`,
Ergebnis-`Border` und den `OnSettingsClicked`-Handler – insgesamt ~60 Zeilen
identisches XAML plus ~12 Zeilen identisches Code-Behind pro Seite.

## Schritte

1. [x] Branch `arc/11-base-form-fuer-verschluesselungen`
2. [x] `Views/CipherEingabe.xaml(.cs)`: gemeinsamer Block (Editor, Statuszeile,
       Button, Ergebnisrahmen) mit `AktionText`, `ModusText`, `EingabeText`,
       `AusgabeText`, `ErgebnisRahmenSichtbar`, Event `AktionAngefordert`
3. [x] `Views/CipherPageBase.cs`: abstrakte `ContentPage` mit ToolbarItem,
       `EingabeVerwenden(...)`, `OnAppearing` → `EinstellungenAnwenden()`,
       abstrakt `AktionAusfuehren()` und `EinstellungsSeiteErzeugen()`
4. [x] Die drei Seiten auf die Basis umgestellt; verfahrensspezifisches XAML
       (Erklärtexte, Polybius-Gitter, Al-Kindi-Karten) bleibt in der Seite
5. [x] `.architektur.md` im Repo-Root als verbindliche Anweisung angelegt und in
       `AGENTS.md`, `docs/architektur.md` und `.agents/neues-verfahren.md` verlinkt

## Verifikation

- [x] Windows-Build: 0 Fehler, 0 Warnungen
- [x] `./scripts/test.sh`: 35/35 grün (Models unverändert)
- [x] Laufzeittest der App (Windows):
      - Cäser: `HALLO WELT` → `KDOOR ZHOW`, Modus auf Entschlüsseln → `EXIIL TBIQ`
      - Einstellungen öffnen sich über das geerbte ToolbarItem; nach Rückkehr
        aktualisieren sich Button-Beschriftung und Statuszeile (OnAppearing)
      - Polybius: Gitter gefüllt, `HALLO` → `23 11 31 31 34`
      - Al-Kindi: keine Statuszeile, keine leere Ergebnisbox, `Analysieren`
        zählt 27 Buchstaben und füllt Balken + Schätzung

## Ergebnis

- Rund 60 Zeilen XAML und 36 Zeilen Code-Behind, die vorher dreimal existierten,
  liegen jetzt einmal in `CipherEingabe`/`CipherPageBase`.
- Eine neue Seite besteht aus Kopftexten in XAML + drei Methoden im Code-Behind.

## Risiken

- XAML-Seite mit eigener Basisklasse: Wurzelelement muss `views:CipherPageBase`
  sein, sonst greift `EingabeVerwenden` nicht → Build bricht dann nicht ab, aber
  `Eingabe` wirft zur Laufzeit. Gegenmaßnahme: `.architektur.md` Abschnitt 1 als
  Vorlage verwenden.
