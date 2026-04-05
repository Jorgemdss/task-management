# Containerized .Net Core 10 API with SQL DB

Intro
=============
Sample project for studying .net features, architectural design decisions and practicing concepts.

Features:
- [x] Containerized Api + DB
- [x] Layer separation
- [x] Authentication (Identity)
- [x] Authorization
- [x] Roles
- [x] Unit tests
- [x] No Repositories
- [x] Custom attribute using AuthorizationHandler (ensure only owner/admin can perform operations on resources)
- [ ] Redis
- [ ] MediatR with CQRS
- [ ] Domain events
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
