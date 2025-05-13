using System;
using System.Net;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SptRag.Admin.Server.Controllers.SptRagDb
{
    [Route("odata/SptRagDb/Documents")]
    public partial class DocumentsController : ODataController
    {
        private SptRag.Admin.Server.Data.SptRagDbContext context;

        public DocumentsController(SptRag.Admin.Server.Data.SptRagDbContext context)
        {
            this.context = context;
        }

    
        [HttpGet]
        [EnableQuery(MaxExpansionDepth=10,MaxAnyAllExpressionDepth=10,MaxNodeCount=1000)]
        public IEnumerable<Client.Document> GetDocuments()
        {
            var items = this.context.Documents.AsQueryable<Client.Document>();
            this.OnDocumentsRead(ref items);

            return items;
        }

        partial void OnDocumentsRead(ref IQueryable<Client.Document> items);

        partial void OnDocumentGet(ref SingleResult<Client.Document> item);

        [EnableQuery(MaxExpansionDepth=10,MaxAnyAllExpressionDepth=10,MaxNodeCount=1000)]
        [HttpGet("/odata/SptRagDb/Documents(Id={Id})")]
        public SingleResult<Client.Document> GetDocument(string key)
        {
            var items = this.context.Documents.Where(i => i.Id == Uri.UnescapeDataString(key));
            var result = SingleResult.Create(items);

            OnDocumentGet(ref result);

            return result;
        }
        partial void OnDocumentDeleted(Client.Document item);
        partial void OnAfterDocumentDeleted(Client.Document item);

        [HttpDelete("/odata/SptRagDb/Documents(Id={Id})")]
        public IActionResult DeleteDocument(string key)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var item = this.context.Documents
                    .Where(i => i.Id == Uri.UnescapeDataString(key))
                    .FirstOrDefault();

                if (item == null)
                {
                    return BadRequest();
                }
                this.OnDocumentDeleted(item);
                this.context.Documents.Remove(item);
                this.context.SaveChanges();
                this.OnAfterDocumentDeleted(item);

                return new NoContentResult();

            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }

        partial void OnDocumentUpdated(Client.Document item);
        partial void OnAfterDocumentUpdated(Client.Document item);

        [HttpPut("/odata/SptRagDb/Documents(Id={Id})")]
        [EnableQuery(MaxExpansionDepth=10,MaxAnyAllExpressionDepth=10,MaxNodeCount=1000)]
        public IActionResult PutDocument(string key, [FromBody] Client.Document item)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (item == null || (item.Id != Uri.UnescapeDataString(key)))
                {
                    return BadRequest();
                }
                this.OnDocumentUpdated(item);
                this.context.Documents.Update(item);
                this.context.SaveChanges();

                var itemToReturn = this.context.Documents.Where(i => i.Id == Uri.UnescapeDataString(key));
                
                this.OnAfterDocumentUpdated(item);
                return new ObjectResult(SingleResult.Create(itemToReturn));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpPatch("/odata/SptRagDb/Documents(Id={Id})")]
        [EnableQuery(MaxExpansionDepth=10,MaxAnyAllExpressionDepth=10,MaxNodeCount=1000)]
        public IActionResult PatchDocument(string key, [FromBody]Delta<Client.Document> patch)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var item = this.context.Documents.Where(i => i.Id == Uri.UnescapeDataString(key)).FirstOrDefault();

                if (item == null)
                {
                    return BadRequest();
                }
                patch.Patch(item);

                this.OnDocumentUpdated(item);
                this.context.Documents.Update(item);
                this.context.SaveChanges();

                var itemToReturn = this.context.Documents.Where(i => i.Id == Uri.UnescapeDataString(key));
                
                this.OnAfterDocumentUpdated(item);
                return new ObjectResult(SingleResult.Create(itemToReturn));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }

        partial void OnDocumentCreated(Client.Document item);
        partial void OnAfterDocumentCreated(Client.Document item);

        [HttpPost]
        [EnableQuery(MaxExpansionDepth=10,MaxAnyAllExpressionDepth=10,MaxNodeCount=1000)]
        public IActionResult Post([FromBody] Client.Document item)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (item == null)
                {
                    return BadRequest();
                }

                this.OnDocumentCreated(item);
                this.context.Documents.Add(item);
                this.context.SaveChanges();

                var itemToReturn = this.context.Documents.Where(i => i.Id == item.Id);

                

                this.OnAfterDocumentCreated(item);

                return new ObjectResult(SingleResult.Create(itemToReturn))
                {
                    StatusCode = 201
                };
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }
        }
    }
}
