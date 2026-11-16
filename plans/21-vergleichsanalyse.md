# Plan: Vergleichsanalyse über alle Verfahren (#21)

- **Issue:** #21
- **Ziel:** Die Häufigkeitsanalyse soll einen Geheimtext auch dann entschlüsseln,
  wenn nicht bekannt ist, mit welchem Verfahren er erzeugt wurde: alle Verfahren
  der App blind durchprobieren, die Ergebnisse bewerten und den besten Kandidaten
  samt Klartext anzeigen.
- **Nicht-Ziel:** Gitter-Schlüsselwörter erraten, Substitutions-Chiffren brechen,
  Vigenère, Änderungen an den bestehenden Verfahren.

## Idee

Das Bewertungsmaß ist schon da: Chi-Quadrat gegen die typische
Buchstabenverteilung der Sprache (`FrequencyAnalysis`). Für jeden Kandidaten
entsteht ein Klartext; je kleiner sein Chi-Quadrat, desto eher ist es die
Sprache – und damit das richtige Verfahren.

## Umsetzung

1. `Models/FrequencyAnalysis.cs`: neue öffentliche Methode
   `ChiQuadrat(text, language)` – der bisher in `GuessCaesarShift` eingebaute
   Vergleich, jetzt einzeln nutzbar (`null` bei zu kurzem Text).
2. `Models/CipherVergleich.cs` (neu, UI-frei):
   - `record Verfahrensbefund(Verfahren, Details, Klartext, Guetewert)`
   - `Analysiere(text, language, polybius)` prüft
     - **Unverschlüsselt** (Text unverändert),
     - **Cäsar** (beste Verschiebung aus `GuessCaesarShift`; Verschiebung 0 wird
       übersprungen, weil das identisch zum unveränderten Text wäre),
     - **Polybius-Quadrat** (Standardgitter und – falls gesetzt – mit dem
       konfigurierten Schlüsselwort), aber nur wenn `SiehtNachKoordinaten(text)`
       gilt: mindestens 2×20 Ziffern, gerade Anzahl, jedes Paar 1–5, keine
       Buchstaben im Geheimtext.
   - Ergebnis absteigend nach Güte (kleiner Chi-Quadrat-Wert zuerst).
3. `Views/CodeAlKindi`: neue Karte „Verfahrensvergleich“ mit den besten drei
   Kandidaten (Platz, Verfahren, Einstellung, Chi² und Klartext), gefüllt aus
   `CipherVergleich.Analysiere(...)` mit der eingestellten Sprache und dem
   Polybius-Schlüsselwort aus den Einstellungen. Erklärtext auf der Seite ergänzt.
   - Die alte Karte „Schätzung (Cäsar)“ ist entfallen (redundant, der Vergleich
     liefert dieselbe Aussage samt Platzierung).
   - Besteht die Eingabe aus Zahlenpaaren, zählt die Buchstabenhäufigkeit den
     Klartext des besten Kandidaten („15“ → E) statt leer zu bleiben.
4. `tests/RunicLab.Tests/CipherVergleichTests.cs`: 8 Tests (unverschlüsselt,
   Cäsar, Polybius, Polybius mit Schlüsselwort, englische Verteilung, zu kurzer
   Text, Koordinaten-Erkennung, Sortierung).

## Verifikation

- [x] `scripts/test.sh`: 43/43 grün
- [x] Windows-Build 0 Warnungen
- [x] Emulator: Polybius-Geheimtext (`14 24 15 …`) in die Analyse gegeben →
      Platz 1 „Polybius-Quadrat – Standardgitter“, Klartext
      `DIEHAEUFIGKEITSANALYSEISTEINEDERAELTESTENMETHODENDERKRYPTOANALYSE`
- [x] Emulator: Cäser-Geheimtext (~150 Buchstaben) → Platz 1 „Cäsar –
      Verschiebung 3“ (Chi² 302,5), Platz 2 „Unverschlüsselt“ (Chi² 8005,0)
- [x] Klartext-Fall durch Test `Analysiere_UnverschluesselterText_…` abgedeckt

## Grenzen

- Ein Polybius-Text mit fremdem Schlüsselwort lässt sich nicht blind lösen: das
  Gitter hat 25!/(…) Möglichkeiten. Der Vergleich probiert Standardgitter und das
  eingestellte Schlüsselwort; beides kann danebenliegen.
- Sehr kurze Texte (< 20 Buchstaben) liefern keine belastbare Bewertung; der
  Vergleich bleibt dann leer und die Karte sagt das.
- Polybius verliert beim Entschlüsseln die Wortgrenzen; der Klartext kommt als
  durchgehende Buchstabenfolge zurück.
