using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Nx.Score.Plugins.NxScorePlugins.Model;

namespace Nx.Score.Plugins.NxScorePlugins.Logic
{
    public static class PinballScoreLogic
    {
        public static List<PinballScore> GetAllScoresFromGameId(Guid pinballGameId, ILocalPluginContext localContext)
        {
            var query = new QueryExpression(PinballScore.FieldNames.EntityName);
            query.ColumnSet.AddColumn(PinballScore.FieldNames.PointsTotal);
            query.Criteria.AddCondition(PinballScore.FieldNames.PinballGameId, ConditionOperator.Equal, pinballGameId);

            var entityList = localContext.CurrentUserService.RetrieveMultiple(query).Entities.ToList();

            return entityList.Select(PinballScore.ParsePinballScore).ToList();
        }

        public static EntityReference GetPinballGameRefFromScore(Entity entity)
        {
            return EntityParsing.GetEntityReferenceFromEntity(entity, PinballScore.FieldNames.PinballGameId);
        }

        public static PinballScore GetPinballScoreFromId(Guid pinballScoreId, ILocalPluginContext localContext)
        {
            return PinballScore.ParsePinballScore(localContext.CurrentUserService.Retrieve(PinballScore.FieldNames.EntityName, pinballScoreId, new ColumnSet(true)));
        }

        public static void UpdateScoreTotals(PinballRoundScore pinballScoreRound, List<PinballRoundScore> allScoresThisRoundNumber, ILocalPluginContext localContext)
        {
            var pinballScore = GetPinballScoreFromId(pinballScoreRound.PinballScoreId.Id, localContext);

            pinballScore = UpdateTotalScoreForRoundNumber(pinballScoreRound.RoundNumber, pinballScore, allScoresThisRoundNumber, localContext);

            var totalScore = pinballScore.PointsRound1 + pinballScore.PointsRound2 + pinballScore.PointsRound3 + pinballScore.PointsRound4 + pinballScore.PointsRound5;

            var scoreToUpdate = CreateNewScoreEntity(pinballScore.Id);
            scoreToUpdate[$"nx_pointsround1"] = pinballScore.PointsRound1;
            scoreToUpdate[$"nx_pointsround2"] = pinballScore.PointsRound2;
            scoreToUpdate[$"nx_pointsround3"] = pinballScore.PointsRound3;
            scoreToUpdate[$"nx_pointsround4"] = pinballScore.PointsRound4;
            scoreToUpdate[$"nx_pointsround5"] = pinballScore.PointsRound5;
            scoreToUpdate["nx_pointstotal"] = totalScore;

            localContext.CurrentUserService.Update(scoreToUpdate);
        }

        private static PinballScore UpdateTotalScoreForRoundNumber(int roundNumber, PinballScore score, List<PinballRoundScore> allScoresThisRoundNumber, ILocalPluginContext localContext)
        {
            var newTotalScoreForRound = allScoresThisRoundNumber.Sum(x => x.RoundPoints);

            score = UpdateRoundPointsOnScore(roundNumber, newTotalScoreForRound, score);

            return score;
        }

        public static PinballScore UpdateRoundPointsOnScore(int roundNumber, int points, PinballScore score)
        {
            switch (roundNumber)
            {
                case 1:
                    score.PointsRound1 = points;
                    break;
                case 2:
                    score.PointsRound2 = points;
                    break;
                case 3:
                    score.PointsRound3 = points;
                    break;
                case 4:
                    score.PointsRound4 = points;
                    break;
                case 5:
                    score.PointsRound5 = points;
                    break;
                default:
                    throw new InvalidPluginExecutionException($"Invalid round number: {roundNumber}");
            }

            return score;
        }

        private static Entity CreateNewScoreEntity(Guid pinballScoreId)
        {
            var newScore = new Entity(PinballScore.FieldNames.EntityName)
            {
                [PinballScore.FieldNames.IdName] = pinballScoreId
            };

            return newScore;
        }
    }



}