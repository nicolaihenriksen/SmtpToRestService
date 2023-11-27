# SmtpToRest
[![Build](https://github.com/nicolaihenriksen/SmtpToRestService/actions/workflows/build_workflow.yml/badge.svg?branch=master)](https://github.com/nicolaihenriksen/SmtpToRestService/actions/workflows/build_workflow.yml)
[![NuGet-Themes](https://img.shields.io/badge/SmtpToRest-Nuget-blue)](https://www.nuget.org/packages/SmtpToRest/)

Simple .NET application converting "e-mails" to REST API calls. Can be executed as a Docker container, as a Windows Service or self-hosted using the standard .NET host builder pattern.

I created this application because I needed to trigger some REST API services in my home automation system when certain types of motion events happened on my CCTV cameras. Although my cameras are [ONVIF](https://www.onvif.org/) compliant, they do no not expose these motion events via the [ONVIF Profile S](https://www.onvif.org/profiles/profile-s/), and thus are unavailable in my home automation system integration.

My workaround for this problem was to configure the cameras to "send an e-mail" when the desired motion events occurred, but instead of using my normal SMTP server to send the email, I configured the cameras to use the SMTP service hosted by the code in this repository instead. This service then converts those "e-mails" into the desired REST API calls I need in my home automation system.

The project has since then become a hobby project where I am experimenting with adding Docker support, CI/CD pipelines and other fun stuff.

### Disclaimer
Currently, the code attempts to convert all e-mail requests to REST API calls and optionally forwards them using an SMTP relay. The current code cannot use any of the information from the e-mail in the actual REST API call, but this could be added relatively simple by replacing some placeholders in the configuration with corresponding values from the e-mail (eg. "sender address", "recipient", "subject", etc.).

## Configuration
The sample configuration below illustrates the current possibilites.

```json
{
  "smtpHost": "localhost",
  "smtpPorts": [ 25, 587 ],
  "apiToken": "<place your API token here if needed>",
  "endpoint": "https://jsonplaceholder.typicode.com/",
  "httpMethod": "GET",
  "smtpRelay": {
    "enabled": true,
    "host": "smtp.gmail.com",
    "port": 587,
    "authenticate": true,
    "username": "myprimaryuser@gmail.com",
    "password": "mypassword"
  },
  "mappings": [
    {
      "key": "list.posts@somedomain.com",
      "service": "posts",
      "smtpRelay": {
        "enabled": false
      }
    },
    {
      "key": "add.post@somedomain.com",
      "customHttpMethod": "POST",
      "service": "posts",
      "content": {
        "title": "Test post",
        "body": "Test post body sent from $(from)",
        "userId": 1
      }
    },
    {
      "key": "add.get@somedomain.com",
      "customApiToken": "eyJ0eXAiOiJKV1QLKiFhbGciOiJIUzI1NiJ9.eyJpc3MieJoLYjY0ZTZkMThh...<cutoff>",
      "customEndpoint": "https://somerestapi.com/",
      "service": "posts",
      "queryString": "title=Test+post&body=Test+post+body&userId=1",
      "smtpRelay": {
        "enabled": true,
        "host": "192.168.1.100",
        "port": 25,
        "authenticate": false
      }
    }
  ]
}
```
<br />

| Property | Description |
| --- | --- |
|smtpHost|<b>Optional</b><br />Defines the host (endpoint) where the SMTP server will be listening. Defaults to "localhost".</b><br /><b>Note</b> Does not support runtime updates|
|smtpPorts|<b>Optional</b><br />Defines ports the SMTP server will be listening on. Defaults to ports 25 and 587.</b><br /><b>Note</b> Does not support runtime updates|
|apiToken|<b>Optional</b><br />Defines the API token used for mappings (unless overridden in the mapping). Should be set if the REST service requires you to provide an API key.|
|endpoint|<b>Required - if "customEndpoint" not set on mapping</b><br />Defines the common endpoint used for mappings (unless overridden in the mapping).|
|httpMethod|<b>Optional - defaults to "GET"</b><br />Defines the common HTTP method to use for mappings (unless overridden in the mapping).|
|mappings|<b>Optional (but boring service if omitted)</b><br />Defines a list of mappings (see below).|
|smtpRelay|<b>Optional</b><br />Defines the shared configuration for SMTP relay used to send/relay the e-mail (see below).|

<br />

### Mapping
A mapping is what the services uses to convert an e-mail into a REST API call. If a mapping is found where the <b>key</b> of the mapping matches the <b>to address</b> of the e-mail, the REST API call defined by the mapping is invoked.

| Property | Description |
| --- | --- |
|customApiToken|<b>Optional</b><br />API token used for this particular mapping. Should be set if the REST service requires you to provide an API key.|
|customEndpoint|<b>Optional</b><br />Defines the endpoint for this particular mapping.|
|customHttpMethod|<b>Optional</b><br />Defines the HTTP method for this particular mapping.|
|service|<b>Required</b><br />Defines the path appended to the enpoint to complete the URL.
|queryString|<b>Optional</b><br />Defines a query string to be appended to the URL (used in GET requests).|
|content|<b>Optional</b><br />Defines the content (often times a JSON object) to be set as the content of the request (often used in POST requests).|
|smtpRelay|<b>Optional</b><br />Defines the configuration for mapping-specific SMTP relay used to send/relay the e-mail (see below).|

<br />

### SmtpRelay
The SMTP relay is used to forward the e-mail to another SMTP server. This is useful if you want to forward the e-mail to another service/recipient.
All values are optional and will fall-back to the default from the configuration if omitted in the mapping.

| Property | Description |
| --- | --- |
|host|<b>Optional</b><br />Defines the hostname or IP address of the SMTP server.|
|port|<b>Optional</b><br />Defines the port used to connect to the SMTP server.|
|authenticate|<b>Optional</b><br />Defines whether or not authentication should be used when connecting to the SMTP server.|
|username|<b>Optional</b><br />Defines the username to use for authentication.|
|password|<b>Optional</b><br />Defines the password to use for authentication.|

<br />

### Token replacements
The following tokens can be used in the configuration and will be replaced with the corresponding values from the e-mail:
| Token | Description | 
| --- | --- |
|$(from)|The The (first) sender address|
|$(to)|The (first) recipient address|
|$(body)|The (string) body of the e-mail|

Each token can be applied as shown above to simply extract the token value in its entirety, or alternatively the token syntax below can be used to narrow down the selection.
| Name | Syntax | Example | Description | 
| --- | --- | --- | --- |
|Substring|$(&lt;token&gt;){&lt;startIndex&gt;}|$(body){15}|Will take the substring of the token starting at the provided &lt;startIndex&gt;.|
|Substring and length|$(&lt;token&gt;){&lt;startIndex&gt;,&lt;length&gt;}|$(body){15,10}|Will take the substring of the token starting at the provided &lt;startIndex&gt; and &lt;length&gt; characters forward.|
|Substring from index of string|$(&lt;token&gt;){[&lt;stringToFind&gt;]}|$(body){[hello]}|Will take the substring of the token starting at index of the provided &lt;stringToFind&gt;.|
|Substring from index of string with offset|$(&lt;token&gt;){[&lt;stringToFind&gt;]+&lt;offset&gt;}|$(body){[hello]+10}|Will take the substring of the token starting at index of the provided &lt;stringToFind&gt; using the provided offset. Note the offset can also be negative.|
|Substring from index of string and length|$(&lt;token&gt;){[&lt;stringToFind&gt;],&lt;length&gt;}|$(body){[hello],40}|Will take the substring of the token starting at index of the provided &lt;stringToFind&gt; and &lt;length&gt; characters forward.|
|Substring from index of string to index of another string|$(&lt;token&gt;){[&lt;stringToFind1&gt;],[&lt;stringToFind2&gt;]}|$(body){[hello],[world]}|Will take the substring of the token starting at index of the provided &lt;stringToFind1&gt; up until the inded of the provided &lt;stringToFind2&gt;.|

Note that the list above is not exhaustive. It demonstrates the different elements of the syntax that can be applied, and these can be mixed and matched for the desirable "from" and "length"/"to" combination.

<br />

## Install as Docker container
See [SmtpToRest.Docker](SmtpToRest.Docker/README.md) for more information.

## Install as Windows Service
See [SmtpToRest.WindowsService](SmtpToRest.WindowsService/README.md) for more information.

## Self-host (e.g. using GenericHost)
See [SmtpToRest](SmtpToRest/README.md) for more information.

<br />

## Credits
In order to minimize my workload, I used the following [Nuget](https://www.nuget.org/) packages:

| Package | Author | Usage |
| --- | --- | --- |
|[SmtpServer](https://www.nuget.org/packages/SmtpServer/) | [Cain O'Sullivan](https://github.com/cosullivan) | I use this package to self-host an SMTP server. |
|[MailKit](https://www.nuget.org/packages/MailKit/) | [Jeffrey Stedfast](https://github.com/jstedfast) | I use this package to convert a byte-stream into a strongly typed MIME object and as an SMTP relay to forward e-mail messages.|
