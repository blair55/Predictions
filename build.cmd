@echo off
REM nuget Install FAKE -OutputDirectory packages -ExcludeVersion
"packages\FAKE\tools\Fake.exe" build.fsx %1
