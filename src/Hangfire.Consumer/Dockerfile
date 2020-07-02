FROM mcr.microsoft.com/dotnet/core/sdk:3.1.401-alpine3.12
WORKDIR /app
COPY [".", "."]
ENTRYPOINT ["dotnet", "Hangfire.Consumer.dll"]
