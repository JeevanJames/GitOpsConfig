using Aargh;

namespace GitOpsConfig.TestHarness.Cli;

[Program(Name = "test-harness")]
#pragma warning disable S2094 // Classes should not be empty
public sealed class Cli : ConsoleProgram
#pragma warning restore S2094 // Classes should not be empty
{
}
