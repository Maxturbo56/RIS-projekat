# Kratka analiza tržišta

## Problem

Digitalne kolekcionarske kartaške igre moraju riješiti nekoliko istih problema: katalog velikog broja karata, korisničku kolekciju, validaciju špilova, progresiju/valutu, historiju partija i administratorsko upravljanje sadržajem.

## Referentne kategorije proizvoda

### Digitalni TCG klijenti

Primjeri poput Hearthstone, Magic: The Gathering Arena i Pokémon TCG sistema pokazuju standardni obrazac: korisnički račun, kolekcija, deck builder, paketi/nagrade i matchmaking/gameplay.

### Companion / collection sistemi

Dio tržišta odvaja samo upravljanje kolekcijom i deckovima od samog game engine-a. Takvi sistemi naglašavaju search/filter, deck rules, statistiku i sinkronizaciju podataka.

## Pozicioniranje Astral Nexusa

Ovaj projekt kombinuje oba pristupa:

1. SQL-backed informacioni sistem za korisnike, kolekciju, špilove i historiju.
2. Browser Match Lab za ključne utility funkcije partije.
3. Administratorski CMS/dashboard za dodavanje i izmjenu sadržaja bez rebuildanja Unity igre.
4. REST API koji kasnije može koristiti drugi klijent, uključujući eventualni Unity ili mobilni klijent.

## Prednosti projekta

- originalni card set i pravila već postoje
- podaci nisu zaključani u ScriptableObject assetima
- admin može mijenjati katalog kroz browser
- korisnička progresija je centralizovana u SQL Serveru
- isti backend može kasnije opsluživati više klijenata

## Tehnička vrijednost

Za ispitni projekt sistem demonstrira više od statičnog CRUD-a: autentifikaciju, role-based authorization, relacijsku bazu, N:M relacije, file upload, validaciju poslovnih pravila, transakciju pri pack opening-u, session state, REST API i responsive upravljačku ploču.
