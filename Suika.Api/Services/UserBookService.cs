using Dapper;
using Suika.Api.Data;
using Suika.Api.Models;

namespace Suika.Api.Services;

public class UserBookService : IUserBookService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserBookService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Create(UserBook userBook)
    {
        var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(@"

        ");
        return result > 0;
    }

    public Task<IEnumerable<UserBook>> GetAll()
    {
        throw new NotImplementedException();
    }
}