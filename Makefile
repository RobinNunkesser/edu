.PHONY: build browser browser-watch serve-stop browser-clean browser-fresh clean-dev-artifacts

PORT ?= 5155
APP_URL := http://127.0.0.1:$(PORT)
STATIC_WEB_ASSETS_CACHE := src/WebSite/obj/Debug/net8.0/rpswa.dswa.cache.json

build:
	dotnet build edu.sln

browser:
	@if lsof -tiTCP:$(PORT) -sTCP:LISTEN >/dev/null 2>&1; then \
		echo "Dev-Server laeuft bereits auf $(APP_URL)"; \
		open $(APP_URL); \
	else \
		dotnet run --project src/WebSite/WebSite.csproj --launch-profile http --urls $(APP_URL); \
	fi

browser-watch:
	@dotnet watch --project src/WebSite/WebSite.csproj run --launch-profile http -- --urls $(APP_URL)

serve-stop:
	@port_pids=$$(lsof -tiTCP:$(PORT) -sTCP:LISTEN 2>/dev/null || true); \
	watch_pids=$$(pgrep -f 'src/WebSite/WebSite.csproj run --launch-profile http' || true); \
	run_pids=$$(pgrep -f 'dotnet run .*--project src/WebSite/WebSite.csproj' || true); \
	devserver_pids=$$(pgrep -f 'blazor-devserver\.dll .*WebSite\.dll' || true); \
	pids=$$(printf '%s\n%s\n%s\n%s\n' "$$port_pids" "$$watch_pids" "$$run_pids" "$$devserver_pids" | awk 'NF' | sort -u | tr '\n' ' '); \
	if [ -n "$$pids" ]; then \
		echo "Stoppe lokale WebSite-Instanzen (PID $$pids)"; \
		kill $$pids; \
		attempts=0; \
		while lsof -tiTCP:$(PORT) -sTCP:LISTEN >/dev/null 2>&1 \
			|| pgrep -f 'src/WebSite/WebSite.csproj run --launch-profile http' >/dev/null 2>&1 \
			|| pgrep -f 'dotnet run .*--project src/WebSite/WebSite.csproj' >/dev/null 2>&1 \
			|| pgrep -f 'blazor-devserver\.dll .*WebSite\.dll' >/dev/null 2>&1 \
			|| lsof "$(STATIC_WEB_ASSETS_CACHE)" >/dev/null 2>&1; do \
			attempts=$$((attempts + 1)); \
			if [ $$attempts -ge 50 ]; then \
				break; \
			fi; \
			perl -e 'select undef, undef, undef, 0.1'; \
		done; \
		remaining_pids=$$(printf '%s\n%s\n%s\n%s\n' \
			"$$(lsof -tiTCP:$(PORT) -sTCP:LISTEN 2>/dev/null || true)" \
			"$$(pgrep -f 'src/WebSite/WebSite.csproj run --launch-profile http' || true)" \
			"$$(pgrep -f 'dotnet run .*--project src/WebSite/WebSite.csproj' || true)" \
			"$$(pgrep -f 'blazor-devserver\.dll .*WebSite\.dll' || true)" | awk 'NF' | sort -u | tr '\n' ' '); \
		if [ -n "$$remaining_pids" ]; then \
			echo "Erzwinge Stop fuer verbleibende Listener (PID $$remaining_pids)"; \
			kill -9 $$remaining_pids; \
			attempts=0; \
			while lsof -tiTCP:$(PORT) -sTCP:LISTEN >/dev/null 2>&1 \
				|| pgrep -f 'src/WebSite/WebSite.csproj run --launch-profile http' >/dev/null 2>&1 \
				|| pgrep -f 'dotnet run .*--project src/WebSite/WebSite.csproj' >/dev/null 2>&1 \
				|| pgrep -f 'blazor-devserver\.dll .*WebSite\.dll' >/dev/null 2>&1 \
				|| lsof "$(STATIC_WEB_ASSETS_CACHE)" >/dev/null 2>&1; do \
				attempts=$$((attempts + 1)); \
				if [ $$attempts -ge 50 ]; then \
					break; \
				fi; \
				perl -e 'select undef, undef, undef, 0.1'; \
			done; \
		fi; \
	else \
		echo "Keine lokale WebSite-Instanz aktiv"; \
	fi

browser-clean:
	@$(MAKE) serve-stop
	@$(MAKE) clean-dev-artifacts
	@echo "Starte lokale WebSite mit frischen Build-Artefakten auf $(APP_URL)"
	@dotnet run --project src/WebSite/WebSite.csproj --launch-profile http --urls $(APP_URL)

browser-fresh:
	@$(MAKE) browser-clean

clean-dev-artifacts:
	@find src tools -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +