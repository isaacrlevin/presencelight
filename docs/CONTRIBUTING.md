# Contributing to PresenceLight

Thank you for your interest in contributing to the PresenceLight project! We welcome contributions from the community to help improve and enhance the project. This guide will provide you with the necessary information to get started.

## Table of Contents
- [Contributing to PresenceLight](#contributing-to-presencelight)
  - [Table of Contents](#table-of-contents)
  - [Getting Started](#getting-started)
  - [Setting Up Local Environment](#setting-up-local-environment)
    - [Obtain Microsoft Entra Client ID.](#obtain-microsoft-entra-client-id)
    - [Debugging Windows App](#debugging-windows-app)
    - [Debugging Web App](#debugging-web-app)
  - [Adding New Functionality](#adding-new-functionality)
  - [Contributing Guidelines](#contributing-guidelines)
  - [Submitting a Pull Request](#submitting-a-pull-request)
  - [Code of Conduct](#code-of-conduct)
  - [License](#license)

## Getting Started

To contribute to PresenceLight, you will need to have the following prerequisites:

- Basic knowledge of Git and GitHub.
- Knowledge of the .NET framework and C# programming language (this project uses the latest version, [.NET 8](https://dot.net)).
- IDE of choice ([Visual Studio 2022](https://visualstudio.microsoft.com/downloads/), [Visual Studio Code](https://code.visualstudio.com/Download), [JetBrains Rider](https://www.jetbrains.com/rider/download))
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) if you want to test the Web project running as a container.

## Setting Up Local Environment

### Obtain Microsoft Entra Client ID.
PresenceLight WILL not work if you try to clone and run, because there is a dependency on Microsoft Entra. Because of this, if you want to contribute to the project at this time, please reach out to the maintainer, [Isaac Levin](mailto:isaac@isaaclevin.com) to obtain the Client ID and Client Secret. Once obtained, the Client ID will need to be placed in 2 locations. Firstly, create create a copy of appsettings.json in both the Desktop and Web Projects, calling this new file `appsettings.Development.json`. Place the Client Id, in the proper locations in each file
- [Desktop Version](https://github.com/isaacrlevin/presencelight/blob/main/src/DesktopClient/PresenceLight/appsettings.json#L13)
- [Web Version](https://github.com/isaacrlevin/presencelight/blob/main/src/PresenceLight.Web/appsettings.json#L6)

If you have access to your own Microsoft Entra tenant, you can also create your own Entra App and use the Client ID as well. More information on that is [here](configure-entra-app.md).

### Debugging Windows App

After this, the app should build and run without issue. You can run the Desktop version as either a standalone .NET WPF app or as a packaged app that is deployed locally to the Windows store.
- To debug standalone version, set [PresenceLight](https://github.com/isaacrlevin/presencelight/blob/main/src/DesktopClient/PresenceLight/PresenceLight.csproj) as startup project
- To debug Microsoft Store version, set to [PresenceLight.Package](https://github.com/isaacrlevin/presencelight/blob/main/src/DesktopClient/PresenceLight.Package/PresenceLight.Package.wapproj)
  - NOTE: You may need to enable additional things in Visual Studio to make this work. More info [here](https://learn.microsoft.com/en-us/visualstudio/debugger/debug-installed-app-package)

### Debugging Web App

After adding the [Client ID](https://github.com/isaacrlevin/presencelight/blob/main/src/PresenceLight.Web/appsettings.json#L6) and [Client Secret](https://github.com/isaacrlevin/presencelight/blob/de14b62d0e6b433735eef653cee48d550747b60d/src/PresenceLight.Web/appsettings.json#L10), you should be able to debug the web version by setting the [Web Project](https://github.com/isaacrlevin/presencelight/blob/main/src/PresenceLight.Web/PresenceLight.Web.csproj) as startup.

## Adding New Functionality

If you are adding new functionality to PresenceLight (adding support for a new light for instance), there are a handful of steps you will need to take to enable the functionality in all versions. To understand what you need to do, it would be helpful to understand what all projects in the solution do.

- ### [PresenceLight.Core](https://github.com/isaacrlevin/presencelight/tree/main/src/PresenceLight.Core)
    This project holds all the shared logic for PresenceLight, including interfacing with Microsoft Entra, Microsoft Graph, all lights, as well as all the models that exist for the solution. More than likely, you will be working inside the [Lights](https://github.com/isaacrlevin/presencelight/tree/main/src/PresenceLight.Core/Lights) folder in this project to add a new folder to include the code to support your feature. The project uses [MediatR](https://github.com/jbogard/MediatR) to send messages in-process across the application. Be sure to follow the existing pattern when adding Requests and Handlers

- ### PresenceLight.Razor
  This project holds all of the UI for the application, and leverages ASP.NET Core Blazor components to achieve this. If you are adding new functionality, you will either update an existing component or add a new one. If you are adding a new component, you will add a new `.razor` file in the [Pages](https://github.com/isaacrlevin/presencelight/tree/main/src/PresenceLight.Razor/Components/Pages) folder and add an entry in the `NavMenu.razor` for your new component. Please follow the existing patterns that you see in the other `.razor` files.

- [PresenceLight](https://github.com/isaacrlevin/presencelight/tree/main/src/DesktopClient/PresenceLight)
    This is the WPF project that runs the desktop version of the application. The application contains a single Window that runs all of the functionality (calling the Graph API, calling handlers to update lights). Once you are ready to test your functionality for the Desktop version, add code to light up the functionality in [`MainWindow.xaml.cs`](https://github.com/isaacrlevin/presencelight/blob/main/src/DesktopClient/PresenceLight/MainWindow.xaml.cs).

- ### [PresenceLight.Package](https://github.com/isaacrlevin/presencelight/tree/main/src/DesktopClient/PresenceLight.Package)
  This project wires up the WPF project to run in the Microsoft store. You should not need to update or add anything to this project.

- ### [PresenceLight.Web](https://github.com/isaacrlevin/presencelight/tree/main/src/PresenceLight.Web)
    This is the project that runs the web version of the application. The project leverages a ASP.NET Core Worker Service to run the functionality that "polls" (calling Graph API, calling handlers for lights). Once you are ready to test your functionality for the Web version, add code to light up the functionality in [`Worker.cs`](https://github.com/isaacrlevin/presencelight/blob/main/src/PresenceLight.Web/Worker.cs).

## Contributing Guidelines

Before you start contributing, please take a moment to review the following guidelines:

1. Fork the repository and create a new branch for your contribution.
2. Make sure your code follows the project's coding style and conventions.
3. Write clear and concise commit messages.
4. Test your changes thoroughly before submitting a pull request. It is important if you are adding new functionality to test both the Desktop AND Web versions.
5. Document any new features or changes in the project's documentation.

## Submitting a Pull Request

Once you have made your changes and are ready to submit a pull request, follow these steps:

1. Push your changes to your forked repository.
2. Go to the original repository and create a new pull request.
3. Provide a clear and descriptive title for your pull request.
4. Include a detailed description of the changes you have made.
5. Wait for the project maintainers to review your pull request and provide feedback.

## Code of Conduct

Please note that by contributing to the PresenceLight project, you are expected to adhere to the project's Code of Conduct. The CoC is simple, be respectful and considerate towards others in all interactions.

## License

PresenceLight is licensed under the [MIT License](https://github.com/isaacrlevin/presencelight/blob/main/LICENSE). By contributing to this project, you agree to license your contributions under the same license.

---

We appreciate your contributions to the PresenceLight project! Thank you for helping us make it better.
