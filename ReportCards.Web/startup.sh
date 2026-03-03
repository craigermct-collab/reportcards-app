#!/bin/bash
# Startup script for Azure App Service
# Ensures Python dependencies are installed on every container start

echo "Installing Python dependencies..."
python3 get-pip.py --break-system-packages 2>/dev/null || true
python3 -m pip install pypdf --break-system-packages --quiet
echo "Python dependencies installed."

# Start the application
dotnet /home/site/wwwroot/ReportCards.Web.dll
