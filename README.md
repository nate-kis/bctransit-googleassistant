# Next Bus (Google Assistant app)

This is a simple application (azure function app) that contains endpoints that a google assistant application calls to determine what the next bus comes for a stop. It works for two locations, home and work and settings to determine what the stop number and bus code is. 

It is hooked up with the google assistant api through dialogflow. 

#### BC Transit API Info
[https://nextride.victoria.bctransit.com/](https://nextride.victoria.bctransit.com/)

#### Google assistant urls
Here are some links to help with the google assistant. Included are some tutorial, api documentation, and sites to setup the project
- [https://cloud.google.com/dialogflow/docs/fulfillment-how#webhook_response](https://cloud.google.com/dialogflow/docs/fulfillment-how#webhook_response)
- [https://console.dialogflow.com/api-client/#/login](https://console.dialogflow.com/api-client/#/login)
- [https://developers.google.com/assistant/sdk/guides/service/python/](https://developers.google.com/assistant/sdk/guides/service/python/)
- [https://console.actions.google.com/u/0/?pli=1](https://console.actions.google.com/u/0/?pli=1)

## Configuration

### localsettings.json

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  },
  "Locations": {
    "Home": {
      "Lat": 0,
      "Lng": 0,
      "RouteCode": "",
      "StopId": ""
    },
    "Work": {
      "Lat": 0,
      "Lng": 0,
      "RouteCode": "",
      "StopId": ""
    }
  }
}
```

### Environment Variables

```
Locations:Home:Lat
Locations:Home:Lng
Locations:Home:RouteCode
Locations:Home:StopId

Locations:Work:Lat
Locations:Work:Lng
Locations:Work:RouteCode
Locations:Work:StopId
```

| Setting   | Info        |
|-----------|-------------|
|RouteCode  | route number you are looking for |
|StopId     | this is not the stop code - look at the api's https://nextride.victoria.bctransit.com/api/PredictionData?stopid= url in network tab when you select a stop on a route |
| Lat / Lng | This application assumes the busses are moving east (from home)/west (from work) for the stops to determine the next bus at a stop |