# SmtpToRest.WindowsService

TODO: Description

## Install as Windows Service
In order to install this as a windows service, open up a command prompt (in Administrator mode) and issue the following command:
```
sc create smtptorestservice binPath="<full-path-to-deployment-dir>\SmtpToRest.WindowsService.exe"
```

Remember to replace **&lt;full-path-to-deployment-dir&gt;** with the actual path where you have deployed the package.

**NOTE:** The **sc** command allows for a number of parameters if you want to tweak the display name, description, startup, dependencies, etc. of the service.