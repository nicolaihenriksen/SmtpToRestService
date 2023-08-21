dotnet publish ./SmtpToRest.WindowsService/SmtpToRest.WindowsService.csproj -p:Configuration=Release -p:DebugType=None -p:DebugSymbols=false -o:./SmtpToRest.WindowsService/publish
rm ./SmtpToRest.WindowsService/publish/appsettings.Development.json
Compress-Archive -Path ./SmtpToRest.WindowsService/publish/* -DestinationPath ./SmtpToRest.WindowsService/publish/SmtpToRest.WindowsService.zip