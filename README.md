# Resort Empire: Paradise Tycoon

Mobilna hra (Unity 2D, na sirku) - skolsky projekt UKF.

## Ako spustit hru

1. Nainstaluj Unity Hub a editor **Unity 6000.6.6f1** (rovnaka verzia pre vsetkych).
2. Naklonuj repozitar: `git clone https://github.com/geniuusxgod/prichut-development.git`
3. V Unity Hub daj **Add > Add project from disk** a vyber priecinok repozitara.
4. Otvor scenu `Assets/Scenes/Main.unity` a stlac Play.
5. Ak je scena prazdna alebo chybna, v menu vyber **Resort Empire > Rebuild Main Scene**.

## Struktura priecinkov

- `Assets/Art/Sprites` - obrazky z dizajnu (ostrov, ocean, budovy, oblaky)
- `Assets/Scenes` - sceny (`Main.unity`)
- `Assets/Scripts` - skripty (`CameraFitter` drzi celu scenu na sirku v zabere kamery)
- `Design` - povodne SVG a referencny obrazok `full_scene.png` od dizajnera

## Git pravidla

- Do `main` sa nepise priamo. Kazda uloha ma vlastnu vetvu: `feature/PRIH-cislo-nazov`.
- Zmeny idu cez Pull Request, ktory musi schvalit aspon jeden dalsi clen timu.
- Commit sprava zacina cislom ulohy, napr. `PRIH-11: ostrov a budovy v scene`.
- Subory `.meta` sa commitu vzdy spolu s obrazkami a skriptami.
