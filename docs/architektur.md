# Architektur

RunicLab ist eine .NET-MAUI-App (net10.0) für Android und Windows. Sie zeigt
klassische Verschlüsselungsverfahren zum Lernen: Cäser, Polybius-Quadrat und
Häufigkeitsanalyse nach Al-Kindi. Es gibt keinen Server, keine Datenbank und
keine Netzwerkschnittstelle – aller Zustand lebt lokal auf dem Gerät.

## Repository-Aufbau

```
src/RunicLab/            einziges App-Projekt (Single-Project-MAUI)
tests/RunicLab.Tests/    xUnit, ohne MAUI-Abhängigkeit
deploy/installer/        Inno-Setup-Skript (Altbestand, nicht in der Pipeline)
docs/                    diese Doku + ADRs
plans/                   Arbeitspläne für größere Umbauten
scripts/                 lokale Build-/Release-Helfer
.agents/                 Checklisten für Agenten
.config/                 dotnet-Tool-Manifest
.github/workflows/       CI (Build + Tests) und Release (APK + ZIP)
```

Die Solution liegt als `RunicLab.slnx` im Root; alle Projektpfade darin sind
repo-relativ, damit weder IDE noch CLI vom Arbeitsverzeichnis abhängen.

## Schichten

| Ebene | Ort | Verantwortung | Darf abhängen von |
| --- | --- | --- | --- |
| Chiffre-Logik | `src/RunicLab/Models/` | reine Algorithmen und Options-Typen | `System.*` |
| Persistenz | `src/RunicLab/Views/*Preferences.cs` | Lesen/Schreiben über `Microsoft.Maui.Storage.Preferences` | Models, MAUI-Essentials |
| UI | `src/RunicLab/Views/*.xaml(.cs)`, `AppShell.xaml` | Darstellung, Events, Navigation | alles darüber |
| Start | `MauiProgram.cs`, `App.xaml.cs`, `Platforms/` | Host-Aufbau, DI-freier Bootstrap, Plattformcode | MAUI |

Die Trennung ist bewusst schmal: Es gibt kein ViewModel-Projekt, weil die Seiten
wenig Zustand haben. Der Zustand wird über `Preferences`-Klassen gekapselt
(`Caeser`-Einstellungen über `AppSettings` + `SettingsCaeser`, `PolybiusPreferences`,
`AlKindiPreferences`), damit die Logik testbar bleibt und die XAML-Dateien dünn sind.

## Verfahren

| Verfahren | Logik | Seite | Einstellungen |
| --- | --- | --- | --- |
| Cäser | `Models/CaeserCipher.cs` | `Views/CodeCaeserVerfahren.xaml` | `Views/SettingsCaeser.xaml` |
| Polybius-Quadrat | `Models/PolybiusCipher.cs` | `Views/CodePolybius.xaml` | `Views/SettingsPolybius.xaml`, `Views/PolybiusGridBuilder.cs` |
| Häufigkeitsanalyse | `Models/FrequencyAnalysis.cs` | `Views/CodeAlKindi.xaml` | `Views/SettingsAlKindi.xaml` |

Jedes Verfahren hat ein „Code“-Fenster (Ein-/Ausgabe) und eine Einstellungsseite.
Neue Verfahren folgen derselben Aufteilung und werden von einer gemeinsamen Basis
abgeleitet (Issue #11) statt kopiert.

## Tests

`tests/RunicLab.Tests` ist ein reines `net10.0`-Projekt ohne MAUI-Workload. Es
bindet die Logikdateien per `<Compile Include="..\..\src\RunicLab\Models\*.cs">`
ein. Folge: Was getestet werden soll, muss UI-frei sein. Sobald ein Verfahren
MAUI-Typen braucht, gehört dieser Teil in die `Views`-Schicht und nicht in die
getestete Klasse.

## Build und Auslieferung

- **CI** (`.github/workflows/ci.yml`): Restore, Tests, Windows-Build und
  unsignierter Android-Build als Nachweis – auf Push und Pull Request gegen `master`.
- **Release** (`.github/workflows/release.yml`): bei Tag `v*` oder manuell;
  baut signierte arm64-APK (Keystore aus Repository-Secrets), prüft die Signatur
  mit `apksigner`, packt den self-contained Windows-Build als ZIP und legt beides
  als Release-Assets ab.
- Ausgeliefert werden ausschließlich diese beiden Artefakte. Installer/MSIX sind
  nicht Teil des Prozesses; `deploy/installer/` bleibt nur als Historie im Repo.

## Randbedingungen

- iOS, Mac Catalyst und Tizen sind im Projekt konfiguriert, werden aber weder
  gebaut noch getestet; sie dürfen nicht kaputtgehen, sind aber nicht Release-relevant.
- Verschlüsselung ist didaktisch, nicht sicher. Die App darf nie als
  Sicherheitswerkzeug beworben werden.
- Kein Netzwerkzugriff: keine Telemetrie, keine Cloud, keine externen APIs.
