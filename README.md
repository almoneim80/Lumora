# Lumora Platform

**Lumora** is a comprehensive web-based learning and career platform built with **.NET 8**. It combines educational content, AI-driven automation, career services, and social interaction to provide a seamless experience for learners and professionals.

---

## Features

### Learning Management
- Training paths (Programs) consisting of multiple courses.
- Each course includes lessons, attachments, tests, and completion certificates.
- Support for live courses via video conferencing (free or paid).
- AI-assisted course content creation: lesson files, translations, test generation, automated grading.

### Career Services
- Job listings and application system integrated within the platform.
- AI-driven interview scheduling with notifications.
- Video interviews via Zoom, Google Meet, or built-in solutions.

### Payments & Subscriptions
- Supports multiple payment gateways: Tabby, Tamara, PayTabs.
- Flexible subscription and purchase management.

### Community & Engagement
- Tourist Club: Social hub for sharing ideas and posts.
- Podcast publishing support.
- Affiliate marketing system integrated.

### User Management & Security
- Multiple authentication methods: Google, Apple, email, phone number.
- Full authorization and role management with granular permissions.
- 2FA, OTP verification, token-based authentication (access + refresh tokens).
- Multi-language support (100+ languages) and strict security standards.
- Complete profile and account management.

### Communication
- Flexible support for SMS and Email notifications.
- Integration with **OpenAI API** and **Div.ai** for AI-based tasks.

### Extensibility
- Plugin architecture (e.g., SMS plugin shown in project structure).
- Easily extendable services and controllers.
- Comprehensive API endpoints for all modules.

---

## Project Structure

Lumora.sln
│
├── plugins/
│ └── Lumora.Plugin.Sms/ # SMS plugin implementation
├── src/
│ ├── Lumora.Core/ # Core services, entities, DTOs, helpers
│ ├── Lumora.Web/ # API project with controllers, filters, middlewares
│ └── Tasks/ # Background task implementations
└── README.md # Project documentation



- **Lumora.Core**: Core business logic, entities, DTOs, services, plugins, integrations.
- **Lumora.Web**: REST API controllers, authentication, authorization, admin and user APIs.
- **Plugins**: Extendable modules like SMS or custom integrations.
- **Tasks**: Background operations and automation tasks.

---

## Technology Stack

- **Backend**: .NET 8, C#  
- **Database**: PostgreSQL, ElasticSearch  
- **Authentication**: Identity, JWT, 2FA, OAuth (Google, Apple)  
- **Payments**: Tabby, Tamara, PayTabs  
- **AI Integration**: OpenAI API, Div.ai  
- **Frontend**: Separate SPA or client-side application can consume API  
- **Messaging**: Flexible email/SMS provider support  
- **Localization**: Multi-language support (100+ languages)  
- **Security**: OTP verification, encryption, access & refresh tokens, authorization & roles  

---

## Setup Instructions

1. Clone the repository:

```bash
git clone https://github.com/username/lumora.git
cd lumora

Restore dependencies:
dotnet restore
Update database connection strings in appsettings.json.
Apply migrations:
dotnet ef database update --project src/Lumora.Core
Run the API:
dotnet run --project src/Lumora.Web
Access API via https://localhost:5001 (or configured port).

---


### Usage
All APIs follow REST conventions.
Authentication required for secured endpoints.
Admin APIs available for management tasks: user, course, job, and payment management.
Plugin system allows adding features like SMS, email, and AI-based services.

### Contributing
Fork the repository.
Create a feature branch: git checkout -b feature-name
Commit your changes: git commit -m "Description"
Push to the branch: git push origin feature-name
Open a Pull Request.

### License
This project is proprietary / MIT License (choose based on your preference).
