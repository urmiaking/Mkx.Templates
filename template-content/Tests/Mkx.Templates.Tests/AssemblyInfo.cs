using Xunit.v3;
using Xunit.Sdk;
using Xunit;
[assembly: CaptureConsole]

// SQLite in-memory test hosts use non-concurrent provider initialization.
[assembly: Parallelization(Mode = ParallelMode.None)]
