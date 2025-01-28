# **Advanced LINQ Api Showcase**

## **Project Overview**

This project is a robust **Customer and Order Management System** that allows users to manage customer information and orders with authentication and authorization. It includes features for adding, updating, retrieving, and deleting customers and orders. Additionally, it has a role-based authorization system using JWT (JSON Web Token) for secure API access.

### **Key Features:**
- Customer management (CRUD operations)
- Order management (CRUD operations)
- Secure authentication with JWT
- Role-based authorization
- Error handling with custom middleware
- Caching with Redis for performance optimization
- Logging with Serilog
- Data validation with FluentValidation
- Swagger for API documentation and testing

---

## **Technologies Used**

- **ASP.NET Core 5**: The core framework used for building the web API.
- **Entity Framework Core**: ORM for interacting with the SQL database.
- **JWT (JSON Web Tokens)**: For secure authentication and authorization.
- **SQL Server**: Database for storing customer and order data.
- **Redis**: In-memory data store used for caching frequently accessed data to improve performance.
- **Serilog**: A logging library used to log system events and exceptions in a structured format.
- **FluentValidation**: A library used for validating user input data and ensuring business rules are followed.
- **Swagger**: For API documentation and testing.
- **Automapper**: For object-to-object mapping between different models.
- **Custom Middleware**: For handling global errors in the application.

---

## **Installation and Setup**

### **Prerequisites**

Before getting started, make sure you have the following installed:

- [Visual Studio 2019 or later](https://visualstudio.microsoft.com/)
- [.NET Core SDK 5.0 or later](https://dotnet.microsoft.com/download)
- [SQL Server or SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Redis](https://redis.io/) for caching (optional but recommended for performance)
  
### **Clone the Repository**

Clone this repository to your local machine using Git:

```bash
git clone https://github.com/your-username/project-repo-name.git
cd project-repo-name
