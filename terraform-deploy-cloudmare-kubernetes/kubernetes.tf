terraform {
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.19"
    }
  }
}

provider "kubernetes" {
  config_path    = "~/.kube/config"
  config_context = "myAKSCluster"
}

resource "kubernetes_namespace" "cloudmare" {
  metadata {
    name = "cloudmare"
  }
}

resource "kubernetes_secret" "db_connection" {
  metadata {
    name      = "db-connection"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  type = "Opaque"
  data = {
    Local = base64encode("Server=mssql;Database=Gateway;User Id=sa;Password=Cloudness1!;Encrypt=False;TrustServerCertificate=True")
  }
}

resource "kubernetes_persistent_volume_claim" "mssql_pvc" {
  metadata {
    name      = "mssql-pvc"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  spec {
    access_modes = ["ReadWriteOnce"]
    resources {
      requests = {
        storage = "5Gi"
      }
    }
    storage_class_name = "managed-csi"
  }
}

resource "kubernetes_deployment" "mssql" {
  metadata {
    name      = "mssql"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = "mssql"
      }
    }
    template {
      metadata {
        labels = {
          app = "mssql"
        }
      }
      spec {
        security_context {
          run_as_user = 0
        }
        container {
          name  = "mssql"
          image = "mcr.microsoft.com/mssql/server:2019-latest"
          port {
            container_port = 1433
          }
          env {
            name  = "ACCEPT_EULA"
            value = "Y"
          }
          env {
            name  = "SA_PASSWORD"
            value = "Cloudness1!"
          }
          env {
            name  = "MSSQL_PID"
            value = "Express"
          }
          resources {
            requests = {
              memory = "512Mi"
              cpu    = "250m"
            }
            limits = {
              memory = "512Mi"
              cpu    = "250m"
            }
          }
          liveness_probe {
            tcp_socket {
              port = 1433
            }
            initial_delay_seconds = 30
            period_seconds        = 15
            failure_threshold     = 3
          }
          readiness_probe {
            tcp_socket {
              port = 1433
            }
            initial_delay_seconds = 15
            period_seconds        = 10
            failure_threshold     = 3
          }
          volume_mount {
            name       = "mssql-storage"
            mount_path = "/var/opt/mssql"
          }
        }
        volume {
          name = "mssql-storage"
          persistent_volume_claim {
            claim_name = kubernetes_persistent_volume_claim.mssql_pvc.metadata[0].name
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "mssql" {
  metadata {
    name      = "mssql"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  spec {
    selector = {
      app = "mssql"
    }
    port {
      port        = 1433
      target_port = 1433
    }
  }
}

locals {
  microservices = [
    { name = "bookms", port = 8080, service_port = 80, prometheus_port = 8080 },
    { name = "userms", port = 8080, service_port = 81, prometheus_port = 8080 },
    { name = "loanms", port = 8080, service_port = 82, prometheus_port = 8080 },
    { name = "roomms", port = 8080, service_port = 83, prometheus_port = 8080 },
  ]
}

resource "kubernetes_deployment" "microservices" {
  for_each = { for ms in local.microservices : ms.name => ms }

  metadata {
    name      = each.value.name
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = each.value.name
      }
    }
    template {
      metadata {
        labels = {
          app = each.value.name
        }
      }
      spec {
        container {
          name  = each.value.name
          image = "roccobalocco/${each.value.name == "bookms" ? "book" : each.value.name == "userms" ? "user" : each.value.name == "loanms" ? "loan" : "room"}:latest"
          port {
            container_port = each.value.port
          }
          resources {
            requests = {
              cpu = "50m"
            }
            limits = {
              cpu = "100m"
            }
          }
          liveness_probe {
            http_get {
              path = "/health/liveness"
              port = each.value.port
            }
            initial_delay_seconds = 10
            period_seconds        = 15
            failure_threshold     = 3
          }
          readiness_probe {
            http_get {
              path = "/health/readiness"
              port = each.value.port
            }
            initial_delay_seconds = 5
            period_seconds        = 10
            failure_threshold     = 3
          }
          env {
            name = "ConnectionStrings__Local"
            value_from {
              secret_key_ref {
                name = kubernetes_secret.db_connection.metadata[0].name
                key  = "Local"
              }
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "microservices" {
  for_each = { for ms in local.microservices : ms.name => ms }

  metadata {
    name      = each.value.name
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
    annotations = {
      "prometheus.io/scrape" = "true"
      "prometheus.io/path"   = "/metrics"
      "prometheus.io/port"   = tostring(each.value.prometheus_port)
    }
  }
  spec {
    selector = {
      app = each.value.name
    }
    port {
      port        = each.value.service_port
      target_port = each.value.port
    }
  }
}

resource "kubernetes_horizontal_pod_autoscaler" "microservices_hpa" {
  for_each = { for ms in local.microservices : ms.name => ms }

  metadata {
    name      = "${each.value.name}-hpa"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  spec {
    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = each.value.name
    }
    min_replicas = 1
    max_replicas = 4
    metric {
      type = "Resource"
      resource {
        name = "cpu"
        target {
          type                = "Utilization"
          average_utilization = 20
        }
      }
    }
  }
}

resource "kubernetes_deployment" "apigateway" {
  metadata {
    name      = "apigateway"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  spec {
    replicas = 1
    selector {
      match_labels = {
        app = "apigateway"
      }
    }
    template {
      metadata {
        labels = {
          app = "apigateway"
        }
      }
      spec {
        container {
          name  = "apigateway"
          image = "roccobalocco/api-gateway:latest"
          port {
            container_port = 8080
          }
          resources {
            requests = {
              cpu = "50m"
            }
            limits = {
              cpu = "100m"
            }
          }
          env {
            name  = "MicroServicesOptions__Room"
            value = "http://roomms:83/api"
          }
          env {
            name  = "MicroServicesOptions__User"
            value = "http://userms:81/api"
          }
          env {
            name  = "MicroServicesOptions__Loan"
            value = "http://loanms:82/api"
          }
          env {
            name  = "MicroServicesOptions__Book"
            value = "http://bookms:80/api"
          }
          env {
            name  = "ASPNETCORE_ENVIRONMENT"
            value = "Release"
          }
          env {
            name = "ConnectionStrings__Local"
            value_from {
              secret_key_ref {
                name = kubernetes_secret.db_connection.metadata[0].name
                key  = "Local"
              }
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "apigateway" {
  metadata {
    name      = "apigateway"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
    annotations = {
      "prometheus.io/scrape" = "true"
      "prometheus.io/path"   = "/metrics"
      "prometheus.io/port"   = "8080"
    }
  }
  spec {
    type = "LoadBalancer"
    selector = {
      app = "apigateway"
    }
    port {
      port        = 80
      target_port = 8080
    }
  }
}

resource "kubernetes_ingress" "apigateway" {
  metadata {
    name      = "apigateway-ingress"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
    annotations = {
      "nginx.ingress.kubernetes.io/enable-cors"            = "true"
      "nginx.ingress.kubernetes.io/cors-allow-origin"      = "*"
      "nginx.ingress.kubernetes.io/cors-allow-methods"     = "GET, POST, PUT, DELETE, OPTIONS"
      "nginx.ingress.kubernetes.io/cors-allow-headers"     = "Authorization, Content-Type"
      "nginx.ingress.kubernetes.io/cors-allow-credentials" = "true"
      "nginx.ingress.kubernetes.io/load-balance"           = "round_robin"
    }
  }

  spec {
    backend {
      service_name = kubernetes_service.apigateway.metadata[0].name
      service_port = 80
    }

    rule {
      http {
        path {
          path = "/"
          backend {
            service_name = kubernetes_service.apigateway.metadata[0].name
            service_port = 80
          }
        }
      }
    }
  }
}

resource "kubernetes_config_map" "prometheus_config" {
  metadata {
    name      = "prometheus-config"
    namespace = kubernetes_namespace.cloudmare.metadata[0].name
  }
  data = {
    "prometheus.yml" = <<EOF
global:
  scrape_interval: 15s
scrape_configs:
  - job_name: 'kubernetes-pods'
    kubernetes_sd_configs:
      - role: pod
    relabel_configs:
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
        action: replace
        target_label: __metrics_path__
        regex: (.+)
      - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
        action: replace
        regex: ([^:]+)(?::\d+)?;(\d+)
        replacement: $1:$2
        target_label: __address__
EOF
  }
}
