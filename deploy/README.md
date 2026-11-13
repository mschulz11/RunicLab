# deploy

Alles, was mit dem Ausliefern zu tun hat, aber kein Anwendungscode ist.

| Ordner | Inhalt | Aktiv? |
| --- | --- | --- |
| `installer/` | `RunicLab.iss` – Inno-Setup-Skript für einen Windows-Installer | nein, Altbestand |

Ausgeliefert werden nur die signierte Android-APK (arm64) und das Windows-ZIP,
beide als Assets eines GitHub-Releases, erzeugt von
`.github/workflows/release.yml`. Ein Installer ist bewusst nicht Teil des
Prozesses: Das ZIP ist portabel (entpacken, `RunicLab.exe` starten), und ein
Installer brächte Signaturzertifikate und Deinstallationsroutinen mit, die für
eine Lern-App niemand braucht.

Wird `installer/` doch wiederbelebt, muss das Skript auf den neuen Pfad
`src/RunicLab/bin/...` zeigen und die Pipeline angepasst werden.
