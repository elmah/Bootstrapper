@echo off
setlocal
pushd "%~dp0"
if "%PROCESSOR_ARCHITECTURE%"=="x86" set MSBUILD=%ProgramFiles%
if defined ProgramFiles(x86) set MSBUILD=%ProgramFiles(x86)%
set MSBUILD=%MSBUILD%\MSBuild\14.0\bin\msbuild
if not exist "%MSBUILD%" (
    echo Microsoft Build Tools 2015 does not appear to be installed on this
    echo machine, which is required to build the solution. You can install
    echo it from the URL below and then try building again:
    echo https://www.microsoft.com/en-us/download/detailscd .aspx?id=48159
    exit /b 1
)
nuget restore              ^
 && call :build 40 Debug   ^
 && call :build 40 Release ^
 && call :build 45 Debug   ^
 && call :build 45 Release
popd
goto :EOF

:build
setlocal
"%MSBUILD%" /p:Configuration=NET%1-%2 /v:m %3 %4 %5 %6 %7 %8 %9
goto :EOF
