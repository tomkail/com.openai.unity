// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenAI.Images;
using OpenAI.Tests.Weather;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenAI.Tests
{
    internal class TestFixture_00_02_Tools : AbstractTestFixture
    {
        [Test]
        public void Test_01_GetTools()
        {
            var tools = Tool.GetAllAvailableTools(forceUpdate: true, clearCache: true).ToList();
            Assert.IsNotNull(tools);
            Assert.IsNotEmpty(tools);
            tools.Add(Tool.GetOrCreateTool(OpenAIClient.ImagesEndPoint, nameof(ImagesEndpoint.GenerateImageAsync)));
            var json = JsonConvert.SerializeObject(tools, Formatting.Indented, OpenAIClient.JsonSerializationOptions);
            Debug.Log(json);
        }

        [Test]
        public async Task Test_02_Tool_Funcs()
        {
            var tools = new List<Tool>
            {
                Tool.FromFunc("test_func", Function),
                Tool.FromFunc<string, string, string>("test_func_with_args", FunctionWithArgs),
                Tool.FromFunc("test_func_weather", () => WeatherService.GetCurrentWeatherAsync("my location", WeatherService.WeatherUnit.Celsius))
            };

            var json = JsonConvert.SerializeObject(tools, Formatting.Indented, OpenAIClient.JsonSerializationOptions);
            Debug.Log(json);
            Assert.IsNotNull(tools);
            var tool = tools[0];
            Assert.IsNotNull(tool);
            var result = tool.InvokeFunction<string>();
            Assert.AreEqual("success", result);
            var toolWithArgs = tools[1];
            Assert.IsNotNull(toolWithArgs);
            toolWithArgs.Function.Arguments = new JObject
            {
                ["arg1"] = "arg1",
                ["arg2"] = "arg2"
            };
            var resultWithArgs = toolWithArgs.InvokeFunction<string>();
            Assert.AreEqual("arg1 arg2", resultWithArgs);

            var toolWeather = tools[2];
            Assert.IsNotNull(toolWeather);
            var resultWeather = await toolWeather.InvokeFunctionAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultWeather));
            Debug.Log(resultWeather);
        }

        private string Function()
        {
            return "success";
        }

        private string FunctionWithArgs(string arg1, string arg2)
        {
            return $"{arg1} {arg2}";
        }
    }
}
