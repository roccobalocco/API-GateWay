## Glossary:

**Minikube**:

A tool that allows you to set up a single-node k8s cluster on a local computer.

**Grafana**:

Open source platform for data analytics and visualization, allowing user to create interactive dashboards and metrics.

**Prometheus**:

Open source monitoring and alerting toolkit that serves as datasource for grafana.

**Polly**:

A library that helps handle failure and improve resilience. It permits to define and apply strategies such as Retry, Circuit Breaker, Rate Limiter, Timeout etc...

**HPA**:

HorizontalPodScaler is an automatical scaling tool that reacts to workload and adjusts the number of replicas in a workload to match observed resource utilization (CPU-Mem).

There is also a VPA (VerticalPodScaler), but while working with microservices that can run alone indipendently it is better to scale the number instead of the performance!

**PVC**:

PersistenceVolumeClaim is a k8s storage kind that permit to manage storage.

PVC is a k8s resource that reprents a request for a storage by a pod, specifying some requirements (size, access mode, etc...).

K8s use a PVC to find a PV that satisfies the PVC's requirements.

It is created by a pod to request storage from a PV, once a PVC is created it can be mounted as volume in a pod.

A PVC represent only the pod request and it is tied to the pod's lifecycle.

**SVC**:

A Service in k8s is an abstraction which defines a logical set of Pods and a policy by which to access them. 

Services enable a loose coupling between dependent pods.

A SVC can be of different types, such as, ClusterIP (expose service on internal IP in the cluster), NodePort (expose service on the same port of each selected Node in the cluster using NAT, making a service accessible from outside using NodeIP:NodePort), LoadBalancer (creates external load balancer in the current cloud assigning a fixed external IP to the service), etc...

**Ingress**:

Is a concept that lets you map traffic to different backends based on rules you define via k8s API.

Ingress exposes routes from outside the cluster to services within the cluster!

To be used you have to deploy an ingress controller such as ingress-nginx. You can then specificy ruls like the host, the list of paths etc...

**Namespace**:

It isolates groups of resources within a single cluster.

**Secret**:

Object used to store and manage sensitive infomation within a k8s cluster, helping prenvting the exposure of sensitive data by keeping it separate from application code and config files.

**Pod**:

The smallest deployable units of computing. Is a group of one or more containers with shared storage and net resources and a specification for how to run the containers.

A pod models an application spcific logical host.

It can run single o multiple containers that need to work togheter.

**Deployment**:

Object that gives declarative update to the applications. 

It describes the life cycle of the app, spcifying, for example, the images to use, the pod number and the update modalities.

It saves time to the update of the app, it executes an update process server side wihout interacting with the client.

It guarantess the execution and availability of a number of pods in every moment.

**Terraform**:

Used to manage k8s resources by utilizing the terraform k8s provider, which allows you to create and manage k8s objs using config files. 

It simplifies the management of Infrastructure as Code (azure, aws).

Platform-agnostic tool to manage your entire stack.

**Manifest**:

Config file writtend in yaml-json, used to describe the desired state of resurces in a k8s cluster.

Declarative way to tell k8s what you want your app and infrastructure to look like.

**Bulkhead**:

Design approach that isolates different components of an application to prevent failures in one part from affecting others.

In my system each microservice gets its own pool of 20 concurrent execution slots with 50 additional queueing slots.

**Circuit Breaker**:

Is a reactive resilience strategy that cuts the execution if the underlying resource is detected unhealthy. If the ratio of failure-success exceeds a threshold then the circuit break up to prevent overloading.

After some time the circuit performs a probe to check if the system has self-healed, the outcome of the probe decide if continue the block or let him go.

**Rate limiting**:

Is a proactive resilience strategy that controls the number of operations that can pass throught. Is a thin layer over API provided by the System.THreading.RateLimiting package. 

It can be used to control inbound load (via rate limiter) or outbound load (via concurrency limiter)

**Health checks**:
