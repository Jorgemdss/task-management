# Containerized .Net Core 10 API with SQL DB

Intro
=============
Sample project for studying .net features, architectural design decisions and .

Features:
- [x] Containerized
- [x] Layer separation
- [x] Authentication (Identity)
- [x] Authorization
- [x] Unit tests
- [x] No Repositories
- [x] Role based authorization (through AuthorizationHandler)
- [ ] Authorize attribute
---

Running the docker container
=============
### Run API and DB

`docker compose up --build`
This command runs both the API and DB images inside a docker container.

### Stoping API and DB
`docker compose down`
This command stops the docker container.

### Stoping and removing volumes
`docker compose down -v`
This command stops and removes the docker container and volumes. ***Attention, this command will delete your DB data!***

---
