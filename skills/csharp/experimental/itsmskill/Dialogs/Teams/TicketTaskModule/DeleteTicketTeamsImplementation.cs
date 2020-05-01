using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSMSkill.Dialogs.Teams.TicketTaskModule
{
    using AdaptiveCards;
    using global::ITSMSkill.Dialogs.Teams.View;
    using global::ITSMSkill.Extensions;
    using global::ITSMSkill.Extensions.Teams;
    using global::ITSMSkill.Extensions.Teams.TaskModule;
    using global::ITSMSkill.Models;
    using global::ITSMSkill.Models.UpdateActivity;
    using global::ITSMSkill.Services;
    using global::ITSMSkill.TeamsChannels;
    using global::ITSMSkill.TeamsChannels.Invoke;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ITSMSkill.Dialogs.Teams
    {
        [TeamsInvoke(FlowType = nameof(TeamsFlowType.DeleteTicket_Form))]
        public class DeleteTicketTeamsImplementation : ITeamsTaskModuleHandler<TaskModuleResponse>
        {
            public DeleteTicketTeamsImplementation(
                 BotSettings settings,
                 BotServices services,
                 ConversationState conversationState,
                 IServiceManager serviceManager,
                 IBotTelemetryClient telemetryClient,
                 IConnectorClient connectorClient)
            {
            }

            public async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var taskModuleMetadata = turnContext.Activity.GetTaskModuleMetadata<TaskModuleMetadata>();

                var id = taskModuleMetadata.FlowData != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(taskModuleMetadata.FlowData))
                        .GetValueOrDefault("IncidentId") : null;

                // Convert JObject to Ticket
                string incidentId = JsonConvert.DeserializeObject<string>(id.ToString());

                return new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse()
                    {
                        Value = new TaskModuleTaskInfo()
                        {
                            Title = "DeleteTicket",
                            Height = "medium",
                            Width = 500,
                            Card = new Attachment
                            {
                                ContentType = AdaptiveCard.ContentType,
                                Content = TicketDialogHelper.GetDeleteConfirmationCard(incidentId)
                            }
                        }
                    }
                };
            }

            public async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext context, CancellationToken cancellationToken)
            {
                var taskModuleMetadata = context.Activity.GetTaskModuleMetadata<TaskModuleMetadata>();

                var id = taskModuleMetadata.FlowData != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(taskModuleMetadata.FlowData))
                        .GetValueOrDefault("IncidentId") : null;

                // Convert JObject to Ticket
                string incidentId = (string)id;

                if (incidentId != null)
                {
                    string ticketCloseReason = string.Empty;

                    // Get User Input from AdatptiveCard
                    var activityValueObject = JObject.FromObject(context.Activity.Value);

                    var isDataObject = activityValueObject.TryGetValue("data", StringComparison.InvariantCultureIgnoreCase, out JToken dataValue);
                    JObject dataObject = null;
                    if (isDataObject)
                    {
                        // Get TicketCloseReason
                        ticketCloseReason = dataObject.GetValue("IncidentCloseReason").Value<string>();
                    }

                    // Create Managemenet object
                    //var management = _serviceManager.CreateManagement(_settings, state.AccessTokenResponse, state.ServiceCache);

                    //// Create Ticket
                    //var result = await management.CloseTicket(incidentId, ticketCloseReason);

                    //// TODO: Figure out what should we update the incident with in order
                    //ActivityReferenceMap activityReferenceMap = await _activityReferenceMapAccessor.GetAsync(
                    //    context,
                    //    () => new ActivityReferenceMap(),
                    //    cancellationToken)
                    //.ConfigureAwait(false);
                    //activityReferenceMap.TryGetValue(context.Activity.Conversation.Id, out var activityReference);

                    //await UpdateActivityHelper.UpdateTaskModuleActivityAsync(
                    //    context,
                    //    activityReference,
                    //    RenderCreateIncidentHelper.CloseTicketCard(result.Tickets[0]),
                    //    cancellationToken);

                    // Return Closed Incident Envelope
                    return new TaskModuleResponse
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Type = "continue",
                            Value = new TaskModuleTaskInfo()
                            {
                                Title = "Incident Deleted",
                                Height = "small",
                                Width = 300,
                                Card = new Attachment
                                {
                                    ContentType = AdaptiveCard.ContentType,
                                    Content = RenderCreateIncidentHelper.ImpactTrackerResponseCard("Incident has been Deleted")
                                }
                            }
                        }
                    };
                }

                // Failed to Delete Incident
                return new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse()
                    {
                        Type = "continue",
                        Value = new TaskModuleTaskInfo()
                        {
                            Title = "Incident Delete Failed",
                            Height = "small",
                            Width = 300,
                            Card = new Attachment
                            {
                                ContentType = AdaptiveCard.ContentType,
                                Content = RenderCreateIncidentHelper.ImpactTrackerResponseCard("Incident Delete Failed")
                            }
                        }
                    }
                };
            }
        }
    }
}
