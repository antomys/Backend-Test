FROM mcr.microsoft.com/dotnet/sdk:10.0.201-alpine3.23 AS build

ARG NUGET_USER
ARG NUGET_PASS
ARG CI_API_V4_URL

COPY ["config/coverlet/coverlet.settings.xml", "./"]
COPY ["Directory.Build.props", "./"]

COPY src/. ./src/
COPY test/. ./tests

RUN dotnet restore "test/BackendTest.Integration.Tests/BackendTest.Integration.Tests.csproj" --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly
RUN dotnet build "test/BackendTest.Integration.Tests/BackendTest.Integration.Tests.csproj" --no-restore --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly

FROM build AS testrunner

WORKDIR test/BackendTest.Integration.Tests

CMD ["dotnet", "test", "--no-restore", "--collect:", "XPlat Code Coverage", "--results-directory", "../../../coverage", "--settings", "../../../coverlet.settings.xml"]
