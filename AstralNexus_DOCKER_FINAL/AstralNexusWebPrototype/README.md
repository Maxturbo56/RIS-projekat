# Astral Nexus Web Prototype

Potpuni web prototip zasnovan na originalnom Unity projektu **Astral Nexus** i njegovim assetima.


## Docker verzija - aplikacija + baza

Za predaju/deployment cijeli sistem se može podići samo pomoću Docker Desktopa. ASP.NET Core aplikacija i Microsoft SQL Server rade u odvojenim kontejnerima, a baza koristi persistent Docker volume.

```bash
docker compose up --build -d
```

Nakon inicijalizacije otvorite `http://localhost:5092`. Za gašenje koristite `docker compose down`, a za potpuni reset baze `docker compose down -v`. Detalji su u `DOCKER.md`. Na Windowsu se može koristiti i `START_DOCKER.bat`.

## Tehnologije

- .NET 10 / ASP.NET Core MVC
- C# frontend logika kroz Razor Views + C# backend
- Entity Framework Core 10
- Microsoft SQL Server 2022 Developer (Docker)
- Cookie autentifikacija + `User` / `Admin` uloge
- REST API endpointi
- Responsive browser UI

Aplikacija ne koristi React, Angular, Node ili drugi frontend/backend programski stack. HTML/CSS služe za prikaz u browseru, dok je aplikacijska logika implementirana u ASP.NET Core/C#.

## Šta je preneseno iz Unity prototipa

- 124 `CardData` ScriptableObject karte i njihove originalne slike
- 10 board karata i njihove slike
- Player collection + coins
- Deck create/read/update/delete
- Originalna pravila Deck Buildera (20 karata, max 3 value-3, max 8 Mage, max 1 Sentinel, max 5 Effect)
- Pack opening: 100 coina, 6 karata, duplikat vraća 1 coin
- Health counter (10 HP), draw/discard, turn passing, dice roll i coin flip
- End-game nagrade i historija mečeva
- Originalni Astral Nexus branding / box art

Unity-specifični `GameObject`, `MonoBehaviour`, scene, animacije i LAN networking nisu kopirani 1:1; njihove relevantne podatkovne i poslovne funkcije su implementirane kao web sistem.

## Funkcionalnosti

### Korisnik

- registracija / prijava / odjava
- profil korisnika
- dark/light theme settings
- dashboard
- pregled, pretraga i filtriranje kolekcije
- Deck Builder CRUD
- otvaranje paketa i historija paketa
- Match Lab (draw, discard, health, turn, dice, coin, board card)
- historija pobjeda/poraza i coin nagrade

### Administrator

- sistemski dashboard
- Card CRUD
- upload slike nove karte
- aktivacija/deaktivacija karata
- upravljanje korisnicima
- dodavanje/oduzimanje coina
- promjena User/Admin uloge
- brisanje korisnika
- pregled globalne statistike

### REST API

- `GET /api/cards`
- `GET /api/cards/{id}`
- `POST /api/cards` (Admin)
- `PUT /api/cards/{id}` (Admin)
- `DELETE /api/cards/{id}` (Admin)
- `GET /api/collection` (prijavljen korisnik)
- `GET /api/decks` (prijavljen korisnik)
- `GET /api/decks/{id}`
- `POST /api/decks`
- `PUT /api/decks/{id}`
- `DELETE /api/decks/{id}`
- `GET /api/stats`

## Alternativno: lokalni razvoj kroz Visual Studio Code

### Potrebno za ovaj alternativni način

1. .NET 10 SDK
2. Docker Desktop
3. Visual Studio Code
4. C# Dev Kit / C# extension za VS Code

### 1. Otvorite folder

Otvorite root folder `AstralNexusWebPrototype` u VS Code-u.

### 2. Pokrenite Microsoft SQL Server (lokalni .NET način)

Ako aplikaciju pokrećete lokalno preko `dotnet run`, bazu možete podići sa:

```bash
docker compose up -d sqlserver
```

SQL Server je dostupan na:

- host: `localhost`
- port: `14333`
- database: `AstralNexusDb` (automatski se kreira)
- user: `sa`
- password: `AstralNexus!2026`

### 3. Restore i run

```bash
dotnet restore
dotnet run --project src/AstralNexus.Web/AstralNexus.Web.csproj
```

Aplikacija se otvara na:

```text
http://localhost:5092
```

Na prvom pokretanju aplikacija:

1. čeka da SQL Server bude spreman,
2. kreira bazu i tabele preko EF Core,
3. učitava 124 originalne karte,
4. učitava 10 board karata,
5. kreira demo i admin korisnika,
6. kreira demo kolekciju, starter deck i testne rezultate.

## Demo nalozi

### Korisnik

- Email: `demo@astralnexus.local`
- Password: `Demo123!`

### Administrator

- Email: `admin@astralnexus.local`
- Password: `Admin123!`

Ovo su namjerno javne development šifre za prototip. Promijeniti prije bilo kakvog javnog deploymenta.

## Reset baze

Ako želite potpuno čistu bazu:

```bash
docker compose down -v
docker compose up -d sqlserver
dotnet run --project src/AstralNexus.Web/AstralNexus.Web.csproj
```

### Ako je prethodno kreiranje baze puklo na tabeli `Matches`

Obavezno uradite puni reset iznad (`docker compose down -v`). `EnsureCreated()` može ostaviti djelimično kreiranu bazu nakon SQL greške, pa samo ponovno pokretanje aplikacije nije dovoljno.

## SQL Server bez Dockera

Ako već imate Microsoft SQL Server, promijenite `ConnectionStrings:DefaultConnection` u:

`src/AstralNexus.Web/appsettings.json`

Aplikacija zahtijeva **Microsoft SQL Server**, ne SQLite.

## Struktura

```text
AstralNexusWebPrototype/
├── Dockerfile
├── .dockerignore
├── docker-compose.yml
├── DOCKER.md
├── START_DOCKER.bat
├── AstralNexusWebPrototype.sln
├── docs/
├── scripts/
└── src/
    └── AstralNexus.Web/
        ├── Controllers/
        │   └── Api/
        ├── Data/
        ├── Extensions/
        ├── Models/
        ├── Services/
        ├── Seed/
        ├── Views/
        └── wwwroot/
```

Detaljno mapiranje Unity → .NET nalazi se u `docs/UNITY_TO_DOTNET_MAPPING.md`.
