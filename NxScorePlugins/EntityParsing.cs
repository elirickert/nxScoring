using Microsoft.Xrm.Sdk;

namespace Nx.Score.Plugins.NxScorePlugins
{
    public static class EntityParsing
    {
        public static int GetOptionSetValueFromEntity(Entity entity, string attributeName)
        {
            if (entity.Attributes.Contains(attributeName))
            {
                var optionSetObject = entity[attributeName];
                if (optionSetObject is AliasedValue aliasedValue)
                {
                    return (int)aliasedValue.Value;
                }
                else if (optionSetObject is OptionSetValue optionSetValue)
                {
                    return optionSetValue.Value;
                }
                else if (optionSetObject is int intValue)
                {
                    return intValue;
                }
                else
                {
                    // For the given JSON, the "Value" property is inside the attribute value
                    dynamic valueObj = optionSetObject;
                    return (int)valueObj?.Value;
                }
            }
            else
            {
                throw new InvalidPluginExecutionException($"{attributeName} is not set");
            }
        }

        public static EntityReference GetEntityReferenceFromEntity(Entity entity, string attributeName)
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

        public static int GetIntValueFromEntity(Entity pinballScoreRound, string attributeName, bool returnDefault = false)
        {
            if (pinballScoreRound.Attributes.Contains(attributeName) && pinballScoreRound[attributeName] is int)
            {
                return (int)pinballScoreRound[attributeName];
            }
            else
            {
                return !returnDefault ? throw new InvalidPluginExecutionException($"{attributeName} is not set") : 0;
            }
        }
    }
}