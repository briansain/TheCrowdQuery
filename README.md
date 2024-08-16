# Akka.Skeleton
## What is this?
Akka.Skeleton is a barebones setup of [Akka .Net](https://google.com). I created this so it's easier to start from scratch when using Akka .Net. It's just what's needed to run an Actor System, so anyone can just start coding. 

## Akka.Skeleton.Persistence
This project sets up only what's necessary to start working with Akka.Persistence. The default uses postgres database as the backend, but there's an option to switch to SQL Server.

### Postgres
Run this docker-compose command from `./docker` folder
```
docker-compose -f postgres.docker-compose up -d
```

This will setup a new postgres server with varaiables that match the connection string.

### SQL Server
Uncomment the SQL Server line in the `Akka.Skeleton.Persistence/Program.cs` file from Postgres to SQL Server. Run the following command from the `./docker` folder
```
docker-compose -f sql.docker-compose up -d
```
This will setup a new sql server but it won't create the akkaskeleton database. Run the following commands to create the database:
1. `docker cp ./sql-init.sh sqlserver:.`
2. `docker exec sqlserver bash sql-init.sh`

## Akka.Skeleton.Cluster
This project sets up only what's necessary to start working with Akka.Cluster. The **EchoActor** uses **DistPubSub** to show that some work is happening. Run the `lighthouse.docker-compose` file to setup lighthouse. This is required for the cluster to form with the seed.
```
docker-compose -f lighthouse.docker-compose up -d
```

## Included
- [X] Logging (with Serilog)
- [X] Actor
- [X] Akka.Skeleton.Persistence
- [X] Akka.Skeleton.Cluster

## To-Do
- [ ] Akka.Skeleton.UnitTests