FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#FROM registry.gitlab.nvg.ru/ezs/bonusservice:build AS build
ARG APPVERSION
ENV APPVERSION=$APPVERSION
WORKDIR /app
COPY NuGet.Config restore-all.sh */*.csproj .
RUN \
    --mount=type=cache,target=/root/.nuget \
    chmod +x restore-all.sh && ./restore-all.sh
COPY . /app
# RUN \
#     --mount=type=cache,target=/root/.nuget \
#     dotnet test CLMPS
RUN \
    --mount=type=cache,target=/root/.nuget \
    dotnet publish BonusService -c Release -o build -p:Product="BonusService" -p:AssemblyTitle="Sitronics. BonusService." \
   -p:InformationalVersion=$APPVERSION -p:FileVersion=$APPVERSION
FROM mcr.microsoft.com/dotnet/aspnet:7.0
ENV ASPNETCORE_URLS=http://+:80
RUN apt update -y && \
      apt install -y --no-install-recommends net-tools && \
      rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/build /app/
EXPOSE 5023
# HEALTHCHECK --interval=15s --timeout=15s --retries=3 \
#     CMD netstat -an | grep 9098 > /dev/null; if [ 0 != $? ]; then exit 1; fi;
CMD ["dotnet","BonusService.dll"]
