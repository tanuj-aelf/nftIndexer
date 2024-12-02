using AeFinder.App.TestBase;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace nftIndexer;

[DependsOn(
    typeof(AeFinderAppTestBaseModule),
    typeof(nftIndexerModule))]
public class nftIndexerTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AeFinderAppEntityOptions>(options => { options.AddTypes<nftIndexerModule>(); });
        
        // Add your Processors.
        // context.Services.AddSingleton<MyLogEventProcessor>();
    }
}