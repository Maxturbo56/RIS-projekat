# Analiza koda i arhitekture

## Presentation layer

`Views/` sadrži Razor prikaz. Nema React/Angular/Vue/Node logike i nema vlastitog JavaScript koda. Forme se šalju ASP.NET Core controllerima.

## Controller layer

Glavni controlleri:

- `AccountController` — register/login/logout
- `DashboardController` — korisnička statistika
- `CollectionController` — kolekcija i filteri
- `DecksController` — web Deck CRUD
- `ShopController` — pack opening transakcija
- `GameController` — Match Lab runtime state i rezultat
- `ProfileController` — profil i tema
- `AdminCardsController` — administratorski Card CRUD
- `AdminUsersController` — administracija korisnika
- `Controllers/Api/*` — REST backend

## Service layer

### `DeckRulesService`

Izdvaja poslovna pravila špila iz UI/controller logike. Isto pravilo koriste web controller i REST API.

### `PasswordService`

Hash/verify PBKDF2 lozinki.

### `CardImageService`

Validacija i čuvanje uploadovanih slika karata u `wwwroot/images/cards/uploads`.

## Data layer

`AppDbContext` je EF Core jedinica pristupa SQL Serveru. Fluent konfiguracija definiše:

- unique indekse
- composite primary keys
- foreign key relacije
- cascade/restrict/set-null delete pravila

`DatabaseSeeder` pretvara podatke originalnih Unity ScriptableObject karata u SQL redove pri prvom pokretanju.

## Transakcije

`ShopController.OpenPack()` koristi SQL transakciju zato što jedna poslovna operacija istovremeno mijenja:

- coin stanje korisnika
- kolekciju
- PackOpening
- šest PackOpeningCard redova

Ako operacija pukne, transakcija sprječava djelimično zapisano stanje.

## Aktivni meč

Aktivni Match Lab state je kratkotrajan i čuva se u ASP.NET session-u. Završeni rezultat, turns i coin nagrada se upisuju u SQL `Matches`, što razdvaja privremeni runtime state od trajne poslovne historije.
