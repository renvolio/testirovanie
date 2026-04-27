
(base) ren@MacBook-Arina Backend.Api.Tests % dotnet test
Восстановление завершено (9,2 с)
  Backend.Api успешно выполнено (25,7 с) → /Users/ren/RiderProjects/testirovanie2/backend/Backend.Api/bin/Debug/net8.0/Backend.Api.dll
  Backend.Api.Tests успешно выполнено (3,8 с) → bin/Debug/net8.0/Backend.Api.Tests.dll
You must install or update .NET to run this application.
App: /Users/ren/RiderProjects/testirovanie2/tests/Backend.Api.Tests/bin/Debug/net8.0/testhost.dll
Architecture: x64
Framework: 'Microsoft.NETCore.App', version '8.0.0' (x64)
.NET location: /usr/local/share/dotnet/
The following frameworks were found:
  6.0.36 at [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
  9.0.1 at [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
Learn more:
https://aka.ms/dotnet/app-launch-failed
To install missing framework, download:
https://aka.ms/dotnet-core-applaunch?framework=Microsoft.NETCore.App&framework_version=8.0.0&arch=x64&rid=osx-x64&os=osx.12
Процесс testhost для источника (-ов) /Users/ren/RiderProjects/testirovanie2/tests/Backend.Api.Tests/bin/Debug/net8.0/Backend.Api.Tests.dll завершился с ошибкой. You must install or update .NET to run this application.
App: /Users/ren/RiderProjects/testirovanie2/tests/Backend.Api.Tests/bin/Debug/net8.0/testhost.dll
Architecture: x64
Framework: 'Microsoft.NETCore.App', version '8.0.0' (x64)
.NET location: /usr/local/share/dotnet/
The following frameworks were found:
  6.0.36 at [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
  9.0.1 at [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
Learn more:
https://aka.ms/dotnet/app-launch-failed
To install missing framework, download:
https://aka.ms/dotnet-core-applaunch?framework=Microsoft.NETCore.App&framework_version=8.0.0&arch=x64&rid=osx-x64&os=osx.12
Дополнительные сведения см. в журналах диагностики.
  Backend.Api.Tests (тест) сбой с ошибками (1) (4,6 с)
    /Users/ren/RiderProjects/testirovanie2/tests/Backend.Api.Tests/bin/Debug/net8.0/Backend.Api.Tests.dll : error TESTRUNABORT: Тестовый запуск прерван.

Сборка сбой с ошибками (1) через 47,2 с
(base) ren@MacBook-Arina Backend.Api.Tests % 

 а теперь вот такая проблема, после того как ты исправил:
 (base) ren@MacBook-Arina Backend.Api.Tests % dotnet test
     /Users/ren/RiderProjects/testirovanie2/tests/Backend.Api.Tests/Backend.Api.Tests.csproj : error NU1201: Проект Backend.Api несовместим с net6.0 (.NETCoreApp,Version=v6.0). Проект Backend.Api поддерживает: net8.0 (.NETCoreApp,Version=v8.0)
 
 Восстановление сбой с ошибками (1) через 4,4 с
 (base) ren@MacBook-Arina Backend.Api.Tests % 
