FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

COPY DigitaizerDart.sln .
COPY DigitaizerDart.WebApp/*.csproj ./DigitaizerDart.WebApp/
RUN dotnet restore

COPY DigitaizerDart.WebApp/. ./DigitaizerDart.WebApp/
WORKDIR /app/DigitaizerDart.WebApp
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/DigitaizerDart.WebApp/out ./
ENTRYPOINT ["dotnet", "DigitaizerDart.WebApp.dll"]

