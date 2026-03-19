# GraphQL .NET Server — Employee Skills API
* After a successful Python prototype that the Lead Solution Architect approved, I went on to build the .Net Version for its scalability as part of a production ready POC/MVP.
---
A **GraphQL API** built with [Hot Chocolate](https://chillicream.com/docs/hotchocolate) on ASP.NET Core 9 and MongoDB, designed to manage employees, their current roles, desired roles, and SFIA skill proficiencies.

> **Note on credentials:** An earlier version of this repository contained a MongoDB Atlas connection string as part of a fictional client scenario used for development and demonstration purposes only. The fictional client — **Standard Bank (SBSA-Test)** — and all associated connection details were placeholder values created specifically to give the project a realistic, professional context during its initial build phase. Those credentials have since been removed from the codebase and replaced with a secure configuration-driven approach. No real client data was ever at risk.

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Data Model](#data-model)
- [GraphQL Schema](#graphql-schema)
- [Request Flow](#request-flow)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [GraphQL Operations](#graphql-operations)
- [Changelog & Resolved Issues](#changelog--resolved-issues)
- [Roadmap](#roadmap)

---

## Overview

This server exposes a GraphQL endpoint (`/graphql`) alongside an interactive **Nitro IDE** (provided by ChilliCream) at the same path. It connects to a MongoDB Atlas cluster and supports querying and mutating `Employee` documents — each of which embeds rich nested data about an employee's current role, desired role, and their individual SFIA skill proficiencies.

The architecture is intentionally lean: no ORM, no repository abstraction layer, just direct MongoDB driver calls wired into a code-first HotChocolate schema. This keeps the dependency graph shallow and the data flow easy to trace end-to-end.

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET / ASP.NET Core | 9.0 |
| GraphQL Server | HotChocolate.AspNetCore | 15.1.10 |
| GraphQL Data | HotChocolate.Data | 15.1.10 |
| Database Driver | MongoDB.Driver | 3.5.0 |
| Database Host | MongoDB Atlas | — |

---

## Project Structure

```
GraphQL.Net Server/
├── GraphQLOps/
│   ├── Query.cs               # Read operations — GetEmployees, GetEmployeeById
│   └── Mutation.cs            # Write operations — AddEmployee
├── Models/
│   ├── Employees.cs           # MongoDB document model (domain layer)
│   ├── Role.cs                # Embedded role subdocument
│   ├── Skill.cs               # Skill reference with SFIA code
│   ├── SkillProficiency.cs    # Employee skill paired with SFIA proficiency level
│   ├── SkillRequired.cs       # Role requirement: a skill at a minimum level
│   ├── AddEmployeeInput.cs    # Root GraphQL mutation input type
│   ├── RoleInput.cs           # Nested input type for roles
│   ├── SkillInput.cs          # Nested input type for skills
│   ├── SkillProficiencyInput.cs
│   └── SkillRequiredInput.cs
├── Program.cs                 # Application bootstrap, DI registration, middleware
└── appsettings.json           # Non-sensitive configuration structure
```

---

## Data Model

The entire employee record is stored as a **single embedded MongoDB document** — there are no foreign-key joins or cross-collection lookups at query time. All role, skill, and proficiency data lives nested inside the employee record, which makes reads fast and the document self-contained.

```mermaid
erDiagram
    EMPLOYEES {
        ObjectId Id PK
        string Name
        bool IsMentor
    }
    ROLE {
        ObjectId Id
        string Title
    }
    SKILL {
        ObjectId Id
        string Name
        string sfia_Code
    }
    SKILL_PROFICIENCY {
        string SkillName
        int sfia_Level
    }
    SKILL_REQUIRED {
        int sfia_Level
    }

    EMPLOYEES ||--|| ROLE : "currentRole (embedded)"
    EMPLOYEES ||--|| ROLE : "desiredRole (embedded)"
    EMPLOYEES ||--o{ SKILL_PROFICIENCY : "skills (embedded list)"
    SKILL_PROFICIENCY ||--|| SKILL : "skill ref (embedded)"
    ROLE ||--o{ SKILL_REQUIRED : "skillRequired (embedded list)"
    SKILL_REQUIRED ||--|| SKILL : "skill ref (embedded)"
```

### Class Relationship Diagram

```mermaid
classDiagram
    class Employees {
        +string? Id
        +string? Name
        +Role? CurrentRole
        +List~SkillProficiency~ Skills
        +Role? DesiredRole
        +bool? IsMentor
    }

    class Role {
        +string? Id
        +string? Title
        +List~SkillRequired~ SkillRequired
    }

    class Skill {
        +string? Id
        +string? Name
        +string? sfia_Code
    }

    class SkillProficiency {
        +string? SkillName
        +int? sfia_Level
        +Skill? Skills
    }

    class SkillRequired {
        +Skill? Skill
        +int? sfia_Level
    }

    Employees "1" --> "1" Role : currentRole
    Employees "1" --> "1" Role : desiredRole
    Employees "1" --> "0..*" SkillProficiency : skills
    Role "1" --> "0..*" SkillRequired : skillRequired
    SkillProficiency "1" --> "1" Skill
    SkillRequired "1" --> "1" Skill
```

### Input Type Separation

The project maintains a deliberate split between **domain models** (used for MongoDB reads/writes, decorated with BSON attributes) and **input types** (used for GraphQL mutations, with no storage concerns). This separation is important because HotChocolate treats input types differently from output types in schema generation.

```mermaid
graph LR
    subgraph GraphQL Input Layer
        AI[AddEmployeeInput]
        RI[RoleInput]
        SPI[SkillProficiencyInput]
        SI[SkillInput]
        SRI[SkillRequiredInput]
        AI --> RI
        AI --> SPI
        RI --> SRI
        SPI --> SI
        SRI --> SI
    end

    subgraph Domain and MongoDB Layer
        E[Employees]
        R[Role]
        SP[SkillProficiency]
        SK[Skill]
        SR[SkillRequired]
        E --> R
        E --> SP
        R --> SR
        SP --> SK
        SR --> SK
    end

    AI -- "Mutation.cs maps to" --> E
```

---

## GraphQL Schema

The schema is **code-first** — HotChocolate infers all GraphQL types from your C# class definitions automatically, using reflection and attribute conventions. You do not write a `.graphql` schema file by hand.

```mermaid
graph TD
    subgraph Schema Root
        Q[Query]
        M[Mutation]
    end

    subgraph Query Operations
        Q -->|getEmployees| EL["[Employees]"]
        Q -->|getEmployeeById - id: String| E["Employees?"]
    end

    subgraph Mutation Operations
        M -->|addEmployee - input: AddEmployeeInput| E2["Employees"]
    end

    subgraph Employees Type
        EL --> F1["id: String"]
        EL --> F2["name: String"]
        EL --> F3["currentRole: Role"]
        EL --> F4["desiredRole: Role"]
        EL --> F5["skills: [SkillProficiency]"]
        EL --> F6["isMentor: Boolean"]
    end

    subgraph Role Type
        F3 --> R1["id: String"]
        F3 --> R2["title: String"]
        F3 --> R3["skillRequired: [SkillRequired]"]
    end

    subgraph SkillRequired Type
        R3 --> SR1["sfia_Level: Int"]
        R3 --> SR2["skill: Skill"]
    end

    subgraph Skill Type
        SR2 --> SK1["id: String"]
        SR2 --> SK2["name: String"]
        SR2 --> SK3["sfia_Code: String"]
    end
```

---

## Request Flow

### Query Flow — GetEmployees

```mermaid
sequenceDiagram
    participant Client
    participant ASP as ASP.NET Core
    participant HC as HotChocolate
    participant Query as Query.cs
    participant Mongo as MongoDB Atlas

    Client->>ASP: POST /graphql { query: "{ employees { id name ... } }" }
    ASP->>HC: Route to GraphQL middleware
    HC->>HC: Parse and validate query document
    HC->>Query: GetEmployees(IMongoDatabase)
    Query->>Mongo: collection.Find(_ => true).ToListAsync()
    Mongo-->>Query: List of Employees documents
    Query-->>HC: List of Employees
    HC->>HC: Project only the requested fields into JSON
    HC-->>ASP: 200 OK { data: { employees: [...] } }
    ASP-->>Client: JSON response
```

### Mutation Flow — AddEmployee

```mermaid
sequenceDiagram
    participant Client
    participant ASP as ASP.NET Core
    participant HC as HotChocolate
    participant Mutation as Mutation.cs
    participant Mongo as MongoDB Atlas

    Client->>ASP: POST /graphql { mutation: "addEmployee(input: { ... })" }
    ASP->>HC: Route to GraphQL middleware
    HC->>HC: Parse, validate and coerce input types
    HC->>Mutation: AddEmployee(AddEmployeeInput, IMongoDatabase)
    Mutation->>Mutation: Map AddEmployeeInput to Employees domain object
    Mutation->>Mongo: collection.InsertOneAsync(employee)
    Mongo-->>Mutation: Write acknowledged + Id assigned
    Mutation-->>HC: Employees object with new MongoDB-generated Id
    HC->>HC: Project requested response fields into JSON
    HC-->>Client: { data: { addEmployee: { id, name, ... } } }
```

### Application Startup and DI Wiring

```mermaid
flowchart TD
    A[Program.cs starts] --> B[Create WebApplicationBuilder]
    B --> C[Read MongoDB config from IConfiguration\nappsettings / User Secrets / env vars]
    C --> D[Build MongoClientSettings from connection string]
    D --> E[Instantiate MongoClient]
    E --> F{Ping admin database}
    F -->|Success| G[Log: Successfully connected to MongoDB]
    F -->|Fail| H[Log: Connection failed with error message]
    G & H --> I[Register IMongoClient as Singleton]
    I --> J[Register IMongoDatabase as Singleton\nusing configured DatabaseName]
    J --> K[Register GraphQL Server\nQuery + Mutation + Filtering + Sorting\nIncludeExceptionDetails only in Development]
    K --> L[Build WebApplication]
    L --> M[Map GET / to Hello World]
    M --> N[MapGraphQL to /graphql]
    N --> O[app.Run - begin accepting requests]
```

---

## Getting Started

### Prerequisites

You will need the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0), a MongoDB Atlas cluster (or a locally running MongoDB instance), and an editor such as Visual Studio 2022, VS Code, or JetBrains Rider.

### Clone and Run

```bash
git clone https://github.com/Shotza247/GraphQL-Server-SetUp.Net.git
cd "GraphQL Server SetUp.Net"

# Restore all NuGet packages
dotnet restore

# Store your MongoDB credentials as local User Secrets (never committed to git)
dotnet user-secrets init --project "GraphQL.Net Server"
dotnet user-secrets set "MongoDB:ConnectionString" "mongodb+srv://<user>:<password>@<cluster>.mongodb.net/" \
  --project "GraphQL.Net Server"
dotnet user-secrets set "MongoDB:DatabaseName" "YourDatabaseName" \
  --project "GraphQL.Net Server"

# Run in Development mode
dotnet run --project "GraphQL.Net Server"
```

The server starts on:
- HTTP: `http://localhost:5280`
- HTTPS: `https://localhost:7228`

Navigate to `http://localhost:5280/graphql` to open the **Nitro GraphQL IDE** in your browser.

---

## Configuration

Sensitive values such as your MongoDB connection string are kept entirely out of source control using **.NET User Secrets** locally and **environment variables** in production or CI/CD pipelines. The `appsettings.json` file only defines the configuration structure — it never holds real credentials.

**`appsettings.json`** — safe to commit, contains no real values:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDB": {
    "ConnectionString": "mongodb+srv://<user>:<password>@<cluster>.mongodb.net/",
    "DatabaseName": "YourDatabaseName"
  }
}
```

In production, override `MongoDB__ConnectionString` and `MongoDB__DatabaseName` using environment variables (ASP.NET Core maps double-underscore `__` to the colon `:` separator automatically).

---

## GraphQL Operations

### Query: Get All Employees

```graphql
query GetAllEmployees {
  employees {
    id
    name
    isMentor
    currentRole {
      title
      skillRequired {
        sfia_Level
        skill {
          name
          sfia_Code
        }
      }
    }
    desiredRole {
      title
    }
    skills {
      skillName
      sfia_Level
      skills {
        name
        sfia_Code
      }
    }
  }
}
```

### Query: Get Employee by ID

```graphql
query GetEmployee($id: String!) {
  employeeById(id: $id) {
    id
    name
    currentRole {
      title
    }
    desiredRole {
      title
    }
  }
}
```

### Mutation: Add Employee

```graphql
mutation AddEmployee($input: AddEmployeeInput!) {
  addEmployee(input: $input) {
    id
    name
    isMentor
    currentRole {
      title
    }
  }
}
```

**Example variables:**
```json
{
  "input": {
    "name": "Jane Smith",
    "isMentor": true,
    "currentRole": {
      "id": "",
      "title": "Senior Developer",
      "skillRequired": [
        {
          "sfia_Level": 5,
          "skill": {
            "id": "",
            "name": "Software Development",
            "sfia_Code": "PROG"
          }
        }
      ]
    },
    "desiredRole": {
      "id": "",
      "title": "Technical Architect",
      "skillRequired": []
    },
    "skills": [
      {
        "skillName": "C#",
        "sfia_Level": 5,
        "skills": {
          "id": "",
          "name": "Software Development",
          "sfia_Code": "PROG"
        }
      }
    ]
  }
}
```

---

## Changelog & Resolved Issues

The following issues were identified during code review and have been fully resolved in the current version. They are documented here for transparency and as a useful reference for anyone reading the git history.

```mermaid
graph TD
    subgraph Resolved
        R1["✅ Collection name mismatch\nMutation now writes to 'Employees'\nmatching the Query collection"]
        R2["✅ Input type separation\nAddEmployeeInput now uses RoleInput\nand SkillProficiencyInput correctly"]
        R3["✅ C# property casing\nSfia_Level and Sfia_Code\naccessed with correct Pascal casing"]
        R4["✅ Hardcoded credentials removed\nConnection string moved to\nIConfiguration with User Secrets support"]
    end
```

**Resolved: MongoDB collection name mismatch** — `Mutation.cs` was writing new employee documents to a collection named `"Employee"` (singular) while `Query.cs` was reading from `"Employees"` (plural). In MongoDB, these are entirely separate collections, meaning every `addEmployee` call appeared to succeed but the created document would never appear in any query result. Both operations now consistently target `"Employees"`.

**Resolved: Input type layer bypassed** — `AddEmployeeInput` was declaring its `CurrentRole`, `DesiredRole`, and `Skills` properties using the domain model types `Role` and `SkillProficiency`, which carry BSON serialisation attributes intended for the database layer. This bypassed the dedicated `RoleInput` and `SkillProficiencyInput` classes entirely and risked schema generation conflicts in HotChocolate. All properties now correctly reference their corresponding GraphQL input types.

**Resolved: C# property casing compile error** — `Mutation.cs` was accessing `s.sfia_Level` and `sr.Skill.sfia_Code` with a lowercase `s`, while the actual input type properties are declared as `Sfia_Level` and `Sfia_Code` (Pascal case). C# property access is case-sensitive, so this was a compile-time error. All property access expressions have been corrected throughout the mutation handler.

**Resolved: Hardcoded credentials removed** — the MongoDB Atlas connection string (created for a fictional client test-case scenario — see the note at the top of this document) was previously embedded directly in `Program.cs`. This has been replaced with a fully configuration-driven approach using `IConfiguration`, .NET User Secrets for local development, and environment variables for deployment. `IncludeExceptionDetails` has also been scoped to the Development environment only, preventing internal stack traces from reaching clients in production.

---

## Roadmap

The following improvements would meaningfully strengthen the project going forward.

**Update and Delete Mutations** — adding `UpdateEmployee` and `DeleteEmployee` mutations would complete the CRUD surface. HotChocolate supports strongly-typed update input patterns that map cleanly to MongoDB's `FindOneAndUpdateAsync` and `DeleteOneAsync`.

**Normalise Roles and Skills into Separate Collections** — currently roles and skills are fully embedded inside each employee document. For a true role-based career-pathing system, extracting roles into their own collection and referencing them by ID would prevent duplication and make global role definition updates atomic across all employees.

**Authentication and Authorisation** — HotChocolate supports `[Authorize]` attribute-based policies via `HotChocolate.Authorization`. Pairing this with ASP.NET Core's JWT bearer middleware would secure the API for multi-tenant use with minimal additional wiring.

**Pagination** — `HotChocolate.Data` provides cursor-based pagination out of the box. Adding `.UsePaging()` to the `GetEmployees` resolver prevents unbounded result sets as the employee count grows, and is a one-line change.

**Integration Tests** — an xUnit test project using `Testcontainers.MongoDb` would enable deterministic integration tests that spin up a real MongoDB instance in Docker, without depending on a live Atlas cluster for every CI run.

---

> Built on [Hot Chocolate 15](https://chillicream.com/docs/hotchocolate) · [MongoDB .NET Driver 3](https://www.mongodb.com/docs/drivers/csharp/) · ASP.NET Core 9
