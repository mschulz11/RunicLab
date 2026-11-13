# Neues Verschlüsselungsverfahren hinzufügen

Gilt für alle Verfahren, die nach dem Umbau aus Issue #11 dazukommen. Ziel ist
eine Seite pro Verfahren plus eine reine Logik-Klasse – kein Copy/Paste einer
bestehenden Seite.

## 1. Logik

1. `src/RunicLab/Models/<Verfahren>Cipher.cs` anlegen.
2. Nur `System.*` verwenden, keine MAUI-Typen, keine UI-Zustände.
3. Signatur an den bestehenden Klassen orientieren (`CaeserCipher`, `PolybiusCipher`,
   `FrequencyAnalysis`): statische `Encrypt`/`Decrypt`-Methoden mit
   `string`-Ein- und -Ausgabe.
4. Testdatei `tests/RunicLab.Tests/<Verfahren>CipherTests.cs` anlegen und in
   `tests/RunicLab.Tests/RunicLab.Tests.csproj` per `<Compile Include>` einbinden.

## 2. Oberfläche

1. `Views/Code<Verfahren>.xaml` + `.xaml.cs` von der gemeinsamen Basis ableiten
   (Base-Form aus #11), nicht von `ContentPage` kopieren.
2. Einstellungen in `Views/Settings<Verfahren>.xaml` bzw. einer
   `*Preferences`-Klasse kapseln, damit der Zustand testbar bleibt.
3. Navigation in `AppShell.xaml` bzw. `Views/MainPage.xaml` ergänzen.

## 3. Abschluss

1. `docs/architektur.md` um das Verfahren ergänzen (Tabelle „Verfahren“).
2. `README.md` → Abschnitt „Funktionen“ und „Projektstruktur“ prüfen.
3. Checkliste in `.agents/review-checkliste.md` abarbeiten.
