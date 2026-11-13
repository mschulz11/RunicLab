# Plan: Projektstruktur umbauen (#12)

- **Issue:** #12
- **Ziel:** App-Code liegt unter `src/`, Nicht-Code in `docs/`, `plans/`,
  `scripts/`, `deploy/`; die Solution ist `RunicLab.slnx`; CI baut weiter grün.
- **Nicht-Ziel:** neue Projekte (Core-Bibliothek, ViewModels), Base-Form für
  Verschlüsselungsseiten (#11), Pipeline-Änderungen (#13).

## Schritte

1. [x] Branch `refactor/12-projektstruktur`
2. [x] `git mv` des App-Codes nach `src/RunicLab/` (App, Views, Models, Resources,
       Platforms, Properties, `RunicLab.csproj`)
3. [x] `installer/` → `deploy/installer/`
4. [x] `<Compile Remove="tests\**">` aus dem App-`.csproj` entfernen
5. [x] Testprojekt: `<Compile Include>`-Pfade auf `..\..\src\RunicLab\Models\` zeigen
6. [x] `RunicLab.sln` → `RunicLab.slnx`
7. [x] Neue Root-Dateien: `.editorconfig`, `nuget.config`, `.mcp.json`, `AGENTS.md`
8. [x] Neue Ordner mit Inhalt: `docs/` (+`docs/adr/`), `plans/`, `scripts/`,
       `.agents/`, `.config/`, `.vscode/`
9. [x] CI/Release: Projektpfade und `cache-dependency-path` angepasst
10. [x] `README.md`: Befehle und Strukturabschnitt aktualisiert

## Verifikation

- [x] `dotnet build src/RunicLab/RunicLab.csproj -f net10.0-windows10.0.19041.0`
- [x] `dotnet test tests/RunicLab.Tests/RunicLab.Tests.csproj`
- [x] `dotnet sln RunicLab.slnx list` zeigt beide Projekte
- [ ] Android-Build (`scripts/build-apk.sh`) – läuft in der CI

## Risiken

- Android-Build bricht durch Pfadannahmen → CI-Lauf auf dem PR als Nachweis.
- Laufende Branches mit alten Pfaden → Merge-Konflikte; nur dieser Branch ist offen.
- `bin/`/`obj/` aus dem Root bleiben als Leiche zurück → vor dem Commit löschen.
