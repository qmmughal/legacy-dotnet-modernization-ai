# Multi-stage build for Meridian.Web. Alternative without a Dockerfile:
#   dotnet publish src/Meridian.Web -c Release /t:PublishContainer
# (uses ContainerFamily=noble-chiseled-extra from the project file).
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet publish src/Meridian.Web/Meridian.Web.csproj -c Release -o /app --no-self-contained

# Chiseled: no shell, no package manager, non-root by default. "-extra" adds ICU and
# time zone data: the pricing display string (en-US currency) and the America/Chicago
# business time zone need them. Plain chiseled images are globalization-invariant.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Meridian.Web.dll"]
