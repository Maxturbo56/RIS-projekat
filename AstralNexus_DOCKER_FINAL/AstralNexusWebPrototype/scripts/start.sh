#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
docker compose up --build -d
echo "Astral Nexus Docker stack je pokrenut: http://localhost:5092"
docker compose ps
