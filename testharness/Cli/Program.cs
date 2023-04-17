using Aargh;
using Aargh.Extensions.SpectreConsole;

using GitOpsConfig.TestHarness.Cli;

using static Spectre.Console.Color;

DebugOutput.UseWriter(SpectreDebugOutputWriter.Default());
Runner runner = new(() => new Cli()
    .UseSpectreConsole(builder => builder
        .SetStyles(labelStyle: GreenYellow,
            argNameStyle: Yellow1,
            argDescriptionStyle: LightSlateGrey,
            commandDescriptionStyle: DodgerBlue1))
    .DisplayHelpOnError()
    .ScanEntryAssembly());
#if DEBUG
//return await runner.UseSpectreDebugRepl(useDefaultStyle: true).RunAsync();
return await runner.UseDebugRepl().RunAsync();
#else
return await runner.RunAsync();
#endif
