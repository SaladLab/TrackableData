@echo off

pushd %~dp0

SET PACKAGEPATH=.\packages\
SET NUGET=.\tools\nuget\NuGet.exe
SET NUGETOPTIONS=-ConfigFile .\tools\nuget\NuGet.Config -OutputDirectory %PACKAGEPATH% -ExcludeVersion

IF NOT EXIST %PACKAGEPATH%FAKE\Ver_4.23.0 (
  %NUGET% install FAKE -Version 4.23.0 %NUGETOPTIONS%
  COPY NUL %PACKAGEPATH%FAKE\Ver_4.23.0
)

IF NOT EXIST %PACKAGEPATH%FAKE.BuildLib\Ver_0.1.4 (
  %NUGET% install FAKE.BuildLib -Version 0.1.4 %NUGETOPTIONS%
  COPY NUL %PACKAGEPATH%FAKE.BuildLib\Ver_0.1.4
)

set encoding=utf-8
"%PACKAGEPATH%FAKE\tools\FAKE.exe" build.fsx %*

popd
