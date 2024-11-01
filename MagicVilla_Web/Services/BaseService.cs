﻿using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System;
using System.Net.Http.Headers;

namespace MagicVilla_Web.Services
{
    public class BaseService : IBaseService
    {
        public APIResponse responseModel { get; set; }
        public IHttpClientFactory httpClient { get; set; }
        public BaseService(IHttpClientFactory httpClient)
        {
            this.responseModel = new();
            this.httpClient = httpClient;
        }
        public async Task<T> SendAsync<T>(APIRequest apiRequest)
        {
            try
            {
                var client = httpClient.CreateClient("MagicAPI");
                //var client = httpClient.CreateClient();

                HttpRequestMessage message = new HttpRequestMessage();
                message.Headers.Add("Accept", "application/json");

                message.RequestUri = new Uri(apiRequest.Url);
                if (apiRequest.Data != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                        Encoding.UTF8, "application/json");
                }
                switch (apiRequest.ApiType)
                {
                    case SD.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;

                }

                HttpResponseMessage apiResponse = null;

                //if (!string.IsNullOrEmpty(apiRequest.Token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Token);
                //}

                apiResponse = await client.SendAsync(message);

                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                try
                {
                    APIResponse ApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                    if (ApiResponse != null && (apiResponse.StatusCode == System.Net.HttpStatusCode.BadRequest
                        || apiResponse.StatusCode == System.Net.HttpStatusCode.NotFound))
                    {
                        ApiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        ApiResponse.IsSuccess = false;
                        ApiResponse.ErrorMessages = new List<string>() { ApiResponse.Errors.ToString() };
                        var res = JsonConvert.SerializeObject(ApiResponse);
                        var returnObj = JsonConvert.DeserializeObject<T>(res);
                        return returnObj;
                    }
                }
                catch (Exception e)
                {
                    var exceptionResponse = JsonConvert.DeserializeObject<T>(apiContent);
                    return exceptionResponse;
                }

                var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
                return APIResponse;

            }
            catch (Exception e)
            {
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false
                };
                var res = JsonConvert.SerializeObject(dto);
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;
            }
        }
        public async Task<byte[]> ConvertToByteArray(Stream file)
        {
            using (var target = new MemoryStream())
            {
                await file.CopyToAsync(target);

                return target.ToArray();
            }
        }
        public async Task<T> SendAsync1<T>(APIRequest apiRequest)
        {
            try
            {
                //await using var stream = System.IO.File.OpenRead("E:\\text.txt"); //mở file nhanh
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:5001");
                    using (var content = new MultipartFormDataContent())
                    {
                        var response = await client.PostAsync("/api/villaAPI/PostVilla3", new StreamContent(apiRequest.StreamFile));
                        var apiContent = await response.Content.ReadAsStringAsync();
                        var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
                        return APIResponse;
                    }
                }
            }
            catch (Exception e)
            {
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false
                };
                var res = JsonConvert.SerializeObject(dto);
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;
            }

            //try
            //{
            //    var client = new HttpClient
            //    {
            //        BaseAddress = new("https://localhost:5001")
            //    };

            //    await using var stream = System.IO.File.OpenRead("E:\\text.txt");
            //    using var request = new HttpRequestMessage(HttpMethod.Post, "/api/villaAPI/PostVilla3");
            //    using var content = new MultipartFormDataContent
            //    {
            //        { new StreamContent(apiRequest.StreamFile), "file", "test.pdf" }
            //    };

            //    request.Content = content;

            //    var apiResponse = await client.SendAsync(request);

            //    var apiContent = await apiResponse.Content.ReadAsStringAsync();

            //    var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
            //    return APIResponse;
            //}
            //catch (Exception e)
            //{
            //    var dto = new APIResponse
            //    {
            //        ErrorMessages = new List<string> { Convert.ToString(e.Message) },
            //        IsSuccess = false
            //    };
            //    var res = JsonConvert.SerializeObject(dto);
            //    var APIResponse = JsonConvert.DeserializeObject<T>(res);
            //    return APIResponse;
            //}
        }
    }
}
