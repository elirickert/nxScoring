using System;
using Microsoft.Xrm.Sdk;
using Nx.Score.Plugins.NxScorePlugins.Model;

namespace Nx.Score.Plugins.NxScorePlugins.Logic
{
    public static class PinballGameLogic
    {
        public static void UpdateWinnerAndAveragePoints(PinballGame pinballGame, ILocalPluginContext localContext)
        {
            var pinballGameToUpdate = CreateNewGame(pinballGame.Id);
            pinballGameToUpdate[PinballGame.FieldNames.AveragePointsFieldName] = pinballGame.AveragePoints;
            if (pinballGame.WinnersScoreId.HasValue)
            {
                pinballGameToUpdate[PinballGame.FieldNames.WinnersScoreFieldName] = new EntityReference(
                    PinballScore.FieldNames.EntityName,
                    pinballGame.WinnersScoreId.Value);
            }

            localContext.CurrentUserService.Update(pinballGameToUpdate);
        }

        private static Entity CreateNewGame(Guid pinballScoreId)
        {
            var newGame = new Entity(PinballGame.FieldNames.EntityName)
            {
                [PinballGame.FieldNames.IdName] = pinballScoreId
            };

            return newGame;
        }
    }
}