// See https://aka.ms/new-console-template for more information

using Marten;
using Marten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Weasel.Core;

var host = new HostBuilder()
    .ConfigureAppConfiguration((_, c) =>
    {
        c.AddJsonFile("appsettings.json");
        c.AddEnvironmentVariables();
    })
    .ConfigureServices((context, collection) =>
    {
        collection.AddMarten(so =>
        {
            so.Connection(context.Configuration.GetConnectionString("Marten"));
            so.AutoCreateSchemaObjects = AutoCreate.All;
            // yolo
            so.Logger(new MartenNullLogger());

            so.Projections.SelfAggregate<Player>();
        });

        collection.AddHostedService<DungeonWorker>();
    })
    .ConfigureLogging((hc, l) =>
    {
        l.AddConfiguration(hc.Configuration.GetSection("Logging"));
        l.AddConsole();
    });

await host.RunConsoleAsync();

public class MartenNullLogger : IMartenLogger
{
    public IMartenSessionLogger StartSession(IQuerySession session)
    {
        return new MartenNullSessionLogger();
    }

    public void SchemaChange(string sql)
    {
    }

    private class MartenNullSessionLogger : IMartenSessionLogger
    {
        public void LogSuccess(NpgsqlCommand command)
        {
            
        }

        public void LogFailure(NpgsqlCommand command, Exception ex)
        {
        }

        public void RecordSavedChanges(IDocumentSession session, IChangeSet commit)
        {
        }

        public void OnBeforeExecute(NpgsqlCommand command)
        {

        }
    }
}