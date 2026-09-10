# Kratki vodič za odbranu

## „Gdje je baza?“

`AppDbContext` u `Data/AppDbContext.cs` definiše EF Core model koji se mapira na Microsoft SQL Server. Connection string je u `appsettings.json`.

## „Pokažite CRUD“

Najlakše: Admin → Karte.

- Create: `AdminCardsController.Create`
- Read: `AdminCardsController.Index`
- Update: `AdminCardsController.Edit`
- Delete: `AdminCardsController.Delete`

Drugi kompletan CRUD postoji nad špilovima u `DecksController` i REST `DecksApiController`.

## „Kako je Unity CardData prebačen?“

Unity `CardData : ScriptableObject` ima `cardName`, `type`, `baseValue`, `description` i `art`. Ti podaci su izvučeni u `Seed/cards.json`, slike su prenesene u `wwwroot/images/cards`, a pri prvom startu se upisuju u SQL tabelu `Cards`.

## „Kako radi Deck Builder?“

`DeckRulesService` implementira pravila iz originalnog `DeckBuilderController.CanAddCard()`:

- tačno 20 karata,
- bez duplikata,
- max 3 karte vrijednosti 3,
- max 8 Mage,
- max 1 Sentinel,
- max 5 Effect.

`DecksController` provjerava da korisnik zaista posjeduje odabrane karte prije upisa `Deck` + `DeckCard` redova u SQL Server.

## „Kako radi pack opening?“

`ShopController.OpenPack()`:

1. provjeri minimum 100 coina,
2. oduzme 100,
3. izabere 6 nasumičnih aktivnih karata,
4. novu kartu doda u `UserCards`,
5. za duplikat vraća 1 coin,
6. cijelo otvaranje zapiše u `PackOpenings` i `PackOpeningCards`.

## „Gdje je backend?“

ASP.NET Core controlleri + C# service layer + EF Core su backend. Pored MVC stranica postoje pravi REST endpointi u `Controllers/Api`.

## „Gdje je upravljačka ploča?“

Prijaviti se kao `admin@astralnexus.local` / `Admin123!` i otvoriti `/Admin`.
