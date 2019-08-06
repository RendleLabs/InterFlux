using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RendleLabs.Interflux.ServerTests
{
    public class BasicTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public BasicTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ForwardsSingleLine()
        {
            var (client, list) = CreateClient();
            const string data = "thing,x=Foo y=0 1565090724516";
            await client.PostAsync("/write", new StringContent(data));
            Assert.Contains(data, list);
        }

        [Fact]
        public async Task ForwardsTwoLines()
        {
            var (client, list) = CreateClient();
            const string data1 = "thing,x=Foo y=1 1565090724516";
            const string data2 = "thing,x=Foo y=2 1565090724516";
            await client.PostAsync("/write", new StringContent($"{data1}\n{data2}"));
            Assert.Contains(data1, list);
            Assert.Contains(data2, list);
        }

        private (HttpClient, List<string>) CreateClient()
        {
            var testForwarders = new TestForwarders();
            var client = _factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services => { services.AddSingleton<IForwarders>(testForwarders); });
                })
                .CreateClient();
            return (client, testForwarders.Lines);
        }
    }

    internal class TestForwarders : IForwarders
    {
        public List<string> Lines { get; } = new List<string>();

        public ValueTask AddAsync(Line line, CancellationToken token)
        {
            Lines.Add(Encoding.UTF8.GetString(line.Memory.Span));
            return default;
        }
    }
}
