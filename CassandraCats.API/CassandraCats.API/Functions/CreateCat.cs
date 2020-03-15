using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using cass = Cassandra;
using CassandraCats.API.Helpers;
using Cassandra.Mapping;
using CassandraCats.API.Models;

namespace CassandraCats.API.Functions
{
    public class CreateCat
    {
        private readonly IConfiguration _config;
        private readonly ILogger<CreateCat> _logger;
        private readonly cass.Cluster _cluster;

        public CreateCat(
            IConfiguration config,
            ILogger<CreateCat> logger,
            cass.Cluster cluster)
        {

            _config = config;
            _logger = logger;
            _cluster = cluster;
        }

        [FunctionName(nameof(CreateCat))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateCat")] HttpRequest req)
        {
            cass.ISession session = _cluster.Connect(_config[Constants.KEYSPACE_NAME]);
            IMapper mapper = new Mapper(session);
            IActionResult returnValue = null;

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var input = JsonConvert.DeserializeObject<Cat>(requestBody);

                var cat = new Cat()
                {
                    cat_id = Guid.NewGuid().ToString(),
                    cat_name = input.cat_name,
                    cat_type = input.cat_type,
                    cat_age = input.cat_age
                };

                mapper.Insert<Cat>(cat);
                returnValue = new OkObjectResult(cat);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not insert new cat. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;

        }
    }
}
