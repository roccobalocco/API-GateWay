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
