# ADR 0001 – Projektstruktur nach src/tests und Solution im slnx-Format

- **Status:** angenommen
- **Betrifft:** Issue #12

## Kontext

Alles lag flach im Repository-Root: App-Quellcode, Ressourcen, Solution, README
und das Testprojekt. Das App-`.csproj` musste das Testverzeichnis explizit
ausschließen (`<Compile Remove="tests\**">`), damit die Tests nicht in die App
kompiliert wurden. Es gab keinen Ort für Architektur-Doku, Pläne, Skripte oder
Agenten-Anweisungen.

## Entscheidung

1. Der App-Code wandert nach `src/RunicLab/`, das Testprojekt bleibt in `tests/`.
2. Die Solution wird von `RunicLab.sln` auf `RunicLab.slnx` umgestellt und enthält
   die Ordner `src/` und `tests/`.
3. Neue Top-Level-Ordner: `docs/`, `plans/`, `scripts/`, `deploy/`, `.agents/`,
   `.config/`, `.vscode/`. `deploy/` nimmt das bisherige `installer/` auf.
4. Neue Root-Dateien: `AGENTS.md` (Agenten-Regeln), `.editorconfig` (Stil),
   `nuget.config` (nur nuget.org, Source-Mapping), `.mcp.json` (MCP-Server).
5. Das App-Projekt bleibt **ein** Projekt. Die Chiffre-Logik wird nicht in eine
   eigene Klassenbibliothek ausgelagert; die Tests binden die Dateien weiterhin
   per `<Compile Include>` ein.

## Konsequenzen

- Positiv: klare Trennung von Code und Nicht-Code, keine Sonderregeln im
  `.csproj` gegen das Testverzeichnis, ein Ort für Doku und Pläne, `.slnx` ist
  merge-freundlicher als `.sln` und wird von `dotnet`/VS 17.14+ gelesen.
- Negativ: tiefere Pfade in allen Kommandozeilenbeispielen (CI, README, Skripte).
- Offen: Sobald die Chiffre-Logik wächst oder ein zweiter Consumer dazukommt,
  lohnt `src/RunicLab.Core/` als echte Bibliothek statt `<Compile Include>`-Links.
  Das wäre ein eigener ADR.

## Alternativen

- **Alles flach lassen:** billig, aber die Sonderbehandlung im `.csproj` und das
  Fehlen von Doku-/Planungsorten bleiben.
- **Sofort Core-Bibliothek + ViewModels:** sauberer, aber ein Umbau, der die
  PR-Größe verdoppelt und für die aktuelle App-Größe keinen Nutzen bringt.
