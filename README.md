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
- Basic Authentication (CMS and Users)
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
  - Separate Basic Auth mechanisms for CMS and users

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
- Publish flow
- Unpublish flow
- Corner case: publish + unpublish in the same batch
- Admin vs user visibility rules

Tools used:
- xUnit
- Moq
- FluentAssertions

### Manual Tests
A separate document describes all manual test scenarios required to validate the challenge.

---

## How to Run

```bash
dotnet restore
dotnet run

## Author 👦🏻

Made by me with lots of ☕ and ❤.