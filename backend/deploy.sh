#!/bin/bash

# Kerko Backend Deployment Script
# This script rebuilds and redeploys the Docker container with your latest changes

set -e  # Exit on error

echo "🚀 Starting deployment..."
echo ""

# Navigate to the backend directory
cd "$(dirname "$0")"

echo "📦 Stopping existing containers..."
docker compose down

echo ""
echo "🧹 Cleaning up old images..."
docker image prune -f

echo ""
echo "🔨 Building new Docker image..."
docker compose build --no-cache

echo ""
echo "🚀 Starting containers..."
docker compose up -d

echo ""
echo "⏳ Waiting for container to be healthy..."
sleep 3

echo ""
echo "📊 Container status:"
docker compose ps

echo ""
echo "✅ Deployment complete!"
echo ""
echo "📝 Useful commands:"
echo "  - View logs:    docker compose logs -f"
echo "  - Stop:         docker compose down"
echo "  - Restart:      docker compose restart"
echo ""
