FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app
EXPOSE 5000

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# spin up the runtime
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "retns.homework.api.dll"]