namespace MessengerAPI.Domain.Common;

public interface IHasDomainEvents
{
    /// <summary>
    /// List of occurred domain events
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    /// <summary>
    /// Clear list of domain events
    /// </summary>
    public void ClearDomainEvents();
}
