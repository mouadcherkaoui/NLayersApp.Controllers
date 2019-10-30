using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NLayersApp.Controllers.Conventions;
using NLayersApp.CQRS.Requests;

namespace NLayersApp.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [BindProperties(SupportsGet = true)]
    [CommandControllerNameConvention(typeof(CommandController<, >))]
    public class CommandController<TKey, TEntity> : Controller
    {
        IMediator _mediator;
        public CommandController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IEnumerable<TEntity>> Get()
        {
            var request = new ReadEntitiesRequest<TEntity>();
            var result = await _mediator.Send(request);
            return result;
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<TEntity> Get(TKey id)
        {
            var request = new ReadEntityRequest<TKey, TEntity>(id);
            var result = await _mediator.Send(request);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TEntity value)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(new { Errors = ModelState.Values, Payload = value });
            }
            var request = new CreateEntityRequest<TEntity>(value);
            var result = await _mediator.Send(request);
            return new OkObjectResult(result) {  StatusCode = 201 };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(TKey id, [FromBody] TEntity value)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(new { Errors = ModelState.Values, Payload = value });
            }
            var request = new UpdateEntityRequest<TKey, TEntity>(id, value);
            var result = await _mediator.Send(request);
            return new OkObjectResult(result) { StatusCode = 200 };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(TKey id)
        {
            var request = new DeleteEntityRequest<TKey, TEntity>(id);
            var result = await _mediator.Send(request);
            return new NoContentResult() { };

        }
    }
}
