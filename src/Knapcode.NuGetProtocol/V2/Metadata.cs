using System.Collections.Generic;

namespace Knapcode.NuGetProtocol.V2
{
    public class Metadata
    {
        public static Metadata Empty => new Metadata
        {
            PackageEntityType = new EntityType
            {
                Properties = new List<EntityProperty>(),
            }
        };

        public EntityType PackageEntityType { get; set; }
    }
}
