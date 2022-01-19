using System.Data.SqlClient;
using Dapper;
using HQPdf.Models;
using DapperExtensions;
using DapperExtensions.Mapper;
using HQPdf.Models.Trojan;
using Parameter = HQPdf.Models.Trojan.Parameter;

namespace Repo;

public class Repo
{
    private readonly string _connString = "";

    public async Task<Forms> GetForm(int id)
    {
        await using var conn = new SqlConnection(_connString);
        return await conn.QueryFirstAsync<Forms>("select * from Forms where Id = @id", new { id });
    }

    public async Task<IEnumerable<Parameter>> GetParametersForForm(int id)
    {
        await using var conn = new SqlConnection(_connString);
        return await conn.QueryAsync<Parameter>("select * from Parameters where FormId = @id", new { id });
    }
    
}