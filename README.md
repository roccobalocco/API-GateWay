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
- *kubectl apply -f kubernetes.yaml* effectively use the manifest 
- *kubectl get all -n cloudmare* see the status
- *minikube service apigateway -n cloudmare* // to start the service apigateay
- *kubectl get po -A* to see the status
- *minikube dashboard --url* to start the dashboard and see the url of it
- *minikube addons enable metrics-server*
- *minikube addons enable ingress*
- *kubectl -n cloudmare get ingress apigateway-ingress* // url to call to effectively use the load balancer

