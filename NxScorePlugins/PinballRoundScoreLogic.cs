using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Nx.Score.Plugins.NxScorePlugins
{
    public class PinballRoundScoreLogic
    {
        public static void UpdateCurrentRoundScore(Entity pinballScoreRound, ILocalPluginContext localContext)
        {
            var previousScoreRound = GetPreviousScoreRound(pinballScoreRound, localContext);
            var previousScoreAfterRound = 0;
            if (previousScoreRound != null)
            {
                previousScoreAfterRound = GetIntValueFromEntity(previousScoreRound, "nx_pointsafterround");
            }

            var currentValueAfterRound = GetIntValueFromEntity(pinballScoreRound, "nx_pointsafterround");

            var scoreRoundToUpdate = CreateNewRoundScore(pinballScoreRound.Id);
            scoreRoundToUpdate["nx_roundpoints"] = currentValueAfterRound - previousScoreAfterRound;

            localContext.CurrentUserService.Update(scoreRoundToUpdate);
        }

        public static void UpdateScoreTotals(Entity pinballScoreRound, ILocalPluginContext localContext)
        {
            var pinballScoreRef = GetPinballScoreRefFromRound(pinballScoreRound);
            var pinballScore = GetPinballScoreFromId(pinballScoreRef, localContext);
            var roundNumber = GetOptionSetValueFromEntity(pinballScoreRound, "nx_roundnumber");
            var allScoreRounds = GetOtherScoresSameRound(pinballScoreRef, roundNumber, pinballScoreRound, localContext);

            var newTotalScoreForRound = allScoreRounds.Sum(x => GetIntValueFromEntity(x, "nx_roundpoints"));
            pinballScore[$"nx_pointsround{roundNumber}"] = newTotalScoreForRound;

            var round1 = GetIntValueFromEntityOrDefault(pinballScore, "nx_pointsround1");
            var round2 = GetIntValueFromEntityOrDefault(pinballScore, "nx_pointsround2");
            var round3 = GetIntValueFromEntityOrDefault(pinballScore, "nx_pointsround3");
            var round4 = GetIntValueFromEntityOrDefault(pinballScore, "nx_pointsround4");
            var round5 = GetIntValueFromEntityOrDefault(pinballScore, "nx_pointsround5");

            var totalScore = round1 + round2 + round3 + round4 + round5;

            var scoreToUpdate = CreateNewScore(pinballScoreRef.Id);
            scoreToUpdate[$"nx_pointsround{roundNumber}"] = newTotalScoreForRound;
            scoreToUpdate["nx_pointstotal"] = totalScore;

            localContext.CurrentUserService.Update(scoreToUpdate);
        }

        public static void UpdateGameWinnerAndStats(Entity pinballScoreRound, ILocalPluginContext localContext)
        {
            var pinballScoreRef = GetPinballScoreRefFromRound(pinballScoreRound);
            var pinballScore = GetPinballScoreFromId(pinballScoreRef, localContext);

            var pinballGameRef = GetPinballGameRefFromScore(pinballScore);

            var pinballScores = GetAllScoresFromGameId(pinballGameRef.Id, localContext);

            var averageScore = pinballScores.Sum(x => GetIntValueFromEntity(x, "nx_pointstotal"))/pinballScores.Count;
            var winnerScore = pinballScores.OrderByDescending(x => x["nx_pointstotal"]).FirstOrDefault();

            var pinballGameToUpdate = CreateNewGame(pinballGameRef.Id);
            pinballGameToUpdate["nx_averagepoints"] = averageScore;
            if (winnerScore != null)
            {
                pinballGameToUpdate["nx_winnerscore"] = new EntityReference("nx_pinballroundscore", winnerScore.Id);

            }

            localContext.CurrentUserService.Update(pinballGameToUpdate);
        }

        private static List<Entity> GetAllScoresFromGameId(Guid pinballGameId, ILocalPluginContext localContext)
        {
            var query = new QueryExpression("nx_pinballscore");
            query.ColumnSet.AddColumn("nx_pointstotal");
            query.Criteria.AddCondition("nx_pinballgameid", ConditionOperator.Equal, pinballGameId);

            return localContext.CurrentUserService.RetrieveMultiple(query).Entities.ToList();;

        }

        private static Entity GetPinballScoreFromId(EntityReference pinballScoreRef, ILocalPluginContext localContext)
        {
            return localContext.CurrentUserService.Retrieve("nx_pinballscore", pinballScoreRef.Id, new ColumnSet(true));
        }

        private static Entity GetPinballGameFromId(EntityReference pinballGameRef, ILocalPluginContext localContext)
        {
            return localContext.CurrentUserService.Retrieve("nx_pinballgame", pinballGameRef.Id, new ColumnSet(true));
        }

        private static int GetIntValueFromEntity(Entity pinballScoreRound, string attributeName)
        {
            if (pinballScoreRound.Attributes.Contains(attributeName) && pinballScoreRound[attributeName] is int)
            {
                return (int)pinballScoreRound[attributeName];
            }
            else
            {
                throw new InvalidPluginExecutionException($"{attributeName} is not set");
            }
        }

        private static int GetIntValueFromEntityOrDefault(Entity pinballScoreRound, string attributeName)
        {
            var value = 0;
            if (pinballScoreRound.Attributes.Contains(attributeName) && pinballScoreRound[attributeName] is int)
            {
                value = (int)pinballScoreRound[attributeName];
            }

            return value;
        }

        private static Entity GetPreviousScoreRound(Entity pinballScoreRound, ILocalPluginContext localContext)
        {
            var previousRoundId = GetPreviousScoreRoundId(pinballScoreRound);

            if (previousRoundId == null)
            {
                return null;
            }

            var query = new QueryExpression("nx_pinballroundscore");
            query.ColumnSet.AddColumn("nx_pointsafterround");
            query.ColumnSet.AddColumn("nx_roundpoints");
            query.Criteria.AddCondition("nx_pinballroundscoreid", ConditionOperator.Equal, previousRoundId.Id);

            return localContext.CurrentUserService.RetrieveMultiple(query).Entities.FirstOrDefault();
        }

        private static List<Entity> GetOtherScoresSameRound(EntityReference pinballScoreRef, int roundNumber, Entity pinballScoreRound, ILocalPluginContext localContext)
        {
            var query = new QueryExpression("nx_pinballroundscore");
            query.ColumnSet.AddColumn("nx_roundpoints");
            query.Criteria.AddCondition("nx_roundnumber", ConditionOperator.Equal, roundNumber);

            var queryNxPinballScore = query.AddLink("nx_pinballscore", "nx_pinballscoreid", "nx_pinballscoreid");
            queryNxPinballScore.EntityAlias = "nx_pinballscore";
            // Add conditions to query_nx_pinballscore.LinkCriteria
            queryNxPinballScore.LinkCriteria.AddCondition("nx_pinballscoreid", ConditionOperator.Equal, pinballScoreRef.Id);

            return localContext.CurrentUserService.RetrieveMultiple(query).Entities.ToList();
        }

        private static EntityReference GetPinballScoreRefFromRound(Entity entity)
        {
            return GetPinballScoreRefFromRound(entity, "nx_pinballscoreid");
        }

        private static EntityReference GetPinballGameRefFromScore(Entity entity)
        {
            return GetPinballScoreRefFromRound(entity, "nx_pinballgameid");
        }

        private static EntityReference GetPinballScoreRefFromRound(Entity entity, string attributeName)
        {
            if (entity.Attributes.Contains(attributeName) && entity[attributeName] is EntityReference)
            {
                EntityReference pinballScoreRef = (EntityReference)entity[attributeName];
                return pinballScoreRef;
            }
            else
            {
                return null;
            }
        }

        private static Entity CreateNewRoundScore(Guid pinballScoreRoundId)
        {
            Entity newRoundScore = new Entity("nx_pinballroundscore");
            newRoundScore["nx_pinballroundscoreid"] = pinballScoreRoundId;

            return newRoundScore;
        }

        private static Entity CreateNewScore(Guid pinballScoreId)
        {
            Entity newRoundScore = new Entity("nx_pinballscore");
            newRoundScore["nx_pinballscoreid"] = pinballScoreId;

            return newRoundScore;
        }

        private static Entity CreateNewGame(Guid pinballScoreId)
        {
            Entity newRoundScore = new Entity("nx_pinballgame");
            newRoundScore["nx_pinballgameid"] = pinballScoreId;

            return newRoundScore;
        }

        private static EntityReference GetPreviousScoreRoundId(Entity pinballScoreRound)
        {
            EntityReference previousRoundRef = null;
            if (pinballScoreRound.Attributes.Contains("nx_previousroundid") && pinballScoreRound["nx_previousroundid"] is EntityReference)
            {
                return (EntityReference)pinballScoreRound["nx_previousroundid"];
            }

            return previousRoundRef;
        }

        private static int GetOptionSetValueFromEntity(Entity entity, string attributeName)
        {
            if (entity.Attributes.Contains(attributeName))
            {
                var roundNumberObj = entity[attributeName];
                if (roundNumberObj is AliasedValue aliasedValue)
                {
                    return (int)aliasedValue.Value;
                }
                else if (roundNumberObj is OptionSetValue optionSetValue)
                {
                    return optionSetValue.Value;
                }
                else if (roundNumberObj is int intValue)
                {
                    return intValue;
                }
                else
                {
                    // For the given JSON, the "Value" property is inside the attribute value
                    dynamic valueObj = roundNumberObj;
                    return (int)valueObj?.Value;
                }
            }
            else
            {
                throw new InvalidPluginExecutionException($"{attributeName} is not set");
            }
        }
    }
}