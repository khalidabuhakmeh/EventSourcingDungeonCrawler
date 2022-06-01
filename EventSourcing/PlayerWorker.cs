using System.Security.Cryptography;
using Marten;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

public class DungeonWorker : BackgroundService
{
    private readonly IDocumentStore db;

    public DungeonWorker(IDocumentStore db)
    {
        this.db = db;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var name = AnsiConsole.Ask<string>("What's your name [green]name[/]?");

        var id = Guid.NewGuid();
        var entering = new Enter {Name = name};
        
        await using (var session = db.OpenSession())
        {
            session.Events.Append(id, entering);
            await session.SaveChangesAsync(stoppingToken);
        }

        AnsiConsole.MarkupLineInterpolated(
            $"Welcome [green]{entering.Name}[/]! Dangers lie ahead, :skull_and_crossbones: but also treasure :wrapped_gift:.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await using var session = db.OpenSession();
            var current = await session.Events.AggregateStreamAsync<Player>(id, token: stoppingToken);

            AnsiConsole.MarkupLine($"[red]{current.Name}: {current.Health}pts[/] | [yellow]Treasure: {current.Treasure} coins[/]");
            
            if (current.Status is AliveStatus.Dead)
            {
                var answer = AnsiConsole.Ask<char>(":skull: Looks like you're dead. Revive (y,n)?");
                if (answer is 'y')
                {
                    session.Events.Append(id, new Revive());
                }
            }
            else
            {
                var command = AnsiConsole.Ask<char>("What would you like to do (Fight (f), Heal (h))?");

                switch (command)
                {
                    case 'f':
                        var hit = new Hit {Damage = RandomNumberGenerator.GetInt32(1, 15)};
                        session.Events.Append(id, hit);
                        AnsiConsole.MarkupLine($"\tOuch! [red]{hit.Damage}pts[/] damage.");
                        
                        // only find treasure if you're fighting
                        if (RandomNumberGenerator.GetInt32(1, 4) is 2)
                        {
                            // found treasure!
                            var amount = RandomNumberGenerator.GetInt32(1, 11);
                            AnsiConsole.MarkupLine($"\t[yellow]YOU FOUND {amount} TREASURE![/]");
                            session.Events.Append(id, new Treasure { Amount = amount });
                        }
                        
                        break;
                    case 'h':
                        session.Events.Append(id, new Heal {Life = 15});
                        AnsiConsole.MarkupLine("\tHealed! [green]15pts[/]");
                        break;
                }

                
            }
            // save round
            await session.SaveChangesAsync(stoppingToken);
        }
    }
}