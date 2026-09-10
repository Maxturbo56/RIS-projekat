#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
docker compose down -v
docker compose up --build -d
echo "Baza je resetovana i cijeli Astral Nexus Docker stack se ponovo pokrece."
