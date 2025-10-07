# Overview
This project is a distributed system built using multiple services including a .NET Server, RabbitMQ, MS SQL Server, Prometheus, and Grafana. 
The system performs reliable asynchronous processing using RabbitMQ and exposes a REST API and gRPC services. 
The project is designed to process a series of requests related to "addition" and maintain the sum, integrating monitoring and observability tools like Prometheus and Grafana.

# Architecture
<img width="1818" height="794" alt="architecture" src="https://github.com/user-attachments/assets/8d6762d7-742f-43c8-b1c3-4b26fe342747" />

# Services Overview
- **RabbitMQ**: Messaging broker that handles asynchronous communication between services.
- **MSSQLSERVER (db)**: Database for storing application data.
- **.NET gRPC**: Implements gRPC service for adding values and communicates with RabbitMQ.
- **.NET REST API**: REST API service that gets the sum via HTTP requests.
- **Prometheus**: Collects and stores metrics from the services for monitoring.
- **Grafana**: Provides a dashboard for visualizing the metrics stored in Prometheus.

# Features Implemented
- **gRPC-based Addition Service**: Built a gRPC service in .NET for addition operations, where messages are processed asynchronously with RabbitMQ.
- **RabbitMQ Integration**: RabbitMQ is used for decoupling communication between services and ensuring reliable message processing.
- **MSSQLSERVER**: Database used for storing the state, with gRPC background server that read the data and publish them to rabbitMQ
- **Outbox Pattern**: Implemented the Outbox Pattern to ensure reliable message delivery by storing the message in a separate table (outboxMessages) within the database, which is later picked up by the message consumer (RabbitMQ).
- **Prometheus + Grafana**: Integrated Prometheus for monitoring request durations, and system health, and Grafana for visualizing metrics in real-time.
- **Docker Compose**: Used Docker Compose to run all services locally and manage dependencies between them.

# Dashboard
<img width="1919" height="933" alt="Screenshot 2025-10-07 060548" src="https://github.com/user-attachments/assets/369ae2b4-a14f-4b82-9314-5cd0cdfdaa45" />

# Technologies Used
- **.NET** for implementing the gRPC service and the REST API service.
- **RabbitMQ** for messaging.
- **MSSQLSERVER** as the primary database.
- **Outbox Pattern** for reliable message delivery.
- **Prometheus** for metrics collection and monitoring.
- **Grafana** for visualization of Prometheus metrics.
- **Docker Compose** for local development and orchestration of services.

# Project Setup
## Docker Compose (for Local Development)
Docker Compose was used to define services such as RabbitMQ, MSSQLSERVER, .NET server, Prometheus, and Grafana.

## Monitoring and Observability
- Integrated Prometheus for tracking service metrics like request durations, CPU usage, and queue times.
- Set up Grafana to visualize metrics collected by Prometheus.
- Custom Prometheus metrics were created, such as `queue_waiting_time_seconds` to measure queue waiting time, and `whole_request_time_soconds` to whole request time

## Running Locally with Docker Compose
1. **Install Docker** (if not already installed):
    - Follow the instructions for your operating system to install Docker from the official Docker website.

2. **Start the Services**:
   
    - Navigate to the project directory.
    - Run the following command to start all services:
      ```bash
      docker-compose up -d
      ```

4. **Verify the Services**:

    - RabbitMQ will be available at [http://localhost:15672](http://localhost:15672).
    - grpc server will be available at [http://localhost:50051](http://localhost:5122).
    - REST server will be accessible at [http://localhost:3001](http://localhost:5138).
    - Prometheus will be available at [http://localhost:9090](http://localhost:9090).
    - Grafana will be available at [http://localhost:3000](http://localhost:3000) (default login: `admin/admin`).

## Load Testing with K6
  - **gRPC Server Load Testing**: Used K6 to simulate load for gRPC calls to the Add endpoint, ramping up users to 15000
    ```bash
    docker-compose run --rm k6 run grpc-test.js
    ```

# Key Observations and Performance Metrics
- **Queue Waiting Time**: Measured the time between receiving a message in the RabbitMQ queue and processing it.
- **Whole Request Duration**: Monitored the duration of gRPC and HTTP requests.
- **CPU Usage**: Used Prometheus to track CPU usage and system performance.
- **Processed Message**: Findout the number of processed messages
