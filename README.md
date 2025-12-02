# Yet Another Sample App (YASA)

![Build](https://github.com/ravendb/sample-blueprint/actions/workflows/build.yml/badge.svg)

## Overview

A sample application providing a simple online shop expierience. It leverages [RavenDB](https://ravendb.net) database as its core database.

<img width="950" height="590" alt="screenshot" src="https://github.com/user-attachments/assets/108cbb63-e937-4b40-9cb0-28123fc93125" />


## Live Demo

A live hosted version of this application can be found here:

**[https://yasa.samples.ravendb.net](https://yasa.samples.ravendb.net)**

Please bear in mind, this application, for sake of simplicity of the deployment, is deployed in XYZ region. This can impact the latency you perceive. (remove if not applicable).

We do clean the environment from time to time.

## Features used

The following RavenDB features are used to build the application:

1. [Vector Search](https://docs.ravendb.net/7.1/ai-integration/vector-search/ravendb-as-vector-database) - RavenDB has a built-in vector database 
1. [Document Refresh](https://docs.ravendb.net/7.1/studio/database/settings/document-refresh) - a scheduled update of selected documents
1. [Azure Storage Queues ETL](https://docs.ravendb.net/7.1/server/ongoing-tasks/etl/queue-etl/azure-queue) - Azure Storage Queues integration

## Technologies

The following technogies were used to build this application:

1. RavenDB 7.1
1. .NET 8
1. Node.js 22
1. ASP.NET Core 8
1. Angular 14

## Run locally

If you want to run the application locally, please follow the steps:

1. Check out the GIT repository
1. Install prerequisites:
   1. [.NET 8.x](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
   1. [Node.js 22.x](https://nodejs.org/en/download)
1. Get the app running by ...

## Community & Support

If you spot a bug, have an idea or a question, please let us know by rasing an issue or creating a pull request. 

We do use a [Discord server](https://discord.gg/ravendb). If you have any doubts, don't hesistate to reach out!

## Contributing

We encourage you to contribute! Please read our [CONTRIBUTING](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed with the [MIT license](LICENSE).
