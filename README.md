# Cancelation token demo

This is a demo of how to use cancelation tokens in .net core.  This app simulates a microservice architecture where a client makes a request to a service that makes a request to another service.  The client can cancel the request at any time and the service will cancel the request to the other service.

## How to run

1. Clone the repo
2. Run `docker-compose up --build`

## App structure
1. Manager api
2. Resource api
    1. ms sql database