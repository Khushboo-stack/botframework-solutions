//namespace ITSMSkill.Dialogs.Teams.TicketTaskModule
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading;
//    using System.Threading.Tasks;
//    using AdaptiveCards;
//    using global::ITSMSkill.Dialogs.Teams.View;
//    using global::ITSMSkill.Extensions;
//    using global::ITSMSkill.Extensions.Teams;
//    using global::ITSMSkill.Extensions.Teams.TaskModule;
//    using global::ITSMSkill.Models;
//    using global::ITSMSkill.Models.UpdateActivity;
//    using global::ITSMSkill.Services;
//    using global::ITSMSkill.TeamsChannels;
//    using global::ITSMSkill.TeamsChannels.Invoke;
//    using Microsoft.Bot.Builder;
//    using Microsoft.Bot.Connector;
//    using Microsoft.Bot.Schema;
//    using Microsoft.Bot.Schema.Teams;
//    using Newtonsoft.Json;
//    using Newtonsoft.Json.Linq;

//    [TeamsInvoke(FlowType = nameof(TeamsFlowType.UpdateTicket_Form))]
//    public class UpdateTicketTaskModuleHandler : ITeamsTaskModuleHandler<TaskModuleResponse>
//    {
//        private readonly IStatePropertyAccessor<SkillState> _stateAccessor;
//        private readonly ConversationState _conversationState;
//        private readonly BotSettings _settings;
//        private readonly BotServices _services;
//        private readonly IServiceManager _serviceManager;
//        private readonly IStatePropertyAccessor<ActivityReferenceMap> _activityReferenceMapAccessor;
//        private readonly IConnectorClient _connectorClient;

//        public UpdateTicketTaskModuleHandler(BotSettings settings,
//             BotServices services,
//             ConversationState conversationState,
//             IServiceManager serviceManager,
//             IBotTelemetryClient telemetryClient,
//             IConnectorClient connectorClient)
//        {
//            _conversationState = conversationState;
//            _settings = settings;
//            _services = services;
//            _stateAccessor = conversationState.CreateProperty<SkillState>(nameof(SkillState));
//            _serviceManager = new ServiceManager();
//            _activityReferenceMapAccessor = _conversationState.CreateProperty<ActivityReferenceMap>(nameof(ActivityReferenceMap));
//            _connectorClient = connectorClient;
//        }

//        public async Task<TaskModuleContinueResponse> OnTeamsTaskModuleFetchAsync(ITurnContext context, CancellationToken cancellationToken)
//        {
//            var taskModuleMetadata = context.Activity.GetTaskModuleMetadata<TaskModuleMetadata>();

//            var ticketDetails = taskModuleMetadata.FlowData != null ?
//                    JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(taskModuleMetadata.FlowData))
//                    .TryGetValue("IncidentDetails", out var ticket) ? (Ticket)ticket : null
//                    : null;

//            return new TaskModuleContinueResponse()
//            {
//                Value = new TaskModuleTaskInfo()
//                {
//                    Title = "Please Update The Card Below",
//                    Height = "medium",
//                    Width = 500,
//                    Card = new Attachment
//                    {
//                        ContentType = AdaptiveCard.ContentType,
//                        Content = TicketDialogHelper.UpdateIncidentCard(ticketDetails)
//                    }
//                }
//            };
//        }

//        public async Task<TaskModuleContinueResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext context, CancellationToken cancellationToken)
//        {
//            var state = await _stateAccessor.GetAsync(context, () => new SkillState());

//            var taskModuleMetadata = context.Activity.GetTaskModuleMetadata<TaskModuleMetadata>();

//            var ticketDetails = taskModuleMetadata.FlowData != null ?
//                JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(taskModuleMetadata.FlowData))
//                .TryGetValue("IncidentDetails", out var ticket) ? (Ticket)ticket : null
//                : null;

//            // If ticket is valid go ahead and update
//            if (ticketDetails != null)
//            {
//                ActivityReferenceMap activityReferenceMap = await _activityReferenceMapAccessor.GetAsync(
//                    context,
//                    () => new ActivityReferenceMap(),
//                    cancellationToken)
//                .ConfigureAwait(false);

//                // Get Activity Id from ActivityReferenceMap
//                activityReferenceMap.TryGetValue(context.Activity.Conversation.Id, out var activityReference);

//                // Get User Input from AdatptiveCard
//                var activityValueObject = JObject.FromObject(context.Activity.Value);

//                var isDataObject = activityValueObject.TryGetValue("data", StringComparison.InvariantCultureIgnoreCase, out JToken dataValue);
//                JObject dataObject = null;
//                if (isDataObject)
//                {
//                    dataObject = dataValue as JObject;

//                    // Get Title
//                    var title = dataObject.GetValue("IncidentTitle");

//                    // Get Description
//                    var description = dataObject.GetValue("IncidentDescription");

//                    // Get Urgency
//                    var urgency = dataObject.GetValue("IncidentUrgency");

//                    // Create Managemenet object
//                    var management = _serviceManager.CreateManagement(_settings, state.AccessTokenResponse, state.ServiceCache);
//                    var result = await management.UpdateTicket(ticketDetails.Id, title.Value<string>(), description.Value<string>(), (UrgencyLevel)Enum.Parse(typeof(UrgencyLevel), urgency.Value<string>()));

//                    if (result.Success)
//                    {
//                        //await _teamsTicketUpdateActivity.UpdateTaskModuleActivityAsync(
//                        //    context,
//                        //    activityReference,
//                        //    RenderCreateIncidentHelper.BuildTicketCard(result.Tickets.FirstOrDefault()),
//                        //    cancellationToken);

//                        // Return Added Incident Envelope
//                        return new TaskModuleContinueResponse()
//                        {
//                            Type = "continue",
//                            Value = new TaskModuleTaskInfo()
//                            {
//                                Title = "Incident Updated",
//                                Height = "small",
//                                Width = 300,
//                                Card = new Attachment
//                                {
//                                    ContentType = AdaptiveCard.ContentType,
//                                    Content = RenderCreateIncidentHelper.ImpactTrackerResponseCard("Incident has been Updated")
//                                }
//                            }
//                        };
//                    }
//                }
//            }

//            // Failed to update incident
//            return new TaskModuleContinueResponse()
//            {
//                Type = "continue",
//                Value = new TaskModuleTaskInfo()
//                {
//                    Title = "Incident Update Failed",
//                    Height = "medium",
//                    Width = 500,
//                    Card = new Attachment
//                    {
//                        ContentType = AdaptiveCard.ContentType,
//                        Content = RenderCreateIncidentHelper.ImpactTrackerResponseCard("Incident Update Failed")
//                    }
//                }
//            };
//        }

//        //Task<TaskModuleResponse> ITeamsFetchActivityHandler<TaskModuleResponse>.OnTeamsTaskModuleFetchAsync(ITurnContext turnContext, CancellationToken cancellationToken)
//        //{
//        //    throw new NotImplementedException();
//        //}

//        //Task<TaskModuleResponse> ITeamsSubmitActivityHandler<TaskModuleResponse>.OnTeamsTaskModuleSubmitAsync(ITurnContext context, CancellationToken cancellationToken)
//        //{
//        //    throw new NotImplementedException();
//        //}
//    }
//}
