rem sqp_481525aa8e6a240dafb1309a41a3bbff321c0367

dotnet sonarscanner begin /k:"TLNG_PNDS_Backend_Test" /d:sonar.host.url="https://wrosqe01.gaz-system.pl"  /d:sonar.token="sqp_c3a68b642254e11ffbd1bcfb481d0280d6c961d7" /d:sonar.exclusions=appsettings.json /d:sonar.cpd.exclusions=**/*.cs /d:sonar.coverage.exclusions=**/*.cs

dotnet build 

dotnet sonarscanner end /d:sonar.token="sqp_c3a68b642254e11ffbd1bcfb481d0280d6c961d7"                                    