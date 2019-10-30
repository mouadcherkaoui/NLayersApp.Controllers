using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NLayersApp.Controllers.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NLayersApp.Controllers.Conventions
{
    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CommandControllerNameConvention : Attribute, IControllerModelConvention//, IActionModelConvention
    {
        private Type _controllerType;
        public CommandControllerNameConvention(Type controllerType)
        {
            _controllerType = controllerType;
        }
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType &&
                controller.ControllerType.GetGenericTypeDefinition() == typeof(CommandController<,>))
            {
                var entityType = controller.ControllerType.GenericTypeArguments[1];
                controller.ControllerName = $"{entityType.Name}s";
            }
        }
    }
}
