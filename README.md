# Lumora Solution Architecture

Lumora is a comprehensive educational and social platform designed to empower users to learn, track progress, and manage courses and content professionally.  
The platform supports subscriptions, payments, social interaction, and administrative workflows, while strictly adhering to Clean Architecture principles.

---

## Table of Contents

1. [Project Overview](#project-overview)
   - [What is Lumora?](#what-is-lumora)
   - [Architectural Vision](#architectural-vision)

2. [Architectural Principles](#architectural-principles)
   - [Layered Design Strategy](#layered-design-strategy)
   - [Strict Dependency Rules](#strict-dependency-rules)

3. [Solution Layers (Deep Dive)](#solution-layers-deep-dive)
   - [🟣 Domain Layer — The Core](#domain-layer--the-core)
   - [🔵 Application Layer — Orchestration](#application-layer--orchestration)
   - [🟡 Infrastructure Layer — Technical Details](#infrastructure-layer--technical-details)
   - [🔴 Web Layer — Entry Point](#web-layer--entry-point)

4. [Technology Stack](#technology-stack)
5. [Project Structure](#project-structure)
6. [Getting Started](#getting-started)
   - [Prerequisites](#prerequisites)
   - [Installation](#installation)
   - [Database Setup](#database-setup)
   - [Running the Application](#running-the-application)
     
7. [Architectural Manifesto (Rules)](#architectural-manifesto-rules)
8. [Contribution Guide](#contribution-guide)
9. [License](#license)


---


## 1. Project Overview

### What is Lumora?

Lumora is an extensible learning and content management ecosystem built for scalability, maintainability, and long-term evolution.  
It is designed to support:
- Course and content management
- User progress tracking
- Subscriptions and payments
- Social interactions
- Administrative control panels

The solution prioritizes **clean separation of concerns** and **business-first design**.

---

### Architectural Vision

The architecture is intentionally structured to:
- Protect business logic from technical concerns
- Enable easy refactoring and feature growth
- Support multiple external integrations without ripple effects
- Enforce discipline through strict dependency boundaries

---

## 2. Architectural Principles

### Layered Design Strategy
The system is divided into **exactly four layers**, each with a single responsibility and a clear dependency direction.

Dependencies always point inward.
```
Web → Application → Domain
Infrastructure ───┘
```

---

### Strict Dependency Rules

- Domain has **zero dependencies**
- Application depends **only on Domain**
- Infrastructure depends **only on Application**
- Web orchestrates everything but contains **no business logic**

Violations are considered architectural defects.

---

## 3. Solution Layers (Deep Dive)

### 🟣 Domain Layer — The Core

The heart of the system.  
Pure business logic with no technical coupling.

**Contains:**
- Entities & Aggregate Roots
- Value Objects
- Enums
- Business Constants (Roles, Permissions)
- Domain Exceptions

**Strict Rules:**
- No EF Core
- No ASP.NET
- No external libraries
- No annotations

This layer must be reusable in any environment.

---

### 🔵 Application Layer — Orchestration

Defines **what the system does**, not **how it does it**.

**Contains:**
- Application Services (Use Cases)
- Interfaces (Repositories, External Services)
- DTOs (Input / Output models)
- Mapping profiles (AutoMapper)

**Responsibilities:**
- Orchestrate business workflows
- Enforce use-case rules
- Coordinate domain entities

**Strict Rules:**
- Depends only on Domain
- No infrastructure details
- No DbContext or HTTP concerns

---

### 🟡 Infrastructure Layer — Technical Details

Handles **all technical implementations**.

**Contains:**
- EF Core DbContexts (PostgreSQL / Elasticsearch)
- Migrations
- Repository implementations
- Identity (ASP.NET Core Identity)
- External services:
  - Email (SMTP & Templates)
  - Payments (PayTabs)
  - Caching
- Background jobs and schedulers

**Strict Rules:**
- Implements interfaces defined in Application
- No business logic allowed

---

### 🔴 Web Layer — Entry Point

The API boundary of the system.

**Contains:**
- RESTful Controllers
- Middleware (Auth, Culture, Exception Handling)
- FluentValidation validators
- Dependency Injection configuration
- Authentication (JWT, Google OAuth)

**Strict Rules:**
- No business logic
- No direct data access
- Controllers are thin orchestration layers only

---

## 4. Technology Stack

- **Framework:** .NET 8
- **Database:** PostgreSQL (EF Core)
- **Search Engine:** Elasticsearch
- **Authentication:** ASP.NET Core Identity
- **Authorization:** JWT + Google OAuth
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **PDF Generation:** IronPdf
- **Background Jobs:** Hosted Services / Schedulers

---

## 5. Project Structure
```
Lumora/
├── Lumora.Domain/ # Pure business logic (No dependencies)
├── Lumora.Application/ # Use cases, interfaces, DTOs
├── Lumora.Infrastructure/ # DB, repositories, external integrations
└── Lumora.Web/ # API, middleware, DI
```

---

## 6. Getting Started

### Prerequisites

- .NET SDK 8.0+
- PostgreSQL
- Elasticsearch

---

### Installation

Clone the repository:

```
git clone https://github.com/almoneim80/Lumora
```
Database Setup
Update connection strings in:

```
Lumora.Web/appsettings.json
```

Apply migrations:

```
dotnet ef database update \
  --project Lumora.Infrastructure \
  --startup-project Lumora.Web
```

Running the Application
```
dotnet run --project Lumora.Web
```

The API will start with all services wired through dependency injection.

---

## 7. Architectural Manifesto (Rules)

Every contributor must follow these rules:
No Infrastructure in Application
Never inject DbContext into Application services.
No Logic in Controllers
Controllers delegate only.
DTOs for Input and Output
Domain entities are never exposed via API.
Interface Segregation
Interfaces live in Application.
Implementations live in Infrastructure.
Single Direction Dependencies
Any reverse dependency is forbidden.
Breaking these rules requires architectural approval.

## 8. Contribution Guide
Create a feature branch
Follow the four-layer dependency model
Ensure no architectural violations
Open a Pull Request for review
Refactoring for architectural clarity is always welcome.

## 9. License
This project is personally owned by the developer.
It may be reused for personal or commercial purposes while respecting author rights.
