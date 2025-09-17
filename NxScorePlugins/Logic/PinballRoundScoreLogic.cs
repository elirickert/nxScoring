using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Nx.Score.Plugins.NxScorePlugins.Model;

namespace Nx.Score.Plugins.NxScorePlugins.Logic
{
    public static class PinballRoundScoreLogic
    {
        public static void UpdateCurrentRoundScore(PinballRoundScore pinballScoreRound, ILocalPluginContext localContext)
        {
            // get previous score round from dv from entity refand then get pointsafterround
            var scoreAfterPreviousRound = 0;
            var previousScoreRound = GetPreviousScoreRound(pinballScoreRound.PreviousRoundScoreId, localContext);
            if (previousScoreRound != null)
            {
                scoreAfterPreviousRound = previousScoreRound.PointsAfterRound;
            }

            // current points after round
            var currentValueAfterRound = pinballScoreRound.PointsAfterRound;

            // update round points
            UpdateRoundPoints(pinballScoreRound, currentValueAfterRound - scoreAfterPreviousRound, localContext);
        }

        public static void UpdateScoreTotals(PinballRoundScore pinballScoreRound, ILocalPluginContext localContext)
        {
            var allScoreRoundsForRoundNumber = GetOtherScoreRoundsSameRoundNumber(pinballScoreRound.PinballScoreId.Id, pinballScoreRound.RoundNumber, localContext);

            PinballScoreLogic.UpdateScoreTotals(pinballScoreRound, allScoreRoundsForRoundNumber, localContext);
        }

        public static void UpdateGameWinnerAndStats(PinballRoundScore pinballScoreRound, ILocalPluginContext localContext)
        {
            var pinballScore = PinballScoreLogic.GetPinballScoreFromId(pinballScoreRound.PinballScoreId.Id, localContext);
            var pinballScores = PinballScoreLogic.GetAllScoresFromGameId(pinballScore.PinballGameId.Id, localContext);

            // GAME STATS
            var averageScore = pinballScores.Sum(x => x.PointsTotal)/pinballScores.Count;
            var winnerScore = pinballScores.OrderByDescending(x => x.PointsTotal).FirstOrDefault();

            var pinballGame = new PinballGame()
            {
                Id = pinballScore.PinballGameId.Id,
                AveragePoints = averageScore,
                WinnersScoreId = winnerScore?.Id
            };

            PinballGameLogic.UpdateWinnerAndAveragePoints(pinballGame, localContext);
        }

        private static PinballRoundScore GetPreviousScoreRound(EntityReference previousRoundRef, ILocalPluginContext localContext)
        {
            if (previousRoundRef == null)
            {
                return null;
            }

            var query = new QueryExpression(PinballRoundScore.FieldNames.EntityName);
            query.ColumnSet.AddColumn("nx_pointsafterround");
            query.ColumnSet.AddColumn("nx_roundpoints");
            query.Criteria.AddCondition(PinballRoundScore.FieldNames.IdName, ConditionOperator.Equal, previousRoundRef.Id);

            return PinballRoundScore.ParsePinballRoundScore(localContext.CurrentUserService.RetrieveMultiple(query).Entities.FirstOrDefault());
        }

        public static List<PinballRoundScore> GetOtherScoreRoundsSameRoundNumber(Guid scoreId, int roundNumber, ILocalPluginContext localContext)
        {
            var query = new QueryExpression(PinballRoundScore.FieldNames.EntityName);
            query.ColumnSet.AddColumn("nx_roundpoints");
            query.Criteria.AddCondition("nx_roundnumber", ConditionOperator.Equal, roundNumber);

            var queryNxPinballScore = query.AddLink("nx_pinballscore", "nx_pinballscoreid", "nx_pinballscoreid");
            queryNxPinballScore.EntityAlias = "nx_pinballscore";
            // Add conditions to query_nx_pinballscore.LinkCriteria
            queryNxPinballScore.LinkCriteria.AddCondition("nx_pinballscoreid", ConditionOperator.Equal, scoreId);

            var roundScoreEntities = localContext.CurrentUserService.RetrieveMultiple(query).Entities.ToList();

            return roundScoreEntities.Select(PinballRoundScore.ParsePinballRoundScore).ToList();
        }

        private static void UpdateRoundPoints(PinballRoundScore pinballRoundScore, int newScore, ILocalPluginContext localContext)
        {
            var scoreRoundToUpdate = CreateNewRoundScoreEntity(pinballRoundScore.Id);
            scoreRoundToUpdate["nx_roundpoints"] = newScore;

            localContext.CurrentUserService.Update(scoreRoundToUpdate);
        }

        private static Entity CreateNewRoundScoreEntity(Guid pinballScoreRoundId)
        {
            var newRoundScore = new Entity(PinballRoundScore.FieldNames.EntityName)
            {
                [PinballRoundScore.FieldNames.IdName] = pinballScoreRoundId
            };

            return newRoundScore;
        }
    }
}