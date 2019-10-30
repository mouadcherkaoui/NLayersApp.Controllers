using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using NLayersApp.Controllers.Controllers;
using NLayersApp.Persistence.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NLayersApp.Controllers.Providers
{
    public class CommandControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        readonly ITypesResolver _typesResolver; 
        public CommandControllerFeatureProvider(ITypesResolver typesResolver)
        {
            _typesResolver = typesResolver;
        }
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            IEnumerable<TypeInfo> types = _typesResolver.RegisteredTypes.Select(t => t.GetTypeInfo());

            foreach (var current in types)
            {
                Type keyType = current.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null)?.PropertyType ?? typeof(string);
                Type apiControllerType = typeof(CommandController<,>).MakeGenericType(new Type[] { keyType, current.AsType() });

                feature.Controllers.Add(apiControllerType.GetTypeInfo());
            }
        }
    }
}
