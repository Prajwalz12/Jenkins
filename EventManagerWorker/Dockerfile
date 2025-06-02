FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as build

WORKDIR /build

COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o /published EventManagerWorker.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
# Change timezone to local time
RUN apk add --no-cache tzdata
ENV TZ Asia/Kolkata
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

WORKDIR /app

COPY --from=build /published .
ENTRYPOINT [ "dotnet", "EventManagerWorker.dll" ]
