# Teg-Demo-WebApi
Api For Teg Demo

This Api project is created to consume event source located at https://teg-coding-challenge.s3.ap-southeast-2.amazonaws.com/events/event-data.json.The source is a json file which contains information about Events and the venues where the events are held. 

This application is created using .Net Core 7 webapi template and c# as the programming language. The application uses a service layer that interacts with the json file in the aws s3 bucket. This application uses a rolling log using serilog package. 

The application validates the data with the schema and also is integrated with polly nuget package that allows it to retry the https call.

Use visual studio to restore packages, build and run the application. The application uses swagger endpoint in dev environment.

If you are using command line, use the followig commands.

## Restore
dotnet restore

## Build 
dotnet build

## Execute
dotnet run.

The application is auto deployed when you check into the main branch. This is accomplished using gitflow.