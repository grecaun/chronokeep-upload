using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using results_uploader.Objects.API;
using results_uploader.Objects;
using results_uploader.Objects.Helpers;

namespace results_uploader.Network.API
{
    public class APIHandlers
    {
        private static HttpClient GetHttpClient()
        {
            var handler = new WinHttpHandler();
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        public static async Task<GetEventsResponse> GetEvents(ResultsAPI api)
        {
            string content;
            Log.D("Network.API.APIHandlers", "Getting events.");
            try
            {
                using (var client = GetHttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(api.URL + "event/my")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", api.AuthToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Log.D("Network.API.APIHandlers", "Status code ok.");
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetEventsResponse>(json);
                        return result;
                    }
                    Log.D("Network.API.APIHandlers", "Status code not ok.");
                    var errjson = await response.Content.ReadAsStringAsync();
                    var errresult = JsonConvert.DeserializeObject<ErrorResponse>(errjson);
                    content = errresult.Message;
                }
            }
            catch (Exception ex)
            {
                Log.D("Network.API.APIHandlers", "Exception thrown.");
                throw new APIException("Exception thrown getting events: " + ex.Message);
            }
            throw new APIException(content);
        }

        public static async Task<GetEventYearsResponse> GetEventYears(ResultsAPI api, string slug)
        {
            string content;
            Log.D("Network.API.APIHandlers", "Getting event years.");
            try
            {
                using (var client = GetHttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(api.URL + "event-year/event"),
                        Content = new StringContent(
                            JsonConvert.SerializeObject(new GetEventRequest
                            {
                                Slug = slug
                            }),
                            Encoding.UTF8,
                            "application/json"
                            )
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", api.AuthToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Log.D("Network.API.APIHandlers", "Status code ok.");
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<GetEventYearsResponse>(json);
                        return result;
                    }
                    Log.D("Network.API.APIHandlers", "Status code not ok.");
                    var errjson = await response.Content.ReadAsStringAsync();
                    var errresult = JsonConvert.DeserializeObject<ErrorResponse>(errjson);
                    content = errresult.Message;
                }
            }
            catch (Exception ex)
            {
                Log.D("Network.API.APIHandlers", "Exception thrown.");
                throw new APIException("Exception thrown getting event years: " + ex.Message);
            }
            throw new APIException(content);
        }

        public static async Task<ModifyEventResponse> AddEvent(ResultsAPI api, APIEvent ev)
        {
            string content;
            Log.D("Network.API.APIHandlers", "Adding event.");
            try
            {
                using (var client = GetHttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(api.URL + "event/add"),
                        Content = new StringContent(
                            JsonConvert.SerializeObject(new ModifyEventRequest
                            {
                                Event = ev
                            }),
                            Encoding.UTF8,
                            "application/json"
                            )
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", api.AuthToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Log.D("Network.API.APIHandlers", "Status code ok.");
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ModifyEventResponse>(json);
                        return result;
                    }
                    Log.D("Network.API.APIHandlers", "Status code not ok.");
                    var errjson = await response.Content.ReadAsStringAsync();
                    var errresult = JsonConvert.DeserializeObject<ErrorResponse>(errjson);
                    content = errresult.Message;
                }
            }
            catch (Exception ex)
            {
                Log.D("Network.API.APIHandlers", "Exception thrown.");
                throw new APIException("Exception thrown adding event: " + ex.Message);
            }
            throw new APIException(content);
        }

        public static async Task<EventYearResponse> AddEventYear(ResultsAPI api, string slug, APIEventYear year)
        {
            string content;
            Log.D("Network.API.APIHandlers", "Adding event year.");
            try
            {
                using (var client = GetHttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(api.URL + "event-year/add"),
                        Content = new StringContent(
                            JsonConvert.SerializeObject(new ModifyEventYearRequest
                            {
                                Slug = slug,
                                Year = year
                            }),
                            Encoding.UTF8,
                            "application/json"
                            )
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", api.AuthToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Log.D("Network.API.APIHandlers", "Status code ok.");
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<EventYearResponse>(json);
                        return result;
                    }
                    Log.D("Network.API.APIHandlers", "Status code not ok.");
                    var errjson = await response.Content.ReadAsStringAsync();
                    var errresult = JsonConvert.DeserializeObject<ErrorResponse>(errjson);
                    content = errresult.Message;
                }
            }
            catch (Exception ex)
            {
                Log.D("Network.API.APIHandlers", "Exception thrown.");
                throw new APIException("Exception thrown adding event year: " + ex.Message);
            }
            throw new APIException(content);
        }

        public static async Task<AddResultsResponse> UploadResults(ResultsAPI api, string slug, string year, List<APIResult> results)
        {
            string content;
            Log.D("Network.API.APIHandlers", "Uploading results.");
            try
            {
                using (var client = GetHttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(api.URL + "results/add"),
                        Content = new StringContent(
                            JsonConvert.SerializeObject(new AddResultsRequest
                            {
                                Slug = slug,
                                Year = year,
                                Results = results
                            }),
                            Encoding.UTF8,
                            "application/json"
                            )
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", api.AuthToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Log.D("Network.API.APIHandlers", "Status code ok.");
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<AddResultsResponse>(json);
                        return result;
                    }
                    Log.D("Network.API.APIHandlers", "Status code not ok.");
                    var errjson = await response.Content.ReadAsStringAsync();
                    var errresult = JsonConvert.DeserializeObject<ErrorResponse>(errjson);
                    content = errresult.Message;
                }
            }
            catch (Exception ex)
            {
                Log.D("Network.API.APIHandlers", "Exception thrown.");
                throw new APIException("Exception thrown uploading results: " + ex.Message);
            }
            throw new APIException(content);
        }

        public static async Task<AddResultsResponse> DeleteResults(ResultsAPI api, string slug, string year)
        {
            string content;
            Log.D("Network.API.APIHandlers", "Deleting results.");
            try
            {
                using (var client = GetHttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri(api.URL + "results/delete"),
                        Content = new StringContent(
                            JsonConvert.SerializeObject(new GetResultsRequest
                            {
                                Slug = slug,
                                Year = year
                            }),
                            Encoding.UTF8,
                            "application/json"
                            )
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", api.AuthToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Log.D("Network.API.APIHandlers", "Status code ok.");
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<AddResultsResponse>(json);
                        return result;
                    }
                    Log.D("Network.API.APIHandlers", "Status code not ok.");
                    var errjson = await response.Content.ReadAsStringAsync();
                    var errresult = JsonConvert.DeserializeObject<ErrorResponse>(errjson);
                    content = errresult.Message;
                }
            }
            catch (Exception ex)
            {
                Log.D("Network.API.APIHandlers", "Exception thrown.");
                throw new APIException("Exception thrown deleting results: " + ex.Message);
            }
            throw new APIException(content);
        }

    }
}
