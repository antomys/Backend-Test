FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0.5-alpine3.23 AS base
WORKDIR /app

# Alpine images default to invariant globalization mode, which breaks culture-aware
# operations (e.g. DateOnly/number parsing) at runtime. Install ICU and disable it.
RUN apk add --no-cache icu-libs

ARG TARGETARCH
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=443
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.201-alpine3.23 AS build
WORKDIR /src

COPY ["Directory.Build.props","./"]

COPY src/. ./src/

RUN dotnet restore "src/BackendTest.Host/BackendTest.Host.csproj" --nologo -v q -a "$TARGETARCH" --property WarningLevel=0 /clp:ErrorsOnly

WORKDIR "src/BackendTest.Host/"

RUN dotnet build "BackendTest.Host.csproj" -a "$TARGETARCH" -c Release --no-restore -o /app/build --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly

FROM build AS publish
RUN dotnet publish "BackendTest.Host.csproj" -a "$TARGETARCH" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "BackendTest.Host.dll"]
