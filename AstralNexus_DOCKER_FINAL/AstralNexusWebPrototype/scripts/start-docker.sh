#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
docker compose up --build -d
echo "Astral Nexus se pokrece na http://localhost:5092"
docker compose ps
