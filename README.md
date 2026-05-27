# GitHub Stargazers Explorer

A full-stack web application designed to explore GitHub repositories and track their stargazers. Built utilizing **.NET 8** on the backend and **React** on the frontend.

## Architecture & Tech Stack

- **Frontend:** React (Vite, TypeScript)
- **Backend:** ASP.NET Core Web API
- **Database:** SQLite
- **Authentication:** JWT
- **CI/CD:** Azure Pipelines

## Project Structure

```text
├── src
│   ├── frontend/                    # React application
│   └── backend/                     # .NET Solution
│       ├── GitHubExplorer.WebApi/   # Main API project
│       └── GitHubExplorer.Tests/    # xUnit Unit Tests
├── .gitignore
└── azure-pipelines.yml