# SmtpToRest.WindowsService

SmtpToRest.WindowsService.exe is a stand-alone application that can be run either directly in the command line, or as a Windows service.

The application expects to find a configuration file (configuration.json) located next to the EXE file.


## Install as Windows Service
In order to install this as a windows service, open up a command prompt (in Administrator mode) and issue the following command:
```
sc create smtptorestservice binPath="<full-path-to-deployment-dir>\SmtpToRest.WindowsService.exe"
```

Remember to replace **&lt;full-path-to-deployment-dir&gt;** with the actual path where you have deployed the package.

**NOTE:** The [sc create command](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/sc-create) allows for a number of parameters if you want to tweak the display name, description, startup, dependencies, etc. of the service.