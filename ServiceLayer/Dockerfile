FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish TaxCalcService -c Release -o TaxCalcService/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/TaxCalcService/out .
ENTRYPOINT ["dotnet", "TaxCalcService.dll"]