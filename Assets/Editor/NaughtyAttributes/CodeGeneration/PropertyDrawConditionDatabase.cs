// This class is auto generated

using System;
using System.Collections.Generic;

namespace NaughtyAttributes.Editor
{
    public static class PropertyDrawConditionDatabase
    {
        private static Dictionary<Type, PropertyDrawCondition> drawConditionsByAttributeType;

        static PropertyDrawConditionDatabase()
        {
            drawConditionsByAttributeType = new Dictionary<Type, PropertyDrawCondition>();
            drawConditionsByAttributeType[typeof(ShowIfAttribute)] = new ShowIfPropertyDrawCondition();
drawConditionsByAttributeType[typeof(HideIfAttribute)] = new HideIfPropertyDrawCondition();

        }

        public static PropertyDrawCondition GetDrawConditionForAttribute(Type attributeType)
        {
            PropertyDrawCondition drawCondition;
            if (drawConditionsByAttributeType.TryGetValue(attributeType, out drawCondition))
            {
                return drawCondition;
            }
            else
            {
                return null;
            }
        }
    }
}

