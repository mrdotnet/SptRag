using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using SptRag.Admin.Server.Data;
using SptRag.Admin.Server.Services;

namespace SptRag.Admin.Server.Controllers
{
    public partial class ExportSptRagDbController : ExportController
    {
        private readonly SptRagDbContext context;
        private readonly SptRagDbService service;

        public ExportSptRagDbController(SptRagDbContext context, SptRagDbService service)
        {
            this.service = service;
            this.context = context;
        }

        [HttpGet("/export/SptRagDb/documents/csv")]
        [HttpGet("/export/SptRagDb/documents/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDocumentsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetDocuments(), Request.Query, false), fileName);
        }

        [HttpGet("/export/SptRagDb/documents/excel")]
        [HttpGet("/export/SptRagDb/documents/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDocumentsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetDocuments(), Request.Query, false), fileName);
        }
    }
}
