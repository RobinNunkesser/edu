.PHONY: build browser serve-stop browser-clean

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

serve-stop:
	@port_pids=$$(lsof -tiTCP:$(PORT) -sTCP:LISTEN 2>/dev/null || true); \
	watch_pids=$$(pgrep -f 'src/WebSite/WebSite.csproj run --launch-profile http' || true); \
	pids=$$(printf '%s\n%s\n' "$$port_pids" "$$watch_pids" | awk 'NF' | sort -u | tr '\n' ' '); \
	if [ -n "$$pids" ]; then \
		echo "Stoppe lokale WebSite-Instanzen (PID $$pids)"; \
		kill $$pids; \
	else \
		echo "Keine lokale WebSite-Instanz aktiv"; \
	fi

browser-clean:
	@$(MAKE) serve-stop
	@dotnet watch --project src/WebSite/WebSite.csproj run --launch-profile http