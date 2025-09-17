using System;
using Microsoft.Xrm.Sdk;

namespace Nx.Score.Plugins.NxScorePlugins.Model
{
    public class PinballRoundScore
    {
        public static class FieldNames
        {
            public const string EntityName = "nx_pinballroundscore";
            public const string IdName = "nx_pinballroundscoreid";

            public const string PointsAfterRound = "nx_pointsafterround";
            public const string RoundPoints = "nx_roundpoints";
            public const string RoundNumber = "nx_roundnumber";
            public const string PreviousRoundId = "nx_previousroundid";
            public const string PinballScoreId = "nx_pinballscoreid";
            public const string PinballGameId = "nx_pinballgameid";
        }

        public Guid Id { get; set; }
        public int PointsAfterRound { get; set; }
        public int RoundPoints { get; set; }
        public int RoundNumber { get; set; }
        public EntityReference PreviousRoundScoreId { get; set; }
        public EntityReference PinballScoreId { get; set; }
        public EntityReference PinballGameId { get; set; }

        public static PinballRoundScore ParsePinballRoundScore(Entity pinballScoreRoundEntity)
        {
            return new PinballRoundScore()
            {
                Id = pinballScoreRoundEntity.Id,
                PointsAfterRound = EntityParsing.GetIntValueFromEntity(pinballScoreRoundEntity, FieldNames.PointsAfterRound, true),
                RoundPoints = EntityParsing.GetIntValueFromEntity(pinballScoreRoundEntity, FieldNames.RoundPoints, true),
                RoundNumber = EntityParsing.GetOptionSetValueFromEntity(pinballScoreRoundEntity, FieldNames.RoundNumber),
                PinballGameId = EntityParsing.GetEntityReferenceFromEntity(pinballScoreRoundEntity, FieldNames.PinballGameId),
                PreviousRoundScoreId = EntityParsing.GetEntityReferenceFromEntity(pinballScoreRoundEntity, FieldNames.PreviousRoundId),
                PinballScoreId = EntityParsing.GetEntityReferenceFromEntity(pinballScoreRoundEntity, FieldNames.PinballScoreId)
            };
        }
    }
}