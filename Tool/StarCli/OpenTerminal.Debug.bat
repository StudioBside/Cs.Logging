@echo off
pushd "%~dp0"

rem wt -d ".\bin\Debug\net9.0\" "C:\Program Files\PowerShell\7\pwsh.exe"
rem "C:\Program Files\PowerShell\7\pwsh.exe" -WorkingDirectory "$($PWD.Path)\bin\Debug\net9.0\"

rem cd .\bin\Debug\net9.0\
rem star /?
rem pwsh

pwsh -NoExit -Command "Set-Alias starx '.\bin\Debug\net9.0\star.exe'; Write-Host 'starx �� �ӽ� alias ���� �߽��ϴ�.';"


rem �Խ� ����� Ŀ�ǵ�
rem  dotnet publish -p:PublishProfile=Properties/PublishProfiles/PublishMacOs.pubxml
rem  dotnet publish -p:PublishProfile=Properties/PublishProfiles/PublishWindows.pubxml

:exit
popd
@echo on
