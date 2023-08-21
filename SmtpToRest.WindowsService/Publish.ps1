dotnet publish SmtpToRest.WindowsService.csproj -p:Configuration=Release -p:DebugType=None -p:DebugSymbols=false -o:./publish
rm ./publish/appsettings.Development.json
Compress-Archive -Path ./publish/* -DestinationPath ./publish/SmtpToRest.WindowsService.zip