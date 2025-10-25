<Lumora>

<Lumora> is a comprehensive educational and social platform designed to empower users to learn, track progress, 
and manage courses and content professionally, with support for subscriptions, payments, social interaction, and administrative tasks.

###  Key Features
Course and Learning Program Management
Supports multiple course types: Online, Path-based, Skill Library.
Manage lessons, exercises, tests, and user progress tracking.
Subscription and Payment System
Support for creating and renewing subscriptions.
Link payments to subscriptions and purchases.
Dynamic Content Management
Supports static and dynamic content (text, images, files).
Database-driven content updates without changing code.
Consultation and Support System
Manage counselors, bookings, and tickets.
Messaging and attachments for professional communication.
Diverse Purchase System
Supports purchasing courses, certificates, consultations, and other digital products.
Handles refunds and detailed purchase statistics.
Reports and Analytics
Track user progress.
Comprehensive analytics for admins and supervisors.
Professional Design and Scalability
Extensible architecture.
Localization support (multi-language).
Clear and organized folder structure.

### Folder Structure
Lumora/
├── plugins/
│   └── Lumora.Plugin.Sms/
│       ├── Dependencies/
│       ├── Configuration/
│       ├── Controllers/
│       ├── DTOs/
│       ├── Exceptions/
│       ├── Interfaces/
│       ├── Services/
│       ├── Tasks/
│       ├── pluginsettings.json
│       ├── SmsPlugin.cs
│       ├── stylecop.json
│       └── Using.cs
├── Lumora.Core/
│   ├── Configuration/
│   │   ├── MappingProfiles/
│   ├── Data/
│   │   ├── DbSets/
│   ├── DataAnnotations/
│   ├── DTOs/
│   │   ├── AffiliateMarketing/
│   │   ├── Authentication/
│   │   ├── Authorization/
│   │   ├── Base/
│   │   ├── Club/
│   │   ├── Email/
│   │   ├── Job/
│   │   ├── LiveCourse/
│   │   ├── MediaDtos/
│   │   ├── Payment/
│   │   ├── Podcast/
│   │   ├── Programs/
│   │   ├── StaticContent/
│   │   ├── Test/
│   │   ├── Token/
│   ├── Elastic/
│   │   ├── Migrations/
│   ├── Entities/
│   │   └── Tables/
│   ├── Enums/
│   ├── Exceptions/
│   ├── Extensions/
│   ├── Formatters/
│   │   └── Csv/
│   ├── Geography/
│   ├── Helpers/
│   │   ├── Customer/
│   ├── Identity/
│   ├── Infrastructure/
│   │   ├── PermissionInfra/
│   │   ├── StaticContentInfra/
│   ├── Interfaces/
│   │   ├── AffiliateMarketingIntf/
│   │   ├── AuthenticationIntf/
│   │   ├── AuthorizationIntf/
│   │   ├── ClubIntf/
│   │   ├── EmailIntf/
│   │   ├── JobIntf/
│   │   ├── LiveCourseIntf/
│   │   ├── PaymentIntf/
│   │   ├── PodcastEpisodeIntf/
│   │   ├── ProgramIntf/
│   │   ├── QueriesIntf/
│   │   ├── StaticContentIntf/
│   │   ├── TestIntf/
│   │   ├── TokenIntf/
│   │   ├── UserIntf/
│   ├── Localization/
│   │   ├── Ratings/
│   ├── Middlewares/
│   ├── Services/
│   │   ├── AffiliateMarketingSvc/
│   │   ├── AuthenticationSvc/
│   │   ├── AuthorizationSvc/
│   │   ├── BaseSvc/
│   │   ├── ClubSvc/
│   │   ├── Core/
│   │   │   ├── Langs/
│   │   │   └── Messages/
│   │   ├── EmailSvc/
│   │   ├── JobServiceSvc/
│   │   ├── LiveCourseSvc/
│   │   ├── OTPSvc/
│   │   ├── PaymentSvc/
│   │   │   ├── Gateways/
│   │   ├── PodcastEpisodeSvc/
│   │   ├── ProgramsSvc/
│   │   ├── QueriesSvc/
│   │   ├── StaticContentSvc/
│   │   ├── TestSvc/
│   │   ├── TokenSvc/
│   │   ├── UserSvc/
│   ├── Tasks/
└── Lumora.Web/
├── Connected Services/
├── Dependencies/
├── Properties/
│   └── PublishProfiles/
├── Controllers/
│   ├── AffiliateMarketingAPI/
│   ├── AuthenticationAPI/
│   │   ├── Admin/
│   ├── AuthorizationAPI/
│   ├── ClubAPI/
│   │   └── Admin/
│   ├── EmailAPI/
│   ├── JobAPI/
│   │   ├── Admin/
│   ├── LiveCourseAPI/
│   │   ├── Admin/
│   ├── LocalizationAPI/
│   ├── PaymentAPI/
│   ├── PodcastEpisodeAPI/
│   │   ├── Admin/
│   ├── ProgramsAPI/
│   │   ├── Admin/
│   ├── StaticContentAPI/
│   ├── TestAPI/
│   │   ├── Admin/
├── Extensions/
├── Filters/
├── Migrations/
├── Resources/
├── Validations/
│   ├── AuthenticationVal/
│   ├── AuthorizationVal/
│   ├── BaseVal/
│   ├── ClubVal/
│   ├── Customer/
│   ├── JobVal/
│   ├── LiveCourseValidators/
│   ├── PodcastEpisodeVal/
│   ├── ProgramVal/
│   ├── TestVal/

### Prerequisites
.NET 8 SDK or later
PostgreSQL
Package managers: NuGet

###Install dependencies:
dotnet restore
Set up the database:
dotnet ef database update
Run the project:
dotnet run

###Development
Service Separation: Each module (Subscription, Course, Payment) is independent for maintainability.
Custom DTOs per Module: Simplifies data transfer between layers.
MemoryCache / Caching: Improves performance for content retrieval.
Localization: Supports both English and Arabic messages.
Logging & Error Handling: Professional logging for easy issue tracking.

###Security
Role-based user and permission management.
Sensitive data protection via environment files (User Secrets / .env).
Secure API endpoints using JWT Tokens or Identity Server.

###Future Expansion
Add additional languages.
Integrate AI/Chatbot to assist users.
Expand the subscription and digital products system.

###Contributors
Lead Developer: Abdulmoneim Omar Alward
Future contributors can be added following professional standards.

###License
This project is personally owned by the developer and can be reused for personal or commercial purposes while respecting author rights.