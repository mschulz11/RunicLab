# Plan: Enigma-Verfahren

- **Issue:** #31
- **Ziel:** Enigma-I-Seite (3 Walzen, UKW A/B/C, Ring-/Grundstellung, Steckerbrett) mit
  Schritt-für-Schritt-Walzenlauf, geprüft gegen Referenzvektoren aus py-enigma.
- **Nicht-Ziel:** M4/vierte Walze, Schlüsselsuche, Änderungen an der Häufigkeitsanalyse (#32).

## Schritte

1. [ ] `Models/TextNormalisierung.cs` – Umlaut-Umschreibung als gemeinsame Stelle
   (`PolybiusCipher.Transliterate` delegiert darauf, Verhalten unverändert)
2. [ ] `Models/EnigmaCipher.cs` – Walzendaten, Schrittwerk mit Doppelschritt,
   Ver-/Entschlüsselung, Steckerbrett-Validierung, Protokoll, Ausgabeformate
3. [ ] `tests/RunicLab.Tests/EnigmaCipherTests.cs` + `<Compile Include>` in der Test-CSProj
4. [ ] Referenzvektoren gegen py-enigma (Einstellungen im Test dokumentiert)
5. [ ] `Views/EnigmaPreferences.cs` – Persistenz aller Schlüsselparameter
6. [ ] `Views/SettingsEnigma.xaml(.cs)` – Walzen, UKW, Ring, Grundstellung, Stecker, Ausgabe
7. [ ] `Views/CodeEnigma.xaml(.cs)` – Seite über `CipherPageBase`
8. [ ] `Views/EnigmaWalzenlauf.xaml(.cs)` – Protokollseite
9. [ ] `MainPage`, `AppShell` – Eintrag und Route
10. [ ] `Models/CipherVergleich.cs` – Enigma als Kandidat mit aktuellen Einstellungen
11. [ ] `README.md`, `docs/architektur.md`, `plans/README.md` aktualisieren

## Verifikation

- [ ] `dotnet build src/RunicLab/RunicLab.csproj -f net10.0-windows10.0.19041.0`
- [ ] `./scripts/test.sh`
- [ ] `dotnet format RunicLab.slnx --verify-no-changes` (nur eigene Dateien müssen sauber sein)
- [ ] Android-Emulator: Debug-Build (x86_64) installiert, Funktionales per `uiautomator`-Dump
- [ ] Testrezept für die optische Prüfung durch den Nutzer

## Risiken

- Doppelschritt falsch → Test der Folge ADU→ADV→AEW→BFX→BFY, unabhängig von der Verdrahtung
- Ringstellung mit Stellung verwechselt → Vektor mit Ring B/C/D und Grundstellung XYZ
- Protokoll läuft vom Text weg → beide aus derselben Schrittliste
