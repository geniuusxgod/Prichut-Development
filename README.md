# Resort Empire

Resort Empire je hra vytvorená v Unity, v ktorej hráč spravuje ostrovný rezort. Projekt je zameraný na rozvoj rezortu a postupné odomykanie nových budov.

## 1. Požiadavky

Na spustenie projektu sú potrebné:

- Unity Hub
- Unity Editor 6000.6.6f1
- Git
- Windows 10/11 alebo iný operačný systém podporovaný použitou verziou Unity
- Pripojenie na internet pri prvom stiahnutí projektu a potrebných balíkov

## 2. Stiahnutie projektu

Projekt je dostupný na GitHub:

https://github.com/geniuusxgod/Prichut-Development

Otvorte PowerShell alebo terminál a zadajte:

```bash
git clone https://github.com/geniuusxgod/Prichut-Development.git
cd Prichut-Development
```

Prepnite sa na vetvu obsahujúcu Unity projekt:

```bash
git fetch origin
git switch feature/PRIH-87-project-setup
```

Táto vetva obsahuje základnú štruktúru Unity projektu. Po zlúčení zmien do vetvy `develop` bude možné používať aktuálnu spoločnú vývojovú verziu.

## 3. Otvorenie projektu v Unity Hub

1. Spustite Unity Hub.
2. Prejdite do sekcie **Projects**.
3. Kliknite na **Add → Add project from disk**.
4. Vyberte priečinok `Prichut-Development`.
5. Skontrolujte, či priečinok obsahuje `Assets`, `Packages` a `ProjectSettings`.
6. Vyberte Unity Editor verzie **6000.6.6f1**.
7. Otvorte projekt a počkajte na dokončenie importu súborov.

Pri prvom spustení môže import projektu trvať niekoľko minút.

## 4. Spustenie hlavnej scény

1. V Unity Editore otvorte panel **Project**.
2. Prejdite do priečinka `Assets/Scenes`.
3. Otvorte scénu `Main.unity`.
4. Kliknite na tlačidlo **Play** v hornej časti Unity Editora.
5. Skontrolujte zobrazenie ostrova a dostupných budov.
6. Otvorte **Window → General → Console** a skontrolujte prípadné chyby.

Cesta k hlavnej scéne:

`Assets/Scenes/Main.unity`

## 5. Testovanie pomocou Unity Device Simulator

Na simuláciu mobilného zariadenia použijeme Unity Device Simulator.

Postup:

1. Otvorte Unity projekt.
2. Prejdite do **Window → Package Manager**.
3. Overte dostupnosť balíka **Device Simulator** a v prípade potreby ho nainštalujte.
4. Otvorte režim **Simulator** v okne Game.
5. Vyberte profil mobilného zariadenia.
6. Nastavte orientáciu obrazovky na **Landscape**.
7. Spustite hlavnú scénu tlačidlom **Play**.
8. Skontrolujte správne zobrazenie ostrova a troch budov.
9. Skontrolujte Unity Console.

Device Simulator umožňuje overiť zobrazenie hry na simulovaných mobilných zariadeniach. Nenahrádza testovanie výkonu a kompatibility na fyzickom telefóne.

## 6. Práca s Git a Git Flow

Pri vývoji projektu používame Git Flow.

Hlavné vetvy:

- `main` – stabilná verzia projektu
- `develop` – spoločná vývojová vetva
- `feature/` – samostatné vetvy pre jednotlivé úlohy

Každá nová funkcionalita alebo zmena sa pripravuje v samostatnej vetve. Po dokončení sa vytvorí Pull Request do vetvy `develop`.

Príklad vytvorenia novej vetvy:

```bash
git switch develop
git pull origin develop
git switch -c feature/PRIH-XXX-nazov-ulohy
```

## 7. Riešenie problémov

**Projekt sa nedá otvoriť:**

- Skontrolujte správnu verziu Unity.
- Overte, či ste vybrali koreňový priečinok projektu.
- Skontrolujte prítomnosť priečinkov `Assets`, `Packages` a `ProjectSettings`.

**
