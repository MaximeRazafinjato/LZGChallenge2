# Dockerfile pour le service de mise à jour LZG Challenge
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copier les fichiers de projet et restaurer les dépendances
COPY *.csproj ./
RUN dotnet restore

# Copier tout le code source et build
COPY . ./
RUN dotnet publish -c Release -o out

# Image runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Créer un utilisateur non-root pour la sécurité
RUN adduser --disabled-password --gecos '' lzgservice && \
    chown -R lzgservice:lzgservice /app

# Copier les fichiers publiés
COPY --from=build-env /app/out .

# Créer le répertoire de logs
RUN mkdir -p /app/logs && chown lzgservice:lzgservice /app/logs

# Changer vers l'utilisateur non-root
USER lzgservice

# Point d'entrée
ENTRYPOINT ["dotnet", "LZGChallenge2.UpdateService.dll"]