using AdaptiveCards;
using ITSMSkill.Extensions;
using ITSMSkill.Extensions.Teams;
using ITSMSkill.Extensions.Teams.TaskModule;
using ITSMSkill.Models;
using ITSMSkill.Services;
using ITSMSkill.TeamsChannels;
using ITSMSkill.TeamsChannels.Invoke;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITSMSkill.Dialogs.Teams
{
    [TeamsInvoke(FlowType = nameof(TeamsFlowType.UpdateTicket_Form))]
    public class UpdateTicketTeamsImplementation : ITeamsTaskModuleHandler<TaskModuleResponse>
    {
        public UpdateTicketTeamsImplementation(
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

            var ticketDetails = taskModuleMetadata.FlowData != null ?
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(taskModuleMetadata.FlowData))
                    .TryGetValue("IncidentDetails", out var ticket) ? (Ticket)ticket : null
                    : null;

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse()
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Title = "Please Update The Card Below",
                        Height = "medium",
                        Width = 500,
                        Card = new Attachment
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = TicketDialogHelper.UpdateIncidentCard(ticketDetails)
                        }
                    }
                }
            };
        }

        public Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
