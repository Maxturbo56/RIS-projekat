# SQL Server model

## Users

- Id (PK)
- UserName (unique)
- Email (unique)
- DisplayName
- Bio
- PasswordHash
- Role
- Coins
- Theme
- CreatedAtUtc

## Cards

- Id (PK)
- ExternalKey (unique)
- Name
- Type
- BaseValue
- Description
- ImagePath
- IsActive

## BoardCards

- Id (PK)
- ExternalKey (unique)
- Name
- Description
- ImagePath
- IsActive

## UserCards

Composite PK: `(UserId, CardId)`

- UserId (FK Users)
- CardId (FK Cards)
- Quantity
- AcquiredAtUtc

## Decks

- Id (PK)
- UserId (FK Users)
- Name
- IsValid
- CreatedAtUtc
- UpdatedAtUtc

Unique: `(UserId, Name)`

## DeckCards

Composite PK: `(DeckId, CardId)`

- DeckId (FK Decks)
- CardId (FK Cards)
- SortOrder

## PackOpenings

- Id (PK)
- UserId (FK Users)
- Cost
- DuplicateCoins
- OpenedAtUtc

## PackOpeningCards

Composite PK: `(PackOpeningId, Position)`

- PackOpeningId (FK PackOpenings)
- CardId (FK Cards)
- Position
- WasDuplicate

## Matches

- Id (PK)
- UserId (FK Users)
- DeckId (nullable FK Decks)
- Result
- TurnsPlayed
- CoinsAwarded
- PlayedAtUtc
