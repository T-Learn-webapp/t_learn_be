LearnFlash.sln
└── src/
    ├── LearnFlash.API/                  # Presentation Layer
    ├── LearnFlash.Application/          # Application Layer
    ├── LearnFlash.Domain/               # Domain Layer (Core)
    ├── LearnFlash.Infrastructure/       # Infrastructure Layer
    ├── LearnFlash.Common/               # Shared Kernel (optional)
    └── LearnFlash.Tests/                # Test projects

└── tests/
    ├── LearnFlash.Application.Tests/
    ├── LearnFlash.Infrastructure.Tests/
    └── LearnFlash.API.IntegrationTests/






LearnFlash.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── SubjectsController.cs
│   ├── MaterialsController.cs
│   ├── FlashcardsController.cs
│   ├── QuizzesController.cs
│   ├── AIController.cs
│   ├── StudyRoomsController.cs
│   └── PaymentsController.cs
├── Hubs/
│   ├── StudyRoomHub.cs
│   └── NotificationHub.cs
├── Middleware/
│   ├── GlobalExceptionMiddleware.cs
│   ├── JwtMiddleware.cs
│   └── RateLimitingMiddleware.cs
├── Filters/                          # Action Filters
├── DTOs/                             # Input/Output Models (Request, Response)
│   ├── Auth/
│   ├── Flashcard/
│   └── Quiz/
├── Extensions/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── LearnFlash.API.csproj

LearnFlash.Application/
├── Common/
│   ├── Interfaces/
│   ├── Behaviors/                    # Pipeline Behaviors (MediatR)
│   ├── Mappings/                     # AutoMapper Profiles
│   └── Exceptions/
├── Features/
│   ├── Auth/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── DTOs/
│   ├── Subjects/
│   ├── LearningMaterials/
│   ├── Flashcards/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Services/
│   ├── AI/
│   │   ├── Commands/GenerateFlashcardsCommand.cs
│   │   └── Services/IAIService.cs
│   ├── Quizzes/
│   ├── StudyRooms/
│   ├── Payments/
│   └── Progress/
├── Interfaces/
│   ├── Repositories/
│   ├── Services/
│   ├── ICurrentUserService.cs
│   └── IRedisService.cs
├── Validators/
├── Services/                         # Application Services
└── LearnFlash.Application.csproj



LearnFlash.Domain/
├── Entities/
│   ├── User.cs
│   ├── Subject.cs
│   ├── LearningMaterial.cs
│   ├── Flashcard.cs
│   ├── Question.cs
│   ├── Quiz.cs
│   ├── StudyRoom.cs
│   ├── Subscription.cs
│   └── Payment.cs
├── Enums/
│   ├── QuestionType.cs
│   ├── SubscriptionPlan.cs
│   ├── PaymentStatus.cs
│   └── DifficultyLevel.cs
├── ValueObjects/
│   ├── Email.cs
│   ├── Content.cs
│   └── Money.cs
├── Events/                           # Domain Events
│   ├── FlashcardCreatedEvent.cs
│   └── QuizCompletedEvent.cs
├── Exceptions/
│   ├── DomainException.cs
│   └── BusinessRuleViolationException.cs
├── Specifications/                   # Specification Pattern (optional)
├── Rules/                            # Business Rules
└── LearnFlash.Domain.csproj


LearnFlash.Infrastructure/
├── Data/
│   ├── LearnFlashDbContext.cs
│   ├── Configurations/               # Entity Configurations
│   │   ├── UserConfiguration.cs
│   │   └── FlashcardConfiguration.cs
│   └── Migrations/
├── Repositories/
│   ├── BaseRepository.cs
│   ├── UserRepository.cs
│   ├── FlashcardRepository.cs
│   └── ...
├── Persistence/
│   ├── Redis/
│   │   ├── RedisService.cs
│   │   └── RedisCacheService.cs
│   └── Caching/
├── Payment/
│   ├── PayOS/
│   │   ├── PayOSService.cs
│   │   └── Models/
│   └── Interfaces/IPaymentService.cs
├── AI/
│   ├── OpenAIService.cs
│   └── Models/
├── Identity/
│   └── JwtTokenService.cs
├── SignalR/
├── BackgroundJobs/                   # Hangfire hoặc Quartz (nếu cần)
├── DependencyInjection.cs            # Extension method để register services
└── LearnFlash.Infrastructure.csproj


LearnFlash.Common/
├── Result.cs                         # Result Pattern
├── PaginatedResult.cs
├── Constants/
├── Helpers/
└── LearnFlash.Common.csproj

Add migration:
dotnet ef migrations add Invite --project TLearn.Infrastructure --startup-project TLearn.API --output-dir TLearn.Infrastructure/Data/Migrations

update Migration :
dotnet ef database update --project TLearn.Infrastructure --startup-project TLearn.API