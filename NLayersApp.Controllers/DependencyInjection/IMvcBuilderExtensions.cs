using Microsoft.Extensions.DependencyInjection;
using NLayersApp.Controllers.Providers;
using NLayersApp.Persistence.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NLayersApp.Controllers.DependencyInjection
{
    public static class IMvcBuilderExtensions
    {
        public static IMvcBuilder UseDynamicControllers(this IMvcBuilder builder, ITypesResolver typesResolver)
        {
            builder.Services.AddSwaggerDocument();

            return builder.ConfigureApplicationPartManager(
                    partManager =>
                    {
                        partManager.FeatureProviders.Add(new CommandControllerFeatureProvider(typesResolver));
                    });
        }
    }
}
