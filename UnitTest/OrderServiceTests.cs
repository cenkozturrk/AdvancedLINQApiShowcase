using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Interfaces;
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
    public class OrderServiceTests
    {
        private Mock<ICacheService> _mockCacheService;
        private Mock<DbSet<Order>> _mockOrderSet;
        private Mock<AppDbContext> _mockContext;
        private Mock<ILogger<OrderService>> _mockLogger; 


        public OrderServiceTests(Mock<ICacheService> mockCacheService, Mock<DbSet<Order>> mockOrderSet, Mock<AppDbContext> mockContext, Mock<ILogger<OrderService>> mockLogger)
        {
            _mockCacheService = mockCacheService;
            _mockOrderSet = mockOrderSet;
            _mockContext = mockContext;
            _mockLogger = mockLogger;
        }

        [Fact]
        public async Task GetOrderByIdAsync_Returns_Order_WhenExists()  
        {
            // Arrange
            var dbOrder = new Order { Id = 1, Name = "Order 1" };
            _mockOrderSet.Setup(m => m.FindAsync(1)).ReturnsAsync(dbOrder);

            _mockContext.Setup(db => db.Set<Order>()).Returns(_mockOrderSet.Object);

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);  

            // Act
            var result = await orderService.GetOrderByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Order 1", result.Name);
        }
        [Fact]
        public async Task GetAllOrderAsync_Returns_OrdersFromCache_WhenCacheExists()
        {      
            // Arrange 
            var cachedOrders = new List<Order>()
            {
                new Order { Id = 1, Name = "Order 1" },
                new Order { Id = 2, Name = "Order 2" }
            };

            _mockCacheService.Setup(c => c.GetData<IEnumerable<Order>>("Orders"))
                .Returns(cachedOrders);

            OrderService orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act 
            var result = await orderService.GetAllOrdersAsync();

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCacheService.Verify(c => c.GetData<IEnumerable<Order>>("Orders"), Times.Once);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsOrdersFromDbAndCaches_WhenCacheMiss()
        {
            // Arrange
            var dbOrders = new List<Order>
        {
            new Order { Id = 1, Name = "Order 1" },
            new Order { Id = 2, Name = "Order 2" }
        }.AsQueryable();

            _mockCacheService.Setup(c => c.GetData<IEnumerable<Order>>("Orders"))
                  .Returns(Enumerable.Empty<Order>());


            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(dbOrders.Provider);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(dbOrders.Expression);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(dbOrders.ElementType);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(dbOrders.GetEnumerator());

            _mockContext.Setup(db => db.Set<Order>()).Returns(_mockOrderSet.Object);

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            var result = await orderService.GetAllOrdersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCacheService.Verify(c => c.GetData<IEnumerable<Order>>("Orders"), Times.Once);
            _mockCacheService.Verify(c => c.SetData("Orders", dbOrders), Times.Once);
            _mockContext.Verify(db => db.Set<Order>(), Times.Once);
        }
        [Fact]
        public async Task GetOrderByIdAsync_ReturnsOrderFromCache_WhenCacheExists()
        {
            // Arrange
            var cachedOrder = new Order { Id = 1, Name = "Order 1" };

            _mockCacheService.Setup(c => c.GetData<Order>("Order_1"))
                 .Returns(() => null); 

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            var result = await orderService.GetOrderByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Order 1", result.Name);
            _mockCacheService.Verify(c => c.GetData<Order>("Order_1"), Times.Once);
            _mockContext.Verify(db => db.Set<Order>(), Times.Never); 
        }

        [Fact]
        public async Task GetOrderByIdAsync_ReturnsOrderFromDbAndCaches_WhenCacheMiss()
        {
            // Arrange
            var dbOrder = new Order { Id = 1, Name = "Order 1" };

            _mockCacheService.Setup(c => c.GetData<Order>("Order_1"))
                 .Returns(() => null); 


            _mockOrderSet.Setup(m => m.FindAsync(1)).ReturnsAsync(dbOrder);
            _mockContext.Setup(db => db.Set<Order>()).Returns(_mockOrderSet.Object);

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            var result = await orderService.GetOrderByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Order 1", result.Name);
            _mockCacheService.Verify(c => c.GetData<Order>("Order_1"), Times.Once);
            _mockCacheService.Verify(c => c.SetData("Order_1", dbOrder), Times.Once);
            _mockContext.Verify(db => db.Set<Order>(), Times.Once);
        }

        [Theory]
        [InlineData(1, "Order 1")]
        [InlineData(2, "Order 2")]
        [InlineData(3, "Order 3")]
        public async Task GetOrderByIdAsync_Returns_Order(int id, string expectedName)
        {
            // Arrange
            var order = new Order { Id = id, Name = expectedName };
            _mockOrderSet.Setup(m => m.FindAsync(id)).ReturnsAsync(order);

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            var result = await orderService.GetOrderByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedName, result.Name);
        }
        [Fact]
        public async Task CreateOrderAsync_ShouldAddOrderToDbAndCache()
        {
            // Arrange
            var newOrder = new Order
            {
                Id = 3,
                Name = "Order 3",
                OrderDate = DateTime.Now,
                CustomerId = 1
            };

            // Mock DB Context to simulate the 'Add' method behavior
            var dbOrders = new List<Order> { newOrder }.AsQueryable();
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(dbOrders.Provider);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(dbOrders.Expression);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(dbOrders.ElementType);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(dbOrders.GetEnumerator());

            _mockContext.Setup(db => db.Set<Order>()).Returns(_mockOrderSet.Object);

            // Mock Cache to simulate setting data
            _mockCacheService.Setup(c => c.SetData("Orders", It.IsAny<IEnumerable<Order>>()))
                             .Verifiable();

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            await orderService.AddOrderAsync(newOrder);

            // Assert
            _mockCacheService.Verify(c => c.SetData("Orders", It.IsAny<IEnumerable<Order>>()), Times.Once);
            _mockContext.Verify(db => db.Set<Order>(), Times.Once);
        }
        [Fact]
        public async Task UpdateOrderAsync_ShouldUpdateOrderInDbAndCache()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                Name = "Order 1",
                OrderDate = DateTime.Now,
                CustomerId = 1
            };

            var updatedOrder = new Order
            {
                Id = 1,
                Name = "Updated Order 1",
                OrderDate = DateTime.Now,
                CustomerId = 1
            };

            // Mock DB Context to simulate fetching the existing order
            var dbOrders = new List<Order> { existingOrder }.AsQueryable();
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(dbOrders.Provider);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(dbOrders.Expression);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(dbOrders.ElementType);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(dbOrders.GetEnumerator());

            _mockContext.Setup(db => db.Set<Order>()).Returns(_mockOrderSet.Object);

            // Mock Cache service to ensure the order gets updated in the cache
            _mockCacheService.Setup(c => c.SetData("Orders", It.IsAny<IEnumerable<Order>>()))
                             .Verifiable(); // Verifies the cache Set method is called

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);   

            // Act
            await orderService.UpdateOrderAsync(updatedOrder);

            // Assert
            _mockContext.Verify(db => db.Set<Order>(), Times.Once);
            _mockCacheService.Verify(c => c.SetData("Orders", It.IsAny<IEnumerable<Order>>()), Times.Once);
            Assert.Equal("Updated Order 1", updatedOrder.Name);
        }
        [Fact]
        public async Task DeleteOrderAsync_ShouldDeleteOrderFromDbAndRefreshCache()
        {
            // Arrange
            var orderToDelete = new Order
            {
                Id = 1,
                Name = "Order to Delete",
                OrderDate = DateTime.Now,
                CustomerId = 1
            };

            var dbOrders = new List<Order> { orderToDelete }.AsQueryable();

            // Mock DB Context to simulate fetching the order
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(dbOrders.Provider);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(dbOrders.Expression);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(dbOrders.ElementType);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(dbOrders.GetEnumerator());

            _mockContext.Setup(db => db.Set<Order>()).Returns(_mockOrderSet.Object);

            // Mock cache refresh 
            _mockCacheService.Setup(c => c.SetData("Orders", It.IsAny<IEnumerable<Order>>())).Verifiable(); 

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, _mockLogger.Object);

            // Act
            await orderService.DeleteOrderAsync(orderToDelete.Id);

            // Assert
            _mockContext.Verify(db => db.Set<Order>(), Times.Once); 
            _mockCacheService.Verify(c => c.SetData("Orders", It.IsAny<IEnumerable<Order>>()), Times.Once); 
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); 
        }


    }

}
