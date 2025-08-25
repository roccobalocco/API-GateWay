kubectl apply -f kill-a-pod.yaml
kubectl apply -f cpu-stress.yaml

sleep 30s && kubectl get podchaos,networkchaos,stresschaos,iochaos -n cloudmare

