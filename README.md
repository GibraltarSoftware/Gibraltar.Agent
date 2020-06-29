# Loupe Agent for .NET Framework #

The Loupe Agent provides a generic facility for capturing log messages, exceptions, and metrics
from .NET applications.  This repository is for the .NET Framework version of the Loupe Agent (for .NET 2.0 through 4.5).

## How do I use Loupe with my Application? ##

To add Loupe to your application we recommend referencing adding the [Gibraltar.Agent package](https://www.nuget.org/packages/Gibraltar.Agent/).
Then you can also add agents for specific .NET Framework extensions that you use like ASP.NET MVC/WebAPI and Entity Framework.

For complete guidance, see [Getting Started Guide](https://doc.onloupe.com/#GettingStarted_Introduction.html)
from the main Loupe documentation for how to get moving.

You can use the [free Loupe Desktop viewer](https://onloupe.com/local-logging/free-net-log-viewer) to
view logs & analyze metrics for your application or use [Loupe Cloud-Hosted](https://onloupe.com/) to add centralized logging,
alerting, and error analysis to your application.

## What's In This Repository ##

This is the repository for the Loupe Agent for .NET Core.
The following NuGet packages live here:

* Gibraltar.Agent: The primary API to use for logging & metrics.


## How To Build These Projects ##

The various projects can all be built with Visual Studio 2017 by opening src\Agent.sln.

## Contributing ##

Feel free to branch this project and contribute a pull request to the development branch. If your changes are incorporated
into the master version they'll be published out to NuGet for everyone to use!
