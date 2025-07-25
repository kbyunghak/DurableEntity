using Azure;
using DurableEntityTest;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Newtonsoft.Json;
using Serilog;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ConnectorStateFunctions
{
    public record StateUpdateRequest(
        [property: System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))] ConnectorState State);

    [Function("SetConnectorState")]
    public async Task<HttpResponseData> SetConnectorState(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "connector/{connectorId}/state")] HttpRequestData req,
        string connectorId,
        [DurableClient] DurableTaskClient client)
    {        
        try
        {
            Log.Information("C# HTTP trigger function processed a request.");

            if (string.IsNullOrWhiteSpace(connectorId))
            {                
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Missing connectorId.");
                return bad;
            }
            var body = await System.Text.Json.JsonSerializer.DeserializeAsync<StateUpdateRequest>(req.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (body == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Missing or invalid request body.");
                return bad;
            }            
            var entityId = new EntityInstanceId(nameof(ConnectorStateEntity), connectorId);
            await client.Entities.SignalEntityAsync(entityId, "SetState", body.State);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Connector {connectorId} state set to '{body.State}'.");
            return response;
        }
        catch (Exception e)
        {
            Log.Error(e, "An unhandled error occurred while processing a post event: {message}", e.Message);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Internal error occurred.");
            return errorResponse;
        }        
    }
    [Function("GetConnectorState")]
    public async Task<HttpResponseData> GetConnectorState(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "connector/{connectorId}/state")] HttpRequestData req,
       string connectorId,
       [DurableClient] DurableTaskClient client)
    {
        try
        {
            Log.Information("C# HTTP trigger function processed a request.");

            var entityId = new EntityInstanceId(nameof(ConnectorStateEntity), connectorId);
            EntityMetadata<ConnectorStateEntity>? entity = null;

            for (int i = 0; i < 3; i++)
            {
                entity = await client.Entities.GetEntityAsync<ConnectorStateEntity>(entityId);
                if (entity?.State != null)
                    break;

                await Task.Delay(300);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);

            if (entity?.State == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync($"Connector {connectorId} does not exist or is not initialized.");
            }
            else
            {
                await response.WriteStringAsync($"Connector {connectorId} state is '{entity.State.State}'.");
            }

            return response;
        }
        catch (Exception e)
        {
            Log.Error(e, "An unhandled error occurred while processing a post event: {message}", e.Message);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Internal error occurred.");
            return errorResponse;
        }
    }
   
}