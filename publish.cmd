@ECHO OFF
SET tmp_folder=docs-temp
SET www_folder=%tmp_folder%/wwwroot
SET docs_folder=docs

dotnet clean src/BlazorAppHandlebarsExample -c Release
dotnet publish src/BlazorAppHandlebarsExample -c Release -o %tmp_folder%

rem delete all files except .gitattributes
for /R "%docs_folder%" %%f in (*) do (if not "%%~xf"==".gitattributes" del "%%~f")

rem delete all sub-folders
for /d %%x in (%docs_folder%\*) do @rd /s /q "%%x"

rem copy all from temp-docs into docs folder
robocopy %www_folder% %docs_folder% /E