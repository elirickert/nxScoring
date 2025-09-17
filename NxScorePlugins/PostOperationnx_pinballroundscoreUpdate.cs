using System;
using System.Text.Json;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Nx.Score.Plugins.NxScorePlugins.Logic;
using Nx.Score.Plugins.NxScorePlugins.Model;

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

                var targetEntity = GetFromPostImage(localContext);

                tracingService.Trace($"target entity: {JsonSerializer.Serialize(targetEntity, new JsonSerializerOptions { WriteIndented = true })}");

                var pinballRoundScore = PinballRoundScore.ParsePinballRoundScore(targetEntity);

                PinballRoundScoreLogic.UpdateCurrentRoundScore(pinballRoundScore, localContext);
                PinballRoundScoreLogic.UpdateScoreTotals(pinballRoundScore, localContext);
                PinballRoundScoreLogic.UpdateGameWinnerAndStats(pinballRoundScore, localContext);
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

        private Entity GetFromPostImage(ILocalPluginContext localContext)
        {
            localContext.PluginExecutionContext.PostEntityImages.TryGetValue("scoresPostImage", out Entity image);

            return image;
        }

        // private Entity GetFullTargetEntity(Guid targetId, ILocalPluginContext localContext)
        // {
        //     // FIXME: only update fields not contained in the input parameters??? use image??
        //     return localContext.CurrentUserService.Retrieve("nx_pinballroundscore", targetId, new ColumnSet(true));
        // }
    }
}
