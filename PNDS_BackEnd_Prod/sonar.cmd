rem sqp_481525aa8e6a240dafb1309a41a3bbff321c0367

dotnet sonarscanner begin /k:"TLNGPNDS_tlng_pnds_backend_03cd3243-e183-4009-a098-b35135cf3c9d" /d:sonar.host.url="https://wrosqe01.gaz-system.pl"  /d:sonar.token="sqp_481525aa8e6a240dafb1309a41a3bbff321c0367" /d:sonar.exclusions=appsettings.json

dotnet build 

dotnet sonarscanner end /d:sonar.token="sqp_481525aa8e6a240dafb1309a41a3bbff321c0367"                                    