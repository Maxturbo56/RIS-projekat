# Docker deployment

Astral Nexus ima Docker verziju aplikacije i baze podataka radi jednostavnog lokalnog pokretanja.

## Servisi

### app
ASP.NET Core MVC aplikacija se gradi multi-stage Dockerfile-om. Build faza koristi `.NET SDK 10.0`, a runtime faza `ASP.NET Core Runtime 10.0`. Aplikacija sluša port `8080` u containeru i mapirana je na host port `5092`.

### sqlserver
Baza koristi službeni `Microsoft SQL Server 2022` Linux image. SQL Server sluša port `1433` u Docker mreži i mapiran je na `14333` na host računaru. Podaci se čuvaju u named volume-u `astral_nexus_sql_data`.

## Komunikacija

Aplikacija se na SQL Server spaja preko Docker DNS naziva `sqlserver`, a ne preko `localhost`:

`Server=sqlserver,1433;Database=AstralNexusDb;...`

Postojeći `DatabaseBootstrapper` čeka dostupnost baze, poziva Entity Framework Core `EnsureCreatedAsync()` i inicijalno učitava seed podatke.

## Pokretanje

```bash
docker compose up --build -d
```

Aplikacija: `http://localhost:5092`

Zaustavljanje:

```bash
docker compose down
```

Potpuni reset baze:

```bash
docker compose down -v
docker compose up --build -d
```
