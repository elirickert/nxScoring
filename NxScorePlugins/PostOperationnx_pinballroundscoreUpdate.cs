using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Nx.Score.Plugins.NxScorePlugins
{
    public class PostOperationnx_pinballroundscoreUpdate : PluginBase
    {
        public PostOperationnx_pinballroundscoreUpdate(string unsecure, string secure)
            : base(typeof(PostOperationnx_pinballroundscoreUpdate))
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

                Entity basicTargetEntity = context.InputParameters.Contains("Target")
                    ? (Entity)context.InputParameters["Target"]
                    : null;

                if (basicTargetEntity == null)
                {
                    throw new InvalidPluginExecutionException("Target Entity is null");
                }

                var targetEntity = GetFullTargetEntity(basicTargetEntity.Id, localContext);

                PinballRoundScoreLogic.UpdateCurrentRoundScore(targetEntity, localContext);
                PinballRoundScoreLogic.UpdateScoreTotals(targetEntity, localContext);
                PinballRoundScoreLogic.UpdateGameWinnerAndStats(targetEntity, localContext);
            }
            catch (Exception ex)
            {
                tracingService?.Trace(
                    "An error occurred executing Plugin Nx.Score.Plugins.NxScorePlugins.PostOperationnx_pinballroundscoreUpdate : {0}",
                    ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred executing Plugin Nx.Score.Plugins.NxScorePlugins.PostOperationnx_pinballroundscoreUpdate .",
                    ex);
            }
        }

        private Entity GetFullTargetEntity(Guid targetId, ILocalPluginContext localContext)
        {
            return localContext.CurrentUserService.Retrieve("nx_pinballroundscore", targetId, new ColumnSet(true));
        }
    }
}
