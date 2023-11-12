## OnionFruit Web Deployment Guide
In order to deploy onionfruit-web a server is required:

- A Windows server with container support enabled
- A Linux server with docker installed 

Additionally, a redis server (with TLS and ACL is recommended) is needed for storing Tor node info

### Worker
The worker task is responsible for generating the assets and populating the redis database used by the server process.

To deploy, create a config file in a persistent directory (in this example, we use `appsettings.json`):

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
  "Worker": {
    // alternative location for sentry DSN
    "Dsn": "",
    
    "Exports": {
      // see below for exports examples
    }
  }
}
```

#### `Worker.Exports`
`Worker.Exports` is a dictionary of locations to export data to.

```json5
{
    // the key used is only used for identification purposes (can be set to what you want)
    "Folder-Export": {
      "Type": "Folder",
      "FolderPath": "OnionFruit-Assets",
      
      // SpecialBasePath is optional - if it is set then the FolderPath will be relative to this directory.
      // see https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Environment.SpecialFolder.cs,192440782c25956f for valid ids
      "SpecialBasePath": "DesktopDirectory"
    },
    "Archive-Upload": {
      "Type": "Archive",

      // upload url to send a PUT request to
      // the file name will be appended to the end of the request url
      "UploadUrl": "https://example.com/assetupload/",
      
      // optional asset .zip prefix (if null it will be set to onionfruit-data)
      "Prefix": "asset-upload",
      
      // optional redis pubsub channel to send a notification informing clients a new archive has been uploaded.
      // if null, no message will be sent.
      "RedisNotificationChannel": ""
    }
}
```

