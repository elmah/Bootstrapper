@echo off
pushd "%~dp0"
call :main %*
popd
goto :EOF

:main
setlocal
if not "%1"=="" set NUGET_PACK_ARGS=%NUGET_PACK_ARGS% -Properties VersionSuffix=-%1
call build && nuget pack Elmah.Bootstrapper.nuspec %NUGET_PACK_ARGS%
goto :EOF
