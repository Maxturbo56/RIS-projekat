/* Reference schema for Astral Nexus.
   The application itself creates the schema through EF Core EnsureCreated(). */

IF DB_ID(N'AstralNexusDb') IS NULL
    CREATE DATABASE AstralNexusDb;
GO
USE AstralNexusDb;
GO

CREATE TABLE Users (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserName nvarchar(50) NOT NULL,
    Email nvarchar(120) NOT NULL,
    DisplayName nvarchar(80) NOT NULL DEFAULT N'',
    Bio nvarchar(500) NOT NULL DEFAULT N'',
    PasswordHash nvarchar(500) NOT NULL,
    Role nvarchar(20) NOT NULL,
    Coins int NOT NULL,
    Theme nvarchar(20) NOT NULL,
    CreatedAtUtc datetime2 NOT NULL
);
CREATE UNIQUE INDEX IX_Users_UserName ON Users(UserName);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

CREATE TABLE Cards (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ExternalKey nvarchar(150) NOT NULL,
    Name nvarchar(150) NOT NULL,
    Type nvarchar(80) NOT NULL,
    BaseValue int NOT NULL,
    Description nvarchar(3000) NOT NULL,
    ImagePath nvarchar(400) NOT NULL,
    IsActive bit NOT NULL
);
CREATE UNIQUE INDEX IX_Cards_ExternalKey ON Cards(ExternalKey);

CREATE TABLE BoardCards (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ExternalKey nvarchar(150) NOT NULL,
    Name nvarchar(150) NOT NULL,
    Description nvarchar(3000) NOT NULL,
    ImagePath nvarchar(400) NOT NULL,
    IsActive bit NOT NULL
);
CREATE UNIQUE INDEX IX_BoardCards_ExternalKey ON BoardCards(ExternalKey);

CREATE TABLE UserCards (
    UserId int NOT NULL,
    CardId int NOT NULL,
    Quantity int NOT NULL,
    AcquiredAtUtc datetime2 NOT NULL,
    CONSTRAINT PK_UserCards PRIMARY KEY(UserId, CardId),
    CONSTRAINT FK_UserCards_Users FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserCards_Cards FOREIGN KEY(CardId) REFERENCES Cards(Id)
);

CREATE TABLE Decks (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId int NOT NULL,
    Name nvarchar(80) NOT NULL,
    IsValid bit NOT NULL,
    CreatedAtUtc datetime2 NOT NULL,
    UpdatedAtUtc datetime2 NOT NULL,
    CONSTRAINT FK_Decks_Users FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_Decks_UserId_Name ON Decks(UserId, Name);

CREATE TABLE DeckCards (
    DeckId int NOT NULL,
    CardId int NOT NULL,
    SortOrder int NOT NULL,
    CONSTRAINT PK_DeckCards PRIMARY KEY(DeckId, CardId),
    CONSTRAINT FK_DeckCards_Decks FOREIGN KEY(DeckId) REFERENCES Decks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DeckCards_Cards FOREIGN KEY(CardId) REFERENCES Cards(Id)
);

CREATE TABLE PackOpenings (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId int NOT NULL,
    Cost int NOT NULL,
    DuplicateCoins int NOT NULL,
    OpenedAtUtc datetime2 NOT NULL,
    CONSTRAINT FK_PackOpenings_Users FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE PackOpeningCards (
    PackOpeningId int NOT NULL,
    CardId int NOT NULL,
    Position int NOT NULL,
    WasDuplicate bit NOT NULL,
    CONSTRAINT PK_PackOpeningCards PRIMARY KEY(PackOpeningId, Position),
    CONSTRAINT FK_PackOpeningCards_PackOpenings FOREIGN KEY(PackOpeningId) REFERENCES PackOpenings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PackOpeningCards_Cards FOREIGN KEY(CardId) REFERENCES Cards(Id)
);

CREATE TABLE Matches (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId int NOT NULL,
    DeckId int NULL,
    Result nvarchar(20) NOT NULL,
    TurnsPlayed int NOT NULL,
    CoinsAwarded int NOT NULL,
    PlayedAtUtc datetime2 NOT NULL,
    CONSTRAINT FK_Matches_Users FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Matches_Decks FOREIGN KEY(DeckId) REFERENCES Decks(Id) ON DELETE NO ACTION
);
GO
