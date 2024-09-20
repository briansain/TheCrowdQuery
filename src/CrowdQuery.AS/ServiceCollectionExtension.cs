using System;
using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Akka.Persistence.Sql.Hosting;
using CrowdQuery.AS.Actors.Question;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdQuery.AS;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCrowdQueryAkka(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new CrowdQueryAkkaConfiguration();
        configuration.Bind("Akka", config);
        if (config.IsInvalid())
        {
            throw new ArgumentException("Must provide a valid 'Akka' section config");
        }

        services.AddAkka("crowdquery-service", builder =>
        {
            builder.ConfigureLoggers(configLoggers =>
            {
                configLoggers.LogLevel = LogLevel.DebugLevel;
                configLoggers.LogConfigOnStart = true;
                configLoggers.ClearLoggers();
                configLoggers.AddLogger<SerilogLogger>();
            })
            .WithSqlPersistence(config.ConnectionString, ProviderName.PostgreSQL15)
            .WithActors((actorSystem, registry) =>
            {
                var questionManager = actorSystem.ActorOf(QuestionManager.PropsFor(), "question-manager");
                registry.Register<QuestionManager>(questionManager);
            });
        });
        return services;
    }
}

/// <summary>
/// Binds to the "Akka" configuration object
/// </summary>
public class CrowdQueryAkkaConfiguration
{
    public string ConnectionString {get;set;} = string.Empty;
    internal bool IsInvalid() 
    {
       return string.IsNullOrEmpty(ConnectionString); 
    }
}
