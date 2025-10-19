#!/bin/bash

# Kerko Backend Quick Deployment Script
# This script rebuilds and redeploys quickly (uses cache)

set -e  # Exit on error

echo "⚡ Starting quick deployment..."
echo ""

# Navigate to the backend directory
cd "$(dirname "$0")"

echo "📦 Stopping existing containers..."
docker compose down

echo ""
echo "🔨 Building new Docker image (with cache)..."
docker compose build

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
echo "✅ Quick deployment complete!"
echo ""
echo "📝 View logs: docker compose logs -f"
echo ""
