@echo off
setlocal
pushd "%~dp0"
nuget restore              ^
 && call :build 40 Debug   ^
 && call :build 40 Release ^
 && call :build 45 Debug   ^
 && call :build 45 Release
popd
goto :EOF

:build
msbuild /p:Configuration=NET%1-%2 /v:m %3 %4 %5 %6 %7 %8 %9
goto :EOF
