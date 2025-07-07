## Project 3: API Gateway
Design and implement a simple microservice application using the API gateway architecture in
order to support advanced non-functional properties such as load balancing and fault tolerance
and security.

During the demonstration it is needed to show the non-functional properties as well as the
functionalities provided thanks to the API gateway and the relative advantages in comparison with
other solutions.

### Project Idea:

The GateWay is the main desk of a public library. The library can handle four different, but connected, topics:
- **Books**
- **Loans**
- **Rooms**
- **Users**

For each topic we have a microservice and the main desk orquestrates the usage of them.

### kubernetes cmds:

- *minikube start* to start minikube on the local machine
- *kubectl get po -A* to see the status
- *minikube dashboard --url* to start the dashboard and see the url of it
- *kubectl apply -f kubernetes.yaml* effectively use the manifest 
- *kubectl get all -n cloudmare* see the status
- *minikube service apigateway -n cloudmare* start the node because of the "fake" load balancer support by mini-kube

