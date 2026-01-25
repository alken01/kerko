#!/bin/bash

cd "$(dirname "$0")"

docker compose down 2>/dev/null
docker compose build --no-cache
docker compose up -d

echo ""
echo "Waiting for container to start..."
sleep 3

if curl -s http://localhost:8080/api/health | grep -q "OK"; then
    echo "Container is running at http://localhost:8080"
    echo "Health check: OK"
else
    echo "Container failed to start. Logs:"
    docker compose logs
fi
