# SmtpToRestService
Simple .NET Core Windows Service converting "e-mails" to REST API calls.

I created this service because I needed to trigger some REST API services in my home automation system when certain types of motion events happened on my CCTV cameras. Although my cameras are [ONVIF](https://www.onvif.org/) compliant, they do no not expose these motion events via the [ONVIF Profile S](https://www.onvif.org/profiles/profile-s/), and thus are unavailable in my home automation system integration.

My workaround for this problem was to configure the cameras to "send an e-mail" when the desired motion events occurred, but instead of using my normal SMTP server to send the email, I configured the cameras to use the SMTP service hosted by the code in this repository instead.

<br />

###### Disclaimer
Currently, the code attempts to convert all e-mail requests to REST API calls, but it would be a relatively simple task to simply forward certain e-mails to an "actual" SMTP server if needed. Furthermore, the current code cannot use any of the information from the e-mail in the actual REST API call, but this could also be added relatively simple by replacing some placeholders in the configuration with corresponding values from the e-mail (eg. "sender address", "recipient", "subject", etc.).

<br />

## Configuration
The sample configuration below illustrates the current possibilites.

```json
{
  "apiToken": "<place your API token here if needed>",
  "endpoint": "https://jsonplaceholder.typicode.com/",
  "httpMethod": "GET",
  "mappings": [
    {
      "key": "list.posts@somedomain.com",
      "service": "posts"
    },
    {
      "key": "add.post@somedomain.com",
      "customHttpMethod": "POST",
      "service": "posts",
      "jsonPostData": {
        "title": "Test post",
        "body": "Test post body",
        "userId": 1
      }
    }
    {
      "key": "add.get@somedomain.com",
      "customApiToken": "eyJ0eXAiOiJKV1QLKiFhbGciOiJIUzI1NiJ9.eyJpc3MieJoLYjY0ZTZkMThh...<cutoff>"
      "customEndpoint": "https://somerestapi.com/",
      "service": "posts",
      "queryString": "title=Test+post&body=Test+post+body&userId=1"
      }
    }
  ]
}
```
<br />

| Property | Description |
| --- | --- |
|apiToken|<b>Optional</b><br />Defines the API token used for mappings (unless overridden in the mapping). Should be set if the REST service requires you to provide an API key.|
|endpoint|<b>Required - if "customEndpoint" not set on mapping</b><br />Defines the common endpoint used for mappings (unless overridden in the mapping).|
|httpMethod|<b>Optional - defaults to "GET"</b><br />Defines the common HTTP method to use for mappings (unless overridden in the mapping).|
|mappings|<b>Optional (but boring service if omitted)</b><br />Defines a list of mappings (see below)|

<br />

#### Mapping
A mapping is what the services uses to convert an e-mail into a REST API call. If a mapping is found where the <b>key</b> of the mapping matches the <b>to address</b> of the e-mail, the REST API call defined by the mapping is invoked.

| Property | Description |
| --- | --- |
|customApiToken|<b>Optional</b><br />API token used for this particular mapping. Should be set if the REST service requires you to provide an API key.|
|customEndpoint|<b>Optional</b><br />Defines the endpoint for this particular mapping.|
|customHttpMethod|<b>Optional</b><br />Defines the HTTP method for this particular mapping.|
|service|<b>Required</b><br />Defines the path appended to the enpoint to complete the URL.
|queryString|<b>Optional</b><br />Defines a query string to be appended to the URL (used in GET requests).|
|jsonPostData|<b>Optional</b><br />Defines a JSON object to be set as the body of the request (used in POST requests).|

<br />

## Credits
In order to minimize my workload, I used the following packages to host and SMTP server and read MIME messages:

| Package | Author | Usage |
| --- | --- | --- |
|[SmtpServer](https://www.nuget.org/packages/SmtpServer/) | [Cain O'Sullivan](https://github.com/cosullivan) | I use this package to self-host and SMTP server. |
|[MimeKitLite](https://www.nuget.org/packages/MimeKitLite/) | [Jeffrey Stedfast](https://github.com/jstedfast) | I use this package to convert a byte-stream into a strongly typed MIME object.|