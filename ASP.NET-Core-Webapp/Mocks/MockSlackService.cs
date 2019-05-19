﻿using ASP.NET_Core_Webapp.Helpers;
using ASP.NET_Core_Webapp.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ASP.NET_Core_Webapp.Mocks
{
    public class MockSlackService : ISlackService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public MockSlackService(IConfiguration config, IHttpClientFactory httpCF)
        {
            this.configuration = config;
            this.httpClientFactory = httpCF;
        }

        public async Task SendEmail(string email, string messageToSend)
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration["SlackBotToken"]);

            //Create an email user lookup request:
            var emailLookupRequest = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/users.lookupByEmail");

            var list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("email", email));

            emailLookupRequest.Content = new FormUrlEncodedContent(list);
            var response = await httpClient.SendAsync(emailLookupRequest);

            EmailLookupResponse responseObject = new EmailLookupResponse();

            //Deserialize the response (first create a string from the request's content, 
            //then deserialize it.
            responseObject = JsonConvert.DeserializeObject<EmailLookupResponse>(await response.Content.ReadAsStringAsync());

            //Create the post message request:
            var postMessageRequest = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/chat.postMessage");

            var messageRequestBody = new List<KeyValuePair<string, string>>();
            messageRequestBody.Add(new KeyValuePair<string, string>("channel", responseObject.user.id));
            messageRequestBody.Add(new KeyValuePair<string, string>("text", messageToSend));
            postMessageRequest.Content = new FormUrlEncodedContent(messageRequestBody);

            await httpClient.SendAsync(postMessageRequest);
        }
    }
}
