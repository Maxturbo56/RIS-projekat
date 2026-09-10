# Astral Nexus - Docker pokretanje

Ova verzija pokreće **cijelu aplikaciju i Microsoft SQL Server bazu kroz Docker Compose**. Na računaru nije potrebno lokalno instalirati .NET SDK ni SQL Server; potreban je samo Docker Desktop.

## Najbrže pokretanje na Windowsu

1. Instalirati i pokrenuti Docker Desktop.
2. Raspakovati projekat.
3. Otvoriti PowerShell ili Terminal u root folderu projekta.
4. Pokrenuti:

```powershell
docker compose up --build -d
```

5. Sačekati oko 20-60 sekundi pri prvom pokretanju dok se SQL Server inicijalizira i aplikacija kreira/seeduje bazu.
6. Otvoriti browser:

```text
http://localhost:5092
```

## Demo nalozi

Korisnik:
- Email: `demo@astralnexus.local`
- Password: `Demo123!`

Administrator:
- Email: `admin@astralnexus.local`
- Password: `Admin123!`

## Provjera kontejnera

```powershell
docker compose ps
```

Log aplikacije:

```powershell
docker compose logs -f app
```

Log SQL Servera:

```powershell
docker compose logs -f sqlserver
```

## Zaustavljanje

```powershell
docker compose down
```

Podaci baze ostaju u Docker volume-u i nakon zaustavljanja.

## Potpuni reset baze

Ova komanda briše i Docker volume baze:

```powershell
docker compose down -v
docker compose up --build -d
```

## Docker arhitektura

```text
Browser
   |
   | http://localhost:5092
   v
Astral Nexus ASP.NET Core container
   |  port 8080 unutar Docker mreže
   |
   | Server=sqlserver,1433
   v
Microsoft SQL Server 2022 container
   |
   v
astral_nexus_sql_data Docker volume
```

Aplikacijski container ne koristi `localhost` za bazu. U Docker mreži se SQL Server dohvaća preko naziva servisa `sqlserver`. Van Dockera je SQL Server i dalje dostupan na `localhost:14333` radi administracije i testiranja.

## Docker fajlovi

- `Dockerfile` - multi-stage build ASP.NET Core aplikacije (.NET 10 SDK -> ASP.NET Core runtime)
- `docker-compose.yml` - pokreće `app` i `sqlserver` servise
- `.dockerignore` - iz build contexta izbacuje nepotrebne lokalne fajlove
- Docker volume `astral_nexus_sql_data` - trajno čuva SQL Server podatke
