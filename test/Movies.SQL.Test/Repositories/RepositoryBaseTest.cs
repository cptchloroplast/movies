using Xunit;
using Movies.SQL.Repositories;
using Movies.SQL.Entities;
using Movies.SQL.Options;
using Movies.SQL.Factories;
using Microsoft.Extensions.Options;
using Moq;
using Movies.Test;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Movies.SQL.Extensions;
using FluentMigrator.Runner;
using System.Data;
namespace Movies.SQL.Test.Repositories;
public abstract class RepositoryBaseTest<T> : IDisposable
    where T : EntityBase
{
    protected abstract IRepository<T> Repository { get; }
    protected readonly IDbConnectionFactory _factory;
    protected readonly IOptionsMonitor<DbConnectionOptions> _options;
    private readonly IDbConnection _hold;
    public RepositoryBaseTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var serviceProvider = new ServiceCollection()
            .AddSQLite(configuration)
            .AddSQLiteMigrationRunner(configuration)
            .BuildServiceProvider();
        
        using var scope = serviceProvider.CreateScope();
        var options = new DbConnectionOptions();
        configuration.Bind(nameof(DbConnectionOptions), options);
        _options = Mock.Of<IOptionsMonitor<DbConnectionOptions>>();
        Mock.Get(_options).Setup(x => x.CurrentValue)
            .Returns(options);
        _factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        _hold = _factory.CreateConnection(options.ConnectionString);
        _hold.Open();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
    public void Dispose()
    {
        _hold.Close();
        _hold.Dispose();
    }
    [Theory]
    [AutoMockData]
    public  async Task CRUD(T entity)
    {
        // Empty
        var empty = await Repository.Read(entity.SystemKey);
        empty.Should().BeNull();
        // Create
        await Repository.Create(entity);
        var created = await Repository.Read(entity.SystemKey);
        created.Should().NotBe(entity);
        created.Should().BeEquivalentTo(entity);
        // Update
        await Repository.Update(entity);
        var updated = await Repository.Read(entity.SystemKey);
        updated.Should().NotBe(entity);
        updated.Should().BeEquivalentTo(entity);
        // Delete
        await Repository.Delete(entity.SystemKey);
        var deleted = await Repository.Read(entity.SystemKey);
        deleted.Should().BeNull();
    }
}