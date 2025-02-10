FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./../shoe-grab-user/*.csproj ./shoe-grab-user/
RUN dotnet restore ./shoe-grab-user/*.csproj
COPY ./../shoe-grab-user/ ./shoe-grab-user/
RUN dotnet publish ./shoe-grab-user/*.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /out .
EXPOSE 10001
EXPOSE 10002
ENV DB_CONNECTION_STRING="Host=your-db-vm-ip;Port=5432;Database=your-db-name;Username=your-username;Password=your-password;"
ENTRYPOINT ["dotnet", "shoe-grab-user.dll"]
