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
    public class CommandControllerFeature : ControllerFeature
    {
        public CommandControllerFeature(ITypesResolver typesResolver)
        {           
            foreach (var current in typesResolver.RegisteredTypes)
            {
                Type keyType = current.GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null).PropertyType ?? typeof(string); 
                //current.GetProperties().FirstOrDefault(p => p.Name.Contains("Id")).PropertyType;

                Type apiControllerType = typeof(CommandController<,>).MakeGenericType(new Type[] { current, keyType });

                Controllers.Add(apiControllerType.GetTypeInfo());
            }
        }
    }
}
