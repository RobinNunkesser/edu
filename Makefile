.PHONY: build browser

PORT ?= 5155
APP_URL := http://localhost:$(PORT)

build:
	dotnet build edu.sln

browser:
	@if lsof -tiTCP:$(PORT) -sTCP:LISTEN >/dev/null 2>&1; then \
		echo "Dev-Server laeuft bereits auf $(APP_URL)"; \
		open $(APP_URL); \
	else \
		dotnet watch --project src/WebSite/WebSite.csproj run --launch-profile http; \
	fi