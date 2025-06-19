using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Newtonsoft.Json;
using System.Net;

public class ConnectorStateFunctions
{
    public record StateRequest(string ConnectorId);

    [Function("SetConnectorFaulted")]
    public async Task<HttpResponseData> SetFaulted(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "connector/faulted")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        StateRequest? data = JsonConvert.DeserializeObject<StateRequest>(requestBody);

        if (string.IsNullOrWhiteSpace(data?.ConnectorId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing connectorId in request body.");
            return bad;
        }

        var entityId = new EntityInstanceId(nameof(ConnectorStateEntity), data.ConnectorId);
        Console.WriteLine($"[Signal] Sending 'faulted' to entity ID: {entityId}");        
        await client.Entities.SignalEntityAsync(entityId, "SetFaulted", null);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"Connector {data.ConnectorId} set to faulted.");
        return response;
    }

    [Function("SetConnectorNormal")]
    public async Task<HttpResponseData> SetNormal(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = "connector/normal")] HttpRequestData req,
       [DurableClient] DurableTaskClient client)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        StateRequest? data = JsonConvert.DeserializeObject<StateRequest>(requestBody);

        if (string.IsNullOrWhiteSpace(data?.ConnectorId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing connectorId in request body.");
            return bad;
        }

        var entityId = new EntityInstanceId(nameof(ConnectorStateEntity), data.ConnectorId);
        Console.WriteLine($"[Signal] Sending 'normal' to entity ID: {entityId}");
        await client.Entities.SignalEntityAsync(entityId, "SetNormal", null);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"Connector {data.ConnectorId} set to normal.");
        return response;
    }

    [Function("GetConnectorState")]
    public async Task<HttpResponseData> GetState(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "connector/state/{connectorId}")] HttpRequestData req,
    string connectorId,
    [DurableClient] DurableTaskClient client)
    {
        if (string.IsNullOrWhiteSpace(connectorId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing connectorId.");
            return bad;
        }

        var entityId = new EntityInstanceId(nameof(ConnectorStateEntity), connectorId);
        var entityResponse = await client.Entities.GetEntityAsync<string>(entityId);

        var response = req.CreateResponse(HttpStatusCode.OK);

        if (entityResponse == null || string.IsNullOrEmpty(entityResponse.State))
        {
            await response.WriteStringAsync($"Connector {connectorId} does not exist.");
        }
        else
        {
            await response.WriteStringAsync($"Connector {connectorId} state is '{entityResponse.State}'.");
        }

        return response;
    }

}
