# Build context: repository root.  docker build -t songthesame-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .editorconfig SongTheSame.slnx ./
COPY src/SongTheSame.Domain/*.csproj src/SongTheSame.Domain/
COPY src/SongTheSame.Application/*.csproj src/SongTheSame.Application/
COPY src/SongTheSame.Infrastructure/*.csproj src/SongTheSame.Infrastructure/
COPY src/SongTheSame.Audio/*.csproj src/SongTheSame.Audio/
COPY src/SongTheSame.Api/*.csproj src/SongTheSame.Api/
RUN dotnet restore src/SongTheSame.Api

COPY src/ src/
# OpenAPI document generation is only needed for the web client build, not for the image.
RUN dotnet publish src/SongTheSame.Api -c Release -o /app --no-restore -p:OpenApiGenerateDocuments=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SongTheSame.Api.dll"]
