using CoreBanking.API.Apis;
using CoreBanking.API.Models;
using CoreBanking.API.Services;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreBanking.UnitTests
{
    public class CoreBankingUnitTests
    {
        private SqliteConnection _connection = default!;
        private DbContextOptions<CoreBankingDbContext> _contextOptions = default!;
        [Fact]
        public async Task Create_Customer_Test()
        {
            // Arrange
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();

                var services = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);

                var customer = new Customer()
                {
                    Id = Guid.NewGuid(),
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };

                // Act
                var result = await CoreBankingApi.CreateCustomers(services, customer);

                // Assert
                Assert.NotNull(result);

                // Check if the customer was added to the database
                var customerFromDb = await context.Customers.FindAsync(customer.Id);
                Assert.NotNull(customerFromDb);
                Assert.Equal(customer.Name, customerFromDb.Name);
                Assert.Equal(customer.Address, customerFromDb.Address);
                Assert.Equal(customer.Id, customerFromDb.Id);
            }
        }

        [Fact]
        public async Task Get_Customer_Test()
        {
            // Arrange
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;
            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var services = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);

                var pagination = new PaginationRequest();

                // Act
                var customer = new Infrastructure.Entity.Customer()
                {
                    Id = Guid.NewGuid(),
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };

                // Act
                var result = await CoreBankingApi.CreateCustomers(services, customer);

                // Assert
                Assert.NotNull(result);

                // Act
                var resultGetCustomer = await CoreBankingApi.GetCustomers(pagination, services);
                // Assert
                Assert.Equal(1, resultGetCustomer?.Value?.TotalCount);
            }
        }

        [Fact]
        public async Task Create_Account_Test()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);

                var customerId = Guid.NewGuid();
                var customer = new Infrastructure.Entity.Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);

                var account = new Infrastructure.Entity.Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var result = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(result);
                // Check if the customer was added to the database
                var customerFromDb = await context.Accounts.FindAsync(account.Id);
                Assert.NotNull(customerFromDb);
                Assert.Equal(account.Balance, customerFromDb.Balance);
                Assert.Equal(account.CustomerId, customerFromDb.CustomerId);
                Assert.Equal(account.Number, customerFromDb.Number);
                Assert.Equal(account.Id, customerFromDb.Id);
            }
        }

        [Fact]
        public async Task Get_Account_Test()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;
            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var pagination = new PaginationRequest();
                var customerId = Guid.NewGuid();
                var customer = new Infrastructure.Entity.Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Infrastructure.Entity.Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultGetAccount = await CoreBankingApi.GetAccounts(service, pagination, customerId);
                // Assert
                Assert.Equal(1, resultGetAccount?.Value?.TotalCount);
            }
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(10000)]
        [InlineData(9999999999)]
        [InlineData(9999999999.99999)]
        public async Task Deposit_Test(decimal depositAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;
            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Infrastructure.Entity.Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Infrastructure.Entity.Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultDeposit = await CoreBankingApi.Deposit(service, account.Id, new DepositionRequest() { Amount = depositAmount });
                // Assert
                Assert.NotNull(resultDeposit);

                // Check if the customer was added to the database
                var accountFromDb = await context.Accounts.FindAsync(account.Id);
                Assert.NotNull(accountFromDb);
                Assert.Equal(depositAmount, accountFromDb.Balance);
                Assert.Equal(account.CustomerId, accountFromDb.CustomerId);
                Assert.Equal(account.Number, accountFromDb.Number);
                Assert.Equal(account.Id, accountFromDb.Id);

            }
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(10000)]
        [InlineData(9999999999)]
        [InlineData(9999999999.99999)]
        public async Task Deposit_Id_IsEmpty_Test(decimal depositAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;
            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Infrastructure.Entity.Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Infrastructure.Entity.Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultDeposit = await CoreBankingApi.Deposit(service, Guid.Empty, new DepositionRequest() { Amount = depositAmount });
                // Assert
                Assert.NotNull(resultDeposit);
                Assert.IsType<BadRequest>(resultDeposit?.Result);

            }
        }

        [Theory]
        [InlineData(-1000)]
        public async Task Deposit_Amount_Error_Test(decimal depositAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;
            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Infrastructure.Entity.Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Infrastructure.Entity.Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultDeposit = await CoreBankingApi.Deposit(service, Guid.Empty, new DepositionRequest() { Amount = depositAmount });
                // Assert
                Assert.NotNull(resultDeposit);
                Assert.IsType<BadRequest>(resultDeposit?.Result);

            }
        }

        [Theory]
        [InlineData(1000)]
        public async Task Deposit_Id_InCorrect_Error_Test(decimal depositAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;
            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultDeposit = await CoreBankingApi.Deposit(service, Guid.NewGuid(), new DepositionRequest() { Amount = depositAmount });
                // Assert
                Assert.NotNull(resultDeposit);
                Assert.IsType<BadRequest>(resultDeposit?.Result);

            }
        }

        [Theory]
        [InlineData(1000)]
        public async Task WithDraw_Id_Empty_Test(decimal withdrawAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultWithdraw = await CoreBankingApi.WithDraw(service, Guid.Empty, new WithdrawalRequest() { Amount = withdrawAmount });
                // Assert
                Assert.NotNull(resultWithdraw);
                Assert.IsType<BadRequest>(resultWithdraw?.Result);
            }
        }

        [Theory]
        [InlineData(-1000)]
        public async Task WithDraw_Amount_Error_Test(decimal withdrawAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultWithdraw = await CoreBankingApi.WithDraw(service, Guid.Empty, new WithdrawalRequest() { Amount = withdrawAmount });
                // Assert
                Assert.NotNull(resultWithdraw);
                Assert.IsType<BadRequest>(resultWithdraw?.Result);
            }
        }

        [Theory]
        [InlineData(-1000)]
        public async Task WithDraw_Id_Incorrect_Test(decimal withdrawAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultWithdraw = await CoreBankingApi.WithDraw(service, Guid.NewGuid(), new WithdrawalRequest() { Amount = withdrawAmount });
                // Assert
                Assert.NotNull(resultWithdraw);
                Assert.IsType<BadRequest>(resultWithdraw?.Result);
            }
        }

        [Theory]
        [InlineData(1000)]
        public async Task WithDraw_Amount_Invalid_Test(decimal withdrawAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);
                // Act
                var resultWithdraw = await CoreBankingApi.WithDraw(service, account.Id, new WithdrawalRequest() { Amount = withdrawAmount });
                // Assert
                Assert.NotNull(resultWithdraw);
                Assert.IsType<BadRequest>(resultWithdraw?.Result);
            }
        }

        [Theory]
        [InlineData(1000)]
        public async Task WithDraw_Success_Test(decimal withdrawAmount)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _contextOptions = new DbContextOptionsBuilder<CoreBankingDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new CoreBankingDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
                var service = new CoreBankingServices(context, NullLogger<CoreBankingServices>.Instance);
                var customerId = Guid.NewGuid();
                var customer = new Customer()
                {
                    Id = customerId,
                    Name = "HuyTQ",
                    Address = "Hanoi",
                };
                // Act
                var customerResult = await CoreBankingApi.CreateCustomers(service, customer);
                // Assert
                Assert.NotNull(customerResult);
                var account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Balance = 1000,
                    CustomerId = customerId,
                    Number = "123456789",
                };
                // Act
                var resultAccount = await CoreBankingApi.CreateAccounts(service, account);
                // Assert
                Assert.NotNull(resultAccount);

                // Act
                var resultDeposit = await CoreBankingApi.Deposit(service, account.Id, new DepositionRequest() { Amount = 2000 });
                // Assert
                Assert.NotNull(resultDeposit);

                // Act
                var resultWithdraw = await CoreBankingApi.WithDraw(service, account.Id, new WithdrawalRequest() { Amount = withdrawAmount });
                // Assert
                Assert.NotNull(resultWithdraw);
                var accountFromDb = await context.Accounts.FindAsync(account.Id);
                Assert.Equal(2000 - withdrawAmount, accountFromDb?.Balance);
            }
        }
    }
}
