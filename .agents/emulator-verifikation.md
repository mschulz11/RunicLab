# Neue Funktionen auf dem Android-Emulator prüfen

Ablauf für Agenten, wenn eine Änderung das Verhalten der App betrifft.

## Aufteilung

| Wer | Was |
| --- | --- |
| Agent | Emulator starten, Debug-Build installieren, App starten, zur betroffenen Seite navigieren, Testrezept schreiben |
| Agent | Funktionales prüfen: `uiautomator dump` auswerten (exakter Text, exakte `bounds`) |
| Mensch | Optik und Bedienung prüfen (Layout, Icon, Lesbarkeit) und Ergebnis zurückmelden |

Grund: Screenshots kosten pro Bild rund tausend Tokens, eine Prüfrunde mit
mehreren Bildern entsprechend viel – und liefern nur „sieht aus wie“, während ein
Text-Dump den tatsächlichen Wert einer Beschriftung zeigt. Für funktionale
Aussagen also Dump statt Bild; für Gestaltung schaut der Mensch.

Screenshots nur, wenn ausdrücklich gewünscht.

## Befehle

```bash
export ANDROID_HOME="C:\Program Files (x86)\Android\android-sdk"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export JAVA_HOME="C:\Program Files\Microsoft\jdk-17.0.21.21-hotspot"

# Emulator (aus PowerShell per Start-Process starten, sonst endet er mit dem Aufruf)
emulator.exe -avd pixel_7_-_api_35 -no-boot-anim
adb wait-for-device
adb shell getprop sys.boot_completed        # bis "1"

# Debug-Build: der Emulator ist x86_64, die Release-APK ist arm64
dotnet build src/RunicLab/RunicLab.csproj -f net10.0-android -c Debug \
  -p:RuntimeIdentifier=android-x64 -t:Install

# Starten
adb shell monkey -p com.companyname.runiclab -c android.intent.category.LAUNCHER 1
```

## Elemente finden und bedienen

```bash
adb shell uiautomator dump /sdcard/ui.xml
adb shell cat /sdcard/ui.xml | sed 's/></>\n</g' \
  | grep -E "EditText|Button" \
  | sed -E 's/.*text="([^"]*)".*class="([^"]*)".*bounds="([^"]*)".*/\1 | \2 | \3/'
```

- `bounds="[x1,y1][x2,y2]"` → Mitte ausrechnen und `input tap <x> <y>`.
  Blinde Koordinaten treffen oft daneben: Tastatur und Scrollposition verschieben
  alles.
- Text eingeben: `adb shell input text "Wort%sWort"` (`%s` = Leerzeichen).
- Feld leeren: Fokus setzen, `input keyevent 123` (Zeilenende), dann `keyevent 67`.
- Lange Texte auf der Seite zuerst ins Bild scrollen (`input swipe`), sonst landet
  der Tipp im Fließtext.
- Orientierung im dunklen Modus: `adb shell cmd uimode night yes|no`.

## Fallstricke

- **Launcher cached Icons.** Nach Icon-Änderungen hilft nur
  `adb shell pm clear com.google.android.apps.nexuslauncher` oder eine
  Neuinstallation, sonst zeigt der Startbildschirm weiter das alte Bild.
- **Activity-Name nicht raten**:
  `adb shell cmd package resolve-activity --brief com.companyname.runiclab`.
- **Kurze Texte täuschen die Häufigkeitsanalyse.** Unter etwa 100 Buchstaben kann
  die Cäsar-Schätzung die falsche Verschiebung wählen; für Prüfungen einen
  längeren Text (mehrere Sätze) verwenden.
- Der Emulator lebt nur bis zum Ende der ADB-Sitzung – nach einem Reboot des
  Rechners neu starten.
