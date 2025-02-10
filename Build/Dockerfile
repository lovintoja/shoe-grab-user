FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./../shoe-grab-user/*.csproj ./shoe-grab-user/
COPY ./../shoe-grab-common/ ./shoe-grab-common/
COPY ./../shoe-grab-user/ ./shoe-grab-user/

RUN dotnet restore ./shoe-grab-user/*.csproj
RUN dotnet publish ./shoe-grab-user/*.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /out .
EXPOSE 10001
EXPOSE 10002
ENV DB_CONNECTION_STRING="Host=192.168.0.121;Port=30002;Database=db2;Username=admin;Password=admin"
ENTRYPOINT ["dotnet", "ShoeGrabUserManagement.dll"]
