## Project 3: API Gateway
Design and implement a simple microservice application using the API gateway architecture to support advanced non-functional properties such as load balancing and fault tolerance
and security.

During the demonstration it is necessary to show the non-functional properties as well as the
functionalities provided thanks to the API gateway and the relative advantages in comparison with
other solutions.

### Project Idea:

The GateWay is the main desk of a public library. The library can handle four different, but connected, topics:
- **Books**
- **Loans**
- **Rooms**
- **Users**

For each topic we have a microservice, and the main desk orchestrates the usage of them.

### kubernetes cmds:

- *minikube start* to start minikube on the local machine
- *kubectl apply -f kubernetes.yaml* effectively uses the manifest
- *kubectl get all -n cloudmare* see the status
- *minikube service apigateway -n cloudmare* to start the service apigateway
- *kubectl get po -A* to see the status
- *minikube dashboard --url* to start the dashboard and see the url of it
- *minikube addons enable metrics-server*
- *minikube addons enable ingress*
- *kubectl -n cloudmare get ingress apigateway-ingress* url to call to effectively use the load balancer

### Prometheus & Grafana cmds:

- *kubectl get pods -n cloudmare* to see all pods and their status
- *kubectl port-forward -n cloudmare svc/prometheus 9090:9090*  
  Access Prometheus UI at [http://localhost:9090](http://localhost:9090)
- *kubectl port-forward -n cloudmare svc/grafana 3000:3000*  
  Access Grafana UI at [http://localhost:3000](http://localhost:3000)  
  (default login: admin/admin, change password on first login)
- *kubectl get svc -n cloudmare* to list services including prometheus and grafana
- *kubectl logs -n cloudmare -l app=prometheus* to see Prometheus logs
- *kubectl logs -n cloudmare -l app=grafana* to see Grafana logs
- *kubectl top pods -n cloudmare* to check pod resource usage (requires metrics-server)
- *kubectl top nodes* to check node resource usage
- *kubectl exec -it -n cloudmare $(kubectl get pods -n cloudmare -l app=prometheus -o jsonpath="{.items[0].metadata.name}") -- /bin/sh*  
  Enter Prometheus pod shell for advanced diagnostics

### Minikube shortcuts for Prometheus and Grafana:

- *minikube service prometheus -n cloudmare --url* to get Prometheus service URL
- *minikube service grafana -n cloudmare --url* to get Grafana service URL

## Actual Limits of ApiGateway

The API Gateway applies both resilience policies for service-to-service communication and rate limiting policies for incoming client requests.

### Resilience Policies (Service Fault Tolerance)

For each microservice the dev can set custom limits, but the standard ones are:
```csharp
options.Retry.MaxRetryAttempts = 5;
options.Retry.BackoffType = DelayBackoffType.Exponential;
options.Retry.MaxDelay = TimeSpan.FromSeconds(2.5);
options.CircuitBreaker.FailureRatio = 0.05;
options.CircuitBreaker.MinimumThroughput = 75;
options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(35);
options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(6);
```

These settings define:
- **Retry logic**: Up to 5 attempts with exponential backoff, max 2.5 seconds between retries.
- **Circuit breaker**: Opens the circuit when 5% of requests fail within a sample of at least 75 requests in 35 seconds, breaking for 6 seconds.

A bulkhead policy is also applied:
```csharp
Policy.BulkheadAsync<HttpResponseMessage>(20, 50)
```

### Rate Limiting Policies (Client Throttling)

The API Gateway uses .NET 8's rate limiting middleware to control client-side request traffic.

`PerIpLimiter` Policy applies rate limiting per IP address:
- Permit Limit: 400 requests per minute
- Queue Limit: 100 requests
- Queue Order: Oldest requests first
```csharp
RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
  new FixedWindowRateLimiterOptions
  {
    PermitLimit = 400,
    Window = TimeSpan.FromMinutes(1),
    QueueLimit = 100,
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
  });
```
`FixedPolicy` Policy applies a global fixed window limit:
- Permit Limit: 800 requests per minute
- Queue Limit: 225 requests
- Queue Order: Oldest-first
```csharp
options.AddFixedWindowLimiter("FixedPolicy", opt =>
  {
    opt.PermitLimit = 800;
    opt.Window = TimeSpan.FromMinutes(1);
    opt.QueueLimit = 225;
    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
  });
```
These policies are applied globally to all controllers via:
```csharp
app.MapControllers()
  .RequireRateLimiting("FixedPolicy")
  .RequireRateLimiting("PerIpLimiter");\
```

## JWT Authentication

The API Gateway uses JWT (JSON Web Token) authentication with settings loaded from the `JWTOptions` section in configuration. The token is validated with the following rules:

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = jwtOptions.Issuer,
    ValidateAudience = true,
    ValidAudience = jwtOptions.Audience,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key)
};
```

These ensure:
- The token was issued by a trusted issuer
- The token is intended for the configured audience
- The expiration and not before times are respected
- The signature is verified using a symmetric key

## Health Checks
The API Gateway exposes two endpoints for health monitoring using the built-in health checks:
- **Liveness Probe**: Checks whether the application is alive (process up and responding)
- **Readiness Probe**: Checks whether the application is ready to serve traffic (e.g., DB migrations done)

They are mapped as:
```csharp
app.MapHealthChecks("/health/liveness", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/readiness", new HealthCheckOptions { Predicate = _ => true });
```