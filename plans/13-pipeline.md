# Plan: Pipeline an Merge auf master koppeln (#13)

- **Issue:** #13
- **Ziel:** CI startet erst nach einem Merge auf `master`; der Head-Branch eines
  gemergten Pull Requests ist danach weg.
- **Nicht-Ziel:** Release-Workflow ändern, neue Check-Stufen (Linting, Coverage)
  hinzufügen, Branch-Protection einrichten.

## Schritte

1. [x] Branch `deploy/13-pipeline-bearbeiten`
2. [x] `ci.yml`: `pull_request`-Trigger entfernt, nur noch `push: [master]` + `workflow_dispatch`
3. [x] `.github/workflows/cleanup-merged-branch.yml`: löscht den Head-Branch bei
       `pull_request: closed` mit `merged == true`; überspringt Default-Branch und Forks
4. [x] Repository-Einstellung „Automatically delete head branches"
       (`delete_branch_on_merge = true`) – greift ohne Workflow-Lauf
5. [x] Doku nachgezogen: `docs/architektur.md`, `README.md`, `.agents/review-checkliste.md`

## Verifikation

- [ ] Nach dem Merge dieses PRs: CI-Lauf auf `master` startet, Branch ist weg
- [ ] `gh api repos/mschulz11/RunicLab --jq .delete_branch_on_merge` → `true`
- [ ] `actionlint`/YAML-Validierung der beiden Workflows (manuell geprüft)

## Konsequenzen

- Pull Requests haben keine automatischen Checks mehr. Vor dem Merge muss lokal
  grün gebaut und getestet werden (siehe Checkliste in `.agents/`).
- Ein Push auf einen Feature-Branch startet keine Pipeline mehr – nur der Merge
  auf `master` tut das.
- Rote Builds fallen erst auf `master` auf. Dafür bleibt `master` garantiert der
  Stand, der wirklich gebaut wurde.
