# API Gateway Demo

## Introduction

From the frontend application (developed specifically to test the resilience of my containers) you can login either as a **regular user** (without the privilege of viewing the health status of the microservices) or as an **administrator**.

The main (and only) page displays the general system status, retrieved from the health API, along with:

- A summary of the requests made by the currently logged-in user
- A testing section where you can choose which API and which microservice to invoke

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822142729281.png" alt="image-20250822142729281" style="zoom:70%;" />

## Usage example

After sending a high number of requests to the **Book** and **User** microservices:

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822111507676.png" alt="image-20250822111507676" style="zoom:40%;" />

We can observe that the **Horizontal Pod Autoscaler (HPA)** starts scaling the pods accordingly.

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822111359124.png" alt="image-20250822111359124" style="zoom:50%;" />

To stress-test the deployment, I sent **20,000 requests**, split between two different microservices.

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822111706109.png" alt="image-20250822111706109" style="zoom:50%;" />

As a result, many pods crashed due to workload intensity and existing policy configurations.

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822111934768.png" alt="image-20250822111934768" style="zoom:50%;" />

- The test on the **Room** service achieved a success rate of **17%**.
  <img src="/home/pietro/.config/Typora/typora-user-images/image-20250822112137939.png" alt="image-20250822112137939" style="zoom:50%;" />
- The test on the **User** service achieved a success rate of **14%**.
  <img src="/home/pietro/.config/Typora/typora-user-images/image-20250822112412029.png" alt="image-20250822112412029" style="zoom:50%;" />

Although 10,000 requests were sent to each microservice, only a small number were processed successfully. The remaining requests were blocked due to circuit-breaking policies, pod crashes, or other constraints.

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822112601763.png" alt="image-20250822112601763" style="zoom:50%;" />

After this test, the system adjusted dynamically:

- Since the latest requests targeted the **User** microservice, the HPA scaled up its number of pods.
- At the same time, the number of pods for the **Room** microservice was scaled down.

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822112824939.png" alt="image-20250822112824939" style="zoom:50%;" />

To trigger the **Per-IP** and **Global Fixed** policies in the Angular web application, you need to send many requests, as they are not sent simultaneously.

## Prometheus

Thanks to the annotations included in the manifest files, **Prometheus** can scrape the metrics of the active pods.

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822122343413.png" alt="image-20250822122343413" style="zoom:50%;" />

## Grafana

**Grafana** connects to Prometheus as a data source and visualizes various performance and health metrics of the system.

<img src="/home/pietro/.config/Typora/typora-user-images/image-20250822144331297.png" alt="image-20250822144331297" style="zoom:50%;" />