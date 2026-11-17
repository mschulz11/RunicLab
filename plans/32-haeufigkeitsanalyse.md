# Plan: Häufigkeitsanalyse ohne Werte aus den Einstellungen

- **Issue:** #32
- **Ziel:** Ein Polybius-Geheimtext wird allein aus der Häufigkeit seiner
  Koordinatenpaare entschlüsselt – ohne das eingestellte Schlüsselwort.
- **Nicht-Ziel:** Vergleichsanalyse, Cäsar-Schätzung, Polybius-Einstellungen und
  Ver-/Entschlüsselungsseite bleiben unverändert.

## Schritte

1. [x] `Models/PolybiusHaeufigkeitsanalyse.cs`: Paare zählen, Rangzuordnung gegen
   die erwartete Sprachverteilung (I übernimmt den Anteil von J), Schwellen
   `MindestVorkommen = 3` und `MindestAnteil = 1 %`, Klartext mit „?“ für offene
   Felder.
2. [x] `Views/PolybiusGridBuilder.cs`: `FillHypothese` – Zelle mit Buchstabe (oder
   „?“) über Paar und Anzahl; gemeinsames `Prepare` mit `Fill`.
3. [x] `Views/CodeAlKindi.xaml(.cs)`: Auswertung entscheidet nicht mehr über den
   besten Kandidaten der Vergleichsanalyse, sondern über die Hypothese; Karte
   „Aus der Häufigkeit erschlossenes Gitter“, Hinweiszeile mit belegten Feldern
   und den drei häufigsten Paaren, Erklärtexte angepasst.
4. [x] Tests: `tests/RunicLab.Tests/PolybiusHaeufigkeitsanalyseTests.cs` inkl.
   `<Compile Include>` in der Test-CSProj.
5. [x] Doku: `README.md`, `docs/architektur.md`, dieser Plan,
   `plans/README.md`.

## Verifikation

- [x] `dotnet build src/RunicLab/RunicLab.csproj -f net10.0-windows10.0.19041.0`
- [x] `./scripts/test.sh` (58 Tests grün, davon 13 neue)
- [ ] `dotnet format RunicLab.slnx --verify-no-changes` ohne neue Meldungen
- [ ] Android-Emulator: Debug-Build installiert, Funktionales per `uiautomator`-Dump
- [ ] manuell (Nutzer): Optik des Rasters und Lesbarkeit des „?“-Textes

## Risiken

- Rangzuordnung wirkt überzeugender als sie ist → Erklärtext nennt sie ausdrücklich
  eine Vermutung, offene Felder bleiben sichtbar „?“.
- J/I verschiebt die Ränge → Umrechnung nur in `Buchstabenrang`, Test prüft Rang 3 =
  I für Deutsch.
- Kurze Texte → unter 20 gültigen Paaren keine Hypothese, sondern der bestehende
  Hinweis.
