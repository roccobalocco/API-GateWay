# Oral Presentation

## Introduction

I developed a microservices-based system orchestrated with Kubernetes, with an API Gateway pattern built in.

The API Gateway pattern allows clients to use a single entry point, while I, as the server, can route the requests to the correct microservice.

This improves the reliability of the system, because each service is independent from the others, even if they share a common database.

In this project, all microservices share the same database. As explained in microservices.io
, the benefit is ACID consistency and simpler management.
Of course, in the real world, the per-service-database pattern is often better. But for a demonstration project, a shared database is acceptable, as long as we are aware of the trade-offs.

Another important concept is the Orchestration Pattern. This is an evolution of the API Gateway, where one service coordinates several others to achieve a goal. This becomes useful when the business logic is more complex, or when the scalability of a single entry point becomes critical. In fact, major cloud providers such as AWS and GCP already provide managed API Gateways and orchestration tools.

## Architecture

The API Gateway handles routing, authentication, resilience policies with Polly, rate limiting, bulkhead isolation, logging, and circuit breakers.

The microservices each handle only their own domain logic.

NGINX ingress provides secure external access with TLS termination and CORS policies.

Prometheus collects metrics, and Grafana visualizes them in dashboards.

### Microservices

Each microservice is deployed independently with a Horizontal Pod Autoscaler (HPA).

HPA automatically increases or decreases the number of pods based on CPU or memory usage.

This elasticity is essential in cloud-native systems to adapt to variable load.

### Kubernetes Components

Pod: the smallest deployable unit in Kubernetes, usually one microservice container.

Service: exposes a Pod internally or externally, ensuring stable networking.

Ingress: provides external access, with routing and TLS termination.

Persistent Volume Claim (PVC): ensures that database data survives Pod restarts.

Horizontal Pod Autoscaler (HPA): scales services up and down based on demand.

Together, these components make the system self-healing, scalable, and resilient.

### API Gateway Details

#### Security:

Authentication is based on JWT, and it is enforced only at the Gateway.

Microservices trust the Gateway’s validation, which ensures a clean separation of concerns.

Kubernetes secrets store sensitive data such as database credentials securely.

TLS and CORS are enforced at the ingress.

#### Resilience (Polly):

Retry Policy: failed requests are retried with exponential backoff, giving time to recover.

Circuit Breaker: when too many requests fail, the circuit “opens” and blocks calls temporarily, preventing overload on failing services.

Bulkhead Isolation: resources are partitioned so that one slow or failing service cannot block the others.

Rate Limiting:

Limits the number of requests per IP and globally, preventing overload or abuse.

This ensures fair resource usage and system stability.

#### Logging & Observability

NLog provides structured logs with timestamps and error details, useful for debugging and auditing.

Prometheus scrapes metrics from all services through the /metrics endpoint.

Grafana visualizes these metrics, allowing me to monitor latency, error rates, and scaling events in real time.

## Conclusion

This project demonstrates how to build a microservice architecture that is scalable, resilient, secure, and observable using Kubernetes and modern cloud-native patterns.

While I used Minikube and a shared database for demonstration purposes, in a real production environment this system could evolve towards per-service databases, a managed Kubernetes cluster, and a dedicated authentication service.
