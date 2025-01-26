using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Models;
using AdvancedLINQApiShowcase.Services;
using Microsoft.EntityFrameworkCore;
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

        public OrderServiceTests(Mock<ICacheService> mockCacheService, Mock<DbSet<Order>> mockOrderSet, Mock<AppDbContext> mockContext)
        {
            _mockCacheService = mockCacheService;
            _mockOrderSet = mockOrderSet;
            _mockContext = mockContext;
        }

        [Fact]
        public async Task GetAllOrderAsync_Returns_OrdersFromCache_WhenCacheExists()
        {
            // Arrange field

            var cachedOrders = new List<Order>()
            {
                new Order { Id = 1, Name = "Order 1" },
                new Order { Id = 2, Name = "Order 2" }
            };

            _mockCacheService.Setup(c => c.GetData<IEnumerable<Order>>("Orders"))
                .Returns(cachedOrders);

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object, null);

            // Act field

            var result = await orderService.GetAllOrdersAsync();

            // Assert field

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
                             .Returns((IEnumerable<Order>)null);

            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(dbOrders.Provider);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(dbOrders.Expression);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(dbOrders.ElementType);
            _mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(dbOrders.GetEnumerator());

            _mockContext.Setup(db => db.Set<Order>()).Returns(_mockOrderSet.Object);

            var orderService = new OrderService(_mockContext.Object, _mockCacheService.Object,null);

            // Act
            var result = await orderService.GetAllOrdersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCacheService.Verify(c => c.GetData<IEnumerable<Order>>("Orders"), Times.Once);
            _mockCacheService.Verify(c => c.SetData("Orders", dbOrders), Times.Once);
            _mockContext.Verify(db => db.Set<Order>(), Times.Once);
        }
    }
}
