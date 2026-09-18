namespace Gorse.NET.Tests;

/// <summary>
/// Integration tests. They need a Gorse instance loaded with the upstream test data
/// (see gorse-io/gorse client/setup-test.sh) and run only when GORSE_TEST_ENDPOINT is
/// set, e.g. GORSE_TEST_ENDPOINT=http://127.0.0.1:8088. Offline tests live in OfflineTests.
/// </summary>
[Category("Integration")]
public partial class Tests
{
    private const string API_KEY = "zhenghaoz";

    private static readonly string? Endpoint = Environment.GetEnvironmentVariable("GORSE_TEST_ENDPOINT");

    private Gorse client = null!;

    [OneTimeSetUp]
    public void ConnectToGorse()
    {
        if (string.IsNullOrEmpty(Endpoint))
        {
            Assert.Ignore("GORSE_TEST_ENDPOINT is not set; skipping tests that need a live Gorse server.");
        }
        client = new Gorse(Endpoint, Environment.GetEnvironmentVariable("GORSE_TEST_API_KEY") ?? API_KEY);
    }

    [OneTimeTearDown]
    public void DisposeClient()
    {
        client?.Dispose();
    }
}
