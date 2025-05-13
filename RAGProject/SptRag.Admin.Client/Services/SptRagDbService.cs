using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace SptRag.Admin.Client.Services
{
    public partial class SptRagDbService
    {
        private readonly HttpClient httpClient;
        private readonly Uri baseUri;
        private readonly NavigationManager navigationManager;

        public SptRagDbService(NavigationManager navigationManager, HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;

            this.navigationManager = navigationManager;
            this.baseUri = new Uri($"{navigationManager.BaseUri}odata/SptRagDb/");
        }


        public async System.Threading.Tasks.Task ExportDocumentsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/sptragdb/documents/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/sptragdb/documents/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async System.Threading.Tasks.Task ExportDocumentsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/sptragdb/documents/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/sptragdb/documents/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnGetDocuments(HttpRequestMessage requestMessage);

        public async Task<Radzen.ODataServiceResult<Document>> GetDocuments(Query query)
        {
            return await GetDocuments(filter:$"{query.Filter}", orderby:$"{query.OrderBy}", top:query.Top, skip:query.Skip, count:query.Top != null && query.Skip != null);
        }

        public async Task<Radzen.ODataServiceResult<Document>> GetDocuments(string filter = default(string), string orderby = default(string), string expand = default(string), int? top = default(int?), int? skip = default(int?), bool? count = default(bool?), string format = default(string), string select = default(string))
        {
            var uri = new Uri(baseUri, $"Documents");
            uri = Radzen.ODataExtensions.GetODataUri(uri: uri, filter:filter, top:top, skip:skip, orderby:orderby, expand:expand, select:select, count:count);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            OnGetDocuments(httpRequestMessage);

            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<Document>>(response);
        }

        partial void OnCreateDocument(HttpRequestMessage requestMessage);

        public async Task<Document> CreateDocument(Document document = default(Document))
        {
            var uri = new Uri(baseUri, $"Documents");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

            httpRequestMessage.Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(document), Encoding.UTF8, "application/json");

            OnCreateDocument(httpRequestMessage);

            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Document>(response);
        }

        partial void OnDeleteDocument(HttpRequestMessage requestMessage);

        public async Task<HttpResponseMessage> DeleteDocument(string id = default(string))
        {
            var uri = new Uri(baseUri, $"Documents('{Uri.EscapeDataString(id.Trim().Replace("'", "''"))}')");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);

            OnDeleteDocument(httpRequestMessage);

            return await httpClient.SendAsync(httpRequestMessage);
        }

        partial void OnGetDocumentById(HttpRequestMessage requestMessage);

        public async Task<Document> GetDocumentById(string expand = default(string), string id = default(string))
        {
            var uri = new Uri(baseUri, $"Documents('{Uri.EscapeDataString(id.Trim().Replace("'", "''"))}')");

            uri = Radzen.ODataExtensions.GetODataUri(uri: uri, filter:null, top:null, skip:null, orderby:null, expand:expand, select:null, count:null);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            OnGetDocumentById(httpRequestMessage);

            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Document>(response);
        }

        partial void OnUpdateDocument(HttpRequestMessage requestMessage);
        
        public async Task<HttpResponseMessage> UpdateDocument(string id = default(string), Document document = default(Document))
        {
            var uri = new Uri(baseUri, $"Documents('{Uri.EscapeDataString(id.Trim().Replace("'", "''"))}')");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri);


            httpRequestMessage.Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(document), Encoding.UTF8, "application/json");

            OnUpdateDocument(httpRequestMessage);

            return await httpClient.SendAsync(httpRequestMessage);
        }
    }
}