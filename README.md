
# ABC Retail – Azure Cloud Web Application (Phase 2)

## Overview
This repository represents **Phase 2** of the ABC Retail cloud-based e-commerce application.

The focus of this phase was to enhance the system with **serverless computing using Azure Functions**, enabling automated background processing, improved scalability, and decoupled transaction workflows.

---

## Technologies Used

### Application
- ASP.NET Core MVC
- C#
- REST-based architecture

### Cloud & Serverless (Microsoft Azure)
- Azure App Service
- Azure Functions
- Azure Queue Storage
- Azure Table Storage
- Azure Blob Storage
- Azure File Storage

### Tools
- Visual Studio
- Azure Portal
- Git & GitHub

---

## Architecture Overview

This phase introduces a **serverless, event-driven architecture**, where Azure Functions process background tasks independently of the web application.

### Key Design Principles
- Decoupling frontend logic from backend processing
- Event-driven workflows using Azure Queues
- Automatic scaling and cost efficiency via serverless execution

---

## Azure Functions – Responsibilities

Azure Functions were implemented to automate and manage backend operations, including:

- Processing order messages triggered by Azure Queue Storage
- Synchronizing data across Azure Storage services
- Handling background transaction workflows
- Improving system reliability and fault tolerance

---

## Application Features

### Serverless Order Processing
- Orders placed by users are added to Azure Queue Storage
- Azure Functions are triggered automatically to process orders
- Backend logic executes independently from user requests

### Storage Integration
- Product and customer data stored in Azure Table Storage
- Images hosted in Azure Blob Storage
- Documents and logs stored in Azure File Storage

### Scalability & Reliability
- Serverless execution removes the need for manual infrastructure management
- Queue-based processing improves system resilience during high load

---

## Screenshots

### Azure Functions
![Azure Functions](screenshots/azure-functions.png)

### Azure Queue Trigger
![Queue Trigger](screenshots/queue-trigger.png)

### Azure Storage Services
![Blob Storage](screenshots/blob-storage.png)
![Table Storage](screenshots/table-storage.png)
![Queue Storage](screenshots/queue-storage.png)

### Application UI
![Catalog Page](screenshots/catalog-page.png)
![Cart Page](screenshots/cart-page.png)

---

## Learning Outcomes
- Serverless computing with Azure Functions
- Event-driven cloud architectures
- Asynchronous message processing
- Improved scalability and reliability
- Real-world cloud automation patterns

---

## Project Evolution
This repository represents **Phase 2** of the project.

- Phase 1: Azure Storage Services  
- Phase 3: Azure SQL Database with geo-replication (final implementation)

---

## Author
**Denell Vandayar**  
Bachelor of Computer and Information Sciences – Application Development  

GitHub: https://github.com/DenellVandayar
