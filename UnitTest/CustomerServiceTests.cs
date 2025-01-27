using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class CustomerServiceTests
    {
        private Mock<ICacheService> _mockCacheService;
        private Mock<DbSet<Customer>> _mockCustomerSet;
        private Mock<AppDbContext> _mockContext;
        private Mock<ILogger<CustomerService>> _mockLogger;
        public CustomerServiceTests(Mock<ICacheService> mockCacheService, Mock<DbSet<Customer>> mockCustomerSet, Mock<AppDbContext> mockContext, Mock<ILogger<CustomerService>> mockLogger)
        {
            _mockCacheService = mockCacheService;
            _mockCustomerSet = mockCustomerSet;
            _mockContext = mockContext;
            _mockLogger = mockLogger;
        }

        // Test method 1: Get customer by id
        [Fact]
        public async Task GetCustomerByIdAsync_Returns_Customer_WhenExists()
        {
            // Arrange
            var dbCustomer = new Customer { Id = 1, Name = "Customer 1" };
            _mockCustomerSet.Setup(m => m.FindAsync(1)).ReturnsAsync(dbCustomer);

            _mockContext.Setup(db => db.Set<Customer>()).Returns(_mockCustomerSet.Object);

            var customerService = new CustomerService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);  // Pass logger here

            // Act
            var result = await customerService.GetCustomerByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Customer 1", result.Name);
        }
        // Test method 2: Get all customers from cache if they exist
        [Fact]
        public async Task GetAllCustomersAsync_Returns_CustomersFromCache_WhenCacheExists()
        {
            // Arrange
            var cachedCustomers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Customer 1" },
                new Customer { Id = 2, Name = "Customer 2" }
            };

            _mockCacheService.Setup(c => c.GetData<IEnumerable<Customer>>("Customers"))
                             .Returns(cachedCustomers);

            CustomerService customerService = new CustomerService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            var result = await customerService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCacheService.Verify(c => c.GetData<IEnumerable<Customer>>("Customers"), Times.Once);
        }

        // Test method 3: Get all customers from DB and cache them when cache misses
        [Fact]
        public async Task GetAllCustomersAsync_Returns_CustomersFromDb_WhenCacheMiss()
        {
            // Arrange
            var dbCustomers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Customer 1" },
                new Customer { Id = 2, Name = "Customer 2" }
            }.AsQueryable();

            _mockCacheService.Setup(c => c.GetData<IEnumerable<Customer>>("Customers"))
                             .Returns(Enumerable.Empty<Customer>());

            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(dbCustomers.Provider);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(dbCustomers.Expression);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(dbCustomers.ElementType);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(dbCustomers.GetEnumerator());

            _mockContext.Setup(db => db.Set<Customer>()).Returns(_mockCustomerSet.Object);

            var customerService = new CustomerService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);  // Pass logger here

            // Act
            var result = await customerService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCacheService.Verify(c => c.GetData<IEnumerable<Customer>>("Customers"), Times.Once);
            _mockCacheService.Verify(c => c.SetData("Customers", dbCustomers), Times.Once);
            _mockContext.Verify(db => db.Set<Customer>(), Times.Once);
        }

        // Test method 4: Create customer
        [Fact]
        public async Task CreateCustomerAsync_ShouldAddCustomerToDbAndCache()
        {
            // Arrange
            var newCustomer = new Customer
            {
                Id = 3,
                Name = "Customer 3",
                Email = "customer3@example.com"
            };

            var dbCustomers = new List<Customer> { newCustomer }.AsQueryable();
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(dbCustomers.Provider);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(dbCustomers.Expression);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(dbCustomers.ElementType);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(dbCustomers.GetEnumerator());

            _mockContext.Setup(db => db.Set<Customer>()).Returns(_mockCustomerSet.Object);

            _mockCacheService.Setup(c => c.SetData("Customers", It.IsAny<IEnumerable<Customer>>()))
                             .Verifiable();

            var customerService = new CustomerService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            await customerService.AddCustomerAsync(newCustomer);

            // Assert
            _mockCacheService.Verify(c => c.SetData("Customers", It.IsAny<IEnumerable<Customer>>()), Times.Once);
            _mockContext.Verify(db => db.Set<Customer>(), Times.Once);
        }

        // Test method 5: Update customer
        [Fact]
        public async Task UpdateCustomerAsync_ShouldUpdateCustomerInDbAndCache()
        {
            // Arrange
            var existingCustomer = new Customer
            {
                Id = 1,
                Name = "Customer 1",
                Email = "customer1@example.com",
            };

            var updatedCustomer = new Customer
            {
                Id = 1,
                Name = "Updated Customer 1",
                Email = "updatedcustomer1@example.com",
            };

            var dbCustomers = new List<Customer> { existingCustomer }.AsQueryable();
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(dbCustomers.Provider);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(dbCustomers.Expression);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(dbCustomers.ElementType);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(dbCustomers.GetEnumerator());

            _mockContext.Setup(db => db.Set<Customer>()).Returns(_mockCustomerSet.Object);

            _mockCacheService.Setup(c => c.SetData("Customers", It.IsAny<IEnumerable<Customer>>()))
                             .Verifiable(); 

            var customerService = new CustomerService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            await customerService.UpdateCustomerAsync(updatedCustomer);

            // Assert
            _mockContext.Verify(db => db.Set<Customer>(), Times.Once);
            _mockCacheService.Verify(c => c.SetData("Customers", It.IsAny<IEnumerable<Customer>>()), Times.Once);
            Assert.Equal("Updated Customer 1", updatedCustomer.Name);
            Assert.Equal("updatedcustomer1@example.com", updatedCustomer.Email);
        }

        // Test method 6: Delete customer
        [Fact]
        public async Task DeleteCustomerAsync_ShouldDeleteCustomerFromDbAndRefreshCache()
        {
            // Arrange
            var customerToDelete = new Customer
            {
                Id = 1,
                Name = "Customer to Delete",
                Email = "customer@example.com",
            };

            var dbCustomers = new List<Customer> { customerToDelete }.AsQueryable();

            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(dbCustomers.Provider);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(dbCustomers.Expression);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(dbCustomers.ElementType);
            _mockCustomerSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(dbCustomers.GetEnumerator());

            _mockContext.Setup(db => db.Set<Customer>()).Returns(_mockCustomerSet.Object);

            _mockCacheService.Setup(c => c.SetData("Customers", It.IsAny<IEnumerable<Customer>>()))
                             .Verifiable();

            var customerService = new CustomerService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            await customerService.DeleteCustomerByIdAsync(customerToDelete.Id);

            // Assert
            _mockContext.Verify(db => db.Set<Customer>(), Times.Once); 
            _mockCacheService.Verify(c => c.SetData("Customers", It.IsAny<IEnumerable<Customer>>()), Times.Once); 
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); 
        }

    }
}
