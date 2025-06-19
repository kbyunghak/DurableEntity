using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;

public interface IConnectorStateEntity
{
    void SetFaulted();
    void SetNormal();
    //string GetState();
}

public class ConnectorStateEntity : IConnectorStateEntity
{
    public string State { get; set; } = "normal";

    public void SetFaulted() => State = "faulted";
    public void SetNormal() => State = "normal";
    //public string GetState() => State;

    public static EntityInstanceId GetId(string connectorId) => new(nameof(ConnectorStateEntity), connectorId);

    [Function(nameof(ConnectorStateEntity))]
    public static Task Run([EntityTrigger] TaskEntityDispatcher dispatcher)
        => dispatcher.DispatchAsync<ConnectorStateEntity>();
}
