# RunicLab

**Text-Verschlüsselung, die Spaß macht** – eine kleine Lern-App für klassische Verschlüsselungsverfahren, gebaut mit .NET MAUI für Android und Windows.

[![CI](https://github.com/mschulz11/RunicLab/actions/workflows/ci.yml/badge.svg)](https://github.com/mschulz11/RunicLab/actions/workflows/ci.yml)
[![Release](https://github.com/mschulz11/RunicLab/actions/workflows/release.yml/badge.svg)](https://github.com/mschulz11/RunicLab/actions/workflows/release.yml)
[![Latest Release](https://img.shields.io/github/v/release/mschulz11/RunicLab)](https://github.com/mschulz11/RunicLab/releases/latest)

## Funktionen

- **Cäser-Verfahren** – Text verschlüsseln und entschlüsseln durch Verschieben des Alphabets
- **Polybius-Quadrat** – Buchstaben als Zahlenpaare aus einem 5×5-Gitter, mit optionalem Schlüsselwort und wählbarem Trennzeichen
- **Häufigkeitsanalyse (Al-Kindi)** – zählt Buchstaben, zeigt Balken und Prozentanteile und schätzt daraus die Cäsar-Verschiebung eines Geheimtextes
- **Flexible Einstellungen** – Verschiebung per Eingabefeld, Stepper oder Slider wählen (0–26), Modus (Ver-/Entschlüsseln) per RadioButton umschalten
- **Beispiel-Alphabet** – zeigt live, wie jeder Buchstabe bei der aktuellen Einstellung verschoben wird
- **Responsives Layout** – Einstellungsseite passt sich an Fenster- und Bildschirmgröße an
- **Hinweise zum Verfahren** – Geschichte und Funktionsweise direkt in der App
- **Zwei Plattformen** – Android (APK) und Windows (portable ZIP)

Alle Verfahren sind bewusst keine sichere Verschlüsselung – die App ist zum Lernen und Ausprobieren gedacht.

## Download

Fertige Builds gibt es unter [Releases](https://github.com/mschulz11/RunicLab/releases/latest):

| Datei | Plattform |
| --- | --- |
| `RunicLab-<version>-arm64.apk` | Android (arm64, signiert) |
| `RunicLab-<version>-win-x64.zip` | Windows 10/11 (64-bit, entpacken und `RunicLab.exe` starten) |

## Technik

- .NET 10 (LTS) mit .NET MAUI
- Zielplattformen: `net10.0-android`, `net10.0-windows10.0.19041.0` (iOS/Mac Catalyst sind im Projekt konfiguriert, werden aber nicht ausgeliefert)
- UI in XAML, Shell-Navigation
- Chiffre-Logik ohne MAUI-Abhängigkeiten (`Models/`), damit sie per Unit-Test prüfbar bleibt
- Tests: xUnit
- CI/CD: GitHub Actions (Build + Tests bei jedem Push, signierte Releases über Tags `v*`)

## Entwicklung

Voraussetzungen: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), MAUI-Workloads, Android SDK (API 36) für Android-Builds.

```bash
# Workloads installieren
dotnet workload install maui-android maui-windows

# Solution bzw. Projekte
dotnet build RunicLab.slnx

# Windows starten
dotnet build src/RunicLab/RunicLab.csproj -f net10.0-windows10.0.19041.0 -t:Run

# Android (arm64) als APK bauen
scripts/build-apk.sh 1.2
```

### Tests und Formatierung

```bash
scripts/test.sh                                   # dotnet test
scripts/format.sh                                 # Stil prüfen
scripts/format.sh --fix                           # Stil korrigieren
scripts/build-windows.sh                          # portables Windows-Paket nach dist/
```

## Projektstruktur

```
src/RunicLab/           MAUI-App
  Models/               Chiffre-Logik (CaeserCipher, PolybiusCipher, FrequencyAnalysis) – ohne MAUI-Abhängigkeit
  Views/                Seiten: MainPage, CodeCaeserVerfahren, CodePolybius, CodeAlKindi + Einstellungsseiten
  Resources/            App-Icon, Splash, Bilder, Fonts
  Platforms/            Plattformspezifischer Startcode (Android, Windows, iOS, Mac Catalyst)
tests/RunicLab.Tests/   xUnit-Tests für die Chiffre-Logik
docs/                   Architektur (docs/architektur.md) und Entscheidungen (docs/adr/)
plans/                  Arbeitspläne für größere Umbauten
scripts/                Build-, Test- und Formatier-Helfer
deploy/                 Auslieferung; deploy/installer/ ist Altbestand
.agents/                Checklisten für KI-Agenten
.github/workflows/      ci.yml (Build & Test), release.yml (signierte Releases)
```

Architektur und Konventionen: [`docs/architektur.md`](docs/architektur.md) und [`AGENTS.md`](AGENTS.md).

## Release erstellen

Ein Release wird über einen Tag `v<version>` (z. B. `v1.2`) oder manuell über den Workflow `Release` ausgelöst. Die Pipeline baut und signiert die Android-APK, prüft die Signatur, packt den Windows-Build als ZIP und legt beides als Release-Assets ab.

Signing-Secrets (`ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASSWORD`, `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASSWORD`) liegen als Repository-Secrets vor.

## Roadmap

Weitere geplante Verbesserungen stehen in den [Issues](https://github.com/mschulz11/RunicLab/issues), u. a.:

- [App-Icon überarbeiten](https://github.com/mschulz11/RunicLab/issues/8)

## Lizenz

MIT – siehe [LICENSE.txt](LICENSE.txt).
