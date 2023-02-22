Azure Functions Component Integration Test Framework
===

This project aims to simplify testing Azure Functions by running the fully integrated host while under the unit test framework to orchestrate test suites.


### How It Works

The framework works by running Azure Functions in a similar way as the Azure Functions V4 CLI.  It builds upon the same [Azure WebJobs SDK](https://github.com/Azure/azure-webjobs-sdk) to provide a hosting platform for the [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/) service.  It runs the runtime within any unit testing framework and provides an HttpClient using `Microsoft.AspNetCore.TestHost.TestServer`.

### License

This project is licensed under [the MIT License](LICENSE.txt)