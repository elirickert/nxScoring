using System;
using Microsoft.Xrm.Sdk;

namespace Nx.Score.Plugins.NxScorePlugins.Model
{
    public class PinballScore
    {
        public static class FieldNames
        {
            public const string EntityName = "nx_pinballscore";
            public const string IdName = "nx_pinballscoreid";

            public const string PointsTotal = "nx_pointstotal";
            public const string PinballGameId = "nx_pinballgameid";
            public const string PointsRound1 = "nx_pointsround1";
            public const string PointsRound2 = "nx_pointsround2";
            public const string PointsRound3 = "nx_pointsround3";
            public const string PointsRound4 = "nx_pointsround4";
            public const string PointsRound5 = "nx_pointsround5";
        }

        public Guid Id { get; set; }
        public int PointsTotal { get; set; }
        public int PointsRound1 { get; set; }
        public int PointsRound2 { get; set; }
        public int PointsRound3 { get; set; }
        public int PointsRound4 { get; set; }
        public int PointsRound5 { get; set; }
        public EntityReference PinballGameId {get; set; }

        public static PinballScore ParsePinballScore(Entity pinballScoreEntity)
        {
            return new PinballScore()
            {
                Id = pinballScoreEntity.Id,
                PointsTotal = EntityParsing.GetIntValueFromEntity(pinballScoreEntity, FieldNames.PointsTotal, true),
                PointsRound1 = EntityParsing.GetIntValueFromEntity(pinballScoreEntity, FieldNames.PointsRound1, true),
                PointsRound2  = EntityParsing.GetIntValueFromEntity(pinballScoreEntity, FieldNames.PointsRound2, true),
                PointsRound3 = EntityParsing.GetIntValueFromEntity(pinballScoreEntity, FieldNames.PointsRound3, true),
                PointsRound4 = EntityParsing.GetIntValueFromEntity(pinballScoreEntity, FieldNames.PointsRound4, true),
                PointsRound5 = EntityParsing.GetIntValueFromEntity(pinballScoreEntity, FieldNames.PointsRound5, true),
                PinballGameId = EntityParsing.GetEntityReferenceFromEntity(pinballScoreEntity, FieldNames.PinballGameId)
            };
        }
    }
}