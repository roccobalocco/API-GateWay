# Checklist del progetto: API Gateway e Kubernetes

## Pianificazione e Architettura

- [x]  Definire i domini dell’applicazione (Books, Loans, Rooms, Users) e progettare microservizi indipendenti per ciascuno.
- [x] Utilizzare .NET 8.0 per implementare i microservizi e l’**API Gateway** (ad esempio Ocelot come gateway API leggero).
- [x] Creare una singola istanza Docker per MSSQL Server come database condiviso e configurarla con persistenza.
- [x] Pianificare l’architettura su Kubernetes, assicurando replicazione delle istanze e bilanciamento del carico.
- [ ] Documentare tutte le scelte architetturali, includendo diagrammi di flusso e sequenza delle chiamate API.

## Microservizi e Database

- [x] Implementare i microservizi ASP.NET Core (CRUD) per Books, Loans, Rooms e Users, ognuno containerizzato con Docker.
- [x] Configurare i servizi con connessioni al database MSSQL condiviso (stringhe di connessione, migrazioni).
- [ ] Implementare endpoint di salute (*health checks*) in ogni servizio per readiness e liveness.
- [ ] Applicare pattern di resilienza (circuit breaker, retry) tra i servizi per tolleranza ai guasti.
- [ ] Verificare che ogni servizio funzioni indipendentemente e sia in grado di scalare orizzontalmente (aumentando il numero di repliche).

## API Gateway

- [x] Scegliere e configurare l’API Gateway (ad es. Ocelot) per instradare le richieste alle API dei microservizi.
- [ ] Implementare logica di routing e versioning nel gateway, eventualmente con più istanze gateway dedicate a funzioni diverse.
- [ ] Abilitare l’autenticazione centralizzata via token JWT (es. IdentityServer) a livello di gateway.
- [ ] Configurare l’autorizzazione (claim/ruoli) nelle rotte protette del gateway.
- [ ] Abilitare HTTPS/TLS per le richieste esterne al gateway, garantendo connessioni criptate.
- [ ] Implementare rate limiting e throttling sul gateway per prevenire attacchi DDoS e gestire correttamente i picchi di traffico.
- [ ] Attivare logging e analisi del traffico API sul gateway per monitorare utilizzo e anomalie.
- [ ] Distribuire più istanze dell’API Gateway in Kubernetes per alta disponibilità (evitare single point of failure).

## Kubernetes e Deployment

- [ ] Scrivere manifest Kubernetes (Deployment, Service) per ogni microservizio e per il gateway.
- [ ] Esportare il gateway all’esterno del cluster con un Service di tipo *LoadBalancer* o configurando un Ingress (NGINX).
- [ ] Configurare un Ingress Controller Kubernetes per instradare il traffico esterno verso il gateway (regole host/path).
- [ ] Impostare ReplicaSet/HPA (Horizontal Pod Autoscaler) per scalare automaticamente i pod in base al carico.
- [ ] Testare il failover: arrestare un pod e verificare che Kubernetes instradi il traffico su pod sani.
- [ ] Eseguire test di scalabilità orizzontale: simulare carichi crescenti e verificare l’aumento di repliche e il bilanciamento del traffico.

## Resilienza e Fault Tolerance

- [ ] Configurare *readiness* e *liveness probes* per rilevare e rimuovere automaticamente i pod non funzionanti.
- [ ] Assicurarsi che il bilanciatore di carico Kubernetes reindirizzi automaticamente il traffico da pod malfunzionanti a quelli sani.
- [ ] Implementare meccanismi di failover e replica (ad es. più repliche dei servizi) per garantire ridondanza.
- [ ] (Opzionale) Valutare l’adozione di un service mesh (Istio, Linkerd) per avanzato bilanciamento interno e comunicazioni sicure end-to-end.

## Sicurezza e Monitoraggio

- [ ] Fornire comunicazioni sicure HTTPS/TLS su tutti i livelli (gateway e servizi).
- [ ] Implementare autenticazione e autorizzazione centralizzate nell’API Gateway.
- [ ] Abilitare rate limiting nel gateway per mitigare traffico anomalo e resistere a tentativi di DoS.
- [ ] Configurare logging centralizzato e metriche (es. ELK, Prometheus/Grafana) per tutti i servizi.
- [ ] Eseguire simulazioni di attacco (es. DDoS di test) per validare le protezioni del sistema.

## Documentazione e Demo

- [ ] Preparare un report dettagliato (<5 pagine) con architettura, scelte tecniche e risultati raggiunti.
- [ ] Includere diagrammi (architettura di sistema, flusso API, sequenze delle chiamate).
- [ ] Documentare i test di carico, failover e sicurezza e i risultati ottenuti.
- [ ] Preparare slide per l’orale, evidenziando come il gateway migliora la sicurezza e il bilanciamento del carico (fault tolerance) delle API.
- [ ] Effettuare una demo live: mostrare bilanciamento del carico, resilienza (failover di pod) e sicurezza (accesso con token).