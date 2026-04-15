.PHONY: build browser

build:
	dotnet build edu.sln

browser:
	dotnet watch --project src/WebSite/WebSite.csproj run --launch-profile http