# SmtpToRest.Docker

SmtpToRest can be run in a docker container by pulling the docker image from [Docker Hub](https://hub.docker.com/).
When executing containerized, it is important to map a volume which holds the directory where the `configuration.json`
file is located. An example docker compose file can be found below.

## Docker Compose

```docker
version: '3'

services:
  app:
    image: nicolaihenriksen/smtptorest:latest
    container_name: smtp_to_rest_service
    ports:
      - "8025:25"
      - "8587:587"
    volumes:
      - ./<mapped-config-dir>:/app/config:ro
```

**NOTE:** Remember to replace `<mapped-config-dir>` with an actual directory where the `configuration.json` file is located.