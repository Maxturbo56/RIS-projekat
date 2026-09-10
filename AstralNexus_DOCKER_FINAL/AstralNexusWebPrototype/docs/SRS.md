# SRS — Astral Nexus

## 1. Svrha sistema

Astral Nexus je web informacioni sistem koji digitalizuje upravljanje kolekcionarskom kartaškom igrom. Sistem centralizuje korisničke račune, katalog karata, kolekcije, špilove, otvaranja paketa, rezultate mečeva i administratorsko upravljanje.

## 2. Akteri

### Gost

- pregled početne stranice
- registracija
- prijava
- javni `GET /api/cards`

### Korisnik

- upravljanje profilom i temom
- pregled kolekcije
- pretraga i filtriranje karata
- kreiranje, pregled, izmjena i brisanje vlastitih špilova
- kupovina/otvaranje paketa
- pokretanje Match Lab meča
- draw/discard, HP, potezi, dice, coin
- završavanje meča i zapis rezultata
- pregled statistike
- korištenje korisničkih REST endpointa

### Administrator

- sve korisničke funkcije
- administratorski dashboard
- CRUD nad kartama
- upload slike karte
- aktivacija/deaktivacija karata
- upravljanje korisnicima, coinima i ulogama
- brisanje korisnika
- globalna statistika sistema
- administratorski REST Card CRUD

## 3. Funkcionalni zahtjevi

### FR-01 Registracija

Sistem mora omogućiti kreiranje korisničkog računa sa jedinstvenim korisničkim imenom i email adresom.

### FR-02 Autentifikacija

Sistem mora omogućiti login/logout i zaštititi korisničke i administratorske resurse prema ulozi.

### FR-03 Profil

Korisnik mora moći pregledati profil, coin stanje, statistiku i urediti display name/bio.

### FR-04 Postavke teme

Korisnik mora moći odabrati tamnu ili svijetlu temu. Izbor se čuva u bazi i cookie-u.

### FR-05 Kolekcija

Sistem mora prikazivati samo karte koje korisnik posjeduje te omogućiti search/filter prema nazivu, opisu, tipu i vrijednosti.

### FR-06 Deck CRUD

Korisnik mora moći kreirati, učitati, ažurirati i obrisati vlastiti špil.

### FR-07 Validacija špila

Špil mora imati tačno 20 različitih karata i zadovoljiti originalna Astral Nexus pravila:

- najviše 3 karte vrijednosti 3
- najviše 8 Mage
- najviše 1 Sentinel
- najviše 5 Effect

Sistem mora provjeriti da korisnik posjeduje svaku odabranu kartu.

### FR-08 Otvaranje paketa

Paket košta 100 coina i sadrži 6 nasumičnih aktivnih karata. Nova karta se dodaje kolekciji. Duplikat daje 1 coin.

### FR-09 Historija paketa

Svako otvaranje i svih šest rezultata moraju biti sačuvani u SQL Serveru.

### FR-10 Match Lab

Sistem mora omogućiti pokretanje meča sa validnim špilom, random board kartom, početnom rukom od šest karata, draw/discard, HP counter, turn pass, dice i coin flip.

### FR-11 Rezultat meča

Pobjeda daje 20 coina. Poraz daje 5 coina plus `turns / 2`. Rezultat mora biti zapisan u SQL Server.

### FR-12 Admin Card CRUD

Administrator mora moći kreirati, čitati, ažurirati i brisati nekorištene karte. Karta koja je dio historijskih podataka deaktivira se umjesto fizičkog brisanja radi referencijalnog integriteta.

### FR-13 Admin korisnici

Administrator mora moći pregledati korisnike, mijenjati coin stanje i uloge te brisati korisnike osim vlastitog aktivnog admin profila.

### FR-14 REST API

Sistem mora izložiti REST endpoint-e za karte, kolekciju, špilove i statistiku.

## 4. Nefunkcionalni zahtjevi

### NFR-01 Tehnologija

Aplikacijska logika mora biti ASP.NET Core / C#. Trajna baza mora biti Microsoft SQL Server.

### NFR-02 Sigurnost lozinki

Lozinke se ne smiju čuvati u plaintext-u. Prototip koristi PBKDF2-SHA256 sa random saltom i 120.000 iteracija.

### NFR-03 Autorizacija

Administratorske funkcije moraju biti zaštićene `Admin` ulogom.

### NFR-04 Referencijalni integritet

SQL relacije i FK pravila moraju spriječiti nekonzistentne Deck/Card/User podatke.

### NFR-05 Responsive UI

Interfejs mora biti upotrebljiv u desktop i mobilnom browseru.

### NFR-06 Perzistencija

Restart web aplikacije ne smije obrisati korisnike, kolekcije, špilove, coine ili historiju. Docker volume čuva SQL podatke.

## 5. Podaci

Glavne relacije:

- User 1:N UserCard
- Card 1:N UserCard
- User 1:N Deck
- Deck N:M Card kroz DeckCard
- User 1:N Match
- User 1:N PackOpening
- PackOpening 1:N PackOpeningCard
- Card 1:N PackOpeningCard

## 6. Granice prototipa

Unity animacije, scene i LAN/Netcode implementacija nisu dio ASP.NET web runtime-a. Match Lab portuje relevantne gameplay utility funkcije i persistence, dok se složeni vizuelni board interaction može razvijati kao zaseban naredni modul.
