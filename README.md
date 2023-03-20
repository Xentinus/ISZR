<p align="center">
  <img src=".github/ISZR_Long_Trans.png">
</p>

# Informatikai Szolgálati jegy Rendszer

Röviden **ISZR**, egy **C# nyelven** készült **ASP.NET MVC keretrendszerű web alkalmazás** amely segítsévégel a felhasználók jogosultságokat, kamerafelvételek lementéseket igényelhetnek a helyi Informatikai csoporttól.

A web alkalmazás célja, hogy a felhasználók egyszerűen, könnyedén igényelhessenek és az igénylések folyamatát is követni tudják.

## Jellemzők

- Reszponzív felület
- Regisztráció mentes, Intranet elven műkődik
- Első bejelentkezéskor a felhasználó beállítja elérhetőségeit és már használható is
- Elérhetőség változtatása bármikor bármire
- Igénylések státuszait a felhasználók és ügyintézők követni tudják
- Külön adminisztrativ felület rendszer karbantartóknak
- Az adatbázis segítségével egyszerűen lehet elvenni és hozzáadni jogosultságokat a rendszerhez
- Könnyen áttekinthető és értelmezhető igénylési lap
- Hivatkozásokkal ellátott igénylési lapok és listák
- Nyomtatható igénylési és jogosultsági listák

## Felhasználói típusok

### Sima felhasználók

<p align="center">
	<img src=".github/user.png">
</p>

A sima felhasználók **csak a számukra igényelt** igényléseket látják. Más felhasználók igényléseit nem látják, és nem is képesek igényléseket létrehozni.

Az igénylések megtekintésén kivül képesek még a saját elérhetőségüket, adataikat átírni és képesek még a rendszerben nem archivált jogosultságokat megtekinteni.

### Ügyintézők

<p align="center">
	<img src=".github/ugyintezo.png">
</p>

Az ügyintézők az összes igénylést látják amenyek a rendszerben találhatóak. Ezen kivül csak **ők képesek igényléseket lérehozni** maguk vagy más felhasználók részére.

<p align="center">
	<img src=".github/ugyintezoRequests.png">
</p>

Továbbá az írányítópulton láthatják a saját igényléseinek statisztikáját is.

### Adminisztrátorok

<p align="center">
	<img src=".github/healthcheck.png">
	<img src=".github/statusChange.png">
</p>

Adminisztrátori jogosultsággal az igénylések státuszait képesek módosítani, valamint csak ők képesek létrehozni (regisztráción kivül) új felhasználókat, osztályokat, csoportokat, beosztásokat, jogosultságokat, kamerákat.

Részükre egy teljesen elkülönített felület látható, bármikor képesek az igénylő rendszerbe és az admin felület között mozogni. Valamint ki lett alakítva egy rendszerállapotot összegző felület, ahol könnyedén megtudhatják, a rendszerrel kapcsolatos fontosabb információkat.

## Felhasználók adatainak karbantartása

<p align="center">
	<img src=".github/userData.png">
</p>

Minden felhasználó képes a saját adatait módosítani, így amennyiben az elérhetősége vagy bármely más változik azt tudja változtatni.

A felhasználókon kivül az adminisztrátorok képesek minden felhasználó adatait megváltoztatni.

## Hivatkozások használata

<p align="center">
	<img src=".github/link2.png">
	<img src=".github/link1.png">
</p>

Minden igénylési lapon és listán az azonosító számok hivatkozásokkal vannak ellátva, ennek segítségével gyorsabban megnyítható az aktuális igénylés.

Nem kell keresni, szimplán csak rá kell kattintani az azonosító számra és már meg is jelenik az igénylés az alkalmazásban.

## Exportálható listák

<p align="center">
	<img src=".github/exportList.png">
</p>

Minden leszűrt igénylési listát vagy jogosultsági listát a felhasználók kitudnak nyomtani.
