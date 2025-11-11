using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PayQueue.Domain.Refunds;
using PayQueue.Domain.Refunds.Models;
using PayQueue.Shared.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace PayQueue.Infrastructure.Refunds;

internal sealed class SqlRefundRepository(IOptions<RefundInquiryOptions> options) : IRefundRepository
{
    private readonly RefundInquiryOptions _options = options.Value;

    public async Task<IReadOnlyCollection<RefundRecord>> GetPendingRefundsAsync(CancellationToken cancellationToken)
    {
        var results = new List<RefundRecord>();

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.usp_GetPendingRefundTransactions", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        IReadOnlyDictionary<string, int>? ordinals = null;
        while (await reader.ReadAsync(cancellationToken))
        {
            ordinals ??= GetRequiredOrdinals(reader, "RefundRowId", "RefundTransactionId", "BaseId");

            if (reader.IsDBNull(ordinals["BaseId"]))
            {
                continue;
            }

            var refundRowId = reader.GetInt32(ordinals["RefundRowId"]);
            var refundTransactionId = reader.IsDBNull(ordinals["RefundTransactionId"])
                ? string.Empty
                : reader.GetString(ordinals["RefundTransactionId"]);

            var baseIdValue = reader.GetString(ordinals["BaseId"]);

            if (!long.TryParse(baseIdValue, out var baseId))
            {
                continue;
            }

            results.Add(new RefundRecord(refundRowId, refundTransactionId, baseId));
        }

        return results;
    }

    public async Task UpdateRefundStatesAsync(IEnumerable<RefundStatusUpdate> updates, CancellationToken cancellationToken)
    {
        var updateList = updates.ToList();
        if (updateList.Count == 0)
        {
            return;
        }

        var table = new DataTable();
        table.Columns.Add("RefundRowId", typeof(long));
        table.Columns.Add("RefundState", typeof(int));
        table.Columns.Add("BodyResponse", typeof(string));

        foreach (var update in updateList)
        {
            table.Rows.Add(update.RefundRowId, update.RefundState, (object?)update.BodyResponse ?? DBNull.Value);
        }

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.usp_UpdateRefundTransactionStates", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };

        var parameter = command.Parameters.AddWithValue("@RefundUpdates", table);
        parameter.SqlDbType = SqlDbType.Structured;
        parameter.TypeName = "dbo.StatusUpdateType";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static IReadOnlyDictionary<string, int> GetRequiredOrdinals(SqlDataReader reader, params string[] requiredColumns)
    {
        var available = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            available[reader.GetName(i)] = i;
        }

        var missing = requiredColumns.Where(column => !available.ContainsKey(column)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Result set from usp_GetPendingRefundTransactions is missing required column(s): {string.Join(", ", missing)}.");
        }

        var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in requiredColumns)
        {
            ordinals[column] = available[column];
        }

        return ordinals;
    }
}


