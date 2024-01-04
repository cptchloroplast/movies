using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Movies.SQL.Extensions;
using Movies.SQL.Factories;
using Movies.SQL.Options;
using Movies.SQL.Repositories;

namespace Movies.SQL.Test.Repositories;
public class TestEntityRepository : RepositoryBase<TestEntity>
{
  public TestEntityRepository(
    ILogger<TestEntityRepository> logger,
        IDbConnectionFactory factory,
        IOptionsMonitor<DbConnectionOptions> options
  ) : base(logger, factory, options) { }
  private const string CREATE = @"
    INSERT INTO TestEntity (
      Integer,
      Long,
      Float,
      DateTime,
      String,
      SystemKey,
      SystemCreatedDate,
      SystemModifiedDate
    )
    VALUES (
      @Integer,
      @Long,
      @Float,
      @DateTime,
      @String,
      @SystemKey,
      @SystemCreatedDate,
      @SystemModifiedDate
    )";
  public override int Create(TestEntity entity) =>
    UseConnection((IDbConnection connection) => 
      {
        var input = entity with { };
        var now = DateTime.UtcNow;
        input.SystemCreatedDate = now;
        input.SystemModifiedDate = now;
        input.SystemKey = Guid.NewGuid();
        return connection.ExecuteCommand(CREATE, input);
      });

  private const string DELETE = @"
    DELETE FROM TestEntity
    WHERE SystemKey = @SystemKey";
  public override int Delete(Guid SystemKey) => 
    UseConnection((IDbConnection connection) => 
      connection.ExecuteCommand(DELETE, new { SystemKey }));

  private const string READ = @"
    SELECT
      Integer,
      Long,
      Float,
      String,
      DateTime,
      SystemKey,
      SystemCreatedDate,
      SystemModifiedDate
    FROM TestEntity 
    WHERE SystemKey = @SystemKey";
  public override TestEntity? Read(Guid SystemKey) =>
    UseConnection((IDbConnection connection) =>
      connection.ExecuteQuery<TestEntity?>(READ, new { SystemKey }));

  private const string UPDATE = @"
  UPDATE TestEntity
  SET
    Integer = @Integer,
    Long = @Long,
    Float = @Float,
    DateTime = @DateTime,
    String = @String,
    SystemModifiedDate = @SystemModifiedDate
  WHERE
    SystemKey = @SystemKey";
  public override int Update(TestEntity entity) => 
    UseConnection((IDbConnection connection) => 
      {
        var input = entity with { };
        input.SystemModifiedDate = DateTime.UtcNow;
        return connection.ExecuteCommand(UPDATE, input);
      });
}