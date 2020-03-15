using Cassandra;
using CassandraCats.API.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

[assembly: WebJobsStartup(typeof(Startup))]
namespace CassandraCats.API.Helpers
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });

            var config = (IConfiguration)builder.Services.First(d => d.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            builder.Services.AddSingleton((s) =>
            {
                SSLOptions options = new SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);
                options.SetHostNameResolver((ipAddress) => config[Constants.CASSANDRA_CONTACT_POINT]);
                Cluster cluster = Cluster.Builder()
                    .WithCredentials(config[Constants.USER_NAME], config[Constants.PASSWORD])
                    .WithPort(int.Parse(config[Constants.CASSANDRA_PORT]))
                    .AddContactPoint(config[Constants.CASSANDRA_PORT])
                    .WithSSL(options)
                    .Build();

                return cluster;
            });
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            return false;
        }
    }
}
