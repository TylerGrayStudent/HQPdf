FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PdfApi/PdfApi.csproj", "PdfApi/"]
RUN dotnet restore "PdfApi/PdfApi.csproj"
COPY . .
WORKDIR "/src/PdfApi"
RUN dotnet build "PdfApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PdfApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PdfApi.dll"]
