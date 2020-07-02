FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.1-alpine

EXPOSE 80
EXPOSE 443

WORKDIR /app
COPY [".", "."]
ENTRYPOINT ["dotnet", "Keda.ExternalScaler.Hangfire.dll"]