using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

namespace RosterApiLambda.Services
{
    public class LogEntryService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }

        public LogEntryService(string organizationId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"ORGANIZATION#{organizationId}";
        }

        public async Task<List<LogEntryModel>> GetLogEntries()
            => await _dataHelper.QueryAsync<LogEntryModel>(_pk, "LOG_ENTRY#");

        public async Task<PutItemResponse> SaveLogEntry(LogEntryModel logEntry)
        {
            // var logEntryId = Guid.NewGuid().ToString().ToLower();
            var logEntryId = DateTimeOffset.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
            logEntry.logEntryId = logEntryId;
            logEntry.entryDateTime = DateTimeOffset.Now.ToString("u");
            logEntry.timestamp = Convert.ToInt32(Math.Floor(DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0));

            return await _dataHelper.PutItemAsync(_pk, $"LOG_ENTRY#{logEntryId}", logEntry);
        }
    }
}
