@echo off
nuget Install FAKE -OutputDirectory packages -ExcludeVersion
"packages\FAKE\tools\Fake.exe" build.fsx