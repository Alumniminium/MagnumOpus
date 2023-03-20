# herstfortress/fxtia:latest

FROM mcr.microsoft.com/dotnet/nightly/sdk:7.0-alpine as build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/published-app --configuration Release


FROM mcr.microsoft.com/dotnet/nightly/runtime:7.0-alpine as runtime
WORKDIR /app
VOLUME [ "/app" ]

COPY --from=build /app/published-app /app

ENTRYPOINT [ "dotnet", "/app/MagnumOpus.dll" ]