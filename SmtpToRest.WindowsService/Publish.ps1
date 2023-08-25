param(
  [string]$BuildVersion = "1.0.0.0"
)

dotnet publish ./SmtpToRest.WindowsService/SmtpToRest.WindowsService.csproj -p:Configuration=Release -p:BuildVersion=$BuildVersion -p:DebugType=None -p:DebugSymbols=false -f net7.0 -o:./SmtpToRest.WindowsService/publish /bl
rm ./SmtpToRest.WindowsService/publish/appsettings.Development.json