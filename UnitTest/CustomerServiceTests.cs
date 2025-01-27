using AdvancedLINQApiShowcase.Caching;
using AdvancedLINQApiShowcase.DataAccess;
using AdvancedLINQApiShowcase.Models;
using Microsoft.EntityFrameworkCore;
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

        public CustomerServiceTests(Mock<ICacheService> mockCacheService, Mock<DbSet<Customer>> mockCustomerSet, Mock<AppDbContext> mockContext)
        {
            _mockCacheService = mockCacheService;
            _mockCustomerSet = mockCustomerSet;
            _mockContext = mockContext;
        }
    }

}
