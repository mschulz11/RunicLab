# Plan: Ausgabe kopieren (#10)

- **Issue:** #10
- **Ziel:** Der verschlüsselte/entschlüsselte Text lässt sich auf Android **und**
  Windows in die Zwischenablage kopieren.
- **Nicht-Ziel:** Markierbarer Text im `Label`, Teilen-Funktion, Zwischenablage-
  Verlauf, Änderungen an den Chiffre-Algorithmen.

## Ursache

Die Ausgabe war ein MAUI-`Label`. Labels sind in MAUI nicht markierbar – auf
Android gibt es deshalb keinen Weg, den Text zu selektieren und zu kopieren
(unter Windows ging es nur, weil man den Text dort per Drag markieren kann).
Ein eigenes Bedienelement ist die einzige plattformübergreifend verlässliche
Lösung.

## Schritte

1. [x] Branch `feature/10-ausgabe-kopieren`
2. [x] `Views/KopierenButton.cs`: Button mit `TextQuelle`-Delegate, kopiert über
       `Clipboard.Default.SetTextAsync` (MAUI Essentials) und zeigt kurz
       „Kopiert ✓“ (Rückmeldung wird per `CancellationTokenSource` zurückgesetzt,
       damit schnelles Mehrfachklicken nicht hängen bleibt)
3. [x] `CipherEingabe`: Button im Ergebnisrahmen, sichtbar sobald `AusgabeText`
       gefüllt ist (`ErgebnisRahmenSichtbar == false` blendet ihn mit aus).
       Damit erben Cäser und Polybius das Kopieren automatisch.
4. [x] `CodeAlKindi`: eigener `KopierenButton` in der Schätzen-Karte, kopiert
       den entschlüsselten Klartext; sichtbar nur, wenn eine Schätzung vorliegt
5. [x] Doku: `.architektur.md` (Kopieren gehört zur Basis), `docs/architektur.md`,
       `README.md` (Funktionen)

## Verifikation (Windows, echte App)

- [x] Cäser: `KDOOR ZHOW` → Ergebnis `HALLO WELT` → „Kopieren“ geklickt →
      Button zeigt `Kopiert ✓`, `Get-Clipboard` liefert `HALLO WELT`
      (Zwischenablage vorher mit `ZWISCHENABLAGE-LEER` überschrieben, damit der
      Beweis eindeutig ist)
- [x] Al-Kindi: 27 Buchstaben analysiert → Klartext-Schätzung im Feld →
      „Kopieren“ → `Get-Clipboard` liefert genau diesen Text
- [x] Ohne Ergebnis ist kein Kopieren-Button sichtbar
- [x] `./scripts/test.sh`: 35/35 grün

## Offen / Grenzen

- Android wurde nicht auf einem Gerät getestet (kein Emulator vorhanden). Der
  Pfad nutzt ausschließlich `Clipboard.Default` aus MAUI Essentials, also dieselbe
  API auf beiden Plattformen; das Verhalten auf Android bleibt bis zum ersten
  Gerätetest eine Annahme und sollte dort einmal geprüft werden.
