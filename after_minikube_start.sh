#!/bin/bash

sleep 10s

minikube addons enable ingress metrics-server

cd ~/RiderProjects/api-gateway

kubectl apply -f kubernetes.yaml

minikube service apigateway -n cloudmare &

(minikube dashboard --url > dashboard.out) &

(kubectl -n cloudmare get ingress apigateway-ingress > ingress.out) &

kubectl port-forward -n cloudmare svc/prometheus 9090:9090 &

kubectl port-forward -n cloudmare svc/grafana 3000:3000 &

sleep 10s

(minikube service prometheus -n cloudmare --url > prometheus_url.out) &

(minikube service grafana -n cloudmare --url > grafana_url.out) &
