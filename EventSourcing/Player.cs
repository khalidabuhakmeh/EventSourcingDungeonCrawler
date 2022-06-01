public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Health { get; set; } = 100;
    public AliveStatus Status { get; set; }
    public int Treasure { get; set; }

    public void Apply(Enter @event)
    {
        if (Status == AliveStatus.None)
        {
            Name = @event.Name;
            Health = 100;
            Status = AliveStatus.Alive;
        }
    }

    public void Apply(Hit @event)
    {
        Health -= @event.Damage;
        // can't go below 0
        Health = Math.Max(Health, 0);

        if (Health == 0)
        {
            Status = AliveStatus.Dead;
        }
    }

    public void Apply(Heal @event)
    {
        if (Status == AliveStatus.Alive)
        {
            Health = Math.Min(Health + @event.Life, 100);
        }
    }

    public void Apply(Revive @event)
    {
        if (Status == AliveStatus.Dead)
        {
            Health = 100;
            Status = AliveStatus.Alive;
            Treasure = 0;
        }
    }

    public void Apply(Treasure @event)
    {
        if (Status == AliveStatus.Alive)
        {
            Treasure += @event.Amount;
        }
    }
}

public class Revive { }

public enum AliveStatus
{
    None,
    Alive,
    Dead
}

public class Enter
{
    public string Name { get; set; }
}

public class Hit
{
    public int Damage { get; set; }
}

public class Heal
{
    public int Life { get; set; }
}

public class Treasure
{
    public int Amount { get; set; }
}