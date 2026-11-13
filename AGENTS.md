# AGENTS.md

Anweisungen für KI-Agenten (und Menschen), die in diesem Repository arbeiten.

**Verbindlich:** [`.architektur.md`](.architektur.md) – Regeln, die bei jeder
Änderung gelten (kurz, imperativ). Ausführliche Beschreibung und Begründung:
[`docs/architektur.md`](docs/architektur.md). Bei Widerspruch gilt
`.architektur.md`.

## Projekt in einem Satz

RunicLab ist eine .NET-MAUI-Lern-App für klassische Verschlüsselungsverfahren
(Cäser, Polybius-Quadrat, Häufigkeitsanalyse nach Al-Kindi) für Android und Windows.

## Struktur

```
src/RunicLab/            MAUI-App (eine Plattform-Projektdatei, alle TFMs)
  Models/                Chiffre-Logik – KEINE MAUI-Abhängigkeiten
  Views/                 XAML-Seiten + Code-Behind + Preferences-Klassen
  Resources/             Icon, Splash, Bilder, Fonts, Styles
  Platforms/             plattformspezifischer Startcode
tests/RunicLab.Tests/    xUnit-Tests; bindet Models/*.cs per <Compile Include> ein
deploy/installer/        Inno-Setup-Skript (Altbestand, nicht Teil der Release-Pipeline)
docs/                    Architektur, Entscheidungen (ADR)
plans/                   Arbeitspläne für größere Umbauten
scripts/                 lokale Build-/Test-/Release-Helfer
.agents/                 Zusatzanweisungen für Agenten (Checklisten, Konventionen)
.config/                 dotnet-Tool-Manifest
```

## Befehle

```bash
dotnet restore RunicLab.slnx
dotnet build src/RunicLab/RunicLab.csproj -f net10.0-windows10.0.19041.0
dotnet test  tests/RunicLab.Tests/RunicLab.Tests.csproj
dotnet format RunicLab.slnx --verify-no-changes   # Stil prüfen
```

Android-APK (arm64) und Windows-ZIP bauen: `scripts/build-apk.sh`, `scripts/build-windows.sh`.

## Regeln

1. **Chiffre-Logik bleibt UI-frei.** Neue Verfahren gehören als reine Klasse nach
   `src/RunicLab/Models/` (static/instance, keine `Microsoft.Maui.*`-Typen) und
   bekommen eine Testdatei in `tests/RunicLab.Tests/`, die per `<Compile Include>`
   eingebunden wird. Nur so laufen die Tests ohne MAUI-Workload.
2. **Neue Verschlüsselungsseiten** erben von `CipherPageBase` und nutzen
   `CipherEingabe` – nicht als Copy/Paste-Duplikat anlegen. Vorlage und
   Pflichtmethoden: `.architektur.md`, Abschnitt 1.
3. **Zielplattformen** sind Android und Windows. iOS/MacCatalyst/Tizen bleiben
   kompilierbar, werden aber nicht ausgeliefert und nicht getestet.
4. **Deliverables** sind ausschließlich die signierte APK (arm64) und das
   Windows-ZIP als Release-Assets. Kein Installer, kein MSIX.
5. **Secrets** niemals committen. Der Keystore liegt außerhalb des Repos; in der
   CI kommen `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASSWORD`,
   `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASSWORD` aus Repository-Secrets.
6. **Sprache:** UI-Texte, Kommentare für Nutzer und Doku auf Deutsch; Identifier
   im Code englisch, außer sie bilden ein deutsches Fachwort ab (`CaeserCipher`,
   `Haeufigkeitsanalyse`-Fallnamen sind historisch gewachsen).
7. **Vor jedem PR** müssen `dotnet build` (Windows-TFM) und `dotnet test` grün sein.
8. **Struktur-Änderungen** (neue Top-Level-Ordner, neue Projekte) brauchen ein
   Update von `docs/architektur.md` und einen Eintrag unter `docs/adr/`.
