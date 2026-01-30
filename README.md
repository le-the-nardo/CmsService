# CMS Entity Ingestion Service

This project implements a service responsible for ingesting entity events sent by a CMS via webhook, persisting versioned entities, and securely exposing data to authenticated users, according to the technical challenge requirements.

---

## Overview

The system receives entity events (publish, unpublish, delete) sent by a CMS in batch format and maintains a versioned history of entities.

Key features:
- Batch event processing
- Entity versioning
- Migrations
- Clear separation between read and write models
- Basic Authentication (CMS and API Users)
- Role-based visibility (User vs Admin)
- Proper handling of versioning corner cases
- Unit Tests for user authentication and business rules

---

## Architecture

- **Domain**
  - `Entity` and `EntityVersion`: core domain models
  - `EntityDomainService`: business rules and state orchestration
- **Infrastructure**
  - Persistence using Entity Framework Core
- **API**
  - REST endpoints
  - Separate Basic Auth mechanisms for CMS and API users

---

## Authentication

There are two authentication flows:

### CMS Authentication
- Used exclusively for the `/cms/events` endpoint
- Responsible for sending ingestion events
- Uses Basic Authentication with dedicated credentials

### User Authentication
- Used for entity read endpoints
- Roles:
  - **User**: can only access published and active entities
  - **Admin**: can access all entities, including unpublished or disabled ones

### Security Note 
⚠️ Authentication credentials are stored in `appsettings.json` **for local development and challenge purposes only**.

---

## Business Rules

- Data is only publicly available after a **publish** event
- `unpublish` removes the published version without deleting the entity
- Admin users can disable entities without affecting CMS data
- Corner case handled:
  - A publish followed by an unpublish within the same batch results in no published version
- Invalid events do not break batch processing
- Unpublish an event already unpublished does not break batch processing

---

## Testing

### Unit Tests
Coverage includes:

Business Rules
- Publish flow
- Idempotency with Publish flow
- Avoid publishing older version
- Unpublish flow
- Unpublish unpublished version
- Unknow event type
- Delete flow

Authentication rules
- Basic Authentication (Admin vs user)

---

## How to Run

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CmsService
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Endpoints via Postman**

### CMS Events
- **POST /cms/events**
  - Endpoint responsible for processing batch events coming from the CMS.
  - Requires authentication using the **CMS system user**.
  - Used exclusively for asynchronous event ingestion and processing.

### Entities
- **GET api/entities/{id}**
- **GET api/entities**
- **POST api/entities/{id}/disable**
  - API endpoints for managing domain entities.
  - Follow REST standards and share the same API authentication mechanis

## Postman Collection

A Postman collection is available to facilitate manual testing of the API.
Location: /CmsService/CMS Service.postman_collection.json

## Author 👦🏻

Made by me with lots of ☕ and ❤.