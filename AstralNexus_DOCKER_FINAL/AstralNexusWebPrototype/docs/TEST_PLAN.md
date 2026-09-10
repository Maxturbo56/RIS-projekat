# Smoke test plan

1. `docker compose up -d`
2. `dotnet run --project src/AstralNexus.Web/AstralNexus.Web.csproj`
3. Otvori `http://localhost:5092`
4. Login kao demo
5. Dashboard mora prikazati kolekciju, starter špil i 2 testna meča
6. Kolekcija: search + filter
7. Špilovi: otvori `Awakening Starter`, edit i save
8. Kreiraj novi špil i zatim ga obriši
9. Paketi: otvori paket i provjeri coin stanje + historiju
10. Match Lab: start starter deck, draw/discard, +/- HP, dice, coin, end turn, finish
11. Profil: izmijeni display name/bio
12. Settings: promijeni Dark/Light temu
13. Logout, login kao admin
14. Admin Cards: kreiraj novu kartu, edit, delete
15. Admin Users: dodaj coine demo useru i vrati stanje
16. Otvori `/api/cards` i provjeri JSON
17. Provjeri SQL Server tabele preko SSMS/Azure Data Studio/DBeaver-a ako je dostupan
