terraform {
  required_providers {
	google = {
	  source  = "hashicorp/google"
	  version = "~> 5.0"
	}
  }
  required_version = ">= 1.4.0"
}

provider "google" {
  project = var.project_id
  region  = var.region
}

resource "google_project_service" "enabled" {
	for_each = toset([
		"run.googleapis.com",
		"artifactregistry.googleapis.com",
		"cloudbuild.googleapis.com",
		"iam.googleapis.com",
		])
		service = each.key
}

resource "google_artifact_registry_repository" "app_repo" {
	location = var.region
	repository_id = "todoapp"
	format = "DOCKER"
	description = "Docker repo for Todo App"
}

resource "google_cloud_run_service" "api" {
  name     = "todoapp-api"
  location = var.region

  template {
    spec {
      containers {
        image = "REGION-docker.pkg.dev/${var.project_id}/todoapp/todoapp-api"
        ports {
          container_port = 80
        }
      }
    }
  }

  traffic {
	percent         = 100
	latest_revision = true
  }

    autogenerate_revision_name = true
}

resource "google_cloud_run_service" "web" {
  name     = "todoapp-web"
  location = var.region
  template {
	spec {
	  containers {
		image = "REGION-docker.pkg.dev/${var.project_id}/todoapp/todoapp-web"
		ports {
		  container_port = 80
		}
	  }
	}
  }
  traffic {
	percent         = 100
	latest_revision = true
  }
	autogenerate_revision_name = true
}

resource "google_cloud_run_service_iam_member" "web_invoker" {
  service = google_cloud_run_service.web.name
  location = var.region
  role    = "roles/run.invoker"
  member  = "allUsers"
}

resource "google_cloud_run_service_iam_member" "api_invoker" {
  service = google_cloud_run_service.api.name
  location = var.region
  role    = "roles/run.invoker"
  member  = "allUsers"
}