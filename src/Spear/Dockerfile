ARG DOTNET_VERSION=7.0
ARG USER_UID=1000
ARG USER_GID=${USER_UID}

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build

WORKDIR /src
COPY --chown=spear:spear src/Spear/Spear.csproj src/Spear/
RUN dotnet restore src/Spear/Spear.csproj
COPY --chown=spear:spear . .

WORKDIR /src/src/Spear
RUN dotnet build Spear.csproj --no-restore -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Spear.csproj --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS final
ARG USER_UID
ARG USER_GID

RUN groupadd --gid ${USER_GID} spear \
    && useradd --uid ${USER_UID} --gid spear --shell /usr/sbin/nologin --create-home spear \
    && mkdir /app \
    && chown spear:spear /app
USER spear

WORKDIR /app
COPY --from=publish --chown=spear:spear /app/publish .
ENTRYPOINT ["dotnet", "Spear.dll"]
