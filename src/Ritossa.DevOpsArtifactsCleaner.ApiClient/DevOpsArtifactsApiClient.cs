﻿using Newtonsoft.Json;
using RestSharp;
using Ritossa.DevOpsArtifactsCleaner.ApiClient.Params;
using Ritossa.DevOpsArtifactsCleaner.ApiClient.Results;

namespace Ritossa.DevOpsArtifactsCleaner.ApiClient
{
    public class DevOpsArtifactsApiClient
    {
        private readonly RestClient _client;
        private readonly JsonSerializerSettings _serializerSettings;
        private const string _apiVersion = "7.2-preview.1";
        private const string _feedBaseUrl = "https://feeds.dev.azure.com";
        private const string _pkgsBaseUrl = "https://pkgs.dev.azure.com";

        #region Singleton Pattern

        private static DevOpsArtifactsApiClient? _instance;

        private DevOpsArtifactsApiClient()
        {
            _client = new RestClient();

            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        public static DevOpsArtifactsApiClient GetInstance()
        {
            return _instance ??= new DevOpsArtifactsApiClient();
        }

        #endregion

        public RestResponse TestConnection(BaseParams parameters)
        {
            var organizationAndProject = 
                GetOrganizationAndProjectSegmentUrl(parameters.Organization, parameters.Project);

            var request =
                new RestRequest($"{_feedBaseUrl}/{organizationAndProject}/_apis/packaging/feeds/{parameters.FeedId}/packages")
                    .AddParameter("protocolType", "npm")
                    .AddParameter("includeAllVersions", false)
                    .AddParameter("includeUrls", false)
                    .AddParameter("includeDescriptions", false)
                    .AddParameter("top", 1)
                    .AddParameter("api-version", _apiVersion)
                    .AddAuthorization(parameters.Pat);

            var response = _client.Execute(request);

            return response;
        }

        public RestResponse<GetAllPackagesResult> GetAllPackages(GetAllPackagesParams parameters)
        {
            var organizationAndProject =
                GetOrganizationAndProjectSegmentUrl(parameters.Organization, parameters.Project);

            var request =
                new RestRequest($"{_feedBaseUrl}/{organizationAndProject}/_apis/packaging/feeds/{parameters.FeedId}/packages")
                    .AddParameter("protocolType", parameters.ProtocolType)
                    .AddParameter("includeAllVersions", parameters.IncludeAllVersions)
                    .AddParameter("includeUrls", false)
                    .AddParameter("includeDescriptions", false)
                    .AddParameter("api-version", _apiVersion)
                    .AddParameter("$skip", parameters.Skip)
                    .AddAuthorization(parameters.Pat);

            var response = _client.Execute<GetAllPackagesResult>(request);

            return response;
        }

        public RestResponse DeleteNugetPackageVersion(DeleteNugetPackageVersionParams parameters)
        {
            var organizationAndProject = GetOrganizationAndProjectSegmentUrl(parameters.Organization, parameters.Project);

            var request =
                new RestRequest(new Uri($"{_pkgsBaseUrl}/{organizationAndProject}/_apis/packaging/feeds/{parameters.FeedId}/nuget/packagesbatch"),
                        Method.Post)
                    .AddHeader("Accept", $"api-version={_apiVersion}")
                    .AddAuthorization(parameters.Pat);

            var body = new
            {
                operation = 2,
                packages = parameters.Packages.ConvertAll(_ => new
                {
                    id = _.IdOrName,
                    version = _.Version
                })
            };

            request.AddBody(JsonConvert.SerializeObject(body, _serializerSettings), contentType: ContentType.Json);

            var response = _client.Execute(request);
            return response;
        }

        public RestResponse UnlistNugetPackageVersion(UnlistNugetPackageVersionParams parameters)
        {
            var organizationAndProject = GetOrganizationAndProjectSegmentUrl(parameters.Organization, parameters.Project);

            var request =
                new RestRequest(
                        new Uri($"{_pkgsBaseUrl}/{organizationAndProject}/_apis/packaging/feeds/{parameters.FeedId}/nuget/packagesbatch"),
                        Method.Post)
                    .AddHeader("Accept", $"api-version={_apiVersion}")
                    .AddAuthorization(parameters.Pat);

            var body = new
            {
                operation = 1,
                data = new
                {
                    listed = false
                },
                packages = parameters.Packages.ConvertAll(_ => new
                {
                    id = _.IdOrName,
                    version = _.Version
                })
            };

            request.AddBody(JsonConvert.SerializeObject(body, _serializerSettings), contentType: ContentType.Json);

            var response = _client.Execute(request);
            return response;
        }

        public RestResponse RelistNugetPackageVersion(RelistNugetPackageVersionParams parameters)
        {
            var organizationAndProject = GetOrganizationAndProjectSegmentUrl(parameters.Organization, parameters.Project);

            var request =
                new RestRequest(
                        new Uri($"{_pkgsBaseUrl}/{organizationAndProject}/_apis/packaging/feeds/{parameters.FeedId}/nuget/packagesbatch"),
                        Method.Post)
                    .AddHeader("Accept", $"api-version={_apiVersion}")
                    .AddAuthorization(parameters.Pat);

            var body = new
            {
                operation = 1,
                data = new
                {
                    listed = true
                },
                packages = parameters.Packages.ConvertAll(_ => new
                {
                    id = _.IdOrName,
                    version = _.Version
                })
            };

            request.AddBody(JsonConvert.SerializeObject(body, _serializerSettings), contentType: ContentType.Json);

            var response = _client.Execute(request);
            return response;
        }

        private string GetOrganizationAndProjectSegmentUrl(string organization, string project)
        {
            if (string.IsNullOrWhiteSpace(project))
                return organization;

            return $"{organization}/{project}";
        }

    }

}
