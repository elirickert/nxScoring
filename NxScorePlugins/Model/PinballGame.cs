using System;
using Microsoft.Xrm.Sdk;

namespace Nx.Score.Plugins.NxScorePlugins.Model
{
    public class PinballGame
    {
        public static class FieldNames
        {
            public const string EntityName = "nx_pinballgame";
            public const string IdName = "nx_pinballgameid";

            public const string AveragePointsFieldName = "nx_averagepoints";
            public const string WinnersScoreFieldName = "nx_winnerscore";
        }

        public Guid Id { get; set; }
        public int AveragePoints { get; set; }
        // public EntityReference WinnersScore { get; set; } // nx_winnerscore
        public Guid? WinnersScoreId { get; set; }
    }
}