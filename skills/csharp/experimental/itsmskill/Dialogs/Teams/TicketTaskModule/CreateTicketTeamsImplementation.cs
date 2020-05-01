﻿namespace ITSMSkill.Dialogs.Teams.TicketTaskModule
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using global::ITSMSkill.Dialogs.Teams.View;
    using global::ITSMSkill.Extensions.Teams;
    using global::ITSMSkill.Models;
    using global::ITSMSkill.Models.UpdateActivity;
    using global::ITSMSkill.Services;
    using global::ITSMSkill.TeamsChannels;
    using global::ITSMSkill.TeamsChannels.Invoke;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Create ticket teams activity handler
    /// </summary>
    [TeamsInvoke(FlowType = nameof(TeamsFlowType.CreateTicket_Form))]
    [FetchHandler(Title = "CreateTicketFetchHandler")]
    public class CreateTicketTeamsImplementation : ITeamsTaskModuleHandler<TaskModuleResponse>
    {
        private readonly IStatePropertyAccessor<SkillState> _stateAccessor;
        private readonly ConversationState _conversationState;
        private readonly BotSettings _settings;
        private readonly BotServices _services;
        private readonly IServiceManager _serviceManager;
        private readonly IStatePropertyAccessor<ActivityReferenceMap> _activityReferenceMapAccessor;
        private readonly IConnectorClient _connectorClient;

        public CreateTicketTeamsImplementation(
             BotSettings settings,
             BotServices services,
             ConversationState conversationState,
             IServiceManager serviceManager,
             IBotTelemetryClient telemetryClient,
             IConnectorClient connectorClient)
        {
            _conversationState = conversationState;
            _settings = settings;
            _services = services;
            _stateAccessor = conversationState.CreateProperty<SkillState>(nameof(SkillState));
            _serviceManager = new ServiceManager();
            _activityReferenceMapAccessor = _conversationState.CreateProperty<ActivityReferenceMap>(nameof(ActivityReferenceMap));
            _connectorClient = connectorClient;
        }

        public async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            var state = await _stateAccessor.GetAsync(context, () => new SkillState());

            ActivityReferenceMap activityReferenceMap = await _activityReferenceMapAccessor.GetAsync(
                context,
                () => new ActivityReferenceMap(),
                cancellationToken)
            .ConfigureAwait(false);

            // Get Activity Id from ActivityReferenceMap
            activityReferenceMap.TryGetValue(context.Activity.Conversation.Id, out var activityReference);

            // Get User Input from AdatptiveCard
            var activityValueObject = JObject.FromObject(context.Activity.Value);

            var isDataObject = activityValueObject.TryGetValue("data", StringComparison.InvariantCultureIgnoreCase, out JToken dataValue);
            JObject dataObject = null;
            if (isDataObject)
            {
                dataObject = dataValue as JObject;

                // Get Title
                var title = dataObject.GetValue("IncidentTitle");

                // Get Description
                var description = dataObject.GetValue("IncidentDescription");

                // Get Urgency
                var urgency = dataObject.GetValue("IncidentUrgency");

                // Create Ticket
                var management = _serviceManager.CreateManagement(_settings, state.AccessTokenResponse, state.ServiceCache);
                var ticketResults = await management.CreateTicket(title.Value<string>(), description.Value<string>(), (UrgencyLevel)Enum.Parse(typeof(UrgencyLevel), urgency.Value<string>()));
                //var ticketResults = await CreateTicketAsync(title.Value<string>(), description.Value<string>(), (UrgencyLevel)Enum.Parse(typeof(UrgencyLevel), urgency.Value<string>()));

                // If Ticket Created Update Activity
                if (ticketResults.Success)
                {
                    await UpdateActivityHelper.UpdateTaskModuleActivityAsync(context, activityReference, ticketResults.Tickets.FirstOrDefault(), _connectorClient, cancellationToken);
                    // Return Added Incident Envelope
                    return new TaskModuleResponse()
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Type = "continue",
                            Value = new TaskModuleTaskInfo()
                            {
                                Title = "Incident Added",
                                Height = "small",
                                Width = 300,
                                Card = new Attachment
                                {
                                    ContentType = AdaptiveCard.ContentType,
                                    Content = RenderCreateIncidentHelper.ImpactTrackerResponseCard("Incident has been created")
                                }
                            }
                        }
                    };
                }
            }

            // Failed to create incident
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Title = "Incident Create Failed",
                        Height = "medium",
                        Width = 500,
                        Card = new Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = RenderCreateIncidentHelper.ImpactTrackerResponseCard("Incident Create Failed")
                        }
                    }
                }
            };
        }

        public async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            return new TaskModuleResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Title = "ImpactTracker",
                        Height = "medium",
                        Width = 500,
                        Card = new Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = TicketDialogHelper.CreateIncidentAdaptiveCard()
                        }
                    }
                }
            };
        }
    }
}
