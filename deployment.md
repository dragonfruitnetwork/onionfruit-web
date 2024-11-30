## OnionFruit Web Deployment Guide
In order to deploy onionfruit-web a server is required:

- A Windows server with container support enabled
- A Linux server with docker installed 

Additionally, a redis server (with TLS and ACL is recommended) is required for storing Tor node info and uploaded file version tracking info.

### Worker
The worker task is responsible for generating the assets and populating the redis database used by the server process.
To deploy, create a config file in a persistent directory (in this example, we use `appsettings.json`):

```json5
{
  // logging config
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  // optional sentry.io DSN client key
  "Dsn": "",
  
  "Redis": {
    // set redis connection string here
    // see https://stackexchange.github.io/StackExchange.Redis/Configuration.html#configuration-options for options
    "ConnectionString": "localhost:6379"
  },
  "S3": {
    // replace with your own s3 bucket details (endpoint can be replaced with region if using AWS)
    "BucketName": "onionfruit",
    "ExpireOldAssetsAfter": 30,
    "Endpoint": "",
    "AccessKey": "",
    "SecretKey": ""
  }
}
```

## Server

The server can be deployed in a similar way, but with a slightly different config file:

```json5
{
  // asp.net core logging config
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  // optional sentry.io DSN client key
  "Dsn": "",
  
  "Redis": {
    // set redis connection string here
    // see https://stackexchange.github.io/StackExchange.Redis/Configuration.html#configuration-options for options
    "ConnectionString": "localhost:6379"
  },

  "Server": {
    "UseBuiltInWorker": "true", // whether to use a built-in worker process (defaults to true if not set). if used, s3 is not required.
    "RemoteAssetPublicUrl": "https://onionfruit-assets.dragonfruit.network/{0}" // only required if built-in worker is disabled.
  }
}
```

You can then use compose file to deploy the server:

```yaml
services:
  onionfruit-api:
    container_name: onionfruit-api
    image: dragonfruitdotnet/onionfruit-web:latest # change latest to worker if you want to run the worker separately
    restart: always
    volumes:
      - /path/to/config/dir:/onionfruit-config:ro
    ports:
      - 80:80
    environment:
      CONFIG_FOLDER_PATH: /onionfruit-config
```
