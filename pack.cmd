@echo off
pushd "%~dp0"
call build && nuget pack Elmah.Bootstrapper.nuspec -Symbol
popd
