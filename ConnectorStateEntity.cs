using DurableEntityTest;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using System.Text.Json.Serialization;

public class ConnectorStateEntity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    /// <summary>
    /// Current state of the connector (e.g., Normal, Faulted).
    /// This value is read-only outside the entity and can only be modified via SetState().
    /// </summary>    
    public ConnectorState State { get; set; }
    /// <summary>
    /// Updates the state of the connector. This is the only allowed way to change the entity state.
    /// </summary>
    /// <param name="newState">The new state to apply to the connector.</param>
    public void SetState(ConnectorState newState)
    {
        State = newState;
    }
    public static EntityInstanceId GetId(string connectorId) => new(nameof(ConnectorStateEntity), connectorId);

    [Function(nameof(ConnectorStateEntity))]
    public static Task Run([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<ConnectorStateEntity>();
}
