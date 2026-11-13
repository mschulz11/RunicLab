# Review-Checkliste

Vor dem Öffnen eines Pull Requests abarbeiten:

## Build & Tests

> Die CI läuft erst nach dem Merge auf `master` (Issue #13). Ein Pull Request
> ohne lokalen grünen Build ist ein ungeprüfter Merge – diese Punkte sind Pflicht.

- [ ] `dotnet build src/RunicLab/RunicLab.csproj -f net10.0-windows10.0.19041.0` ohne Warnungen, die neu sind
- [ ] `dotnet test tests/RunicLab.Tests/RunicLab.Tests.csproj` grün
- [ ] `dotnet format RunicLab.slnx --verify-no-changes` sauber

## Code

- [ ] Neue Chiffre-Logik liegt in `src/RunicLab/Models/` und hat Tests
- [ ] Keine `Microsoft.Maui.*`-Typen in `Models/`
- [ ] Keine Copy/Paste-Seite: neue Seite leitet von der gemeinsamen Basis ab
- [ ] Keine Secrets, Keys oder lokalen Pfade im Diff
- [ ] `async void` nur in Event-Handlern

## Doku & Struktur

- [ ] `docs/architektur.md` ist aktuell, wenn sich Ordner/Projekte geändert haben
- [ ] Bei grundlegenden Entscheidungen: neues ADR unter `docs/adr/`
- [ ] `README.md` nennt neue Befehle, wenn sich der Build-Ablauf ändert

## Release-relevant

- [ ] Zielplattformen weiterhin nur Android + Windows
- [ ] `deploy/` unverändert bzw. Änderung begründet (Installer ist Altbestand)
