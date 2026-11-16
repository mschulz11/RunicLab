# Plan: Nach Einstellungsänderung neu berechnen (#23)

- **Issue:** #23
- **Ziel:** Wurde schon ver-/entschlüsselt, rechnet die Seite nach einer Änderung
  der Einstellungen beim Zurückkehren automatisch mit den neuen Werten neu.
- **Nicht-Ziel:** Live-Neuberechnung beim Tippen im Eingabefeld, Speichern der
  letzten Eingabe über App-Neustarts hinweg, Änderungen an den Algorithmen.

## Ausgangslage

Nur `CodeAlKindi` rechnete nach Rückkehr aus den Einstellungen neu – über ein
eigenes Feld `_letzteEingabe`. Cäser und Polybius zeigten weiter das alte
Ergebnis, obwohl die Statuszeile bereits den neuen Modus bzw. die neue
Verschiebung nannte. Alle Einstellungsseiten schreiben ihre Werte sofort in
`Preferences`; die Zielseite liest sie in `OnAppearing`.

## Umsetzung

Alles in `Views/CipherPageBase.cs`, keine Kopie pro Seite:

1. `EingabeVerwenden` hängt nicht mehr direkt `AktionAusfuehren` ein, sondern
   `Ausfuehren()` – das merkt sich `Eingabe.EingabeText` und
   `EinstellungsMerkmale()` des Laufs.
2. Neuer Haken `protected virtual string EinstellungsMerkmale() => string.Empty` –
   Fingerabdruck der ergebnisrelevanten Einstellungen:

   | Seite | Fingerabdruck |
   | --- | --- |
   | `CodeCaeserVerfahren` | `Verschiebung\|Modus` |
   | `CodePolybius` | `Keyword\|Separator\|Umlaute\|Entschlüsseln` |
   | `CodeAlKindi` | `Sprache\|ZeigeErwartung\|TopAnzahl` |

3. `OnAppearing`: erst `EinstellungenAnwenden()` (Beschriftungen, Gitter,
   Statuszeile), dann – nur wenn schon einmal gerechnet wurde **und** der
   Fingerabdruck abweicht – erneut `Ausfuehren()`.
4. Ein zwischenzeitlich geleertes Eingabefeld bekommt die zuletzt gerechnete
   Eingabe zurück, damit Eingabe und Ergebnis nicht auseinanderlaufen; ein
   geänderter Feldinhalt hat Vorrang und wird mit den neuen Einstellungen
   gerechnet.
5. `CodeAlKindi` verliert sein eigenes `_letzteEingabe` und nutzt
   `WurdeBerechnet` aus der Basis.

## Verifikation (Android 15 Emulator, echte App)

- [x] Cäser: `Buchstaben` mit Verschiebung 3 → `Exfkvwdehq`; Einstellungen →
      Verschiebung 5 → zurück → Ergebnis `Gzhmxyfgjs`, Statuszeile „Verschiebung 5“
- [x] Polybius: `Hallo Welt` → `23 11 31 31 34 52 15 31 44`; Trennzeichen auf
      Bindestrich → zurück → `23-11-31-31-34 52-15-31-44` ohne Button-Druck
- [ ] Einstellungen ohne Änderung → kein Neuberechnen (Fingerabdruck gleich;
      im Code geprüft, optisch nicht unterscheidbar)
- [x] Al-Kindi: Analyse mit Deutsch, dann Sprache auf Englisch → zurück →
      „24 Buchstaben gezählt · Sprache: Englisch“, Erwartungswerte neu berechnet
- [x] Windows-Build 0 Warnungen, `scripts/test.sh` 35/35 grün
