---
name: runiclab-workflow
description: "Use when working on RunicLab: branch, emulator verification, PR and release."
version: 1.1.0
author: Hermes Agent
license: MIT
platforms: [windows]
metadata:
  hermes:
    tags: [maui, dotnet, android, emulator, github, release]
    related_skills: []
---

# RunicLab: Arbeitsablauf

## Overview

RunicLab ist eine .NET-MAUI-Lern-App für klassische Chiffren, ausgeliefert für
Android und Windows. Dieses Dokument hält den Ablauf fest, mit dem Änderungen
gebaut, geprüft und veröffentlicht werden: Branch → Build/Tests → Prüfung auf dem
Emulator → PR → Squash-Merge → Tag für den Release.

Die verbindlichen Coderegeln stehen in `.architektur.md`; hier steht das
Vorgehen drumherum.

## When to Use

- Eine Funktion oder ein Fehler in RunicLab soll umgesetzt, geprüft oder released werden.
- Ein PR, ein Release-Tag oder die Emulator-Umgebung ist betroffen.
- Nicht dafür: reine Chiffre-Mathematik ohne Bezug zum Ablauf (dafür
  `.architektur.md` und `.agents/neues-verfahren.md`).

## Repo-Regeln (Kurzfassung)

- Chiffre-Logik nach `src/RunicLab/Models/` **ohne** `Microsoft.Maui.*`, plus
  Testdatei in `tests/RunicLab.Tests/`, eingebunden per `<Compile Include>` in der
  Test-CSProj – sonst läuft der Test nicht mit.
- Seiten erben von `CipherPageBase` und nutzen `CipherEingabe`; keine Kopien.
- Vor jedem PR: Windows-Build, `./scripts/test.sh`, `dotnet format --verify-no-changes`.
  Format meldet Altlasten (`Platforms/*` BOM, `SettingsCaeser.xaml.cs`
  using-Reihenfolge); nur eigene Dateien müssen sauber sein.
- Größere Arbeiten: `plans/<issue>-<titel>.md` + Zeile in `plans/README.md`.
- Offene Aufgaben sind GitHub-Issues. UI-Texte und Doku deutsch, Identifier englisch.

## Prüfung auf dem Emulator

Aufteilung, die Tokens spart und trotzdem belastbar prüft:

| Wer | Was |
| --- | --- |
| Agent | Emulator starten, Debug-Build installieren, App starten, zur Seite navigieren, Testrezept schreiben |
| Agent | Funktionales am Text prüfen: `uiautomator dump` liefert exakte Beschriftungen und `bounds` |
| Mensch | Optik und Bedienung prüfen (Layout, Icon, Lesbarkeit) und zurückmelden |

**Keine Screenshot-Analyse.** Ein Bild kostet rund tausend Tokens und liefert nur
„sieht aus wie“; ein Text-Dump kostet einen Bruchteil und liefert den
tatsächlichen Wert. Screenshots nur, wenn ausdrücklich gewünscht.

```bash
export ANDROID_HOME="C:\Program Files (x86)\Android\android-sdk"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export JAVA_HOME="C:\Program Files\Microsoft\jdk-17.0.21.21-hotspot"

# Emulator starten (aus PowerShell per Start-Process, sonst endet er mit dem Aufruf)
"C:\Program Files (x86)\Android\android-sdk\emulator\emulator.exe" \
  -avd pixel_7_-_api_35 -no-boot-anim
adb wait-for-device
adb shell getprop sys.boot_completed        # bis "1"

# Debug-Build: der Emulator ist x86_64, die Release-APK ist arm64
dotnet build src/RunicLab/RunicLab.csproj -f net10.0-android -c Debug \
  -p:RuntimeIdentifier=android-x64 -t:Install

# Starten (Activity-Name nicht raten)
adb shell cmd package resolve-activity --brief com.companyname.runiclab
adb shell monkey -p com.companyname.runiclab -c android.intent.category.LAUNCHER 1
```

Elemente finden und bedienen:

```bash
adb shell uiautomator dump /sdcard/ui.xml
adb shell cat /sdcard/ui.xml | sed 's/></>\n</g' \
  | grep -E "EditText|Button" \
  | sed -E 's/.*text="([^"]*)".*class="([^"]*)".*bounds="([^"]*)".*/\1 | \2 | \3/'
```

- `bounds="[x1,y1][x2,y2]"` → Mitte berechnen, `input tap <x> <y>`.
  Blinde Koordinaten treffen daneben, sobald Tastatur oder Scrollposition anders sind.
- Text eingeben: `adb shell input text "Wort%sWort"` (`%s` = Leerzeichen).
- Feld leeren: Fokus setzen, `input keyevent 123`, dann `keyevent 67`.
- Lange Seiten zuerst scrollen (`input swipe`), sonst landet der Tipp im Fließtext.
- Tag/Nacht prüfen: `adb shell cmd uimode night yes|no`.

## PR und Release

```bash
git checkout -b feature/<issue>-<kurztitel>
# ... implementieren, Build + ./scripts/test.sh grün ...
git push -u origin feature/<issue>-<kurztitel>
gh pr create --head <branch> --base master --title "… (#<issue>)" --body-file <datei>
git fetch origin -q            # sonst scheitert --force-with-lease mit "stale info"
gh pr merge <nr> --squash --delete-branch
```

- PR-Titel und -Body deutsch, `Closes #<issue>` nicht vergessen.
- Body **immer** über `--body-file`; Backticks in doppelten Anführungszeichen
  werden von der Shell ausgeführt und zerlegen den Text.
- Release nur über Tag: `git tag -a vX.Y -m "…"` + `git push origin vX.Y`. Der
  Workflow baut signierte arm64-APK und Windows-ZIP. Danach prüfen:
  `gh release view vX.Y --json assets`, APK mit
  `apksigner verify --print-certs` und `aapt2 dump badging` (versionName).
- Deliverables sind ausschließlich APK und Windows-ZIP – kein Installer, kein MSIX.

## Common Pitfalls

1. **Release-APK auf den Emulator schieben.** Die ist arm64; der Emulator braucht
   den Debug-Build mit `-p:RuntimeIdentifier=android-x64`.
2. **Launcher zeigt das alte Icon.** Pixel-Launcher cached Icons: nach
   Icon-Änderungen `adb shell pm clear com.google.android.apps.nexuslauncher`
   oder neu installieren.
3. **Falsche Frequenz-Schätzung als Fehler verbuchen.** Unter etwa 100 Buchstaben
   kann Chi-Quadrat die falsche Cäsar-Verschiebung wählen – für Prüfungen einen
   mehrsätzigen Text verwenden.
4. **Leerer Branch nach Feature-Abbruch.** Branches, die auf einem schon
   gemergten Feature basieren, erst `git rebase --onto origin/master <alter-base>`
   rebasen, sonst entstehen Konflikte im Squash-Merge.
5. **Bildschirmfotos zur Verifikation.** Teuer und unpräzise; Text-Dump plus
   menschlicher Blick auf die Optik ist die Abmachung.

## Verification Checklist

- [ ] Windows-Build ohne neue Warnungen, `./scripts/test.sh` grün
- [ ] Neue Logik hat Tests in `tests/RunicLab.Tests/` und ist in der CSProj eingebunden
- [ ] Auf dem Emulator installiert; Beschriftungen/Ergebnisse am Text-Dump geprüft
- [ ] Testrezept an den Menschen übergeben, Rückmeldung abgewartet
- [ ] PR mit `Closes #<issue>` gemergt
- [ ] Bei Release: Tag gepusht, Assets vorhanden, APK-Signatur und `versionName` geprüft
