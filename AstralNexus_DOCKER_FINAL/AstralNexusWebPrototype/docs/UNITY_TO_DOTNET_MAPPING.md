# Unity → ASP.NET Core mapiranje

| Unity dio | Web/.NET implementacija | SQL Server |
|---|---|---|
| `CardData : ScriptableObject` | `Card` entity + seed importer | `Cards` |
| `BoardManager` + BoardCards | `BoardCard` + random izbor pri startu meča | `BoardCards` |
| `PlayerCollectionData` | `ApplicationUser` + `UserCard` | `Users`, `UserCards` |
| `PlayerCollectionManager.AddCard()` | `ShopController.OpenPack()` / EF Core | `UserCards` |
| `PlayerCollectionManager.AddCoins()` | coin izmjene na `ApplicationUser` | `Users.Coins` |
| `DeckSaveData` JSON | `Deck` + `DeckCard` entities | `Decks`, `DeckCards` |
| `DeckBuilderController.SaveDeck()` | `DecksController.Create/Edit()` | `Decks`, `DeckCards` |
| `CanAddCard()` pravila | `DeckRulesService` | validacija prije SQL upisa |
| `OpenPackScript` | `ShopController` | `PackOpenings`, `PackOpeningCards`, `UserCards` |
| `HealthCounter` | `GameSessionState.PlayerHp/OpponentHp` | finalni rezultat ide u `Matches` |
| `DeckController.DrawOne()` | `GameController.Draw()` | aktivni meč je session state |
| `DiscardManager` | `GameSessionState.Discard` | finalna historija u `Matches` |
| `PassHandHandler` | `GameController.EndTurn()` | `TurnsPlayed` u `Matches` |
| `DiceRoller` | `GameController.RollDice()` | runtime funkcija |
| `CoinFlipper` | `GameController.FlipCoin()` | runtime funkcija |
| `EndGamePanelScript` | `GameController.Finish()` | `Matches`, `Users.Coins` |
| Unity MainMenu | ASP.NET Core landing/dashboard | — |
| Unity ShopScene | Razor `Shop` UI | SQL podaci |
| Unity DeckBuilder scene | Razor `Decks/Editor` UI | SQL podaci |
| Unity LAN networking | nije portan 1:1 | REST API predstavlja mrežni backend sistema |

## Ključna arhitektura

```text
Browser
  │
  ▼
ASP.NET Core MVC / Razor
  │
  ├── Account / Dashboard / Collection / Decks / Shop / Game
  ├── Admin dashboard
  └── REST API Controllers
  │
  ▼
C# Services + Entity Framework Core
  │
  ▼
Microsoft SQL Server
```

ScriptableObject podaci više nisu glavni runtime storage. Pri prvom startu originalni podaci iz `Seed/cards.json` i `Seed/board-cards.json` ulaze u SQL Server i od tada se sistem ponaša kao standardna C#/SQL aplikacija.
