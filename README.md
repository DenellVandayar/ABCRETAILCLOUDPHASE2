
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
<img width="1366" height="768" alt="Screenshot 2024-09-28 152928" src="https://github.com/user-attachments/assets/749a376f-ca6e-427f-aeb1-2a80d86f0cdb" />
<img width="1366" height="768" alt="Screenshot 2024-09-28 153217" src="https://github.com/user-attachments/assets/b15c2235-2bc7-49f8-94ba-25931699d4ad" />
<img width="1366" height="768" alt="Screenshot 2024-09-28 153245" src="https://github.com/user-attachments/assets/0e065551-1fc9-46af-99c2-288e2f733c0e" />
<img width="1366" height="768" alt="Screenshot 2024-09-28 153319" src="https://github.com/user-attachments/assets/497c7137-d35f-4067-a123-ac8b3aff1116" />


### Azure Storage Services
<img width="1366" height="768" alt="Screenshot 2024-08-29 121454" src="https://github.com/user-attachments/assets/c65e0b41-0534-47a5-b935-796ed9187f13" />
<img width="1366" height="768" alt="Screenshot 2024-08-29 121534" src="https://github.com/user-attachments/assets/861cd58e-eb17-415b-9aa8-d1e54244d5e0" />
<img width="1366" height="768" alt="Screenshot 2024-08-29 123457" src="https://github.com/user-attachments/assets/258c0e80-683d-4dfa-9c12-5fcc2cdfa4df" />


### Application UI
<img width="1366" height="625" alt="Screenshot 2024-08-29 122443" src="https://github.com/user-attachments/assets/242d876e-1a29-4264-be1f-14f56508b7ec" />
<img width="1366" height="625" alt="Screenshot 2024-08-29 120550" src="https://github.com/user-attachments/assets/b5492be8-589b-4024-b61b-b7f0db81a7a7" />


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
