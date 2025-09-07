using System;
using Microsoft.Xrm.Sdk;

namespace Nx.Score.Plugins.NxScorePlugins
{

    public class PostOperationnx_pinballroundscoreCreate: PluginBase
    {
        public PostOperationnx_pinballroundscoreCreate(string unsecure, string secure)
            : base(typeof(PostOperationnx_pinballroundscoreCreate))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
    {
        if (localContext == null)
        {
            throw new InvalidPluginExecutionException(nameof(localContext));
        }
        ITracingService tracingService = localContext.TracingService;

        try
        {
            IPluginExecutionContext context = (IPluginExecutionContext)localContext.PluginExecutionContext;
            IOrganizationService currentUserService = localContext.CurrentUserService;

            Entity targetEntity = context.InputParameters.Contains("Target") ? (Entity)context.InputParameters["Target"] : null;

            PinballRoundScoreLogic.UpdateCurrentRoundScore(targetEntity, localContext);
            PinballRoundScoreLogic.UpdateScoreTotals(targetEntity, localContext);
            PinballRoundScoreLogic.UpdateGameWinnerAndStats(targetEntity, localContext);
        }
        catch (Exception ex)
        {
            tracingService?.Trace("An error occurred executing Plugin Nx.Score.Plugins.NxScorePlugins.PostOperationnx_pinballroundscoreCreate : {0}", ex.ToString());
            throw new InvalidPluginExecutionException("An error occurred executing Plugin Nx.Score.Plugins.NxScorePlugins.PostOperationnx_pinballroundscoreCreate .", ex);
        }
    }
}
}
